using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace Luna.Types {
    class LTexture {
        public Int32 Scale;
        public Int32 Mipmaps;
        public Int32 Offset;
        public Int32 Length;
        public long Base;

        public LTexture(BinaryReader _reader) {
            this.Scale = _reader.ReadInt32();
            this.Mipmaps = _reader.ReadInt32();
            this.Offset = _reader.ReadInt32();
            this.Base = _reader.BaseStream.Position;
        }
    }
}
