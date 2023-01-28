namespace Eco.Mods
{
    using System.IO;
    using System.Reflection;
    using Eco.Gameplay.Players;
    using Eco.Shared.Localization;
    using Eco.Shared.Utils;
    using Eco.Shared.Math;
    using Eco.Simulation.Time;
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
    using Eco.Core.FileStorage;
    using static Eco.Gameplay.Property.RentForProperty;
    using System.Text;
    using Eco.Mods.Organisms;
    using Eco.Plugins;
    using Eco.Gameplay.Interactions;

    [ChatCommandHandler]
    public class PoliceCommands
    {
        private static string dir = PoliceConfig.logDir;

        private static Plugins.PoliceConfig Plugin = (Eco.Plugins.PoliceConfig)null;

        public static void Initialize(Eco.Plugins.PoliceConfig plugin) => Plugin = plugin;

        [ChatCommand("Performs commands designed for the server.", ChatAuthorizationLevel.User)]
        public static void Police(User user) { }

        [ChatSubCommand("Police", "Target vehicle to give a ticket to owner and teleport vehicle to impound", "Impound", ChatAuthorizationLevel.User)]
        public static void GiveTicket(User user, INetObject target, string reason)
        {
            string itemName = "Parking Ticket";

            if (user.SelectedItem is not BadgeItem)
            {
                user.Player.InfoBox(new LocString("Are you an officer? Let me see your badge."));
                return;
            }

            if (!(target is WorldObject worldObject) || !worldObject.HasComponent(typeof(VehicleComponent)))
                user.Player.Error(Localizer.DoStr("Can only use this command when targeting a vehicle"));
            else
            {
                IAlias owners = worldObject.Owners;
                var owner = owners.OneUser();
                Vector3i impound = new Vector3i(Plugins.PoliceConfig.ImpoundPosX, Plugins.PoliceConfig.ImpoundPosY + 2, Plugins.PoliceConfig.ImpoundPosZ);
                worldObject.Rotation = Quaternion.LookRotation(worldObject.Rotation.Forward, Vector3i.Up);
                worldObject.Position = impound;
                AdminCommands.Give(owner, itemName, 1);
                user.Player.InfoBox(new LocString("You gave a parking ticket to " + owner.Name));

                if (owner.IsOnline)
                    owner.Player.InfoBox(new LocString("Your vehicle is in the impound!\n" + reason));
                WriteLog(user.Name, owner.Name, TimeFormatter.FormatDateLong(WorldTime.Seconds), reason, worldObject.Name);

                if (!PoliceManager.ticketCount.ContainsKey(owner.Name))
                    PoliceManager.ticketCount.Add(owner.Name, 1);
                else PoliceManager.ticketCount[owner.Name] += 1;
                Log.WriteErrorLineLocStr(Localizer.Format("[Altered Eco]: {0} gave a parking ticket to {1} on {2}.", (object)user.Name, (object)owner.Name, (object)TimeFormatter.FormatDateLong(WorldTime.Seconds)));
            }
        }

        [ChatSubCommand("Police", "View all Traffic reports", "trecords", ChatAuthorizationLevel.User)]
        public static void TrafficRecords(User user)
        {

            if (user.SelectedItem is not BadgeItem)
            {
                user.Player.InfoBox(new LocString("Are you an officer? Let me see your badge."));
                return;
            }

            var title = new StringBuilder();
            title.Append("<color=#3e78d6>Traffic Records</color>");
            var message = ReadLog();
            user.Player.OpenInfoPanel(title.ToString(), new LocString(message.ToString()), "Traffic Records");
        }

        [ChatSubCommand("Police", "View all players in jail with details about sentence time", "prisoners", ChatAuthorizationLevel.User)]
        public static void Prisoners(User user)
        {

            if (user.SelectedItem is not BadgeItem)
            {
                user.Player.InfoBox(new LocString("Are you an officer? Let me see your badge."));
                return;
            }

            var info = PoliceManager.incarceratedPlayers;

            var title = new StringBuilder();
            var message = new StringBuilder();
            title.Append("<color=#3e78d6>Prisoner Report</color>");

            foreach (var prisoner in info)
            {
                message.AppendLineLoc(FormattableStringFactory.Create("<color=#3e78d6>NAME:</color> {0}    <color=#3e78d6>CELL:</color> {4}    <color=#3e78d6>RELEASE:</color> {1}    <color=#3e78d6>TIME LEFT:</color> {2} hours    <color=#3e78d6>ESCAPES:</color> {3}", (object)prisoner.Key, (object)TimeFormatter.FormatDateLong(prisoner.Value), (object)TimeUtil.SecondsToHours(prisoner.Value - WorldTime.Seconds).ToString("0.00"), (object)PoliceManager.escapeAttempts[prisoner.Key], (object)PoliceManager.occupiedCells[prisoner.Key]));
            }
            user.Player.OpenInfoPanel(title.ToString(), new LocString(message.ToString()), "Prisoner Report");
        }

        public static async void WriteLog(string officer, string offender, string time, string reason, string vehicle)
        {
            try
            {
                if (!Directory.Exists(dir))
                    Directory.CreateDirectory(dir);

                var report = GenerateReport(officer, offender, time, reason, vehicle);
                File.AppendAllLines(dir + @"\TicketLog.alteredeco", report);
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
                foreach (var line in File.ReadAllLines(dir + @"\TicketLog.alteredeco"))
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

        public static List<string> GenerateReport(string officer, string offender, string time, string reason, string vehicle)
        {
            List<string> rs = new List<string>();
            rs.Add("\n========Incident Report========");
            rs.Add("Officer: " + officer);
            rs.Add("Offender: " + offender);
            rs.Add("Vehicle: " + vehicle);
            rs.Add("Time: " + time);
            rs.Add("Reason: " + reason);
            return rs;
        }
    }
}
