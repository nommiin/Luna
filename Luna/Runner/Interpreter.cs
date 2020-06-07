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

        public Interpreter(Game _assets) {
            this.Assets = _assets;
        }
    }
}
