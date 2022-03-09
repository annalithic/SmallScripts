using System;
using System.Collections.Generic;
using SoulsFormats;
using System.IO;
using ImageMagick;

namespace SmallScripts {
    class Souls {

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
            }
            //foreach(string path in Directory.EnumerateFiles(@"E:\Extracted\Souls\Elden Ring\map", "*.dds")) {
            //    File.Move(path, Path.GetDirectoryName(path) + "\\" + Path.GetFileName(path).Substring(13));
            //}
           
        }

        public static void EldenRingMapCompose() {
            /*
            Dictionary<uint, uint> paths = new Dictionary<uint, uint>();
            foreach(string tex in Directory.EnumerateFiles(@"E:\Extracted\Souls\Elden Ring\map", "*.dds", SearchOption.TopDirectoryOnly)) {
                string[] words = Path.GetFileNameWithoutExtension(tex).Split('_');
                uint x = ushort.Parse(words[2]); uint y = uint.Parse(words[3]); uint lookup = x + (y << 16);
                uint val = uint.Parse(words[4], System.Globalization.NumberStyles.HexNumber);
                if (!paths.ContainsKey(lookup)) paths[lookup] = val;
                else if (paths[lookup] < val) paths[lookup] = val;
            }
            */
            MagickImage image = new MagickImage(MagickColors.Black, 256 * 41, 256 * 41);

            /*
            int test = 0;
            
            foreach(uint lookup in paths.Keys) {
                //test++; if (test > 10) break;
                string filename = String.Format("M00_L0_{0:00}_{1:00}_{2:X8}.dds", lookup & ushort.MaxValue, lookup >> 16, paths[lookup]);
                Console.WriteLine(filename);
                MagickImage tile = new MagickImage(@"E:\Extracted\Souls\Elden Ring\map\" + filename);
                image.Draw(new Drawables().Composite(
                    (lookup & ushort.MaxValue) * 256,
                    image.Height - (lookup >> 16) * 256 - 256,
                    tile
                    ));
            }
            */

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
            Console.WriteLine("saving");
            image.Write(@"E:\Extracted\Souls\Elden Ring\numbers.tga");
            Console.WriteLine("done");
        }
    }
}
