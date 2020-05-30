using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Luna.Assets;
using Luna.Runner;
using Luna.Types;

namespace Luna {
    public enum LOpcode {
        popv = 5,
        conv = 7,
        mul = 8,
        div = 9,
        rem = 10,
        mod = 11,
        add = 12,
        sub = 13,
        and = 14,
        or = 15,
        xor = 16,
        neg = 17,
        not = 18,
        shl = 19,
        shr = 20,
        set = 21,
        pop = 69,
        pushv = 128,
        pushi = 132,
        dup = 134,
        callv = 153,
        ret = 156,
        exit = 157,
        popz = 158,
        b = 182,
        bt = 183,
        bf = 184,
        pushenv = 186,
        popenv = 187,
        push = 192,
        pushl = 193,
        pushg = 194,
        pushb = 195,
        call = 217,
        brk = 255,
        setstatic = 256,
        isstaticok = 257,
        setowner = 258,
        pushac = 259,
        popaf = 260,
        pushaf = 261,
        chkindex = 271,
        unknown = 1000
    }

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

    public enum LConditionType {
        None,
        LessThan,
        LessEqual,
        Equal,
        NotEqual,
        GreaterEqual,
        GreaterThan
    }

    class Instruction {
        public LOpcode Opcode;
        public byte Argument;
        public Int16 Data;
        public Int32 Raw;

        public Instruction(Int32 _instruction) {
            this.Opcode = Instruction.GetOpcode(_instruction);
            this.Argument = (byte)((_instruction >> 16) & 0xFF);
            this.Data = (Int16)(_instruction & 0xFFFF);
            this.Raw = _instruction;
        }

        public virtual void Perform(Interpreter _vm, Domain _environment, Stack<LValue> _stack) {
            Console.WriteLine("[WARNING] - Attempted to perform unimplemented operation \"{0}\"", this.Opcode);
        }

        public static LOpcode GetOpcode(Int32 _instruction) {
            int _opcodeGet = ((_instruction >> 24) & 0xFF);
            if (_opcodeGet == 255) {
                int _int = (_instruction & UInt16.MaxValue);
                switch (_int) {
                    case UInt16.MaxValue - 0: return LOpcode.chkindex;
                    case UInt16.MaxValue - 1: return LOpcode.pushaf;
                    case UInt16.MaxValue - 2: return LOpcode.popaf;
                    case UInt16.MaxValue - 3: return LOpcode.pushac;
                    case UInt16.MaxValue - 4: return LOpcode.setowner;
                    case UInt16.MaxValue - 5: return LOpcode.isstaticok;
                    case UInt16.MaxValue - 6: return LOpcode.setstatic;
                }
                return LOpcode.brk;
            }
            return (LOpcode)_opcodeGet;
        }
        
        public override string ToString() {
            return $"Opcode: {((Enum.IsDefined(typeof(LOpcode), this.Opcode) == true) ? "LOpcode." + Enum.GetName(typeof(LOpcode), this.Opcode) : "???")}, Argument: {this.Argument}, Data: {this.Data}";
        }
    }
}

namespace Luna.Instructions {
    class PushImmediate : Instruction {
        public LValue Value;
        public PushImmediate(Int32 _instruction, Game _game, LCode _code, BinaryReader _reader) : base(_instruction) {
            this.Value = new LValue(LType.Number, (double)this.Data);
        }

        public override void Perform(Interpreter _vm, Domain _domain, Stack<LValue> _stack) {
            _stack.Push(this.Value);
        }

        public override string ToString() {
            return $"PushImmediate({this.Value.Value})";
        }
    }

    class Push : Instruction {
        public LValue Value;
        public LVariable Variable;
        public LArgumentType Type;
        public Push(Int32 _instruction, Game _game, LCode _code, BinaryReader _reader) : base(_instruction) {
            this.Type = (LArgumentType)this.Argument;
            switch (this.Type) {
                case LArgumentType.Error: {
                    this.Value = new LValue(LType.Number, this.Data);
                    break;
                }

                case LArgumentType.Integer: {
                    this.Value = new LValue(LType.Number, _reader.ReadInt32());
                    break;
                }

                case LArgumentType.Long: {
                    this.Value = new LValue(LType.Number, _reader.ReadInt64());
                    break;
                }

                case LArgumentType.String: {
                    this.Value = new LValue(LType.String, _game.StringMapping[_reader.ReadInt32()].Value);
                    break;
                }

                case LArgumentType.Variable: {
                    this.Variable = _game.Variables[_game.VariableMapping[(int)((_code.Base + _reader.BaseStream.Position)) - 4]];
                    _reader.ReadInt32();
                    break;
                }

                default: {
                    throw new Exception(String.Format("Could not parse unimplemented push type {0}", this.Type));
                }
            }
        }

