using ImageMagick;
using ImageMagick.Formats;
using MW;
using Newtonsoft.Json.Linq;
using SmallScripts;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Util;
using System.Diagnostics;
using System.Text;
using System.Runtime.CompilerServices;

namespace MW {
	struct QuestStage {
		public string id;
		public int stage;
	}
    struct QuestReq {
        public string id;
		public string comparison;
        public int stage;
    }

    class Info {
		public int choice;
		string name;
		public List<int> choiceNumbers;
		public List<Info> choices;
		public List<QuestStage> quests;
        public List<QuestReq> questReqs;

        public string playerFaction;
        public int playerRank;

        public string topic;
        public string speaker;

		public string miscFilters;

        static Dictionary<string, string> filterComparison = new Dictionary<string, string>() {
                { "Equal", "=" },
                { "Less", "<" },
                { "LessEqual", "<=" },
                { "Greater", ">" },
                { "GreaterEqual", ">=" },
                { "NotEqual", "!=" }
        };

        public Info(string topic, JToken info) {
			this.topic = topic;
			choice = -1;
			quests = new List<QuestStage>();
			questReqs = new List<QuestReq>();
            choiceNumbers = new List<int>();
			choices = new List<Info>();

			//if (rec["result"] == null) continue;
			if (info["result"] != null) {
                string result = info.Str("result");
                foreach (string line in result.Split('\n')) {
					int commentIndex = line.IndexOf(';');
                    string lineNoComment = commentIndex == -1 ? line : line.Substring(0, commentIndex);

                    string[] split = lineNoComment.SplitQuotes();
                    for (int word = 0; word < split.Length; word++) {
                        if (split[word] == "Journal") {
                            string quest = split[word + 1].Trim('"');
                            int stage = int.Parse(split[word + 2]);
							quests.Add(new QuestStage { id = quest, stage = stage });
                            word += 2;
                        } else if (split[word] == "Choice") {
							for(int splitNum = word + 2; splitNum < split.Length; splitNum += 2) {
								if (split[splitNum - 1] == ":") splitNum++; //AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA
								choiceNumbers.Add(int.Parse(split[splitNum]));
							}
							break;
						}
                    }
				}
            }




            playerFaction = info["player_faction"] == null ? "" : info.Str("player_faction");
            playerRank = info["data"]["player_rank"].Int();
			speaker = (info["speaker_id"] == null) ? "unknown speaker" : info.Str("speaker_id");



            miscFilters = "";
            foreach (var filter in info["filters"]) {
                string filter_type = filter.Str("filter_type");
                string filter_function = filter.Str("filter_function");
                string filter_id = filter.Str("id");
                string filter_comparison = filterComparison[filter.Str("filter_comparison")];
                string filter_value = filter["value"]["Integer"] != null ? filter["value"]["Integer"].Int().ToString() : filter["value"]["Float"].Float().ToString();

                if (filter_type == "Journal") {
					questReqs.Add(new QuestReq { id = filter_id, comparison = filter_comparison, stage = filter["value"]["Integer"].Int() });
                } else if (filter_function == "Choice") {
					choice = filter["value"]["Integer"].Int();
                }
                miscFilters = miscFilters + $"{filter_type} {filter_function} {filter_id} {filter_comparison} {filter_value}|";
            }
        }
	}
}

namespace SmallScripts {
	static class TES3 {

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

                if(split[11] != set) continue;

				meshes.Add(split[1]);
            }

			//foreach (string mesh in meshes) Console.WriteLine(mesh);

