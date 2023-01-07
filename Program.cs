using System.IO;
using System;
using System.Collections.Generic;
using System.Linq;
using ImageMagick;
using ImageMagick.Formats;
using System.Threading;
using System.Diagnostics;

namespace SmallScripts {
	class Program {
		static void Main(string[] args) {

			//PoE.PoeUIImages(@"F:\Extracted\PathOfExile\3.20.Sanctum\ROOT"); return;

			//for (int g = 0; g < 30; g++) Console.WriteLine(64 * g + 12); return;

			//TES3.MWMapResize(40, 64); return;
			TES3.MWRegionDoors(@"F:\Extracted\BGS\tr_mainland.json"); return;


			TES3.MWMapCombine(); return;


			TES3.MWDoors(@"F:\Extracted\BGS\tr_mainland.json"); return;


			PoE.LeagueWeeks(); return;

			foreach (string file in Directory.EnumerateFiles(@"F:\Extracted\GI\Texture2D", "*.png")) {
				if (file.Contains('#')) {
					string filename = Path.GetFileNameWithoutExtension(file).Split('#')[0] + ".png";
					if (File.Exists(Path.Combine(Path.GetDirectoryName(file), filename)))
						File.Move(file, Path.Combine(@"F:\Extracted\GI\extra", Path.GetFileName(file)));
                }
            } return;

			TES3.MWQuests(@"F:\Extracted\BGS\morrowind.json", @"F:\Extracted\BGS\tribunal.json", @"F:\Extracted\BGS\bloodmoon.json", @"F:\Extracted\BGS\tr_mainland.json"); return;


			if(args.Length == 1) {
				ConvertJxl(args[0], 3, ".bmp");
			}
			//TES3.FO3LodCombine(); return;
			//TES3.FO76DepthMap(@"E:\Extracted\BGS\fo76utils\papermap_city_h.dds"); return;

			

			TESO.CreateTileMap(@"E:\Extracted\ESO\mapmod\tamriel.png", 4);
			return;

			//Souls.EldenRingUnpackTex(@"C:\Games\Steam\steamapps\common\ELDEN RING\Game\menu\71_maptile-tpfbhd\71_MapTile");
			Souls.EldenRingMapCompose4();
			return;

			int i = 0;
			foreach(string file in Directory.EnumerateFiles(@"E:\Extracted\ESO\model", "*.gr2")) {
				string newpath = @"F:\Extracted\ESO\model\" + Path.GetFileName(file);
				if (!File.Exists(newpath)) {
					Console.WriteLine(newpath);
					File.Copy(file, newpath);
				}
				i++; if (i % 1000 == 0) Console.WriteLine(i);
            }

			//Souls.EldenRingUnpackTex(@"C:\Games\Steam\steamapps\common\ELDEN RING\Game\menu\71_maptile-tpfbhd\71_MapTile");

			//Souls.EldenRingMapCompose4();

			/*
			using(TextReader r = new StreamReader(File.OpenRead(@"E:\Anna\Desktop\a.reg"))) {
				string line = r.ReadLine();
				while(line != null ) {
					if (line.Contains("imperator")) Console.WriteLine(line);
					line = r.ReadLine();
				}
            }
			*/

			//Paradox.V3SOL();
			//Souls.EldenRingAssetMap("sarcophagi", "AEG099_373", "AEG099_377", "AEG099_378", "AEG007_705", "AEG007_706", "AEG007_707");
			//Souls.EldenRingAssetMap("skyruins", "AEG007_076", "AEG007_077", "AEG007_078", "AEG007_079", "AEG007_080");

			//BTL btl = new BTL();
			//btl.Version = 18;
			//btl.Write(@"C:\Games\Steam\steamapps\common\ELDEN RING\Game\map\m31\m31_18_00_00\m31_18_00_00_0000.btl.dcx", DCX.Type.DCX_KRAK);
			//foreach (string path in Directory.EnumerateFiles(@"C:\Games\Steam\steamapps\common\ELDEN RING\Game\map\m10\m10_01_00_00", "*.mapbnd.dcx")) Console.WriteLine(path);
			//Souls.EldenRingMapCompose3();
			//Souls.EldenRingUnpackTex(@"E:\Extracted\Souls\Elden Ring\menu\hi\00_solo-tpfbhd\00_Solo");
			//Souls.EldenRingMapCompose();
			//PoE.NativeMonsters();
			//PoE.NativeMonsters();
			//V3.RandomiseInterestGroups();
			//TES3.MWTesAnnwynColorMap();

			//HashSet<string> bgsbooks = new HashSet<string>(File.ReadAllLines(@"E:\Anna\Desktop\bgsbooksnew.txt"));
			//HashSet<string> a = new HashSet<string>(File.ReadAllLines(@"E:\Anna\Desktop\uespbook\tes5.txt"));
			//HashSet<string> b = new HashSet<string>(File.ReadAllLines(@"E:\Anna\Desktop\templorespacebooksfromtable.txt"));
			//foreach (string line in File.ReadAllLines(@"E:\Anna\Desktop\tes5lorespacetemp.txt")) if (!a.Contains(line)) Console.WriteLine(line);
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

		static void ConvertJxl(string path, int threads = 3, string extension = ".png") {

			List<string>[] lists = new List<string>[threads];
			for (int i = 0; i < lists.Length; i++) lists[i] = new List<string>();
			int current = 0;

			foreach (string file in Directory.EnumerateFiles(path, "*" + extension, SearchOption.AllDirectories)) {
				if (File.Exists(file.Replace(extension, ".jxl"))) {
					FileInfo jxlinfo = new FileInfo(file.Replace(extension, ".jxl"));
					if(jxlinfo.Length > 0) continue;
				}
				FileInfo info = new FileInfo(file); if (info.Length == 0) continue;
				lists[current].Add(file);
				current = (current + 1) % threads;
				
			}
			Stopwatch s = Stopwatch.StartNew();
			Thread[] thread = new Thread[threads];
			for(int i = 0; i < threads; i++) {
				thread[i] = new Thread(ConvertJxlThread);
				thread[i].Start(lists[i]);
            }
			for (int i = 0; i < threads; i++) thread[i].Join();
			Console.WriteLine(s.ElapsedMilliseconds);
        }

		static void ConvertJxlThread(Object obj) {
			List<string> files = (List<string>)obj;
			JxlWriteDefines write = new JxlWriteDefines() { Effort = 3 };
			for(int i = 1; i < files.Count; i++) {

				Console.WriteLine($"{i}/{files.Count} {files[i]}");
				MagickImage image = new MagickImage(files[i]);
				image.Quality = 100;
				image.Write(Path.Combine(Path.GetDirectoryName(files[i]), Path.GetFileNameWithoutExtension(files[i])) + ".jxl", write);
				File.Delete(files[i]);
			}
			Console.WriteLine("Done");
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
