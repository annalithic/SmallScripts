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
using System.Dynamic;
using System.Xml.Linq;

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
			if (info["script_text"] != null) {
                string result = info.Str("script_text");
                foreach (string line in result.Split('\n')) {
					int commentIndex = line.IndexOf(';');
                    string lineNoComment = commentIndex == -1 ? line : line.Substring(0, commentIndex);

                    string[] split = lineNoComment.SplitQuotes(',', '.');
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
                string filter_function = filter.Str("function");
                string filter_id = filter.Str("id");
                string filter_comparison = filterComparison[filter.Str("comparison")];
                string filter_value = filter["value"]["type"].Str() == "Integer" ? filter["value"]["data"].Int().ToString() : filter["value"]["data"].Float().ToString();

                if (filter_type == "Journal") {
					questReqs.Add(new QuestReq { id = filter_id, comparison = filter_comparison, stage = filter["value"]["data"].Int() });
                } else if (filter_function == "Choice") {
					choice = filter["value"]["data"].Int();
                }
                miscFilters = miscFilters + $"{filter_type} {filter_function} {filter_id} {filter_comparison} {filter_value}|";
            }
        }
	}
}

namespace SmallScripts {
	static class TES3 {



		struct Vector2 {
			public float x;
			public float y;


			public Vector2(float x, float y) {
				this.x = x;
				this.y = y;
			}

			public static Vector2 operator +(Vector2 v1, Vector2 v2) {
				return new Vector2(v1.x + v2.x, v1.y + v2.y);
			}

            public static Vector2 operator -(Vector2 v1, Vector2 v2) {
                return new Vector2(v1.x - v2.x, v1.y - v2.y);
            }

            public static Vector2 operator /(Vector2 v1, int div) {
				return new Vector2(v1.x / div, v1.y / div);
			}

			public override string ToString() {
				return $"({(long)x},{(long)y})";
			}

			public (int, int) ToCell() {
				return ((int)(x/8192), (int)(y/8192));
			}

			public float LengthSquared() {
				return x*x + y*y;
			}

			public Vector2 MapPosition() {
                int cellSize = 64;
                int xAdd = 39 * 8192;
                int yAdd = 34 * 8192;
				return new Vector2((x + xAdd) * cellSize / 8192, (yAdd - y) * cellSize / 8192);

            }
        }

		class GroupInfo {
			public bool ignore;
			public string ignoreReason;
			public string gameplayType;
			public string owner;
			public string tileset;
			public string name;
			public string parent;
			public string regions;
			public string district;
			public float npcSearchOverride;

			//not taken from premade data
			public int cellCount;
			public int npcCount;
			public int guardCount;
			public int hostileCount;
			public bool onlyNpcsAreEnslaved;

			//mapping time
			public HashSet<Vector2> doorPositions;

			public GroupInfo() {
				onlyNpcsAreEnslaved = true;
				doorPositions = new HashSet<Vector2>();
			}

			public Vector2 MergePosition() {
				Vector2 average = new Vector2();
				foreach (Vector2 pos in doorPositions) {
					average += pos;
				}
				average /= doorPositions.Count;
				return average;
			}

			public override string ToString() {
				string ignoreText = ignore ? "Ignore" : "";
				string position = "NO POSITION";
				if (doorPositions.Count > 0) {
					position = (MergePosition() / 8192).ToString();
				}
				return $"{ignoreText}@{ignoreReason}@{gameplayType}@{owner}@{tileset}@{name}@{cellCount}@{regions}@{district}@{parent}@{npcSearchOverride}@{npcCount}@{guardCount}@{hostileCount}@{position}";
			}

			public string MapPopulationCircle() {

				var position = MergePosition().MapPosition();
                if (district == "Solstheim") position += new Vector2(448, -384);

                int friendlyCount = npcCount + guardCount;

				float sizeGrowthMult = 4.5f;
				int size = 11 - (int)sizeGrowthMult + (int)(Math.Sqrt(friendlyCount) * sizeGrowthMult);

				float textGrowthMult = 1.3f;
				int textSize = 8 - (int)textGrowthMult + (int)(Math.Sqrt(friendlyCount) * textGrowthMult);

				string count = friendlyCount.ToString();
				string shape = "circle";
				string sizeString = $"width:{size}px;height:{size}px;";

				if (friendlyCount <= 4) {
					count = "";
					shape = "diamond";
					sizeString = "";
				}

				if (owner != "") shape = shape + " " + owner;
				StringBuilder t = new StringBuilder($"{name}: {npcCount} NPC"); 
				if (npcCount > 1) t.Append("s");
				if (guardCount > 0) {
					t.Append($" + {guardCount} Guard"); 
					if (guardCount > 1) t.Append("s");
				}

                return $"<div class=\"{shape} {gameplayType}\" style=\"left:{(int)(position.x + 0.5)};top:{(int)(position.y + 0.5)};{sizeString}font-size:{textSize}pt\" title=\"{t.ToString()}\">{count}</div>";
			}
		}

