using ImageMagick;
//using ImageMagick.Formats.Dds;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using Util;

namespace SmallScripts {
	static class TESO {


		public static void CreateTileMap(string path, int size) {
			MagickImage fullMap = new MagickImage(path);
			var images = fullMap.CropToTiles(fullMap.Width / size, fullMap.Width / size);
			int i = 0;
			//var defines = new DdsWriteDefines() { Compression = DdsCompression.None, Mipmaps = 0 };
			foreach (var image in images) {
				image.Format = MagickFormat.Dds;
				//image.SetCompression(CompressionMethod.NoCompression);
				image.Alpha(AlphaOption.On);
				//image.Settings.SetDefines(defines);
				string fileName = Path.GetFileNameWithoutExtension(path) + "_" + i.ToString() + ".dds";
				Console.WriteLine(fileName);
				image.Write(Path.GetDirectoryName(path) + "\\" + fileName);
				i++;
			}
		}


		static void ESOReadDefs() {
			Dictionary<string, string> defNames = new Dictionary<string, string>();
			foreach (string line in File.ReadAllLines(@"F:\Extracted\ESO\defids.txt")) {
				string[] words = line.Split(' ');
				defNames[words[0]] = words[1];
			}

			foreach (string path in Directory.EnumerateFiles(@"F:\Junk\Backup\BethesdaGameStudioUtils\esoapps\EsoExtractData\x64\Release\badlandsdata3\000\", "*pressed.EsoFileData")) {
				string defIndex = Path.GetFileName(path).Substring(13, 3);
				if (defNames.ContainsKey(defIndex)) defIndex = defNames[defIndex] + "_" + defIndex;
				else defIndex = "UnknownDef_" + defIndex;
				Console.WriteLine(defIndex);

				using (BinaryReader r = new BinaryReader(File.OpenRead(path))) {
					r.Seek(8);
					uint numRecords = r.ReadUInt32B();
					if (numRecords > 1) {
						using (TextWriter writer = new StreamWriter(File.Open($@"F:\Extracted\ESO\defnamedump\{defIndex}.txt", FileMode.Create))) {
							r.Seek(4);
							for (uint i = 0; i < numRecords; i++) {
								r.Seek(28);
								uint dataSize = r.ReadUInt32B();
								long pos = r.BaseStream.Position;
								r.Seek(4);
								ushort nameSize = r.ReadUInt16B();
								if (nameSize > 0 && nameSize + 6 < dataSize && nameSize < 128) {
									string name = new string(r.ReadChars(nameSize));
									//Console.WriteLine(name);
									writer.WriteLine(name);
								}
								r.BaseStream.Seek(pos + dataSize, SeekOrigin.Begin);
							}
							writer.Flush();
						}
					}
				}
			}
		}

		static void ESOReadDefs2() {
			byte[] zeroesForWriting = new byte[128];

			Dictionary<string, string> defNames = new Dictionary<string, string>();
			foreach (string line in File.ReadAllLines(@"F:\Extracted\ESO\defids.txt")) {
				string[] words = line.Split(' ');
				defNames[words[0]] = words[1];
			}

			foreach (string path in Directory.EnumerateFiles(@"F:\Junk\Backup\BethesdaGameStudioUtils\esoapps\EsoExtractData\x64\Release\badlandsdata3\000\", "*pressed.EsoFileData")) {
				string defIndex = Path.GetFileName(path).Substring(13, 3);
				if (defNames.ContainsKey(defIndex)) defIndex = defIndex + "_" + defNames[defIndex];
				else defIndex = defIndex + "_UnknownDef";
				//Console.WriteLine(defIndex);

				using (BinaryReader r = new BinaryReader(File.OpenRead(path))) {
					r.Seek(8);
					uint numRecords = r.ReadUInt32B();
					Console.WriteLine($"{numRecords}\t{defIndex}");
					/*
					if (numRecords > 1) {
						using (BinaryWriter writer = new BinaryWriter(File.Open($@"F:\Extracted\ESO\defs\{defIndex}.dat", FileMode.Create))) {
							writer.Write(numRecords);
							r.Seek(4);
							for (uint i = 0; i < numRecords; i++) {
								r.Seek(28);
								uint dataSize = r.ReadUInt32B();
								long pos = r.BaseStream.Position;
								r.Seek(4);
								ushort nameSize = r.ReadUInt16B();
								if (nameSize > 0 && nameSize + 6 < dataSize && nameSize < 128) {
									writer.Write(r.ReadBytes(nameSize));
									writer.Write(zeroesForWriting, 0, 32 - nameSize);
									r.Seek(1);
									writer.Write(r.ReadBytes((int)dataSize - nameSize - 7));
									//Console.WriteLine(name);
								}
								r.BaseStream.Seek(pos + dataSize, SeekOrigin.Begin);
							}
							writer.Flush();
						}
					}
					*/
				}
			}
		}

		static void ESOReadMapTileNames() {
			using (BinaryReader r = new BinaryReader(File.OpenRead(@"F:\Junk\Backup\BethesdaGameStudioUtils\esoapps\EsoExtractData\x64\Release\badlandsdata3\000\6000000000000044_Uncompressed.EsoFileData"))) {
				r.Seek(8);
				uint numRecords = r.ReadUInt32B();
				if (numRecords > 1) {
					using (TextWriter writer = new StreamWriter(File.Open($@"F:\Extracted\ESO\worldtilemap.txt", FileMode.Create))) {
						r.Seek(4);
						for (uint i = 0; i < numRecords; i++) {
							r.Seek(28);
							uint dataSize = r.ReadUInt32B();
							long pos = r.BaseStream.Position;
							r.Seek(4);
							ushort nameSize = r.ReadUInt16B();
							if (nameSize > 0 && nameSize + 6 < dataSize && nameSize < 128) {
								string name = new string(r.ReadChars(nameSize));
								r.Seek(19);
								uint worldId = r.ReadUInt32B();
								//Console.WriteLine(name);
								writer.WriteLine($"{worldId} {name}");
							}
							r.BaseStream.Seek(pos + dataSize, SeekOrigin.Begin);
						}
						writer.Flush();
					}
				}
			}
		}

		static void ESODefNames() {
			foreach (string path in Directory.EnumerateFiles(@"F:\Anna\Visual Studio\ESOExplorer\out\build\x64-Debug\ESOBrowser\Database\Defs\", "*.dir")) {
				foreach (string line in File.ReadAllLines(path)) {
					if (!line.StartsWith("DEF ")) continue;
					string[] words = line.Split(' ');
					Console.WriteLine(string.Format("{0:X3} {1}", Int32.Parse(words[1]), words[2]));
				}
			}
		}

		static void ESOWorldIDParseTest() {
			foreach (string file in Directory.EnumerateFiles(@"F:\Junk\Backup\BethesdaGameStudioUtils\esoapps\EsoExtractData\x64\Release\badlandsworld\034")) {
				ulong id = UInt64.Parse(Path.GetFileNameWithoutExtension(file), System.Globalization.NumberStyles.HexNumber);
				Console.WriteLine(Path.GetFileNameWithoutExtension(file) + " " + EsoWorldFileDesc(id));
			}
		}

		static string EsoWorldFileDesc(ulong id) {
			//return string.Format("{0:X}", id >> 112);
			if ((id >> 120) == 0x44) return $"{id & 0xffff}.toc";
			if ((id >> 120) == 0x40) return $"{(id >> 37) & 0x7ff}_{(id >> 32) & 0x1f}_{(id >> 16) & 0xffff}_{id & 0xffff}.cell";
			if ((id >> 120) == 0x48) return $"{(id >> 37) & 0x7ff}_{(id >> 32) & 0x1f}.file";

			return "";
		}

		static void ESOMapTexTimeline() {
			//string[] mapfolders = File.ReadAllLines(@"F:\Extracted\ESO\maptexfolders.txt");
			string[] mapfolders = new string[] {"glenumbra", "betnikh", "stormhaven", "strosmkai", "alikr", "balfoyen", "bleakrock", "deshaan", "stonefalls", "auridon", "eastmarch",
			"khenarthi", "shadowfen", "greenshade", "therift", "bankorai", "coldharbour", "grahtwood", "rivenspire", "malabaltor", "reapersmarch", "cyrodiil",
			"craglorn", "orsinium", "thievesguild", "darkbrotherhood", "vvardenfell", "clockworkcity", "summerset", "murkmire", "elsweyr", "dragonhold", "skyrim", "markarth", "blackwood"};
			foreach (string folder in mapfolders) {
				//string matchtext = @"\art\maps\" + folder;
				string matchtext = @"\treasuremaps\treasuremap_" + folder;
				List<byte> matches = new List<byte>();
				TextReader reader = new StreamReader(File.OpenRead(@"F:\Anna\Visual Studio\gr2obj\x64\Release\texplusmeshplustreasure.txt"));
				//TextReader reader = new StreamReader(File.OpenRead(@"F:\Extracted\ESO\maptexids2.txt"));
				int count = 0;
				string line;
				line = reader.ReadLine();
				do {
					if (line.Contains(matchtext)) {
						matches.Add(255);
						count++;
					} else matches.Add(0);
					line = reader.ReadLine();
				} while (line != null);

				Console.WriteLine($"{folder}: {count}");

				var resizer = new MagickGeometry(1024, 64);
				resizer.IgnoreAspectRatio = true;

				MagickImage image = new MagickImage(matches.ToArray(), new MagickReadSettings { Width = matches.Count, Height = 1, Format = MagickFormat.Gray });
				image.Interpolate = PixelInterpolateMethod.Bilinear;
				image.Resize(resizer);
				image.Write("treasuremaps_" + folder + ".png");

				reader.Close();
			}


		}

		static void ESOReadIDS(string pattern, string name, int resolution = 1, RegexOptions options = RegexOptions.IgnoreCase) {
			List<byte> matches = new List<byte>();
			TextReader reader = new StreamReader(File.OpenRead(@"F:\Anna\Visual Studio\gr2obj\x64\Release\texplusmesh.txt"));
			Regex r = new Regex(pattern, options);
			int count = 0;
			while (true) {
				bool match = false;
				for (int i = 0; i < resolution; i++) {
					string line = reader.ReadLine();
					if (line == null) {
						if (match) {
							matches.Add(255);
							count++;
						} else {
							matches.Add(0);
						}
						goto Break;
					}
					if (!match && r.IsMatch(line)) match = true;
				}
				if (match) {
					matches.Add(255);
					count++;
				} else {
					matches.Add(0);
				}
			}
			Break:;
			Console.WriteLine($"{name}: {count}");

			var resizer = new MagickGeometry(1024, 64);
			resizer.IgnoreAspectRatio = true;

			MagickImage image = new MagickImage(matches.ToArray(), new MagickReadSettings { Width = matches.Count, Height = 1, Format = MagickFormat.Gray });
			image.Interpolate = PixelInterpolateMethod.Bilinear;
			image.Resize(resizer);
			image.Write(name + ".png");

			reader.Close();
		}

		static void ESOFindUnnamedTextures() {
			HashSet<int> named = new HashSet<int>();
			HashSet<int> unnamed = new HashSet<int>();

			foreach (string line in File.ReadAllLines(@"F:\Anna\Visual Studio\gr2obj\x64\Release\texunnamed.txt")) {
				unnamed.Add(Int32.Parse(line));
			}
			foreach (string line in File.ReadAllLines(@"F:\Anna\Visual Studio\gr2obj\x64\Release\texnamedids.txt")) {
				named.Add(Int32.Parse(line));
			}
			foreach (string path in Directory.EnumerateFiles(@"F:\Extracted\ESO\badlandsrelease\texfixture")) {
				int id = Int32.Parse(Path.GetFileNameWithoutExtension(path));
				if (unnamed.Contains(id) && !named.Contains(id)) {
					Console.WriteLine(id);
					File.Copy(path, $@"F:\Extracted\ESO\badlandsrelease\texfixtureunnamed\{Path.GetFileName(path)}");
				}
			}
		}

		static void ESOFindUnreferencedTextures() {
			HashSet<int> ids = new HashSet<int>();
			foreach (string path in Directory.EnumerateFiles(@"F:\Extracted\ESO\badlandsrelease\texfixture")) {
				ids.Add(Int32.Parse(Path.GetFileNameWithoutExtension(path)));
			}
			foreach (string line in File.ReadAllLines(@"F:\Anna\Visual Studio\gr2obj\x64\Release\texids.txt")) {
				ids.Remove(Int32.Parse(line));
			}
			foreach (int id in ids) {
				Console.WriteLine(id);
				File.Copy($@"F:\Extracted\ESO\badlandsrelease\texfixture\{id}.dds", $@"F:\Extracted\ESO\badlandsrelease\texfixtureunused\{id}.dds");
			}
		}

		static void ESODatSizes() {
			string path = @"F:\Misc\DepotDownloader\depots\306131\year0\The Elder Scrolls Online\depot\";
			string ptsPath = @"F:\Misc\DepotDownloader\depots\306131\year1\The Elder Scrolls Online\depot\";
			string[] files = Directory.GetFiles(path, "*.dat");
			foreach (string file in files) {
				string newFile = ptsPath + Path.GetFileName(file);
				FileInfo info = new FileInfo(file);
				FileInfo newInfo = new FileInfo(newFile);
				Console.WriteLine($"{info.Name} | {(newInfo.Length - info.Length) / 1048576}");
			}
		}
	}
}
