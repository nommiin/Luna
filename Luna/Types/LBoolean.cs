using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Luna.Types {
    class LBoolean {
        public bool Value;

        public LBoolean(Int32 _val) {
            this.Value = (_val >= 0.5);
        }

        public static implicit operator bool(LBoolean b) => b.Value;
        public static explicit operator LBoolean(bool b) => new LBoolean(1);
    }
}
