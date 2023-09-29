using ImageMagick;
using System;
using System.Collections.Generic;
using System.IO;

namespace SmallScripts {
    class Starfield {

        public static void TestDensityMaps() {
            foreach(string path in Directory.EnumerateFiles(@"F:\Extracted\Starfield\DensityMaps", "*.dds", SearchOption.AllDirectories)) {
                string filename = Path.GetFileName(path);
                if (!filename.Contains("1k")) continue;
                string replacement = null;
                if (filename.Contains("_base")) replacement = @"F:\Extracted\Starfield\black.dds";
                else if (filename.Contains("_flatinner")) replacement = @"F:\Extracted\Starfield\black.dds";
                else if (filename.Contains("_flatouter")) replacement = @"F:\Extracted\Starfield\black.dds";
                else if (filename.Contains("_talus")) replacement = @"F:\Extracted\Starfield\black.dds";
                else if (filename.Contains("_flow")) replacement = @"F:\Extracted\Starfield\black.dds";
                //solid also exists but is barely used.

                if (replacement == null) continue;
                Console.WriteLine(path);

                string dest = path.Replace(@"F:\Extracted\Starfield\DensityMaps", @"C:\Anna\Documents\My Games\Starfield\Data");
                if(!Directory.Exists(Path.GetDirectoryName(dest))) Directory.CreateDirectory(Path.GetDirectoryName(dest));
                File.Copy(replacement, dest, true);

            }
        }
        public static void ListAnimalBiomes() {

            Dictionary<string, string> dictB = new Dictionary<string, string>();
            foreach(string line in File.ReadAllLines(@"E:\Anna\Anna\Delphi\TES5Edit\Build\Edit Scripts\aaaCCT.txt")) {
                string edid = line.Substring(0, line.IndexOf('|'));
                dictB[edid] = line;
            }


            Dictionary<string, List<string>> animals = new Dictionary<string, List<string>>();
            Dictionary<string, string> planets = new Dictionary<string, string>();

            foreach (string line in File.ReadAllLines(@"E:\Anna\Anna\Delphi\TES5Edit\Build\Edit Scripts\aaaplanetcreatures.txt")) {
                string[] words = line.Split('|');
                string key = words[2];
                if (!animals.ContainsKey(key)) {
                    animals[key] = new List<string>();
                    planets[key] = words[0];
                }
                animals[key].Add(words[1]);
            }
            foreach(string key in animals.Keys) {
                if (!dictB.ContainsKey(key)) continue;
                animals[key].Sort();
                Console.Write(key + '|' + planets[key] + "|Biomes: ");
                for (int i = 0; i < animals[key].Count - 1; i++) Console.Write(animals[key][i] + ", ");
                Console.Write(animals[key][animals[key].Count - 1] + '|');
                Console.WriteLine(dictB[key]);
            }
        }

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
                        } 
                            //Console.WriteLine(new string(':', indent) + $" [[Sf:{bodies[i]}|{bodies[i]}]]");
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
