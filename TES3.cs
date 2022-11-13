using ImageMagick;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace SmallScripts {
	static class TES3 {
		
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
