namespace Eco.Mods
{
    using Eco.Gameplay.Players;
    using Eco.Shared.Localization;
    using Eco.Shared.Utils;
    using Eco.Shared.Math;
    using Eco.Simulation.Time;
    using Eco.Core.Plugins.Interfaces;
    using System.Runtime.CompilerServices;
    using Eco.Gameplay.Systems.Messaging.Chat.Commands;
    using Eco.Gameplay.Items;
    using System;
    using System.Collections.Generic;
    using Eco.Gameplay.Utils;
    using Eco.World;
    using Eco.Gameplay.Objects;
    using Eco.Gameplay.Components;
    using Eco.Shared.Networking;
    using Eco.Gameplay.Aliases;
    using Vector3 = System.Numerics.Vector3;
    using Eco.Gameplay.Systems.Chat;
    using Eco.Gameplay.UI;
    using Eco.Mods.TechTree;
    using Eco.Plugins;
    using Eco.Mods.Organisms;
    using System.Text;
    using Eco.Gameplay.Property;

    [ChatCommandHandler]
    public class JailCommands
    {
        private static Plugins.PoliceConfig Plugin = (Eco.Plugins.PoliceConfig)null;
        private static string dir = PoliceConfig.logDir;
        public static void Initialize(Eco.Plugins.PoliceConfig plugin) => Plugin = plugin;

        [ChatSubCommand("Police", "Send a player to a jail cell", "arrest", ChatAuthorizationLevel.User)]
        public static void SendtoCell(User user, User offender, int cellNumber, int sentenceTime, string reason)
        {
            var x = -1; var y = -1; var z = -1;
            Player player = offender.Player;
            var inGroup = PoliceManager.isPrisoner(offender);

            if (user.SelectedItem is not BadgeItem)
            {
                user.Player.InfoBox(new LocString("Are you an officer? Let me see your badge."));
                return;
            }

            if (inGroup) { user.Player.InfoBox(new LocString(offender.Name + " is already in jail")); return; }

            if (!offender.IsOnline) { user.Player.InfoBox(new LocString(offender.Name + " is not online")); return; }

            if (offender != null || cellNumber != null)
            {
                if (cellNumber == 1)
                {
                    x = Plugins.PoliceConfig.Cell1PosX; y = Plugins.PoliceConfig.Cell1PosY; z = Plugins.PoliceConfig.Cell1PosZ;
                }
                else if (cellNumber == 2)
                {
                    x = Plugins.PoliceConfig.Cell2PosX; y = Plugins.PoliceConfig.Cell2PosY; z = Plugins.PoliceConfig.Cell2PosZ;
                }
                else if (cellNumber >= 3)
                {
                    user.Player.InfoBox(new LocString(cellNumber + " is not a valid cell number."));
                    return;
                }
            }
            else return;
            var cellPos = new Vector3i(x, y, z);
            player.SetPosition((Vector3)cellPos);
            PoliceManager.Arrest(offender, cellPos, (double)sentenceTime);
            WriteLog(user.Name, offender.Name, sentenceTime.ToString(), TimeFormatter.FormatDateLong(WorldTime.Seconds + TimeUtil.HoursToSeconds(sentenceTime)), reason);
            offender.Player.OkBoxAwaitable(new LocString("You are in Jail for " + sentenceTime.ToString() + " hours!\n" + reason));
            user.Player.InfoBox(new LocString("You sent " + offender.Name + " to jail cell #" + cellNumber));
            Log.WriteErrorLineLocStr(Localizer.Format("[Altered Eco]: {0} sent {1} to jail cell #{2} on {3} for `{4}`.", (object)user.Name, (object)offender.Name, (object)cellNumber, (object)TimeFormatter.FormatDateLong(WorldTime.Seconds), (object)reason));
        }

