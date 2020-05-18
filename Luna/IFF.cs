using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Luna.Assets;

namespace Luna {
    class IFF {
        public Game Data;
        public MemoryStream Stream;
        public BinaryReader Reader;
        public BinaryWriter Writer;
        public Dictionary<string, Chunk> Chunks;
        public delegate void Handler(Game _game, BinaryReader _reader, BinaryWriter _writer, Chunk _chunk);
        public Dictionary<string, Handler> Handlers = new Dictionary<string, Handler>() {
            { "STRG", ChunkHandlers.STRG },
            { "GEN8", ChunkHandlers.GEN8 },
            { "ROOM", ChunkHandlers.ROOM },
            { "VARI", ChunkHandlers.VARI }
        };

        public IFF(string _path, Game _data=null) {
            if (File.Exists(_path) == true) {
                this.Stream = new MemoryStream(File.ReadAllBytes(_path));
                this.Reader = new BinaryReader(this.Stream);
                this.Writer = new BinaryWriter(this.Stream);
                using (Chunk _chunkHeader = new Chunk(this.Reader)) {
                    if (_chunkHeader.Name == "FORM") {
                        this.Data = _data;
                        this.Chunks = new Dictionary<string, Chunk>();
                        this.Data.Chunks = this.Chunks;
                        while (this.Reader.BaseStream.Position < _chunkHeader.Base + _chunkHeader.Length) {
                            Chunk _chunkGet = new Chunk(this.Reader);
                            this.Chunks[_chunkGet.Name] = _chunkGet;
                            this.Reader.BaseStream.Seek(_chunkGet.Length, SeekOrigin.Current);
                        }
                    } else throw new Exception("Invalid IFF file was given, got " + _chunkHeader.Name);
                }
            } else throw new FileNotFoundException("Could not find given IFF file", _path);
        }

        public void Parse() {
            foreach(KeyValuePair<string, Handler> _handlerGet in this.Handlers) {
                if (this.Chunks.ContainsKey(_handlerGet.Key) == true) {
                    Chunk _chunkGet = this.Chunks[_handlerGet.Key];
                    if (_chunkGet != null) {
                        this.Reader.BaseStream.Seek(_chunkGet.Base, SeekOrigin.Begin);
                        _handlerGet.Value(this.Data, this.Reader, this.Writer, _chunkGet);
                    }
                }
            }
        }
    }

    static class ChunkHandlers {
        public static void STRG(Game _game, BinaryReader _reader, BinaryWriter _writer, Chunk _chunk) {
            for (Int32 i = 0, _i = _reader.ReadInt32(); i < _i; i++) {
                LString _stringGet = new LString(_reader);
                _game.Strings.Add(_stringGet.Offset, _stringGet);
            }
        }

