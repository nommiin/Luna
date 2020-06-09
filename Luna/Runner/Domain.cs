using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Luna.Runner;
using Luna.Assets;
using Luna.Types;

namespace Luna.Runner {
    class Domain {
        public double Scope = 0;
        public LInstance Instance = null;
        public Stack<LValue> Stack = new Stack<LValue>();
        public Int32 ProgramCounter = 0;

        public Domain(LInstance _inst) {
            Scope = _inst.ID;
            Instance = _inst;
        }

        public void ExecuteCode(Game _assets, LCode _code) {
            Int32 _programLength = _code.Instructions.Count;
            for (ProgramCounter = 0; ProgramCounter < _programLength; ProgramCounter++) {
                _code.Instructions[ProgramCounter].Perform(_assets, this, _code, Stack);
            }
        }
    }
}
