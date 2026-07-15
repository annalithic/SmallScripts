using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ImageMagick;
using Newtonsoft.Json.Linq;

namespace SmallScripts {
    internal class TES3Lod {
        //the tech keeps changing!
        static string[] lodLevelTextures = { "sml.dds", "mid.dds", "far.dds" };
        static string[] lodLevelFolders = {
            @"C:\Games\MorrowindMods\lodrocksmall\meshes",
            @"C:\Games\MorrowindMods\lodrockmid\meshes",
            @"C:\Games\MorrowindMods\lodrockfar\meshes",
            @"C:\Games\MorrowindMods\lodtreesmall\meshes",
            @"C:\Games\MorrowindMods\lodtreemid\meshes",
            @"C:\Games\MorrowindMods\lodtreefar\meshes",
            @"C:\Games\MorrowindMods\lodbuildsmall\meshes",
            @"C:\Games\MorrowindMods\lodbuildmid\meshes",
            @"C:\Games\MorrowindMods\lodbuildfar\meshes",

        };

        class ModelStats {
            public string model;
            public string sharedModel;
            public string type;
            public string dist;
            public string category;
            public string comment;
            public bool tr;

            public int tris;
            public int trisDist;

            public int count;
            public int countTR;

            public Dictionary<string, int> formCounts;
            public Dictionary<string, int> formCountsTR;
        }

        public static void TES3StaticList3(params string[] espPaths) {



            HashSet<string> staticFormTypes = new HashSet<string> { "Static", "Activator", "Container", "Door", };

            var models = new Dictionary<string, ModelStats>();

            foreach (string line in File.ReadAllLines(@"E:\Extracted\Morrowind\mwmesh.txt")) {
                var words = line.Split('\t');
                ModelStats stats = new ModelStats();
                stats.model = words[2].Replace('\\', '/');
                stats.sharedModel = words[0];
                stats.type = words[4];
                stats.dist = words[5];
                stats.category = words[6];
                stats.comment = words[18];
                models[stats.model] = stats;
            }

            foreach (string line in File.ReadAllLines(@"E:\Projects\PythonScripts\meshstats3.txt")) {
                string[] words = line.Split('|');
                string modelPath = words[0].ToLower();
                string model = modelPath.Substring(modelPath.IndexOf("\\meshes\\") + "\\meshes\\".Length).Replace('\\', '/');
                model = model.Replace("_dist.nif", ".nif");
                bool dist = modelPath.Contains("morrowindmods\\lod");

                if (!models.ContainsKey(model)) models[model] = new ModelStats() { model = model };
                var stats = models[model];
                if (dist)
                    stats.trisDist = int.Parse(words[1]);
                else {
                    stats.tris = int.Parse(words[1]);
                    stats.trisDist = stats.tris;
                    stats.tr = !modelPath.Contains("morrowind\\vanilla");
                }
            }


            Dictionary<string, string> idModels = new Dictionary<string, string>();
            Console.WriteLine("STATS....");
            foreach (string espPath in espPaths) {
                Console.WriteLine(espPath);
                JArray esp = JArray.Parse(File.ReadAllText(espPath));
                for (int i = 0; i < esp.Count; i++) {
                    var entry = esp[i];
                    if (entry["type"] != null && staticFormTypes.Contains(entry.Str("type")) && entry["id"] != null && entry["mesh"] != null) {
                        string id = entry.Str("id");
                        string mesh = entry.Str("mesh").ToLower().Replace("\\", "/");
                        if (!models.ContainsKey(mesh)) {
                            Console.WriteLine($"missing mesh stats for {mesh}");
                            continue;
                        }
                        var modelStats = models[mesh];
                        if (modelStats.formCounts == null) {
                            modelStats.formCounts = new Dictionary<string, int>();
                            modelStats.formCountsTR = new Dictionary<string, int>();
                        }
                        modelStats.formCounts[id] = 0;
                        modelStats.formCountsTR[id] = 0;
                        idModels[id] = mesh;
                    }
                }
            }

            Console.WriteLine("REFS....");
            for (int espIdx = 0; espIdx < espPaths.Length; espIdx++) {
                string espPath = espPaths[espIdx];
                Console.WriteLine(espPath);

                string espFilename = Path.GetFileName(espPath);
                bool isVanilla = espFilename == "Morrowind.json" || espFilename == "Tribunal.json" || espFilename == "Bloodmoon.json";

                JArray esp = JArray.Parse(File.ReadAllText(espPath));
                for (int i = 0; i < esp.Count; i++) {
                    if (esp[i]["type"] != null && esp[i]["type"].Value<string>() == "Cell") {
                        if (esp[i]["data"]["flags"].Value<string>().IndexOf("IS_INTERIOR") != -1) continue; //interior
                        JArray refs = (JArray)esp[i]["references"];
                        for (int refNum = 0; refNum < refs.Count; refNum++) {
                            string id = refs[refNum]["id"].Value<string>();
                            if (!idModels.ContainsKey(id)) continue;
                            var modelStats = models[idModels[id]];
                            if (isVanilla) modelStats.formCounts[id]++;
                            else modelStats.formCountsTR[id]++;
                        }
                    }
                }
            }

            Console.WriteLine("\r\n\r\n");

            foreach (var stats in models.Values) {
                if (stats.formCounts == null) continue;

                int meshCount = 0;
                int trMeshCount = 0;

                string modalForm = "";
                int modalCount = -1;
                foreach (string id in stats.formCounts.Keys) {
                    int count = stats.formCounts[id];
                    int trCount = stats.formCountsTR[id];
                    if (count + trCount > modalCount) {
                        modalCount = count + trCount;
                        modalForm = id;
                    }
                    meshCount += count;
                    trMeshCount += trCount;
                }

                if (meshCount == 0 && trMeshCount == 0) continue;

                string tr = stats.tr ? "tr" : "mw";


                Console.WriteLine($"{stats.sharedModel}|{modalForm}|{stats.model}|{tr}|{stats.category}|{stats.type}|{stats.dist}|{meshCount}|{trMeshCount}|{stats.tris}|{stats.trisDist}|{stats.comment}");
            }
        }