        [ChatSubCommand("Police", "Release a player from jail", "release", ChatAuthorizationLevel.User)]
        public static void ReleasefromJail(User user, User prisoner)
        {
            Player player = prisoner.Player;
            var inGroup = PoliceManager.isPrisoner(prisoner);

            if (user.SelectedItem is not BadgeItem)
            {
                user.Player.InfoBox(new LocString("Are you an officer? Let me see your badge."));
                return;
            }

            if (!prisoner.IsOnline) { user.Player.InfoBox(new LocString(prisoner.Name + " is not online")); return; }

            if (inGroup)
            {
                player.SetPosition(new Vector3i(Plugins.PoliceConfig.ImpoundPosX, Plugins.PoliceConfig.ImpoundPosY, Plugins.PoliceConfig.ImpoundPosZ));
                PoliceManager.Release(prisoner);
                user.Player.InfoBox(new LocString("You let " + prisoner.Name + " out of jail"));
            }
            else user.Player.InfoBox(new LocString(prisoner.Name + " is not a prisoner"));
        }

        [ChatSubCommand("Police", "Check how much time you have left in jail", "jailtime", ChatAuthorizationLevel.User)]
        public static void checkSentence(User user)
        {
            if (PoliceManager.isPrisoner(user))
            {
                var timeLeft = PoliceManager.TimeLeft(user).ToString("0.00");
                user.Player.InfoBox(new LocString("You have " + timeLeft + " hours left in your sentence"));
            }
            else user.Player.InfoBox(new LocString("You are not a prisoner"));
        }

        [ChatSubCommand("Police", "Change how much time a player has left in jail", "modifySentence", ChatAuthorizationLevel.User)]
        public static void modifySentence(User user, User prisoner, int hoursToChangeBy)
        {
            if (user.SelectedItem is not BadgeItem)
            {
                user.Player.InfoBox(new LocString("Are you an officer? Let me see your badge."));
                return;
            }

            PoliceManager.modifySentence(prisoner, (double)hoursToChangeBy);
        }

        [ChatSubCommand("Police", "Change the location of a jail cell", "moveCell", ChatAuthorizationLevel.Admin)]
        public static void changeCellPos(User user, int cellNum, int x, int y, int z)
        {
            PoliceConfig.ChangeCellLoc(cellNum, x, y, z);
            user.Player.InfoBox(new LocString("Cell location " + cellNum + " changed to (" + x + "," + y + "," + z + ")"));
        }

        [ChatSubCommand("Police", "Change the location of impound", "moveImpound", ChatAuthorizationLevel.Admin)]
        public static void changeImpoundPos(User user, int x, int y, int z)
        {
            PoliceConfig.ChangeImpoundLoc(x, y, z);
            user.Player.InfoBox(new LocString("Impound location changed to (" + x + "," + y + "," + z + ")"));
        }

        [ChatSubCommand("Police", "View all Jail Records", "jrecords", ChatAuthorizationLevel.User)]
        public static void JailRecords(User user)
        {

            if (user.SelectedItem is not BadgeItem)
            {
                user.Player.InfoBox(new LocString("Are you an officer? Let me see your badge."));
                return;
            }

            var title = new StringBuilder();
            title.Append("<color=#3e78d6>Jail Records</color>");
            var message = ReadLog();
            user.Player.OpenInfoPanel(title.ToString(), new LocString(message.ToString()), "Jail Records");
        }

        [ChatSubCommand("Police", "View Police Records for a player", "crecord", ChatAuthorizationLevel.User)]
        public static void CitizenRecord(User user, User citizen)
        {

            if (user.SelectedItem is not BadgeItem)
            {
                user.Player.InfoBox(new LocString("Are you an officer? Let me see your badge."));
                return;
            }

            var title = new StringBuilder();
            title.Append("<color=#3e78d6>Police Records</color>");
            var message = GenerateCitizenRecord(citizen.Name);
            user.Player.OpenInfoPanel(title.ToString(), new LocString(message.ToString()), "Police Records");
        }

        [ChatSubCommand("AlteredEco", "Give yourself any item (Forced, ignores restrictions)", "AFG", ChatAuthorizationLevel.Admin)]
        public static void ActualForceGive(User user, string itemName, int number = 1)
        {
            Item obj = CommandsUtil.ClosestMatchingEntity<Item>(user, itemName, (IReadOnlyList<Item>)Item.AllItems, (Func<Item, string>)(x => x.GetType().Name), (Func<Item, LocString>)(x => x.DisplayName));
            if (obj == null)
                return;
            AdminCommands.ForceGive(user, obj, number);
        }


