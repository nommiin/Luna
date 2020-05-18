using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace Luna {
    class Chunk : IDisposable {
        #region Handlers
        public delegate void Handler(Game _game, BinaryReader _reader, BinaryWriter _writer, Chunk _chunk);
        public static Dictionary<string, Handler> Handlers = new Dictionary<string, Handler>() {
            { "STRG", ChunkHandlers.STRG },
            { "GEN8", ChunkHandlers.GEN8 },
            { "ROOM", ChunkHandlers.ROOM },
            { "VARI", ChunkHandlers.VARI }
        };

        #endregion

        public string Name;
        public Int32 Length;
        public long Base;

        public Chunk(BinaryReader _reader) {
            this.Name = ASCIIEncoding.ASCII.GetString(_reader.ReadBytes(4));
            this.Length = _reader.ReadInt32();
            this.Base = _reader.BaseStream.Position;
#if (DEBUG == true)
            Console.WriteLine("Chunk: {0}, Length: {1} bytes", this.Name, this.Length);
#endif
        }

        public void Dispose() {
            //
        }
    }
}