        public static void MeshTextures(string set) {

            HashSet<string> meshes = new HashSet<string>();
            Dictionary<string, int> texCounts = new Dictionary<string, int>();
            Dictionary<string, List<string>> texMeshes = new Dictionary<string, List<string>>();


            foreach (string line in File.ReadAllLines(@"F:\Extracted\Morrowind\lodmeshes3.txt")) {
                string[] split = line.Split('\t');
                if (split.Length < 8) {
                    continue;
                }

                if (split[10] != "mw") {
                    continue; //temp
                }

                if (split[11] != set) continue;

                meshes.Add(split[1]);
            }

            //foreach (string mesh in meshes) Console.WriteLine(mesh);

            foreach (string line in File.ReadAllLines(@"E:\Anna\Anna\Visual Studio\PythonScripts\texnames.txt")) {
                string[] split = line.Split('|');
                if (meshes.Contains(split[0])) {
                    for (int i = 1; i < split.Length; i++) {
                        string tex = split[i];
                        if (tex.Length == 0) continue;
                        int slash = tex.LastIndexOf('\\'); if (slash != -1) tex = tex.Substring(slash + 1);
                        tex = tex.ToLower();
                        tex = tex.Substring(0, tex.Length - 4);
                        if (!texCounts.ContainsKey(tex)) {
                            texCounts[tex] = 0;
                            texMeshes[tex] = new List<string>();
                        }
                        texCounts[tex]++;
                        texMeshes[tex].Add(split[0]);
                    }
                }
            }

            foreach (string tex in texCounts.Keys) {
                Console.Write($"{tex};{texCounts[tex]}");
                for (int i = 0; i < texMeshes[tex].Count; i++) Console.Write(";" + texMeshes[tex][i]);
                Console.WriteLine();
            }

        }
        public static void LodMeshes3() {



            Dictionary<string, int> meshLodLevels = new Dictionary<string, int>();

            foreach (string line in File.ReadAllLines(@"F:\Extracted\Morrowind\lodmeshes4.txt")) {
                string[] split = line.Split('\t');
                if (split.Length < 6) {
                    continue;
                }

                //if (split[10] != "mw") {
                //Console.WriteLine(split[10]);
                //continue; //temp
                //}

                int folderOffset = -1;

                string type = split[4];
                if (type == "rock") folderOffset = 0;
                else if (type == "tree") folderOffset = 3;
                else if (type == "build") folderOffset = 6;
                if (folderOffset == -1) continue;


                string mesh = split[2];

                string levelStr = split[5];
                int level = folderOffset; if (levelStr == "mid") level += 1; else if (levelStr == "far") level += 2;

                if (!meshLodLevels.ContainsKey(mesh) || level > meshLodLevels[mesh]) meshLodLevels[mesh] = level;
            }

            Console.WriteLine("PARSED");

            foreach (string mesh in meshLodLevels.Keys) {
                bool convert = true;
                int lodLevel = meshLodLevels[mesh];

                string destSuffix = mesh.Substring(0, mesh.Length - 4) + "_dist.nif";
                for (int i = 0; i < lodLevelFolders.Length; i++) {
                    string path = Path.Combine(lodLevelFolders[i], destSuffix);
                    if (File.Exists(path)) {
                        if (i == lodLevel) {
                            convert = false;
                        } else {
                            Console.WriteLine("DELETING FILE " + mesh);
                            File.Delete(path);
                        }
                    }
                }
                if (!convert) continue;

                string dest = Path.Combine(lodLevelFolders[lodLevel], destSuffix);

                string file = Path.Combine(@"C:\Games\MorrowindMods\Morrowind Optimization Patch\00 Core\meshes", mesh);
                if (!File.Exists(file)) file = Path.Combine(@"C:\Games\MorrowindMods\TD_Addon\00 Data Files\meshes", mesh);
                if (!File.Exists(file)) file = Path.Combine(@"C:\Games\MorrowindMods\Tamriel Data\meshes", mesh);
                if (!File.Exists(file)) file = Path.Combine(@"E:\Extracted\Morrowind\VANILLA\meshes", mesh);
                if (!File.Exists(file)) {
                    Console.WriteLine("MISSING FILE    " + mesh);
                    continue;
                }
                //if (!File.Exists(dest)) {
                Console.WriteLine(dest);
                if (!Directory.Exists(Path.GetDirectoryName(dest))) Directory.CreateDirectory(Path.GetDirectoryName(dest));
                //File.Copy(file, dest);
                Process process = new Process();
                process.StartInfo.FileName = @"E:\Programs\Python 3.12\python.exe";
                process.StartInfo.Arguments = $@" ""E:\Projects\PythonScripts\texreplace.py"" ""{file}"" ""{dest}"" ""{lodLevelTextures[lodLevel % 3]}""";
                process.StartInfo.CreateNoWindow = true;
                process.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
                process.Start();
                process.WaitForExit();// Waits here for the process to exit.

                //}

            }
        }

