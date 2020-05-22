using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace Luna.Assets {
    class LCode {
        public long Base;
        public string Name;
        public Int32 Length;
        public Int16 LocalCount;
        public Int16 ArgCount;
        public byte Flags;
        public Int32 Offset;
        public MemoryStream Bytecode;
        public BinaryReader Reader;

        public LCode(Game _game, BinaryReader _reader) {
            this.Name = _game.GetString(_reader.ReadInt32());
            this.Length = _reader.ReadInt32();
            this.LocalCount = (short)(_reader.ReadInt16() & 0x1FFF);
            this.ArgCount = _reader.ReadInt16();
            this.Flags = (byte)((this.ArgCount >> 13) & 7);
            this.ArgCount = (short)(this.ArgCount & 0x1FFF);
            this.Offset = _reader.ReadInt32() - 4;
            _reader.BaseStream.Seek(this.Offset, SeekOrigin.Current);
            this.Base = _reader.BaseStream.Position;
            this.Bytecode = new MemoryStream(_reader.ReadBytes(this.Length));
            this.Reader = new BinaryReader(this.Bytecode);
#if (DEBUG == true)
            Console.WriteLine(this);
            Console.WriteLine("Bytecode:\n{0}", BitConverter.ToString(this.Bytecode.ToArray()).Replace("-", " "));
#endif
        }

        public override string ToString() {
            return $"Code: {this.Name}, Length: {this.Length} bytes, Locals: {this.LocalCount}, Arguments: {this.ArgCount}, Offset: {this.Offset}";
        }
    }
}
