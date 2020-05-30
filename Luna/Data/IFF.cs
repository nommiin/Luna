using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Luna.Assets;

namespace Luna {
    class IFF {
        public Game Assets;
        public MemoryStream Stream;
        public BinaryReader Reader;
        public Dictionary<string, Chunk> Chunks;
        public delegate void Callback(Game _data);

        public IFF(string _path, Game _data=null) {
            if (File.Exists(_path) == true) {
                this.Stream = new MemoryStream(File.ReadAllBytes(_path));
                this.Reader = new BinaryReader(this.Stream);
                using (Chunk _chunkHeader = new Chunk(this.Reader)) {
                    if (_chunkHeader.Name == "FORM") {
                        this.Assets = _data;
                        this.Chunks = new Dictionary<string, Chunk>();
                        this.Assets.Chunks = this.Chunks;
                        while (this.Reader.BaseStream.Position < _chunkHeader.Base + _chunkHeader.Length) {
                            Chunk _chunkGet = new Chunk(this.Reader);
                            this.Chunks[_chunkGet.Name] = _chunkGet;
                            this.Reader.BaseStream.Seek(_chunkGet.Length, SeekOrigin.Current);
                        }
                    } else throw new Exception("Invalid IFF file was given, got " + _chunkHeader.Name);
                }
            } else throw new FileNotFoundException("Could not find given IFF file", _path);
        }

        public void Parse(Callback _callback) {
            foreach(KeyValuePair<string, Chunk.Handler> _handlerGet in Chunk.Handlers) {
                if (this.Chunks.ContainsKey(_handlerGet.Key) == true) {
                    Chunk _chunkGet = this.Chunks[_handlerGet.Key];
                    if (_chunkGet != null) {
                        this.Reader.BaseStream.Seek(_chunkGet.Base, SeekOrigin.Begin);
                        _handlerGet.Value(this.Assets, this.Reader, _chunkGet);
                    }
                }
            }
            _callback(this.Assets);
        }
    }
}