        public static void LodMeshes2() {

            foreach (string line in File.ReadAllLines(@"F:\Extracted\Morrowind\lodmeshes2.txt")) {
                string[] split = line.Split('\t');
                if (split.Length < 5) {
                    continue;
                }
                if (split[3] != "rock" || !(split[4] == "far" || split[4] == "mid")) continue;

                string mesh = split[1];

                string file = Path.Combine(@"C:\Games\MorrowindMods\Morrowind Optimization Patch\00 Core\meshes", mesh);
                if (!File.Exists(file)) file = Path.Combine(@"C:\Games\MorrowindMods\TD_Addon\01 TR BSA\meshes", mesh);
                if (!File.Exists(file)) file = Path.Combine(@"E:\Extracted\Morrowind\TR\meshes", mesh);
                if (!File.Exists(file)) file = Path.Combine(@"E:\Extracted\Morrowind\VANILLA\meshes", mesh);
                if (!File.Exists(file)) {
                    Console.WriteLine("MISSING FILE    " + mesh);
                    continue;
                }
                string dest = Path.Combine(@"C:\Games\MorrowindMods\lodtest\meshes", mesh.Substring(0, mesh.Length - 4) + "_dist.nif");
                if (!File.Exists(dest)) {
                    Console.WriteLine(dest);
                    if (!Directory.Exists(Path.GetDirectoryName(dest))) Directory.CreateDirectory(Path.GetDirectoryName(dest));
                    File.Copy(file, dest);
                }

            }
        }


