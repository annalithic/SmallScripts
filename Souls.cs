using System;
using System.Collections.Generic;
using SoulsFormats;
using System.IO;
using System.Xml;
using ImageMagick;

namespace SmallScripts {
    class Souls {

        public static void EldenRingBtlUnkDump() {
            foreach(string path in Directory.EnumerateFiles(@"C:\Games\Steam\steamapps\common\ELDEN RING\Game\map", "*.btl.dcx", SearchOption.AllDirectories)) {
                try {
                    BTL btl = BTL.Read(path);
                    for (int i = 0; i < btl.Lights.Count; i++) {
                        var light = btl.Lights[i];
                        Console.Write(Path.GetFileNameWithoutExtension(path) + " ");
                        Console.Write($"UNK64 {light.Unk64[0]} {light.Unk64[1]} {light.Unk64[2]} {light.Unk64[3]} ");
                        Console.Write($"UNK84 {light.Unk84[0]} {light.Unk84[1]} {light.Unk84[2]} {light.Unk84[3]} ");
                        Console.Write($"UNKA0 {light.UnkA0[0]} {light.UnkA0[1]} {light.UnkA0[2]} {light.UnkA0[3]} ");
                        Console.WriteLine($"UNKC0 {light.UnkC0[0]} {light.UnkC0[1]} {light.UnkC0[2]} {light.UnkC0[3]} {light.Name}");
                    }
                } catch { }
            }
        }

        public static void EldenRingWrongWarpMap() {

            MagickImage image = new MagickImage(MagickColors.Black, 7168, 9216);


            foreach (string line in File.ReadAllLines(@"E:\Extracted\Souls\Elden Ring\erplayerwarp.txt")) {
                string[] words = line.Split('|');
                string[] map = words[0].Split('_');
                int cellX = int.Parse(map[1]); int cellY = int.Parse(map[2]);
                float x = float.Parse(words[2]); float y = float.Parse(words[4]);

                float posX = (x + 128 + (cellX - 32) * 256);
                float posY = (9216 - (y + 128 + (cellY - 28) * 256));

                int cellPosX = (128 + (cellX - 32) * 256);
                int cellPosY = (9216 - (128 + (cellY - 28) * 256));

                Console.WriteLine($"{words[0]} {posX} {posY}");

                image.Draw(new Drawables().StrokeColor(MagickColors.White).StrokeWidth(4).FillColor(MagickColors.White).Line(posX, posY, cellPosX, cellPosY));//.Ellipse(posX, posY, 6, 6, 0, 360));

            }

            Console.WriteLine("writing");

            image.Write(@"E:\Extracted\Souls\Elden Ring\warps.png");
            Console.WriteLine("done");
        }

