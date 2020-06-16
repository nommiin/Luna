using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace Luna {
    class Chunk : IDisposable {
        public delegate void Handler(Game _game, BinaryReader _reader, Chunk _chunk);
        public static Dictionary<string, Handler> Handlers = new Dictionary<string, Handler>() {
            { "STRG", ChunkHandler.STRG },
            { "GEN8", ChunkHandler.GEN8 },
            { "TXTR", ChunkHandler.TXTR },
            { "VARI", ChunkHandler.VARI },
            { "FUNC", ChunkHandler.FUNC },
            { "CODE", ChunkHandler.CODE },
            { "SCPT", ChunkHandler.SCPT },
            { "GLOB", ChunkHandler.GLOB },
            { "SPRT", ChunkHandler.SPRT },
            { "OBJT", ChunkHandler.OBJT },
            { "ROOM", ChunkHandler.ROOM }
        };

        public string Name;
        public Int32 Length;
        public long Base;

        public Chunk(BinaryReader _reader) {
            Name = Encoding.ASCII.GetString(_reader.ReadBytes(4));
            Length = _reader.ReadInt32();
            Base = _reader.BaseStream.Position;
#if (DEBUG == true)
            Console.WriteLine(this);
#endif
        }

        public void Dispose() { }

        public override string ToString() {
            return $"Chunk: {Name}, Length: {Length} bytes";
        }
    }
}
