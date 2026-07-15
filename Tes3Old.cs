using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using ImageMagick;
using ImageMagick.Formats;
using Newtonsoft.Json.Linq;

namespace SmallScripts {
    internal class Tes3Old {
        public static void TES3IntCellResizeTest() {
            foreach (string imagePath in Directory.EnumerateFiles(@"F:\Extracted\Morrowind\tombs", "*.bmp")) {
                MagickImage image = new MagickImage(imagePath);
                image.Resize(image.Width / 2, image.Height / 2);
                image.Trim(); image.RePage();
                image.Draw(new Drawables().FillColor(MagickColors.White).FontPointSize(24).Gravity(Gravity.Northwest).Text(4, 4, Path.GetFileName(imagePath).Split('.')[0]));
                image.Write(Path.Combine(@"F:\Extracted\Morrowind\tombsa", Path.GetFileNameWithoutExtension(imagePath) + ".png"));
                Console.WriteLine(imagePath);

            }
        }

        public static void TES3ListInts(string espPath) {
            JArray esp = JArray.Parse(File.ReadAllText(espPath));
            List<string> ints = new List<string>();

            for (int i = 0; i < esp.Count; i++) {
                if (esp[i]["type"] != null && esp[i]["type"].Value<string>() == "Cell" && ((esp[i]["data"]["flags"].Value<int>() & 1) == 1)) {
                    ints.Add(esp[i]["id"].Value<string>());
                }
            }
            if (ints.Count == 0) return;

            int groupSize = 400;
            for (int start = 0; start < ints.Count; start += groupSize) {
                Console.WriteLine("if(i<1)");
                Console.WriteLine("coc,\"" + ints[start] + "\"");
                for (int i = 1; i < groupSize; i++) {
                    if (i + start >= ints.Count) break;
                    Console.WriteLine($"elseif(i<{i + 1})");
                    Console.WriteLine("coc,\"" + ints[i + start] + "\"");
                }
                Console.WriteLine("\r\n\r\n\r\n\r\n\r\n\r\n\r\n\r\n\r\n\r\n");
            }

        }

        public static void TES3GridmapCoords() {
            MagickImage image = new MagickImage(MagickColors.Black, 40 * 128, 40 * 128);
            for (int x = 0; x < 128; x++) {
                Console.WriteLine(x);
                for (int y = 0; y < 128; y++) {
                    image.Draw(new Drawables().FillColor(MagickColors.ForestGreen).Text(x * 40, 128 * 40 - y * 40 - 40, $"{x - 64},{y - 64}"));
                }
            }
            image.Write("gridmapcoords.png");
        }

        public static void MWRegionCreateMaps(string espPath) {
            int minX = int.MaxValue; int minY = int.MaxValue;
            int maxX = int.MinValue; int maxY = int.MinValue;

            JArray esp = JArray.Parse(File.ReadAllText(espPath));
            string[,] cellRegions = new string[256, 256];
            for (int i = 0; i < esp.Count; i++) {
                if (esp[i]["type"] != null && esp[i]["type"].Value<string>() == "Cell" && ((esp[i]["data"]["flags"].Value<int>() & 1) == 0)) {
                    //string cellName = esp[i]["id"].Value<string>();
                    var cellX = esp[i]["data"]["grid"][0].Value<int>();
                    var cellY = esp[i]["data"]["grid"][1].Value<int>();
                    string region = esp[i]["region"] is null ? "Wilderness" : esp[i]["region"].Value<string>();
                    if (region != "Wilderness") {
                        Console.WriteLine($"coe,{cellX},{cellY}");
                        if (cellX > maxX) maxX = cellX;
                        if (cellY > maxY) maxY = cellY;
                        if (cellX < minX) minX = cellX;
                        if (cellY < minY) minY = cellY;

                    }
                }
            }

            Console.WriteLine();
            Console.WriteLine($"{minX},{minY} to {maxX},{maxY}");
            //foreach (string cell in cells) Console.WriteLine(cell);

        }

        public static void TES3WeatherValues() {
            string[] weatherTypes = new string[] { "Clear", "Cloudy", "Foggy", "Thunderstorm", "Rain", "Overcast", "Ashstorm", "Blight", "Snow", "Blizzard" };
            string[] tods = new string[] { "Sunrise", "Day", "Sunset", "Night" };
            string[] weatherValues = new string[] { "Sky", "Fog", "Ambient", "Sun" };

            foreach (string value in weatherValues) {
                Console.WriteLine($"    vec4 {value}Colors[{weatherTypes.Length * tods.Length}]=vec4[](");
                foreach (string weather in weatherTypes) {
                    foreach (string tod in tods) {
                        string search = $"Weather_{weather}_{value}_{tod}_Color";
                        foreach (string line in File.ReadAllLines(@"F:\Extracted\Morrowind\weathercolors.txt")) {
                            if (line.StartsWith(search)) {
                                var words = line.Split(',');
                                float r = ((float)byte.Parse(words[1])) / 255;
                                float g = ((float)byte.Parse(words[2])) / 255;
                                float b = ((float)byte.Parse(words[3])) / 255;
                                Console.WriteLine($"        vec4({r},{g},{b},1),");
                            }
                        }

                    }
                }
                Console.WriteLine("    );\r\n");
            }
        }

        public static void TES3WeatherColors() {
            string[] lines = File.ReadAllLines(@"F:\Extracted\Morrowind\weathercolors.txt");
            MagickImage image = new MagickImage(MagickColors.White, 512, lines.Length * 32);
            for (int i = 0; i < lines.Length; i++) {
                var words = lines[i].Split(',');
                byte r = byte.Parse(words[1]);
                byte g = byte.Parse(words[2]);
                byte b = byte.Parse(words[3]);
                MagickColor color = new MagickColor(r, g, b);

                image.Draw(new Drawables().FillColor(color).Rectangle(0, 32 * i, 512, 32 * i + 32));
                image.Draw(new Drawables().StrokeColor(MagickColors.Black).StrokeWidth(4).Text(4, 32 * i + 16, words[0]));
                image.Draw(new Drawables().FillColor(MagickColors.White).Text(4, 32 * i + 16, words[0]));

                Console.WriteLine(words[0]);
                //if (i > 10) break;
            }
            image.Write("MorrowindWeatherColors.png");

        }

        public static void FO3LodCombine() {
            MagickImageCollection lodimages = new MagickImageCollection();
            for (int y = 0; y < 32; y++) {
                for (int x = 31; x >= 0; x--) {
                    string path = $@"F:\Extracted\BGS\Fallout3\textures\landscape\lod\wasteland\diffuse\wasteland.n.level4.x{x * 4 - 64}.y{y * 4 - 64}.dds";
                    lodimages.Add(new MagickImage(path));
                }
            }
            var combined = lodimages.Montage(new MontageSettings() { Geometry = new MagickGeometry(256) });
            Console.WriteLine("Writing...");
            combined.Write("fo3.png");
            Console.WriteLine("Done.");
        }