        public static void EldenRingListUnusedAsset() {
            HashSet<string> assets = new HashSet<string>(File.ReadAllLines(@"E:\Extracted\Souls\Elden Ring\aeglist.txt"));
            foreach(string msbPath in Directory.EnumerateFiles(@"E:\Extracted\Souls\Elden Ring\mapstudio\", "*.txt")) {
                foreach(string line in File.ReadAllLines(msbPath)) {
                    int offset = line.IndexOf("AEG");
                    if (offset != -1) assets.Remove(line.Substring(offset, 10));
                }
            }
            foreach (string asset in assets) Console.WriteLine(asset);
        }


        public static void EldenRingWeaponRequirements() {

            HashSet<int> skipMovesetCategories = new HashSet<int>(new int[] { 48, 49, 47, 41, 51, 44, 45, 46, 52 });
            List<string> samurai = new List<string>();
            List<string> vagabond = new List<string>();

            List<string>[] strReqs = new List<string>[100];
            List<string>[] dexReqs = new List<string>[100];
            List<string>[] intReqs = new List<string>[100];
            List<string>[] faiReqs = new List<string>[100];
            List<string>[] arcReqs = new List<string>[100];

            using (TextReader r = new StreamReader(File.OpenRead(@"E:\Extracted\Souls\Elden Ring\Copy of Elden Ring Weapon Data Sheet (1.03) - EquipParamWeapon (1.03).tsv"))) {
                string[] headers = r.ReadLine().Split('\t'); //for (int i = 0; i < headers.Length; i++) Console.WriteLine(string.Format("{0:000} {1}", i, headers[i]));
                string line = r.ReadLine();
                while(line != null) {
                    string[] vals = line.Split('\t');

                    if (vals[0] == vals[26] && vals[1] != "" && !skipMovesetCategories.Contains(int.Parse(vals[72]))) {
                        Console.WriteLine($"{vals[72]} {vals[1]} ");
                        int strReq = int.Parse(vals[81]) ; if (strReq <= 8) strReq = 0;
                        if (strReqs[strReq] == null) strReqs[strReq] = new List<string>();
                        strReqs[strReq].Add(vals[1]);

                        int dexReq = int.Parse(vals[82]); if (dexReq <= 9) dexReq = 0; 
                        if (dexReqs[dexReq] == null) dexReqs[dexReq] = new List<string>();
                        dexReqs[dexReq].Add(vals[1]);

                        int intReq = int.Parse(vals[83]); //if (intReq <= 7) intReq = 0;
                        if (intReqs[intReq] == null) intReqs[intReq] = new List<string>();
                        intReqs[intReq].Add(vals[1]);

                        int faiReq = int.Parse(vals[84]); //if (faiReq <= 6) faiReq = 0;
                        if (faiReqs[faiReq] == null) faiReqs[faiReq] = new List<string>();
                        faiReqs[faiReq].Add(vals[1]);

                        int arcReq = int.Parse(vals[197]); //if (arcReq <= 7) arcReq = 0;
                        if (arcReqs[arcReq] == null) arcReqs[arcReq] = new List<string>();
                        arcReqs[arcReq].Add(vals[1]);

                        if (strReq <= 28 && dexReq <= 18 && intReq <= 9 && faiReq <= 9 && arcReq <= 7) vagabond.Add(vals[1]);
                        if (strReq <= 25 && dexReq <= 20 && intReq <= 9 && faiReq <= 8 && arcReq <= 8) samurai.Add(vals[1]);
                    }

                    line = r.ReadLine();
                }
            }
            /*
            Console.WriteLine("\nSTR");
            for(int i = 0; i < 99; i++) {
                if (strReqs[i] != null) {
                    Console.Write(string.Format("{0:00}: ", i));
                    for (int w = 0; w < strReqs[i].Count; w++) Console.Write(strReqs[i][w] + ", ");
                    Console.WriteLine();
                }
            }

            Console.WriteLine("\nDEX");
            for (int i = 0; i < 99; i++) {
                if (dexReqs[i] != null) {
                    Console.Write(string.Format("{0:00}: ", i));
                    for (int w = 0; w < dexReqs[i].Count; w++) Console.Write(dexReqs[i][w] + ", ");
                    Console.WriteLine();
                }
            }

            Console.WriteLine("\nINT");
            for (int i = 1; i < 99; i++) {
                if (intReqs[i] != null) {
                    Console.Write(string.Format("{0:00}: ", i));
                    for (int w = 0; w < intReqs[i].Count; w++) Console.Write(intReqs[i][w] + ", ");
                    Console.WriteLine();
                }
            }

            Console.WriteLine("\nFAI");
            for (int i = 1; i < 99; i++) {
                if (faiReqs[i] != null) {
                    Console.Write(string.Format("{0:00}: ", i));
                    for (int w = 0; w < faiReqs[i].Count; w++) Console.Write(faiReqs[i][w] + ", ");
                    Console.WriteLine();
                }
            }

            Console.WriteLine("\nARC");
            for (int i = 1; i < 99; i++) {
                if (arcReqs[i] != null) {
                    Console.Write(string.Format("{0:00}: ", i));
                    for (int w = 0; w < arcReqs[i].Count; w++) Console.Write(arcReqs[i][w] + ", ");
                    Console.WriteLine();
                }
            }
            */

            Console.WriteLine($"\nVAGABOND {vagabond.Count}");
            foreach (string weap in vagabond) if (!samurai.Contains(weap)) Console.Write(weap + ", ");

            Console.WriteLine($"\n\nVAGABOND {samurai.Count}");
            foreach (string weap in samurai) if (!vagabond.Contains(weap)) Console.Write(weap + ", ");

        }

        public static void EldenRingListMapMask() {
            XmlDocument doc = new XmlDocument();
            doc.Load(@"C:\Games\Steam\steamapps\common\ELDEN RING\Game\menu\71_maptile-mtmskbnd-dcx\GR\data\INTERROOT_win64\menu\ScaleForm\maptile\mask\MENU_MapTile_M00.mtmsk");
            XmlNode root = doc.LastChild;
            for(int i = 0; i < root.ChildNodes.Count; i++) {
                XmlNode node = root.ChildNodes[i];
                string coord = string.Format("{0:00000}", int.Parse(node.Attributes[1].Value));
                string value = string.Format("{0:X8}", int.Parse(node.Attributes[2].Value));
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
            foreach(string path in Directory.EnumerateFiles(dir, "*.tpf")) {
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
            foreach (string tex in Directory.EnumerateFiles(@"E:\Extracted\Souls\Elden Ring\maptest\extra\", "*.dds", SearchOption.TopDirectoryOnly)) {
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
            

            foreach (string tex in Directory.EnumerateFiles(@"E:\Extracted\Souls\Elden Ring\maptest\extra\", "*.dds", SearchOption.TopDirectoryOnly)) {
                
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
            image.Write($@"E:\Extracted\Souls\Elden Ring\maptest\extra2\{searchVal}.png");
        }

        public static void EldenRingMapSort() {
            Dictionary<string, uint> values = new Dictionary<string, uint>();

            foreach(string line in File.ReadAllLines(@"E:\Extracted\Souls\Elden Ring\mapmask.txt")) {
                if (!line.StartsWith("MENU_MAPTILE_M00_L2")) continue;
                values[line.Substring(0, 26)] = uint.Parse(line.Substring(26, 8), System.Globalization.NumberStyles.HexNumber);
            }


            foreach(string file in Directory.EnumerateFiles(@"C:\Games\Steam\steamapps\common\ELDEN RING\Game\menu\71_maptile-tpfbhd\71_MapTile", "*.dds")) {
                string filename = Path.GetFileNameWithoutExtension(file).ToUpper();
                if (!filename.StartsWith("MENU_MAPTILE_M00_L2")) continue;
                uint value = uint.Parse(filename.Substring(26, 8), System.Globalization.NumberStyles.HexNumber);

                if (value == 0) { Console.WriteLine("EMPT " + filename);
                    File.Copy(file, @"E:\Extracted\Souls\Elden Ring\maptest2\empty\" + Path.GetFileName(file), true);
                    continue; 
                }
                string nameKey = filename.Substring(0, 26);
                if (values.ContainsKey(nameKey)) {
                    if (values[nameKey] == value) { 
                        Console.WriteLine("FULL " + filename);
                        File.Copy(file, @"E:\Extracted\Souls\Elden Ring\maptest2\full\" + Path.GetFileName(file), true);
                    } else if ((values[nameKey] | value) > values[nameKey]) { 
                        Console.WriteLine("XTRA  " + filename);
                        File.Copy(file, @"E:\Extracted\Souls\Elden Ring\maptest2\extra\" + Path.GetFileName(file), true);
                    } else {
                        Console.WriteLine("PART  " + filename);
                        File.Copy(file, @"E:\Extracted\Souls\Elden Ring\maptest2\partial\" + Path.GetFileName(file), true);
                    }
                } else {
                    Console.WriteLine("UNUS " + filename);
                }

            }
        }


        public static void EldenRingMapCompose4() {


            uint[,] vals = new uint[41,41];

            XmlDocument doc = new XmlDocument();
            doc.Load(@"C:\Games\Steam\steamapps\common\ELDEN RING\Game\menu\71_maptile-mtmskbnd-dcx\GR\data\INTERROOT_win64\menu\ScaleForm\maptile\mask\MENU_MapTile_M00.mtmsk");
            XmlNode root = doc.LastChild;
            for (int i = 0; i < root.ChildNodes.Count; i++) {
                XmlNode node = root.ChildNodes[i];

                string coord = string.Format("{0:00000}", int.Parse(node.Attributes[1].Value));
                if (!coord.StartsWith("0")) continue;
                int x = int.Parse(coord.Substring(1, 2)); int y = int.Parse(coord.Substring(3, 2));

                vals[x, y] = uint.Parse(node.Attributes[2].Value);
            }


            MagickImage image = new MagickImage(MagickColors.Black, 256 * 41, 256 * 41);


            foreach (string tex in Directory.EnumerateFiles(@"C:\Games\Steam\steamapps\common\ELDEN RING\Game\menu\71_maptile-tpfbhd\71_MapTile\", "*.dds", SearchOption.TopDirectoryOnly)) {
                string filename = Path.GetFileNameWithoutExtension(tex).ToUpper();
                if (!filename.StartsWith("MENU_MAPTILE_M00_L0")) continue;
                string[] words = filename.Split('_');
                uint x = ushort.Parse(words[4]); uint y = uint.Parse(words[5]);
                uint val = uint.Parse(words[6], System.Globalization.NumberStyles.HexNumber);

                if(vals[x,y] == val) {
                    MagickImage tile = new MagickImage(tex);
                    image.Draw(new Drawables().Composite(x * 256, image.Height - y * 256 - 256, tile));
                    Console.WriteLine(filename);
                }
            }
            Console.WriteLine("saving");
            image.Write(@"E:\Extracted\Souls\Elden Ring\map1.03.tga");
            Console.WriteLine("done");
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
            
            foreach(uint lookup in paths.Keys) {
                string filename = String.Format("M00_L0_{0:00}_{1:00}_{2:X8}.dds", lookup & ushort.MaxValue, lookup >> 16, paths[lookup]);
                Console.WriteLine(filename);
                MagickImage tile = new MagickImage(@"E:\Extracted\Souls\Elden Ring\worldmap\" + filename);
                image.Draw(new Drawables().Composite(
                    (lookup & ushort.MaxValue) * 256,
                    image.Height - (lookup >> 16) * 256 - 256,
                    tile
                    ));
            }

            Console.WriteLine("saving");
            image.Write(@"E:\Extracted\Souls\Elden Ring\map1.03.tga");
            Console.WriteLine("done");
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
    }
}