        public static async void WriteLog(string officer, string offender, string sentence, string release, string reason)
        {
            try
            {
                if (!Directory.Exists(dir))
                    Directory.CreateDirectory(dir);

                var report = GenerateRecord(officer, offender, sentence, release, reason);
                File.AppendAllLines(dir + @"\ArrestLog.alteredeco", report);
            }
            catch (Exception e)
            {
                Log.WriteErrorLineLocStr($"Failed to Write Log: " + e);
            }
        }

        public static StringBuilder ReadLog()
        {
            StringBuilder lines = new StringBuilder();

            try
            {
                foreach (var line in File.ReadAllLines(dir + @"\ArrestLog.alteredeco"))
                {
                    lines.AppendLineLoc(FormattableStringFactory.Create(line));
                }
            }
            catch (Exception e)
            {
                Log.WriteErrorLineLocStr($"Failed to Read Log: " + e);
            }
            return lines;
        }

        public static List<string> GenerateRecord(string officer, string offender, string sentence, string release, string reason)
        {
            int newValue;

            List<string> rs = new List<string>();
            rs.Add("\n========Arrest Record========");
            rs.Add("Officer Name: " + officer);
            rs.Add("Prisoner Name: " + offender);
            rs.Add("Sentence Time: " + sentence + " hours");
            rs.Add("Arrest Date: " + TimeFormatter.FormatDateLong(WorldTime.Seconds));
            rs.Add("Release Date: " + release);
            rs.Add("Reason: " + reason);
            return rs;
        }

        public static StringBuilder GenerateCitizenRecord(string player)
        {
            StringBuilder lines = new StringBuilder();
            var user = UserManager.FindUserByName(player);
            var userName = player.ToLower()[player.Length - 1] == char.Parse("s") ? player : player + "'s";
            var hasRecord = PoliceManager.arrestCount.ContainsKey(player);
            var hasEscaped = PoliceManager.escapeAttempts.ContainsKey(player);
            var hasTickets = PoliceManager.ticketCount.ContainsKey(player);
            var id = user.SlgId == null ? user.SteamId : user.SlgId;
            int newValue;

            lines.AppendLineLoc(FormattableStringFactory.Create("<color=#119da4>========" + userName + "'s Record========</color>"));
            lines.AppendLineLoc(FormattableStringFactory.Create("\n<color=#00a8e8>Id Number: </color>" + id));
            if (user.GetResidencyHouse() is not null)
                lines.AppendLineLoc(FormattableStringFactory.Create("<color=#00a8e8>Home Address: </color>" + user.GetResidencyHouse().MarkedUpName + "  " + user.GetResidencyHouse().CenterPos.ToString()));
            else lines.AppendLineLoc(FormattableStringFactory.Create("<color=#00a8e8>Home Address: </color>" + "Homeless"));
            lines.AppendLineLoc(FormattableStringFactory.Create("<color=#00a8e8>Birth Date: </color>" + TimeFormatter.FormatDateLong(user.CreationTime)));
            lines.AppendLineLoc(FormattableStringFactory.Create("<color=#00a8e8>Age: </color>" + TimeFormatter.FormatTimeSince(user.CreationTime, WorldTime.Seconds)));
            lines.AppendLineLoc(FormattableStringFactory.Create("<color=#00a8e8>Reputation: </color>" + user.Reputation));
            newValue = hasTickets ? PoliceManager.ticketCount.GetOrDefault(player) : 0;
            lines.AppendLineLoc(FormattableStringFactory.Create("<color=#00a8e8>Number of Tickets: </color>" + newValue));
            newValue = hasRecord ? PoliceManager.arrestCount.GetOrDefault(player) : 0;
            lines.AppendLineLoc(FormattableStringFactory.Create("<color=#00a8e8>Number of Arrests: </color>" + newValue));
            newValue = hasEscaped ? PoliceManager.escapeAttempts.GetOrDefault(player) : 0;
            lines.AppendLineLoc(FormattableStringFactory.Create("<color=#00a8e8>Escape Attempts: </color>" + newValue));
            return lines;
        }
    }
}