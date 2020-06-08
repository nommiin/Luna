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
        public Stack<bool> Ownership;
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
            this.Ownership = new Stack<bool>();
            this.Thread = new Thread(() => {
                this.Parse(_game);
            });
            this.Thread.Start();
            _game.Threads.Add(this.Thread);
        }

        public void Parse(Game _game) {
            // Parse all instructions
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

            // Map out branching
            for(int i = 0; i < this.Instructions.Count; i++) {
                switch (this.Instructions[i].Opcode) {
                    case LOpcode.b:
                    case LOpcode.bt:
                    case LOpcode.bf: {
                        Instructions.Branch _instructionBranch = this.Instructions[i] as Instructions.Branch;
                        if (this.BranchTable.ContainsKey(_instructionBranch.Offset) == true) {
                            _instructionBranch.Jump = this.BranchTable[_instructionBranch.Offset] - 1;
                            Console.WriteLine("Jump: {0} -> {1}", i, this.BranchTable[_instructionBranch.Offset] - 1);
                        } else {
                            // TODO: seems if a jump is at the end of the code, it breaks! for now it looks like just setting the jump to the last instruction is fine
                            _instructionBranch.Jump = this.Instructions.Count;
                            //throw new Exception("Could not find proper offset for branch instruction");
                        }
                        break;
                    }
                }
            }

            // Clean up useless instructions
            Stack<Instructions.Pop> _instructionRefs = new Stack<Instructions.Pop>();
            for(int i = 0; i < this.Instructions.Count; i++) {
                switch (this.Instructions[i].Opcode) {
                    case LOpcode.setowner: {
                        this.Instructions.RemoveAt(--i);
                        this.Instructions.RemoveAt(i--);
                        this.Instructions.RemoveAt(i + 2);

                        switch (this.Instructions[i + 2].Opcode) {
                            case LOpcode.pushi: {
                                Instructions.Pop _instructionRef = this.Instructions[i + 3] as Instructions.Pop;
                                _instructionRef.ArraySize = Math.Max(_instructionRef.ArraySize, (Int32)(double)(this.Instructions[i + 2] as Instructions.PushImmediate).Value.Value);
                                _instructionRefs.Push(_instructionRef);
                                break;
                            }
                        }
                        break;
                    }
                }
            }

            // Initialize all arrays
            while (_instructionRefs.Count > 0) {
                Instructions.Pop _instructionGet = _instructionRefs.Pop();
                _instructionGet.Value.Array = new List<LValue>();
                for (int i = 0; i < _instructionGet.ArraySize; i++) {
                    _instructionGet.Value.Array.Add(new LValue(LType.Number, (double)0));
                }
            }

            // Print out finalized bytecode
#if (DEBUG)
            Console.WriteLine(this.Name);
            for (int i = 0; i < this.Instructions.Count; i++) {
                Console.Write("{0} - {1} ", i, this.Instructions[i].Opcode);
                switch (this.Instructions[i].Opcode) {
                    case LOpcode.call: {
                        Instructions.Call _instGet = this.Instructions[i] as Instructions.Call;
                        Console.Write("(Function={0})", _instGet.FunctionName);
                        break;
                    }

                    case LOpcode.pop: {
                        Instructions.Pop _instGet = this.Instructions[i] as Instructions.Pop;
                        Console.Write("(Variable={0}, IsArray={1})", _instGet.Variable.Name, _instGet.IsArray);
                        break;
                    }

                    case LOpcode.push: {
                        Instructions.Push _instGet = this.Instructions[i] as Instructions.Push;
                        switch (_instGet.Type) {
                            case LArgumentType.Variable: {
                                Console.Write("(Variable={0})", _instGet.Variable.Name);
                                break;
                            }

                            default: {
                                Console.Write("(Value={0})", _instGet.Value.Value);
                                break;
                            }
                        }
                        break;
                    }

                    case LOpcode.pushi: {
                        Instructions.PushImmediate _instGet = this.Instructions[i] as Instructions.PushImmediate;
                        Console.Write("(Value={0})", _instGet.Value.Value);
                        break;
                    }
                }
                Console.WriteLine();
            }
            Console.WriteLine();
#endif
        }

        public override string ToString() {
            return $"Code: {this.Name}, Length: {this.Length} bytes, Locals: {this.LocalCount}, Arguments: {this.ArgCount}, Offset: {this.Offset}";
        }
    }
}
