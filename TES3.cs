using ImageMagick;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace SmallScripts {
	static class TES3 {

		public static void TES3IntCellResizeTest() {
			MagickImageCollection coll = new MagickImageCollection();
			foreach (string imagePath in Directory.EnumerateFiles(@"F:\Extracted\Morrowind\tombsa", "*.png")) {
				MagickImage image = new MagickImage(imagePath);
				coll.Add(image);
			}
			var montage = coll.Montage(new MontageSettings());
			montage.Write(@"F:\Extracted\Morrowind\tombstest.png");

			return;
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
			for(int start = 0; start < ints.Count; start += groupSize) {
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
					if(region != "Wilderness") {
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

		public static void TES3LocalMapCombine(int tileSize, int x1, int y1, int x2, int y2) {
			MagickImage montage = new MagickImage(MagickColors.Black, tileSize * 8, tileSize * 8);
			int xCount = x2 - x1; int yCount = y2 - y1;

			for (int y = 0; y < xCount; y++) {
				Console.Write("-");
				for (int x = 0; x < yCount; x++) {
					string search = $@"F:\Anna\Desktop\maps3 - Copy\{x1 + x},{y1 + y}.bmp";
					if (File.Exists(search)) {
						MagickImage image = new MagickImage(search);
						montage.Draw(new Drawables().Composite(x * tileSize, (yCount - y) * tileSize, image));

					}
					//else Console.WriteLine(search);
				}
			}
			montage.Write($@"F:\Anna\Desktop\map.png");

		}

		public static void TES3LocalMapCombine() {

			int tileSize = 1024;

			for(int mapX = -2; mapX < 3; mapX++) {
				for (int mapY = -2; mapY < 4; mapY++) {
					int startY =  mapY * 8; int startX = mapX * 8;
					MagickImage montage = new MagickImage(MagickColors.Black, tileSize * 8, tileSize * 8);

					for (int y = 0; y < 8; y++) {
						Console.Write("-");
						for (int x = 0; x < 8; x++) {
							string search = $@"F:\Anna\Desktop\maps\{startX + x},{startY + y}.bmp";
							if (File.Exists(search)) {
								MagickImage image = new MagickImage(search);
								montage.Draw(new Drawables().Composite(x * tileSize, (7 - y) * tileSize, image));

							}
							//else Console.WriteLine(search);
						}
					}
					Console.WriteLine();

					Console.WriteLine($"{mapX},{mapY}");
					montage.Write($@"F:\Anna\Desktop\maps1\{mapX},{mapY}.png");

                }
            }
        }

		public static void TES3WeatherValues() {
			string[] weatherTypes = new string[] { "Clear", "Cloudy", "Foggy", "Thunderstorm", "Rain", "Overcast", "Ashstorm", "Blight", "Snow", "Blizzard" };
			string[] tods = new string[] { "Sunrise", "Day", "Sunset", "Night" };
			string[] weatherValues = new string[] { "Sky", "Fog", "Ambient", "Sun" };

			foreach(string value in weatherValues) {
				Console.WriteLine($"    vec4 {value}Colors[{weatherTypes.Length * tods.Length}]=vec4[](");
				foreach(string weather in weatherTypes) {
					foreach(string tod in tods) {
						string search = $"Weather_{weather}_{value}_{tod}_Color";
						foreach (string line in File.ReadAllLines(@"F:\Extracted\Morrowind\weathercolors.txt")){
							if (line.StartsWith(search)) {
								var words = line.Split(',');
								float r = ((float)byte.Parse(words[1]))/ 255;
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
			for(int i = 0; i < lines.Length; i++) {
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
			for(int y = 0; y < 32; y++) {
				for(int x = 31; x >= 0; x--) {
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

		public static void MWMapResize(int oldSize, int newSize) {
			foreach(string line in File.ReadAllLines(@"E:\Anna\Anna\Visual Studio\MWMap\index.html")) {
				if (!line.Contains("style=\"left:")) continue;
				var split = line.Split('"');
				var split2 = new string[7];
				split2[0] = split[0];
				split2[1] = split[1];
				split2[2] = split[4];
				split2[3] = split[5];
				split2[4] = split[2];
				split2[5] = split[3];
				split2[6] = split[6];
				//var split2 = split[5].Split(':', ';');
				//float left = float.Parse(split2[1]);
				//float top = float.Parse(split2[3]);
				//split[5] = $"left:{(int)(left + 0.5)};top:{(int)(top + 0.5)}";
				//split[5] = $"left:{left * newSize / oldSize};top:{top * newSize / oldSize}";
				for (int i = 0; i < split2.Length - 1; i++) Console.Write(split2[i] + "\"");
				Console.WriteLine(split2[split2.Length - 1]);
			}
        }


		public static void OpenMWMapCombine(string folder, int tileSize = 256) {

			int minX = int.MaxValue; int maxX = int.MinValue;
			int minY = int.MaxValue; int maxY = int.MinValue;


			foreach (string path in Directory.EnumerateFiles(folder, "*.bmp")) {
				string[] split = Path.GetFileName(path).Split(new char[1] { '.' }, StringSplitOptions.None);
				int x = int.Parse(split[split.Length - 3]); if (x > maxX) maxX = x; if (x < minX) minX = x;
				int y = int.Parse(split[split.Length - 2]); if (y > maxY) maxY = y; if (y < minY) minY = y;
				Console.WriteLine($"{x} {y}");
			}

			Console.WriteLine($"{minX},{minY} to {maxX},{maxY}");



			MagickImage map = new MagickImage(MagickColors.Black, (maxX - minX) * tileSize, (maxY - minY) * tileSize);

			int i = 0;
			foreach (string path in Directory.EnumerateFiles(folder, "*.bmp")) {
				string[] split = Path.GetFileName(path).Split(new char[1] { '.' }, StringSplitOptions.None);
				int x = int.Parse(split[split.Length - 3]);
				int y = int.Parse(split[split.Length - 2]);
				MagickImage image = new MagickImage(path);
				map.Draw(new Drawables().Composite((x - minX) * tileSize, map.Height - (y - minY) * tileSize - tileSize, image));
				i++; if (i % 10 == 0) 
					Console.WriteLine(path);
			}
			map.Write("openmwmap.bmp");
		}


		public static void MWMapCombine() {

			int minX = int.MaxValue; int maxX = int.MinValue;
			int minY = int.MaxValue; int maxY = int.MinValue;


			foreach (string path in Directory.EnumerateFiles(@"E:\Programs\Steam\steamapps\common\Morrowind\Maps", "*.bmp")) {
				string coord = Path.GetFileName(path);
				coord = coord.Substring(coord.IndexOf('(') + 1);
				var split = coord.Split(',', ')');
				int x = int.Parse(split[0]); if (x > maxX) maxX = x; if (x < minX) minX = x;
				int y = int.Parse(split[1]); if (y > maxY) maxY = y; if (y < minY) minY = y;
				Console.WriteLine($"{x} {y}");
            }

			Console.WriteLine($"{minX},{minY} to {maxX},{maxY}");

			

			MagickImage map = new MagickImage(MagickColors.Black, (maxX - minX) * 256, (maxY - minY) * 256);

			int i = 0;
			foreach (string path in Directory.EnumerateFiles(@"E:\Programs\Steam\steamapps\common\Morrowind\Maps", "*.bmp")) {
				string coord = Path.GetFileName(path);
				coord = coord.Substring(coord.IndexOf('(') + 1);
				var split = coord.Split(',', ')');
				int x = int.Parse(split[0]);
				int y = int.Parse(split[1]);
				MagickImage image = new MagickImage(path);
				map.Draw(new Drawables().Composite((x - minX) * 256, map.Height - (y - minY) * 256  -256, image));
				i++; if (i % 10 == 0) Console.WriteLine(path);
			}
			map.Write("morrowind.bmp");
		}

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

		public static void MWDoors(string espPath) {
			int cellSize = 64;
			int xAdd = 42 * 8192;
			int yAdd = 38 * 8192;

			HashSet<string> cells = new HashSet<string>();
			Dictionary<string, string> cellTypes = new Dictionary<string, string>();
			foreach(string line in File.ReadAllLines(@"F:\Extracted\Morrowind\celltypes.txt")) {
				var split = line.Split('\t');
				cellTypes[split[0]] = split[1];
            }
			

			JArray esp = JArray.Parse(File.ReadAllText(espPath));
			for(int i = 0; i < esp.Count; i++) {
				if (esp[i]["type"] != null && esp[i]["type"].Value<string>() == "Cell") {
					string cellName = esp[i]["id"].Value<string>();
					//Console.WriteLine(cellName);
					JArray refs = (JArray)esp[i]["references"];
					for(int refNum = 0; refNum < refs.Count; refNum++) {
						if(refs[refNum]["door_destination_coords"] != null) {
							if(refs[refNum]["door_destination_cell"] != null) {
								//Console.WriteLine(cellName + " -> " + refs[refNum]["door_destination_cell"].Value<string>());
							} else {
								JArray coords = (JArray)refs[refNum]["door_destination_coords"];
								float x = coords[0].Value<float>();
								float y = coords[1].Value<float>();
								float xMap = (x + xAdd) * cellSize / 8192;
								float yMap = (yAdd - y) * cellSize / 8192;

								string type = cellTypes.ContainsKey(cellName) ? cellTypes[cellName] : "unknown";

								int cellX = (int)(x/8192);
								int cellY = (int)(y / 8192);
								cells.Add(cellName);
								Console.WriteLine($"<div class=\"icon {type.Substring(0,3)} {type}\" style=\"left:{(int)(xMap + 0.5)};top:{(int)(yMap + 0.5)};\" title=\"{cellName}\"></div>");
								//Console.WriteLine($"{cellName} -> ({xMap},{yMap})");
							}
                        }
                    }
				}
			}
			Console.WriteLine("\r\n\r\n\r\n");
			//foreach (string cell in cells) Console.WriteLine(cell);

		}
		public static void MWQuests(params string[] espPaths) {

			Dictionary<string, string> questNames = new Dictionary<string, string>();
			Dictionary<string, string> questFiles = new Dictionary<string, string>();

			foreach(string espPath in espPaths) {
				string file = Path.GetFileNameWithoutExtension(espPath);
				Console.WriteLine(espPath);
				JArray esp = JArray.Parse(File.ReadAllText(espPath));
				int i = 0;
				while (i < esp.Count) {
					if (esp[i]["type"] != null && esp[i]["type"].Value<string>() == "Dialogue") {
						string type = esp[i]["dialogue_type"].Value<string>();
						if (type == "Journal") {
							string id = esp[i]["id"].Value<string>();
							if(!questFiles.ContainsKey(id)) questFiles[id] = file;
							if (!questNames.ContainsKey(id) || questNames[id] == "NO QUEST NAMEAAAAAAAAAAAAAAAAAAAAA") {
								string name = "NO QUEST NAMEAAAAAAAAAAAAAAAAAAAAA";
								string zero = null;
								string name2 = null;
								i++;
								while (esp[i]["type"] != null && esp[i]["type"].Value<string>() == "Info") {
									if (esp[i]["data"]["disposition"].Value<int>() == 0) {
										zero = esp[i]["text"].Value<string>();
										if (esp[i]["quest_name"] != null) name = esp[i]["text"].Value<string>();
									} else if (esp[i]["quest_name"] != null) name2 = esp[i]["text"].Value<string>();
									i++;
								}
								if (name == "NO QUEST NAMEAAAAAAAAAAAAAAAAAAAAA" && zero != null) questNames[id] = zero;
								else if (name == "NO QUEST NAMEAAAAAAAAAAAAAAAAAAAAA" && name2 != null) questNames[id] = name2;
								else questNames[id] = name;
								i--;
							}
						}
					}
					i++;
				}
			}
			foreach (string quest in questNames.Keys) Console.WriteLine(questFiles[quest] + "|" + quest + "|" + questNames[quest]);
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


		//ListMWRefs2(2000, @"F:\Extracted\Tools\morrowind.json", @"F:\Extracted\Tools\tribunal.json", @"F:\Extracted\Tools\bloodmoon.json", @"F:\Extracted\Tools\trmainland.json", @"F:\Extracted\Tools\trpreview.json");
		//ListMWRefs("meshes.txt", @"F:\Extracted\Tools\morrowind.json", @"F:\Extracted\Tools\tribunal.json", @"F:\Extracted\Tools\bloodmoon.json");
		//ListMWRefs("meshestr.txt", @"F:\Extracted\Tools\morrowind.json", @"F:\Extracted\Tools\tribunal.json", @"F:\Extracted\Tools\bloodmoon.json", @"F:\Extracted\Tools\tamrieldata.json", @"F:\Extracted\Tools\trmainland.json", @"F:\Extracted\Tools\trpreview.json");
		//MWIDMeshLookup(@"F:\Extracted\Tools\morrowind.json", @"F:\Extracted\Tools\tribunal.json", @"F:\Extracted\Tools\bloodmoon.json", @"F:\Extracted\Tools\tamrieldata.json", @"F:\Extracted\Tools\trmainland.json", @"F:\Extracted\Tools\trpreview.json");

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
	}
}
