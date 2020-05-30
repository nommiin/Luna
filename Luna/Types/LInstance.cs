using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Luna.Types {
    class LInstance {
        public Int32 ID;
        public Dictionary<string, LValue> Variables;

        public LInstance(Int32 _id) {
            this.ID = _id;
            this.Variables = new Dictionary<string, LValue>();
        }
    }
}