        public static void FO76DepthMap(string path) {
            MagickImage image = new MagickImage(path);
            ushort[] data = new ushort[image.Width * image.Height];
            int i = 0;
            foreach (var pixel in image.GetPixels()) {
                var color = pixel.ToColor();
                int col = color.G + color.B / 256;
                //Console.WriteLine(col);
                data[i] = (ushort)col;
                i++;
            }
            byte[] byteData = new byte[data.Length * 2];
            Buffer.BlockCopy(data, 0, byteData, 0, byteData.Length);

            //MagickImage output = new MagickImage(byteData, new PixelReadSettings(image.Width, image.Height, StorageType.Short, "R"));

            //output.Format = MagickFormat.Gray;
            //output.Depth = 16;

            //output.Write(path.Replace(".dds", ".r16"));
        }
        /*
		public static void MWTesAnnwynColorMap() {
			Dictionary<MagickColor, MagickColor> colorMap = new Dictionary<MagickColor, MagickColor>();
			colorMap[MagickColors.Black] = new MagickColor(0, 0, 0);

			string[] lines = File.ReadAllLines(@"F:\Extracted\BethesdaGameStudioUtils\TESAnnwyn\tesannwyn-ltex3.dat");

			MagickImage defaultLand = new MagickImage(@"E:\Programs\Steam\steamapps\common\Morrowind\Data Files\textures\_land_default.tga");
			defaultLand.Resize(1, 1);
			MagickColor defaultColor = (MagickColor)defaultLand.GetPixels().GetPixel(0, 0).ToColor();
			colorMap[MagickColors.Black] = defaultColor;


			foreach (string line in lines) {
				string[] words = line.Split(',');
				if (words.Length < 4) continue;

				string texname = words[2].ToLower().Replace(".tga", ".dds");
				if (File.Exists(@"F:\Extracted\Morrowind\textures\" + texname)) {
					MagickImage image = new MagickImage(@"F:\Extracted\Morrowind\textures\" + texname);

					image.Resize(1, 1);
					ushort g = (ushort) (byte.Parse(words[0]) * 256);
					//g++;
					MagickColor c1 = new MagickColor(0, 0, 0);
					MagickColor c2 = (MagickColor)image.GetPixels().GetPixel(0, 0).ToColor();
					colorMap[c1] = c2;
					Console.WriteLine($"{c1.R} {c1.G} {c1.B} - {c2.R} {c2.G} {c2.B} - {texname}");
					//Console.WriteLine($"{words[0]} {texname} {color.R} {color.G} {color.B}");
				}
			}


			BinaryReader r = new BinaryReader(File.OpenRead(@"F:\Extracted\BethesdaGameStudioUtils\TESAnnwyn\tesannwyn-vtex3.bmp"));
			r.BaseStream.Seek(18, SeekOrigin.Begin);
			int width = r.ReadInt32();
			int height = r.ReadInt32();
			
			PixelReadSettings pixelRead = new PixelReadSettings(width, height, StorageType.Short, "R");

			r.BaseStream.Seek(54, SeekOrigin.Begin);
			byte[] data = r.ReadBytes(2 * width * height);

			for (byte i = 0; i < 107; i++) data[i * 2] = (byte) (i + 1);

			r.Close();
			MagickImage map = new MagickImage(data, pixelRead);
			map.Flip();
			map.Evaluate(Channels.All, EvaluateOperator.Multiply, 256);
			//map.Depth = 8;
			//map.ColorSpace = ColorSpace.RGB;
			//map.Format = MagickFormat.Rgb;

			Console.WriteLine(map.Depth);
			Console.WriteLine(map.Format);

			//map.Write(@"F:\Extracted\BethesdaGameStudioUtils\TESAnnwyn\tesannwyn-vtex3-recolor.png");
			//map.Depth = 8; map.Format = MagickFormat.Rgb;

			//MagickReadSettings colorReadSettings = new MagickReadSettings { Format = MagickFormat.Rgb, Depth = 16, Width = width, Height = height };
			MagickImage colorImage = new MagickImage(MagickColors.Red, width, height);
			colorImage.Composite(map);

			colorImage.ColorFuzz = new Percentage(0);
			foreach (var color in colorMap.Keys) {
				colorImage.Opaque(color, colorMap[color]);
			}
			colorImage.Write(@"F:\Extracted\BethesdaGameStudioUtils\TESAnnwyn\tesannwyn-vtex3-recolor.png");






		}
		*/

        public static void MWRegionDoors(string espPath) {
            int cellSize = 64;
            int xAdd = 42 * 8192;
            int yAdd = 38 * 8192;

            //HashSet<string> cells = new HashSet<string>();
            Dictionary<string, string> cellTypes = new Dictionary<string, string>();
            foreach (string line in File.ReadAllLines(@"F:\Extracted\Morrowind\celltypes.txt")) {
                var split = line.Split('\t');
                cellTypes[split[0]] = split[1];
            }


            JArray esp = JArray.Parse(File.ReadAllText(espPath));
            string[,] cellRegions = new string[256, 256];
            for (int i = 0; i < esp.Count; i++) {
                if (esp[i]["type"] != null && esp[i]["type"].Value<string>() == "Cell" && ((esp[i]["data"]["flags"].Value<int>() & 1) == 0)) {
                    //string cellName = esp[i]["id"].Value<string>();
                    var cellX = esp[i]["data"]["grid"][0].Value<int>();
                    var cellY = esp[i]["data"]["grid"][1].Value<int>();
                    string region = esp[i]["region"] is null ? "Wilderness" : esp[i]["region"].Value<string>();
                    cellRegions[128 + cellX, 128 + cellY] = region;
                }
            }

            Dictionary<string, string> intCellRegions = new Dictionary<string, string>();


            for (int i = 0; i < esp.Count; i++) {
                if (esp[i]["type"] != null && esp[i]["type"].Value<string>() == "Cell" && ((esp[i]["data"]["flags"].Value<int>() & 1) == 1)) {
                    string cellName = esp[i]["id"].Value<string>();
                    //var cellX = esp[i]["data"]["grid"][0].Value<int>();
                    //var cellY = esp[i]["data"]["grid"][1].Value<int>();
                    //string region = esp[i]["region"] is null ? "Wilderness" : esp[i]["region"].Value<string>(); 

                    //if (!isInterior) Console.WriteLine($"{region} {cellName} {cellX} {cellY}");
                    JArray refs = (JArray)esp[i]["references"];
                    for (int refNum = 0; refNum < refs.Count; refNum++) {

                        if (refs[refNum]["door_destination_coords"] != null) {

                            if (refs[refNum]["door_destination_cell"] != null) {
                                //Console.WriteLine(cellName + " -> " + refs[refNum]["door_destination_cell"].Value<string>());
                            } else {

                                JArray coords = (JArray)refs[refNum]["door_destination_coords"];
                                float x = coords[0].Value<float>();
                                float y = coords[1].Value<float>();
                                float xMap = (x + xAdd) * cellSize / 8192;
                                float yMap = (yAdd - y) * cellSize / 8192;

                                string type = cellTypes.ContainsKey(cellName) ? cellTypes[cellName] : "unknown";
                                cellName = cellName.Split(',')[0];

                                float xAdj = (x > 0) ? x : x - 8192; //to counteract rounding down
                                float yAdj = (y > 0) ? y : y - 8192; //to counteract rounding down
                                int cellX = (int)(xAdj / 8192);
                                int cellY = (int)(yAdj / 8192);

                                string region = cellRegions[cellX + 128, cellY + 128];
                                if (region is null) region = "NULLREGION";
                                region = region.EndsWith(" Region") ? region.Substring(0, region.Length - 7) : region;

                                if (!intCellRegions.ContainsKey(cellName)) {
                                    intCellRegions[cellName] = region;
                                    Console.WriteLine($"{region}_{type}_{cellName}_{cellX}_{cellY}");
                                } else if (intCellRegions[cellName] != region) Console.WriteLine($"CELL IN MULTIPLE REGIONS {cellName} {intCellRegions[cellName]} {region}");

                                //cells.Add(cellName);
                                //
                                //Console.WriteLine($"{cellName} -> ({xMap},{yMap})");
                            }
                        }
                    }
                }
            }
            Console.WriteLine("\r\n\r\n\r\n");
            //foreach (string cell in cells) Console.WriteLine(cell);

        }

