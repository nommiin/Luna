using System;
using System.Collections.Generic;
using System.Drawing;
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
        public Bitmap BitmapData;

        public LTexture(BinaryReader _reader) {
            this.Scale = _reader.ReadInt32();
            this.Mipmaps = _reader.ReadInt32();
            this.Offset = _reader.ReadInt32();
            this.Base = _reader.BaseStream.Position;
        }

        public void LoadImage(BinaryReader _reader) {
            _reader.BaseStream.Seek(this.Offset, SeekOrigin.Begin);
            byte[] data = _reader.ReadBytes(this.Length);
            BitmapData = new Bitmap(new MemoryStream(data));
        }
    }
}
