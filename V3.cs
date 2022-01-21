using System;
using System.Collections.Generic;
using System.IO;

namespace SmallScripts {
    class V3 {
        static string[] interestGroups = { "Trade Unions", "Intelligentsia", "Industrialists", "Armed Forces", "Petty Bourgeoisie", "Landowners", "Devout", "Rural Folk" };
        static Dictionary<string, int[]> ideologies;
        static List<string> ideologyNames;
        static Random r;

        public static void RandomiseInterestGroups() {
            ideologies = new Dictionary<string, int[]>();
            ideologyNames = new List<string>();
            r = new Random();

            foreach (string line in File.ReadAllLines(@"F:\Extracted\Victoria3\ideologyrandom.txt")) {
                string[] words = line.Split('\t');
                ideologies[words[0]] = new int[8];
                ideologyNames.Add(words[0]);
                for (int i = 0; i < 8; i++) ideologies[words[0]][i] = int.Parse(words[i + 1]);
            }
            

            CreateRandomGroup(3);
            CreateRandomGroup(3);
            CreateRandomGroup(3);

            CreateRandomGroup(3);
        }

        static void CreateRandomGroup(int ideologyCount) {
            HashSet<string> newIdeologySet = new HashSet<string>();
            while (newIdeologySet.Count < ideologyCount) newIdeologySet.Add(ideologyNames[r.Next(ideologyNames.Count)]);
            int selectedIG = -1;
            int maxScore = int.MinValue;

            Console.WriteLine(GetIG(newIdeologySet));
            foreach (string ideology in newIdeologySet)
                Console.WriteLine(" " + ideology);
            Console.WriteLine();
        }

        static string GetIG(HashSet<string> igIdeologies) {
            int[] scores = new int[8];
            foreach(string ideology in igIdeologies) {
                for (int i = 0; i < 8; i++) scores[i] += ideologies[ideology][i];
            }
            Random r = new Random();
            int startIG = r.Next(8);

            //for (int i = 0; i < 8; i++) Console.WriteLine($"{interestGroups[i]} {scores[i]}");
            //Console.WriteLine();
            int returnIG = startIG;
            int y = startIG;

            do {
                y = (y+1) % 8;
                if (scores[y] > scores[returnIG]) returnIG = y;
            } while (y != startIG);

            return interestGroups[returnIG];
        }
        
    }
}