        public static void MWUnknownDoords(string espPath) {
            Dictionary<string, string> cellTypes = new Dictionary<string, string>();
            HashSet<string> cells = new HashSet<string>();
            foreach (string line in File.ReadAllLines(@"F:\Extracted\Morrowind\celltypes.txt")) {
                var split = line.Split('\t');
                cellTypes[split[0]] = split[1];
            }

            JArray esp = JArray.Parse(File.ReadAllText(espPath));
            for (int i = 0; i < esp.Count; i++) {
                if (esp[i]["type"] != null && esp[i]["type"].Value<string>() == "Cell") {
                    string cellName = esp[i]["id"].Value<string>();
                    //Console.WriteLine(cellName);
                    JArray refs = (JArray)esp[i]["references"];
                    for (int refNum = 0; refNum < refs.Count; refNum++) {
                        if (refs[refNum]["door_destination_coords"] != null) {
                            if (refs[refNum]["door_destination_cell"] != null) {
                                //Console.WriteLine(cellName + " -> " + refs[refNum]["door_destination_cell"].Value<string>());
                            } else {
                                cells.Add(cellName);
                                //Console.WriteLine($"{cellName} -> ({xMap},{yMap})");
                            }
                        }
                    }
                }
            }

            foreach (string cell in cells) if (!cellTypes.ContainsKey(cell)) Console.WriteLine(cell);



        }

        struct Float2 {
            public float x;
            public float y;
        }

        public static void DoorsListNew(string espPath) {
            int cellSize = 64;
            int xAdd = 42 * 8192;
            int yAdd = 38 * 8192;

            HashSet<string> cells = new HashSet<string>();

            Dictionary<string, List<Float2>> mergePositions = new Dictionary<string, List<Float2>>();
            Dictionary<string, string> mergeTypes = new Dictionary<string, string>();

            Dictionary<string, string> cellTypes = new Dictionary<string, string>();
            Dictionary<string, string> cellSources = new Dictionary<string, string>();


            foreach (string line in File.ReadAllLines(@"F:\Extracted\Morrowind\celltypes.txt")) {
                var split = line.Split('\t');
                cellTypes[split[0]] = split[1];
                if (split.Length > 2) cellSources[split[0]] = split[2];
            }


            JArray esp = JArray.Parse(File.ReadAllText(espPath));

            Dictionary<int, string> cellRegions = new Dictionary<int, string>();
            for (int i = 0; i < esp.Count; i++) {
                var cell = esp[i];

                if (cell["type"] != null && cell["type"].Value<string>() == "Cell") {
                    bool isInterior = (cell["data"]["flags"].Value<int>() & 1) > 0;
                    if (!isInterior) {
                        int x = cell["data"]["grid"][0].Value<int>();
                        int y = cell["data"]["grid"][1].Value<int>();

                        if (cell["region"] != null) {
                            string region = cell["region"].Value<string>();
                            cellRegions[x + y * 128] = region;
                        }
                    }
                }
            }

            for (int i = 0; i < esp.Count; i++) {
                if (esp[i]["type"] != null && esp[i]["type"].Value<string>() == "Cell") {
                    string cellName = esp[i]["id"].Value<string>();


                    //Console.WriteLine(cellName);
                    JArray refs = (JArray)esp[i]["references"];
                    for (int refNum = 0; refNum < refs.Count; refNum++) {
                        if (refs[refNum]["door_destination_coords"] != null) {
                            if (refs[refNum]["door_destination_cell"] != null) {
                                //Console.WriteLine(cellName + " -> " + refs[refNum]["door_destination_cell"].Value<string>());
                            } else {
                                if (cells.Contains(cellName)) continue;
                                cells.Add(cellName);

                                JArray coords = (JArray)refs[refNum]["door_destination_coords"];
                                float x = coords[0].Value<float>();
                                float y = coords[1].Value<float>();
                                int cellX = (int)(x / 8192);
                                int cellY = (int)(y / 8192);
                                //float xMap = (x + xAdd) * cellSize / 8192;
                                //float yMap = (yAdd - y) * cellSize / 8192;

                                string type = cellTypes.ContainsKey(cellName) ? cellTypes[cellName] : "unknown";
                                string source = cellSources.ContainsKey(cellName) ? cellSources[cellName] : "";

                                int _t = cellName.IndexOf(',');
                                if (_t == -1) _t = cellName.IndexOf(':');
                                string mergeName = _t != -1 ? cellName.Substring(0, _t) : "";

                                string region = cellRegions.ContainsKey(cellX + cellY * 128) ? cellRegions[cellX + cellY * 128] : "wilderness";

                                Console.WriteLine($"{cellName}@{type}@{mergeName}@{region}@{source}");

                                //Console.WriteLine($"{cellName} -> ({xMap},{yMap})");
                            }
                        }
                    }
                }
            }
            /*
            foreach (string mergeName in mergePositions.Keys) {
                float x = 0;
                float y = 0;
                var positions = mergePositions[mergeName];

                foreach (Float2 pos in positions) {
                    x += pos.x; y += pos.y;
                }
                x /= positions.Count; y /= positions.Count;
                string type = mergeTypes[mergeName];
                string markerType = type.Contains("_town") || type.Contains("_city") || type.Contains("_fort") ? "mark" : "icon";
                Console.WriteLine($"<div class=\"{markerType} {mergeTypes[mergeName].Substring(0, 3)} {mergeTypes[mergeName]}\" style=\"left:{(int)(x + 0.5)};top:{(int)(y + 0.5)};\" title=\"{mergeName}\"></div>");

            }
			*/
            //Console.WriteLine("\r\n\r\n\r\n");
            //foreach (string cell in cells) Console.WriteLine(cell);

        }

        class CellInfo {
            public string name;
            public string type;
            public string merge;
            public bool settlement;

            public static Dictionary<string, CellInfo> GetCellInfo(params string[] cellTypeFiles) {
                var dict = new Dictionary<string, CellInfo>();
                foreach (string cellTypeFile in cellTypeFiles) {
                    foreach (string line in File.ReadAllLines(cellTypeFile)) {
                        string[] split = line.Split('\t');
                        CellInfo info = new CellInfo() { name = split[0], type = split[1], merge = split[2], settlement = split[4] == "Settlement" };
                        dict[info.name] = info;
                    }
                }
                return dict;
            }
        }

