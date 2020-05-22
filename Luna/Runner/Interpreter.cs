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

    public enum LVariableType {
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
                Console.WriteLine(_vm.Stack.Pop());
            }}
        };

        public static Dictionary<LOpcode, InstructionHandler> Instructions = new Dictionary<LOpcode, InstructionHandler>() {
            {LOpcode.pushi, delegate (Interpreter _vm, LCode _code, BinaryReader _reader, Instruction _inst) {
                _vm.Stack.Push(_inst.Data);
            }},
            {LOpcode.push, delegate (Interpreter _vm, LCode _code, BinaryReader _reader, Instruction _inst) {
                switch ((LArgumentType)_inst.Argument) {
                    case LArgumentType.String: {
                        _vm.Stack.Push(_vm.Data.StringMapping[_reader.ReadInt32()]);
                        break;
                    }
                }
            }},
            {LOpcode.pushl, delegate (Interpreter _vm, LCode _code, BinaryReader _reader, Instruction _inst) {
                LArgumentType _argType = (LArgumentType)_inst.Argument;
                switch (_argType) {
                    case LArgumentType.Variable: {
                        string _varName = _vm.Data.StringMapping[_reader.ReadInt32() & 0xFFFF].Value;
                        _vm.Stack.Push(_vm.GetVariable((LVariableType)(Int16)_inst.Data, _varName));
                        break;
                    }

                    default: {
                        throw new Exception(String.Format("Could not pushl unimplemented type: \"{0}\"", _inst.Argument));
                    }
                }
            }},
            {LOpcode.pop, delegate (Interpreter _vm, LCode _code, BinaryReader _reader, Instruction _inst) {
                LArgumentType _argFrom = (LArgumentType)((_inst.Argument >> 4) & 0xF), _argTo = (LArgumentType)(_inst.Argument & 0xF);
                switch (_argTo) {
                    case LArgumentType.Variable: {
                        LVariable _varGet = _vm.Data.Variables[_vm.Data.VariableMapping[(int)(_code.Base + _reader.BaseStream.Position)]];
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
                dynamic _valRight = _vm.Stack.Pop();
                dynamic _valLeft = _vm.Stack.Pop();
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
                if (_vm.Stack.Pop() == false) {
                    // TODO: BRANCH FALSE
                }
            }},
            {LOpcode.bt, delegate (Interpreter _vm, LCode _code, BinaryReader _reader, Instruction _inst) {
                if (_vm.Stack.Pop() == false) {
                    // TODO: BRANCH TRUE
                }
            }},
            {LOpcode.conv, delegate (Interpreter _vm, LCode _code, BinaryReader _reader, Instruction _inst) {
                // ?!
            }},
            {LOpcode.popz, delegate (Interpreter _vm, LCode _code, BinaryReader _reader, Instruction _inst) {
                // ?!
            }},
            {LOpcode.call, delegate (Interpreter _vm, LCode _code, BinaryReader _reader, Instruction _inst) {
                LFunction _funcGet = _vm.Data.Functions[_vm.Data.FunctionMapping[(int)((_code.Base + _reader.BaseStream.Position) + 4)]];
                Functions[_funcGet.Name](_vm, _inst.Data);
                _reader.BaseStream.Seek(sizeof(Int32), SeekOrigin.Current);
            }},
            {LOpcode.add, delegate (Interpreter _vm, LCode _code, BinaryReader _reader, Instruction _inst) {
                dynamic _valRight = _vm.Stack.Pop();
                dynamic _valLeft = _vm.Stack.Pop();
                _vm.Stack.Push(_valLeft + _valRight);
            }},
            {LOpcode.sub, delegate (Interpreter _vm, LCode _code, BinaryReader _reader, Instruction _inst) {
                dynamic _valRight = _vm.Stack.Pop();
                dynamic _valLeft = _vm.Stack.Pop();
                _vm.Stack.Push(_valLeft - _valRight);
            }},
            {LOpcode.mul, delegate (Interpreter _vm, LCode _code, BinaryReader _reader, Instruction _inst) {
                dynamic _valRight = _vm.Stack.Pop();
                dynamic _valLeft = _vm.Stack.Pop();
                _vm.Stack.Push(_valLeft * _valRight);
            }},
            {LOpcode.div, delegate (Interpreter _vm, LCode _code, BinaryReader _reader, Instruction _inst) {
                dynamic _valRight = _vm.Stack.Pop();
                dynamic _valLeft = _vm.Stack.Pop();
                _vm.Stack.Push(_valLeft / _valRight);
            }},
            {LOpcode.rem, delegate (Interpreter _vm, LCode _code, BinaryReader _reader, Instruction _inst) {
                dynamic _valRight = _vm.Stack.Pop();
                dynamic _valLeft = _vm.Stack.Pop();
                _vm.Stack.Push(Math.Round(_valLeft / _valRight));
            }},
            {LOpcode.mod, delegate (Interpreter _vm, LCode _code, BinaryReader _reader, Instruction _inst) {
                dynamic _valRight = _vm.Stack.Pop();
                dynamic _valLeft = _vm.Stack.Pop();
                _vm.Stack.Push(_valLeft % _valRight);
            }},
            {LOpcode.xor, delegate (Interpreter _vm, LCode _code, BinaryReader _reader, Instruction _inst) {
                dynamic _valRight = _vm.Stack.Pop();
                dynamic _valLeft = _vm.Stack.Pop();
                _vm.Stack.Push(_valLeft ^ _valRight);
            }},
            {LOpcode.and, delegate (Interpreter _vm, LCode _code, BinaryReader _reader, Instruction _inst) {
                dynamic _valRight = _vm.Stack.Pop();
                dynamic _valLeft = _vm.Stack.Pop();
                _vm.Stack.Push(_valLeft & _valRight);
            }},
            {LOpcode.or, delegate (Interpreter _vm, LCode _code, BinaryReader _reader, Instruction _inst) {
                dynamic _valRight = _vm.Stack.Pop();
                dynamic _valLeft = _vm.Stack.Pop();
                _vm.Stack.Push(_valLeft | _valRight);
            }},
            {LOpcode.shl, delegate (Interpreter _vm, LCode _code, BinaryReader _reader, Instruction _inst) {
                dynamic _valRight = _vm.Stack.Pop();
                dynamic _valLeft = _vm.Stack.Pop();
                _vm.Stack.Push(_valLeft << _valRight);
            }},
            {LOpcode.shr, delegate (Interpreter _vm, LCode _code, BinaryReader _reader, Instruction _inst) {
                dynamic _valRight = _vm.Stack.Pop();
                dynamic _valLeft = _vm.Stack.Pop();
                _vm.Stack.Push(_valLeft >> _valRight);
            }}
        };

        public dynamic GetVariable(LVariableType _scope, string _name) {
            return this.Variables[_scope].First(x => x.Key.Name == _name).Value;
        }

        public void SetVariable(LVariable _var, dynamic _value) {
            this.Variables[_var.Type][_var] = _value;
        }

        public Game Data;
        public Stack<dynamic> Stack = new Stack<dynamic>();
        public Dictionary<LVariableType, Dictionary<LVariable, dynamic>> Variables = new Dictionary<LVariableType, Dictionary<LVariable, dynamic>>();
        public Interpreter(Game _game) {
            foreach(LVariableType _type in Enum.GetValues(typeof(LVariableType))) {
                this.Variables[_type] = new Dictionary<LVariable, dynamic>();
            }
            this.Data = _game;
        }

        public void ExecuteScript(string _script) {
#if (DEBUG == true)
            if (this.Data.Code.ContainsKey(_script) == false) {
                throw new Exception(String.Format("Could not execute non-existent script named \"{0}\"", _script));
            }
#endif
            this.ExecuteScript(this.Data.Code[_script]);
        }

        public void ExecuteScript(LCode _code) {
            BinaryReader _codeReader = _code.Reader;
            while (_codeReader.BaseStream.Position < _codeReader.BaseStream.Length) {
                Instruction _instGet = Instruction.Decode(_codeReader.ReadInt32());
#if (DEBUG == true)
                if (Instructions.ContainsKey(_instGet.Opcode) == false) {
                    throw new Exception(String.Format("Could not process unimplemented opcode: \"{0}\"", _instGet.Opcode));
                }
#endif
                Instructions[_instGet.Opcode](this, _code, _codeReader, _instGet);
#if (DEBUG == true)
                /*Console.WriteLine("Stack Size: {0}", this.Stack.Count);
                foreach(dynamic _stackItem in this.Stack.ToArray()) {
                    Console.WriteLine("- " + _stackItem);
                }*/
#endif
            }

#if (DEBUG == true)
            Console.WriteLine("\nVariables:");
            foreach(KeyValuePair<LVariableType, Dictionary<LVariable, dynamic>> _v in this.Variables) {
                foreach(KeyValuePair<LVariable, dynamic> _vv in _v.Value) {
                    Console.WriteLine("{0}.{1} = {2}", _vv.Key.Type, _vv.Key.Name, _vv.Value);
                }
                //Console.WriteLine("{0}.{1} = {2}", _v.Key.Type, _v.Key.Name, _v.Value);
            }
#endif
        }
    }
}
