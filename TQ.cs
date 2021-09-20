using System;
using System.IO;
using ImageMagick;
using Util;

namespace SmallScripts {
	static class TQ {

		/*
		TQ.CreateMap(@"Levels\World\Greece", @"XPack2\Levels\Corinthia", @"XPack2\Levels\Primrose\DelphiActorsTemple");
		TQ.CreateMap(@"Levels\World\Egypt", @"XPack2\Levels\Primrose\RhakotisSlumsExtra02b");
		TQ.CreateMap(@"Levels\World\Orient", @"Levels\World\Babylon", @"XPack2\Levels\WorldQuest");
		TQ.CreateMap(@"Levels\World\Olympus");
		TQ.CreateMap(@"XPack\Levels");
		TQ.CreateMap(@"XPack2\Levels\CelticHeartlands", @"XPack2\Levels\Asgard", @"XPack2\Levels\DarkLands", @"XPack2\Levels\Jotunheim", @"XPack2\Levels\Muspelheim",
					 @"XPack2\Levels\Scandia", @"XPack2\Levels\WildLands", @"XPack2\Levels\Yggdrasil", @"XPack2\Levels\Primrose\PrimroseGrid01");
		TQ.CreateMap(@"XPack3\Levels");
		*/

		static void CreateMap(params string[] locations) {
			Console.WriteLine(locations[0]);
			BinaryReader reader = new BinaryReader(File.OpenRead(@"F:\Extracted\titanquest\MapDecompiler Ragnarok\world01.wrl"));
			reader.Seek(183332);
			uint levelCount = reader.ReadUInt32();
			TQLevel[] levels = new TQLevel[levelCount];
			int minX = int.MaxValue; int minY = int.MaxValue;
			int maxX = int.MinValue; int maxY = int.MinValue;
			for (int i = 0; i < levelCount; i++) {
				levels[i] = new TQLevel(reader);

				bool hasLoc = false;
				for (int loc = 0; loc < locations.Length; loc++) if (levels[i].path.StartsWith(locations[loc])) hasLoc = true;
				if (!hasLoc) continue;

				if (levels[i].x < minX) minX = levels[i].x;
				if (levels[i].z < minY) minY = levels[i].z;
				if (levels[i].x + levels[i].sizeX > maxX) maxX = levels[i].x + levels[i].sizeX;
				if (levels[i].z + levels[i].sizeY > maxY) maxY = levels[i].z + levels[i].sizeY;
			}
			reader.Close();
			Console.WriteLine($"({(maxX - minX) * 2},{(maxY - minY) * 2})");


			MagickImage image = new MagickImage(MagickColors.Transparent, (maxX - minX) * 2, (maxY - minY) * 2);
			for (int i = 0; i < levelCount; i++) {

				bool hasLoc = false;
				for (int loc = 0; loc < locations.Length; loc++) if (levels[i].path.StartsWith(locations[loc])) hasLoc = true;
				if (!hasLoc) continue;

				Console.WriteLine(levels[i].path);
				string mappath = @"F:\Extracted\titanquest\MapDecompiler Ragnarok\" + levels[i].path.Substring(0, levels[i].path.Length - 3) + "tga";
				MagickImage map;
				if (File.Exists(mappath) && new FileInfo(mappath).Length > 0) {
					map = new MagickImage(mappath);
					map.Flip();
					image.Composite(map, (levels[i].x - minX) * 2, (levels[i].z - minY) * 2, CompositeOperator.Over);
				} //else map = new MagickImage(MagickColors.LightPink, levels[i].sizeX * 2, levels[i].sizeY * 2);
			}
			image.Write($"TQ_{locations[0].Replace('\\', '_')}.png");
		}

		struct TQLevel {

			public string path;
			public float[] bounds;
			public int x;
			public int y;
			public int z;
			public int sizeX;
			public int sizeY;

			public TQLevel(BinaryReader r) {
				path = new string(r.ReadChars(r.ReadInt32())).Replace('/', '\\');
				bounds = new float[6];
				for (int i = 0; i < 6; i++) bounds[i] = r.ReadSingle();
				x = r.ReadInt32();
				y = r.ReadInt32();
				z = r.ReadInt32();
				r.Seek(16);
				r.Seek(r.ReadInt32());
				sizeX = Convert.ToInt32(bounds[0]) + Convert.ToInt32(bounds[3]);
				sizeY = Convert.ToInt32(bounds[2]) + Convert.ToInt32(bounds[5]);
			}
		}
	}
}