        public static void MapNpcsNew(params string[] espPaths) {
            bool mapInsteadOfList = false;

            int cellSize = 64;
            int xAdd = 42 * 8192;
            int yAdd = 38 * 8192;

            var cellInfo = CellInfo.GetCellInfo(@"E:\Extracted\Morrowind\celltypesGF.txt");
            Dictionary<(int, int), List<(float, float, CellInfo)>> cellDoors = new Dictionary<(int, int), List<(float, float, CellInfo)>>();

            Dictionary<string, string> scriptText = new Dictionary<string, string>();

            Dictionary<string, string> npcs = new Dictionary<string, string>();
            HashSet<string> npcDisabled = new HashSet<string>();
            HashSet<string> npcHostile = new HashSet<string>();

            Dictionary<string, int> npcCounts = new Dictionary<string, int>();
            Dictionary<string, int> guardCounts = new Dictionary<string, int>();

            HashSet<string> guardNames = new HashSet<string>();

            List<JToken> npcRefs = new List<JToken>();
            foreach (string espPath in espPaths) {
                JArray esp = JArray.Parse(File.ReadAllText(espPath));
                for (int i = 0; i < esp.Count; i++) {
                    var form = esp[i];
                    string formType = form["type"].Str();
                    if (formType == "Script") {
                        scriptText[form.Str("id")] = form.Str("text");
                    } else if (formType == "Npc") {
                        string formId = form["id"].Str();
                        if (form["data"]["stats"] != null && form["data"]["stats"]["health"].Int() == 0) continue;
                        string npcName = form["name"].Str();
                        if (npcName.StartsWith("Guard") || npcName.EndsWith("Guard") || npcName.EndsWith("Sharpshooter") || npcName.Contains("Ordinator")) guardNames.Add(npcName);
                        npcs[formId] = npcName;
                        if (form["ai_data"]["fight"].Value<int>() > 70) npcHostile.Add(formId);
                        if (form["script"] != null) {
                            string scriptName = form.Str("script");
                            if (!scriptText.ContainsKey(scriptName)) continue;
                            string script = scriptText[scriptName];
                            script = script.Replace("\r\n", " ");
                            script = script.Replace("\t", " ");
                            foreach (string token in script.Split(new char[] { ' ', ',' }, StringSplitOptions.RemoveEmptyEntries)) {
                                if (token == "disable" || token == "Disable") {
                                    npcDisabled.Add(formId);
                                    break;
                                }
                            }
                        }
                    } else if (formType == "Cell") {
                        if (form["data"]["flags"].Value<string>().IndexOf("IS_INTERIOR") != -1) {
                            if (!mapInsteadOfList) {
                                string cellName = form.Str("name");
                                if (!cellInfo.ContainsKey(cellName)) continue;
                                if (!cellInfo[cellName].settlement) continue;
                                JArray refs = (JArray)form["references"];
                                for (int refNum = 0; refNum < refs.Count; refNum++) {
                                    string refId = refs[refNum].Str("id");
                                    if (npcs.ContainsKey(refId)) {
                                        string npcName = npcs[refId];
                                        string mergeName = cellInfo[cellName].merge;
                                        if (guardNames.Contains(npcName)) {
                                            if (!guardCounts.ContainsKey(mergeName)) guardCounts[mergeName] = 0;
                                            guardCounts[mergeName]++;
                                        } else {
                                            if (!npcCounts.ContainsKey(mergeName)) npcCounts[mergeName] = 0;
                                            npcCounts[mergeName]++;
                                        }
                                        Console.WriteLine($"{cellInfo[cellName].merge}|{cellName}|{npcs[refId]}");
                                    }
                                }
                            }
                        } else {
                            //Console.WriteLine(cellName);
                            JArray refs = (JArray)form["references"];
                            for (int refNum = 0; refNum < refs.Count; refNum++) {
                                var reference = refs[refNum];

                                if (reference["destination"] != null) {
                                    float x = reference["translation"][0].Value<float>();
                                    float y = reference["translation"][1].Value<float>();
                                    int cellX = (int)(x / 8192);
                                    int cellY = (int)(y / 8192);

                                    string destinationCell = reference["destination"].Str("cell");
                                    if (cellInfo.ContainsKey(destinationCell)) {
                                        if (!cellDoors.ContainsKey((cellX, cellY))) cellDoors[(cellX, cellY)] = new List<(float, float, CellInfo)>();
                                        cellDoors[(cellX, cellY)].Add((x, y, cellInfo[destinationCell]));
                                    } else {
                                        Console.WriteLine(destinationCell + " MISSING CELL INFO");
                                    }
                                } else if (npcs.ContainsKey(reference.Str("id"))) {
                                    npcRefs.Add(reference);
                                }
                            }
                        }
                    }
                }
            }


            float defaultSearchDist = 3250;
            Dictionary<string, float> settlementRadiusOverrides = new Dictionary<string, float>() {
                { "Caldera", 2000 },
                { "Vivec", 6000 },
                { "Molag Mar", 6000 },
                { "Tel Aruhn",  5000 },

                { "Fort Ancylis",  5000 },
                { "Bal Foyen",  2000 },
                { "Dondril", 6000 },
                { "Ald Iuval", 3600 },
                { "Narsis", 4500 },
                { "Necrom", 10000 },
                { "Akamora", 4000 },
                { "Alt Bosara", 4000 },
                { "Marog", 4000 },
                { "Tel Mothrivra", 4000 },
                { "Port Telvannis", 11000 },
                { "Oran Plantation", 6400 }
            };


            foreach (var npc in npcRefs) {
                string id = npc.Str("id");
                float x = npc["translation"][0].Value<float>();
                float y = npc["translation"][1].Value<float>();
                int cellX = (int)(x / 8192);
                int cellY = (int)(y / 8192);
                string closestCell = "";
                float findDist = float.MaxValue;
                string closestSettlement = "";

                for (int searchY = cellY - 1; searchY <= cellY + 1; searchY++) {
                    for (int searchX = cellX - 1; searchX <= cellX + 1; searchX++) {
                        if (cellDoors.ContainsKey((searchX, searchY))) {
                            foreach (var door in cellDoors[(searchX, searchY)]) {
                                if (!door.Item3.settlement) continue;
                                float xDist = door.Item1 - x; xDist = xDist * xDist;
                                float yDist = door.Item2 - y; yDist = yDist * yDist;
                                float dist = xDist + yDist;
                                if (dist < findDist) {
                                    findDist = dist;
                                    closestCell = mapInsteadOfList ? $"|{door.Item3.name} {(int)(Math.Sqrt(findDist))}" : door.Item3.name;
                                    closestSettlement = door.Item3.merge;
                                }

                            }
                        }
                    }
                }
                float searchDist = settlementRadiusOverrides.ContainsKey(closestSettlement) ? settlementRadiusOverrides[closestSettlement] : defaultSearchDist;
                searchDist = searchDist * searchDist;

                float xMap = (x + xAdd) * cellSize / 8192;
                float yMap = (yAdd - y) * cellSize / 8192;
                string extraClass = searchDist > findDist ? " settlement" : npcDisabled.Contains(id) ? " disable" : npcHostile.Contains(id) ? " hostile" : "";
                if (mapInsteadOfList) {
                    Console.Write($"<div class=\"npc{extraClass}\" style=\"left:{(int)(xMap + 0.5)};top:{(int)(yMap + 0.5)};\" title=\"{npcs[id]}{closestCell}\"></div>"); Console.WriteLine();
                } else {
                    if (extraClass == " settlement") {
                        string npcName = npcs[id];
                        if (guardNames.Contains(npcName)) {
                            if (!guardCounts.ContainsKey(closestSettlement)) guardCounts[closestSettlement] = 0;
                            guardCounts[closestSettlement]++;
                        } else {
                            if (!npcCounts.ContainsKey(closestSettlement)) npcCounts[closestSettlement] = 0;
                            npcCounts[closestSettlement]++;
                        }
                        Console.WriteLine($"{closestSettlement}|Near {closestCell}|{npcName}");
                    }
                }
                //Console.WriteLine($"{npcs[id]} | {closestCell}");
            }

            Console.WriteLine();
            foreach (string settlement in npcCounts.Keys) {
                int guardCount = guardCounts.ContainsKey(settlement) ? guardCounts[settlement] : 0;
                Console.WriteLine($"{settlement}|{npcCounts[settlement]}|{guardCount}");
            }

        }


