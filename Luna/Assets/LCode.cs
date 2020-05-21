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
        public Int32 Magic;
        public Int32 Offset;
        public MemoryStream Bytecode;
        public BinaryReader Reader;

        public LCode(Game _game, BinaryReader _reader) {
            Int32 _codeOffset = _reader.ReadInt32();
            this.Base = _reader.BaseStream.Position;
            _reader.BaseStream.Seek(_codeOffset, SeekOrigin.Begin); ;
            this.Name = _game.GetString(_reader.ReadInt32());
            this.Length = _reader.ReadInt32();
            this.LocalCount = (short)(_reader.ReadInt16() & 0x1FFF);
            this.ArgCount = _reader.ReadInt16();
            Console.WriteLine("Local Count: {0}", this.LocalCount);
            this.Flags = (byte)((this.ArgCount >> 13) & 7);
            Console.WriteLine("Flags: {0}", this.Flags);
            this.ArgCount = (short)(this.ArgCount & 0x1FFF);
            this.Magic = _reader.ReadInt32();
            this.Offset = _reader.ReadInt32();
            _reader.BaseStream.Seek((_reader.BaseStream.Position - 4) + this.Offset, SeekOrigin.Begin);
            Console.WriteLine("Base: {0}, ArgCount: {1}", _reader.BaseStream.Position, this.ArgCount);
            this.Bytecode = new MemoryStream(_reader.ReadBytes(this.Length));
            this.Reader = new BinaryReader(this.Bytecode);
            _reader.BaseStream.Seek(this.Base, SeekOrigin.Begin);
#if (DEBUG)
            Console.WriteLine("Code: {0}, Length: {1} bytes, Magic: {2}, Offset: {3}", this.Name, this.Length, this.Magic, this.Offset);
            Console.WriteLine("Bytecode:\n{0}", BitConverter.ToString(this.Bytecode.ToArray()).Replace("-", " "));
            Console.WriteLine("First Instruction: {0}", Instruction.Decode(this.Reader.ReadInt32()));
#endif
        }
    }
}
