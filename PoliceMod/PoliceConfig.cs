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
        [Serialized] public static string logDir { get; set; } = @"C:\Users\c_bat\Desktop\AlteredEco";

        [Serialized] public static int Cell1PosX { get; set; } = 561;
        [Serialized] public static int Cell1PosY { get; set; } = 55;
        [Serialized] public static int Cell1PosZ { get; set; } = 518;

        [Serialized] public static int Cell2PosX { get; set; } = 561;
        [Serialized] public static int Cell2PosY { get; set; } = 55;
        [Serialized] public static int Cell2PosZ { get; set; } = 514;

        [Serialized] public static int ImpoundPosX { get; set; } = 470;
        [Serialized] public static int ImpoundPosY { get; set; } = 76;
        [Serialized] public static int ImpoundPosZ { get; set; } = 500;

        [Serialized] public static Vector3i Cell1Pos = new Vector3i(Cell1PosX, Cell1PosY, Cell1PosZ);
        [Serialized] public static Vector3i Cell2Pos = new Vector3i(Cell2PosX, Cell2PosY, Cell2PosZ);
    }
}