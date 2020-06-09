using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using System.IO;
using Luna.Types;
using Luna.Assets;
using Luna.Runner;

namespace Luna.Runner {
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

    [AttributeUsage(AttributeTargets.Class)]
    public class InstructionDefinition : Attribute {
        public LOpcode[] Opcode;
        public InstructionDefinition(LOpcode _opcode) {
            Opcode = new LOpcode[] { _opcode };
        }

        public InstructionDefinition(LOpcode[] _opcode) {
            Opcode = _opcode;
        }

        public static void Initalize() {
            IEnumerable<Type> _instTypes = Assembly.GetAssembly(typeof(Instruction)).GetTypes().Where(e => e.IsSubclassOf(typeof(Instruction)));
            foreach(Type _instType in _instTypes) {
                LOpcode[] _instOpcodes = _instType.GetCustomAttribute<InstructionDefinition>().Opcode;
                
                foreach(LOpcode _instOpcode in _instOpcodes) {
                    Instruction.Mapping.Add(_instOpcode,
                        (Int32 _instruction, Game _game, LCode _code, BinaryReader _reader) => Activator.CreateInstance(_instType, new object[] { _instruction, _game, _code, _reader }) as Instruction
                    );
                }
            }
        }
    }

    class Instruction {
        public static Dictionary<LOpcode, Func<Int32, Game, LCode, BinaryReader, Instruction>> Mapping = new Dictionary<LOpcode, Func<int, Game, LCode, BinaryReader, Instruction>>();
        public static LOpcode[] Ignore = {
            LOpcode.conv
        };

        public static LOpcode GetOpcode(Int32 _instruction) {
            int _opcodeGet = (_instruction >> 24) & 0xFF;
            if (_opcodeGet == 255) {
                int _int = _instruction & UInt16.MaxValue;
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

        public LOpcode Opcode;
        public byte Argument;
        public Int16 Data;
        public Int32 Raw;

        public Instruction(Int32 _instruction) {
            Raw = _instruction;
            Opcode = GetOpcode(_instruction);
            Argument = (byte)((_instruction >> 16) & 0xFF);
            Data = (Int16)(_instruction & 0xFFFF);
        }

        public virtual void Perform(Game _assets, Domain _environment, LCode _code, Stack<LValue> _stack) {
            Console.WriteLine("[WARNING] - Could not perform action for unimplemented opcode: {0} in {1}", Opcode, _code.Name);
        }
    }
}

namespace Luna.Instructions {
    [InstructionDefinition(LOpcode.pushi)]
    class PushImmediate : Instruction {
        public LValue Value;
        public PushImmediate(Int32 _instruction, Game _game, LCode _code, BinaryReader _reader) : base(_instruction) {
            Value = new LValue(LType.Number, (double)Data);
        }

        public override void Perform(Game _assets, Domain _environment, LCode _code, Stack<LValue> _stack) {
            _stack.Push(Value);
        }
    }

    [InstructionDefinition(new LOpcode[] { LOpcode.push, LOpcode.pushl })]
    class Push : Instruction {
        public LValue Value;
        public LVariable Variable;
        public LArgumentType Type;
        public Push(Int32 _instruction, Game _game, LCode _code, BinaryReader _reader) : base(_instruction) {
            Type = (LArgumentType)Argument;
            switch (Type) {
                case LArgumentType.Error: {
                    Value = new LValue(LType.Number, (double)Data);
                    break;
                }

                case LArgumentType.Integer: {
                    Value = new LValue(LType.Number, _reader.ReadInt32());
                    break;
                }

                case LArgumentType.Long: {
                    Value = new LValue(LType.Number, _reader.ReadInt64());
                    break;
                }

                case LArgumentType.Double: {
                    Value = new LValue(LType.Number, _reader.ReadDouble());
                    break;
                }

                case LArgumentType.String: {
                    Value = new LValue(LType.String, _game.StringMapping[_reader.ReadInt32()].Value);
                    break;
                }

                case LArgumentType.Variable: {
                    Variable = _game.Variables[_game.VariableMapping[(int)(_code.Base + _reader.BaseStream.Position) - 4]];
                    _reader.ReadInt32();
                    break;
                }

                default: {
                    throw new Exception(String.Format("Could not parse unimplemented push type {0}", Type));
                }
            }
        }

        public override void Perform(Game _assets, Domain _environment, LCode _code, Stack<LValue> _stack) {
            switch (Type) {
                case LArgumentType.Variable: {
                    _stack.Push(_environment.Instance.Variables[Variable.Name]);
                    break;
                }

                default: {
                    _stack.Push(Value);
                    break;
                }
            }
        }
    }

