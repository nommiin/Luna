using System;
using System.Windows.Forms;
using System.IO;
using Luna.Runner;

namespace Luna {
    class Program {
        public static IFF GameData = null;
        public static string[] Arguments;
        public static string GameLocation = null;

        static void Main(string[] args) {
            // Initalize definitions
            InstructionDefinition.Initalize();
            FunctionDefinition.Initalize();

            // Parse arguments
            Program.Arguments = GetArguments(args);
            for(int i = 0; i < Program.Arguments.Length; i++) {
                switch (Program.Arguments[i]) {
                    case "-game": {
                        Program.GameLocation = Program.Arguments[++i];
                        break;
                    }
                }
            }
            if (Program.GameLocation == null) Program.GameLocation = Path.GetDirectoryName(Program.Arguments[0]) + "\\data.win";

            // Check existence
            if (File.Exists(Program.GameLocation) == false) {
                // NOTE: Runner.exe will display a file selection dialogue if no -game argument is provided
                //       but I don't really think we need to replicate the same behaviour.
                MessageBox.Show("Could not find specified game file: \"" + Program.GameLocation + "\"", "An error has occured", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            // Load game
            Program.GameData = new IFF(Program.GameLocation, new Game());
            Program.GameData.Parse(delegate (Game _game) {
                _game.Initalize(false);
            });
        }

        static string[] GetArguments(string[] args) {
            string[] _argumentList = new string[args.Length + 1];
            _argumentList[0] = System.Reflection.Assembly.GetEntryAssembly().Location; // first argument is always runner
            for (int i = 0; i < args.Length; i++) _argumentList[i + 1] = args[i];
            return _argumentList;
        }
    }

    public static class Extensions {
        public static bool ReadLBoolean(this BinaryReader _reader) {
            return (_reader.ReadInt32() == 1 ? true : false);
        }
    }
}
