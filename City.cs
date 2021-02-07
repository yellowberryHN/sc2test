using System;
using System.Collections.Generic;
using System.Text;

namespace sc2test
{
    public class City
    {
        public static int MAX_SIZE = 128;

        public string name;
        public byte[][] altitude;
        public byte[][] terrain;
        public byte[][] buildings;

        public City()
        {
            altitude = new byte[MAX_SIZE][];
            for (int i = 0; i < MAX_SIZE; i++)
            {
                altitude[i] = new byte[MAX_SIZE];
            }

            terrain = new byte[MAX_SIZE][];
            for (int i = 0; i < MAX_SIZE; i++)
            {
                terrain[i] = new byte[MAX_SIZE];
            }

            buildings = new byte[MAX_SIZE][];
            for (int i = 0; i < MAX_SIZE; i++)
            {
                buildings[i] = new byte[MAX_SIZE];
            }
        }
    }
}
