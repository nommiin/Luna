using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace Luna.Assets {
    class LCode {
        public long Base;
        public string Name;
        public Int32 Length;
        public Int16 LocalCount;
        public Int16 ArgCount;
        public byte Flags;
        public Int32 Offset;
        public MemoryStream Bytecode;
        public BinaryReader Reader;
        public Dictionary<long, Int32> BranchTable;
        public List<Instruction> InstructionList;
        public static Dictionary<LOpcode, Func<Int32, Game, LCode, BinaryReader, Instruction>> InstructionMapping = new Dictionary<LOpcode, Func<Int32, Game, LCode, BinaryReader, Instruction>>() {
            { LOpcode.pushi, (Int32 _instruction, Game _game, LCode _code, BinaryReader _reader) => new Luna.Instructions.PushImmediate(_instruction, _game, _code, _reader) },
            { LOpcode.pushl, (Int32 _instruction, Game _game, LCode _code, BinaryReader _reader) => new Luna.Instructions.Push(_instruction, _game, _code, _reader) },
            { LOpcode.push, (Int32 _instruction, Game _game, LCode _code, BinaryReader _reader) => new Luna.Instructions.Push(_instruction, _game, _code, _reader) },
            { LOpcode.pushg, (Int32 _instruction, Game _game, LCode _code, BinaryReader _reader) => new Luna.Instructions.PushGlobal(_instruction, _game, _code, _reader) },
            { LOpcode.pushb, (Int32 _instruction, Game _game, LCode _code, BinaryReader _reader) => new Luna.Instructions.PushBuiltin(_instruction, _game, _code, _reader) },
            { LOpcode.pop, (Int32 _instruction, Game _game, LCode _code, BinaryReader _reader) => new Luna.Instructions.Pop(_instruction, _game, _code, _reader) },
            { LOpcode.set, (Int32 _instruction, Game _game, LCode _code, BinaryReader _reader) => new Luna.Instructions.Conditional(_instruction, _game, _code, _reader) },
            { LOpcode.b, (Int32 _instruction, Game _game, LCode _code, BinaryReader _reader) => new Luna.Instructions.Branch(_instruction, _game, _code, _reader) },
            { LOpcode.bt, (Int32 _instruction, Game _game, LCode _code, BinaryReader _reader) => new Luna.Instructions.BranchTrue(_instruction, _game, _code, _reader) },
            { LOpcode.bf, (Int32 _instruction, Game _game, LCode _code, BinaryReader _reader) => new Luna.Instructions.BranchFalse(_instruction, _game, _code, _reader) },
            { LOpcode.call, (Int32 _instruction, Game _game, LCode _code, BinaryReader _reader) => new Luna.Instructions.Call(_instruction, _game, _code, _reader) },
            { LOpcode.add, (Int32 _instruction, Game _game, LCode _code, BinaryReader _reader) => new Luna.Instructions.Add(_instruction, _game, _code, _reader) },
            { LOpcode.sub, (Int32 _instruction, Game _game, LCode _code, BinaryReader _reader) => new Luna.Instructions.Subtract(_instruction, _game, _code, _reader) },
            { LOpcode.mul, (Int32 _instruction, Game _game, LCode _code, BinaryReader _reader) => new Luna.Instructions.Multiply(_instruction, _game, _code, _reader) },
            { LOpcode.div, (Int32 _instruction, Game _game, LCode _code, BinaryReader _reader) => new Luna.Instructions.Divide(_instruction, _game, _code, _reader) },
            { LOpcode.rem, (Int32 _instruction, Game _game, LCode _code, BinaryReader _reader) => new Luna.Instructions.Remainder(_instruction, _game, _code, _reader) },
            { LOpcode.mod, (Int32 _instruction, Game _game, LCode _code, BinaryReader _reader) => new Luna.Instructions.Modulo(_instruction, _game, _code, _reader) },
            { LOpcode.xor, (Int32 _instruction, Game _game, LCode _code, BinaryReader _reader) => new Luna.Instructions.Xor(_instruction, _game, _code, _reader) },
            { LOpcode.and, (Int32 _instruction, Game _game, LCode _code, BinaryReader _reader) => new Luna.Instructions.And(_instruction, _game, _code, _reader) },
            { LOpcode.not, (Int32 _instruction, Game _game, LCode _code, BinaryReader _reader) => new Luna.Instructions.Not(_instruction, _game, _code, _reader) },
            { LOpcode.neg, (Int32 _instruction, Game _game, LCode _code, BinaryReader _reader) => new Luna.Instructions.Negate(_instruction, _game, _code, _reader) },
            { LOpcode.or, (Int32 _instruction, Game _game, LCode _code, BinaryReader _reader) => new Luna.Instructions.Or(_instruction, _game, _code, _reader) },
            { LOpcode.shl, (Int32 _instruction, Game _game, LCode _code, BinaryReader _reader) => new Luna.Instructions.ShiftLeft(_instruction, _game, _code, _reader) },
            { LOpcode.shr, (Int32 _instruction, Game _game, LCode _code, BinaryReader _reader) => new Luna.Instructions.ShiftRight(_instruction, _game, _code, _reader) },
            { LOpcode.dup, (Int32 _instruction, Game _game, LCode _code, BinaryReader _reader) => new Luna.Instructions.Duplicate(_instruction, _game, _code, _reader) },
            { LOpcode.popz, (Int32 _instruction, Game _game, LCode _code, BinaryReader _reader) => new Luna.Instructions.Discard(_instruction, _game, _code, _reader) },
            { LOpcode.brk, (Int32 _instruction, Game _game, LCode _code, BinaryReader _reader) => new Luna.Instructions.Discard(_instruction, _game, _code, _reader) },
        };

        public LCode(Game _game, BinaryReader _reader) {
            this.Name = _game.GetString(_reader.ReadInt32());
            this.Length = _reader.ReadInt32();
            this.LocalCount = (short)(_reader.ReadInt16() & 0x1FFF);
            this.ArgCount = _reader.ReadInt16();
            this.Flags = (byte)((this.ArgCount >> 13) & 7);
            this.ArgCount = (short)(this.ArgCount & 0x1FFF);
            this.Offset = _reader.ReadInt32() - 4;
            _reader.BaseStream.Seek(this.Offset, SeekOrigin.Current);
            this.Base = _reader.BaseStream.Position;
            this.Bytecode = new MemoryStream(_reader.ReadBytes(this.Length));
            this.Reader = new BinaryReader(this.Bytecode);
            this.BranchTable = new Dictionary<long, int>();
            this.InstructionList = new List<Instruction>();

            // Parse instructions
            while (this.Reader.BaseStream.Position < this.Reader.BaseStream.Length) {
                BranchTable[this.Reader.BaseStream.Position] = this.InstructionList.Count;
                Int32 _instructionGet = this.Reader.ReadInt32();
                LOpcode _instructionOpcode = Instruction.GetOpcode(_instructionGet);
                if (_instructionOpcode != LOpcode.conv) {
                    if (LCode.InstructionMapping.ContainsKey(_instructionOpcode) == true) {
                        this.InstructionList.Add(InstructionMapping[_instructionOpcode](_instructionGet, _game, this, this.Reader));
                    } else {
                        throw new Exception(String.Format("Could not find instruction mapping for \"{0}\"", _instructionOpcode));
                    }
                }
            }

            // Update branches
            for(int i = 0; i < this.InstructionList.Count; i++) {
                Instructions.Branch _instGet = this.InstructionList[i] as Instructions.Branch;
                switch (this.InstructionList[i].Opcode) {
                    case LOpcode.b:
                    case LOpcode.bt:
                    case LOpcode.bf: {
                        if (this.BranchTable.ContainsKey(_instGet.Offset) == true) {
                            _instGet.Jump = this.BranchTable[_instGet.Offset] - 1;
                            Console.WriteLine("Jump: {0} -> {1}", i, this.BranchTable[_instGet.Offset] - 1);
                        } else {
                            throw new Exception("Could not find proper offset for branch instruction");
                        }
                        break;
                    }
                }
            }
#if (DEBUG == true)
            Console.WriteLine(this);
            Console.WriteLine("Bytecode:\n{0}", BitConverter.ToString(this.Bytecode.ToArray()).Replace("-", " "));
#endif
        }

        public override string ToString() {
            return $"Code: {this.Name}, Length: {this.Length} bytes, Locals: {this.LocalCount}, Arguments: {this.ArgCount}, Offset: {this.Offset}";
        }
    }
}