        public override void Perform(Interpreter _vm, Domain _environment, Stack<LValue> _stack) {
            switch (this.Type) {
                case LArgumentType.Variable: {
                    switch (this.Variable.Scope) {
                        case LVariableScope.Global: {
                            _stack.Push(_vm.Data.Instances[(Int32)LVariableScope.Global].Variables[this.Variable.Name]);
                            break;
                        }

                        case LVariableScope.Static: {
                            _stack.Push(_vm.Data.Instances[(Int32)LVariableScope.Static].Variables[this.Variable.Name]);
                            break;
                        }

                        case LVariableScope.Local: {
                            _stack.Push(_environment.LocalVariables[this.Variable.Name]);
                            break;
                        }
                    }
                    break;
                }

                default: {
                    _stack.Push(this.Value);
                    break;
                }
            }
        }

        public override string ToString() {
            return $"Push(Type: {this.Type})";
        }
    }

    class PushGlobal : Instruction {
        public LValue Value;
        public string Variable;
        public PushGlobal(Int32 _instruction, Game _game, LCode _code, BinaryReader _reader) : base(_instruction) {
            switch ((LArgumentType)this.Argument) {
                case LArgumentType.Variable: {
                    this.Variable = _game.Variables[_game.VariableMapping[(int)((_code.Base + _reader.BaseStream.Position)) - 4]].Name;
                    _reader.ReadInt32();
                    break;
                }

                default: {
                    throw new Exception(String.Format("Could not parse unimplemented global push type: \"{0}\"", (LArgumentType)this.Argument));
                }
            }
        }

        public override void Perform(Interpreter _vm, Domain _environment, Stack<LValue> _stack) {
            _stack.Push(_vm.Data.Instances[(Int32)LVariableScope.Global].Variables[this.Variable]);
        }

        public override string ToString() {
            return $"PushGlobal({this.Variable})";
        }
    }

    class PushBuiltin : Instruction {
        public string Variable;
        public PushBuiltin(Int32 _instruction, Game _game, LCode _code, BinaryReader _reader) : base(_instruction) {
            this.Variable = _game.Variables[_game.VariableMapping[(int)((_code.Base + _reader.BaseStream.Position)) - 4]].Name;
            _reader.ReadInt32();
        }

        public override void Perform(Interpreter _vm, Domain _environment, Stack<LValue> _stack) {
            switch (this.Variable) {
                case "current_time": _stack.Push(new LValue(LType.Number, _vm.Timer.ElapsedMilliseconds)); break;
            }
        }

        public override string ToString() {
            return $"PushBuiltin({this.Variable})";
        }
    }

    class Pop : Instruction {
        public LVariable Variable;
        public LArgumentType ArgTo;
        public LArgumentType ArgFrom;
        public Pop(Int32 _instruction, Game _game, LCode _code, BinaryReader _reader) : base(_instruction) {
            this.ArgTo = (LArgumentType)(this.Argument & 0xF);
            this.ArgFrom = (LArgumentType)((this.Argument >> 4) & 0xF);
            switch (this.ArgTo) {
                case LArgumentType.Variable: {
                    Int32 _varOffset = (int)((_code.Base + _reader.BaseStream.Position)) - 4;
                    this.Variable = _game.Variables[_game.VariableMapping[_varOffset]];
                    _reader.ReadInt32();
                    break;
                }
            }
        }

        public override void Perform(Interpreter _vm, Domain _environment, Stack<LValue> _stack) {
            switch (this.Variable.Scope) {
                case LVariableScope.Global: {
                    _vm.Data.Instances[(Int32)LVariableScope.Global].Variables[this.Variable.Name] = _stack.Pop();
                    break;
                }

                case LVariableScope.Static: {
                    _vm.Data.Instances[(Int32)LVariableScope.Static].Variables[this.Variable.Name] = _stack.Pop();
                    break;
                }

                case LVariableScope.Instance: {
                    _environment.Scope.Variables[this.Variable.Name] = _stack.Pop();
                    break;
                }

                case LVariableScope.Local: {
                    _environment.LocalVariables[this.Variable.Name] = _stack.Pop();
                    break;
                }

                default: {
                    throw new Exception(String.Format("Could not pop variable to scope: {0}", this.Variable.Scope));
                }
            }
        }

        public override string ToString() {
            return $"Pop(To: {this.ArgTo}, From: {this.ArgFrom}, Variable: {this.Variable.Name})";
        }
    }

    class Conditional : Instruction {
        public LConditionType Type;
        public Conditional(Int32 _instruction, Game _game, LCode _code, BinaryReader _reader) : base(_instruction) {
            this.Type = (LConditionType)((this.Data >> 8) & 0xFF);
        }

