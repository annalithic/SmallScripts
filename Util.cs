using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Text;

namespace SmallScripts {
    public static class Util {
        public static int Int(this JToken obj) { return obj.Value<int>(); }
        public static float Float(this JToken obj) { return obj.Value<float>(); }

        public static string Str(this JToken obj, string s) { return obj[s].Value<string>(); }

        public static string Str(this JToken obj) { return obj.Value<string>(); }

        public static string[] SplitQuotes(this string s, params char[] extraSplitChars) {
            HashSet<char> splitChars = new HashSet<char>() { ' ', '\t' };
            if (extraSplitChars != null) foreach (var c in extraSplitChars) splitChars.Add(c);
            if (s.Length == 0) return new string[0];

            bool inQuotes = false;
            bool inWord = true;
            List<string> words = new List<string>();
            StringBuilder builder = new StringBuilder();
            for (int i = 0; i < s.Length; i++) {
                if (splitChars.Contains(s[i]) && !inQuotes) {
                    if (inWord) {
                        inWord = false;
                        words.Add(builder.ToString());
                        builder.Clear();
                    } else continue;
                } else if (s[i] == '"') {
                    if (!inQuotes && inWord) {
                        inWord = false;
                        words.Add(builder.ToString());
                        builder.Clear();
                    }
                    inQuotes = !inQuotes;
                } else {
                    inWord = true;
                    builder.Append(s[i]);
                }
            }
            if (inWord) words.Add(builder.ToString());
            return words.ToArray();
        }
    }
}
