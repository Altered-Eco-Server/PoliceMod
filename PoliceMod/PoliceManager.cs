using System;
using System.Threading;
using Eco.Core.Plugins.Interfaces;
using Eco.Core.Utils;
using Eco.Gameplay.Players;
using Eco.Gameplay.Disasters;
using Eco.Shared.Localization;
using System.Runtime.CompilerServices;
using Eco.Shared.Utils;
using Eco.Simulation.Time;
using Eco.Gameplay.Systems.Messaging.Notifications;
using Eco.Shared.Services;
using Eco.Core.Tests;
using Eco.Gameplay.Systems.Messaging.Chat.Commands;
using Eco.Mods.TechTree;
using Eco.Shared.Serialization;
using Eco.Core.Serialization;
using Eco.Gameplay.Effects;
using Eco.Shared.Math;
using Eco.Gameplay.Aliases;
using Eco.Gameplay.Property;
using Eco.Gameplay.Items;
using Eco.Gameplay.Objects;
using Eco.Gameplay.Rooms;
using Eco.Shared;
using Eco.Shared.IoC;
using Eco.Simulation;
using Eco.Simulation.Agents;
using Eco.Simulation.WorldLayers;
using Eco.Simulation.WorldLayers.Layers;
using Eco.World;
using Eco.World.Blocks;
using System.Collections.Generic;
using Eco.Gameplay.Components;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Threading;
using System.Threading.Tasks;
using Eco.Core;
using Eco.Core.Plugins;
using Eco.Shared.Voxel;
using Eco.Gameplay.GameActions;
using Eco.Gameplay.Economy;
using System.IO;
using Eco.Plugins;
using static Eco.Gameplay.Disasters.DisasterPlugin;
using System.Collections;
using Newtonsoft.Json;
using Eco.Mods.Organisms;

namespace Eco.Mods.TechTree
{
    [Serialized]
    public class PoliceManager : IModKitPlugin, IInitializablePlugin
    {
        public Timer Timer;
        private static string dir = PoliceConfig.logDir;
        [Serialized] public static Dictionary<string, double> incarceratedPlayers = new Dictionary<string, double>();
        [Serialized] public static Dictionary<string, int> occupiedCells = new Dictionary<string, int>();
        [Serialized] public static Dictionary<string, int> escapeAttempts = new Dictionary<string, int>();
        [Serialized] public static Dictionary<string, int> arrestCount = new Dictionary<string, int>();
        [Serialized] public static Dictionary<string, int> ticketCount = new Dictionary<string, int>();

        public void Initialize(TimedTask timer)
        {
            Timer = new(Timer_tick, null, 10000, 10000);

            if (!Directory.Exists(dir))
                Directory.CreateDirectory(dir);

            if (WorldTime.Seconds == null || WorldTime.Seconds < 60) Reset();

            if (File.Exists(dir + @"\IncarceratedPlayers.json"))
                incarceratedPlayers = JsonConvert.DeserializeObject<Dictionary<string, double>>(File.ReadAllText(dir + @"\IncarceratedPlayers.json"));
            if (File.Exists(dir + @"\OccupiedCells.json"))
                occupiedCells = JsonConvert.DeserializeObject<Dictionary<string, int>>(File.ReadAllText(dir + @"\OccupiedCells.json"));
            if (File.Exists(dir + @"\EscapeAttempts.json"))
                escapeAttempts = JsonConvert.DeserializeObject<Dictionary<string, int>>(File.ReadAllText(dir + @"\EscapeAttempts.json"));
            if (File.Exists(dir + @"\ArrestCount.json"))
                arrestCount = JsonConvert.DeserializeObject<Dictionary<string, int>>(File.ReadAllText(dir + @"\ArrestCount.json"));
            if (File.Exists(dir + @"\TicketCount.json"))
                arrestCount = JsonConvert.DeserializeObject<Dictionary<string, int>>(File.ReadAllText(dir + @"\TicketCount.json"));
        }

        public static void Arrest(User user, Vector3i cellPos, double sentenceHours)
        {
            var userName = user.Name;
            var releaseTime = WorldTime.Seconds + TimeUtil.HoursToSeconds(sentenceHours);
            if (!incarceratedPlayers.ContainsKey(userName))
                incarceratedPlayers.Add(user.Name, releaseTime);
            if (!occupiedCells.ContainsKey(userName))
                occupiedCells.Add(userName, GetCellNum(cellPos));
            if (!escapeAttempts.ContainsKey(userName))
                escapeAttempts.Add(userName, 0);
            if (!arrestCount.ContainsKey(userName))
                arrestCount.Add(userName, 1);
            else arrestCount[userName]++;
        }