        public override void Perform(Interpreter _vm, Domain _environment, Stack<LValue> _stack) {
            LValue _compRight = _stack.Pop();
            LValue _compLeft = _stack.Pop();
            switch (this.Type) {
                case LConditionType.Equal: _stack.Push(_compLeft == _compRight); break;
                case LConditionType.NotEqual: _stack.Push(_compLeft != _compRight); break;
                case LConditionType.LessThan: _stack.Push(_compLeft < _compRight); break;
                case LConditionType.LessEqual: _stack.Push(_compLeft <= _compRight); break;
                case LConditionType.GreaterEqual: _stack.Push(_compLeft >= _compRight); break;
                case LConditionType.GreaterThan: _stack.Push(_compLeft > _compRight); break;
            }
        }

        public override string ToString() {
            return $"Conditional({this.Type})";
        }
    }

    class Branch : Instruction {
        public Int32 Offset;
        public Int32 Jump = -1;
        public Branch(Int32 _instruction, Game _game, LCode _code, BinaryReader _reader) : base(_instruction) {
            this.Offset = (Int32)((_reader.BaseStream.Position + (this.Raw << 9 >> 7)) - 4);
        }

        public override void Perform(Interpreter _vm, Domain _environment, Stack<LValue> _stack) {
            _vm.ProgramCounter = this.Jump;
        }

        public override string ToString() {
            return $"Branch(Offset: {this.Offset}, Jump: {this.Jump})";
        }
    }

    class BranchTrue : Branch {
        public BranchTrue(Int32 _instruction, Game _game, LCode _code, BinaryReader _reader) : base(_instruction, _game, _code, _reader) {}
        public override void Perform(Interpreter _vm, Domain _environment, Stack<LValue> _stack) {
            if ((double)_stack.Pop().Value == 1.0) {
                _vm.ProgramCounter = this.Jump;
            }
        }
    }

    class BranchFalse : Branch {
        public BranchFalse(Int32 _instruction, Game _game, LCode _code, BinaryReader _reader) : base(_instruction, _game, _code, _reader) {}
        public override void Perform(Interpreter _vm, Domain _environment, Stack<LValue> _stack) {
            if ((double)_stack.Pop().Value == 0.0) {
                _vm.ProgramCounter = this.Jump;
            }
        }
    }

    /*class Convert : Instruction {
        public Convert(Int32 _instruction) : base(_instruction) { }
    }*/

    class Call : Instruction {
        public string Function;
        public Int32 Count;
        public Call(Int32 _instruction, Game _game, LCode _code, BinaryReader _reader) : base(_instruction) {
            this.Function = _game.Functions[_game.FunctionMapping[(int)((_code.Base + _reader.BaseStream.Position))]].Name;
            this.Count = this.Data;
            _reader.ReadInt32();
        }

        public override void Perform(Interpreter _vm, Domain _environment, Stack<LValue> _stack) {
            Console.WriteLine("CALL WITH {0} ARGUMENTS", this.Count);
            Interpreter.Functions[this.Function](_vm, this.Count, _stack);
        }

        public override string ToString() {
            return $"Call({this.Function})";
        }
    }

    class Add : Instruction {
        public Add(Int32 _instruction, Game _game, LCode _code, BinaryReader _reader) : base(_instruction) { }
        public override void Perform(Interpreter _vm, Domain _environment, Stack<LValue> _stack) {
            LValue _valRight = _stack.Pop();
            LValue _valLeft = _stack.Pop();
            _stack.Push(_valLeft + _valRight);
        }
    }

    class Subtract : Instruction {
        public Subtract(Int32 _instruction, Game _game, LCode _code, BinaryReader _reader) : base(_instruction) { }
        public override void Perform(Interpreter _vm, Domain _environment, Stack<LValue> _stack) {
            LValue _valRight = _stack.Pop();
            LValue _valLeft = _stack.Pop();
            _stack.Push(_valLeft - _valRight);
        }
    }

    class Multiply : Instruction {
        public Multiply(Int32 _instruction, Game _game, LCode _code, BinaryReader _reader) : base(_instruction) { }
        public override void Perform(Interpreter _vm, Domain _environment, Stack<LValue> _stack) {
            LValue _valRight = _stack.Pop();
            LValue _valLeft = _stack.Pop();
            _stack.Push(_valLeft * _valRight);
        }
    }

    class Divide : Instruction {
        public Divide(Int32 _instruction, Game _game, LCode _code, BinaryReader _reader) : base(_instruction) { }
        public override void Perform(Interpreter _vm, Domain _environment, Stack<LValue> _stack) {
            LValue _valRight = _stack.Pop();
            LValue _valLeft = _stack.Pop();
            _stack.Push(_valLeft / _valRight);
        }
    }