    [InstructionDefinition(LOpcode.pushg)]
    class PushGlobal : Instruction {
        public LValue Value;
        public string Variable;
        public PushGlobal(Int32 _instruction, Game _game, LCode _code, BinaryReader _reader) : base(_instruction) {
            switch ((LArgumentType)Argument) {
                case LArgumentType.Variable: {
                    Variable = _game.Variables[_game.VariableMapping[(int)(_code.Base + _reader.BaseStream.Position) - 4]].Name;
                    _reader.ReadInt32();
                    break;
                }

                default: {
                    throw new Exception(String.Format("Could not parse unimplemented global push type: \"{0}\"", (LArgumentType)Argument));
                }
            }
        }
    }

    [InstructionDefinition(LOpcode.pushb)]
    class PushBuiltin : Instruction {
        public string Variable;

        public PushBuiltin(Int32 _instruction, Game _game, LCode _code, BinaryReader _reader) : base(_instruction)
        {
            Variable = _game.Variables[_game.VariableMapping[(int)(_code.Base + _reader.BaseStream.Position) - 4]].Name;
            _reader.ReadInt32();
        }

        public override void Perform(Game _assets, Domain _environment, LCode _code, Stack<LValue> _stack)
        {
            _stack.Push(new LValue(LType.String,Variable));
        }
    }

    [InstructionDefinition(LOpcode.pop)]
    class Pop : Instruction {
        public LVariable Variable;
        public LArgumentType ArgTo;
        public LArgumentType ArgFrom;
        public Pop(Int32 _instruction, Game _game, LCode _code, BinaryReader _reader) : base(_instruction) {
            ArgTo = (LArgumentType)(Argument & 0xF);
            ArgFrom = (LArgumentType)((Argument >> 4) & 0xF);
            switch (ArgTo) {
                case LArgumentType.Variable: {
                    Int32 _varOffset = (int)(_code.Base + _reader.BaseStream.Position) - 4;
                    Variable = _game.Variables[_game.VariableMapping[_varOffset]];
                    _reader.ReadInt32();
                    break;
                }

                default: {
                    throw new Exception(String.Format("Could not pop from {0} to {1}", ArgTo, ArgFrom));
                }
            }
        }

        public override void Perform(Game _assets, Domain _environment, LCode _code, Stack<LValue> _stack) {
            _environment.Instance.Variables[Variable.Name] = _stack.Pop();
            /*
            Console.WriteLine(Variable.Name);
            */
        }
    }

    [InstructionDefinition(LOpcode.popz)]
    class Discard : Instruction {
        public Discard(Int32 _instruction, Game _game, LCode _code, BinaryReader _reader) : base(_instruction) { }
        public override void Perform(Game _assets, Domain _environment, LCode _code, Stack<LValue> _stack) {
            _stack.Pop();
        }
    }

    [InstructionDefinition(LOpcode.dup)]
    class Duplicate : Instruction {
        public Duplicate(Int32 _instruction, Game _game, LCode _code, BinaryReader _reader) : base(_instruction) { }
        public override void Perform(Game _assets, Domain _environment, LCode _code, Stack<LValue> _stack) {
            _stack.Push(_stack.Peek());
        }
    }

    [InstructionDefinition(new LOpcode[] { LOpcode.call, LOpcode.callv })]
    class Call : Instruction {
        public Function.Handler Function;
        public LValue[] Arguments;
        public Int32 Count;
        public Call(Int32 _instruction, Game _game, LCode _code, BinaryReader _reader) : base(_instruction) {
            string _funcName = _game.Functions[_game.FunctionMapping[(int)(_code.Base + _reader.BaseStream.Position)]].Name;
            if (Runner.Function.Mapping.ContainsKey(_funcName)) {
                Function = Runner.Function.Mapping[_funcName];
            } else {
                throw new Exception(String.Format("Could not find function mapping for \"{0}\"", _funcName));
            }
            /*try {
                this.Function = Luna.Runner.Function.Mapping[];
            } catch (Exception e) {
                throw new Exception(String.Format("Could not find instruction mapping for ));
            }*/
            Count = Data;
            Arguments = new LValue[Count];
            _reader.ReadInt32();
        }

        public override void Perform(Game _assets, Domain _environment, LCode _code, Stack<LValue> _stack) {
            for(int i = 0; i < Count; i++) Arguments[i] = _stack.Pop();
            Function(_assets, _environment, Arguments, Count, _stack);
        }
    }

    [InstructionDefinition(LOpcode.set)]
    class Conditional : Instruction {
        public LConditionType Type;
        public Conditional(Int32 _instruction, Game _game, LCode _code, BinaryReader _reader) : base(_instruction) {
            Type = (LConditionType)((Data >> 8) & 0xFF);
        }

        public override void Perform(Game _assets, Domain _environment, LCode _code, Stack<LValue> _stack) {
            LValue _compRight = _stack.Pop();
            LValue _compLeft  = _stack.Pop();
            switch (Type) {
                case LConditionType.Equal: _stack.Push(_compLeft == _compRight); break;
                case LConditionType.NotEqual: _stack.Push(_compLeft != _compRight); break;
                case LConditionType.LessThan: _stack.Push(_compLeft < _compRight); break;
                case LConditionType.GreaterThan: _stack.Push(_compLeft > _compRight); break;
                case LConditionType.LessEqual: _stack.Push(_compLeft <= _compRight); break;
                case LConditionType.GreaterEqual: _stack.Push(_compLeft >= _compRight); break;
            }
        }
    }

