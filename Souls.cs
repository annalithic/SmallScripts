using System;
using System.Collections.Generic;
using SoulsFormats;
using System.IO;
using System.Xml;
using ImageMagick;

namespace SmallScripts {
    class Souls {

        public static void EldenRingListMapMask() {
            XmlDocument doc = new XmlDocument();
            doc.Load(@"C:\Games\Steam\steamapps\common\ELDEN RING\Game\menu\71_maptile-mtmskbnd-dcx\GR\data\INTERROOT_win64\menu\ScaleForm\maptile\mask\MENU_MapTile_M00.mtmsk");
            XmlNode root = doc.LastChild;
            for(int i = 0; i < root.ChildNodes.Count; i++) {
                //if (i > 100) break;
                XmlNode node = root.ChildNodes[i];
                string coord = string.Format("{0:00000}", int.Parse(node.Attributes[1].Value));
                string value = string.Format("{0:X8}", int.Parse(node.Attributes[2].Value));
                //int x = int.Parse(coord.Substring(1, 2)); int y = int.Parse(coord.Substring(3, 2)); int level = int.Parse(coord.Substring(0, 1));
                Console.WriteLine($"M00_L{coord[0]}_{coord.Substring(1,2)}_{coord.Substring(3,2)}_{value}");
            }
        }

