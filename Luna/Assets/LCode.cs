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
            Name = _game.GetString(_reader.ReadInt32());
            Length = _reader.ReadInt32();
            LocalCount = (short)(_reader.ReadInt16() & 0x1FFF);
            ArgCount = _reader.ReadInt16();
            Flags = (byte)((ArgCount >> 13) & 7);
            ArgCount = (short)(ArgCount & 0x1FFF);
            Offset = _reader.ReadInt32() - 4;
            _reader.BaseStream.Seek(Offset, SeekOrigin.Current);
            Base = _reader.BaseStream.Position;
            Bytecode = new MemoryStream(_reader.ReadBytes(Length));
            Reader = new BinaryReader(Bytecode);
            Instructions = new List<Instruction>();
            BranchTable = new Dictionary<long, Int32>();
            Thread = new Thread(() => {
                Parse(_game);
            });
            Thread.Start();
            _game.Threads.Add(Thread);
        }

        public void Parse(Game _game) {
            while (Reader.BaseStream.Position < Reader.BaseStream.Length) {
                BranchTable[Reader.BaseStream.Position] = Instructions.Count;
                Int32 _instructionGet = Reader.ReadInt32();
                LOpcode _opcodeGet = Instruction.GetOpcode(_instructionGet);
                if (!Instruction.Ignore.Contains(_opcodeGet)) {
                    if (Instruction.Mapping.ContainsKey(_opcodeGet)) {
                        Console.WriteLine("Opcode in Code: {0} in {1}",_opcodeGet,Name);
                        Instructions.Add(Instruction.Mapping[_opcodeGet](_instructionGet, _game, this, Reader));
                    } else {
                        throw new Exception(String.Format("Could not find instruction mapping for {0} at {1} in {2}", _opcodeGet, Reader.BaseStream.Position, Name));
                    }
                }
            }

            for(int i = 0; i < Instructions.Count; i++) {
                Instructions.Branch _instructionGet = Instructions[i] as Instructions.Branch;
                switch (Instructions[i].Opcode) {
                    case LOpcode.b:
                    case LOpcode.bt:
                    case LOpcode.bf: {
                        if (BranchTable.ContainsKey(_instructionGet.Offset)) {
                            _instructionGet.Jump = BranchTable[_instructionGet.Offset] - 1;
                            Console.WriteLine("Jump: {0} -> {1}", i, BranchTable[_instructionGet.Offset] - 1);
                        } else {
                            // TODO: seems if a jump is at the end of the code, it breaks! for now it looks like just setting the jump to the last instruction is fine
                            _instructionGet.Jump = Instructions.Count;
                            //throw new Exception("Could not find proper offset for branch instruction");
                        }
                        break;
                    }
                }
            }
        }

        public override string ToString() {
            return $"Code: {Name}, Length: {Length} bytes, Locals: {LocalCount}, Arguments: {ArgCount}, Offset: {Offset}";
        }
    }
}
