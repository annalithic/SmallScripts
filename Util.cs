using Newtonsoft.Json.Linq;

namespace SmallScripts {
    public static class Util {
        public static int Int(this JToken obj) { return obj.Value<int>(); }
        public static float Float(this JToken obj) { return obj.Value<float>(); }

        public static string Str(this JToken obj) { return obj.Value<string>(); }
    }
}
