using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Luna.Assets;
using Luna.Types;
using Luna.Runner;

namespace Luna.Runner {
    class Decompiler {
        /*
            NOTE:
            This is a simple ad-hoc decompiler intended strictly for debugging purposes.
            Any pull requests or commits that attempt to turn this into a decompiler for
            the creating a GameMaker project are strictly prohibited and will be immediately
            declined.
        */

        public string Output = "";
        public Decompiler(LCode _code, List<Instruction> _instructionList) {
            Stack<dynamic> _programStack = new Stack<dynamic>();

            for(int i = 0; i < _instructionList.Count; i++) {
                Instruction _instructionGet = _instructionList[i];
                switch (_instructionGet.Opcode) {
                    case LOpcode.push: {
                        _programStack.Push((_instructionGet as Instructions.Push).Value.ToString());
                        break;
                    }

                    case LOpcode.pushi: {
                        _programStack.Push((_instructionGet as Instructions.PushImmediate).Value.ToString());
                        break;
                    }

                    case LOpcode.pushb: {
                        _programStack.Push((_instructionGet as Instructions.PushBuiltin).Variable);
                        break;
                    }

                    case LOpcode.pop: {
                        Instructions.Pop _instructionPop = _instructionGet as Instructions.Pop;
                        switch ((LVariableScope)_instructionPop.Data) {
                            case LVariableScope.Global: this.Output += "global."; break;
                            case LVariableScope.Instance: this.Output += "self."; break;
                            case LVariableScope.Local: this.Output += "var "; break;
                            case LVariableScope.Static: this.Output += "static "; break;
                            default: {
                                this.Output += $"({_instructionPop.Data}).";
                                break;
                            }
                        }
                        this.Output += $"{_instructionPop.Variable.Name} = {_programStack.Pop()};\n";
                        break;
                    }

                    case LOpcode.popz: {
                        _programStack.Pop();
                        break;
                    }

                    case LOpcode.call: {
                        Instructions.Call _instructionCall = _instructionGet as Instructions.Call;
                        this.Output += $"{_instructionCall.FunctionName}(";
                        for(int j = 0; j < _instructionCall.Count; j++) {
                            this.Output += _programStack.Pop();
                            if (j < _instructionCall.Count - 1) this.Output += ", ";
                        }
                        this.Output += ");\n";
                        _programStack.Push(0);
                        break;
                    }
                }
            }
        }
    }
}
