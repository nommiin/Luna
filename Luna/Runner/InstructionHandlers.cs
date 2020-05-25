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
#if (PRINT_EXEC)
            Console.WriteLine("PushImmediate({0})", _vm.Stack.Peek().Value);
#endif
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
#if (PRINT_EXEC)
            Console.WriteLine("Push(Type: {0}, {1})", _argType, _vm.Stack.Peek().Value);
#endif
        }

        public static void DoPop(Interpreter _vm, LCode _code, BinaryReader _reader, Instruction _inst) {
            LArgumentType _argFrom = (LArgumentType)((_inst.Argument >> 4) & 0xF), _argTo = (LArgumentType)(_inst.Argument & 0xF);
#if (PRINT_EXEC)
            Console.WriteLine("Pop(From: {0}, To: {1}, {2})", _argFrom, _argTo, _vm.Stack.Peek().Value);
#endif
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
#if (PRINT_EXEC)
            Console.Write("Conditional({0} is {1} {2}) = ", _valLeft.Value, _condType, _valRight.Value);
#endif
            switch (_condType) {
                case LConditionType.Equal: _vm.Stack.Push(_valLeft == _valRight); break;
                case LConditionType.NotEqual: _vm.Stack.Push(_valLeft != _valRight); break;
                case LConditionType.LessThan: _vm.Stack.Push(_valLeft < _valRight); break;
                case LConditionType.LessEqual: _vm.Stack.Push(_valLeft <= _valRight); break;
                case LConditionType.GreaterEqual: _vm.Stack.Push(_valLeft >= _valRight); break;
                case LConditionType.GreaterThan: _vm.Stack.Push(_valLeft > _valRight); break;
                default: {
                    throw new Exception(String.Format("Could not process conditional type: \"{0}\"", _inst.Data));
                }
            }
#if (PRINT_EXEC)
            Console.WriteLine(_vm.Stack.Peek().Value);
#endif
        }

        public static void DoBranch(Interpreter _vm, LCode _code, BinaryReader _reader, Instruction _inst) {
            Int32 _branchDistance = (_inst.Raw << 9 >> 7);
#if (PRINT_EXEC)
            Console.WriteLine("Branch() -> {0} bytes ({1})", _branchDistance, (_reader.BaseStream.Position - 4) + _branchDistance);
#endif
            _reader.BaseStream.Seek((_reader.BaseStream.Position - 4) + _branchDistance, SeekOrigin.Begin);
        }

        public static void DoBranchTrue(Interpreter _vm, LCode _code, BinaryReader _reader, Instruction _inst) {
            Int32 _branchDistance = (_inst.Raw << 9 >> 7);
#if (PRINT_EXEC)
            Console.WriteLine("BranchTrue({0} == 1) -> {1} bytes ({2})", _vm.Stack.Peek().Value, _branchDistance, (_reader.BaseStream.Position - 4) + _branchDistance);
#endif
            if (_vm.Stack.Pop().Value == 1) {
                _reader.BaseStream.Seek((_reader.BaseStream.Position - 4) + (_inst.Raw << 9 >> 7), SeekOrigin.Begin);
            }
        }

        public static void DoBranchFalse(Interpreter _vm, LCode _code, BinaryReader _reader, Instruction _inst) {
            Int32 _branchDistance = (_inst.Raw << 9 >> 7);
#if (PRINT_EXEC)
            Console.WriteLine("BranchFalse({0} == 0) -> {1} bytes ({2})", _vm.Stack.Peek().Value, _branchDistance, (_reader.BaseStream.Position - 4) + _branchDistance);
#endif
            if (_vm.Stack.Pop().Value == 0) {
                _reader.BaseStream.Seek((_reader.BaseStream.Position - 4) + _branchDistance, SeekOrigin.Begin);
            }
        }

        public static void DoDiscard(Interpreter _vm, LCode _code, BinaryReader _reader, Instruction _inst) {
#if (PRINT_EXEC)
            Console.WriteLine("Discard({0})", _vm.Stack.Peek().Value);
#endif
            _vm.Stack.Pop();
        }

        public static void DoDuplicate(Interpreter _vm, LCode _code, BinaryReader _reader, Instruction _inst) {
#if (PRINT_EXEC)
            Console.WriteLine("Duplicate({0})", _vm.Stack.Peek().Value);
#endif
            _vm.Stack.Push(_vm.Stack.Peek());
        }

        public static void DoConvert(Interpreter _vm, LCode _code, BinaryReader _reader, Instruction _inst) {
#if (PRINT_EXEC)
            Console.WriteLine("Convert({0})", _vm.Stack.Peek().Value);
#endif
            //_vm.Stack.Push(_vm.Stack.Peek());
        }

        public static void DoCall(Interpreter _vm, LCode _code, BinaryReader _reader, Instruction _inst) {
            LFunction _funcGet = _vm.Data.Functions[_vm.Data.FunctionMapping[(int)((_code.Base + _reader.BaseStream.Position))]];
            if (Interpreter.Functions.ContainsKey(_funcGet.Name) == true) {
#if (PRINT_EXEC)
                Console.WriteLine("Call: ({0}, {1} Arguments)", _funcGet.Name, _inst.Data);
#endif
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
#if (PRINT_EXEC)
            Console.WriteLine("Add({0} + {1}) = {2}", _valLeft.Value, _valRight.Value, _vm.Stack.Peek().Value);
#endif
        }

        public static void DoSubtract(Interpreter _vm, LCode _code, BinaryReader _reader, Instruction _inst) {
            LValue _valRight = _vm.Stack.Pop();
            LValue _valLeft = _vm.Stack.Pop();
            _vm.Stack.Push(_valLeft - _valRight);
#if (PRINT_EXEC)
            Console.WriteLine("Subtract({0} - {1}) = {2}", _valLeft.Value, _valRight.Value, _vm.Stack.Peek().Value);
#endif
        }

        public static void DoMultiply(Interpreter _vm, LCode _code, BinaryReader _reader, Instruction _inst) {
            LValue _valRight = _vm.Stack.Pop();
            LValue _valLeft = _vm.Stack.Pop();
            _vm.Stack.Push(_valLeft * _valRight);
#if (PRINT_EXEC)
            Console.WriteLine("Multiply({0} * {1}) = {2}", _valLeft.Value, _valRight.Value, _vm.Stack.Peek().Value);
#endif
        }

        public static void DoDivide(Interpreter _vm, LCode _code, BinaryReader _reader, Instruction _inst) {
            LValue _valRight = _vm.Stack.Pop();
            LValue _valLeft = _vm.Stack.Pop();
            _vm.Stack.Push(_valLeft / _valRight);
#if (PRINT_EXEC)
            Console.WriteLine("Divide({0} / {1}) = {2}", _valLeft.Value, _valRight.Value, _vm.Stack.Peek().Value);
#endif
        }

        public static void DoRemainder(Interpreter _vm, LCode _code, BinaryReader _reader, Instruction _inst) {
            LValue _valRight = _vm.Stack.Pop();
            LValue _valLeft = _vm.Stack.Pop();
            _vm.Stack.Push(new LValue(LType.Number, Math.Floor(_valLeft.Value / _valRight.Value)));
#if (PRINT_EXEC)
            Console.WriteLine("Remainder(floor({0} / {1})) = {2}", _valLeft.Value, _valRight.Value, _vm.Stack.Peek().Value);
#endif
        }

        public static void DoModulo(Interpreter _vm, LCode _code, BinaryReader _reader, Instruction _inst) {
            LValue _valRight = _vm.Stack.Pop();
            LValue _valLeft = _vm.Stack.Pop();
            _vm.Stack.Push(_valLeft % _valRight);
#if (PRINT_EXEC)
            Console.WriteLine("Modulo({0} % {1}) = {2}", _valLeft.Value, _valRight.Value, _vm.Stack.Peek().Value);
#endif
        }

        public static void DoXor(Interpreter _vm, LCode _code, BinaryReader _reader, Instruction _inst) {
            LValue _valRight = _vm.Stack.Pop();
            LValue _valLeft = _vm.Stack.Pop();
            _vm.Stack.Push(_valLeft ^ _valRight.Value);
#if (PRINT_EXEC)
            Console.WriteLine("Xor({0} ^ {1}) = {2}", _valLeft.Value, _valRight.Value, _vm.Stack.Peek().Value);
#endif
        }

        public static void DoAnd(Interpreter _vm, LCode _code, BinaryReader _reader, Instruction _inst) {
            LValue _valRight = _vm.Stack.Pop();
            LValue _valLeft = _vm.Stack.Pop();
            _vm.Stack.Push(_valLeft & _valRight.Value);
#if (PRINT_EXEC)
            Console.WriteLine("And({0} & {1}) = {2}", _valLeft.Value, _valRight.Value, _vm.Stack.Peek().Value);
#endif
        }

        public static void DoOr(Interpreter _vm, LCode _code, BinaryReader _reader, Instruction _inst) {
            LValue _valRight = _vm.Stack.Pop();
            LValue _valLeft = _vm.Stack.Pop();
            _vm.Stack.Push(_valLeft | _valRight.Value);
#if (PRINT_EXEC)
            Console.WriteLine("Or({0} | {1}) = {2}", _valLeft.Value, _valRight.Value, _vm.Stack.Peek().Value);
#endif
        }

        public static void DoShiftLeft(Interpreter _vm, LCode _code, BinaryReader _reader, Instruction _inst) {
            LValue _valRight = _vm.Stack.Pop();
            LValue _valLeft = _vm.Stack.Pop();
            _vm.Stack.Push(_valLeft << _valRight.Value);
#if (PRINT_EXEC)
            Console.WriteLine("ShiftLeft({0} << {1}) = {2}", _valLeft.Value, _valRight.Value, _vm.Stack.Peek().Value);
#endif
        }

        public static void DoShiftRight(Interpreter _vm, LCode _code, BinaryReader _reader, Instruction _inst) {
            LValue _valRight = _vm.Stack.Pop();
            LValue _valLeft = _vm.Stack.Pop();
            _vm.Stack.Push(_valLeft >> _valRight.Value);
#if (PRINT_EXEC)
            Console.WriteLine("ShiftRight({0} >> {1}) = {2}", _valLeft.Value, _valRight.Value, _vm.Stack.Peek().Value);
#endif
        }
    }
}