			foreach (string line in File.ReadAllLines(@"E:\Anna\Anna\Visual Studio\PythonScripts\texnames.txt")) {
                string[] split = line.Split('|');
				if (meshes.Contains(split[0])) {
                    for (int i = 1; i < split.Length; i++) {
						string tex = split[i];
						if (tex.Length == 0) continue;
						int slash = tex.LastIndexOf('\\'); if(slash != -1) tex = tex.Substring(slash + 1);
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

			foreach(string tex in texCounts.Keys) {
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

				if(!meshLodLevels.ContainsKey(mesh) || level > meshLodLevels[mesh]) meshLodLevels[mesh] = level;
            }

			Console.WriteLine("PARSED");

			foreach(string mesh in meshLodLevels.Keys) {
				bool convert = true;
				int lodLevel = meshLodLevels[mesh];

                string destSuffix = mesh.Substring(0, mesh.Length - 4) + "_dist.nif";
				for(int i = 0; i < lodLevelFolders.Length; i++) {
					string path = Path.Combine(lodLevelFolders[i], destSuffix);
                    if (File.Exists(path)) {
						if(i == lodLevel) {
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
                process.StartInfo.FileName = @"E:\Programs\Python3.9\python.exe";
                process.StartInfo.Arguments = $@" ""E:\A\A\Visual Studio\PythonScripts\texreplace.py"" ""{file}"" ""{dest}"" ""{lodLevelTextures[lodLevel % 3]}""";
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
				if(line.Length > 0) lines.Add(line);
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
				if(!File.Exists(dest)) {
					Console.WriteLine(dest);
					if (!Directory.Exists(Path.GetDirectoryName(dest))) Directory.CreateDirectory(Path.GetDirectoryName(dest));
                    File.Copy(file, dest);
                }

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
                    if (entry["type"] != null && staticFormTypes.Contains(entry.Str("type")) && entry["id"] != null && entry["mesh"] != null)  {
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
				for(int i = 0; i < meshForms[mesh].Count; i++) {
					string form = meshForms[mesh][i];
					int formCount = formCounts[form];
					if(formCount > modalCount) {
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

        public static void TES3QuestInfo(string espPath) {

			Dictionary<string, List<MW.Info>> questInfos = new Dictionary<string, List<MW.Info>>();
			Dictionary<string, string> questNames = new Dictionary<string, string>();



			string topic = "";

			HashSet<string> npcs = new HashSet<string>();
			Dictionary<string, string> npcCells = new Dictionary<string, string>();

			Dictionary<int, MW.Info> topicChoices = new Dictionary<int, MW.Info>();
			List<MW.Info> topicInfos = new List<MW.Info>();

			JArray esp = JArray.Parse(File.ReadAllText(espPath));

			bool inTopic = false;

			for (int i = 0; i < esp.Count; i++) {

				var rec = esp[i];

				if (rec["type"] == null) continue;
				string type = rec["type"].Value<string>();

                if (type == "Info") {
					inTopic = true;
                    if (rec["quest_name"] != null && rec["quest_name"].Int() == 1) {
                        questNames[topic] = rec.Str("text");
                    }


                    MW.Info info = new MW.Info(topic, rec);

                    if (info.choice == -1) {
                        //if the info sets a quest stage and doesn't require stages of the same quest, add it to the list of starters for that quest (should recursively check choices too)
                        foreach (MW.QuestStage quest in info.quests) {
                            bool starter = true;
                            foreach (MW.QuestReq req in info.questReqs) {
                                if (req.id == quest.id && !(req.comparison[0] == '<' || req.stage == 0)) { //if comparison is less than, evaluates true if don't have quest already
                                    starter = false; break;
                                }
                            }
                            if (starter) {
                                if (!questInfos.ContainsKey(quest.id)) questInfos[quest.id] = new List<MW.Info>();
                                questInfos[quest.id].Add(info);
                            }
                        }
                    } else {
                        topicChoices[info.choice] = info;
                    }
                } else {
					if(inTopic) { //ended list of topic infos
                        foreach (Info info in topicInfos) {
                            foreach (int choiceNum in info.choiceNumbers) {
								info.choices.Add(topicChoices[choiceNum]);
							}
                        }
                        topicInfos.Clear();
                        topicChoices.Clear();
                    }

                    if (type == "Npc") {
                        npcs.Add(rec["id"].Value<string>());

                    } else if (type == "Cell") {



                        string cellName = rec["id"].Value<string>();

                        if (cellName == "" && rec["region"] != null) cellName = rec["region"].Value<string>();
                        if ((rec["data"]["flags"].Value<int>() & 1) == 0) {
                            int cellX = esp[i]["data"]["grid"][0].Value<int>();
                            int cellY = esp[i]["data"]["grid"][1].Value<int>();
                            cellName = cellName + $" ({cellX},{cellY})";
                        }


                        foreach (var refr in (JArray)rec["references"]) {
                            string id = refr["id"].Value<string>();
                            if (npcs.Contains(id)) {
                                if (npcCells.ContainsKey(id)) {
                                    npcCells[id] = "Multiple Cells";
                                } else {
                                    npcCells[id] = cellName;
                                }

                            }
                        }

                    } else if (type == "Dialogue") {
                        topic = rec.Str("id");
                    }
                }

            }



            foreach (string quest in questInfos.Keys) {
                string questName = questNames.ContainsKey(quest) ? questNames[quest] : "";
				foreach(MW.Info info in questInfos[quest]) {
					foreach(MW.QuestStage questStage in info.quests) {
						if (questStage.id != quest) continue;

                        string location = npcCells.ContainsKey(info.speaker) ? npcCells[info.speaker] : "unknown location";

                        Console.WriteLine($"{quest}|{questName}|{questStage.stage}|{info.speaker}|{location}|{info.playerFaction}|{info.playerRank}|{info.topic}|{info.miscFilters}");
						break;
					}
				}
            }
        }

		public static void MWDoors(string espPath, float cellStartX, float cellStartY, float cellSizeX, float cellSizeY = 0) {
			if (cellSizeY == 0) cellSizeY = cellSizeX;
			float unitSizeX = cellSizeX * 8192;
            float unitSizeY = cellSizeY * 8192;
            float minX = cellStartX * 8192;
			float minY = cellStartY * 8192;
			float maxX = minX + unitSizeX;
			float maxY = minY + unitSizeY;

			JArray esp = JArray.Parse(File.ReadAllText(espPath));
			for (int i = 0; i < esp.Count; i++) {
				if (esp[i]["type"] != null && esp[i]["type"].Value<string>() == "Cell") {
					if ((esp[i]["data"]["flags"].Value<int>() & 1) == 1) continue; //interior
					JArray refs = (JArray)esp[i]["references"];
					for (int refNum = 0; refNum < refs.Count; refNum++) {
						if (refs[refNum]["door_destination_cell"] == null) continue;
						JArray coords = (JArray)refs[refNum]["translation"];
						float x = coords[0].Value<float>(); if (x < minX || x > maxX) continue;
						float y = coords[1].Value<float>(); if (y < minY || y > maxY) continue;
						string cellName = refs[refNum]["door_destination_cell"].Value<string>();
						int xPos = (int)((x - minX) * 1000 / unitSizeX);
						int yPos = 1000 - (int)((y - minY) * 1000 / unitSizeY);

						Console.WriteLine($"{{{{Image Mark|{xPos}|{yPos}|{cellName}|{cellName}|position=on}}}}");
					}
				}
			}
			Console.WriteLine("\r\n\r\n\r\n");
		}

		public static void TES3IntCellResizeTest() {
			foreach (string imagePath in Directory.EnumerateFiles(@"F:\Extracted\Morrowind\tombs", "*.bmp")) {
				MagickImage image = new MagickImage(imagePath);
				image.Resize(image.Width / 2, image.Height / 2);
				image.Trim(); image.RePage();
				image.Draw(new Drawables().FillColor(MagickColors.White).FontPointSize(24).Gravity(Gravity.Northwest).Text(4, 4, Path.GetFileName(imagePath).Split('.')[0]));
				image.Write(Path.Combine(@"F:\Extracted\Morrowind\tombsa", Path.GetFileNameWithoutExtension(imagePath) + ".png"));
				Console.WriteLine(imagePath);

			}
		}

		public static void TES3ListInts(string espPath) {
			JArray esp = JArray.Parse(File.ReadAllText(espPath));
			List<string> ints = new List<string>();

			for (int i = 0; i < esp.Count; i++) {
				if (esp[i]["type"] != null && esp[i]["type"].Value<string>() == "Cell" && ((esp[i]["data"]["flags"].Value<int>() & 1) == 1)) {
					ints.Add(esp[i]["id"].Value<string>());
				}
			}
			if (ints.Count == 0) return;

			int groupSize = 400;
			for(int start = 0; start < ints.Count; start += groupSize) {
				Console.WriteLine("if(i<1)");
				Console.WriteLine("coc,\"" + ints[start] + "\"");
				for (int i = 1; i < groupSize; i++) {
					if (i + start >= ints.Count) break;
					Console.WriteLine($"elseif(i<{i + 1})");
					Console.WriteLine("coc,\"" + ints[i + start] + "\"");
				}
				Console.WriteLine("\r\n\r\n\r\n\r\n\r\n\r\n\r\n\r\n\r\n\r\n");
			}

		}

		public static void TES3GridmapCoords() {
			MagickImage image = new MagickImage(MagickColors.Black, 40 * 128, 40 * 128);
			for (int x = 0; x < 128; x++) {
				Console.WriteLine(x);
				for (int y = 0; y < 128; y++) {
					image.Draw(new Drawables().FillColor(MagickColors.ForestGreen).Text(x * 40, 128 * 40 - y * 40 - 40, $"{x - 64},{y - 64}"));
				}
			}
			image.Write("gridmapcoords.png");
        }


		public static void MWRegionCreateMaps(string espPath) {
			int minX = int.MaxValue; int minY = int.MaxValue;
			int maxX = int.MinValue; int maxY = int.MinValue;

			JArray esp = JArray.Parse(File.ReadAllText(espPath));
			string[,] cellRegions = new string[256, 256];
			for (int i = 0; i < esp.Count; i++) {
				if (esp[i]["type"] != null && esp[i]["type"].Value<string>() == "Cell" && ((esp[i]["data"]["flags"].Value<int>() & 1) == 0)) {
					//string cellName = esp[i]["id"].Value<string>();
					var cellX = esp[i]["data"]["grid"][0].Value<int>();
					var cellY = esp[i]["data"]["grid"][1].Value<int>();
					string region = esp[i]["region"] is null ? "Wilderness" : esp[i]["region"].Value<string>();
					if(region != "Wilderness") {
						Console.WriteLine($"coe,{cellX},{cellY}");
						if (cellX > maxX) maxX = cellX;
						if (cellY > maxY) maxY = cellY;
						if (cellX < minX) minX = cellX;
						if (cellY < minY) minY = cellY;

					}
				}
			}

			Console.WriteLine();
			Console.WriteLine($"{minX},{minY} to {maxX},{maxY}");
			//foreach (string cell in cells) Console.WriteLine(cell);

		}

		public static void LocalMapCombine(string folder) {
			int minX = int.MaxValue; int maxX = int.MinValue;
			int minY = int.MaxValue; int maxY = int.MinValue;
			foreach(string file in Directory.EnumerateFiles(folder, "*.bmp")) {
				string coord = file.Substring(file.LastIndexOf('(') + 1);
				coord = coord.Substring(0, coord.IndexOf(')'));
				int x = int.Parse(coord.Substring(0, coord.IndexOf(',')));
				if(x > maxX) maxX = x; if (x < minX) minX = x;
				int y = int.Parse(coord.Substring(coord.IndexOf(',') + 1));
				if (y > maxY) maxY = y; if (y < minY) minY = y;
			}
			Console.WriteLine($"({minX},{minY}) to ({maxX},{maxY})");
			Console.WriteLine($"{(maxX - minX) * 512}x{(maxY - minY) * 512}");
			TES3LocalMapCombine(512, minX, minY, maxX, maxY);

        }

		public static void TES3LocalMapCombine(int tileSize, int x1, int y1, int x2, int y2) {
			MagickImage montage = new MagickImage(MagickColors.Black, tileSize * 8, tileSize * 8);
			int xCount = x2 - x1; int yCount = y2 - y1;

			for (int y = 0; y < xCount; y++) {
				Console.Write("-");
				for (int x = 0; x < yCount; x++) {
					string search = $@"F:\Anna\Desktop\maps3 - Copy\{x1 + x},{y1 + y}.png";
					if (File.Exists(search)) {
						MagickImage image = new MagickImage(search);
						montage.Draw(new Drawables().Composite(x * tileSize, (yCount - y) * tileSize, image));

					}
					//else Console.WriteLine(search);
				}
			}
			montage.Write($@"F:\Anna\Desktop\map.png");

		}

		public static void TES3LocalMapCombine() {

			int tileSize = 1024;

			for(int mapX = -2; mapX < 3; mapX++) {
				for (int mapY = -2; mapY < 4; mapY++) {
					int startY =  mapY * 8; int startX = mapX * 8;
					MagickImage montage = new MagickImage(MagickColors.Black, tileSize * 8, tileSize * 8);

					for (int y = 0; y < 8; y++) {
						Console.Write("-");
						for (int x = 0; x < 8; x++) {
							string search = $@"C:\Anna\Documents\My Games\OpenMW\maps\{startX + x},{startY + y}.bmp";
							if (File.Exists(search)) {
								MagickImage image = new MagickImage(search);
								montage.Draw(new Drawables().Composite(x * tileSize, (7 - y) * tileSize, image));

							}
							//else Console.WriteLine(search);
						}
					}
					Console.WriteLine();

					Console.WriteLine($"{mapX},{mapY}");
					montage.Write($@"F:\Anna\Desktop\maps1\{mapX},{mapY}.png");

                }
            }
        }

		public static void TES3WeatherValues() {
			string[] weatherTypes = new string[] { "Clear", "Cloudy", "Foggy", "Thunderstorm", "Rain", "Overcast", "Ashstorm", "Blight", "Snow", "Blizzard" };
			string[] tods = new string[] { "Sunrise", "Day", "Sunset", "Night" };
			string[] weatherValues = new string[] { "Sky", "Fog", "Ambient", "Sun" };

			foreach(string value in weatherValues) {
				Console.WriteLine($"    vec4 {value}Colors[{weatherTypes.Length * tods.Length}]=vec4[](");
				foreach(string weather in weatherTypes) {
					foreach(string tod in tods) {
						string search = $"Weather_{weather}_{value}_{tod}_Color";
						foreach (string line in File.ReadAllLines(@"F:\Extracted\Morrowind\weathercolors.txt")){
							if (line.StartsWith(search)) {
								var words = line.Split(',');
								float r = ((float)byte.Parse(words[1]))/ 255;
								float g = ((float)byte.Parse(words[2])) / 255;
								float b = ((float)byte.Parse(words[3])) / 255;
								Console.WriteLine($"        vec4({r},{g},{b},1),");
							}
						}

					}
				}
				Console.WriteLine("    );\r\n");
            }
		}


		public static void TES3WeatherColors() {
			string[] lines = File.ReadAllLines(@"F:\Extracted\Morrowind\weathercolors.txt");
			MagickImage image = new MagickImage(MagickColors.White, 512, lines.Length * 32);
			for(int i = 0; i < lines.Length; i++) {
				var words = lines[i].Split(',');
				byte r = byte.Parse(words[1]);
				byte g = byte.Parse(words[2]);
				byte b = byte.Parse(words[3]);
				MagickColor color = new MagickColor(r, g, b);

				image.Draw(new Drawables().FillColor(color).Rectangle(0, 32 * i, 512, 32 * i + 32));
				image.Draw(new Drawables().StrokeColor(MagickColors.Black).StrokeWidth(4).Text(4, 32 * i + 16, words[0]));
				image.Draw(new Drawables().FillColor(MagickColors.White).Text(4, 32 * i + 16, words[0]));

				Console.WriteLine(words[0]);
				//if (i > 10) break;
            }
			image.Write("MorrowindWeatherColors.png");

		}

		public static void FO3LodCombine() {
			MagickImageCollection lodimages = new MagickImageCollection();
			for(int y = 0; y < 32; y++) {
				for(int x = 31; x >= 0; x--) {
					string path = $@"F:\Extracted\BGS\Fallout3\textures\landscape\lod\wasteland\diffuse\wasteland.n.level4.x{x * 4 - 64}.y{y * 4 - 64}.dds";
					lodimages.Add(new MagickImage(path));
                }
            }
			var combined = lodimages.Montage(new MontageSettings() { Geometry = new MagickGeometry(256) });
			Console.WriteLine("Writing...");
			combined.Write("fo3.png");
			Console.WriteLine("Done.");
		}

		public static void FO76DepthMap(string path) {
			MagickImage image = new MagickImage(path);
			ushort[] data = new ushort[image.Width * image.Height];
			int i = 0;
			foreach (var pixel in image.GetPixels()) {
				var color = pixel.ToColor();
				int col = color.G + color.B / 256;
				//Console.WriteLine(col);
				data[i] = (ushort)col;
				i++;
            }
			byte[] byteData = new byte[data.Length * 2];
			Buffer.BlockCopy(data, 0, byteData, 0, byteData.Length);

			//MagickImage output = new MagickImage(byteData, new PixelReadSettings(image.Width, image.Height, StorageType.Short, "R"));

			//output.Format = MagickFormat.Gray;
			//output.Depth = 16;

			//output.Write(path.Replace(".dds", ".r16"));
        }
		/*
		public static void MWTesAnnwynColorMap() {
			Dictionary<MagickColor, MagickColor> colorMap = new Dictionary<MagickColor, MagickColor>();
			colorMap[MagickColors.Black] = new MagickColor(0, 0, 0);

			string[] lines = File.ReadAllLines(@"F:\Extracted\BethesdaGameStudioUtils\TESAnnwyn\tesannwyn-ltex3.dat");

			MagickImage defaultLand = new MagickImage(@"E:\Programs\Steam\steamapps\common\Morrowind\Data Files\textures\_land_default.tga");
			defaultLand.Resize(1, 1);
			MagickColor defaultColor = (MagickColor)defaultLand.GetPixels().GetPixel(0, 0).ToColor();
			colorMap[MagickColors.Black] = defaultColor;


			foreach (string line in lines) {
				string[] words = line.Split(',');
				if (words.Length < 4) continue;

				string texname = words[2].ToLower().Replace(".tga", ".dds");
				if (File.Exists(@"F:\Extracted\Morrowind\textures\" + texname)) {
					MagickImage image = new MagickImage(@"F:\Extracted\Morrowind\textures\" + texname);

					image.Resize(1, 1);
					ushort g = (ushort) (byte.Parse(words[0]) * 256);
					//g++;
					MagickColor c1 = new MagickColor(0, 0, 0);
					MagickColor c2 = (MagickColor)image.GetPixels().GetPixel(0, 0).ToColor();
					colorMap[c1] = c2;
					Console.WriteLine($"{c1.R} {c1.G} {c1.B} - {c2.R} {c2.G} {c2.B} - {texname}");
					//Console.WriteLine($"{words[0]} {texname} {color.R} {color.G} {color.B}");
				}
			}


			BinaryReader r = new BinaryReader(File.OpenRead(@"F:\Extracted\BethesdaGameStudioUtils\TESAnnwyn\tesannwyn-vtex3.bmp"));
			r.BaseStream.Seek(18, SeekOrigin.Begin);
			int width = r.ReadInt32();
			int height = r.ReadInt32();
			
			PixelReadSettings pixelRead = new PixelReadSettings(width, height, StorageType.Short, "R");

			r.BaseStream.Seek(54, SeekOrigin.Begin);
			byte[] data = r.ReadBytes(2 * width * height);

			for (byte i = 0; i < 107; i++) data[i * 2] = (byte) (i + 1);

			r.Close();
			MagickImage map = new MagickImage(data, pixelRead);
			map.Flip();
			map.Evaluate(Channels.All, EvaluateOperator.Multiply, 256);
			//map.Depth = 8;
			//map.ColorSpace = ColorSpace.RGB;
			//map.Format = MagickFormat.Rgb;

			Console.WriteLine(map.Depth);
			Console.WriteLine(map.Format);

			//map.Write(@"F:\Extracted\BethesdaGameStudioUtils\TESAnnwyn\tesannwyn-vtex3-recolor.png");
			//map.Depth = 8; map.Format = MagickFormat.Rgb;

			//MagickReadSettings colorReadSettings = new MagickReadSettings { Format = MagickFormat.Rgb, Depth = 16, Width = width, Height = height };
			MagickImage colorImage = new MagickImage(MagickColors.Red, width, height);
			colorImage.Composite(map);

			colorImage.ColorFuzz = new Percentage(0);
			foreach (var color in colorMap.Keys) {
				colorImage.Opaque(color, colorMap[color]);
			}
			colorImage.Write(@"F:\Extracted\BethesdaGameStudioUtils\TESAnnwyn\tesannwyn-vtex3-recolor.png");






		}
		*/

		public static void MWMapResize(int oldSize, int newSize) {
			foreach(string line in File.ReadAllLines(@"E:\Anna\Anna\Visual Studio\MWMap\index.html")) {
				if (!line.Contains("style=\"left:")) continue;
				var split = line.Split('"');
				var split2 = new string[7];
				split2[0] = split[0];
				split2[1] = split[1];
				split2[2] = split[4];
				split2[3] = split[5];
				split2[4] = split[2];
				split2[5] = split[3];
				split2[6] = split[6];
				//var split2 = split[5].Split(':', ';');
				//float left = float.Parse(split2[1]);
				//float top = float.Parse(split2[3]);
				//split[5] = $"left:{(int)(left + 0.5)};top:{(int)(top + 0.5)}";
				//split[5] = $"left:{left * newSize / oldSize};top:{top * newSize / oldSize}";
				for (int i = 0; i < split2.Length - 1; i++) Console.Write(split2[i] + "\"");
				Console.WriteLine(split2[split2.Length - 1]);
			}
        }


		public static void OpenMWMapCombine(string folder, int tileSize = 256) {

			int minX = int.MaxValue; int maxX = int.MinValue;
			int minY = int.MaxValue; int maxY = int.MinValue;


			foreach (string path in Directory.EnumerateFiles(folder, "*.bmp")) {
                string coord = path.Substring(path.LastIndexOf('(') + 1);
                coord = coord.Substring(0, coord.IndexOf(')'));
                int x = int.Parse(coord.Substring(0, coord.IndexOf(',')));
                if (x > maxX) maxX = x; if (x < minX) minX = x;
                int y = int.Parse(coord.Substring(coord.IndexOf(',') + 1));
                if (y > maxY) maxY = y; if (y < minY) minY = y;
                Console.Write($"{x} {y}, ");
			}
			Console.WriteLine();
			Console.WriteLine($"{minX},{minY} to {maxX},{maxY}");
			int xCount = maxX - minX + 1; int yCount = maxY - minY + 1;
			Console.WriteLine($"{xCount * tileSize}x{yCount * tileSize}");


			MagickImageCollection montage = new MagickImageCollection();
			for (int i = 0; i < xCount * yCount; i++) montage.Add(new MagickImage(MagickColors.Black, 1, 1));


			//MagickImage map = new MagickImage(MagickColors.Black, (maxX - minX + 1) * tileSize, (maxY - minY + 1) * tileSize);

			foreach (string path in Directory.EnumerateFiles(folder, "*.bmp")) {
                string coord = path.Substring(path.LastIndexOf('(') + 1);
                coord = coord.Substring(0, coord.IndexOf(')'));
                int x = int.Parse(coord.Substring(0, coord.IndexOf(',')));
                int y = int.Parse(coord.Substring(coord.IndexOf(',') + 1));
                int xOffset = x - minX;
				int yOffset = maxY - y;
				montage[xOffset + yOffset * xCount] = new MagickImage(path);

				//map.Draw(new Drawables().Composite((x - minX) * tileSize, map.Height - (y - minY) * tileSize - tileSize, image));
				//i++; if (i % 10 == 0) 
					Console.WriteLine(path);
			}

			MontageSettings montageSettings = new MontageSettings() { Geometry = new MagickGeometry(tileSize), TileGeometry = new MagickGeometry(xCount, yCount) };
			var map = montage.Montage(montageSettings);
			WebPWriteDefines write = new WebPWriteDefines() { Lossless = true, Method = 0 };
			//JxlWriteDefines write = new JxlWriteDefines() { Effort = 2 };
			map.Quality = 20;

			int imageCount = 0;
			while (File.Exists($"openmwmap_{imageCount}.webp")) imageCount++;

			map.Write($"openmwmap_{imageCount}.webp", write);
		}


		public static void MWMapCombine() {

			int minX = int.MaxValue; int maxX = int.MinValue;
			int minY = int.MaxValue; int maxY = int.MinValue;


			foreach (string path in Directory.EnumerateFiles(@"E:\Programs\Steam\steamapps\common\Morrowind\Maps", "*.bmp")) {
				string coord = Path.GetFileName(path);
				coord = coord.Substring(coord.IndexOf('(') + 1);
				var split = coord.Split(',', ')');
				int x = int.Parse(split[0]); if (x > maxX) maxX = x; if (x < minX) minX = x;
				int y = int.Parse(split[1]); if (y > maxY) maxY = y; if (y < minY) minY = y;
				Console.WriteLine($"{x} {y}");
            }

			Console.WriteLine($"{minX},{minY} to {maxX},{maxY}");

			

			MagickImage map = new MagickImage(MagickColors.Black, (maxX - minX) * 256, (maxY - minY) * 256);

			int i = 0;
			foreach (string path in Directory.EnumerateFiles(@"E:\Programs\Steam\steamapps\common\Morrowind\Maps", "*.bmp")) {
				string coord = Path.GetFileName(path);
				coord = coord.Substring(coord.IndexOf('(') + 1);
				var split = coord.Split(',', ')');
				int x = int.Parse(split[0]);
				int y = int.Parse(split[1]);
				MagickImage image = new MagickImage(path);
				map.Draw(new Drawables().Composite((x - minX) * 256, map.Height - (y - minY) * 256  -256, image));
				i++; if (i % 10 == 0) Console.WriteLine(path);
			}
			map.Write("morrowind.bmp");
		}

		public static void MWRegionDoors(string espPath) {
			int cellSize = 64;
			int xAdd = 42 * 8192;
			int yAdd = 38 * 8192;

			//HashSet<string> cells = new HashSet<string>();
			Dictionary<string, string> cellTypes = new Dictionary<string, string>();
			foreach (string line in File.ReadAllLines(@"F:\Extracted\Morrowind\celltypes.txt")) {
				var split = line.Split('\t');
				cellTypes[split[0]] = split[1];
			}


			JArray esp = JArray.Parse(File.ReadAllText(espPath));
			string[,] cellRegions = new string[256, 256];
			for (int i = 0; i < esp.Count; i++) {
				if (esp[i]["type"] != null && esp[i]["type"].Value<string>() == "Cell" && ((esp[i]["data"]["flags"].Value<int>() & 1) == 0)) {
					//string cellName = esp[i]["id"].Value<string>();
					var cellX = esp[i]["data"]["grid"][0].Value<int>();
					var cellY = esp[i]["data"]["grid"][1].Value<int>();
					string region = esp[i]["region"] is null ? "Wilderness" : esp[i]["region"].Value<string>();
					cellRegions[128 + cellX, 128 + cellY] = region;
				}
			}

			Dictionary<string, string> intCellRegions = new Dictionary<string, string>();


			for (int i = 0; i < esp.Count; i++) {
				if (esp[i]["type"] != null && esp[i]["type"].Value<string>() == "Cell" && ((esp[i]["data"]["flags"].Value<int>() & 1) == 1)) {
					string cellName = esp[i]["id"].Value<string>();
					//var cellX = esp[i]["data"]["grid"][0].Value<int>();
					//var cellY = esp[i]["data"]["grid"][1].Value<int>();
					//string region = esp[i]["region"] is null ? "Wilderness" : esp[i]["region"].Value<string>(); 

					//if (!isInterior) Console.WriteLine($"{region} {cellName} {cellX} {cellY}");
					JArray refs = (JArray)esp[i]["references"];
					for (int refNum = 0; refNum < refs.Count; refNum++) {

						if (refs[refNum]["door_destination_coords"] != null) {

							if (refs[refNum]["door_destination_cell"] != null) {
								//Console.WriteLine(cellName + " -> " + refs[refNum]["door_destination_cell"].Value<string>());
							} else {

								JArray coords = (JArray)refs[refNum]["door_destination_coords"];
								float x = coords[0].Value<float>();
								float y = coords[1].Value<float>();
								float xMap = (x + xAdd) * cellSize / 8192;
								float yMap = (yAdd - y) * cellSize / 8192;

								string type = cellTypes.ContainsKey(cellName) ? cellTypes[cellName] : "unknown";
								cellName = cellName.Split(',')[0];

								float xAdj = (x > 0) ? x : x - 8192; //to counteract rounding down
								float yAdj = (y > 0) ? y : y - 8192; //to counteract rounding down
								int cellX = (int)(xAdj / 8192);
								int cellY = (int)(yAdj / 8192);

								string region = cellRegions[cellX + 128, cellY + 128];
								if (region is null) region = "NULLREGION";
								region = region.EndsWith(" Region") ? region.Substring(0, region.Length - 7) : region;

								if (!intCellRegions.ContainsKey(cellName)) {
									intCellRegions[cellName] = region;
									Console.WriteLine($"{region}_{type}_{cellName}_{cellX}_{cellY}");
								} else if (intCellRegions[cellName] != region) Console.WriteLine($"CELL IN MULTIPLE REGIONS {cellName} {intCellRegions[cellName]} {region}");

								//cells.Add(cellName);
								//
								//Console.WriteLine($"{cellName} -> ({xMap},{yMap})");
							}
						}
					}
				}
			}
			Console.WriteLine("\r\n\r\n\r\n");
			//foreach (string cell in cells) Console.WriteLine(cell);

		}

		public static void MWUnknownDoords(string espPath) {
            Dictionary<string, string> cellTypes = new Dictionary<string, string>();
            HashSet<string> cells = new HashSet<string>();
            foreach (string line in File.ReadAllLines(@"F:\Extracted\Morrowind\celltypes.txt")) {
                var split = line.Split('\t');
                cellTypes[split[0]] = split[1];
            }

            JArray esp = JArray.Parse(File.ReadAllText(espPath));
            for (int i = 0; i < esp.Count; i++) {
                if (esp[i]["type"] != null && esp[i]["type"].Value<string>() == "Cell") {
                    string cellName = esp[i]["id"].Value<string>();
                    //Console.WriteLine(cellName);
                    JArray refs = (JArray)esp[i]["references"];
                    for (int refNum = 0; refNum < refs.Count; refNum++) {
                        if (refs[refNum]["door_destination_coords"] != null) {
                            if (refs[refNum]["door_destination_cell"] != null) {
                                //Console.WriteLine(cellName + " -> " + refs[refNum]["door_destination_cell"].Value<string>());
                            } else {
                                cells.Add(cellName);
                                //Console.WriteLine($"{cellName} -> ({xMap},{yMap})");
                            }
                        }
                    }
                }
            }

			foreach (string cell in cells) if (!cellTypes.ContainsKey(cell)) Console.WriteLine(cell);



        }

        struct Float2 {
			public float x;
			public float y;
        }

        public static void DoorsListNew(string espPath) {
            int cellSize = 64;
            int xAdd = 42 * 8192;
            int yAdd = 38 * 8192;

			HashSet<string> cells = new HashSet<string>();

            Dictionary<string, List<Float2>> mergePositions = new Dictionary<string, List<Float2>>();
            Dictionary<string, string> mergeTypes = new Dictionary<string, string>();

            Dictionary<string, string> cellTypes = new Dictionary<string, string>();
			Dictionary<string, string> cellSources = new Dictionary<string, string>();


            foreach (string line in File.ReadAllLines(@"F:\Extracted\Morrowind\celltypes.txt")) {
                var split = line.Split('\t');
                cellTypes[split[0]] = split[1];
				if(split.Length > 2) cellSources[split[0]] = split[2];
            }


            JArray esp = JArray.Parse(File.ReadAllText(espPath));

            Dictionary<int, string> cellRegions = new Dictionary<int, string>();
            for (int i = 0; i < esp.Count; i++) {
				var cell = esp[i];

                if (cell["type"] != null && cell["type"].Value<string>() == "Cell") {
                    bool isInterior = (cell["data"]["flags"].Value<int>() & 1) > 0;
					if(!isInterior) {
                        int x = cell["data"]["grid"][0].Value<int>();
                        int y = cell["data"]["grid"][1].Value<int>();

                        if (cell["region"] != null) {
							string region = cell["region"].Value<string>();
							cellRegions[x + y * 128] = region;
                        }
                    }
                }
            }

            for (int i = 0; i < esp.Count; i++) {
                if (esp[i]["type"] != null && esp[i]["type"].Value<string>() == "Cell") {
                    string cellName = esp[i]["id"].Value<string>();


                    //Console.WriteLine(cellName);
                    JArray refs = (JArray)esp[i]["references"];
                    for (int refNum = 0; refNum < refs.Count; refNum++) {
                        if (refs[refNum]["door_destination_coords"] != null) {
                            if (refs[refNum]["door_destination_cell"] != null) {
                                //Console.WriteLine(cellName + " -> " + refs[refNum]["door_destination_cell"].Value<string>());
                            } else {
                                if (cells.Contains(cellName)) continue;
                                cells.Add(cellName);

                                JArray coords = (JArray)refs[refNum]["door_destination_coords"];
                                float x = coords[0].Value<float>();
                                float y = coords[1].Value<float>();
								int cellX = (int)(x / 8192);
								int cellY = (int)(y / 8192);
                                //float xMap = (x + xAdd) * cellSize / 8192;
                                //float yMap = (yAdd - y) * cellSize / 8192;

                                string type = cellTypes.ContainsKey(cellName) ? cellTypes[cellName] : "unknown";
								string source = cellSources.ContainsKey(cellName) ? cellSources[cellName] : "";

                                int _t = cellName.IndexOf(',');
                                if (_t == -1) _t = cellName.IndexOf(':');
                                string mergeName = _t != -1 ? cellName.Substring(0, _t) : "";

								string region = cellRegions.ContainsKey(cellX + cellY * 128) ? cellRegions[cellX + cellY * 128] : "wilderness";

								Console.WriteLine($"{cellName}@{type}@{mergeName}@{region}@{source}");

                                //Console.WriteLine($"{cellName} -> ({xMap},{yMap})");
                            }
                        }
                    }
                }
            }
			/*
            foreach (string mergeName in mergePositions.Keys) {
                float x = 0;
                float y = 0;
                var positions = mergePositions[mergeName];

                foreach (Float2 pos in positions) {
                    x += pos.x; y += pos.y;
                }
                x /= positions.Count; y /= positions.Count;
                string type = mergeTypes[mergeName];
                string markerType = type.Contains("_town") || type.Contains("_city") || type.Contains("_fort") ? "mark" : "icon";
                Console.WriteLine($"<div class=\"{markerType} {mergeTypes[mergeName].Substring(0, 3)} {mergeTypes[mergeName]}\" style=\"left:{(int)(x + 0.5)};top:{(int)(y + 0.5)};\" title=\"{mergeName}\"></div>");

            }
			*/
            //Console.WriteLine("\r\n\r\n\r\n");
            //foreach (string cell in cells) Console.WriteLine(cell);

        }


        public static void DoorsMerged(string espPath) {
            int cellSize = 64;
            int xAdd = 42 * 8192;
            int yAdd = 38 * 8192;



			Dictionary<string, List<Float2>> mergePositions = new Dictionary<string, List<Float2>>();
			Dictionary<string, string> mergeTypes = new Dictionary<string, string>();

			Dictionary<string, string> mergeNames = new Dictionary<string, string>();

            Dictionary<string, string> cellTypes = new Dictionary<string, string>();
            foreach (string line in File.ReadAllLines(@"F:\Extracted\Morrowind\celltypes2.txt")) {
                var split = line.Split('\t');
                cellTypes[split[0]] = split[1];
				if (split[2] != "") mergeNames[split[0]] = split[2];
            }


            JArray esp = JArray.Parse(File.ReadAllText(espPath));
            for (int i = 0; i < esp.Count; i++) {
				var cell = esp[i];
                if (cell["type"] != null && cell["type"].Value<string>() == "Cell") {
                    string cellName = cell["id"].Value<string>();

                    bool isInterior = (cell["data"]["flags"].Value<int>() & 1) > 0;
                    if (!isInterior) continue;


                    //Console.WriteLine(cellName);
                    JArray refs = (JArray)cell["references"];
                    for (int refNum = 0; refNum < refs.Count; refNum++) {
                        if (refs[refNum]["door_destination_coords"] != null) {
                            if (refs[refNum]["door_destination_cell"] != null) {
                                //Console.WriteLine(cellName + " -> " + refs[refNum]["door_destination_cell"].Value<string>());
                            } else {
                                JArray coords = (JArray)refs[refNum]["door_destination_coords"];
                                float x = coords[0].Value<float>();
                                float y = coords[1].Value<float>();
                                float xMap = (x + xAdd) * cellSize / 8192;
                                float yMap = (yAdd - y) * cellSize / 8192;

                                string type = cellTypes.ContainsKey(cellName) ? cellTypes[cellName] : "unknown";

								if(mergeNames.ContainsKey(cellName)) {
									string mergeName = mergeNames[cellName];
                                    if (!mergePositions.ContainsKey(mergeName)) {
                                        mergePositions[mergeName] = new List<Float2>();
                                        mergeTypes[mergeName] = type;
                                    }
                                    mergePositions[mergeName].Add(new Float2 { x = xMap, y = yMap });
                                } else {
                                    Console.WriteLine($"<div class=\"icon {type.Substring(0, 3)} {type}\" style=\"left:{(int)(xMap + 0.5)};top:{(int)(yMap + 0.5)};\" title=\"{cellName}\"></div>");
                                }
                                //Console.WriteLine($"{cellName} -> ({xMap},{yMap})");
                            }
                        }
                    }
                }
            }

			foreach(string mergeName in mergePositions.Keys) {
				float x = 0;
				float y = 0;
				var positions = mergePositions[mergeName];

                foreach (Float2 pos in positions) {
					x += pos.x; y += pos.y;
				}
				x /= positions.Count; y /= positions.Count;
				string type = mergeTypes[mergeName];
				string markerType = type.Contains("_town") || type.Contains("_city") || type.Contains("_fort") ? "mark" : "icon";
                Console.WriteLine($"<div class=\"{markerType} {mergeTypes[mergeName].Substring(0, 3)} {mergeTypes[mergeName]}\" style=\"left:{(int)(x + 0.5)};top:{(int)(y + 0.5)};\" title=\"{mergeName}\"></div>");

            }
            //Console.WriteLine("\r\n\r\n\r\n");
            //foreach (string cell in cells) Console.WriteLine(cell);

        }

        public static void MWListUnknownUnusedDoorCells(string espPath) {
            int cellSize = 64;
            int xAdd = 42 * 8192;
            int yAdd = 38 * 8192;

            Dictionary<string, string> cellTypes = new Dictionary<string, string>();
            foreach (string line in File.ReadAllLines(@"F:\Extracted\Morrowind\celltypes.txt")) {
                var split = line.Split('\t');
                cellTypes[split[0]] = split[1];
            }

            HashSet<string> cells = new HashSet<string>(cellTypes.Keys);


            JArray esp = JArray.Parse(File.ReadAllText(espPath));
            for (int i = 0; i < esp.Count; i++) {
                if (esp[i]["type"] != null && esp[i]["type"].Value<string>() == "Cell") {
                    string cellName = esp[i]["id"].Value<string>();
                    //Console.WriteLine(cellName);
                    JArray refs = (JArray)esp[i]["references"];
                    for (int refNum = 0; refNum < refs.Count; refNum++) {
                        if (refs[refNum]["door_destination_coords"] != null) {
                            if (refs[refNum]["door_destination_cell"] != null) {
                                //Console.WriteLine(cellName + " -> " + refs[refNum]["door_destination_cell"].Value<string>());
                            } else {
								if (!cellTypes.ContainsKey(cellName)) Console.WriteLine(cellName);
								cells.Remove(cellName);
                            }
                        }
                    }
                }
            }

            Console.WriteLine("\r\n\r\n\r\n");
            foreach (string cell in cells) Console.WriteLine(cell);

        }


        public static void MWDoors(string espPath) {
			int cellSize = 64;
			int xAdd = 42 * 8192;
			int yAdd = 38 * 8192;

			HashSet<string> cells = new HashSet<string>();
			Dictionary<string, string> cellTypes = new Dictionary<string, string>();
			foreach(string line in File.ReadAllLines(@"F:\Extracted\Morrowind\celltypes2.txt")) {
				var split = line.Split('\t');
				cellTypes[split[0]] = split[1];
            }
			

			JArray esp = JArray.Parse(File.ReadAllText(espPath));
			for(int i = 0; i < esp.Count; i++) {
				var cell = esp[i];
				if (cell["type"] != null && cell["type"].Value<string>() == "Cell") {

                    bool isInterior = (cell["data"]["flags"].Value<int>() & 1) > 0;
					if (!isInterior) continue;

                    string cellName = cell["id"].Value<string>();
					//Console.WriteLine(cellName);
					JArray refs = (JArray)cell["references"];
					for(int refNum = 0; refNum < refs.Count; refNum++) {
						if(refs[refNum]["door_destination_coords"] != null) {
							if(refs[refNum]["door_destination_cell"] != null) {
								//Console.WriteLine(cellName + " -> " + refs[refNum]["door_destination_cell"].Value<string>());
							} else {
								JArray coords = (JArray)refs[refNum]["door_destination_coords"];
								float x = coords[0].Value<float>();
								float y = coords[1].Value<float>();
								float xMap = (x + xAdd) * cellSize / 8192;
								float yMap = (yAdd - y) * cellSize / 8192;

								string type = cellTypes.ContainsKey(cellName) ? cellTypes[cellName] : "unknown";

								int cellX = (int)(x/8192);
								int cellY = (int)(y / 8192);
								cells.Add(cellName);
								Console.WriteLine($"<div class=\"icon {type.Substring(0,3)} {type}\" style=\"left:{(int)(xMap + 0.5)};top:{(int)(yMap + 0.5)};\" title=\"{cellName}\"></div>");
								//Console.WriteLine($"{cellName} -> ({xMap},{yMap})");
							}
                        }
                    }
				}
			}
			//Console.WriteLine("\r\n\r\n\r\n");
			//foreach (string cell in cells) Console.WriteLine(cell);

		}
		public static void MWQuests(params string[] espPaths) {

			Dictionary<string, string> questNames = new Dictionary<string, string>();
			Dictionary<string, string> questFiles = new Dictionary<string, string>();

			foreach(string espPath in espPaths) {
				string file = Path.GetFileNameWithoutExtension(espPath);
				Console.WriteLine(espPath);
				JArray esp = JArray.Parse(File.ReadAllText(espPath));
				int i = 0;
				while (i < esp.Count) {
					if (esp[i]["type"] != null && esp[i]["type"].Value<string>() == "Dialogue") {
						string type = esp[i]["dialogue_type"].Value<string>();
						if (type == "Journal") {
							string id = esp[i]["id"].Value<string>();
							if(!questFiles.ContainsKey(id)) questFiles[id] = file;
							if (!questNames.ContainsKey(id) || questNames[id] == "NO QUEST NAMEAAAAAAAAAAAAAAAAAAAAA") {
								string name = "NO QUEST NAMEAAAAAAAAAAAAAAAAAAAAA";
								string zero = null;
								string name2 = null;
								i++;
								while (esp[i]["type"] != null && esp[i]["type"].Value<string>() == "Info") {
									if (esp[i]["data"]["disposition"].Value<int>() == 0) {
										zero = esp[i]["text"].Value<string>();
										if (esp[i]["quest_name"] != null) name = esp[i]["text"].Value<string>();
									} else if (esp[i]["quest_name"] != null) name2 = esp[i]["text"].Value<string>();
									i++;
								}
								if (name == "NO QUEST NAMEAAAAAAAAAAAAAAAAAAAAA" && zero != null) questNames[id] = zero;
								else if (name == "NO QUEST NAMEAAAAAAAAAAAAAAAAAAAAA" && name2 != null) questNames[id] = name2;
								else questNames[id] = name;
								i--;
							}
						}
					}
					i++;
				}
			}
			foreach (string quest in questNames.Keys) Console.WriteLine(questFiles[quest] + "|" + quest + "|" + questNames[quest]);
		}

		public static void MWBooks(string espPath) {
			JArray esp = JArray.Parse(File.ReadAllText(espPath));
			for (int i = 0; i < esp.Count; i++) {
				if (esp[i]["type"] != null && esp[i]["type"].Value<string>() == "Book") {
					string id = esp[i]["id"].Value<string>();
					string name = esp[i]["name"].Value<string>();
					string icon = esp[i]["icon"].Value<string>();
					Console.WriteLine($"{name}|{id}|{icon}");
					//if (id == "") id = "Wilderness";
					//string region = esp[i]["region"] != null ? esp[i]["region"].Value<string>() : "no_region";
					//int x = esp[i]["data"]["grid"][0].Value<int>();
					//int y = esp[i]["data"]["grid"][1].Value<int>();
				}
			}

		}


		//ListMWRefs2(2000, @"F:\Extracted\Tools\morrowind.json", @"F:\Extracted\Tools\tribunal.json", @"F:\Extracted\Tools\bloodmoon.json", @"F:\Extracted\Tools\trmainland.json", @"F:\Extracted\Tools\trpreview.json");
		//ListMWRefs("meshes.txt", @"F:\Extracted\Tools\morrowind.json", @"F:\Extracted\Tools\tribunal.json", @"F:\Extracted\Tools\bloodmoon.json");
		//ListMWRefs("meshestr.txt", @"F:\Extracted\Tools\morrowind.json", @"F:\Extracted\Tools\tribunal.json", @"F:\Extracted\Tools\bloodmoon.json", @"F:\Extracted\Tools\tamrieldata.json", @"F:\Extracted\Tools\trmainland.json", @"F:\Extracted\Tools\trpreview.json");
		//MWIDMeshLookup(@"F:\Extracted\Tools\morrowind.json", @"F:\Extracted\Tools\tribunal.json", @"F:\Extracted\Tools\bloodmoon.json", @"F:\Extracted\Tools\tamrieldata.json", @"F:\Extracted\Tools\trmainland.json", @"F:\Extracted\Tools\trpreview.json");

		static Dictionary<string, string> MWIDMeshLookup(params JArray[] esps) {
			string[] meshTypesArray = new string[] { "Static", "Door", "MiscItem", "Weapon", "Container", "Light", "Armor", "Clothing", "Activator", "Ingredient", "Book", "Alchemy" };
			HashSet<string> meshTypes = new HashSet<string>(); foreach (string type in meshTypesArray) meshTypes.Add(type);

			Dictionary<string, string> lookup = new Dictionary<string, string>();
			foreach (JArray esp in esps) {
				for (int i = 0; i < esp.Count; i++) {
					string type = esp[i]["type"] != null ? esp[i]["type"].Value<string>() : "";
					if (meshTypes.Contains(type)) {
						string mesh = esp[i]["mesh"] != null ? esp[i]["mesh"].Value<string>() : "";
						if (mesh != "") lookup[esp[i]["id"].Value<string>()] = mesh.ToLower();
					}
				}
			}
			return lookup;
		}




		static void ListMWRefs(string outFileName, params string[] espPaths) {
			Dictionary<string, int> refCounts = new Dictionary<string, int>();

			JArray[] esps = new JArray[espPaths.Length];
			for (int i = 0; i < esps.Length; i++) {
				Console.WriteLine(Path.GetFileNameWithoutExtension(espPaths[i]));
				esps[i] = JArray.Parse(File.ReadAllText(espPaths[i]));
			}

			Dictionary<string, string> meshLookup = MWIDMeshLookup(esps);
			Console.WriteLine("Gathered meshes");

			//gather refs
			for (int espIndex = 0; espIndex < esps.Length; espIndex++) {
				for (int i = 0; i < esps[espIndex].Count; i++) {
					if (esps[espIndex][i]["type"] != null && esps[espIndex][i]["type"].Value<string>() == "Cell") {
						//string id = esps[espIndex][i]["id"].Value<string>();
						//if (id == "") id = "Wilderness";
						//string region = esps[espIndex][i]["region"] != null ? esps[espIndex][i]["region"].Value<string>() : "no_region";
						//int x = esps[espIndex][i]["data"]["grid"][0].Value<int>();
						//int y = esps[espIndex][i]["data"]["grid"][1].Value<int>();
						bool isInterior = (esps[espIndex][i]["data"]["flags"].Value<int>() & 1) > 0;
						if (!isInterior) {
							//Console.WriteLine($"{id}, {region}, {x},{y}");
							for (int refIndex = 0; refIndex < esps[espIndex][i]["references"].Count(); refIndex++) {
								string refId = esps[espIndex][i]["references"][refIndex]["id"].Value<string>();
								if (!refCounts.ContainsKey(refId)) refCounts[refId] = 1;
								else refCounts[refId] = refCounts[refId] + 1;
							}
						}
					}
				}
			}

			TextWriter writer = new StreamWriter(File.Open(outFileName, FileMode.Create));
			Dictionary<string, int> meshCounts = new Dictionary<string, int>();
			foreach (string id in refCounts.Keys) {
				if (meshLookup.ContainsKey(id)) {
					string mesh = meshLookup[id].ToLower();
					if (!meshCounts.ContainsKey(mesh)) meshCounts[mesh] = refCounts[id];
					else meshCounts[mesh] = meshCounts[mesh] + refCounts[id];
					//writer.WriteLine($"{id}|{meshLookup[id]}|{refCounts[id]}");
				}
				//else Console.WriteLine($"{id}: {refCounts[id]}");
			}
			foreach (string mesh in meshCounts.Keys) {
				writer.WriteLine($"{mesh}|{meshCounts[mesh]}");
			}
			writer.Flush();
			writer.Close();

		}


		static void MWIDMeshLookup(params string[] espPaths) {

			string[] meshTypesArray = new string[] { "Static", "Door", "MiscItem", "Weapon", "Container", "Light", "Armor", "Clothing", "Activator", "Ingredient", "Book", "Alchemy" };
			HashSet<string> meshTypes = new HashSet<string>(); foreach (string type in meshTypesArray) meshTypes.Add(type);

			TextWriter w = new StreamWriter(File.Open("idlookup.txt", FileMode.Create));

			foreach (string espPath in espPaths) {

				Console.Write(Path.GetFileNameWithoutExtension(espPath));
				JArray esp = JArray.Parse(File.ReadAllText(espPath));
				Console.WriteLine(" parsed");

				for (int i = 0; i < esp.Count; i++) {
					string type = esp[i]["type"] != null ? esp[i]["type"].Value<string>() : "";
					if (meshTypes.Contains(type)) {
						string mesh = esp[i]["mesh"] != null ? esp[i]["mesh"].Value<string>() : "";
						if (mesh != "") w.WriteLine(esp[i]["id"].Value<string>() + "|" + mesh.ToLower());
					}
				}
			}
			w.Flush();
			w.Close();
		}



		static void ListMWRefs2(int heatmapMax, params string[] espPaths) {


            Dictionary<string, (int, int)> meshes = new Dictionary<string, (int, int)>();
			foreach (string line in File.ReadAllLines(@"E:\Anna\Desktop\io_scene_mw\lib\meshes.txt")) {
				string[] words = line.Split('|');
				meshes[words[0]] = (int.Parse(words[1]), int.Parse(words[2]));
			}

			Dictionary<string, string> idLookup = new Dictionary<string, string>();
			foreach (string line in File.ReadAllLines(@"F:\Anna\Visual Studio\SmallScripts\SmallScripts\bin\x64\Debug\idlookup.txt")) {
				string[] words = line.Split('|');
				idLookup[words[0]] = words[1];
			}

			int imageSize = 128;
			byte[] imageData = new byte[imageSize * imageSize];

			foreach (string espPath in espPaths) {

				Console.Write(Path.GetFileNameWithoutExtension(espPath));
				JArray esp = JArray.Parse(File.ReadAllText(espPath));
				Console.WriteLine(" parsed");

				for (int i = 0; i < esp.Count; i++) {
					if (esp[i]["type"] != null && esp[i]["type"].Value<string>() == "Cell") {

						string id = esp[i]["id"].Value<string>();
						if (id == "") id = "Wilderness";

						string region = esp[i]["region"] != null ? esp[i]["region"].Value<string>() : "Null Region";

						int x = esp[i]["data"]["grid"][0].Value<int>();
						int y = esp[i]["data"]["grid"][1].Value<int>();

						bool isInterior = (esp[i]["data"]["flags"].Value<int>() & 1) > 0;
						if (isInterior) continue;

						int meshesCount = 0;
						int triShapeCount = 0;

						for (int refIndex = 0; refIndex < esp[i]["references"].Count(); refIndex++) {
							string refId = esp[i]["references"][refIndex]["id"].Value<string>();
							if (!idLookup.ContainsKey(refId) || !meshes.ContainsKey(idLookup[refId])) continue;
							meshesCount++;
							triShapeCount += meshes[idLookup[refId]].Item1;
						}

						byte col = (byte)(Math.Min(triShapeCount * 256 / heatmapMax, 254) + 1);
						int pixel = (x + imageSize / 2) % imageSize + (y * -1 + imageSize / 2) * imageSize;
						if (pixel >= 0 && pixel < imageData.Length && col > imageData[pixel]) {
							imageData[pixel] = col;

							Console.WriteLine($"{x},{y}|{id}|{region}|{meshesCount}|{triShapeCount}");
						}
					}
				}
			}

			MagickReadSettings settings = new MagickReadSettings() { Format = MagickFormat.Gray, Width = imageSize, Height = imageSize };
			MagickImage image = new MagickImage(imageData, settings);

			image.Write($"heatmap_{heatmapMax}.png");
		}
	}
}