		static string GetCellGroup(string s, Dictionary<string, GroupInfo> groupInfo = null) {
			var split = s.Split(',', ':');
            string g = split.FirstOrDefault();
			if (g == "Solstheim" && split.Length > 1) g = split[1].Trim();
			if (groupInfo != null) {
				if (!groupInfo.ContainsKey(g)) Console.WriteLine($"GROUP MISSING: {g}");
				else while (groupInfo.ContainsKey(g) && groupInfo[g].parent != "") {
						g = groupInfo[g].parent;
					}
			}
			return g;
		}

		public enum PopulationDumpMode {
			DuplicateNpcs,
			GroupTable,
			GroupPopulationMap,
			ExteriorNpcMap
		}
		public static void PopulationNew(PopulationDumpMode mode, string extraDataFolder, params string[] espPaths) {

			var groupInfo = new Dictionary<string, GroupInfo>();

			foreach (string line in File.ReadAllLines(Path.Combine(extraDataFolder,"groups.txt"))) {
				if (line.StartsWith("HEADER")) continue;
				var words = line.Split('\t');
				groupInfo[words[5]] = new GroupInfo {
					ignore = words[0] == "Ignore",
					ignoreReason = words[1],
					gameplayType = words[2],
					owner = words[3],
					tileset = words[4],
					name = words[5],
					regions = words[7],
					district = words[8],
					parent = words[9],
					npcSearchOverride = words[10] == "" ? -1 : float.Parse(words[10])
				};
			}

			var exteriors = new HashSet<int>();
			var interiors = new Dictionary<string, int>();

			var npcs = new HashSet<string>();
			var npcsHostile = new HashSet<string>();
			var slaveIds = new HashSet<string>();

			//exterior npc finding - copying old method, look at these fucked up types
			var doorRefs = new Dictionary<(int, int), List<(Vector2 pos, string cell)>>();
			var npcRefs = new List<(Vector2 pos, string id)>();

			//duplicate/guard detection
			var npcGuardNames = new HashSet<string>();
			var npcNameIgnore = new HashSet<string>();
			var npcIdIgnore = new HashSet<string>();
			foreach(string line in File.ReadAllLines(Path.Combine(extraDataFolder, "npcs.txt"))) {
				var split = line.Split('\t');
				if (split[0] == "Ignore") npcNameIgnore.Add(split[2]);
				else if (split[0] == "Alternate") 
					foreach(string id in split[1].Split(',')) npcIdIgnore.Add(id);
				else if (split[0] == "Guard") npcGuardNames.Add(split[2]);
			}


			var npcCounts = new Dictionary<string, int>();
            var npcNames = new Dictionary<string, string>();
            var npcClasses = new Dictionary<string, string>();


			foreach (string espPath in espPaths) {
				JArray esp = JArray.Parse(File.ReadAllText(espPath));
				for (int i = 0; i < esp.Count; i++) {
					var form = esp[i];
					if (form.Str("type") == "Cell") {
						if (form["data"].Str("flags").IndexOf("IS_INTERIOR") == -1) {
							exteriors.Add(i);
						} else {
							string cellName = form.Str("name");
							interiors[cellName] = i;
						}
					} else if (form.Str("type") == "Npc") {
						if (form["data"]["stats"] != null && form["data"]["stats"].Int("health") == 0) continue;
						
						string npcId = form.Str("id");
						string npcName = form.Str("name");

						if (npcIdIgnore.Contains(npcId)) continue;
						if (npcNameIgnore.Contains(npcName)) continue;

						npcNames[npcId] = npcName;
                        npcClasses[npcId] = form.Str("class");


                        if (form.Str("class") == "Slave") slaveIds.Add(npcId);

						if (form["ai_data"].Int("fight") >= 71) {
							npcsHostile.Add(npcId);
						} else {
							npcs.Add(npcId);
						}
					}

				}

				foreach (string cellName in interiors.Keys) {
					var cell = esp[interiors[cellName]];
					if (((JArray)cell["references"]).Count == 0) continue;


					string group = GetCellGroup(cellName, groupInfo);
					var info = groupInfo[group];
					info.cellCount++;

					foreach (JObject reference in (JArray)cell["references"]) {
						string refId = reference.Str("id");
						if (npcs.Contains(refId)) {

                            if (!slaveIds.Contains(refId)) info.onlyNpcsAreEnslaved = false;

                            if (npcGuardNames.Contains(npcNames[refId]))
								info.guardCount++;
							else
								info.npcCount++;

							if(!info.ignore) {
                                string npcCountName = $"{npcNames[refId]}@{npcClasses[refId]}";
                                if (!npcCounts.ContainsKey(npcCountName)) npcCounts[npcCountName] = 1;
                                else npcCounts[npcCountName]++;
                            }

                            

						} else if (npcsHostile.Contains(refId)) info.hostileCount++;
					}
				}

				foreach (int i in exteriors) {
					var cell = esp[i];
					foreach (JObject reference in (JArray)cell["references"]) {
						string refId = reference.Str("id");
						if (reference["destination"] != null) {
                            string destination = reference["destination"].Str("cell");
                            if (!interiors.ContainsKey(destination)) continue;
                            string destinationGroup = GetCellGroup(destination);

                            JArray coords = (JArray)reference["translation"];
							var doorPos = new Vector2(coords[0].Value<float>(), coords[1].Value<float>());
                            groupInfo[destinationGroup].doorPositions.Add(doorPos);

							var cellCoords = doorPos.ToCell();

							if (!doorRefs.ContainsKey(cellCoords)) doorRefs[cellCoords] = new List<(Vector2, string)>();
							doorRefs[cellCoords].Add((doorPos, destination));

                        } else if (npcs.Contains(refId)) {

                            string npcCountName = $"{npcNames[refId]}@{npcClasses[refId]}";
                            if (!npcCounts.ContainsKey(npcCountName)) npcCounts[npcCountName] = 1;
                            else npcCounts[npcCountName]++;

                            JArray coords = (JArray)reference["translation"];
                            var pos = new Vector2(coords[0].Value<float>(), coords[1].Value<float>());
							npcRefs.Add((pos, refId));
						}
                    }
				}

				if(mode == PopulationDumpMode.DuplicateNpcs) {
                    foreach (string npc in npcCounts.Keys) {
                        int i = npcCounts[npc];
                        if (i < 2) continue;
						StringBuilder s = new StringBuilder($"{npc}@{i}");
						string name = npc.Split('@')[0];
						//wtf is a big-O
                        foreach(string npcId in npcNames.Keys) 
							if (npcNames[npcId] == name) {
								s.Append('@'); s.Append(npcId);
						}
						Console.WriteLine(s.ToString());
                    }
					return;
                }



                string closestCell = "NONE";
                float defaultSearchDist = 3250;

				if(mode == PopulationDumpMode.ExteriorNpcMap) {
                    foreach (var group in groupInfo.Values) {
                        if (group.parent != "" || group.doorPositions.Count == 0) continue;
                        string gameplayType = group.gameplayType == "Dungeon" ? "d" : "s";
                        var mapPos = group.MergePosition().MapPosition();
                        if (group.district == "Solstheim") mapPos += new Vector2(640, -128);
                        Console.WriteLine($"<div class=\"group {gameplayType}\" style=\"left:{(int)(mapPos.x + 0.5)};top:{(int)(mapPos.y + 0.5)};\" title=\"{group.name}\"></div>");
                    }
                }

                foreach (var npc in npcRefs) {

					bool foundOverride = false;

					(int x, int y) cell = npc.pos.ToCell();
					float findDist = float.MaxValue;
                    for (int searchY = cell.y - 1; searchY <= cell.y + 1; searchY++) {
                        for (int searchX = cell.x - 1; searchX <= cell.x + 1; searchX++) {
                            if (doorRefs.ContainsKey((searchX, searchY))) {
                                foreach (var door in doorRefs[(searchX, searchY)]) {
                                    float dist = (npc.pos - door.pos).LengthSquared();
                                    if (dist < findDist) {
                                        var group = groupInfo[GetCellGroup(door.cell, groupInfo)];
										if (group.gameplayType == "Dungeon" || group.ignore) continue;
										if (group.npcSearchOverride > 0) {
											if (dist > (group.npcSearchOverride * group.npcSearchOverride)) continue;
											foundOverride = true;
										}
                                        findDist = dist;
										closestCell = door.cell;
                                    }
                                }
                            }
                        }
                    }

                    string foundType = "notfound";
					if(findDist < defaultSearchDist * defaultSearchDist || foundOverride) {
						var group = groupInfo[GetCellGroup(closestCell, groupInfo)];

                        if (!slaveIds.Contains(npc.id)) group.onlyNpcsAreEnslaved = false;

                        if (npcGuardNames.Contains(npcNames[npc.id]))
                            group.guardCount++;
                        else
                            group.npcCount++;
						foundType = group.gameplayType == "Dungeon" ? "dungeon" : "settlement";
					}
                    
					if(mode == PopulationDumpMode.ExteriorNpcMap) {
						string titleText = closestCell == "NONE" ? "" : $"{npc.id} - {closestCell}|{(int)(Math.Sqrt(findDist))}";
						var mapPos = npc.pos.MapPosition();
						//AAAAAAAAAAAAAAAAAAAAAAAAAAAAA
                        if (groupInfo[GetCellGroup(closestCell, groupInfo)].district == "Solstheim") mapPos += new Vector2(640, -128);
                        Console.WriteLine($"<div class=\"npc {foundType}\" style=\"left:{(int)(mapPos.x + 0.5)};top:{(int)(mapPos.y + 0.5)};\" title=\"{titleText}\"></div>");
					}
				}

				foreach (var info in groupInfo.Values) {
					if(mode == PopulationDumpMode.GroupTable) {
                        Console.WriteLine(info);
                    } else if (mode == PopulationDumpMode.GroupPopulationMap) {
                        if (info.ignore || info.doorPositions.Count == 0 || info.parent != "" || (info.npcCount + info.guardCount) <= 0 || info.onlyNpcsAreEnslaved) continue;
                        Console.WriteLine(info.MapPopulationCircle());
                    }
				}



			}


		}

