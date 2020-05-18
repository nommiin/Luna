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
        public long Base;

        public LString(BinaryReader _reader) {
            this.Offset = _reader.ReadInt32();
            this.Base = _reader.BaseStream.Position;
            _reader.BaseStream.Seek(this.Offset, SeekOrigin.Begin);
            this.Value = ASCIIEncoding.ASCII.GetString(_reader.ReadBytes(_reader.ReadInt32()));
            _reader.BaseStream.Seek(this.Base, SeekOrigin.Begin);
            this.Offset += 4;
#if (DEBUG == true)
            //Console.WriteLine("String: {0}, Offset: {1}", this.Value, this.Offset);
#endif
        }

        public LString(string _value) {
            this.Value = _value;
            this.Offset = -1;
            this.Base = -1;
        }

        public override string ToString() {
            return this.Value;
        }
    }
}