        public static void EldenRingUpdateListNewMapParts() {
            foreach (string path in File.ReadAllLines(@"E:\Extracted\Souls\Elden Ring\1_0_3_changedmaps.txt")) {
                HashSet<string> oldParts = new HashSet<string>();
                foreach (string line in File.ReadAllLines(path.Replace("\\mapstudio\\", "\\mapstudio1.02\\"))) oldParts.Add(line.Substring(0, line.IndexOf('|')));
                foreach (string line in File.ReadAllLines(path)) {
                    string[] words = line.Split('|');
                    if (!oldParts.Contains(words[0])) Console.WriteLine($"{Path.GetFileNameWithoutExtension(path)}.msb | {words[0]} | &{words[1]} | ({words[2]},{words[3]},{words[4]})");
                }
            }
        }
        public static void EldenRingListUnusedMapPieces() {
            

            HashSet<string> mapPieces = new HashSet<string>();

            foreach (string line in File.ReadAllLines(@"E:\Extracted\Souls\Elden Ring\mapbnds.txt")) mapPieces.Add(line);

            foreach(string path in Directory.EnumerateFiles(@"E:\Extracted\Souls\Elden Ring\mapstudio\", "*.txt")) {
                foreach(string line in File.ReadAllLines(path)) {
                    if (line[line.IndexOf('|') + 1] != '0') continue;
                    mapPieces.Remove(Path.GetFileNameWithoutExtension(path) + '_' + line.Substring(1, line.IndexOf('_') - 1));
                }

            }

            foreach (string m in mapPieces) Console.WriteLine(m);
            
        }

        public static void EldenRingLegacyMapConv() {
            Dictionary<string, string> mapNames = new Dictionary<string, string>();
            foreach (string line in File.ReadAllLines(@"E:\Extracted\Souls\Elden Ring\mapnames.txt")) {
                string[] words = line.Split(':');
                mapNames[words[0]] = words[1];
            }
            foreach(string line in File.ReadAllLines(@"E:\Extracted\Souls\Elden Ring\legacyconv.txt")) {
                string[] words = line.Split('\t');
                string a = mapNames.ContainsKey(words[0]) ? mapNames[words[0]] : words[0];
                string b = mapNames.ContainsKey(words[1]) ? mapNames[words[1]] : words[1];
                Console.WriteLine(a + "|" + b);
            }

        }

        public static void EldenRingBossList() {
            Dictionary<string, string> mapNames = new Dictionary<string, string>();
            foreach(string line in File.ReadAllLines(@"E:\Extracted\Souls\Elden Ring\mapnames.txt")) {
                string[] words = line.Split(':');
                mapNames[words[0]] = words[1];
            }
            foreach(string line in File.ReadAllLines(@"E:\Extracted\Souls\Elden Ring\bosses.txt")) {
                string[] words = line.Split(':');
                if (!mapNames.ContainsKey(words[0])) Console.WriteLine(words[0]); else
                Console.WriteLine($"{words[1]}}}{words[0]}}}{mapNames[words[0]]}");
            }
        }

        public static void EldenRingUnpackTex(string dir) {
            foreach(string path in Directory.EnumerateFiles(dir, "*.tpf.dcx")) {
                TPF tpf = TPF.Read(path);
                if (tpf.Textures.Count != 1) Console.WriteLine(path);
                File.WriteAllBytes(path.Substring(0, path.Length - 7) + "dds", tpf.Textures[0].Bytes);
                File.Delete(path);
            }
            //foreach(string path in Directory.EnumerateFiles(@"E:\Extracted\Souls\Elden Ring\map", "*.dds")) {
            //    File.Move(path, Path.GetDirectoryName(path) + "\\" + Path.GetFileName(path).Substring(13));
            //}
           
        }

        public static void EldenRingMapCompose3() {
            HashSet<uint> vals = new HashSet<uint>();
            foreach (string tex in Directory.EnumerateFiles(@"E:\Extracted\Souls\Elden Ring\maptest\extra\old\", "*.dds", SearchOption.TopDirectoryOnly)) {
                string[] words = Path.GetFileNameWithoutExtension(tex).Split('_');
                uint val = uint.Parse(words[6], System.Globalization.NumberStyles.HexNumber);
                vals.Add(val);
            }
            foreach (uint val in vals) EldenRingMapCompose2(val);
        }

        public static void EldenRingMapCompose2(uint searchVal) {

            Console.WriteLine(searchVal);

            int tiles = 41;
            int tileSize = 256;
            MagickImage image = new MagickImage(MagickColor.FromRgb(0,255,0), tiles * tileSize, tiles * tileSize);
            

            foreach (string tex in Directory.EnumerateFiles(@"E:\Extracted\Souls\Elden Ring\maptest\extra\old\", "*.dds", SearchOption.TopDirectoryOnly)) {
                
                string[] words = Path.GetFileNameWithoutExtension(tex).Split('_');
                uint x = ushort.Parse(words[4]); uint y = uint.Parse(words[5]); uint lookup = x + (y << 16);
                uint val = uint.Parse(words[6], System.Globalization.NumberStyles.HexNumber);
                if (val != searchVal) continue;
                Console.WriteLine(Path.GetFileName(tex));
                MagickImage tile = new MagickImage(tex);
                image.Draw(new Drawables().Composite(
                    x * tileSize,
                    image.Height - y * tileSize - tileSize,
                    tile
                    ));
            }
            Console.WriteLine("Writing...");
            image.Write($@"E:\Extracted\Souls\Elden Ring\maptest\extracombined\{searchVal}.png");
        }

        public static void EldenRingMapSort() {
            Dictionary<string, uint> values = new Dictionary<string, uint>();

            foreach(string line in File.ReadAllLines(@"E:\Extracted\Souls\Elden Ring\mapmask.txt")) {
                if (!line.StartsWith("MENU_MAPTILE_M00_L0")) continue;
                values[line.Substring(0, 26)] = uint.Parse(line.Substring(26, 8), System.Globalization.NumberStyles.HexNumber);
            }


            foreach(string file in Directory.EnumerateFiles(@"C:\Games\Steam\steamapps\common\ELDEN RING\Game\menu\71_maptile-tpfbhd\71_MapTile", "*.dds")) {
                string filename = Path.GetFileNameWithoutExtension(file).ToUpper();
                if (!filename.StartsWith("MENU_MAPTILE_M00_L0")) continue;
                uint value = uint.Parse(filename.Substring(26, 8), System.Globalization.NumberStyles.HexNumber);

                if (value == 0) { Console.WriteLine("EMPT " + filename);
                    File.Copy(file, @"E:\Extracted\Souls\Elden Ring\maptest\empty\" + Path.GetFileName(file));
                    continue; 
                }
                string nameKey = filename.Substring(0, 26);
                if (values.ContainsKey(nameKey)) {
                    if (values[nameKey] == value) { 
                        Console.WriteLine("FULL " + filename);
                        File.Copy(file, @"E:\Extracted\Souls\Elden Ring\maptest\full\" + Path.GetFileName(file));
                    } else if ((values[nameKey] | value) > values[nameKey]) { 
                        Console.WriteLine("XTRA  " + filename);
                        File.Copy(file, @"E:\Extracted\Souls\Elden Ring\maptest\extra\" + Path.GetFileName(file));
                    } else {
                        Console.WriteLine("PART  " + filename);
                        File.Copy(file, @"E:\Extracted\Souls\Elden Ring\maptest\partial\" + Path.GetFileName(file));
                    }
                } else {
                    Console.WriteLine("UNUS " + filename);
                }

            }
        }

        public static void EldenRingMapCompose() {

            HashSet<string> usedTiles = new HashSet<string>(File.ReadAllLines(@"E:\Extracted\Souls\Elden Ring\mapmask.txt"));


            
            Dictionary<uint, uint> paths = new Dictionary<uint, uint>();
            foreach(string tex in Directory.EnumerateFiles(@"C:\Games\Steam\steamapps\common\ELDEN RING\Game\menu\71_maptile-tpfbhd\71_MapTile\", "*.dds", SearchOption.TopDirectoryOnly)) {
                string filename = Path.GetFileNameWithoutExtension(tex).ToUpper();
                if (!filename.StartsWith("MENU_MAPTILE_M00_L0")) continue;
                string[] words = filename.Split('_');
                uint x = ushort.Parse(words[4]); uint y = uint.Parse(words[5]); uint lookup = x + (y << 16);
                uint val = uint.Parse(words[6], System.Globalization.NumberStyles.HexNumber);
                if (usedTiles.Contains(filename)) {
                    paths[lookup] = val;
                } else if (val == 0 && !paths.ContainsKey(lookup)) paths[lookup] = 0;
            }

            MagickImage image = new MagickImage(MagickColors.Black, 256 * 41, 256 * 41);
            

            
            int test = 0;

            foreach(uint lookup in paths.Keys) {
                //test++; if (test > 10) break;
                string filename = String.Format("M00_L0_{0:00}_{1:00}_{2:X8}.dds", lookup & ushort.MaxValue, lookup >> 16, paths[lookup]);
                Console.WriteLine(filename);
                //MagickImage tile = new MagickImage(@"C:\Games\Steam\steamapps\common\ELDEN RING\Game\menu\71_maptile-tpfbhd\71_MapTile\" + filename);
                MagickImage tile = new MagickImage(@"E:\Extracted\Souls\Elden Ring\worldmap\" + filename);
                image.Draw(new Drawables().Composite(
                    (lookup & ushort.MaxValue) * 256,
                    image.Height - (lookup >> 16) * 256 - 256,
                    tile
                    ));
            }
            

            //NUMBERS STUFF
            /*
            HashSet<string> maps = new HashSet<string>(File.ReadAllLines(@"E:\Extracted\Souls\Elden Ring\m60.txt"));
            int startX = 28; int startY = 24; int cellSize = 256; int textSize = 48; //00
            //int startX = 14; int startY = 12; int cellSize = 512; int textSize = 64; //01
            //int startX = 7; int startY = 6; int cellSize = 1024; int textSize = 96; //02

            for (int y = 0; y < 41; y++) {
                for (int x = 0; x < 41; x++) {
                    if(maps.Contains(string.Format(@"/map/mapstudio/m60_{0:00}_{1:00}_10.msb.dcx", x + startX, y + startY)))
                    image.Draw(new Drawables().FontPointSize(textSize).Font("Lucida Console").FillColor(MagickColors.White).TextAlignment(TextAlignment.Center)
                        .Text(cellSize * x + cellSize/2, image.Height - cellSize * y - cellSize/2, 
                        string.Format("{0:00}_{1:00}_10", x+startX, y+startY)));
                }
            }
            */
            Console.WriteLine("saving");
            image.Write(@"E:\Extracted\Souls\Elden Ring\map1.02.tga");
            Console.WriteLine("done");
        }
    }
}
