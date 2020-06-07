using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.IO;
using Luna.Types;
using Luna.Runner;

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
        public List<Instruction> Instructions;
        public Dictionary<long, Int32> BranchTable;
        public Thread Thread;

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
            this.Instructions = new List<Instruction>();
            this.BranchTable = new Dictionary<long, Int32>();
            this.Thread = new Thread(() => {
                this.Parse(_game);
            });
            this.Thread.Start();
            _game.Threads.Add(this.Thread);
        }

        public void Parse(Game _game) {
            while (this.Reader.BaseStream.Position < this.Reader.BaseStream.Length) {
                this.BranchTable[this.Reader.BaseStream.Position] = this.Instructions.Count;
                Int32 _instructionGet = this.Reader.ReadInt32();
                LOpcode _opcodeGet = Instruction.GetOpcode(_instructionGet);
                if (Instruction.Ignore.Contains(_opcodeGet) == false) {
                    if (Instruction.Mapping.ContainsKey(_opcodeGet) == true) {
                        this.Instructions.Add(Instruction.Mapping[_opcodeGet](_instructionGet, _game, this, this.Reader));
                    } else {
                        throw new Exception(String.Format("Could not find instruction mapping for {0} at {1} in {2}", _opcodeGet, this.Reader.BaseStream.Position, this.Name));
                    }
                }
            }

            for(int i = 0; i < this.Instructions.Count; i++) {
                Instructions.Branch _instructionGet = this.Instructions[i] as Instructions.Branch;
                switch (this.Instructions[i].Opcode) {
                    case LOpcode.b:
                    case LOpcode.bt:
                    case LOpcode.bf: {
                        if (this.BranchTable.ContainsKey(_instructionGet.Offset) == true) {
                            _instructionGet.Jump = this.BranchTable[_instructionGet.Offset] - 1;
                            Console.WriteLine("Jump: {0} -> {1}", i, this.BranchTable[_instructionGet.Offset] - 1);
                        } else {
                            throw new Exception("Could not find proper offset for branch instruction");
                        }
                        break;
                    }
                }
            }
        }

        public override string ToString() {
            return $"Code: {this.Name}, Length: {this.Length} bytes, Locals: {this.LocalCount}, Arguments: {this.ArgCount}, Offset: {this.Offset}";
        }
    }
}