        public static void MapNpcs(string espPath) {
            int cellSize = 64;
            int xAdd = 42 * 8192;
            int yAdd = 38 * 8192;

            Dictionary<string, string> scriptText = new Dictionary<string, string>();

            Dictionary<string, string> npcs = new Dictionary<string, string>();
            HashSet<string> npcDisabled = new HashSet<string>();
            HashSet<string> npcHostile = new HashSet<string>();

            JArray esp = JArray.Parse(File.ReadAllText(espPath));
            for (int i = 0; i < esp.Count; i++) {
                var form = esp[i];
                string formType = form["type"].Str();
                if (formType == "Script") {
                    scriptText[form.Str("id")] = form.Str("text");
                } else if (formType == "Npc") {
                    string formId = form["id"].Str();
                    string npcName = form["name"].Str();
                    if (npcName == "Mendyn Hereloth") Console.WriteLine(npcName);
                    npcs[formId] = npcName;
                    if (form["ai_data"]["fight"].Value<int>() > 70) npcHostile.Add(formId);
                    if (form["script"] != null) {
                        string scriptName = form.Str("script");
                        if (!scriptText.ContainsKey(scriptName)) continue;
                        string script = scriptText[scriptName];
                        script = script.Replace("\r\n", " ");
                        script = script.Replace("\t", " ");
                        foreach (string token in script.Split(new char[] { ' ', ',' }, StringSplitOptions.RemoveEmptyEntries)) {
                            if (token == "disable" || token == "Disable") {
                                npcDisabled.Add(formId);
                                break;
                            }
                        }
                    }
                } else if (formType == "Cell") {
                    if (form["data"]["flags"].Value<string>().IndexOf("IS_INTERIOR") != -1) continue;

                    //Console.WriteLine(cellName);
                    JArray refs = (JArray)form["references"];
                    for (int refNum = 0; refNum < refs.Count; refNum++) {
                        var reference = refs[refNum];
                        string refId = reference.Str("id");
                        if (!npcs.ContainsKey(refId)) continue;

                        float x = reference["translation"][0].Value<float>();
                        float y = reference["translation"][1].Value<float>();
                        float xMap = (x + xAdd) * cellSize / 8192;
                        float yMap = (yAdd - y) * cellSize / 8192;
                        string extraClass = npcDisabled.Contains(refId) ? " disable" : npcHostile.Contains(refId) ? " hostile" : "";
                        Console.Write($"<div class=\"npc{extraClass}\" style=\"left:{(int)(xMap + 0.5)};top:{(int)(yMap + 0.5)};\" title=\"{npcs[refId]}\"></div>"); Console.WriteLine();

                    }
                }
            }
        }

        public static void DoorsMerged(string espPath, bool merge = true) {
            int cellSize = 64;
            int xAdd = 42 * 8192;
            int yAdd = 38 * 8192;



            Dictionary<string, List<Float2>> mergePositions = new Dictionary<string, List<Float2>>();

            Dictionary<string, string> cellTypes = new Dictionary<string, string>();
            Dictionary<string, string> cellRegions = new Dictionary<string, string>();

            Dictionary<string, string> mergeTypes = new Dictionary<string, string>();
            Dictionary<string, string> mergeRegions = new Dictionary<string, string>();

            Dictionary<string, string> mergeNames = new Dictionary<string, string>();

            foreach (string line in File.ReadAllLines(@"F:\Extracted\Morrowind\celltypesgf.txt")) {
                var split = line.Split('\t');
                string name = split[0];
                cellTypes[name] = split[1];
                if (split[2] != "") mergeNames[name] = split[2];
                cellRegions[name] = split[3];
            }


            JArray esp = JArray.Parse(File.ReadAllText(espPath));
            for (int i = 0; i < esp.Count; i++) {
                var cell = esp[i];
                if (cell["type"].Value<string>() != "Cell") continue;
                if (cell["data"]["flags"].Value<string>().IndexOf("IS_INTERIOR") == -1) continue;

                string cellName = cell["name"].Value<string>();

                //Console.WriteLine(cellName);
                JArray refs = (JArray)cell["references"];
                for (int refNum = 0; refNum < refs.Count; refNum++) {
                    var obj = refs[refNum];
                    if (obj["destination"] != null) {
                        if (obj["destination"]["cell"].Value<string>() == "") {
                            //Console.WriteLine(cellName + " -> " + refs[refNum]["door_destination_cell"].Value<string>());
                            JArray coords = (JArray)refs[refNum]["destination"]["translation"];
                            float x = coords[0].Value<float>();
                            float y = coords[1].Value<float>();
                            float xMap = (x + xAdd) * cellSize / 8192;
                            float yMap = (yAdd - y) * cellSize / 8192;

                            string type = cellTypes.ContainsKey(cellName) ? cellTypes[cellName] : "unknown";
                            string region = cellRegions.ContainsKey(cellName) ? cellRegions[cellName] : "unknown";

                            if (mergeNames.ContainsKey(cellName) && merge) {
                                string mergeName = mergeNames[cellName];
                                if (!mergePositions.ContainsKey(mergeName)) {
                                    mergePositions[mergeName] = new List<Float2>();
                                    mergeTypes[mergeName] = type;
                                    mergeRegions[mergeName] = region;
                                }
                                mergePositions[mergeName].Add(new Float2 { x = xMap, y = yMap });
                            } else {
                                Console.Write($"<div class=\"icon {type.Substring(0, 3)} {type} {region}\" style=\"left:{(int)(xMap + 0.5)};top:{(int)(yMap + 0.5)};\" title=\"{cellName}\"></div>");
                                Console.WriteLine();
                            }
                        } else {
                            //Console.WriteLine($"{cellName} -> ({xMap},{yMap})");
                        }
                    }

                }
            }

            foreach (string mergeName in mergePositions.Keys) {
                float x = 0;
                float y = 0;
                var positions = mergePositions[mergeName];

                foreach (Float2 pos in positions) {
                    x += pos.x; y += pos.y;
                }
                x /= positions.Count; y /= positions.Count;
                string type = mergeTypes[mergeName];
                string region = mergeRegions[mergeName];
                string markerType = type.Contains("_town") || type.Contains("_city") || type.Contains("_fort") ? "mark" : "icon";
                Console.WriteLine($"<div class=\"{markerType} {mergeTypes[mergeName].Substring(0, 3)} {mergeTypes[mergeName]} {region}\" style=\"left:{(int)(x + 0.5)};top:{(int)(y + 0.5)};\" title=\"{mergeName}\"></div>");

            }
            //Console.WriteLine("\r\n\r\n\r\n");
            //foreach (string cell in cells) Console.WriteLine(cell);

        }