        public static void LodMeshes() {
            HashSet<string> lines = new HashSet<string>();
            foreach (string line in File.ReadAllLines(@"F:\Extracted\Morrowind\lodmeshes.txt")) {
                if (line.Length > 0) lines.Add(line);
            }

            foreach (string line in lines) {
                string file = Path.Combine(@"E:\Games\MorrowindMods\Morrowind Optimization Patch\00 Core\meshes", line);
                if (!File.Exists(file)) file = Path.Combine(@"E:\Games\MorrowindMods\TD_Addon\01 TR BSA\meshes", line);
                if (!File.Exists(file)) file = Path.Combine(@"E:\Extracted\Morrowind\TR\meshes", line);
                if (!File.Exists(file)) file = Path.Combine(@"E:\Extracted\Morrowind\VANILLA\meshes", line);
                if (!File.Exists(file)) {
                    Console.WriteLine("MISSING FILE    " + line);
                    continue;
                }
                string dest = Path.Combine(@"E:\Games\MorrowindMods\lodtest\meshes", line.Substring(0, line.Length - 4) + "_dist.nif");
                if (!File.Exists(dest)) {
                    Console.WriteLine(dest);
                    if (!Directory.Exists(Path.GetDirectoryName(dest))) Directory.CreateDirectory(Path.GetDirectoryName(dest));
                    File.Copy(file, dest);
                }

            }
        }

        public static void CreateBlankDistMeshes(string folder, string outfolder, string emptyMeshPath = @"E:\Extracted\Morrowind\empty.nif") {
            foreach (string path in Directory.EnumerateFiles(folder, "*.nif", SearchOption.AllDirectories)) {
                string pathRelative = Path.Combine(outfolder, path.Substring(folder.Length + 1));
                pathRelative = pathRelative.Substring(0, pathRelative.Length - 4) + "_dist.nif";
                if (!Directory.Exists(Path.GetDirectoryName(pathRelative))) Directory.CreateDirectory(Path.GetDirectoryName(pathRelative));
                //if(!File.Exists(pathRelative))
                File.Copy(emptyMeshPath, pathRelative, true);
            }
        }