        public static void GEN8(Game _game, BinaryReader _reader, BinaryWriter _writer, Chunk _chunk) {
            _game.DebugEnabled = ((_reader.ReadInt32() - 4352) == 0 ? true : false);
            _game.Name = _game.GetString(_reader.ReadInt32());
            _game.Configuration = _game.GetString(_reader.ReadInt32());
            _game.RoomMax = _reader.ReadInt32();
            _game.RoomMaxTile = _reader.ReadInt32();
            _game.ID = _reader.ReadInt32();
            _reader.BaseStream.Seek(sizeof(Int32) * 4, SeekOrigin.Current);
            _game.GameName = _game.GetString(_reader.ReadInt32());
            _game.Build = new Version(_reader.ReadInt32(), _reader.ReadInt32(), _reader.ReadInt32(), _reader.ReadInt32());
            _game.RoomWidth = _reader.ReadInt32();
            _game.RoomHeight = _reader.ReadInt32();
            Int32 _gameFlags = _reader.ReadInt32();
            _game.Flags.Fullscreen = new LBoolean(_gameFlags & 1);
            _game.Flags.Interpolate = new LBoolean((_gameFlags >> 3) & 1);
            _game.Flags.Scale = new LBoolean((_gameFlags >> 4) & 1);
            _game.Flags.ShowCursor = new LBoolean((_gameFlags >> 5) & 1);
            _game.Flags.Resizable = new LBoolean((_gameFlags >> 6) & 1);
            _game.Flags.Screenshot = new LBoolean((_gameFlags >> 7) & 1);
            _game.Flags.IsSteam = new LBoolean((_gameFlags >> 13) & 1);
            _game.Flags.IsPlayer = _game.Flags.IsSteam;
            _game.Flags.Borderless = new LBoolean((_gameFlags >> 14) & 1);
            _game.Flags.IsJavaScript = new LBoolean((_gameFlags >> 15) & 1);
            _game.CRC = _reader.ReadInt32();
            _game.MD5 = _reader.ReadBytes(16);
            _game.Timestamp = _reader.ReadInt64();
            _game.DisplayName = _game.GetString(_reader.ReadInt32());
#if (DEBUG == true)
            Console.WriteLine("Project: Name: {0}, Game Name: {1}, Display Name: {2}", _game.Name, _game.GameName, _game.DisplayName);
#endif
            _game.Flags.Targets = _reader.ReadInt64();
            _game.Classifications = _reader.ReadInt64();
            _game.AppID = _reader.ReadInt32();
            _game.DebugPort = _reader.ReadInt32();
            _game.RoomCount = _reader.ReadInt32();
            for (Int32 i = 0; i < _game.RoomCount; i++) {
                _game.RoomOrder.Add(_reader.ReadInt32());
            }

            if (_game.Build.Major >= 2) {
                _reader.BaseStream.Seek(sizeof(Int64) * 5, SeekOrigin.Current);
                _game.GameSpeed = _reader.ReadSingle();
                _game.AllowStats = _reader.ReadBoolean();
                _game.GUID = new Guid(_reader.ReadBytes(16));
            }
        }

        public static void ROOM(Game _game, BinaryReader _reader, BinaryWriter _writer, Chunk _chunk) {
            for (Int32 i = 0, _i = _reader.ReadInt32(); i < _i; i++) {
                LRoom _roomGet = new LRoom(_game, _reader);
                _game.Rooms.Add(_roomGet.Name, _roomGet);
            }
        }

        public static void VARI(Game _game, BinaryReader _reader, BinaryWriter _writer, Chunk _chunk) {
            _game.LocalVariables = _reader.ReadInt32();
            _game.InstanceVariables = _reader.ReadInt32();
            _game.GlobalVariables = _reader.ReadInt32();
            while (_reader.BaseStream.Position < _chunk.Base + _chunk.Length) {
                LVariable _varGet = new LVariable(_game, _reader);
                if (_varGet.Count > 0) {
                    _game.VariableMapping[_varGet.Offset + 4] = _game.Variables.Count;
                    for (Int32 i = 0; i < _varGet.Count - 1; i++) {
                        if (i > 0) _game.VariableMapping[(Int32)_reader.BaseStream.Position] = _game.Variables.Count;
                        _reader.BaseStream.Seek(_varGet.Offset + 4, SeekOrigin.Begin);
                        _varGet.Offset += _reader.ReadInt32() & 0xFFFF;
                    }
                }
                _game.Variables.Add(_varGet);
                _reader.BaseStream.Seek(_varGet.Base, SeekOrigin.Begin);
                if ((_chunk.Base + _chunk.Length) - _reader.BaseStream.Position < LVariable.Length) break;
            }

#if (DEBUG == true)
            foreach(KeyValuePair<int, int> _varMap in _game.VariableMapping) {
                Console.WriteLine("{0} => {1}.{2}", _varMap.Key, _game.Variables[_varMap.Value].Type, _game.Variables[_varMap.Value].Name);
            }
#endif
        }
    }
}