		public static void CellGraph(params string[] espPaths) {

			var interiors = new Dictionary<string, int>();
			var connections = new HashSet<string>();

			var exteriors = new HashSet<int>();
			var surfaceGroups = new HashSet<string>();
			var usedGroups = new HashSet<string>();

			foreach (string espPath in espPaths) {
				JArray esp = JArray.Parse(File.ReadAllText(espPath));
				for (int i = 0; i < esp.Count; i++) {
					var form = esp[i];
					if (form.Str("type") != "Cell") continue;
					if (form["data"].Str("flags").IndexOf("IS_INTERIOR") == -1) {
						exteriors.Add(i);
					} else {
						string cellName = form.Str("name");
						interiors[cellName] = i;
					}

				}

				foreach (int i in exteriors) {
					var exterior = esp[i];
					foreach (JObject reference in (JArray)exterior["references"]) {
						if (reference["destination"] == null) continue;
						string destination = reference["destination"].Str("cell");
						if (!interiors.ContainsKey(destination)) continue;
						string destinationGroup = destination.Split(',', ':').FirstOrDefault();
						surfaceGroups.Add(destinationGroup);
					}
				}

				foreach (string cellName in interiors.Keys) {

					var form = esp[interiors[cellName]];

					foreach (JObject reference in (JArray)form["references"]) {
						if (reference["destination"] == null) continue;
						string destination = reference["destination"].Str("cell");
						if (!interiors.ContainsKey(destination)) continue;

						string cellGroup = cellName.Split(',', ':').FirstOrDefault();
						string destinationGroup = destination.Split(',', ':').FirstOrDefault();

						if (destinationGroup == cellGroup) continue;

						if (string.Compare(cellGroup, destinationGroup) < 0) connections.Add($"\"{cellGroup}\"--\"{destinationGroup}\"");
						else connections.Add($"\"{destinationGroup}\"--\"{cellGroup}\"");

						usedGroups.Add(cellGroup);
						usedGroups.Add(destinationGroup);


						//                  if (destination == cellName) continue;
						//                  if (!interiors.ContainsKey(destination)) continue;

						//if (string.Compare(cellName, destination) < 0) connections.Add($"\"{cellName}\"--\"{destination}\"");
						//else connections.Add($"\"{destination}\"--\"{cellName}\"");
					}
				}

				foreach (string connection in connections) Console.WriteLine(connection);
				Console.WriteLine();
				foreach (string group in usedGroups) if (surfaceGroups.Contains(group)) Console.WriteLine($"\"{group}\" [style=filled,color=lightgreen]");
			}


		}

