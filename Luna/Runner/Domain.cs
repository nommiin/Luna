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

            Console.WriteLine("Instance Variables: {0}", Instance.Variables.Count);
            int i = 0;
            foreach(KeyValuePair<string, LValue> _var in Instance.Variables) {
                string _s = "";
                switch (_var.Value.Type) {
                    case LType.Array: {
                        List<LValue> _arr = _var.Value.Array;
                        _s += "[";
                        for(int j = 0; j < _arr.Count; j++) {
                            _s += _arr[j].Value.ToString();
                            if (j < _arr.Count - 1) _s += ", ";
                        }
                        _s += "]";
                        break;
                    }

                    default: {
                        _s = _var.Value.ToString();
                        break;
                    }
                }
                Console.WriteLine("{0}: {1} = {2}", i++, _var.Key, _s);
            }
            /*
            for(int i = 0; i < this.Instance.Variables.Count; i++) {
                Console.WriteLine("{0}: {1} = {2}", i, this.Instance.Variables[i].)
            }*/
        }
    }
}
