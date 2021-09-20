using System.IO;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SmallScripts {
	class Program {
		static void Main(string[] args) {

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
