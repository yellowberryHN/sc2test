using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Text;

namespace sc2test
{
    class Program
    {
        static void Main(string[] args)
        {
            City city = new City();

            string filename = args.Length > 0 ? args[0] : "test.sc2";

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
        }

        public static void ParseChunk(City city, BinaryReader reader)
        {
            string chunkId = Encoding.ASCII.GetString(reader.ReadBytes(4));
            int chunkLen = reader.ReadInt32();

            Console.WriteLine("{0}: {1} bytes raw", chunkId, chunkLen);
            
            switch (chunkId)
            {
                case "CNAM": // City Name
                    var cnamLen = reader.Read();
                    byte[] buffer = new byte[cnamLen];
                    reader.Read(buffer, 0, cnamLen);
                    var size = Array.IndexOf(buffer, (byte)0);
                    city.name = Encoding.ASCII.GetString(buffer, 0, size < 0 ? cnamLen : size);
                    break;
                case "MISC": // Misc Data
                    using (var rleReader = new BinaryReader(new MemoryStream(unRLE(reader.ReadBytes(chunkLen)))))
                    {
                        Console.WriteLine(BitConverter.ToString(rleReader.ReadBytes((int)rleReader.BaseStream.Length)));
                    }
                    break;
                case "ALTM": // Altitude
                    for (int y = 0; y < City.MAX_SIZE; y++)
                    {
                        for (int x = 0; x < City.MAX_SIZE; x++)
                        {
                            var altmRaw = reader.ReadBytes(2);
                            city.altitude[y][x] = (byte)(altmRaw[1] & 0x1f);
                        }
                    }
                    break;
                case "XTER": // Terrain Data
                    using (var rleReader = new BinaryReader(new MemoryStream(unRLE(reader.ReadBytes(chunkLen)))))
                    {
                        for (int y = 0; y < City.MAX_SIZE; y++)
                        {
                            for (int x = 0; x < City.MAX_SIZE; x++)
                            {
                                city.terrain[y][x] = rleReader.ReadByte();
                            }
                        }
                    }
                    break;
                case "XBLD": // Buildings
                    using (var rleReader = new BinaryReader(new MemoryStream(unRLE(reader.ReadBytes(chunkLen)))))
                    {
                        for (int y = 0; y < City.MAX_SIZE; y++)
                        {
                            for (int x = 0; x < City.MAX_SIZE; x++)
                            {
                                city.buildings[y][x] = rleReader.ReadByte();
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

        public static byte[] unRLE(byte[] bytes)
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
