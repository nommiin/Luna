using System;
using System.Text;
using System.IO;

namespace Luna.Types {
    class LString {
        public string Value;
        public Int32 Offset;

        public LString(BinaryReader _reader, Int32 _offset) {
            this.Value = Encoding.ASCII.GetString(_reader.ReadBytes(_reader.ReadInt32()));
            this.Offset = _offset + 4;
        }

        public override string ToString() {
            return this.Value;
        }
    }
}
