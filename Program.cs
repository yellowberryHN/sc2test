using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Text;

namespace sc2test
{
    class Program
    {
        public static int buildingCount;
        public static bool includeTrees = true;

        static void Main(string[] args)
        {
            City city = new City();

            string filename = args.Length > 0 ? args[0] : "test.sc2";

#if DEBUG
            // dumb and bad.
            filename = string.Concat("../../../cities/", filename);
#endif

            var rawStream = new MemoryStream();
            using (var fs = File.OpenRead(filename))
            {
                fs.CopyTo(rawStream);
                rawStream.Position = 0;
            }

            var reader = new BinaryReaderBE(rawStream);

            var iffType = Encoding.ASCII.GetString(reader.ReadBytes(4));
            if(iffType != "FORM") throw new InvalidDataException(string.Format("Incorrect IFF type: {0}, expected FORM",iffType));

            var filesize = reader.ReadInt32();
            if(filesize + 8 != reader.BaseStream.Length) throw new InvalidDataException(string.Format("Incorrect filesize: {0}, expected {1}", filesize, reader.BaseStream.Length - 8));

            var container = Encoding.ASCII.GetString(reader.ReadBytes(4));
            if(container != "SCDH") throw new InvalidDataException(string.Format("Incorrect container: {0}, expected SCDH",container));

            while(reader.BaseStream.Position < reader.BaseStream.Length)
            {
                ParseChunk(city, reader);
            }

            Console.WriteLine("City name: {0}", city.name);
            Console.WriteLine("Building count: {0}", buildingCount);
        }

