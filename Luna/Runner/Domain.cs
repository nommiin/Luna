using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Luna.Runner {
    class Domain {
        public Stack<LValue> Stack = new Stack<LValue>();
        public Dictionary<string, LValue> LocalVariables = new Dictionary<string, LValue>();
    }
}
