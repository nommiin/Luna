//#define PRINT_EXEC

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Reflection;
using Luna.Assets;
using Luna.Types;
using System.Diagnostics;

namespace Luna.Runner {
    public enum LArgumentType {
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

    public enum LVariableScope {
        Global = -5,
        Instance = -1,
        Local = -7,
        Static = -16,
        Unknown = -6
    }

    public enum LConditionType {
        None,
        LessThan,
        LessEqual,
        Equal,
        NotEqual,
        GreaterEqual,
        GreaterThan
    }

    class Interpreter {
        public delegate void InstructionHandler(Interpreter _vm, LCode _code, BinaryReader _reader, Instruction _inst);
        public delegate void FunctionHandler(Interpreter _vm, Int32 _count);

        public static Dictionary<string, FunctionHandler> Functions = new Dictionary<string, FunctionHandler>();
        public static Dictionary<LOpcode, InstructionHandler> Instructions = new Dictionary<LOpcode, InstructionHandler>() {
            { LOpcode.pushi, InstructionHandlers.DoPushImmediate },
            { LOpcode.push, InstructionHandlers.DoPush },
            { LOpcode.pushl, InstructionHandlers.DoPush },
            { LOpcode.pushg, InstructionHandlers.DoPushGlobal },
            { LOpcode.pushb, InstructionHandlers.DoPushBuiltin },
            { LOpcode.pop, InstructionHandlers.DoPop },
            { LOpcode.set, InstructionHandlers.DoConditional },
            { LOpcode.bf, InstructionHandlers.DoBranchFalse },
            { LOpcode.bt, InstructionHandlers.DoBranchTrue },
            { LOpcode.b, InstructionHandlers.DoBranch },
            { LOpcode.popz, InstructionHandlers.DoDiscard },
            { LOpcode.dup, InstructionHandlers.DoDuplicate },
            { LOpcode.conv, InstructionHandlers.DoConvert },
            { LOpcode.call, InstructionHandlers.DoCall },
            { LOpcode.add, InstructionHandlers.DoAdd },
            { LOpcode.sub, InstructionHandlers.DoSubtract },
            { LOpcode.mul, InstructionHandlers.DoMultiply },
            { LOpcode.div, InstructionHandlers.DoDivide },
            { LOpcode.rem, InstructionHandlers.DoRemainder },
            { LOpcode.mod, InstructionHandlers.DoModulo },
            { LOpcode.xor, InstructionHandlers.DoXor },
            { LOpcode.and, InstructionHandlers.DoAnd },
            { LOpcode.or, InstructionHandlers.DoOr },
            { LOpcode.shl, InstructionHandlers.DoShiftLeft },
            { LOpcode.shr, InstructionHandlers.DoShiftRight }
        };

        public LValue GetVariable(LVariableScope _scope, string _name) {
            return this.Variables[_scope].First(x => x.Key.Name == _name).Value;
        }

        public LValue GetVariable(LVariable _var) {
            return this.Variables[_var.Scope][_var];
        }

        public void SetVariable(LVariable _var, LValue _value) {
#if (PRINT_EXEC)
            Console.WriteLine("SetVariable({0}) = {1}", _var.Name, _value.Value);
#endif
            this.Variables[_var.Scope][_var] = _value;
        }

        public Int32 Count;
        public Game Data;
        public Stopwatch Timer = new Stopwatch();
        public Stack<LValue> Stack = new Stack<LValue>();
        public Dictionary<LVariableScope, Dictionary<LVariable, LValue>> Variables = new Dictionary<LVariableScope, Dictionary<LVariable, LValue>>();
        public Interpreter(Game _game) {
            /*MemberInfo[] _functionHandlers = FunctionHandlers.GetMembers(BindingFlags.Static);
            for(int i = 0; i < _functionHandlers.Length; i++) {
                Console.WriteLine(_functionHandlers[i].Name);
            }*/
            MethodInfo[] _functionHandlers = typeof(FunctionHandlers).GetMethods(BindingFlags.Public | BindingFlags.Static);
            for (int i = 0; i < _functionHandlers.Length; i++) {
                string _functionName = _functionHandlers[i].Name;
                while (_functionName[0] == '_') _functionName = _functionName.Substring(1);
                Functions.Add(_functionName, (FunctionHandler)Delegate.CreateDelegate(typeof(FunctionHandler), _functionHandlers[i]));
            }

            foreach (LVariableScope _type in Enum.GetValues(typeof(LVariableScope))) {
                this.Variables[_type] = new Dictionary<LVariable, LValue>();
            }

            /*
            [WARNING] The opcode "popv" is not implemented.
[WARNING] The opcode "neg" is not implemented.
[WARNING] The opcode "not" is not implemented.
[WARNING] The opcode "pushv" is not implemented.
[WARNING] The opcode "callv" is not implemented.
[WARNING] The opcode "ret" is not implemented.
[WARNING] The opcode "exit" is not implemented.
[WARNING] The opcode "pushenv" is not implemented.
[WARNING] The opcode "popenv" is not implemented.
[WARNING] The opcode "pushg" is not implemented.
[WARNING] The opcode "pushb" is not implemented.
[WARNING] The opcode "brk" is not implemented.
[WARNING] The opcode "unknown" is not implemented.
    */
            this.Data = _game;
        }

        public void ExecuteScript(string _script) {
#if (DEBUG == true)
            if (this.Data.Code.ContainsKey(_script) == false) {
                throw new Exception(String.Format("Could not execute script named \"{0}\"", _script));
            }
#endif
            this.Count = 0;
            this.ExecuteScript(this.Data.Code[_script]);
        }

        public void ExecuteScript(LCode _code) {
            BinaryReader _codeReader = _code.Reader;

            while (_codeReader.BaseStream.Position < _codeReader.BaseStream.Length) {
                Instruction _instGet = Instruction.Decode(_codeReader.ReadInt32());
#if (DEBUG == true)
                if (Instructions.ContainsKey(_instGet.Opcode) == false) {
                    throw new Exception(String.Format("Could not process unimplemented opcode: \"{0}\" at instruction {1} ({2} bytes)", _instGet.Opcode, this.Count, _codeReader.BaseStream.Position));
                }
#endif
                Instructions[_instGet.Opcode](this, _code, _codeReader, _instGet);
                this.Count++;
            }

#if (DEBUG == true)
            Console.WriteLine("\nVariables:");
            foreach(KeyValuePair<LVariableScope, Dictionary<LVariable, LValue>> _v in this.Variables) {
                foreach(KeyValuePair<LVariable, LValue> _vv in _v.Value) {
                    Console.WriteLine("{0}.{1} = {2} ({3})", _vv.Key.Scope, _vv.Key.Name, _vv.Value.Value, _vv.Value.Type);
                }
                //Console.WriteLine("{0}.{1} = {2}", _v.Key.Type, _v.Key.Name, _v.Value);
            }
#endif
        }
    }
}
