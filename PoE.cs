using System;
using System.IO;
using Util;

namespace SmallScripts {
	static class PoE {

		static void SMDToObj(string inPath) {
			using(BinaryReader r = new BinaryReader(File.Open(inPath, FileMode.Open))) {
				SMD smd = new SMD(r);
			}
		}
		static void POEMonsterTypes() {
			string[] lines = File.ReadAllLines(@"F:\Extracted\PathOfExile\3.15.Expedition\MonsterTypes.csv");
			string[] monsterTypes = new string[lines.Length - 1];
			for (int i = 0; i < monsterTypes.Length; i++) monsterTypes[i] = lines[i + 1].Substring(0, lines[i + 1].IndexOf(','));

			lines = File.ReadAllLines(@"F:\Extracted\PathOfExile\3.15.Expedition\MonsterVarieties.csv");
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
