using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Luna.Assets;
using Luna.Types;

namespace Luna.Runner {
    static class InstructionHandlers {
        public static void DoPushImmediate(Interpreter _vm, LCode _code, BinaryReader _reader, Instruction _inst) {
            _vm.Stack.Push(new LValue(LType.Number, _inst.Data));
        }

        public static void DoPush(Interpreter _vm, LCode _code, BinaryReader _reader, Instruction _inst) {
            LArgumentType _argType = (LArgumentType)_inst.Argument;
            switch (_argType) {
                case LArgumentType.Error: {
                    _vm.Stack.Push(new LValue(LType.Number, _inst.Data));
                    break;
                }

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
        }

        public static void DoPop(Interpreter _vm, LCode _code, BinaryReader _reader, Instruction _inst) {
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
        }

        public static void DoConditional(Interpreter _vm, LCode _code, BinaryReader _reader, Instruction _inst) {
            LValue _valRight = _vm.Stack.Pop();
            LValue _valLeft = _vm.Stack.Pop();
            LConditionType _condType = (LConditionType)((_inst.Data >> 8) & 0xFF);
            switch (_condType) {
                case LConditionType.Equal: _vm.Stack.Push(_valLeft == _valRight); break;
                case LConditionType.NotEqual: _vm.Stack.Push(_valLeft != _valRight); break;
                case LConditionType.LessThan: _vm.Stack.Push(_valLeft < _valRight); break;
                case LConditionType.LessEqual: _vm.Stack.Push(_valLeft < _valRight); break;
                case LConditionType.GreaterThan: _vm.Stack.Push(_valLeft > _valRight); break;
                case LConditionType.GreaterEqual: _vm.Stack.Push(_valLeft > _valRight); break;
                default: {
                    throw new Exception(String.Format("Could not process conditional type: \"{0}\"", _inst.Data));
                }
            }
        }

        public static void DoBranchFalse(Interpreter _vm, LCode _code, BinaryReader _reader, Instruction _inst) {
            if (_vm.Stack.Pop().Value == 0) {
                _reader.BaseStream.Seek((_reader.BaseStream.Position - 4) + (_inst.Raw << 9 >> 7), SeekOrigin.Begin);
            }
        }

        public static void DoBranchTrue(Interpreter _vm, LCode _code, BinaryReader _reader, Instruction _inst) {
            if (_vm.Stack.Pop().Value == 1) {
                _reader.BaseStream.Seek((_reader.BaseStream.Position - 4) + (_inst.Raw << 9 >> 7), SeekOrigin.Begin);
            }
        }

        public static void DoBranch(Interpreter _vm, LCode _code, BinaryReader _reader, Instruction _inst) {
            _reader.BaseStream.Seek((_reader.BaseStream.Position - 4) + (_inst.Raw << 9 >> 7), SeekOrigin.Begin);
        }

        public static void DoDiscard(Interpreter _vm, LCode _code, BinaryReader _reader, Instruction _inst) {
            _vm.Stack.Pop();
        }

        public static void DoCall(Interpreter _vm, LCode _code, BinaryReader _reader, Instruction _inst) {
            LFunction _funcGet = _vm.Data.Functions[_vm.Data.FunctionMapping[(int)((_code.Base + _reader.BaseStream.Position))]];
            if (Interpreter.Functions.ContainsKey(_funcGet.Name) == true) {
                Interpreter.Functions[_funcGet.Name](_vm, _inst.Data);
            } else {
                throw new Exception(String.Format("Could not execute function named \"{0}\" at instruction {1}", _funcGet.Name, _vm.Count));
            }
            _reader.BaseStream.Seek(sizeof(Int32), SeekOrigin.Current);
        }

        public static void DoAdd(Interpreter _vm, LCode _code, BinaryReader _reader, Instruction _inst) {
            LValue _valRight = _vm.Stack.Pop();
            LValue _valLeft = _vm.Stack.Pop();
            _vm.Stack.Push(_valLeft + _valRight);
        }

        public static void DoSubtract(Interpreter _vm, LCode _code, BinaryReader _reader, Instruction _inst) {
            LValue _valRight = _vm.Stack.Pop();
            LValue _valLeft = _vm.Stack.Pop();
            _vm.Stack.Push(_valLeft - _valRight);
        }

        public static void DoMultiply(Interpreter _vm, LCode _code, BinaryReader _reader, Instruction _inst) {
            LValue _valRight = _vm.Stack.Pop();
            LValue _valLeft = _vm.Stack.Pop();
            _vm.Stack.Push(_valLeft * _valRight);
        }

        public static void DoDivide(Interpreter _vm, LCode _code, BinaryReader _reader, Instruction _inst) {
            LValue _valRight = _vm.Stack.Pop();
            LValue _valLeft = _vm.Stack.Pop();
            _vm.Stack.Push(_valLeft / _valRight);
        }

        public static void DoRemainder(Interpreter _vm, LCode _code, BinaryReader _reader, Instruction _inst) {
            LValue _valRight = _vm.Stack.Pop();
            LValue _valLeft = _vm.Stack.Pop();
            _vm.Stack.Push(new LValue(LType.Number, Math.Floor(_valLeft.Value / _valRight.Value)));
        }

        public static void DoModulo(Interpreter _vm, LCode _code, BinaryReader _reader, Instruction _inst) {
            LValue _valRight = _vm.Stack.Pop();
            LValue _valLeft = _vm.Stack.Pop();
            _vm.Stack.Push(_valLeft % _valRight);
        }

        public static void DoXor(Interpreter _vm, LCode _code, BinaryReader _reader, Instruction _inst) {
            LValue _valRight = _vm.Stack.Pop();
            LValue _valLeft = _vm.Stack.Pop();
            _vm.Stack.Push(_valLeft ^ _valRight.Value);
        }

        public static void DoAnd(Interpreter _vm, LCode _code, BinaryReader _reader, Instruction _inst) {
            LValue _valRight = _vm.Stack.Pop();
            LValue _valLeft = _vm.Stack.Pop();
            _vm.Stack.Push(_valLeft & _valRight.Value);
        }

        public static void DoOr(Interpreter _vm, LCode _code, BinaryReader _reader, Instruction _inst) {
            LValue _valRight = _vm.Stack.Pop();
            LValue _valLeft = _vm.Stack.Pop();
            _vm.Stack.Push(_valLeft | _valRight.Value);
        }

        public static void DoShiftLeft(Interpreter _vm, LCode _code, BinaryReader _reader, Instruction _inst) {
            LValue _valRight = _vm.Stack.Pop();
            LValue _valLeft = _vm.Stack.Pop();
            _vm.Stack.Push(_valLeft << _valRight.Value);
        }

        public static void DoShiftRight(Interpreter _vm, LCode _code, BinaryReader _reader, Instruction _inst) {
            LValue _valRight = _vm.Stack.Pop();
            LValue _valLeft = _vm.Stack.Pop();
            _vm.Stack.Push(_valLeft >> _valRight.Value);
        }
    }
}