		public static void ListCellsNew(params string[] espPaths) {

			var regionNames = new Dictionary<string, string>();
			var regionDistricts = new Dictionary<string, string>();
			foreach (string line in File.ReadAllLines(@"E:\Extracted\Morrowind\NEW_CELL_DATA\regions.txt")) {
				var split = line.Split('\t');
				regionNames[split[2]] = split[4];
				regionDistricts[split[4]] = split[0];
			}


			var exteriors = new List<(int id, string name, bool isInterior, HashSet<string> regions, string group, string subgroup)>();
			var interiors = new Dictionary<string, (int id, string name, bool isInterior, HashSet<string> regions, string group, string subgroup)>();

			var interiorRegionsPropagated = new HashSet<string>();
			var interiorRegionsFrontier = new HashSet<string>();
			var interiorRegionsFrontierNew = new HashSet<string>();

			var groupCounts = new Dictionary<string, int>();
			var subgroupCounts = new Dictionary<string, int>();

			//lol
			var groupRegions = new Dictionary<string, Dictionary<string, int>>();

			foreach (string espPath in espPaths) {
				JArray esp = JArray.Parse(File.ReadAllText(espPath));

				//gather cells
				for (int i = 0; i < esp.Count; i++) {
					var form = esp[i];
					if (form.Str("type") != "Cell") continue;
					(int id, string name, bool isInterior, HashSet<string> regions, string group, string subgroup) cell
						= (i, form.Str("name"), form["data"].Str("flags").IndexOf("IS_INTERIOR") != -1, new HashSet<string>(), "", "");
					if (cell.isInterior) {
						if (((JArray)form["references"]).Count == 0) continue;
						string[] words = cell.name.Split(',', ':');

						

						cell.group = GetCellGroup(cell.name);
                        if (!groupCounts.ContainsKey(cell.group)) groupCounts[cell.group] = 1;
						else groupCounts[cell.group]++;

						if (words.Length > 1) {
							cell.subgroup = words[0] + "/" + words[1].Trim();
							if (!subgroupCounts.ContainsKey(cell.subgroup)) subgroupCounts[cell.subgroup] = 1;
							else subgroupCounts[cell.subgroup]++;
						}



						interiors[cell.name] = cell;

					} else {
						cell.regions.Add(form["region"] == null ? "NO_REGION" : regionNames[form.Str("region")]);
						exteriors.Add(cell);
					}

				}

				//ext->int connections
				foreach (var exterior in exteriors) {
					var form = esp[exterior.id];

					foreach (JObject reference in (JArray)form["references"]) {
						if (reference["destination"] == null) continue;
						string destination = reference["destination"].Str("cell");
						if (!interiors.ContainsKey(destination)) continue;
						var interior = interiors[destination];

						string region = exterior.regions.FirstOrDefault();
						interior.regions.Add(region);
						interiorRegionsFrontier.Add(destination);

						//group region counts
						string group = groupCounts.ContainsKey(destination) ? destination : interior.group;
						if (!groupRegions.ContainsKey(group)) groupRegions[group] = new Dictionary<string, int>();
						if (!groupRegions[group].ContainsKey(region)) groupRegions[group][region] = 0;
						groupRegions[group][region]++;
					}
				}

				//TODO THIS IS WRONG NEED PROPER TREE SEARCH?
				while (interiorRegionsFrontier.Count > 0) {
					//Console.WriteLine($"FRONTIER LENGTH {interiorRegionsFrontier.Count}");
					foreach (var frontierCellName in interiorRegionsFrontier) {
						var interior = interiors[frontierCellName];
						var form = esp[interior.id];
						foreach (JObject reference in (JArray)form["references"]) {
							if (reference["destination"] == null) continue;
							string destination = reference["destination"].Str("cell");

							if (!interiors.ContainsKey(destination)) continue;
							if (interiorRegionsPropagated.Contains(destination)) continue;

							var destinationInterior = interiors[destination];
							foreach (var region in interior.regions) destinationInterior.regions.Add(region);
							interiorRegionsFrontierNew.Add(destination);
							interiorRegionsPropagated.Add(frontierCellName);
						}
					}
					interiorRegionsFrontier = new HashSet<string>(interiorRegionsFrontierNew);
					interiorRegionsFrontierNew.Clear();
				}
			}


			//foreach (string cellName in interiors.Keys) {
			//	StringBuilder s = new StringBuilder();
			//	s.Append(cellName); s.Append('@');
			//	var interior = interiors[cellName];
			//	if (interior.regions.Count > 0) {
			//		foreach (string region in interior.regions) {
			//			s.Append(region); s.Append(", ");
			//		}
			//		s.Remove(s.Length - 2, 2);
			//		s.Append('@');
			//		foreach (string region in interior.regions) {
			//			s.Append(regionDistricts[region]); s.Append(", ");
			//		}
			//                 s.Remove(s.Length - 2, 2);
			//             } else s.Append('@');


			//             s.Append('@'); s.Append(groupCounts[interior.group]);
			//	s.Append('@'); s.Append(interior.group);
			//	if(interior.subgroup != "" && subgroupCounts[interior.subgroup] > 1) {
			//		s.Append('@'); s.Append(interior.subgroup);
			//	}

			//             Console.WriteLine(s.ToString());
			//}
			//Console.WriteLine();

			foreach (string group in groupCounts.Keys) {
				StringBuilder s = new StringBuilder(group);
				s.Append('@');
				s.Append(groupCounts[group]);
				s.Append("@");
				if (groupRegions.ContainsKey(group)) {
					if (groupRegions[group].Count > 1) {
						foreach (string region in groupRegions[group].Keys) {
							s.Append($"({region})x{groupRegions[group][region]}, ");
						}
						s.Remove(s.Length - 2, 2);

					} else {
						s.Append(groupRegions[group].FirstOrDefault().Key);
					}
					s.Append('@');
					s.Append(regionDistricts[groupRegions[group].Keys.FirstOrDefault()]);
				}
				Console.WriteLine(s.ToString());
			}
		}


