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

        public delegate void FunctionHandler(Interpreter _vm, Int32 _count, Stack<LValue> _stack);
        public static Dictionary<string, FunctionHandler> Functions = new Dictionary<string, FunctionHandler>();

        public Interpreter(Game _game) {
            // Load runner functions via reflection
            MethodInfo[] _functionHandlers = typeof(FunctionHandlers).GetMethods(BindingFlags.Public | BindingFlags.Static);
            for (int i = 0; i < _functionHandlers.Length; i++) {
                MethodInfo _functionGet = _functionHandlers[i];
                Functions.Add(_functionGet.GetCustomAttribute<FunctionDefinition>().Name, (FunctionHandler)Delegate.CreateDelegate(typeof(FunctionHandler), _functionGet));
            }

            // Setup defaults
            this.Environment = new Domain();
            this.Timer = Stopwatch.StartNew();
            this.Data = _game;
        }

        public void ExecuteScript(LInstance _scope, string _script) {
            this.ExecuteScript(_scope, this.Data.Code[_script]);
        }

        public void ExecuteScript(LInstance _scope, LCode _code) {
            this.Environment.Scope = _scope;
            for(this.ProgramCounter = 0; this.ProgramCounter < _code.InstructionList.Count; this.ProgramCounter++) {
                //Console.WriteLine("PC: {0} - {1}", this.ProgramCounter, _code.InstructionList[this.ProgramCounter].Opcode);
                _code.InstructionList[this.ProgramCounter].Perform(this, this.Environment, this.Environment.Stack);
            }

#if (DEBUG)
            Console.WriteLine("Instance Count: {0}", this.Data.Instances.Count);
            foreach(KeyValuePair<Int32, LInstance> _i in this.Data.Instances) {
                Console.WriteLine("- Instance: {0} (Count: {1})", _i.Key, _i.Value.Variables.Count);
                foreach(KeyValuePair<string, LValue> _v in _i.Value.Variables) {
                    Console.WriteLine("  - {0} = {1}", _v.Key, _v.Value.Convert(LType.String).Value);
                }
            }
            Console.Write("\n");

            Console.WriteLine("Local Variables: (Count: {0})", this.Environment.LocalVariables.Count);
            foreach(KeyValuePair<string, LValue> _v in this.Environment.LocalVariables) {
                Console.WriteLine("- {0} = {1}", _v.Key, _v.Value.Convert(LType.String).Value);
            }
           /*
            Console.WriteLine("Global Variables: ({0})", this.GlobalVariables.Count);
            foreach (KeyValuePair<string, LValue> _g in this.GlobalVariables) {
                Console.WriteLine("- {0} = {1}", _g.Key, _g.Value.Convert(LType.String).Value);
            }

            Console.WriteLine("Static Variables: ({0})", this.StaticVariables.Count);
            foreach (KeyValuePair<string, LValue> _g in this.StaticVariables) {
                Console.WriteLine("- {0} = {1}", _g.Key, _g.Value.Convert(LType.String).Value);
            }

            Console.WriteLine("Instance Variables: ({0})", this.InstanceVariables.Count);
            foreach (KeyValuePair<LVariableScope, Dictionary<string, LValue>> _g in this.InstanceVariables) {
                Console.WriteLine("- {0}", _g.Key);
                foreach (KeyValuePair<string, LValue> _i in _g.Value) {
                    Console.WriteLine("   - {0} = {1}", _i.Key, _i.Value.Convert(LType.String).Value);
                }
            }

            Console.WriteLine("Local Variables: ({0})", this.Environment.LocalVariables.Count);
            foreach (KeyValuePair<string, LValue> _g in this.Environment.LocalVariables) {
                Console.WriteLine("- {0} = {1}", _g.Key, _g.Value.Convert(LType.String).Value);
            }*/
#endif
        }
    }
}
