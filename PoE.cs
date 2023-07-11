﻿using System;
using System.Collections.Generic;
using System.IO;
using Util;
using System.Globalization;
using System.Text;
using ImageMagick;
using System.Diagnostics;
using System.Threading;

namespace SmallScripts {
	static class PoE {

		public static void PoeExtractFSB(int threads = 4) {
			//int debugCount = 0;
			List<string>[] folders = new List<string>[threads];
			for (int i = 0; i < threads; i++) folders[i] = new List<string>();
			int threadIdx = 0;
			int debug = 0;
			foreach (string folder in Directory.EnumerateDirectories(@"F:\Extracted\PathOfExile\3.21.Crucible\audio\bank\")) {
				//debug++;
				//if (debug > 40) break;
				folders[threadIdx].Add(folder);
				threadIdx = (threadIdx + 1) % threads;
			}

			Thread[] thread = new Thread[threads];
			for (int i = 0; i < threads; i++) {
				thread[i] = new Thread(ExtractFSBThread);
				thread[i].Start(folders[i]);
			}

		}


		static void ExtractFSBThread(object obj) {

			List<string> folders = (List<string>)obj;
			for(int i = 0; i < folders.Count; i++) {
				//debugCount++;
				//if (debugCount > 10) break;
				Console.WriteLine($"({i}/{folders.Count}) {Path.GetFileName(folders[i])}");
					using (Process myProcess = new Process()) {
						myProcess.StartInfo.UseShellExecute = false;
						// You can start any process, HelloWorld is a do-nothing example.
						myProcess.StartInfo.WorkingDirectory = folders[i];
						myProcess.StartInfo.FileName = @"F:\Extracted\PathOfExile\3.21.Crucible\audio\fsb_aud_extr.exe";
						myProcess.StartInfo.Arguments = @" 00000000.fsb";
						myProcess.StartInfo.CreateNoWindow = true;
						myProcess.Start();
						myProcess.WaitForExit();
						// This code assumes the process you are starting will terminate itself.
						// Given that it is started without a window so you cannot terminate it
						// on the desktop, it must terminate itself or you can do it programmatically
						// from this application using the Kill method.
					}
			}

			Console.WriteLine("THREAD DONE ----------------------------------------------");
		}

		public static void PoeUIImages(string folder) {
			//PoeUIImages(folder, "Art/UIImages1.txt");
			PoeUIImages(folder, "Art/UIShopImages.txt");
			PoeUIImages(folder, "Art/UIDivinationImages.txt");
			PoeUIImages(folder, "Art/UIXbox.txt");
			PoeUIImages(folder, "Art/UIPS4.txt");

		}

		public static void PoeUIImages(string baseFolder, string lookupPath) {

			HashSet<string> done = new HashSet<string>();

			foreach(string line in File.ReadAllLines(Path.Combine(baseFolder, lookupPath), Encoding.Unicode)) {
				var words = StringSplitIgnoreQuotes(line);
				string name = Path.Combine(baseFolder, "UIImages", words[0].Trim('"'));

				if (done.Contains(name)) continue;

				string atlas = Path.Combine(baseFolder, words[1].Trim('"'));

				int x1 = int.Parse(words[2]); int y1 = int.Parse(words[3]);
				int x2 = int.Parse(words[4]); int y2 = int.Parse(words[5]);


				if (!File.Exists(atlas)) {
					Console.WriteLine(atlas + " NOT EXTRACTED");
					continue;
				}

				MagickImage img = new MagickImage(atlas);
				img.Crop(new MagickGeometry(x1, y1, x2 - x1, y2 - y1));
				if (!Directory.Exists(Path.GetDirectoryName(name))) Directory.CreateDirectory(Path.GetDirectoryName(name));
				img.Write(name + ".png");
				done.Add(name);
				Console.WriteLine(name);

				//foreach (var word in words) Console.Write(word + "|"); Console.WriteLine();
            }
        }

		public static string[] StringSplitIgnoreQuotes(string line) {
			List<string> words = new List<string>();
			StringBuilder current = new StringBuilder();
			bool quoted = false;
			for(int i = 0; i < line.Length; i++) {
				if (line[i] == '"') quoted = !quoted;
				if (!quoted && line[i] == ' ') {
					words.Add(current.ToString());
					current.Clear();
				} else current.Append(line[i]);
            }
			words.Add(current.ToString());
			return words.ToArray();
        }

		public static void LeagueWeeks() {
			foreach(string line in File.ReadAllLines(@"E:\Extracted\PathOfExile\leaguedates.txt")) {
				string[] words = line.Split('\t');

				DateTime date = DateTime.ParseExact(words[1], "yyyy-MM-dd", CultureInfo.InvariantCulture);
				DateTime year = new DateTime(date.Year, 1, 1);
				int days = (date - year).Days;
				int weeks = (days - 1) / 7 + 1;
				Console.WriteLine($"{date.Year} | {weeks} | {words[0]}");
			}
		}

		public static void MapTopologies() {
			Dictionary<int, string> topologyNames = new Dictionary<int, string>();
			foreach(string line in File.ReadAllLines(@"F:\Extracted\PathOfExile\3.16.Scourge\topologies.txt")) {
				string[] words = line.Split('\t');
				topologyNames[int.Parse(words[0])] = words[2];
            }

			foreach (string line in File.ReadAllLines(@"F:\Extracted\PathOfExile\3.16.Scourge\maps.txt")) {
				string[] words = line.Split('\t');
				Console.Write($"{words[0]} {words[1]}");
				foreach (int id in ReadIdArray(words[15])) Console.Write(" " + (topologyNames.ContainsKey(id) ? topologyNames[id] : id.ToString()));
				Console.WriteLine();
			}
        }

		public static void UniqueList() {
			string[] uniqueStashTypes = new string[]{
				"Flask", "Amulet", "Ring", "Claw", "Dagger", "Wand", "Sword", "Axe", "Mace", "Bow", "Staff", "Quiver", 
				"Belt", "Gloves", "Boots", "Body Armour", "Helmet", "Shield", "Map", "Jewel", "Watchstone", "HeistContract"
			};

			HashSet<string> chanceable = new HashSet<string>(File.ReadAllLines(@"F:\Extracted\PathOfExile\3.16.Scourge\chanceable.txt"));

			Dictionary<string, string> uniqueTimeline = new Dictionary<string, string>();
			foreach(string line in File.ReadAllLines(@"F:\Extracted\PathOfExile\3.16.Scourge\uniquetimeline.txt")) {
				string[] split = line.Split('\t');
				uniqueTimeline[split[1]] = split[0];
            }


			string[] words = File.ReadAllLines(@"F:\Extracted\PathOfExile\3.17.Siege\Words.csv");
			for(int i = 0; i < words.Length - 1; i++) {
				words[i] = words[i + 1].Split('\t')[1];
			}

			string[] visualIdentities = File.ReadAllLines(@"F:\Extracted\PathOfExile\3.17.Siege\ItemVisualIdentity.csv");
			for (int i = 0; i < visualIdentities.Length - 1; i++) {
				visualIdentities[i] = visualIdentities[i + 1].Split('\t')[1];
			}

			string[] txtUniques = File.ReadAllLines(@"F:\Extracted\PathOfExile\3.17.Siege\UniqueStashLayout.csv");


			string[] uniqueNames = new string[txtUniques.Length - 1];
			int[] uniqueTypes = new int[txtUniques.Length - 1];
			string[] uniqueArt = new string[txtUniques.Length - 1];
			bool[] isHidden = new bool[txtUniques.Length - 1];

			for (int i = 0; i < txtUniques.Length - 1; i++) {
				string[] split = txtUniques[i + 1].Split('\t');
				string name = words[ReadId(split[0])];
				uniqueNames[i] = name;
				uniqueTypes[i] = ReadId(split[2]);
				uniqueArt[i] = visualIdentities[ReadId(split[1])];
				isHidden[i] = split[8] == "False";
			}

			for (int i = 0; i < uniqueNames.Length; i++) {
				string version = uniqueTimeline.ContainsKey(uniqueNames[i]) ? uniqueTimeline[uniqueNames[i]] : "unk";
				Console.WriteLine($"{i}|{uniqueNames[i]}|{uniqueStashTypes[uniqueTypes[i]]}|{version}|{isHidden[i]}|{chanceable.Contains(uniqueNames[i])}|{uniqueArt[i]}");
			}



		}

		public static void NativeMonsters() {

			//string[] txtWorldAreas = File.ReadAllLines(@"F:\Extracted\PathOfExile\3.16.Scourge\WorldAreas.csv");
			//string[] worldAreas = new string[txtWorldAreas.Length - 1];
			//string[] worldAreaNames = new string[txtWorldAreas.Length - 1];
			//for (int i = 0; i < txtWorldAreas.Length - 1; i++) {
			//	string[] words = txtWorldAreas[i + 1].Split('\t');
			//	worldAreas[i] = words[0];
			//	worldAreaNames[i] = words[1];
			//}

			string[] tags = File.ReadAllLines(@"E:\Extracted\PathOfExile\3.17.Siege\Tags.csv");
			for (int i = 0; i < tags.Length - 1; i++) tags[i] = tags[i + 1].Substring(0, tags[i+1].IndexOf('\t'));


			string[] monsterTypes = File.ReadAllLines(@"E:\Extracted\PathOfExile\3.17.Siege\MonsterTypes.csv");
			for (int i = 0; i < monsterTypes.Length - 1; i++) monsterTypes[i] = monsterTypes[i + 1].Substring(0, monsterTypes[i+1].IndexOf('\t'));
			List<string>[] monsterTypeVarieties = new List<string>[monsterTypes.Length];

			string[] txtMonsterVarieties = File.ReadAllLines(@"E:\Extracted\PathOfExile\3.17.Siege\MonsterVarieties.csv");
			//string[] monsterVarieties = new string[txtMonsterVarieties.Length - 1];
			//string[] monsterVarietyNames = new string[txtMonsterVarieties.Length - 1];
			//int[] monsterVarietyTypes = new int[txtMonsterVarieties.Length - 1];

			for (int i = 1; i < txtMonsterVarieties.Length - 1; i++) {
				string[] words = txtMonsterVarieties[i + 1].Split('\t');
				//monsterVarieties[i] = words[0].Substring(18);
				//monsterVarietyNames[i] = words[32];

				int type = ReadId(words[1]);
				if (monsterTypeVarieties[type] == null) monsterTypeVarieties[type] = new List<string>();
				monsterTypeVarieties[type].Add($"|{words[0].Substring(18)} ({words[32]})");

				//string type = monsterTypes[ReadId(words[1])];
				//Console.WriteLine($"{words[0]}|{words[7]}|{words[32]}|{type}|{words[46]}|{words[70]}|{words[72]}|{words[73]}|{words[74]}|{words[82]}|{words[83]}|{words[85]}");
				
				
				//foreach (int id in ReadIdArray(words[19])) Console.Write($"{tags[id]}, ");
				//Console.WriteLine();
				//monsterVarietyTypes[i] = int.Parse(words[1].Substring(1, words[1].IndexOf(',') - 1));
			}

			for(int i = 0; i < monsterTypes.Length; i++) {
				Console.Write($"{i}|{monsterTypes[i]}");
				if (monsterTypeVarieties[i] != null)
					for (int v = 0; v < Math.Min(monsterTypeVarieties[i].Count, 10); v++)
						Console.Write(monsterTypeVarieties[i][v]);
				Console.WriteLine();
            }
			/*
			string[] txtMonsterPacks = File.ReadAllLines(@"F:\Extracted\PathOfExile\3.16.Scourge\MonsterPacks.csv");
			string[] monsterPacks = new string[txtMonsterPacks.Length - 1];
			List<int>[] worldAreaPacks = new List<int>[worldAreas.Length];
			for (int i = 0; i < txtMonsterPacks.Length - 1; i++) {
				string[] words = txtMonsterPacks[i + 1].Split('\t');
				monsterPacks[i] = words[0];
				int worldArea = ReadId(words[1], 2);
				if (worldAreaPacks[worldArea] == null) worldAreaPacks[worldArea] = new List<int>();
				worldAreaPacks[worldArea].Add(i);
			}
			for(int i = 1; i < 50; i++) {
				Console.Write($"{worldAreas[i]}|{worldAreaNames[i]}");
				if (worldAreaPacks[i] != null) foreach (int id in worldAreaPacks[i]) Console.Write($"|{monsterPacks[id]}");
				Console.WriteLine();
            }
			*/
			

			//for (int i = 1000; i < 1500; i++) Console.WriteLine($"{monsterVarieties[i]} | {monsterVarietyNames[i]} | {monsterTypes[monsterVarietyTypes[i]]}");
        }

		static int ReadId(string word, int start = 1) {
			if (word.StartsWith("<")) start = 1;
			return int.Parse(word.Substring(start, word.IndexOf(',') - start));
		}

		static int[] ReadIdArray(string word) {
			if (!word.StartsWith("[")) return null;
			List<int> ids = new List<int>();
			string[] words = word.Split('<');
			for (int i = 1; i < words.Length; i++) {
				ids.Add(int.Parse(words[i].Substring(0, words[i].IndexOf(','))));
            }

			return ids.ToArray();
        }

		static void SMDToObj(string inPath) {
			using(BinaryReader r = new BinaryReader(File.Open(inPath, FileMode.Open))) {
				SMD smd = new SMD(r);
			}
		}
		public static void MonsterTypes() {
			string[] lines = File.ReadAllLines(@"F:\Extracted\PathOfExile\3.17.Siege\MonsterTypes.csv");
			string[] monsterTypes = new string[lines.Length - 1];
			for (int i = 0; i < monsterTypes.Length; i++) monsterTypes[i] = lines[i + 1].Substring(0, lines[i + 1].IndexOf(','));

			lines = File.ReadAllLines(@"F:\Extracted\PathOfExile\3.17.Siege\MonsterVarieties.csv");
			string[] monsterVarietyIDs = new string[lines.Length - 1];
			string[] monsterVarietyNames = new string[lines.Length - 1];
			int[] monsterVarietyTypes = new int[lines.Length - 1];
			for (int i = 2; i < monsterVarietyIDs.Length; i++) {
				monsterVarietyIDs[i - 1] = lines[i].Substring(18, lines[i].IndexOf(',') - 18);
				monsterVarietyTypes[i - 1] = int.Parse(lines[i].Substring(lines[i].IndexOf('<') + 1, 4).Split(',')[0]);
				monsterVarietyNames[i - 1] = lines[i].Split(',')[34];
			}

			for (int i = 0; i < monsterTypes.Length; i++) {
				for (int k = 0; k < monsterVarietyIDs.Length; k++) {
					if (monsterVarietyTypes[k] == i) Console.WriteLine($"{monsterTypes[monsterVarietyTypes[k]]}|{monsterVarietyIDs[k]}|{monsterVarietyNames[k]}");
				}
			}

		}
	}

	public struct SMDVert {
		public float x;
		public float y;
		public float z;
		public float u;
		public float v;
	}



	class SMD {
		byte version;
		ushort numShapes;
		uint numTris;
		uint numVerts;

		public uint[] idx;

		public SMD (BinaryReader r) {
			version = r.ReadByte();
			if (version != 3) {
				Console.WriteLine($"version {version} not implemented!");
				return;
			}
			r.Seek(1);
			numShapes = r.ReadUInt16();
			r.Seek(41);
			numTris = r.ReadUInt32();
			numVerts = r.ReadUInt32();
			idx = new uint[numTris * 3];
			if(numVerts > 65535) {
				for (int i = 0; i < idx.Length; i++) idx[i] = r.ReadUInt32();
			} else {
				for (int i = 0; i < idx.Length; i++) idx[i] = r.ReadUInt16();
			}

			

		}
	}
}
