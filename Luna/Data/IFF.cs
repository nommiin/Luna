using System;
using System.Collections.Generic;
using System.IO;

namespace Luna {
    class IFF {
        public Game Assets;
        public MemoryStream Stream;
        public BinaryReader Reader;
        public Dictionary<string, Chunk> Chunks;
        public delegate void Callback(Game _data);

        public IFF(string _path, Game _data=null) {
            Stream = new MemoryStream(File.ReadAllBytes(_path));
            Reader = new BinaryReader(Stream);
            using (Chunk _chunkHeader = new Chunk(Reader)) {
                if (_chunkHeader.Name == "FORM") {
                    Assets = _data;
                    Chunks = new Dictionary<string, Chunk>();
                    Assets.Chunks = Chunks;
                    while (Reader.BaseStream.Position < _chunkHeader.Base + _chunkHeader.Length) {
                        Chunk _chunkGet = new Chunk(Reader);
                        Chunks[_chunkGet.Name] = _chunkGet;
                        Reader.BaseStream.Seek(_chunkGet.Length, SeekOrigin.Current);
                    }
                } else throw new Exception("Invalid IFF file was given, got " + _chunkHeader.Name);
            }
        }

        public void Parse(Callback _callback) {
            foreach(KeyValuePair<string, Chunk.Handler> _handlerGet in Chunk.Handlers) {
                if (Chunks.ContainsKey(_handlerGet.Key)) {
                    Chunk _chunkGet = Chunks[_handlerGet.Key];
                    if (_chunkGet != null) {
                        Reader.BaseStream.Seek(_chunkGet.Base, SeekOrigin.Begin);
                        _handlerGet.Value(Assets, Reader, _chunkGet);
                        if (Reader.BaseStream.Position > _chunkGet.Base+_chunkGet.Length) throw new IOException("Reading outside of chunk!");
                    }
                }
            }
            for (int i = 0; i < Assets.Threads.Count; i++) Assets.Threads[i].Join();
            _callback(Assets);
        }
    }
}
