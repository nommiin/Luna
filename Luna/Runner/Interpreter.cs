using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Luna.Assets;
using Luna.Types;

namespace Luna.Runner {
    class Interpreter {
        public Game Assets;
        public Stack<LValue> Stack = new Stack<LValue>();
        public Int32 ProgramCounter;

        public Interpreter(Game _assets) {
            this.Assets = _assets;
        }

        public void ExecuteCode(Domain _environment, LCode _code) {
            Int32 _programLength = _code.Instructions.Count;
            for(this.ProgramCounter = 0; this.ProgramCounter < _programLength; this.ProgramCounter++) {
                _code.Instructions[this.ProgramCounter].Perform(this, _environment, _code, this.Stack);
            }
            //List<Instruction> _instList = _code.Instructions;
        }
    }
}