    class Remainder : Instruction {
        public Remainder(Int32 _instruction, Game _game, LCode _code, BinaryReader _reader) : base(_instruction) { }
        public override void Perform(Interpreter _vm, Domain _environment, Stack<LValue> _stack) {
            LValue _valRight = _stack.Pop();
            LValue _valLeft = _stack.Pop();
            _stack.Push(_valLeft.Set(LType.Number, _valLeft / _valRight));
        }
    }

    class Modulo : Instruction {
        public Modulo(Int32 _instruction, Game _game, LCode _code, BinaryReader _reader) : base(_instruction) { }
        public override void Perform(Interpreter _vm, Domain _environment, Stack<LValue> _stack) {
            LValue _valRight = _stack.Pop();
            LValue _valLeft = _stack.Pop();
            _stack.Push(_valLeft % _valRight);
        }
    }

    class Xor : Instruction {
        public Xor(Int32 _instruction, Game _game, LCode _code, BinaryReader _reader) : base(_instruction) { }
        public override void Perform(Interpreter _vm, Domain _environment, Stack<LValue> _stack) {
            long _valRight = (long)_stack.Pop().Value;
            long _valLeft = (long)_stack.Pop().Value;
            _stack.Push(new LValue(LType.Number, _valLeft ^ _valRight));
        }
    }

    class And : Instruction {
        public And(Int32 _instruction, Game _game, LCode _code, BinaryReader _reader) : base(_instruction) { }
        public override void Perform(Interpreter _vm, Domain _environment, Stack<LValue> _stack) {
            long _valRight = (long)_stack.Pop().Value;
            long _valLeft = (long)_stack.Pop().Value;
            _stack.Push(new LValue(LType.Number, _valLeft & _valRight));
        }
    }

    class Not : Instruction {
        public Not(Int32 _instruction, Game _game, LCode _code, BinaryReader _reader) : base(_instruction) { }
        public override void Perform(Interpreter _vm, Domain _environment, Stack<LValue> _stack) {
            bool _valLeft = (bool)_stack.Pop().Value;
            _stack.Push(new LValue(LType.Number, !_valLeft));
        }
    }

    class Negate : Instruction {
        public Negate(Int32 _instruction, Game _game, LCode _code, BinaryReader _reader) : base(_instruction) { }
        public override void Perform(Interpreter _vm, Domain _environment, Stack<LValue> _stack) {
            long _valLeft = (long)_stack.Pop().Value;
            _stack.Push(new LValue(LType.Number, ~_valLeft));
        }
    }

    class Or : Instruction {
        public Or(Int32 _instruction, Game _game, LCode _code, BinaryReader _reader) : base(_instruction) { }
        public override void Perform(Interpreter _vm, Domain _environment, Stack<LValue> _stack) {
            long _valRight = (long)_stack.Pop().Value;
            long _valLeft = (long)_stack.Pop().Value;
            _stack.Push(new LValue(LType.Number, _valLeft | _valRight));
        }
    }

    class ShiftLeft : Instruction {
        public ShiftLeft(Int32 _instruction, Game _game, LCode _code, BinaryReader _reader) : base(_instruction) { }
        public override void Perform(Interpreter _vm, Domain _environment, Stack<LValue> _stack) {
            int _valRight = (int)_stack.Pop().Value;
            long _valLeft = (long)_stack.Pop().Value;
            _stack.Push(new LValue(LType.Number, _valLeft << _valRight));
        }
    }

    class ShiftRight : Instruction {
        public ShiftRight(Int32 _instruction, Game _game, LCode _code, BinaryReader _reader) : base(_instruction) { }
        public override void Perform(Interpreter _vm, Domain _environment, Stack<LValue> _stack) {
            int _valRight = (int)_stack.Pop().Value;
            long _valLeft = (long)_stack.Pop().Value;
            _stack.Push(new LValue(LType.Number, _valLeft >> _valRight));
        }
    }

    class Discard : Instruction {
        public Discard(Int32 _instruction, Game _game, LCode _code, BinaryReader _readern) : base(_instruction) { }
        public override void Perform(Interpreter _vm, Domain _environment, Stack<LValue> _stack) {
            _stack.Pop();
        }
    }

    class Duplicate : Instruction {
        public Duplicate(Int32 _instruction, Game _game, LCode _code, BinaryReader _reader) : base(_instruction) { }
        public override void Perform(Interpreter _vm, Domain _environment, Stack<LValue> _stack) {
            _stack.Push(_stack.Peek());
        }
    }

    class SetOwner : Instruction {
        public SetOwner(Int32 _instruction, Game _game, LCode _code, BinaryReader _reader) : base(_instruction) { }
        public override void Perform(Interpreter _vm, Domain _environment, Stack<LValue> _stack) {
            _stack.Pop();
        }
    }
}