        public static void MWListUnknownUnusedDoorCells(string espPath) {
            int cellSize = 64;
            int xAdd = 42 * 8192;
            int yAdd = 38 * 8192;

            Dictionary<string, string> cellTypes = new Dictionary<string, string>();

            foreach (string line in File.ReadAllLines(@"F:\Extracted\Morrowind\celltypesGF.txt")) {
                var split = line.Split('\t');
                cellTypes[split[0]] = split[1];
            }

            HashSet<string> cells = new HashSet<string>(cellTypes.Keys);


            JArray esp = JArray.Parse(File.ReadAllText(espPath));
            for (int i = 0; i < esp.Count; i++) {
                var form = esp[i];
                if (form["type"].Value<string>() != "Cell") continue;

                string cellName = form["name"].Value<string>();
                //Console.WriteLine(cellName);
                JArray refs = (JArray)form["references"];
                for (int refNum = 0; refNum < refs.Count; refNum++) {
                    if (refs[refNum]["destination"] == null) continue;
                    if (refs[refNum]["destination"]["cell"].Value<string>() == "") {
                        if (!cellTypes.ContainsKey(cellName)) Console.WriteLine(cellName);
                        cells.Remove(cellName);
                        //Console.WriteLine(cellName + " -> " + refs[refNum]["door_destination_cell"].Value<string>());
                    } else {
                    }
                }
            }

            Console.WriteLine("\r\n\r\n\r\n");
            foreach (string cell in cells) Console.WriteLine(cell);

        }


        public static void MWDoors(string espPath) {
            int cellSize = 64;
            int xAdd = 42 * 8192;
            int yAdd = 38 * 8192;

            HashSet<string> cells = new HashSet<string>();
            Dictionary<string, string> cellTypes = new Dictionary<string, string>();
            foreach (string line in File.ReadAllLines(@"F:\Extracted\Morrowind\celltypes2.txt")) {
                var split = line.Split('\t');
                cellTypes[split[0]] = split[1];
            }


            JArray esp = JArray.Parse(File.ReadAllText(espPath));
            for (int i = 0; i < esp.Count; i++) {
                var cell = esp[i];
                if (cell["type"] != null && cell["type"].Value<string>() == "Cell") {

                    bool isInterior = (cell["data"]["flags"].Value<int>() & 1) > 0;
                    if (!isInterior) continue;

                    string cellName = cell["id"].Value<string>();
                    //Console.WriteLine(cellName);
                    JArray refs = (JArray)cell["references"];
                    for (int refNum = 0; refNum < refs.Count; refNum++) {
                        if (refs[refNum]["door_destination_coords"] != null) {
                            if (refs[refNum]["door_destination_cell"] != null) {
                                //Console.WriteLine(cellName + " -> " + refs[refNum]["door_destination_cell"].Value<string>());
                            } else {
                                JArray coords = (JArray)refs[refNum]["door_destination_coords"];
                                float x = coords[0].Value<float>();
                                float y = coords[1].Value<float>();
                                float xMap = (x + xAdd) * cellSize / 8192;
                                float yMap = (yAdd - y) * cellSize / 8192;

                                string type = cellTypes.ContainsKey(cellName) ? cellTypes[cellName] : "unknown";

                                int cellX = (int)(x / 8192);
                                int cellY = (int)(y / 8192);
                                cells.Add(cellName);
                                Console.WriteLine($"<div class=\"icon {type.Substring(0, 3)} {type}\" style=\"left:{(int)(xMap + 0.5)};top:{(int)(yMap + 0.5)};\" title=\"{cellName}\"></div>");
                                //Console.WriteLine($"{cellName} -> ({xMap},{yMap})");
                            }
                        }
                    }
                }
            }
            //Console.WriteLine("\r\n\r\n\r\n");
            //foreach (string cell in cells) Console.WriteLine(cell);

        }

        public static void MWBooks(string espPath) {
            JArray esp = JArray.Parse(File.ReadAllText(espPath));
            for (int i = 0; i < esp.Count; i++) {
                if (esp[i]["type"] != null && esp[i]["type"].Value<string>() == "Book") {
                    string id = esp[i]["id"].Value<string>();
                    string name = esp[i]["name"].Value<string>();
                    string icon = esp[i]["icon"].Value<string>();
                    Console.WriteLine($"{name}|{id}|{icon}");
                    //if (id == "") id = "Wilderness";
                    //string region = esp[i]["region"] != null ? esp[i]["region"].Value<string>() : "no_region";
                    //int x = esp[i]["data"]["grid"][0].Value<int>();
                    //int y = esp[i]["data"]["grid"][1].Value<int>();
                }
            }

        }

        static Dictionary<string, string> MWIDMeshLookup(params JArray[] esps) {
            string[] meshTypesArray = new string[] { "Static", "Door", "MiscItem", "Weapon", "Container", "Light", "Armor", "Clothing", "Activator", "Ingredient", "Book", "Alchemy" };
            HashSet<string> meshTypes = new HashSet<string>(); foreach (string type in meshTypesArray) meshTypes.Add(type);

            Dictionary<string, string> lookup = new Dictionary<string, string>();
            foreach (JArray esp in esps) {
                for (int i = 0; i < esp.Count; i++) {
                    string type = esp[i]["type"] != null ? esp[i]["type"].Value<string>() : "";
                    if (meshTypes.Contains(type)) {
                        string mesh = esp[i]["mesh"] != null ? esp[i]["mesh"].Value<string>() : "";
                        if (mesh != "") lookup[esp[i]["id"].Value<string>()] = mesh.ToLower();
                    }
                }
            }
            return lookup;
        }

        static void ListMWRefs(string outFileName, params string[] espPaths) {
            Dictionary<string, int> refCounts = new Dictionary<string, int>();

            JArray[] esps = new JArray[espPaths.Length];
            for (int i = 0; i < esps.Length; i++) {
                Console.WriteLine(Path.GetFileNameWithoutExtension(espPaths[i]));
                esps[i] = JArray.Parse(File.ReadAllText(espPaths[i]));
            }

            Dictionary<string, string> meshLookup = MWIDMeshLookup(esps);
            Console.WriteLine("Gathered meshes");

            //gather refs
            for (int espIndex = 0; espIndex < esps.Length; espIndex++) {
                for (int i = 0; i < esps[espIndex].Count; i++) {
                    if (esps[espIndex][i]["type"] != null && esps[espIndex][i]["type"].Value<string>() == "Cell") {
                        //string id = esps[espIndex][i]["id"].Value<string>();
                        //if (id == "") id = "Wilderness";
                        //string region = esps[espIndex][i]["region"] != null ? esps[espIndex][i]["region"].Value<string>() : "no_region";
                        //int x = esps[espIndex][i]["data"]["grid"][0].Value<int>();
                        //int y = esps[espIndex][i]["data"]["grid"][1].Value<int>();
                        bool isInterior = (esps[espIndex][i]["data"]["flags"].Value<int>() & 1) > 0;
                        if (!isInterior) {
                            //Console.WriteLine($"{id}, {region}, {x},{y}");
                            for (int refIndex = 0; refIndex < esps[espIndex][i]["references"].Count(); refIndex++) {
                                string refId = esps[espIndex][i]["references"][refIndex]["id"].Value<string>();
                                if (!refCounts.ContainsKey(refId)) refCounts[refId] = 1;
                                else refCounts[refId] = refCounts[refId] + 1;
                            }
                        }
                    }
                }
            }

            TextWriter writer = new StreamWriter(File.Open(outFileName, FileMode.Create));
            Dictionary<string, int> meshCounts = new Dictionary<string, int>();
            foreach (string id in refCounts.Keys) {
                if (meshLookup.ContainsKey(id)) {
                    string mesh = meshLookup[id].ToLower();
                    if (!meshCounts.ContainsKey(mesh)) meshCounts[mesh] = refCounts[id];
                    else meshCounts[mesh] = meshCounts[mesh] + refCounts[id];
                    //writer.WriteLine($"{id}|{meshLookup[id]}|{refCounts[id]}");
                }
                //else Console.WriteLine($"{id}: {refCounts[id]}");
            }
            foreach (string mesh in meshCounts.Keys) {
                writer.WriteLine($"{mesh}|{meshCounts[mesh]}");
            }
            writer.Flush();
            writer.Close();

        }

