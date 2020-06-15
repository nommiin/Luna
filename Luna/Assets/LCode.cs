using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.IO;
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

            // Map out branching & environments
            for(int i = 0; i < this.Instructions.Count; i++) {
                switch (this.Instructions[i].Opcode) {
                    case LOpcode.b:
                    case LOpcode.bt:
                    case LOpcode.bf: {
                        Instructions.Branch _instructionBranch = this.Instructions[i] as Instructions.Branch;
                        if (this.BranchTable.ContainsKey(_instructionBranch.Offset) == true) {
                            _instructionBranch.Jump = this.BranchTable[_instructionBranch.Offset] - 1;
                        } else {
                            _instructionBranch.Jump = this.Instructions.Count;
                        }
                        break;
                    }

                    case LOpcode.pushenv: {
                        Instructions.PushEnvironment _instructionEnv = this.Instructions[i] as Instructions.PushEnvironment;
                        if (this.BranchTable.ContainsKey(_instructionEnv.Offset) == true) {
                            _instructionEnv.Jump = this.BranchTable[_instructionEnv.Offset];
                        } else {
                            _instructionEnv.Jump = this.Instructions.Count;
                        }
                        break;
                    }
                    case LOpcode.popenv: {
                        Instructions.PopEnvironment _instructionEnv = this.Instructions[i] as Instructions.PopEnvironment;
                        if (this.BranchTable.ContainsKey(_instructionEnv.Offset) == true) {
                            _instructionEnv.Jump = this.BranchTable[_instructionEnv.Offset];
                        } else {
                            _instructionEnv.Jump = this.Instructions.Count;
                        }
                        break;
                    }
                }
            }

            // Check for arrays
            for(int i = 0; i < this.Instructions.Count; i++) {
                switch (this.Instructions[i].Opcode) {
                    case LOpcode.setowner: {
                        // Find next pop instruction
                        for(int j = i; j < this.Instructions.Count; j++) {
                            if (this.Instructions[j].Opcode == LOpcode.pop) {
                                this.Instructions.RemoveAt(--i);   // push.i <id>
                                break;
                            } else if (this.Instructions[j].Opcode == LOpcode.call) {
                                Instructions.Call _instructionGet = this.Instructions[j] as Instructions.Call;
                                if (_instructionGet.FunctionName == "@@NewGMLArray@@") {
                                    this.Instructions.RemoveAt(--i);
                                    break;
                                }
                            }
                        }
                        break;
                    }
                }
            }

#if (DEBUG)
            // Print out finalized bytecode
            string _bytecodeOutput = this.Name + "\n";
            for (int i = 0; i < this.Instructions.Count; i++) {
                _bytecodeOutput += String.Format("[{1}] - {0}", this.Instructions[i].Opcode, this.Instructions[i].Raw.ToString("X"));
                switch (this.Instructions[i].Opcode) {
                    case LOpcode.call: {
                        Instructions.Call _instructionGet = this.Instructions[i] as Instructions.Call;
                        _bytecodeOutput += String.Format("(Function={0})", _instructionGet.FunctionName);
                        break;
                    }

                    case LOpcode.pop: {
                        Instructions.Pop _instructionGet = this.Instructions[i] as Instructions.Pop;
                        _bytecodeOutput += String.Format("(Variable={0}, Scope={1})", _instructionGet.Variable.Name, _instructionGet.Data);
                        break;
                    }

                    case LOpcode.push: {
                        Instructions.Push _instructionGet = this.Instructions[i] as Instructions.Push;
                        switch (_instructionGet.Type) {
                            case LArgumentType.Variable: {
                                _bytecodeOutput += String.Format("(Variable={0})", _instructionGet.Variable.Name);
                                break;
                            }

                            default: {
                                _bytecodeOutput += String.Format("(Value={0})", _instructionGet.Value.Value);
                                break;
                            }
                        }
                        break;
                    }

                    case LOpcode.pushb: {
                        Instructions.PushBuiltin _instructionGet = this.Instructions[i] as Instructions.PushBuiltin;
                        _bytecodeOutput += String.Format("(Variable={0})", _instructionGet.Variable);
                        break;
                    }

                    case LOpcode.pushi: {
                        Instructions.PushImmediate _instructionGet = this.Instructions[i] as Instructions.PushImmediate;
                        _bytecodeOutput += String.Format("(Value={0})", _instructionGet.Value.Value);
                        break;
                    }
                }
                _bytecodeOutput += "\n";
            }
            Console.WriteLine(_bytecodeOutput);
#endif
        }

        public override string ToString() {
            return $"Code: {this.Name}, Length: {this.Length} bytes, Locals: {this.LocalCount}, Arguments: {this.ArgCount}, Offset: {this.Offset}";
        }
    }
}
