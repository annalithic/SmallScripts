using System.IO;
using System;
using System.Collections.Generic;
using System.Linq;
using ImageMagick;
using ImageMagick.Formats;
using System.Threading;
using System.Diagnostics;
using Newtonsoft.Json.Linq;
using System.Threading.Tasks;
using System.Net.Http;
using System.Net;

namespace SmallScripts {
	class Program {


		static void Combine(int start, int total, int[] numbers, bool[] found) {
			total = total + numbers[start];
            for (int i = start + 1; i < numbers.Length; i++) {
				Combine(i, total, numbers, found);
			}
			found[total] = true;
		}

		static void NumberGrid(int textSize = 8,int cellSize = 40, int minX = -42, int minY = -64, int maxX = 60, int maxY = 37) {

			int cellsX = maxX - minX; int cellsY = maxY - minY;
			MagickImage image = new MagickImage(MagickColors.Black, cellsX * cellSize, cellsY * cellSize);

            for (int y = 0; y < cellsX; y++) {
                for (int x = 0; x < cellsY; x++) {
                        image.Draw(new Drawables().FontPointSize(textSize).Font("Lucida Console").FillColor(MagickColors.White).TextAlignment(TextAlignment.Center)
                            .Text(cellSize * x + cellSize / 2, image.Height - cellSize * y - cellSize / 2, $"{minX + x},{minY + y}"));
                }
				Console.Write('*');
            }
            WebPWriteDefines write = new WebPWriteDefines() { Lossless = true, Method = 0 };
            image.Quality = 20;
            int imageCount = 0;
            while (File.Exists($"gridnumbers_{imageCount}.webp")) imageCount++;
            image.Write($"gridnumbers_{imageCount}.webp", write);
        }

        static void MapGenieMontage2(string folder) {
			int start = 65024 + 128;
			for (int blockY = 0; blockY < 8; blockY++) {
				for (int blockX = 0; blockX < 8; blockX++) {
					MagickImageCollection images = new MagickImageCollection();
					for (int y = 0; y < 32; y++) {
						for (int x = 0; x < 32; x++) {
							string imagePath = folder + $"/{start + y + blockY * 32}_{start + x + blockX * 32}.jpg";
							Console.WriteLine(imagePath);
							images.Add(new MagickImage(imagePath));
						}
					}
					var montage = images.Montage(new MontageSettings() { Geometry = new MagickGeometry(256) });
					montage.Write($@"E:\Extracted\ACRED\block_{blockY}_{blockX}.webp", new WebPWriteDefines { Lossless = true });
				}
			}
        }

        static void MapGenieMontage(string folder) {
            MagickImageCollection images = new MagickImageCollection();
            foreach (string path in Directory.EnumerateFiles(folder, "*.jpg")) {
                images.Add(new MagickImage(path));
            }
            var montage = images.Montage(new MontageSettings() { Geometry = new MagickGeometry(256) });
            montage.Write(folder + "/montage.png");
        }

        static void MapGenieRequest() {
            WebClient client = new WebClient();
			//for (int y = 2032; y < 2048; y++) {
			//    for (int x = 2032; x < 2048; x++) {
			//        GetMapTile(client, $"https://tiles.mapgenie.io/games/assassins-creed-shadows/japan/satellite-v1/12/{y}/{x}.jpg", x, y);
			//    }
			//}

			for (int y = 65024 + 128; y < 65536 - 128; y++) {
				for (int x = 65024 + 128; x < 65536 - 128; x++) {
					GetMapTile(client, $"https://tiles.mapgenie.io/games/assassins-creed-shadows/japan/satellite-v1/17/{y}/{x}.jpg", x, y);
				}
			}

		}

		static void GetMapTile(WebClient client, string address, int x, int y) {
            string filename = string.Format("E:/Extracted/ACRED/tiles/{0:D3}_{1:D3}.jpg", y, x);
			if (File.Exists(filename)) return;
            Console.WriteLine(address);
            try {
                client.DownloadFile(new Uri(address), filename);
            } catch { }
        }


