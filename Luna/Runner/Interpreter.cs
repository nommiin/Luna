using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Luna {
    public enum LOperandType {
        Error = 15,
        Double = 0,
        Single,
        Integer,
        Long,
        Boolean,
        Variable,
        String,
        Instance,
        Delete,
        Undefined,
        UnsignedInteger
    }

    public enum LVariableType {
        Global = -5,
        Instance = -1,
        Local = -7,
        Static = -16,
        Unknown = -6
    }

    class Interpreter {
        public delegate void Instruction();
        public delegate void Function();

        public static Dictionary<LOpcode, Instruction> Handlers = new Dictionary<LOpcode, Instruction>() {
            {LOpcode.b, () => {
                Console.WriteLine("DO NOTHING");
            }}
        };

        public static Dictionary<string, Function> Functions = new Dictionary<string, Function>() {
            {"show_debug_message", () => {
                Console.WriteLine("ABC");   
            }}
        };

        public Interpreter(Game _game) {
            
        }
    }
}
