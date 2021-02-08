using System;
using System.Collections.Generic;
using System.Text;

namespace sc2test
{
    public class City
    {
        public static int MAX_SIZE = 128;

        public string name;

        public Tile[][] tiles;

        public int[] miscData = new int[1200];

        public string[] labels = new string[256];

        public City()
        {
            tiles = new Tile[MAX_SIZE][];
            for (int i = 0; i < MAX_SIZE; i++)
            {
                tiles[i] = new Tile[MAX_SIZE];
                for (int j = 0; j < MAX_SIZE; j++)
                {
                    tiles[i][j] = new Tile();
                }
            }
        }
    }
}