        public static void TES3StaticList2(params string[] espPaths) {
            HashSet<string> staticFormTypes = new HashSet<string> { "Static", "Activator", "Container", "Door", };

            Dictionary<string, string> meshLodData = new Dictionary<string, string>();
            foreach (string line in File.ReadAllLines(@"F:\Extracted\Morrowind\lodmeshes4.txt")) {
                int idx = line.IndexOf('\t');
                string mesh = line.Substring(0, idx);
                string data = line.Substring(idx + 1);
                if (data.Length > 0) meshLodData[mesh] = data.Replace('\t', '|');
            }


            Dictionary<string, int> meshTris = new Dictionary<string, int>();
            Dictionary<string, int> meshSizes = new Dictionary<string, int>();

            foreach (string line in File.ReadAllLines(@"E:\A\A\Visual Studio\PythonScripts\meshstats2.txt")) {
                string[] words = line.Split('|');
                string mesh = words[0].ToLower();
                meshTris[mesh] = int.Parse(words[1]);
                meshSizes[mesh] = (int)Math.Sqrt(double.Parse(words[3]));
            }

            Dictionary<string, List<string>> meshForms = new Dictionary<string, List<string>>();
            Dictionary<string, int> formCounts = new Dictionary<string, int>();
            Dictionary<string, int> formCountsTR = new Dictionary<string, int>();


            Console.WriteLine("STATS....");
            foreach (string espPath in espPaths) {
                Console.WriteLine(espPath);
                JArray esp = JArray.Parse(File.ReadAllText(espPath));
                for (int i = 0; i < esp.Count; i++) {
                    var entry = esp[i];
                    if (entry["type"] != null && staticFormTypes.Contains(entry.Str("type")) && entry["id"] != null && entry["mesh"] != null) {
                        string id = entry.Str("id");
                        string mesh = entry.Str("mesh").ToLower();
                        if (!meshForms.ContainsKey(mesh)) meshForms[mesh] = new List<string>();
                        meshForms[mesh].Add(id);
                        formCounts[id] = 0;
                        formCountsTR[id] = 0;
                    }
                }
            }

            Console.WriteLine("REFS....");
            for (int espIdx = 0; espIdx < espPaths.Length; espIdx++) {
                string espPath = espPaths[espIdx];
                Console.WriteLine(espPath);
                JArray esp = JArray.Parse(File.ReadAllText(espPath));
                for (int i = 0; i < esp.Count; i++) {
                    if (esp[i]["type"] != null && esp[i]["type"].Value<string>() == "Cell") {
                        if ((esp[i]["data"]["flags"].Value<int>() & 1) == 1) continue; //interior
                        JArray refs = (JArray)esp[i]["references"];
                        for (int refNum = 0; refNum < refs.Count; refNum++) {
                            string id = refs[refNum]["id"].Value<string>();
                            if (!formCounts.ContainsKey(id)) continue;
                            if (espIdx > 0) formCountsTR[id]++;
                            else formCounts[id]++;
                        }
                    }
                }
            }

            Console.WriteLine("\r\n\r\n");

            foreach (string mesh in meshForms.Keys) {
                int meshCount = 0;
                int trMeshCount = 0;

                string modalForm = "";
                int modalCount = -1;
                for (int i = 0; i < meshForms[mesh].Count; i++) {
                    string form = meshForms[mesh][i];
                    int formCount = formCounts[form];
                    if (formCount > modalCount) {
                        modalCount = formCount;
                        modalForm = form;
                    }
                    meshCount += formCount;
                }
                for (int i = 0; i < meshForms[mesh].Count; i++) {
                    string form = meshForms[mesh][i];
                    int formCount = formCountsTR[form];
                    if (formCount > modalCount) {
                        modalCount = formCount;
                        modalForm = form;
                    }
                    trMeshCount += formCount;
                }
                if (meshCount == 0 && trMeshCount == 0) continue;
                string lodData = meshLodData.ContainsKey(mesh) ? meshLodData[mesh] : "";
                int triCount = meshTris.ContainsKey(mesh) ? meshTris[mesh] : 0;
                int meshSize = meshSizes.ContainsKey(mesh) ? meshSizes[mesh] : 0;

                Console.WriteLine($"{modalForm}|{mesh}|{meshCount}|{trMeshCount}|{triCount}|{lodData}".TrimEnd('|'));
            }
        }


