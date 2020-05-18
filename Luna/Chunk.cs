using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace Luna {
    class Chunk : IDisposable {
        public string Name;
        public Int32 Length;
        public long Base;

        public Chunk(BinaryReader _reader) {
            this.Name = ASCIIEncoding.ASCII.GetString(_reader.ReadBytes(4));
            this.Length = _reader.ReadInt32();
            this.Base = _reader.BaseStream.Position;
            Console.WriteLine(this.ToString());
        }

        public override string ToString() {
            return $"Name: {this.Name}, Length: {this.Length} bytes, Base: {this.Base}";
        }

        public void Dispose() {
            //
        }
    }
}