        static void Main(string[] args) {
			TES3.MapNpcs(@"E:\Extracted\Morrowind\TR_Mainland.json"); return;

            TES3.MWDoors(@"E:\Extracted\Morrowind\TR_Mainland.json", 0.5625f, -44.3125f, 1.5f); return; //hlerynhul

            //MapGenieMontage2(@"E:\Extracted\ACRED\tiles"); return;
            //MapGenieRequest(); return;
            TES3.TES3QuestInfo(@"E:\Extracted\Morrowind\TR_Mainland.json"); return;
            TES3.DoorsMerged(@"E:\Extracted\Morrowind\TR_Mainland.json", true); return;
            TES3.MWListUnknownUnusedDoorCells(@"E:\Extracted\Morrowind\TR_Mainland.json"); return;
            TES3.MWDoors(@"E:\Extracted\Morrowind\TR_Mainland.json", 4.75f, -52, 3.5f); return; //naris
            //TES3.MWDoors(@"E:\Extracted\Morrowind\trmainland.json"); return;
            //TES3.MWDoors(@"E:\Extracted\Morrowind\trmainland.json", 5.125f, -34.125f, 0.75f); return; //idathren
            //TES3.MWDoors(@"E:\Extracted\Morrowind\trmainland.json", 0.875f, -32.5f, 1.75f); return; //hlan oek
            //TES3.MWDoors(@"E:\Extracted\Morrowind\trmainland.json", 4.5f, -28.5f, 3f, 2f); return; //almas thirr
            //TES3.MWDoors(@"F:\Extracted\BGS\tr_mainland.json", 16.75f, 14.5f, 2); return; //Firewatch
            //TES3.MWDoors(@"F:\Extracted\BGS\tr_mainland.json", 24.75f, 0.75f, 1); return; //Helnim


            PoE.PoeUIImages(@"E:\Extracted\PathOfExile2\Day5\art"); return;

			Souls.EldenRingMapCompose4("00"); return;

			Souls.EldenRingListMapMask(@"F:\Extracted\Elden Ring\DEEELCEE\menu\71_maptile-mtmskbnd-dcx\MENU_MapTile_M00.mtmsk"); return;

			foreach(string tile in Directory.EnumerateFiles(@"F:\Extracted\Elden Ring\DEEELCEE\menu\71_maptile-tpfbhd", "*.dds", SearchOption.AllDirectories)) {
				Console.WriteLine(tile);
				File.Move(tile, @"F:\Extracted\Elden Ring\DEEELCEE\menu\dds\" + Path.GetFileName(tile));
			}
			return;
			//TES3.MWListUnknownUnusedDoorCells(@"E:\Extracted\Morrowind\tes3conv_new\TR_24_06_17.json"); return;
			//PoE.LeagueWeeks(); return;

			//TES3.MWListUnknownUnusedDoorCells(@"E:\Extracted\Morrowind\tes3conv_new\TR_24_05_21.json"); return;
            TES3.DoorsMerged(@"E:\Extracted\Morrowind\tes3conv_new\TR_24_06_17.json", true); return;

            //TES3.DoorsListNew(@"E:\Extracted\Morrowind\tes3conv_new\TR_24_05_21.json");
            //TES3.DoorsListNew(@"E:\Extracted\Morrowind\tes3conv\NEWmergeTR.json"); return;

            TES3.LodMeshes3(); return;



            TES3.TES3StaticList2(@"E:\Extracted\Morrowind\tes3conv\MWMerge.json", @"E:\Extracted\Morrowind\tes3conv\TR-22-02-24.json"); return;


            TES3.MeshTextures("dwe"); return;
            //TES3.DoorsMerged(@"E:\Extracted\Morrowind\trmainland.json"); return;

            //TES3.TES3QuestInfo(@"E:\Extracted\Morrowind\bloodmoon.json"); return;

            //TES3.TES3QuestInfo(@"E:\Extracted\Morrowind\tes3conv\TR_Mainland.json"); return;



			List<string> esps = new List<string>() { @"E:\Extracted\Morrowind\morrowind.json", @"E:\Extracted\Morrowind\bloodmoon.json" };
			foreach (string path in Directory.EnumerateFiles(@"E:\Extracted\Morrowind\tes3conv", "*.json")) esps.Add(path);
			TES3.TES3StaticList(esps.ToArray()); return;


            string filename2 = @"F:\Anna\Desktop\3waybillboard2.nif";
			int level = 1;
            for (int i = 1; i <= 4; i++) {
				File.Copy(filename2, string.Format(@"E:\Games\MorrowindMods\lodtest\meshes\tr\f\tr_flora_ow_big{0:00}_dist_{1}.nif", i, level), true);
			}
            for (int i = 1; i <= 7; i++) {
                File.Copy(filename2, string.Format(@"E:\Games\MorrowindMods\lodtest\meshes\tr\f\tr_flora_ow_med{0:00}_dist_{1}.nif", i, level), true);
            }
            for (int i = 1; i <= 3; i++) {
                File.Copy(filename2, string.Format(@"E:\Games\MorrowindMods\lodtest\meshes\tr\f\tr_flora_ow_sma{0:00}_dist_{1}.nif", i, level), true);
            }
            for (int i = 1; i <= 8; i++) {
                File.Copy(filename2, string.Format(@"E:\Games\MorrowindMods\lodtest\meshes\tr\f\tr_flora_ow_tal{0:00}_dist_{1}.nif", i, level), true);
            }
            return;

            TES3.OpenMWMapCombine(@"F:\Extracted\Morrowind\MAPSTRNEWSMALL\maps", 128); return;

            foreach (string path in Directory.EnumerateFiles(@"E:\Extracted\Morrowind\tes3conv", "*.json")) TES3.MWDoors(path); return;


            NumberGrid(); return;




            foreach (string line in File.ReadAllLines(@"F:\Anna\Desktop\a.txt")) {
				string[] words = line.Split('|');
				int y = int.Parse(words[2]);
				words[2] = (y * 2 / 3).ToString();
				Console.Write(words[0]);
				for(int i = 1; i < words.Length; i++) Console.Write('|' + words[i]);
				Console.WriteLine();
			} return;


            //fun with combinatronics
            bool[] found = new bool[500];
			int[] numbers = new int[] { 1, 2, 4, 8, 16, 32, 64 };

			for(int i = 0; i < numbers.Length; i++) Combine(i, 0, numbers, found);


            for (int i = 0; i < 100; i++) Console.Write(found[i] ? "__ " : string.Format("{0} ", i));




			return;

			Starfield.TestDensityMaps(); return;
			Starfield.ListAnimalBiomes(); return;

			Starfield.ListPlanets(); return;

			Dictionary<string, List<string>> plants = new Dictionary<string, List<string>>();
			foreach (string line in File.ReadAllLines(@"E:\Anna\Anna\Delphi\TES5Edit\Build\Edit Scripts\planets.txt")) {
				string[] words = line.Split('|');
				if (words.Length <= 1) continue;
				string name = $"{words[0]}|{words[2]}|{words[3]}|";
				if (!plants.ContainsKey(name)) plants[name] = new List<string>();
				plants[name].Add(words[1]);

			}

			foreach(string plant in plants.Keys) {
				Console.Write(plant);
				for (int i = 0; i < plants[plant].Count - 1; i++) Console.Write(plants[plant][i] + ", ");
				if (plants[plant].Count > 0) Console.Write(plants[plant][plants[plant].Count - 1]);
				Console.WriteLine();
			}

			return;
			/*
			Dictionary<string, List<string>> biomes = new Dictionary<string, List<string>>();
			Dictionary<string, List<string>> planets = new Dictionary<string, List<string>>();
			HashSet<string> lifePlanets = new HashSet<string>();
			foreach(string line in File.ReadAllLines(@"E:\Anna\Anna\Delphi\TES5Edit\Build\Edit Scripts\planets.txt")) {


				string[] words = line.Split('|');

				if (!words[1].Contains("NoLife") && !words[1].Contains("LifeExtreme")) lifePlanets.Add(words[0]);

				if (!biomes.ContainsKey(words[1])) biomes[words[1]] = new List<string>();
				biomes[words[1]].Add(words[0]);

				if (!planets.ContainsKey(words[0])) planets[words[0]] = new List<string>();
				planets[words[0]].Add(words[1]);
			}
			foreach (string planet in lifePlanets) {
				planets[planet].Sort();
				Console.Write(planet + ": ");
				foreach (string biome in planets[planet]) {
					Console.Write(biome + ", ");
				}
				Console.WriteLine();
			}
			foreach (string planet in planets.Keys) {
				if (lifePlanets.Contains(planet)) continue;
				planets[planet].Sort();
				Console.Write(planet + ": ");
				foreach (string biome in planets[planet]) {
					Console.Write(biome + ", ");
				}
				Console.WriteLine();
			}

			return;

						foreach (string biome in biomes.Keys) {
			biomes[biome].Sort();
			Console.Write(biome + ": ");
			foreach(string planet in biomes[biome]) {
				Console.Write(planet + ", ");
			}
            Console.WriteLine();
            }

				*/


			return;

			PoE.PoeUIImages(@"F:\Extracted\PathOfExile\3.22.Ancestor\art"); return;


			string bipedfolder = @"F:\Extracted\PathOfExile\3.22.Ancestor\monsters\genericbiped\bipedsmall\animations";
			foreach (string file in Directory.EnumerateFiles(bipedfolder, "*.ast", SearchOption.AllDirectories)) {
				string newname = file.Substring(bipedfolder.Length + 1).Replace('\\', '_');
				Console.WriteLine(newname);
				File.Copy(file, Path.Combine(@"F:\Extracted\PathOfExile\3.22.Ancestor\bipedS", newname));
            }
			return;

			AverageImages(@"F:\Extracted\PathOfExile\4.0.POE2\TrailerFrames\30_SingingCaverns_Monk");
			return;

			foreach(string file in Directory.EnumerateFiles(@"F:\Extracted\Riven", "*.png", SearchOption.AllDirectories)) {
				string[] words = Path.GetFileNameWithoutExtension(file).Split('_', '.');
				if (words.Length == 1) continue;

				MagickImage image = new MagickImage(file);
				if(image.Width >= 608 && image.Height >= 392) {
					Console.WriteLine(file);
					string folder = Path.Combine(@"F:\Extracted\RivenPictures", words[1]);
					if (!Directory.Exists(folder)) Directory.CreateDirectory(folder);
					File.Copy(file, Path.Combine(folder, Path.GetFileName(file)), true);
                }
            }
			return;

			PoE.PoeUIImages(@"F:\Extracted\PathOfExile\a\art"); return;

			Random r = new Random(0);
			string[] pictures = Directory.EnumerateFiles(@"F:\BACKUP\PicturesMAKENEWFOLDER", "*.*", SearchOption.AllDirectories).ToArray();
			Console.WriteLine("SORTING");
			Array.Sort(pictures);
			Console.WriteLine("RANDOMIZING");
			//fisher-yates
			for(int count = pictures.Length - 1; count > 0; count--) {
				int swapVal = r.Next(count);
				string temp = pictures[count];
				pictures[count] = pictures[swapVal];
				pictures[swapVal] = temp;
            }
			for(int folderNum = 0; folderNum < (pictures.Length + 100) / 100; folderNum++) {
				if(!Directory.Exists($@"F:\BACKUP\PicturesSelected\{folderNum + 1}")) {
					Console.WriteLine($"saving {folderNum + 1}");
					Directory.CreateDirectory($@"F:\BACKUP\PicturesSelected\{folderNum + 1}");
					//Directory.CreateDirectory($@"F:\BACKUP\PicturesSelected\{folderNum + 1}\cherry");
					for (int asd = 0; asd < 100; asd++) {
						File.Copy(pictures[asd + folderNum * 100], $@"F:\BACKUP\PicturesSelected\{folderNum + 1}\" + Path.GetFileName(pictures[asd + folderNum * 100]), true);
					}
					break;
				}
			}


			return;
			PoE.PoeExtractFSB(6);


			return;

			for(int cultures = 1; cultures <= 20; cultures++) {
				for(int professions = 1; professions <= 20; professions++) {
					string res = (professions >= 15 && cultures <= 12) || (professions == 14 && cultures <= 13) || (professions == 13 && cultures <= 14) || (professions <= 12 && cultures <= 15) ? "Y" : "N";
					string test = (cultures + Math.Max(Math.Min(professions, 15), 12)) < 28 ? "Y" : "N";
					Console.Write($"{test}_{res} ");
				}
				Console.WriteLine();
            }
			return;

            if (args.Length == 1) {
                ConvertJxl(args[0], 3, ".png");
            }

			return;
            //TES3.OpenMWMapCombine(@"C:\maps", 512); return;

            foreach (string path in Directory.EnumerateFiles(@"F:\Extracted\Diablo\Textures", "*.dds")) {
				if (Path.GetFileName(path).StartsWith("mmap_")) {
					var image = new MagickImage(path);
					image.Write(Path.Combine(@"F:\Extracted\Diablo\mmap\", Path.GetFileNameWithoutExtension(path) + ".png"));
                }
            }
			return;


			int xCount; int yCount; int tilesize; string fileName;

			xCount = 40;
			yCount = 40;
			fileName = "zmap_Sanctuary_Eastern_Continent";
			tilesize = 256;

			xCount = 13;
			yCount = 13;
			fileName = "zmap_Kehj_Hell";
			tilesize = 512;

			xCount = 9;
			yCount = 9;
			fileName = "zmap_Kehj_Hell_Sightless_Eye";
			tilesize = 512;


			MagickImageCollection montage = new MagickImageCollection();
			for(int y = 0; y < yCount; y++ ) {
				for(int x = 0; x < xCount; x++) {
					MagickImage image = new MagickImage(string.Format(@"F:\Extracted\Diablo\Textures\{2}_{0:00}_{1:00}.dds", x, y, fileName));
					if(image.Width != tilesize || image.Height != tilesize) image.Resize(256, 256);
					montage.Add(image); ;
					Console.WriteLine($"{x} {y}");
				}
			}

			MontageSettings montageSettings = new MontageSettings() { Geometry = new MagickGeometry(256), TileGeometry = new MagickGeometry(xCount, yCount) };
			var map = montage.Montage(montageSettings);
			WebPWriteDefines write = new WebPWriteDefines() { Lossless = true, Method = 0 };
			//JxlWriteDefines write = new JxlWriteDefines() { Effort = 2 };
			map.Write($"{fileName}.webp", write);



			PoE.LeagueWeeks(); return;

			//TES3.TES3QuestInfo(@"F:\Extracted\BGS\bloodmoon.json"); return;
			//TES3.TES3GridmapCoords(); return;
			for (int j = 1; j <= 60; j++) Console.WriteLine($"*{j}. "); return;





			//TES3.TES3IntCellResizeTest();
			//TES3.TES3ListInts(@"F:\Extracted\BGS\morrowind.json"); return;

			TES3.MWRegionCreateMaps(@"F:\Extracted\BGS\morrowind.json"); return;
			//TES3.TES3LocalMapCombine(1024, 3, -); return;
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


			//TES3.FO3LodCombine(); return;
			//TES3.FO76DepthMap(@"E:\Extracted\BGS\fo76utils\papermap_city_h.dds"); return;

			

			TESO.CreateTileMap(@"E:\Extracted\ESO\mapmod\tamriel.png", 4);
			return;

			//Souls.EldenRingUnpackTex(@"C:\Games\Steam\steamapps\common\ELDEN RING\Game\menu\71_maptile-tpfbhd\71_MapTile");
			Souls.EldenRingMapCompose4();
			return;
			/*
			int i = 0;
			foreach(string file in Directory.EnumerateFiles(@"E:\Extracted\ESO\model", "*.gr2")) {
				string newpath = @"F:\Extracted\ESO\model\" + Path.GetFileName(file);
				if (!File.Exists(newpath)) {
					Console.WriteLine(newpath);
					File.Copy(file, newpath);
				}
				i++; if (i % 1000 == 0) Console.WriteLine(i);
            }
			*/
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


		static void AverageImages(string folder, string extension = "*.png") {
			long[] pixels = null;
			int imageCount = 0;
			int width = 0;
			int height = 0;
			foreach(string imagePath in Directory.EnumerateFiles(folder, extension)) {
				Console.WriteLine(Path.GetFileName(imagePath));
				MagickImage image = new MagickImage(imagePath);
				if (pixels is null) {
					width = image.Width;
					height = image.Height;
					pixels = new long[width * height * 3];
				}
				int i = 0;
				foreach(var pixel in image.GetPixels()) {
					
					pixels[i] += pixel[0];
					pixels[i+1] += pixel[1];
					pixels[i+2] += pixel[2];
					i+=3;
				}
				imageCount++;
			}
			Console.WriteLine("Dividing");
			byte[] pixelData = new byte[pixels.Length];
			for(int i = 0; i < pixelData.Length; i++) {
				pixelData[i] = (byte)(pixels[i] / imageCount);
            }
			Console.WriteLine("Writing");
			MagickImage output = new MagickImage();
			output.ReadPixels(pixelData, new PixelReadSettings(width, height, StorageType.Char, PixelMapping.RGB));
			output.Quality = 100;
			output.Write(Path.Combine(folder, "Merged.webp"), new WebPWriteDefines() { Lossless = true });
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
