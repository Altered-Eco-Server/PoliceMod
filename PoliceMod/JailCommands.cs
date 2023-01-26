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

    [ChatCommandHandler]
    public class JailCommands
    {
        private static Plugins.PoliceConfig Plugin = (Eco.Plugins.PoliceConfig)null;

        public static void Initialize(Eco.Plugins.PoliceConfig plugin) => Plugin = plugin;

        [ChatSubCommand("AlteredEco", "Send a player to a jail cell", "arrest", ChatAuthorizationLevel.Admin)]
        public static void SendtoCell(User user, User offender, int cellNumber, int sentenceTime, string reason)
        {
            var x = -1; var y = -1; var z = -1;
            Player player = offender.Player;

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
            var inGroup = PrisonerManager.isPrisoner(offender);
            if (!inGroup) PrisonerManager.Arrest(offender, cellPos, (double)sentenceTime);
            offender.Player.OkBoxAwaitable(new LocString("You are in Jail for " + sentenceTime.ToString() + " hours!\n" + reason));
            user.Player.InfoBox(new LocString("You sent " + offender.Name + " to jail cell #" + cellNumber));
            Log.WriteErrorLineLocStr(Localizer.Format("[Altered Eco]: {0} sent {1} to jail cell #{2} on {3} for `{4}`.", (object)user.Name, (object)offender.Name, (object)cellNumber, (object)TimeFormatter.FormatDateLong(WorldTime.Seconds), (object)reason));
        }

        [ChatSubCommand("AlteredEco", "Release a player from jail", "release", ChatAuthorizationLevel.Admin)]
        public static void ReleasefromJail(User user, User offender)
        {
            Player player = offender.Player;
            var inGroup = PrisonerManager.isPrisoner(offender);

            if (inGroup)
            {
                player.SetPosition(new Vector3i(Plugins.PoliceConfig.ImpoundPosX, Plugins.PoliceConfig.ImpoundPosY, Plugins.PoliceConfig.ImpoundPosZ));
                PrisonerManager.Release(offender);
                user.Player.InfoBox(new LocString("You let " + offender.Name + " out of jail"));
            }
            else user.Player.InfoBox(new LocString(offender.Name + " is not a prisoner"));
        }

        [ChatSubCommand("AlteredEco", "Check how much time you have left in jail", "jailtime", ChatAuthorizationLevel.User)]
        public static void checkSentence(User user)
        {
            var timeLeft = PrisonerManager.TimeLeft(user).ToString("0.00");
            user.Player.InfoBox(new LocString("You have " + timeLeft + " hours left in your sentence"));
        }

        [ChatSubCommand("AlteredEco", "Change how much time a player has left in jail", "modifySentence", ChatAuthorizationLevel.Admin)]
        public static void modifySentence(User user, User prisioner, int hoursToChangeBy)
        {
            PrisonerManager.modifySentence(prisioner, (double)hoursToChangeBy);
        }

        [ChatSubCommand("AlteredEco", "Change the location of a jail cell", "moveCell", ChatAuthorizationLevel.Admin)]
        public static void changeCellPos(User user, int cellNum, int x, int y, int z)
        {
            PoliceConfig.ChangeCellLoc(cellNum, x, y, z);
            user.Player.InfoBox(new LocString("Cell location " + cellNum + " changed to (" + x + "," + y + "," + z + ")"));
        }

        [ChatSubCommand("AlteredEco", "Change the location of impound", "moveImpound", ChatAuthorizationLevel.Admin)]
        public static void changeImpoundPos(User user, int x, int y, int z)
        {
            PoliceConfig.ChangeImpoundLoc(x, y, z);
            user.Player.InfoBox(new LocString("Impound location changed to (" + x + "," + y + "," + z + ")"));
        }

        [ChatSubCommand("AlteredEco", "Give yourself any item (Forced, ignores restrictions)", "AFG", ChatAuthorizationLevel.Admin)]
        public static void ActualForceGive(User user, string itemName, int number = 1)
        {
            Item obj = CommandsUtil.ClosestMatchingEntity<Item>(user, itemName, (IReadOnlyList<Item>)Item.AllItems, (Func<Item, string>)(x => x.GetType().Name), (Func<Item, LocString>)(x => x.DisplayName));
            if (obj == null)
                return;
            AdminCommands.ForceGive(user, obj, number);
        }
    }
}
