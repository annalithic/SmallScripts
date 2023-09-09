using System;
using System.Collections.Generic;
using System.IO;

namespace SmallScripts {
    class Starfield {

        struct System {
            public string name;
            public string[] bodies;
            public int[] parents;

            public System(string name) {
                this.name = name;
                this.bodies = new string[64];
                this.parents = new int[64];
            }

            public void WriteBodies(HashSet<string> lifePlanets, int indent, int parent) {
                for(int i = 0; i < bodies.Length; i++) {
                    if(parents[i] == parent && bodies[i] != null) {
                        if(lifePlanets.Contains(bodies[i])) {
                            lifePlanets.Remove(bodies[i]);
                            Console.WriteLine(new string(':', indent) + $" [[Sf:{bodies[i]}|{bodies[i]}]] [[File:SF-icon-life.svg|14px|sub]]");
                        } else 
                            Console.WriteLine(new string(':', indent) + $" [[Sf:{bodies[i]}|{bodies[i]}]]");
                        WriteBodies(lifePlanets, indent + 1, i);
                    }
                }
            }
        }

        public static void ListPlanets() {

            HashSet<string> lifePlanets = new HashSet<string>(File.ReadAllLines(@"F:\Extracted\Starfield\planetsLife.txt"));

            Dictionary<int, System> systems = new Dictionary<int, System>();
            Dictionary<string, int> systemNameAsdoihgd = new Dictionary<string, int>();

            foreach(string line in File.ReadAllLines(@"E:\Anna\Anna\Delphi\TES5Edit\Build\Edit Scripts\starIds.txt")) {
                string[] words = line.Split('|');
                if (words.Length != 2) continue;
                int starIndex = int.Parse(words[1]);
                systems[starIndex] = new System(words[0]);
                systemNameAsdoihgd[words[0]] = starIndex;
            }

            foreach (string line in File.ReadAllLines(@"E:\Anna\Anna\Delphi\TES5Edit\Build\Edit Scripts\planetIds.txt")) {
                string[] words = line.Split('|');
                if (words[1] != "Planet" && words[1] != "Moon") continue;
                if (words.Length <= 1) continue;
                int starIndex = int.Parse(words[2]);
                int planetIndex = int.Parse(words[4]);
                int parentIndex = int.Parse(words[3]);

                systems[starIndex].bodies[planetIndex] = $"{words[0]}";
                systems[starIndex].parents[planetIndex] = parentIndex;
            }



            List<string> systemNames = new List<string>(systemNameAsdoihgd.Keys);
            systemNames.Sort();

            foreach(string systemName in systemNames) {
                System sys = systems[systemNameAsdoihgd[systemName]];
                Console.WriteLine($"==[[Sf:{sys.name} System|{sys.name}]]==");
                sys.WriteBodies(lifePlanets, 1, 0);
                Console.WriteLine();
            }

            foreach(string planet in lifePlanets) {
                Console.WriteLine("MISSED " + planet);
            }
        }


    }
}
