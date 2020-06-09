using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Diagnostics;
using Luna.Runner;

namespace Luna {
    class Program {
        static void Main(string[] args) {
            // Initalize definitions
            InstructionDefinition.Initalize();
            FunctionDefinition.Initalize();

            // Load game
            IFF _wad = new IFF(@"E:\Luna\Sample\LunaBuiltin.win", new Game());
            _wad.Parse(delegate (Game _game) {
                _game.Initalize(true);
            });
            Console.ReadKey();
        }
    }

    public static class Extensions {
        public static bool ReadLBoolean(this BinaryReader _reader) {
            return (_reader.ReadInt32() == 1 ? true : false);
        }
    }
}
