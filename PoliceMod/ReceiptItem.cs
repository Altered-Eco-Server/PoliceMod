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

    [Serialized]
    [LocDisplayName("Gate Pass")]
    [MaxStackSize(200)]
    [Weight(0)]
    [Ecopedia("Food", "Ingredients", createAsSubPage: true)]
    public partial class ReceiptItem : Item
    {
        public override LocString DisplayDescription => Localizer.DoStr("Gate Pass to get out of impound. One time use only.");
    }
}