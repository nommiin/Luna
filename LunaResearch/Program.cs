using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace LunaResearch {
    class Program {
        static void Main(string[] args) {
            Save.Load(Environment.GetEnvironmentVariable("localappdata") + @"\LunaTest\lol.bin");
            Console.ReadKey();
        }
    }

    static class Save {
        public static void Load(string _file) {
            using (BinaryReader _reader = new BinaryReader(File.OpenRead(_file))) {
                Console.WriteLine("Size: {0} bytes", _reader.BaseStream.Length);

                Console.WriteLine("GameID: {0}", _reader.ReadInt32());
            }
        }
    }
}