        static void MWIDMeshLookup(params string[] espPaths) {

            string[] meshTypesArray = new string[] { "Static", "Door", "MiscItem", "Weapon", "Container", "Light", "Armor", "Clothing", "Activator", "Ingredient", "Book", "Alchemy" };
            HashSet<string> meshTypes = new HashSet<string>(); foreach (string type in meshTypesArray) meshTypes.Add(type);

            TextWriter w = new StreamWriter(File.Open("idlookup.txt", FileMode.Create));

            foreach (string espPath in espPaths) {

                Console.Write(Path.GetFileNameWithoutExtension(espPath));
                JArray esp = JArray.Parse(File.ReadAllText(espPath));
                Console.WriteLine(" parsed");

                for (int i = 0; i < esp.Count; i++) {
                    string type = esp[i]["type"] != null ? esp[i]["type"].Value<string>() : "";
                    if (meshTypes.Contains(type)) {
                        string mesh = esp[i]["mesh"] != null ? esp[i]["mesh"].Value<string>() : "";
                        if (mesh != "") w.WriteLine(esp[i]["id"].Value<string>() + "|" + mesh.ToLower());
                    }
                }
            }
            w.Flush();
            w.Close();
        }



        static void ListMWRefs2(int heatmapMax, params string[] espPaths) {


            Dictionary<string, (int, int)> meshes = new Dictionary<string, (int, int)>();
            foreach (string line in File.ReadAllLines(@"E:\Anna\Desktop\io_scene_mw\lib\meshes.txt")) {
                string[] words = line.Split('|');
                meshes[words[0]] = (int.Parse(words[1]), int.Parse(words[2]));
            }

            Dictionary<string, string> idLookup = new Dictionary<string, string>();
            foreach (string line in File.ReadAllLines(@"F:\Anna\Visual Studio\SmallScripts\SmallScripts\bin\x64\Debug\idlookup.txt")) {
                string[] words = line.Split('|');
                idLookup[words[0]] = words[1];
            }

            int imageSize = 128;
            byte[] imageData = new byte[imageSize * imageSize];

            foreach (string espPath in espPaths) {

                Console.Write(Path.GetFileNameWithoutExtension(espPath));
                JArray esp = JArray.Parse(File.ReadAllText(espPath));
                Console.WriteLine(" parsed");

                for (int i = 0; i < esp.Count; i++) {
                    if (esp[i]["type"] != null && esp[i]["type"].Value<string>() == "Cell") {

                        string id = esp[i]["id"].Value<string>();
                        if (id == "") id = "Wilderness";

                        string region = esp[i]["region"] != null ? esp[i]["region"].Value<string>() : "Null Region";

                        int x = esp[i]["data"]["grid"][0].Value<int>();
                        int y = esp[i]["data"]["grid"][1].Value<int>();

                        bool isInterior = (esp[i]["data"]["flags"].Value<int>() & 1) > 0;
                        if (isInterior) continue;

                        int meshesCount = 0;
                        int triShapeCount = 0;

                        for (int refIndex = 0; refIndex < esp[i]["references"].Count(); refIndex++) {
                            string refId = esp[i]["references"][refIndex]["id"].Value<string>();
                            if (!idLookup.ContainsKey(refId) || !meshes.ContainsKey(idLookup[refId])) continue;
                            meshesCount++;
                            triShapeCount += meshes[idLookup[refId]].Item1;
                        }

                        byte col = (byte)(Math.Min(triShapeCount * 256 / heatmapMax, 254) + 1);
                        int pixel = (x + imageSize / 2) % imageSize + (y * -1 + imageSize / 2) * imageSize;
                        if (pixel >= 0 && pixel < imageData.Length && col > imageData[pixel]) {
                            imageData[pixel] = col;

                            Console.WriteLine($"{x},{y}|{id}|{region}|{meshesCount}|{triShapeCount}");
                        }
                    }
                }
            }

            MagickReadSettings settings = new MagickReadSettings() { Format = MagickFormat.Gray, Width = imageSize, Height = imageSize };
            MagickImage image = new MagickImage(imageData, settings);

            image.Write($"heatmap_{heatmapMax}.png");
        }


        public static void MWDoors(string espPath, float cellStartX, float cellStartY, float cellSizeX, float cellSizeY = 0) {
            if (cellSizeY == 0) cellSizeY = cellSizeX;
            float unitSizeX = cellSizeX * 8192;
            float unitSizeY = cellSizeY * 8192;
            float minX = cellStartX * 8192;
            float minY = cellStartY * 8192;
            float maxX = minX + unitSizeX;
            float maxY = minY + unitSizeY;

            JArray esp = JArray.Parse(File.ReadAllText(espPath));
            for (int i = 0; i < esp.Count; i++) {
                var form = esp[i];
                if (form["type"].Value<string>() != "Cell") continue;
                if (form["data"]["flags"].Value<string>().IndexOf("IS_INTERIOR") != -1) continue; //interior
                JArray refs = (JArray)form["references"];
                for (int refNum = 0; refNum < refs.Count; refNum++) {
                    if (refs[refNum]["destination"] == null) continue;
                    JArray coords = (JArray)refs[refNum]["translation"];
                    float x = coords[0].Value<float>(); if (x < minX || x > maxX) continue;
                    float y = coords[1].Value<float>(); if (y < minY || y > maxY) continue;
                    string cellName = refs[refNum]["destination"]["cell"].Value<string>();
                    int xPos = (int)((x - minX) * 1000 / unitSizeX);
                    int yPos = 1000 - (int)((y - minY) * 1000 / unitSizeY);

                    Console.WriteLine($"{{{{Image Mark|{xPos}|{yPos}|{cellName}|{cellName}|position=on}}}}");
                }

            }
            Console.WriteLine("\r\n\r\n\r\n");
        }

