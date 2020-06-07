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

        public Domain(LInstance _inst) {
            this.Scope = _inst.ID;
            this.Instance = _inst;
        }
    }
}
