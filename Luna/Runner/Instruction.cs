using System;
using System.Collections.Generic;
using System.Linq;
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
            this.Opcode = new LOpcode[] { _opcode };
        }

        public InstructionDefinition(LOpcode[] _opcode) {
            this.Opcode = _opcode;
        }

        public static void Initalize() {
            IEnumerable<Type> _instructionTypes = Assembly.GetAssembly(typeof(Instruction)).GetTypes().Where(e => e.IsSubclassOf(typeof(Instruction)));
            foreach(Type _instType in _instructionTypes) {
                LOpcode[] _instructionOpcodes = _instType.GetCustomAttribute<InstructionDefinition>().Opcode;
                
                foreach(LOpcode _instructionOpcode in _instructionOpcodes) {
                    Instruction.Mapping.Add(_instructionOpcode,
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

        public LOpcode Opcode;
        public byte Argument;
        public Int16 Data;
        public Int32 Raw;

        public Instruction(Int32 _instruction) {
            this.Raw = _instruction;
            this.Opcode = Instruction.GetOpcode(_instruction);
            this.Argument = (byte)((_instruction >> 16) & 0xFF);
            this.Data = (Int16)(_instruction & 0xFFFF);
        }

        public virtual void Perform(Game _assets, Domain _environment, LCode _code, Stack<LValue> _stack) {
            Console.WriteLine("[WARNING] - Could not perform action for unimplemented opcode: {0} in {1}", this.Opcode, _code.Name);
        }
    }
}

namespace Luna.Instructions {
    #region Push Variations
    [InstructionDefinition(LOpcode.pushi)]
    class PushImmediate : Instruction {
        public LValue Value;
        public PushImmediate(Int32 _instruction, Game _game, LCode _code, BinaryReader _reader) : base(_instruction) {
            this.Value = new LValue(LType.Number, (double)this.Data);
        }

        public override void Perform(Game _assets, Domain _environment, LCode _code, Stack<LValue> _stack) {
            _stack.Push(this.Value);
        }
    }

    [InstructionDefinition(LOpcode.pushb)]
    class PushBuiltin : Instruction {
        public string Variable;
        public PushBuiltin(Int32 _instruction, Game _game, LCode _code, BinaryReader _reader) : base(_instruction) {
            this.Variable = _game.Variables[_game.VariableMapping[(int)((_code.Base + _reader.BaseStream.Position)) - 4]].Name;
            _reader.ReadInt32();
        }

        public override void Perform(Game _assets, Domain _environment, LCode _code, Stack<LValue> _stack) {
            switch (this.Variable) {
                case "room": {
                    _stack.Push(LValue.Real((double)_assets.CurrentRoom.Index));
                    break;
                }

                case "room_width": {
                    _stack.Push(LValue.Real((double)_assets.CurrentRoom.Width));
                    break;
                }
                case "room_height": {
                    _stack.Push(LValue.Real((double)_assets.CurrentRoom.Height));
                    break;
                }

                default: {
                    throw new Exception(String.Format("Could not return built-in variable named: \"{0}\"", this.Variable));
                }
            }
        }
    }

    [InstructionDefinition(LOpcode.pushl)]
    class PushLocal : Instruction {
        public string Variable;
        public PushLocal(Int32 _instruction, Game _game, LCode _code, BinaryReader _reader) : base(_instruction) {
            this.Variable = _game.Variables[_game.VariableMapping[(int)((_code.Base + _reader.BaseStream.Position)) - 4]].Name;
            _reader.ReadInt32();
        }

        public override void Perform(Game _assets, Domain _environment, LCode _code, Stack<LValue> _stack) {
            switch ((LArgumentType)this.Argument) {
                case LArgumentType.Variable: {
                    _stack.Push(_environment.Locals[this.Variable]);
                    break;
                }

                default: {
                    throw new Exception(String.Format("Could not parse unimplemented local push type: \"{0}\"", (LArgumentType)this.Argument));
                }
            }
        }
    }

    [InstructionDefinition(LOpcode.pushg)]
    class PushGlobal : Instruction {
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

        public override void Perform(Game _assets, Domain _environment, LCode _code, Stack<LValue> _stack) {
            switch ((LArgumentType)this.Argument) {
                case LArgumentType.Variable: {
                    _stack.Push(_assets.GlobalScope.Variables[this.Variable]);
                    break;
                }

                default: {
                    throw new Exception(String.Format("Could not push unimplemented global type: \"{0}\"", (LArgumentType)this.Argument));
                }
            }
        }
    }

    [InstructionDefinition(LOpcode.push)]
    class Push : Instruction {
        public bool IsArray;
        public LValue Value;
        public LVariable Variable;
        public LArgumentType Type;
        public Push(Int32 _instruction, Game _game, LCode _code, BinaryReader _reader) : base(_instruction) {
            this.IsArray = false;
            this.Type = (LArgumentType)this.Argument;
            switch (this.Type) {
                case LArgumentType.Error: {
                    this.Value = new LValue(LType.Number, (double)this.Data);
                    break;
                }

                case LArgumentType.Integer: {
                    this.Value = new LValue(LType.Number, (double)_reader.ReadInt32());
                    break;
                }

                case LArgumentType.Long: {
                    this.Value = new LValue(LType.Number, (double)_reader.ReadInt64());
                    break;
                }

                case LArgumentType.Double: {
                    this.Value = new LValue(LType.Number, (double)_reader.ReadDouble());
                    break;
                }

                case LArgumentType.String: {
                    this.Value = new LValue(LType.String, (string)_game.StringMapping[_reader.ReadInt32()].Value);
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

        public override void Perform(Game _assets, Domain _environment, LCode _code, Stack<LValue> _stack) {
            switch (this.Type) {
                case LArgumentType.Variable: {
                    if (this.Data == 0 && _stack.Count > 0) {
                        LValue _valPush = _stack.Pop();
                        Dictionary<string, LValue> _variableList = Helper.GetVariables(_assets, _environment, (double)_stack.Pop().Value);
                        LValue _varFind = _variableList[this.Variable.Name];
                        switch (_varFind.Type) {
                            case LType.Array: {
                                _stack.Push(_varFind.Array[(int)(double)_valPush.Value]);
                                break;
                            }

                            default: {
                                throw new Exception(String.Format("Could not handle push for type: {0}", _varFind.Type));
                            }
                        }
                    } else {
                        _stack.Push(Helper.GetVariables(_assets, _environment, this.Data)[this.Variable.Name]);
                    }
                    break;
                }

                default: {
                    _stack.Push(this.Value);
                    break;
                }
            }
        }
    }
    #endregion

    #region Pop Variations
    [InstructionDefinition(LOpcode.pop)]
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

                default: {
                    throw new Exception(String.Format("Could not pop from {0} to {1}", this.ArgTo, this.ArgFrom));
                }
            }
        }

        public override void Perform(Game _assets, Domain _environment, LCode _code, Stack<LValue> _stack) {
            if (_environment.ArrayNext == true) {
                int _arrayIndex = (int)(double)_stack.Pop().Value;
                Dictionary<string, LValue> _variableList = Helper.GetVariables(_assets, _environment, (double)_stack.Pop().Value);

                if (_variableList.ContainsKey(this.Variable.Name) == false || _variableList[this.Variable.Name].Type != LType.Array) {
                    _variableList[this.Variable.Name] = new LValue(LType.Array, new LValue[_arrayIndex + 1]);
                    for(int i = 0; i <= _arrayIndex; i++) {
                        _variableList[this.Variable.Name].Array[i] = new LValue(LType.Number, (double)0);
                    }
                }

                LValue _valGet = _variableList[this.Variable.Name];
                if (_arrayIndex > _valGet.Array.Length) {
                    LValue _valCopy = new LValue(LType.Array, new LValue[_arrayIndex + 1]);
                    for(int i = 0; i <= _arrayIndex; i++) {
                        if (i < _valGet.Array.Length) {
                            _valCopy.Array[i] = _valGet.Array[i];
                        } else {
                            _valCopy.Array[i] = new LValue(LType.Number, (double)0);
                        }
                    }
                    _variableList[this.Variable.Name] = _valCopy;
                    _valGet = _valCopy;
                }
                _valGet.Array[_arrayIndex] = _stack.Pop();
                _environment.ArrayNext = false;
            } else {
                double _varScope = this.Data;
                if (this.Data == 0) {
                    switch ((double)_stack.Pop()) {
                        case -9: {
                            _varScope = _stack.Pop();
                            break;
                        }
                    }
                }
                Helper.GetVariables(_assets, _environment, _varScope)[this.Variable.Name] = _stack.Pop();
            }
        }
    }
    #endregion

    #region Functions
    [InstructionDefinition(new LOpcode[] { LOpcode.call, LOpcode.callv })]
    class Call : Instruction {
        public string FunctionName;
        public Function.Handler Function;
        public LValue[] Arguments;
        public Int32 Count;
        public Call(Int32 _instruction, Game _game, LCode _code, BinaryReader _reader) : base(_instruction) {
            this.FunctionName = _game.Functions[_game.FunctionMapping[(int)((_code.Base + _reader.BaseStream.Position))]].Name;
            if (Runner.Function.Mapping.ContainsKey(this.FunctionName) == true) {
                this.Function = Runner.Function.Mapping[this.FunctionName];
            } else {
                throw new Exception(String.Format("Could not find function mapping for \"{0}\"", this.FunctionName));
            }

            this.Count = this.Data;
            this.Arguments = new LValue[this.Count];
            _reader.ReadInt32();
        }

        public override void Perform(Game _assets, Domain _environment, LCode _code, Stack<LValue> _stack) {
            for(int i = 0; i < this.Count; i++) this.Arguments[i] = _stack.Pop();
            _stack.Push(this.Function(_assets, _environment, this.Arguments, this.Count, _stack));
        }
    }
    #endregion

    #region Conditional
    [InstructionDefinition(LOpcode.set)]
    class Conditional : Instruction {
        public LConditionType Type;
        public Conditional(Int32 _instruction, Game _game, LCode _code, BinaryReader _reader) : base(_instruction) {
            this.Type = (LConditionType)((this.Data >> 8) & 0xFF);
        }

        public override void Perform(Game _assets, Domain _environment, LCode _code, Stack<LValue> _stack) {
            LValue _compRight = _stack.Pop();
            LValue _compLeft  = _stack.Pop();
            switch (this.Type) {
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
            this.Offset = (Int32)((_reader.BaseStream.Position + (this.Raw << 9 >> 7)) - 4);
        }

        public override void Perform(Game _assets, Domain _environment, LCode _code, Stack<LValue> _stack) {
            _environment.ProgramCounter = this.Jump;
        }
    }

    [InstructionDefinition(LOpcode.bt)]
    class BranchTrue : Branch {
        public BranchTrue(Int32 _instruction, Game _game, LCode _code, BinaryReader _reader) : base(_instruction, _game, _code, _reader) { }
        public override void Perform(Game _assets, Domain _environment, LCode _code, Stack<LValue> _stack) {
            if ((double)_stack.Pop().Value == 1) {
                _environment.ProgramCounter = this.Jump;
            }
        }
    }

    [InstructionDefinition(LOpcode.bf)]
    class BranchFalse : Branch {
        public BranchFalse(Int32 _instruction, Game _game, LCode _code, BinaryReader _reader) : base(_instruction, _game, _code, _reader) { }
        public override void Perform(Game _assets, Domain _environment, LCode _code, Stack<LValue> _stack) {
            if ((double)_stack.Pop().Value == 0) {
                _environment.ProgramCounter = this.Jump;
            }
        }
    }
    #endregion

    #region Math
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
        public override void Perform(Game _assets, Domain _environment, LCode _code, Stack<LValue> _stack) {
            LValue _valRight = _stack.Pop();
            LValue _valLeft = _stack.Pop();
            _stack.Push(_valLeft * _valRight);
        }
    }

    [InstructionDefinition(LOpcode.div)]
    class Divide : Instruction {
        public Divide(Int32 _instruction, Game _game, LCode _code, BinaryReader _reader) : base(_instruction) { }
        public override void Perform(Game _assets, Domain _environment, LCode _code, Stack<LValue> _stack) {
            LValue _valRight = _stack.Pop();
            LValue _valLeft = _stack.Pop();
            _stack.Push(_valLeft / _valRight);
        }
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
    #endregion

    #region Virtual Machine
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

    [InstructionDefinition(LOpcode.setowner)]
    class SetOwner : Instruction {
        public SetOwner(Int32 _instruction, Game _game, LCode _code, BinaryReader _reader) : base(_instruction) {}
        public override void Perform(Game _assets, Domain _environment, LCode _code, Stack<LValue> _stack) {
            _environment.ArrayNext = true;
        }
    }
    #endregion
}

namespace Luna.Instructions {
    static class Helper {
        public static Dictionary<string, LValue> GetVariables(Game _assets, Domain _environment, double _scope) {
            switch ((LVariableScope)_scope) {
                case LVariableScope.Instance: return _environment.Instance.Variables;
                case LVariableScope.Local: return _environment.Locals;
                default: {
                    LInstance _instGet = LInstance.Find(_assets, _scope);
                    if (_instGet != null) return _instGet.Variables;
                    throw new Exception(String.Format("Could not find object or instance with the index: {0}", _scope));
                }
            }
        }
    }
}