		public static void UESPQuestRead(string file = @"C:\Users\a\Downloads\UESPWiki-20260312044642.xml") {
			System.Xml.XmlDocument doc = new System.Xml.XmlDocument();
			doc.LoadXml(File.ReadAllText(file));
			foreach (System.Xml.XmlNode page in doc.DocumentElement.ChildNodes) {
				if (page.Name != "page") continue;
				string pageName = page["title"].InnerText;
				if (pageName.StartsWith("Category:")) continue;
				string location = "NO LOCATION";
				string type = "NO TYPE";
				string id = "NO ID";
				string giver = "NO GIVER";
				string desc = "NO DESC";
				string text = page["revision"]["text"].InnerText;
				if (text.StartsWith("#REDIRECT")) continue;

				string[] lines = text.Split('\n');
				foreach (string line in lines) {
					if (line.StartsWith("|Loc=")) location = line.Substring(5);
					else if (line.StartsWith("|ID=")) id = line.Substring(4);
					else if (line.StartsWith("|Giver=")) giver = line.Substring(7);
					else if (line.StartsWith("|type=")) type = line.Substring(6);
					else if (line.StartsWith("|description=")) desc = line.Substring(13);
				}
				Console.WriteLine($"{pageName}@{type}@{id}@{giver}@{location}@{desc}");
			}
		}