        public static void TES3StaticList(params string[] espPaths) {
            HashSet<string> staticFormTypes = new HashSet<string> { "Static", "Activator", "Container", "Door", };

            Dictionary<string, string> meshLodData = new Dictionary<string, string>();
            foreach (string line in File.ReadAllLines(@"F:\Extracted\Morrowind\lodmeshes3.txt")) {
                StringBuilder lodData = new StringBuilder();
                string[] split = line.Split('\t');
                string mesh = split[1];
                if (split.Length > 6) lodData.Append(split[6] + "|");
                if (split.Length > 7) lodData.Append(split[7] + "|");
                if (split.Length > 8) lodData.Append(split[8] + "|");
                if (split.Length > 9) lodData.Append(split[9] + "|");
                if (lodData.Length > 0) meshLodData[mesh] = lodData.ToString();
            }


            Dictionary<string, int> meshTris = new Dictionary<string, int>();
            Dictionary<string, int> meshSizes = new Dictionary<string, int>();

            foreach (string line in File.ReadAllLines(@"E:\Anna\Anna\Visual Studio\PythonScripts\meshstats.txt")) {
                string[] words = line.Split('|');
                string mesh = words[0].ToLower();
                meshTris[mesh] = int.Parse(words[1]);
                meshSizes[mesh] = (int)Math.Sqrt(double.Parse(words[3]));
            }

            Dictionary<string, List<string>> meshForms = new Dictionary<string, List<string>>();
            Dictionary<string, int> formCounts = new Dictionary<string, int>();
            Dictionary<string, int> formCountsTR = new Dictionary<string, int>();


            Console.WriteLine("STATS....");
            foreach (string espPath in espPaths) {
                Console.WriteLine(espPath);
                JArray esp = JArray.Parse(File.ReadAllText(espPath));
                for (int i = 0; i < esp.Count; i++) {
                    var entry = esp[i];
                    if (entry["type"] != null && staticFormTypes.Contains(entry.Str("type")) && entry["id"] != null && entry["mesh"] != null) {
                        string id = entry.Str("id");
                        string mesh = entry.Str("mesh").ToLower();
                        if (!meshForms.ContainsKey(mesh)) meshForms[mesh] = new List<string>();
                        meshForms[mesh].Add(id);
                        formCounts[id] = 0;
                        formCountsTR[id] = 0;
                    }
                }
            }

            Console.WriteLine("REFS....");
            foreach (string espPath in espPaths) {
                bool tr = !(Path.GetFileNameWithoutExtension(espPath) == "morrowind" || Path.GetFileNameWithoutExtension(espPath) == "bloodmoon");
                Console.WriteLine(espPath);
                JArray esp = JArray.Parse(File.ReadAllText(espPath));
                for (int i = 0; i < esp.Count; i++) {
                    if (esp[i]["type"] != null && esp[i]["type"].Value<string>() == "Cell") {
                        if ((esp[i]["data"]["flags"].Value<int>() & 1) == 1) continue; //interior
                        JArray refs = (JArray)esp[i]["references"];
                        for (int refNum = 0; refNum < refs.Count; refNum++) {
                            string id = refs[refNum]["id"].Value<string>();
                            if (!formCounts.ContainsKey(id)) continue;
                            if (tr) formCountsTR[id]++;
                            else formCounts[id]++;
                        }
                    }
                }
            }

            Console.WriteLine("\r\n\r\n");

            foreach (string mesh in meshForms.Keys) {
                int meshCount = 0;
                int trMeshCount = 0;

                string modalForm = "";
                int modalCount = -1;
                for (int i = 0; i < meshForms[mesh].Count; i++) {
                    string form = meshForms[mesh][i];
                    int formCount = formCounts[form];
                    if (formCount > modalCount) {
                        modalCount = formCount;
                        modalForm = form;
                    }
                    meshCount += formCount;
                }
                for (int i = 0; i < meshForms[mesh].Count; i++) {
                    string form = meshForms[mesh][i];
                    int formCount = formCountsTR[form];
                    if (formCount > modalCount) {
                        modalCount = formCount;
                        modalForm = form;
                    }
                    trMeshCount += formCount;
                }
                if (meshCount == 0 && trMeshCount == 0) continue;
                string lodData = meshLodData.ContainsKey(mesh) ? meshLodData[mesh] : "";
                int triCount = meshTris.ContainsKey(mesh) ? meshTris[mesh] : 0;
                int meshSize = meshSizes.ContainsKey(mesh) ? meshSizes[mesh] : 0;

                Console.WriteLine($"{modalForm}|{mesh}|{meshCount}|{trMeshCount}|{meshCount * triCount}|{trMeshCount * triCount}|{triCount}|{lodData}".TrimEnd('|'));
            }
        }

  

    }
}
