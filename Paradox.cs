using ImageMagick;
using System;
using System.Collections.Generic;
using System.IO;

namespace SmallScripts {
	static class Paradox {
		static void EKMaskResize() {
			string maskPath = @"F:\Media Libraries\Anna\Documents\Paradox Interactive\Crusader Kings III\mod\blankmap\gfx\TEST\mask.png";
			foreach (string path in Directory.EnumerateFiles(@"F:\Media Libraries\Anna\Documents\Paradox Interactive\Crusader Kings III\mod\blankmap\gfx\TEST\mask")) {
				File.Copy(maskPath, @"F:\Media Libraries\Anna\Documents\Paradox Interactive\Crusader Kings III\mod\blankmap\gfx\TEST\maskresize\" + Path.GetFileName(path));
			}
		}

		static void ImperatorAreas() {
			string[] lines = File.ReadAllLines(@"F:\Media Libraries\Anna\Files\eso\areas.txt");
			Dictionary<string, List<int>> areas = new Dictionary<string, List<int>>();
			Dictionary<string, HashSet<string>> regions = new Dictionary<string, HashSet<string>>();
			for (int i = 1; i <= lines.Length; i++) {
				string area = lines[i - 1].Split('\t')[1];
				if (!areas.ContainsKey(area)) areas[area] = new List<int>();
				areas[area].Add(i);

				string region = lines[i - 1].Split('\t')[0];
				if (!regions.ContainsKey(region)) regions[region] = new HashSet<string>();
				regions[region].Add(area);

			}
			foreach (string area in areas.Keys) {
				Console.Write(area + " = {\n    provinces = {");
				foreach (int i in areas[area]) Console.Write(" " + i.ToString());
				Console.Write(" }\n}\n\n");
			}
			Console.WriteLine(); Console.WriteLine();
			foreach (string region in regions.Keys) {
				Console.Write(region + " = {\n    areas = {");
				foreach (string area in regions[region]) Console.Write("\n        " + area);
				Console.Write("\n    }\n}\n\n");
			}
		}

		static void ImperatorProvinceColorPicker(string filename) {
			string[] lines = File.ReadAllLines(@"C:\Games\ImperatorRome\game\map_data\definition.csv");
			using (MagickImage image = new MagickImage(MagickColors.White, 64, 32 * (400))) {
				image.Format = MagickFormat.Png;
				for (int i = 0; i < 402; i++) {
					string[] vals = lines[i + 2].Split(';');
					new Drawables().FillColor(MagickColor.FromRgb(Byte.Parse(vals[1]), Byte.Parse(vals[2]), Byte.Parse(vals[3]))).Rectangle(0, i * 32, 64, i * 32 + 32).Draw(image);


					//new Drawables().FillColor(MagickColors.White).FontPointSize(24).TextAlignment(TextAlignment.Center).Text(32, i * 32 + 25, vals[0]).Draw(image);
				}
				image.Write(filename + ".png");
			}


		}

		static void ImperatorProvinceColorPicker2() {
			string[] lines = File.ReadAllLines(@"C:\Games\Crusader Kings III\game\map_data\definition.csv");
			using (MagickImage image = new MagickImage(MagickColors.White, 1, 400)) {
				image.Format = MagickFormat.Png;
				for (int i = 0; i < 1000; i++) {
					string[] vals = lines[i + 1].Split(';');
					Console.WriteLine($"{vals[1]} {vals[2]} {vals[3]} {vals[0]}");
					//new Drawables().FillColor(MagickColor.FromRgb(Byte.Parse(vals[1]), Byte.Parse(vals[2]), Byte.Parse(vals[3]))).Rectangle(0, i, 1, i + 1).Draw(image);
					//new Drawables().FillColor(MagickColors.White).FontPointSize(24).TextAlignment(TextAlignment.Center).Text(32, i * 32 + 25, vals[0]).Draw(image);
				}
				//image.Write(filename + ".png");
			}
		}
	}
}