		public static void ListRegions(params string[] espPaths) {
			Dictionary<string, string> regions = new Dictionary<string, string>();

			foreach (string espPath in espPaths) {
				Console.WriteLine(espPath);
				JArray esp = JArray.Parse(File.ReadAllText(espPath));
				for (int i = 0; i < esp.Count; i++) {
					var entry = esp[i];
					if (entry["type"] != null && entry.Str("type") == "Region") {
						regions[entry.Str("id")] = entry.Str("name");
					}
				}
			}

			foreach (string region in regions.Keys) {
				Console.WriteLine($"{region}@{regions[region]}");
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

				if (type == "DialogueInfo") {
					inTopic = true;
					if (rec["quest_state"] != null && rec["quest_state"]["type"].Str() == "Name") {
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
					if (inTopic) { //ended list of topic infos
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



						string cellName = rec["name"].Value<string>();

						if (cellName == "" && rec["region"] != null) cellName = rec["region"].Value<string>();

						if (rec["data"]["flags"].Value<string>().IndexOf("IS_INTERIOR") != -1) {
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
				foreach (MW.Info info in questInfos[quest]) {
					foreach (MW.QuestStage questStage in info.quests) {
						if (questStage.id != quest) continue;

						string location = npcCells.ContainsKey(info.speaker) ? npcCells[info.speaker] : "unknown location";

						Console.WriteLine($"{quest}|{questName}|{questStage.stage}|{info.speaker}|{location}|{info.playerFaction}|{info.playerRank}|{info.topic}|{info.miscFilters}");
						break;
					}
				}
			}
		}















		class Quest {
			public string name;
			public string nameHistory;
			public string added;
			public string removed;
		}

		public static void MWQuestHistory() {

			Dictionary<string, Quest> quests = new Dictionary<string, Quest>();

			foreach (string path in Directory.EnumerateFiles(@"E:\Extracted\Morrowind\trhistory", "*.txt")) {
				string version = Path.GetFileName(path).Substring(0, 5);
				HashSet<string> questCheck = new HashSet<string>(quests.Keys);

				foreach (string line in File.ReadAllLines(path)) {
					string[] words = line.Split('|');
					string id = words[0];
					questCheck.Remove(id);
					string name = words[1];
					if (!quests.ContainsKey(id)) {
						Quest q = new Quest();
						q.name = name;
						q.added = version;
						q.nameHistory = "";
						q.removed = "";
						quests[id] = q;
					} else {
						var q = quests[id];
						if (q.name != name) {
							if (q.name.ToLower() != name.ToLower()) q.nameHistory += $"{q.name} (until {version})";
							q.name = name;
						}
						if (q.removed != "") q.removed = "";
					}
					foreach (string missingQuest in questCheck) {
						if (quests[missingQuest].removed == "") quests[missingQuest].removed = version;
					}
				}



			}

			foreach (string id in quests.Keys) {
				var q = quests[id];
				int i = q.name.IndexOf(':');
				string category = i == -1 ? "" : q.name.Substring(0, i);
				//if(q.added == "22.11" || q.removed == "22.11")
				Console.WriteLine($"{q.added}|{q.removed}|{id}|{category}|{q.name}|{q.nameHistory}");
			}
		}

		public static void MWQuests(params string[] espPaths) {
			string unnamed = "Unnamed";

			Dictionary<string, string> questNames = new Dictionary<string, string>();
			Dictionary<string, string> questFiles = new Dictionary<string, string>();

			foreach (string espPath in espPaths) {
				string file = Path.GetFileNameWithoutExtension(espPath);
				Console.WriteLine(espPath);
				JArray esp = JArray.Parse(File.ReadAllText(espPath));
				int i = 0;
				while (i < esp.Count) {
					if (esp[i]["type"] != null && esp[i]["type"].Value<string>() == "Dialogue") {
						string type = esp[i]["dialogue_type"].Value<string>();
						if (type == "Journal") {
							string id = esp[i]["id"].Value<string>();
							if (!questFiles.ContainsKey(id)) questFiles[id] = file;
							if (!questNames.ContainsKey(id) || questNames[id] == unnamed) {
								string name = unnamed;
								string zero = null;
								string name2 = null;
								i++;
								while (esp[i]["type"] != null && esp[i]["type"].Value<string>() == "DialogueInfo") {
									if (esp[i]["data"]["disposition"].Value<int>() == 0) {
										zero = esp[i]["text"].Value<string>();
										if (esp[i]["quest_name"] != null) name = esp[i]["text"].Value<string>();
									} else if (esp[i]["quest_name"] != null) name2 = esp[i]["text"].Value<string>();
									i++;
								}
								if (name == unnamed && zero != null) questNames[id] = zero;
								else if (name == unnamed && name2 != null) questNames[id] = name2;
								else questNames[id] = name;
								i--;
							}
						}
					}
					i++;
				}
			}
			using (TextWriter w = new StreamWriter(File.Open(espPaths[0] + "_quests.txt", FileMode.Create))) {
				foreach (string quest in questNames.Keys) w.WriteLine(quest + "|" + questNames[quest]);
			}


		}








	}

}
