using System.ComponentModel;
using System.Collections.Generic;
using System.Runtime.Serialization;
using Eco.Core.Utils;
using Eco.Shared.Math;
using Eco.Shared.Serialization;


namespace Eco.Plugins
{
    [Serialized]
    public class PoliceConfig
    {
        [Serialized] public static string logDir { get; set; } = @"C:\Users\luluq\Desktop\AlteredEco";

        [Serialized] public static int Cell1PosX { get; set; } = 561;
        [Serialized] public static int Cell1PosY { get; set; } = 55;
        [Serialized] public static int Cell1PosZ { get; set; } = 518;

        [Serialized] public static int Cell2PosX { get; set; } = 546;
        [Serialized] public static int Cell2PosY { get; set; } = 55;
        [Serialized] public static int Cell2PosZ { get; set; } = 555;

        [Serialized] public static int ImpoundPosX { get; set; } = 420;
        [Serialized] public static int ImpoundPosY { get; set; } = 55;
        [Serialized] public static int ImpoundPosZ { get; set; } = 100;

        [Serialized] public static Vector3i Cell1Pos = new Vector3i(Cell1PosX, Cell1PosY, Cell1PosZ);
        [Serialized] public static Vector3i Cell2Pos = new Vector3i(Cell2PosX, Cell2PosY, Cell2PosZ);
    }
}