        public static void Release(User user)
        {
            var userName = user.Name;
            if (incarceratedPlayers.ContainsKey(userName))
            {
                incarceratedPlayers.Remove(userName);
                occupiedCells.Remove(userName);
                user.Player.InfoBox(new LocString("You are free!"));
            }
        }

        public static bool isPrisoner(User user)
        {
            var userName = user.Name;
            if (incarceratedPlayers.ContainsKey(userName))
                return true;
            return false;
        }

        public static void modifySentence(User user, double changeByHours)
        {
            var userName = user.Name;
            if (!incarceratedPlayers.ContainsKey(userName))
                return;

            incarceratedPlayers[userName] += TimeUtil.HoursToSeconds(changeByHours);
        }

        public static float TimeLeft(User user)
        {
            var userName = user.Name;
            var timeLeft = (float)TimeUtil.SecondsToHours(incarceratedPlayers.GetOrDefault(userName) - WorldTime.Seconds);
            return timeLeft;
        }

        public static Vector3i GetCellPos(int cellNum)
        {
            Vector3i cellPos = new Vector3i();
            switch (cellNum)
            {
                case 1: cellPos = PoliceConfig.Cell1Pos; break;
                case 2: cellPos = PoliceConfig.Cell2Pos; break;
            }
            return cellPos;
        }

        public static int GetCellNum(Vector3i cellPos)
        {
            int cellNum = 0;
            if (cellPos == PoliceConfig.Cell1Pos)
                cellNum = 1;
            if (cellPos == PoliceConfig.Cell2Pos)
                cellNum = 2;

            return cellNum;
        }

        private static void Reset()
        {
            File.Delete(dir + @"\IncarceratedPlayers.json");
            File.Delete(dir + @"\OccupiedCells.json");
            File.Delete(dir + @"\EscapeAttempts.json");
            File.Delete(dir + @"\PoliceLog.alteredeco");
        }

        static void Timer_tick(object state)
        {
            try
            {
                foreach (var player in incarceratedPlayers)
                {
                    User user = UserManager.FindUserByName(player.Key);
                    if (user.IsOnline)
                    {
                        var isEscaping = Vector3i.DistanceSq(user.Position.XYZi(), GetCellPos(occupiedCells.GetOrDefault(user.Name))) >= 400;

                        if (WorldTime.Seconds >= player.Value)
                        {
                            Release(user);
                        }
                        if (isEscaping)
                        {
                            user.Player.SetPosition(GetCellPos(occupiedCells.GetOrDefault(player.Key)));
                            incarceratedPlayers[user.Name] += TimeUtil.HoursToSeconds(2);
                            escapeAttempts[user.Name] += 1;
                            ReputationManager.Obj.GiveRep("Tried to escape jail", user, -5, null, true);
                            user.Player.OkBox(new LocString("You just added 2 hours to your sentence for trying to escape!"));
                        }
                    }
                }

                using (StreamWriter file = File.CreateText(dir + @"\IncarceratedPlayers.json"))
                {
                    JsonSerializer serializer = new JsonSerializer();
                    serializer.Serialize(file, incarceratedPlayers);
                }
                using (StreamWriter file = File.CreateText(dir + @"\OccupiedCells.json"))
                {
                    JsonSerializer serializer = new JsonSerializer();
                    serializer.Serialize(file, occupiedCells);
                }
                using (StreamWriter file = File.CreateText(dir + @"\EscapeAttempts.json"))
                {
                    JsonSerializer serializer = new JsonSerializer();
                    serializer.Serialize(file, escapeAttempts);
                }
                using (StreamWriter file = File.CreateText(dir + @"\ArrestCount.json"))
                {
                    JsonSerializer serializer = new JsonSerializer();
                    serializer.Serialize(file, arrestCount);
                }
                using (StreamWriter file = File.CreateText(dir + @"\TicketCount.json"))
                {
                    JsonSerializer serializer = new JsonSerializer();
                    serializer.Serialize(file, ticketCount);
                }
            }
            catch (Exception e)
            {

            }
        }
        public string GetStatus() => "";
        public string GetCategory() => string.Empty;
    }
}