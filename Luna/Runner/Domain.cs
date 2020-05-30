using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Luna.Types;

namespace Luna.Runner {
    class Domain {
        public LInstance Scope = null;
        public Stack<LValue> Stack = new Stack<LValue>();
        public Dictionary<string, LValue> LocalVariables = new Dictionary<string, LValue>();
    }
}
