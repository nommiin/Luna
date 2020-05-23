using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Luna.Assets;
using Luna.Types;

namespace Luna {
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
        GreaterThan,
        GreaterEqual
    }

    class Interpreter {
        public delegate void InstructionHandler(Interpreter _vm, LCode _code, BinaryReader _reader, Instruction _inst);
        public delegate void FunctionHandler(Interpreter _vm, Int32 _count);

        public static Dictionary<string, FunctionHandler> Functions = new Dictionary<string, FunctionHandler>() {
            {"show_debug_message", (Interpreter _vm, Int32 _count) => {
                Console.WriteLine(_vm.Stack.Pop().Value);
            }},
            {"string", (Interpreter _vm, Int32 _count) => {
                _vm.Stack.Push(_vm.Stack.Pop().Convert(LType.String));
            }},
            {"real", (Interpreter _vm, Int32 _count) => {
                _vm.Stack.Push(_vm.Stack.Pop().Convert(LType.Number));
            }}
        };

        public static Dictionary<LOpcode, InstructionHandler> Instructions = new Dictionary<LOpcode, InstructionHandler>() {
            {LOpcode.pushi, delegate (Interpreter _vm, LCode _code, BinaryReader _reader, Instruction _inst) {
                _vm.Stack.Push(new LValue(LType.Number, _inst.Data));
            }},
            {LOpcode.push, delegate (Interpreter _vm, LCode _code, BinaryReader _reader, Instruction _inst) {
                LArgumentType _argType = (LArgumentType)_inst.Argument;
                switch (_argType) {
                    case LArgumentType.String: {
                        _vm.Stack.Push(new LValue(LType.String, _vm.Data.StringMapping[_reader.ReadInt32()].Value));
                        break;
                    }

                    case LArgumentType.Variable: {
                        LVariable _varGet = _vm.Data.Variables[_vm.Data.VariableMapping[(int)((_code.Base + _reader.BaseStream.Position)) - 4]];
                        _vm.Stack.Push(_vm.GetVariable(_varGet));
                        _reader.BaseStream.Seek(sizeof(Int32), SeekOrigin.Current);
                        break;
                    }

                    default: {
                        throw new Exception(String.Format("Could not push unimplemented type: \"{0}\"", _argType));
                    }
                }
            }},
            /*{LOpcode.pushl, delegate (Interpreter _vm, LCode _code, BinaryReader _reader, Instruction _inst) {
                // ?!
            }},*/
            {LOpcode.pop, delegate (Interpreter _vm, LCode _code, BinaryReader _reader, Instruction _inst) {
                LArgumentType _argFrom = (LArgumentType)((_inst.Argument >> 4) & 0xF), _argTo = (LArgumentType)(_inst.Argument & 0xF);
                switch (_argTo) {
                    case LArgumentType.Variable: {
                        LVariable _varGet = _vm.Data.Variables[_vm.Data.VariableMapping[(int)(_code.Base + _reader.BaseStream.Position) - 4]];
                        _vm.SetVariable(_varGet, _vm.Stack.Pop());
                        _reader.BaseStream.Seek(sizeof(Int32), SeekOrigin.Current);
                        break;
                    }

                    default: {
                        throw new Exception(String.Format("Could not pop unimplemented type: \"{0}\"", _argTo));
                    }
                }
            }},
            {LOpcode.set, delegate (Interpreter _vm, LCode _code, BinaryReader _reader, Instruction _inst) {
                LValue _valRight = _vm.Stack.Pop();
                LValue _valLeft = _vm.Stack.Pop();
                switch ((LConditionType)((_inst.Data >> 8) & 0xFF)) {
                    case LConditionType.Equal:        _vm.Stack.Push(_valLeft == _valRight); break;
                    case LConditionType.NotEqual:     _vm.Stack.Push(_valLeft != _valRight); break;
                    case LConditionType.LessThan:     _vm.Stack.Push(_valLeft < _valRight);  break;
                    case LConditionType.LessEqual:    _vm.Stack.Push(_valLeft < _valRight);  break;
                    case LConditionType.GreaterThan:  _vm.Stack.Push(_valLeft > _valRight);  break;
                    case LConditionType.GreaterEqual: _vm.Stack.Push(_valLeft > _valRight);  break;
                    default: {
                        throw new Exception(String.Format("Could not process conditional type: \"{0}\"", _inst.Data));
                    }
                }
            }},
            {LOpcode.bf, delegate (Interpreter _vm, LCode _code, BinaryReader _reader, Instruction _inst) {
                if (_vm.Stack.Pop().Value == 0) {
                    _reader.BaseStream.Seek((_reader.BaseStream.Position - 4) + (_inst.Raw << 9 >> 7), SeekOrigin.Begin);
                }
            }},
            {LOpcode.bt, delegate (Interpreter _vm, LCode _code, BinaryReader _reader, Instruction _inst) {
                if (_vm.Stack.Pop().Value == 1) {
                    _reader.BaseStream.Seek((_reader.BaseStream.Position - 4) + (_inst.Raw << 9 >> 7), SeekOrigin.Begin);
                }
            }},
            {LOpcode.b, delegate (Interpreter _vm, LCode _code, BinaryReader _reader, Instruction _inst) {
                _reader.BaseStream.Seek((_reader.BaseStream.Position - 4) + (_inst.Raw << 9 >> 7), SeekOrigin.Begin);
            }},
            /*{LOpcode.conv, delegate (Interpreter _vm, LCode _code, BinaryReader _reader, Instruction _inst) {
                // ?!
            }},*/
            /*{LOpcode.popz, delegate (Interpreter _vm, LCode _code, BinaryReader _reader, Instruction _inst) {
                // ?!
            }},*/
            {LOpcode.call, delegate (Interpreter _vm, LCode _code, BinaryReader _reader, Instruction _inst) {
                LFunction _funcGet = _vm.Data.Functions[_vm.Data.FunctionMapping[(int)((_code.Base + _reader.BaseStream.Position))]];
                if (Functions.ContainsKey(_funcGet.Name) == true) {
                    Functions[_funcGet.Name](_vm, _inst.Data);
                } else {
                    throw new Exception(String.Format("Could not execute function named \"{0}\" at instruction {1}", _funcGet.Name, _vm.Count));
                }
                _reader.BaseStream.Seek(sizeof(Int32), SeekOrigin.Current);
            }},
            {LOpcode.add, delegate (Interpreter _vm, LCode _code, BinaryReader _reader, Instruction _inst) {
                LValue _valRight = _vm.Stack.Pop();
                LValue _valLeft = _vm.Stack.Pop();
                _vm.Stack.Push(_valLeft + _valRight);
            }},
            {LOpcode.sub, delegate (Interpreter _vm, LCode _code, BinaryReader _reader, Instruction _inst) {
                LValue _valRight = _vm.Stack.Pop();
                LValue _valLeft = _vm.Stack.Pop();
                _vm.Stack.Push(_valLeft - _valRight);
            }},
            {LOpcode.mul, delegate (Interpreter _vm, LCode _code, BinaryReader _reader, Instruction _inst) {
                LValue _valRight = _vm.Stack.Pop();
                LValue _valLeft = _vm.Stack.Pop();
                _vm.Stack.Push(_valLeft * _valRight);
            }},
            {LOpcode.div, delegate (Interpreter _vm, LCode _code, BinaryReader _reader, Instruction _inst) {
                LValue _valRight = _vm.Stack.Pop();
                LValue _valLeft = _vm.Stack.Pop();
                _vm.Stack.Push(_valLeft / _valRight);
            }},
            {LOpcode.rem, delegate (Interpreter _vm, LCode _code, BinaryReader _reader, Instruction _inst) {
                LValue _valRight = _vm.Stack.Pop();
                LValue _valLeft = _vm.Stack.Pop();
                LValue _valResult = Math.Floor(_valLeft.Value / _valRight.Value);
                _vm.Stack.Push(_valResult);
            }},
            {LOpcode.mod, delegate (Interpreter _vm, LCode _code, BinaryReader _reader, Instruction _inst) {
                LValue _valRight = _vm.Stack.Pop();
                LValue _valLeft = _vm.Stack.Pop();
                _vm.Stack.Push(_valLeft % _valRight);
            }},
            {LOpcode.xor, delegate (Interpreter _vm, LCode _code, BinaryReader _reader, Instruction _inst) {
                LValue _valRight = _vm.Stack.Pop();
                LValue _valLeft = _vm.Stack.Pop();
                _vm.Stack.Push(_valLeft ^ _valRight.Value);
            }},
            {LOpcode.and, delegate (Interpreter _vm, LCode _code, BinaryReader _reader, Instruction _inst) {
                LValue _valRight = _vm.Stack.Pop();
                LValue _valLeft = _vm.Stack.Pop();
                _vm.Stack.Push(_valLeft & _valRight.Value);
            }},
            {LOpcode.or, delegate (Interpreter _vm, LCode _code, BinaryReader _reader, Instruction _inst) {
                LValue _valRight = _vm.Stack.Pop();
                LValue _valLeft = _vm.Stack.Pop();
                _vm.Stack.Push(_valLeft | _valRight.Value);
            }},
            {LOpcode.shl, delegate (Interpreter _vm, LCode _code, BinaryReader _reader, Instruction _inst) {
                LValue _valRight = _vm.Stack.Pop();
                LValue _valLeft = _vm.Stack.Pop();
                _vm.Stack.Push(_valLeft << _valRight.Value);
            }},
            {LOpcode.shr, delegate (Interpreter _vm, LCode _code, BinaryReader _reader, Instruction _inst) {
                LValue _valRight = _vm.Stack.Pop();
                LValue _valLeft = _vm.Stack.Pop();
                _vm.Stack.Push(_valLeft >> _valRight.Value);
            }}
        };

        public LValue GetVariable(LVariableScope _scope, string _name) {
            return this.Variables[_scope].First(x => x.Key.Name == _name).Value;
        }

        public LValue GetVariable(LVariable _var) {
            return this.Variables[_var.Scope][_var];
        }

        public void SetVariable(LVariable _var, LValue _value) {
            this.Variables[_var.Scope][_var] = _value;
        }

        public Int32 Count;
        public Game Data;
        public Stack<LValue> Stack = new Stack<LValue>();
        public Dictionary<LVariableScope, Dictionary<LVariable, LValue>> Variables = new Dictionary<LVariableScope, Dictionary<LVariable, LValue>>();
        public Interpreter(Game _game) {
            foreach(LVariableScope _type in Enum.GetValues(typeof(LVariableScope))) {
                this.Variables[_type] = new Dictionary<LVariable, LValue>();
            }
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
