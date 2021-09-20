using System;
using System.IO;

namespace SmallScripts {
	static class PoE {
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
}
