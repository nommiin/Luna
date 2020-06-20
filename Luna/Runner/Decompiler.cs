using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Luna.Assets;
using Luna.Types;
using Luna.Runner;

namespace Luna.Runner.Debug {
    static class Decompiler {
        /*
            NOTE:
            This is a simple ad-hoc decompiler intended strictly for debugging purposes.
            Any pull requests or commits that attempt to turn this into a decompiler for
            the creating a GameMaker project are strictly prohibited and will be immediately
            declined.
        */
        
        public static string Decompile(List<Instruction> _instructionList) {
            string _decomOutput = "";
            Stack<dynamic> _decomStack = new Stack<dynamic>();
            foreach(Instruction _instructionGet in _instructionList) {
                switch (_instructionGet.Opcode) {
                    case LOpcode.b:
                    case LOpcode.bt:
                    case LOpcode.bf: {
                        Instructions.Branch _instructionBranch = _instructionGet as Instructions.Branch;
                        _decomOutput += "// branch->" + _instructionBranch.Jump + "\n";
                        break;
                    }

                    case LOpcode.pushi: {
                        Instructions.PushImmediate _instructionPushImmediate = _instructionGet as Instructions.PushImmediate;
                        _decomStack.Push(_instructionPushImmediate.Value.ToString());
                        break;
                    }

                    case LOpcode.push: {
                        Instructions.Push _instructionPush = _instructionGet as Instructions.Push;
                        switch (_instructionPush.Type) {
                            case LArgumentType.Variable: {
                                _decomStack.Push(_instructionPush.Variable.Name);
                                break;
                            }

                            case LArgumentType.String: {
                                _decomStack.Push("\"" + _instructionPush.Value.String + "\"");
                                break;
                            }

                            default: {
                                _decomStack.Push(_instructionPush.Value.Number.ToString());
                                break;
                            }
                        }
                        break;
                    }

                    case LOpcode.pop: {
                        Instructions.Pop _instructionPop = _instructionGet as Instructions.Pop;
                        switch (_instructionPop.Variable.Scope) {
                            case LVariableScope.Global: {
                                _decomOutput += "global.";
                                break;
                            }

                            case LVariableScope.Instance: {
                                _decomOutput += "self.";
                                break;
                            }

                            case LVariableScope.Local: {
                                _decomOutput += "local.";
                                break;
                            }

                            case LVariableScope.Static: {
                                _decomOutput += "static.";
                                break;
                            }
                        }
                        _decomOutput += _instructionPop.Variable.Name + " = ";
                        _decomOutput += _decomStack.Pop().ToString() + ";\n";
                        break;
                    }

                    case LOpcode.call: {
                        Instructions.Call _instructionCall = _instructionGet as Instructions.Call;
                        _decomOutput += _instructionCall.FunctionName + "(";
                        for(int i = 0; i < _instructionCall.Count; i++) {
                            _decomOutput += _decomStack.Pop().ToString();
                            if (i < _instructionCall.Count - 1) _decomOutput += ", ";
                        }
                        _decomOutput += ");\n";
                        break;
                    }

                    case LOpcode.exit: {
                        _decomOutput += "exit;\n";
                        break;
                    }
                }
            }
            return _decomOutput;
        }
    }
}
