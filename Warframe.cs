using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;

namespace SmallScripts.Warframe {

    struct Entry {

        public long offset;
        DateTime fileTime;
        uint sizeCompressed;
        uint sizeUncompressed;
        uint reserved;
        public int parentID;
        public string fileName;

        public Entry(BinaryReader r) {
            offset = r.ReadInt64();
            long date = r.ReadInt64();
            fileTime = date == -1 ? DateTime.MinValue : DateTime.FromFileTime(date);
            sizeCompressed = r.ReadUInt32();
            sizeUncompressed = r.ReadUInt32();
            reserved = r.ReadUInt32();
            parentID = r.ReadInt32();
            fileName = new string(r.ReadChars(64)).TrimEnd('\0');
        }
    }

    class TOC {
        uint version;
        List<Entry> entries;
        List<string> directories;

        public TOC(string path) {
            using (BinaryReader reader = new BinaryReader(File.OpenRead(path))) {
                if (reader.ReadUInt32() != 409454158) { Console.WriteLine("WRONG MAGIC"); return; }
                version = reader.ReadUInt32();
                entries = new List<Entry>();
                directories = new List<string>(); directories.Add("");
                while(reader.BaseStream.Position < reader.BaseStream.Length) {
                    entries.Add(new Entry(reader));
                    if(entries[entries.Count - 1].offset == -1) {
                        directories.Add(directories[entries[entries.Count - 1].parentID] + entries[entries.Count - 1].fileName + "/");
                    }
                }
            }
        }

        public void Print() {
            for(int i = 0; i < entries.Count; i++) {
                Console.WriteLine(directories[entries[i].parentID] + entries[i].fileName);
                //if (entries[i].parentID == 0) PrintRecursive(i, "");
            }
        }

        public void Print(string path) {
            if (!Directory.Exists(Path.GetDirectoryName(path))) Directory.CreateDirectory(Path.GetDirectoryName(path));
            using(TextWriter writer = new StreamWriter(File.Open(path, FileMode.Create))) {
                for (int i = 0; i < entries.Count; i++) {
                    writer.WriteLine(directories[entries[i].parentID] + entries[i].fileName);
                    //if (entries[i].parentID == 0) PrintRecursive(i, "");
                }
            }

        }

        void PrintRecursive(int i, string indent) {
            Console.WriteLine(indent + entries[i].fileName);
            //for (int y = 0; y < children[i].Count; y++) PrintRecursive(children[i][y], indent + "  ");
        }
    }

    
}
