using System;
using System.IO;


namespace Util {
    public static class BinaryReaderEx {
        public static uint AssertUint32(this BinaryReader r, uint comp) {
            uint val = r.ReadUInt32();
            if (val != comp) Console.WriteLine($"ASSERTION FAILED {val} !+ {comp}");
            return val;
        }

        public static string ReadSTDString(this BinaryReader r) {
            string s = new string(r.ReadChars(r.ReadUInt16()));
            r.ReadByte();
            return s;
        }

        public static void Seek(this BinaryReader r, int i) {
            r.BaseStream.Seek(i, SeekOrigin.Current);
        }

        public static uint ReadUInt32B(this BinaryReader r) {
            byte[] b = r.ReadBytes(4);
            return (uint)(b[3] | (b[2] << 8) | (b[1] << 16) | (b[0] << 24));
        }

        public static ushort ReadUInt16B(this BinaryReader r) {
            byte[] b = r.ReadBytes(2);
            return (ushort)(b[1] | (b[0] << 8));
        }
    }
}
