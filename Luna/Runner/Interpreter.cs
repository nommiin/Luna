using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using System.Diagnostics;
using Luna.Assets;
using Luna.Types;
using Luna.Runner;

namespace Luna.Runner {
    class Interpreter {
        public Game Data;
        public Stopwatch Timer;
        public Domain Environment;
        public Int32 ProgramCounter;

        public Dictionary<string, LValue> GlobalVariables = new Dictionary<string, LValue>();
        public Dictionary<string, LValue> StaticVariables = new Dictionary<string, LValue>();
        public Dictionary<LVariableScope, Dictionary<string, LValue>> InstanceVariables = new Dictionary<LVariableScope, Dictionary<string, LValue>>();

        public delegate void FunctionHandler(Stack<LValue> _stack);
        public static Dictionary<string, FunctionHandler> Functions = new Dictionary<string, FunctionHandler>();

        public Interpreter(Game _game) {
            // Load runner functions via reflection
            MethodInfo[] _functionHandlers = typeof(FunctionHandlers).GetMethods(BindingFlags.Public | BindingFlags.Static);
            for (int i = 0; i < _functionHandlers.Length; i++) {
                MethodInfo _functionGet = _functionHandlers[i];
                Functions.Add(_functionGet.GetCustomAttribute<FunctionDefinition>().Name, (FunctionHandler)Delegate.CreateDelegate(typeof(FunctionHandler), _functionHandlers[i]));
            }

            // Setup defaults
            this.Environment = new Domain();
            this.Timer = Stopwatch.StartNew();
            this.Data = _game;
        }

        public void ExecuteScript(string _script) {
            this.ExecuteScript(this.Data.Code[_script]);
        }

        public void ExecuteScript(LCode _code) {
            for(this.ProgramCounter = 0; this.ProgramCounter < _code.InstructionList.Count; this.ProgramCounter++) {
                //Console.WriteLine("PC: {0} - {1}", this.ProgramCounter, _code.InstructionList[this.ProgramCounter].Opcode);
                _code.InstructionList[this.ProgramCounter].Perform(this, this.Environment, this.Environment.Stack);
            }
        }
    }
}
