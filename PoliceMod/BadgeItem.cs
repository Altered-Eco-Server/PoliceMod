namespace Eco.Mods.TechTree
{
    using System.Collections.Generic;
    using Eco.Core.Items;
    using Eco.Gameplay.Components;
    using Eco.Gameplay.Items;
    using Eco.Gameplay.Players;
    using Eco.Gameplay.Skills;
    using Eco.Gameplay.Systems.TextLinks;
    using Eco.Shared.Localization;
    using Eco.Shared.Serialization;
    using System;
    using System.ComponentModel;
    using System.Linq;
    using Eco.Core.Controller;
    using Eco.Gameplay.DynamicValues;
    using Eco.Gameplay.GameActions;
    using Eco.Gameplay.Interactions;
    using Eco.Gameplay.Systems.Tooltip;
    using Eco.Shared.Items;
    using Eco.Shared.Utils;
    using System.Runtime.CompilerServices;
    using Eco.Simulation.Time;
    using Eco.Gameplay.Objects;
    using Eco.Gameplay.Components.Auth;
    using Eco.Gameplay.Aliases;

    [Serialized]
    [LocDisplayName("Badge")]
    [MaxStackSize(200)]
    [Weight(0)]
    public partial class BadgeItem : Item
    {
        public override LocString DisplayDescription => Localizer.DoStr("Respect My Authoritah!");

        public override InteractResult OnActLeft(InteractionContext context)
        {
            var player = context.Player;
            if (context.Target is WorldObject worldObject && worldObject.HasComponent(typeof(VehicleComponent)))
            {
                worldObject.GetComponent<VehicleComponent>().Mounts.MountSeat(0, player);
                worldObject.GetComponent<VehicleComponent>().Driver = player;
                return InteractResult.SuccessLoc($"You took control of {worldObject.Owners.OneUser}'s vehicle");
            }
            return InteractResult.FailureLoc($"Can only use this power for vehicles");
        }
    }
}