    [InstructionDefinition(LOpcode.b)]
    class Branch : Instruction {
        public Int32 Offset;
        public Int32 Jump = -1;
        public Branch(Int32 _instruction, Game _game, LCode _code, BinaryReader _reader) : base(_instruction) {
            Offset = (Int32)(_reader.BaseStream.Position + (Raw << 9 >> 7) - 4);
        }

        public override void Perform(Game _assets, Domain _environment, LCode _code, Stack<LValue> _stack) {
            _environment.ProgramCounter = Jump;
        }
    }

    [InstructionDefinition(LOpcode.bt)]
    class BranchTrue : Branch {
        public BranchTrue(Int32 _instruction, Game _game, LCode _code, BinaryReader _reader) : base(_instruction, _game, _code, _reader) { }
        public override void Perform(Game _assets, Domain _environment, LCode _code, Stack<LValue> _stack) {
            if ((double)_stack.Pop().Value == 1) {
                _environment.ProgramCounter = Jump;
            }
        }
    }

    [InstructionDefinition(LOpcode.bf)]
    class BranchFalse : Branch {
        public BranchFalse(Int32 _instruction, Game _game, LCode _code, BinaryReader _reader) : base(_instruction, _game, _code, _reader) { }
        public override void Perform(Game _assets, Domain _environment, LCode _code, Stack<LValue> _stack) {
            if ((double)_stack.Pop().Value == 0) {
                _environment.ProgramCounter = Jump;
            }
        }
    }

    [InstructionDefinition(LOpcode.add)]
    class Add : Instruction {
        public Add(Int32 _instruction, Game _game, LCode _code, BinaryReader _reader) : base(_instruction) { }
        public override void Perform(Game _assets, Domain _environment, LCode _code, Stack<LValue> _stack) {
            LValue _valRight = _stack.Pop();
            LValue _valLeft = _stack.Pop();
            _stack.Push(_valLeft + _valRight);
        }
    }

    [InstructionDefinition(LOpcode.sub)]
    class Subtract : Instruction {
        public Subtract(Int32 _instruction, Game _game, LCode _code, BinaryReader _reader) : base(_instruction) { }
        public override void Perform(Game _assets, Domain _environment, LCode _code, Stack<LValue> _stack) {
            LValue _valRight = _stack.Pop();
            LValue _valLeft = _stack.Pop();
            _stack.Push(_valLeft - _valRight);
        }
    }

    [InstructionDefinition(LOpcode.mul)]
    class Multiply : Instruction {
        public Multiply(Int32 _instruction, Game _game, LCode _code, BinaryReader _reader) : base(_instruction) { }
    }

    [InstructionDefinition(LOpcode.div)]
    class Divide : Instruction {
        public Divide(Int32 _instruction, Game _game, LCode _code, BinaryReader _reader) : base(_instruction) { }
    }

    [InstructionDefinition(LOpcode.rem)]
    class Remainder : Instruction {
        public Remainder(Int32 _instruction, Game _game, LCode _code, BinaryReader _reader) : base(_instruction) { }
    }

    [InstructionDefinition(LOpcode.mod)]
    class Modulo : Instruction {
        public Modulo(Int32 _instruction, Game _game, LCode _code, BinaryReader _reader) : base(_instruction) { }
    }

    [InstructionDefinition(LOpcode.xor)]
    class Xor : Instruction {
        public Xor(Int32 _instruction, Game _game, LCode _code, BinaryReader _reader) : base(_instruction) { }
    }

    [InstructionDefinition(LOpcode.and)]
    class And : Instruction {
        public And(Int32 _instruction, Game _game, LCode _code, BinaryReader _reader) : base(_instruction) { }
    }

    [InstructionDefinition(LOpcode.not)]
    class Not : Instruction {
        public Not(Int32 _instruction, Game _game, LCode _code, BinaryReader _reader) : base(_instruction) { }
    }

    [InstructionDefinition(LOpcode.neg)]
    class Negate : Instruction {
        public Negate(Int32 _instruction, Game _game, LCode _code, BinaryReader _reader) : base(_instruction) { }
    }

    [InstructionDefinition(LOpcode.or)]
    class Or : Instruction {
        public Or(Int32 _instruction, Game _game, LCode _code, BinaryReader _reader) : base(_instruction) { }
    }

    [InstructionDefinition(LOpcode.shl)]
    class ShiftLeft : Instruction {
        public ShiftLeft(Int32 _instruction, Game _game, LCode _code, BinaryReader _reader) : base(_instruction) { }
    }

    [InstructionDefinition(LOpcode.shr)]
    class ShiftRight : Instruction {
        public ShiftRight(Int32 _instruction, Game _game, LCode _code, BinaryReader _reader) : base(_instruction) { }
    }
}