        public static void MapFlora(params string[] espPaths) {
            int cellSize = 64;
            int xAdd = 42 * 8192;
            int yAdd = 38 * 8192;

            float minX = float.MaxValue;
            float minY = float.MaxValue;
            float maxX = float.MinValue;
            float maxY = float.MinValue;

            Dictionary<string, string> floraIds = new Dictionary<string, string>();
            Dictionary<string, List<Vector2>> floraPositions = new Dictionary<string, List<Vector2>>();
            Dictionary<string, Dictionary<string, int>> floraRegionCounts = new Dictionary<string, Dictionary<string, int>>();
            Dictionary<string, int> floraEspIds = new Dictionary<string, int>();
            Dictionary<string, int> regionCellCounts = new Dictionary<string, int>();
            Dictionary<string, int> floraTotals = new Dictionary<string, int>();

            for (int espIndex = 0; espIndex < espPaths.Length; espIndex++) {
                Console.WriteLine(espPaths[espIndex]);
                JArray esp = JArray.Parse(File.ReadAllText(espPaths[espIndex]));
                for (int i = 0; i < esp.Count; i++) {
                    var form = esp[i];
                    string formType = form["type"].Str();
                    if (formType == "Container") {
                        string id = form.Str("id");
                        if (id.Contains("Flora") || id.Contains("flora")) {
                            string floraType = form.Str("name");
                            //int letter = id.Length - 1;
                            //while (letter >= 0 && (char.IsDigit(id[letter]) || id[letter] == '_')) {
                            //	letter--;
                            //}
                            //string floraType = id.Substring(0, letter + 1);
                            //Console.WriteLine($"{id} - {floraType} - {form.Str("name")}");
                            floraIds[id] = floraType;
                            if (!floraEspIds.ContainsKey(floraType)) {
                                floraTotals[floraType] = 0;
                                floraEspIds[floraType] = espIndex;
                            }
                        }
                    } else if (formType == "Cell") {
                        if (form["data"]["flags"].Value<string>().IndexOf("IS_INTERIOR") != -1) {

                        } else {

                            string region = form["region"] == null ? "NO REGION" : form.Str("region");
                            if (!regionCellCounts.ContainsKey(region)) regionCellCounts[region] = 0;
                            regionCellCounts[region]++;
                            //Console.WriteLine(cellName);
                            JArray refs = (JArray)form["references"];
                            for (int refNum = 0; refNum < refs.Count; refNum++) {
                                var reference = refs[refNum];
                                string refId = reference.Id();
                                if (floraIds.ContainsKey(refId)) {
                                    string floraType = floraIds[refId];
                                    if (!floraRegionCounts.ContainsKey(floraType)) floraRegionCounts[floraType] = new Dictionary<string, int>();
                                    var regionCounts = floraRegionCounts[floraType];
                                    if (!regionCounts.ContainsKey(region)) regionCounts[region] = 0;
                                    regionCounts[region]++;
                                    floraTotals[floraType]++;

                                    if (!floraPositions.ContainsKey(floraType)) floraPositions[floraType] = new List<Vector2>();
                                    float x = reference["translation"][0].Value<float>();
                                    float y = reference["translation"][1].Value<float>();
                                    if (x < minX) minX = x;
                                    if (x > maxX) maxX = x;
                                    if (y < minY) minY = y;
                                    if (y > maxY) maxY = y;
                                    floraPositions[floraType].Add(new Vector2 { x = x, y = y });


                                    //float xMap = (x + xAdd) * cellSize / 8192;
                                    //float yMap = (yAdd - y) * cellSize / 8192;
                                    //Console.WriteLine($"<div class=\"flora {floraType}\" style=\"left:{(int)(xMap + 0.5)};top:{(int)(yMap + 0.5)};\" title=\"{refId}\"></div>");
                                }
                            }
                        }
                    }
                }
            }

            //foreach (string floraType in floraRegionCounts.Keys) {
            //	if (floraTotals[floraType] < 2) continue;
            //	var regionCounts = floraRegionCounts[floraType];
            //             foreach (string region in regionCounts.Keys) {
            //		int count = regionCounts[region];
            //		Console.WriteLine($"{floraEspIds[floraType]}|{floraType}|{region}|{count}|{((float)count)/regionCellCounts[region]}");
            //             }
            //         }



            //return;
            int cellMinX = (int)(minX / 8192) - 1;
            int cellMinY = (int)(minY / 8192) - 1;
            int cellMaxX = (int)(maxX / 8192) + 1;
            int cellMaxY = (int)(maxY / 8192) + 1;
            int cellsX = cellMaxX - cellMinX;
            int cellsY = cellMaxY - cellMinY;

            Console.WriteLine($"({cellMinX},{cellMinY}) to ({cellMaxX},{cellMaxY})");
            //MagickImageCollection images = new MagickImageCollection();

            foreach (string floraType in floraPositions.Keys) {
                Console.WriteLine(floraType);
                byte[] data = new byte[cellsX * cellSize * cellsY * cellSize * 4];

                foreach (Vector2 pos in floraPositions[floraType]) {
                    int xMap = (int)(pos.x * cellSize / 8192) - (cellMinX * cellSize);
                    int yMap = (int)(pos.y * cellSize / 8192) - (cellMinY * cellSize);
                    int offset = (xMap + yMap * cellSize * cellsX) * 4;
                    byte newval = (byte)Math.Min(data[offset] + 64, 255);
                    data[offset] = newval;
                    data[offset + 1] = newval;
                    data[offset + 2] = newval;
                    data[offset + 3] = 255;
                }

                MagickImage image = new MagickImage(data, new MagickReadSettings { Width = cellsX * cellSize, Height = cellsY * cellSize, Depth = 8, Format = MagickFormat.Rgba });
                image.Flip();
                image.Blur(8, 2);
                image.Level(0, 8192);
                //images.Add(image);
                WebPWriteDefines defines = new WebPWriteDefines() { Lossless = true };
                image.Write(@"E:\Extracted\Morrowind\floramaps\" + floraType + ".webp", defines);

            }
            //images.Write(@"E:\Extracted\Morrowind\floramaps\floramaps.psd");
        }



        public static void CellListAll(string cellTypesFilePath, params string[] espPaths) {
            Dictionary<string, string> cellTypes = new Dictionary<string, string>();
            Dictionary<string, string> cellGroups = new Dictionary<string, string>();
            Dictionary<string, string> cellRegions = new Dictionary<string, string>();
            foreach (string line in File.ReadAllLines(cellTypesFilePath)) {
                var split = line.Split('\t');
                cellTypes[split[0]] = split[1];
                cellGroups[split[0]] = split[2];
                cellRegions[split[0]] = split[3];
            }
            HashSet<string> noDupes = new HashSet<string>();
            foreach (string espPath in espPaths) {
                JArray esp = JArray.Parse(File.ReadAllText(espPath));
                for (int i = 0; i < esp.Count; i++) {
                    var form = esp[i];
                    string formType = form["type"].Str();
                    if (formType == "Cell") {
                        if (form["data"]["flags"].Value<string>().IndexOf("IS_INTERIOR") == -1) continue;
                        string name = form.Str("name");
                        if (noDupes.Contains(name)) continue;
                        noDupes.Add(name);
                        string type = "UNKNOWN";
                        string group = "";
                        string region = type;
                        if (cellTypes.ContainsKey(name)) {
                            type = cellTypes[name];
                            group = cellGroups[name];
                            region = cellRegions[name];
                        } else {
                            Console.WriteLine($"{name}@{type}@{group}@{region}");
                        }
                    }
                }
            }
        }

    }
}
