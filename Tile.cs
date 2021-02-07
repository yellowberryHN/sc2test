using System;
using System.Collections.Generic;
using System.Text;

namespace sc2test
{
    public class Tile
    {
        [Flags]
        public enum Corners
        {
            None = 0,
            BottomLeft = 0b0001,
            BottomRight = 0b0010,
            TopLeft = 0b0100,
            TopRight = 0b1000
        }

        public enum Zone
        {
            None = 0,
            LightResidential = 0b0001,
            DenseResidential = 0b0010,
            LightCommercial = 0b0011,
            DenseCommercial = 0b0100,
            LightIndustrial = 0b0101,
            DenseIndustrial = 0b0110,
            Military = 0b0111,
            Airport = 0b1000,
            Seaport = 0b1001
        }

        public int altitude;
        public byte terrain;
        public byte building;
        public Zone zone;
        public Corners corners;
        public byte underground;
        public byte textLabel;

        public bool underwater;
    }
}
