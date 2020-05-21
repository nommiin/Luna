using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace Luna.Types {
    class LString {
        public string Value;
        public Int32 Offset;

        public LString(BinaryReader _reader, Int32 _offset) {
            this.Value = ASCIIEncoding.ASCII.GetString(_reader.ReadBytes(_reader.ReadInt32()));
            this.Offset = _offset + 4;
        }

        public LString(string _value) {
            this.Value = _value;
            this.Offset = -1;
        }

        public override string ToString() {
            return this.Value;
        }
    }
}
