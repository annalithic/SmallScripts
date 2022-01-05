using System.IO;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SmallScripts {
	class Program {
		static void Main(string[] args) {
			//HashSet<string> bgsbooks = new HashSet<string>(File.ReadAllLines(@"E:\Anna\Desktop\bgsbooksnew.txt"));
			HashSet<string> a = new HashSet<string>(File.ReadAllLines(@"E:\Anna\Desktop\uespbook\tes5.txt"));
			//HashSet<string> b = new HashSet<string>(File.ReadAllLines(@"E:\Anna\Desktop\templorespacebooksfromtable.txt"));
			foreach (string line in File.ReadAllLines(@"E:\Anna\Desktop\tes5lorespacetemp.txt")) if (!a.Contains(line)) Console.WriteLine(line);
			//foreach (string line in File.ReadAllLines(@"E:\Anna\Desktop\c.txt")) if (!b.Contains(line)) Console.WriteLine(line);

			//using (TextWriter w = new StreamWriter(File.Create(@"E:\Anna\Desktop\bgsbooks2.txt"))) {
			//	foreach (string line in File.ReadAllLines(@"E:\Anna\Desktop\bgsbooks.txt")) {
			//		if (!bgsbooks.Contains(line) || line.Length < 3) w.WriteLine(line);
			//   }

			//}

			//TES3.MWBooks(@"F:\Extracted\Morrowind\morrowind.json");
			//Console.WriteLine();
			//TES3.MWBooks(@"F:\Extracted\Morrowind\tribunal.json");
			//Console.WriteLine();
			//TES3.MWBooks(@"F:\Extracted\Morrowind\bloodmoon.json");
			//TESO.CreateTileMap(@"F:\Extracted\ESO\mapmod\Tamriel.png", 4);
			//TESO.CreateTileMap(@"F:\Extracted\ESO\mapmod\blackwood_base.png", 4);
			//TESO.CreateTileMap(@"F:\Extracted\ESO\mapmod\westernskryim_base.png", 4);
			//TESO.CreateTileMap(@"F:\Extracted\ESO\mapmod\southernelsweyr_base.png", 4);
			//TESO.CreateTileMap(@"F:\Extracted\ESO\mapmod\reapersmarch_base.png", 2);
			/*
			HashSet<string> bookcaseTitles = new HashSet<string>();
			foreach(string line in File.ReadAllLines(@"F:\Anna\Files\Unity\esoworldedit\ESOWorldTests\bin\Debug\bookcase4.txt")) {
				bookcaseTitles.Add(line.Split('|')[1]);
			}
			foreach(string line in File.ReadAllLines(@"E:\Anna\Desktop\a.txt")) {
				if (bookcaseTitles.Contains(line)) Console.WriteLine("*" + line);
				else Console.WriteLine();
			}

			string[] lines = File.ReadAllLines(@"E:\Anna\Desktop\ojoj.txt");
			for(int i = 0; i < lines.Length; i += 3)
			{
				int a = int.Parse(lines[i]);
				int b = int.Parse(lines[i+1]);
				int c = int.Parse(lines[i+2]);

				Console.WriteLine($"f {a + 1} {b + 1} {c + 1}");
			}
			*/
			//foreach (string path in Directory.EnumerateFiles(@"C:\Games\Steam\steamapps\common\Warframe\Cache.Windows", "*.toc")) {
			//	Warframe.TOC toc = new Warframe.TOC(path);
			//	Console.WriteLine(Path.GetFileName(path));
			//	toc.Print(@"F:\Extracted\Warframe\TOCW\" + Path.GetFileNameWithoutExtension(path) + ".txt");
			//}
			/*
			foreach (string path in Directory.EnumerateFiles(@"F:\Extracted\Warframe\TOCW\", "*.txt")) {
				string[] lines = File.ReadAllLines(path).Distinct().ToArray();
				Console.WriteLine(path);
				Array.Sort(lines);

				File.WriteAllLines(path, lines);
			}
			*/
			//Warframe.TOC toc = new Warframe.TOC(@"C:\Games\Steam\steamapps\common\Warframe\Cache.Windows\B.Misc.toc");
			//toc.Print();
		}

		static void MHIconRenames2() {
			string[] names = Directory.EnumerateFiles(@"D:\Extracted\MH5R\game\romfs\re_chunk_000\natives\NSW\gui\80_Texture\boss_icon", "*.png").ToArray();
			foreach (string path in names) {
				string name = Path.GetFileName(path);
				if (!name.StartsWith("em")) continue;
				name = name.Replace("_00_", "_");
				name = name.Replace("IAM.tex.28", "MH5R");
				File.Copy(path, @"F:\Media Libraries\Anna\Files\web\FloorBelow.github.io\mh\" + name);
			}
		}

		static void MHIconRenames() {
			string[] names = Directory.EnumerateFiles(@"D:\Extracted\MH5\chunk\ui\note\tex\micon\png").ToArray();
			foreach(string path in names) {
				string name = Path.GetFileName(path);
				name = name.Replace("BC7S_", "");
				name = name.Replace("_ID", "_MH5");
				File.Copy(path, @"F:\Media Libraries\Anna\Files\web\FloorBelow.github.io\mh\" + name);
			}
		}



		static void TroyUnits() {
			List<string> cultures = new List<string>();
			Dictionary<string, bool[]> units = new Dictionary<string, bool[]>();
			string[] lines = File.ReadAllLines(@"F:\_Extracted\TOTALWAR\troy\units.tsv");
			for(int i = 2; i < lines.Length; i++) {
				string[] vals = lines[i].Split('\t');
				if (!cultures.Contains(vals[1])) cultures.Add(vals[1]);
				if (!units.ContainsKey(vals[0])) units[vals[0]] = new bool[19];
				units[vals[0]][cultures.IndexOf(vals[1])] = true;
			}
			Console.WriteLine(cultures.Count);
			using(TextWriter writer = new StreamWriter(File.Open("out.tsv", FileMode.Create))) {
				writer.Write("CULTURES");
				for (int i = 0; i < cultures.Count; i++) writer.Write("\t" + cultures[i]);
				writer.WriteLine();
				foreach(var unit in units) {
					writer.Write(unit.Key);
					for (int i = 0; i < 19; i++) writer.Write("\t" + unit.Value[i]);
					writer.WriteLine();
				}
			}
		}

		static void ACSMusic() {
			foreach (string path in Directory.EnumerateFiles("F:\\_Extracted\\ACSMusic\\new", "*.ogg")) {
				bool onechannel = false;
				using (var vorbis = new NVorbis.VorbisReader(path)) {
					Console.WriteLine(Path.GetFileNameWithoutExtension(path) + ": " + vorbis.Channels.ToString());
					if (vorbis.Channels == 1) onechannel = true;
				}
				if(onechannel) Directory.Move(path, Path.GetDirectoryName(path) + "\\onechannel\\" + Path.GetFileName(path));
			}
		}
	}
}
