using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Luna;
using Luna.Assets;
using Luna.Runner;
using Luna.Types;

namespace Luna.Types {
    class LMap {
        public static List<LMap> Registry = new List<LMap>();
        public static LMap GetMap(Int32 _index) {
            return LMap.Registry[_index];
        }

        public Int32 Index;
        public Dictionary<dynamic, LValue> Data;
        public LMap() {
            this.Data = new Dictionary<dynamic, LValue>();
            this.Index = LMap.Registry.Count;
            LMap.Registry.Add(this);
        }
    }
}