        public static void ParseChunk(City city, BinaryReader reader)
        {
            string chunkId = Encoding.ASCII.GetString(reader.ReadBytes(4));
            int chunkLen = reader.ReadInt32();

            Console.WriteLine("{0} raw: {1}", chunkId, chunkLen);
            
            switch (chunkId)
            {
                case "CNAM": // City Name
                    var cnamLen = reader.ReadByte();
                    byte[] buffer = new byte[cnamLen];
                    reader.Read(buffer, 0, cnamLen);
                    var size = Array.IndexOf(buffer, (byte)0);
                    city.name = Encoding.ASCII.GetString(buffer, 0, size < 0 ? cnamLen : size);
                    break;
                case "MISC": // Misc Data
                    using (var rleReader = new BinaryReaderBE(new MemoryStream(DecompressRLE(reader.ReadBytes(chunkLen)))))
                    {
                        Console.WriteLine("{0} decomp: {1}", chunkId, rleReader.BaseStream.Length);
                        for (int i = 0; i < city.miscData.Length; i++)
                        {
                            city.miscData[i] = rleReader.ReadInt32();
                        }
                        //Console.WriteLine(string.Join(", ", city.miscData));
                    }
                    break;
                case "ALTM": // Altitude
                    for (int y = 0; y < City.MAX_SIZE; y++)
                    {
                        for (int x = 0; x < City.MAX_SIZE; x++)
                        {
                            var altmRaw = reader.ReadBytes(2);
                            city.tiles[y][x].altitude = (byte)(altmRaw[1] & 0x1f);
                        }
                    }
                    break;
                case "XTER": // Terrain Data
                    using (var rleReader = new BinaryReaderBE(new MemoryStream(DecompressRLE(reader.ReadBytes(chunkLen)))))
                    {
                        Console.WriteLine("{0} decomp: {1}", chunkId, rleReader.BaseStream.Length);
                        for (int y = 0; y < City.MAX_SIZE; y++)
                        {
                            for (int x = 0; x < City.MAX_SIZE; x++)
                            {
                                city.tiles[y][x].terrain = rleReader.ReadByte();
                            }
                        }
                    }
                    break;
                case "XBLD": // Buildings
                    using (var rleReader = new BinaryReaderBE(new MemoryStream(DecompressRLE(reader.ReadBytes(chunkLen)))))
                    {
                        Console.WriteLine("{0} decomp: {1}", chunkId, rleReader.BaseStream.Length);
                        for (int y = 0; y < City.MAX_SIZE; y++)
                        {
                            for (int x = 0; x < City.MAX_SIZE; x++)
                            {
                                byte building = rleReader.ReadByte();
                                city.tiles[y][x].building = building;
                                if (building >= 0x0D || (includeTrees && building != 0x00))
                                {
                                    //Console.Write("{0:X2}|", city.buildings[y][x]);
                                    buildingCount++;
                                }
                            }
                        }
                    }
                    break;
                case "XZON": // Zoning
                    using (var rleReader = new BinaryReaderBE(new MemoryStream(DecompressRLE(reader.ReadBytes(chunkLen)))))
                    {
                        Console.WriteLine("{0} decomp: {1}", chunkId, rleReader.BaseStream.Length);
                        for (int y = 0; y < City.MAX_SIZE; y++)
                        {
                            for (int x = 0; x < City.MAX_SIZE; x++)
                            {
                                var zoneRaw = rleReader.ReadByte();
                                city.tiles[y][x].corners = (Tile.Corners)(zoneRaw & 0xf0);
                                city.tiles[y][x].zone = (Tile.Zone)(zoneRaw & 0xf);

                                //Console.WriteLine("Corners: {0}, Zone: {1}", city.tiles[y][x].corners, city.tiles[y][x].zone);
                            }
                        }
                    }
                    break;
                case "XUND": // Underground
                    using (var rleReader = new BinaryReaderBE(new MemoryStream(DecompressRLE(reader.ReadBytes(chunkLen)))))
                    {
                        Console.WriteLine("{0} decomp: {1}", chunkId, rleReader.BaseStream.Length);
                        for (int y = 0; y < City.MAX_SIZE; y++)
                        {
                            for (int x = 0; x < City.MAX_SIZE; x++)
                            {
                                byte underground = rleReader.ReadByte();
                                city.tiles[y][x].underground = underground;
                            }
                        }
                    }
                    break;
                case "XTXT":
                    using (var rleReader = new BinaryReaderBE(new MemoryStream(DecompressRLE(reader.ReadBytes(chunkLen)))))
                    {
                        Console.WriteLine("{0} decomp: {1}", chunkId, rleReader.BaseStream.Length);
                        for (int y = 0; y < City.MAX_SIZE; y++)
                        {
                            for (int x = 0; x < City.MAX_SIZE; x++)
                            {
                                byte textLabel = rleReader.ReadByte();
                                city.tiles[y][x].textLabel = textLabel;
                            }
                        }
                    }
                    break;
                case "XLAB":
                    using (var rleReader = new BinaryReaderBE(new MemoryStream(DecompressRLE(reader.ReadBytes(chunkLen)))))
                    {
                        Console.WriteLine("{0} decomp: {1}", chunkId, rleReader.BaseStream.Length);
                        for (int i = 0; i < 256; i++)
                        {
                            var labelLen = rleReader.ReadByte();
                            if(labelLen > 24) throw new IndexOutOfRangeException(string.Format("Label {0} tried to be longer than 24 characters!", i));
                            city.labels[i] = Encoding.ASCII.GetString(rleReader.ReadBytes(labelLen));
                            //Console.WriteLine("Label {0}: \"{1}\"", i, city.labels[i]); 
                            rleReader.BaseStream.Seek(24-labelLen, SeekOrigin.Current);
                        }
                        //Console.WriteLine(BitConverter.ToString(rleReader.ReadBytes((int)rleReader.BaseStream.Length)).Replace('-', ' '));
                    }
                    break;
                case "XBIT":
                    using (var rleReader = new BinaryReaderBE(new MemoryStream(DecompressRLE(reader.ReadBytes(chunkLen)))))
                    {
                        Console.WriteLine("{0} decomp: {1}", chunkId, rleReader.BaseStream.Length);
                        for (int y = 0; y < City.MAX_SIZE; y++)
                        {
                            for (int x = 0; x < City.MAX_SIZE; x++)
                            {
                                byte flags = rleReader.ReadByte();
                                city.tiles[y][x].flags = (Tile.Flags)flags;
                            }
                        }
                    }
                    break;
                default:
                    // If we don't know what kind of chunk it is, ignore it.
                    reader.BaseStream.Seek(chunkLen, SeekOrigin.Current);
                    break;
            }

            return;
        }

        public static byte[] DecompressRLE(byte[] bytes)
        {
            List<byte> newBytes = new List<byte>();

            using (var reader = new BinaryReaderBE(new MemoryStream(bytes)))
            {
                while (reader.BaseStream.Position < reader.BaseStream.Length)
                {
                    int n = reader.ReadByte();
                    bool alt = n >= 129 ? true : false;

                    if (alt) n -= 127;
                    else if (n == 0) break;

                    byte content = reader.ReadByte();

                    for (int i = 0; i < n; i++) newBytes.Add(alt ? content : (i == 0 ? content : reader.ReadByte()));
                }
            }

            return newBytes.ToArray();
        }
    }
}
