using System.ComponentModel;
using System.Collections.Generic;
using System.Runtime.Serialization;
using Eco.Core.Utils;
using Eco.Shared.Math;
using Eco.Shared.Serialization;
using Newtonsoft.Json;
using Eco.Shared.Utils;
using Eco.Shared.Localization;
using Eco.Shared.Networking;
using Eco.Mods.Organisms;

namespace Eco.Plugins
{
    [Serialized]
    public class Police_Config
    {
        public int Cell1PosX { get; set; }
        public int Cell1PosY { get; set; }
        public int Cell1PosZ { get; set; }

        public int Cell2PosX { get; set; }
        public int Cell2PosY { get; set; }
        public int Cell2PosZ { get; set; }

        public int ImpoundPosX { get; set; }
        public int ImpoundPosY { get; set; }
        public int ImpoundPosZ { get; set; }
    }

    public class PoliceConfig
    {
        [Serialized] public static string logDir = @"C:\Users\luluq\Desktop\AlteredEco";

        public static Police_Config configObj => JsonConvert.DeserializeObject<Police_Config>(File.ReadAllText(logDir + @"\PoliceConfig.json"));
        [Serialized] public static int Cell1PosX = configObj.Cell1PosX;
        [Serialized] public static int Cell1PosY = configObj.Cell1PosY;
        [Serialized] public static int Cell1PosZ = configObj.Cell1PosZ;

        [Serialized] public static int Cell2PosX = configObj.Cell2PosX;
        [Serialized] public static int Cell2PosY = configObj.Cell2PosY;
        [Serialized] public static int Cell2PosZ = configObj.Cell2PosZ;

        [Serialized] public static int ImpoundPosX = configObj.ImpoundPosX;
        [Serialized] public static int ImpoundPosY = configObj.ImpoundPosY;
        [Serialized] public static int ImpoundPosZ = configObj.ImpoundPosZ;

        [Serialized] public static Vector3i Cell1Pos = new Vector3i(Cell1PosX, Cell1PosY, Cell1PosZ);
        [Serialized] public static Vector3i Cell2Pos = new Vector3i(Cell2PosX, Cell2PosY, Cell2PosZ);

        public static void ChangeCellLoc(int cellNum, int x, int y, int z)
        {
            if (cellNum == 1)
            {
                Cell1PosX = x;
                Cell1PosY = y;
                Cell1PosZ = z;
                Cell1Pos = new Vector3i(x, y, z);
            }
            else if (cellNum == 2)
            {
                Cell2PosX = x;
                Cell2PosY = y;
                Cell2PosZ = z;
                Cell2Pos = new Vector3i(x, y, z);
            }
            Serialize();
        }

        public static void ChangeImpoundLoc(int x, int y, int z)
        {

            ImpoundPosX = x;
            ImpoundPosY = y;
            ImpoundPosZ = z;
            Serialize();
        }

        private static void Serialize()
        {
            using (StreamWriter config = File.CreateText(logDir + @"\PoliceConfig.json"))
            {
                var settings = new Police_Config()
                {
                    Cell1PosX = Cell1PosX,
                    Cell1PosY = Cell1PosY,
                    Cell1PosZ = Cell1PosZ,
                    Cell2PosX = Cell2PosX,
                    Cell2PosY = Cell2PosY,
                    Cell2PosZ = Cell2PosZ,
                    ImpoundPosX = ImpoundPosX,
                    ImpoundPosY = ImpoundPosY,
                    ImpoundPosZ = ImpoundPosZ
                };
                JsonSerializer serializer = new JsonSerializer();
                serializer.Serialize(config, settings);
            }
        }
    }
}