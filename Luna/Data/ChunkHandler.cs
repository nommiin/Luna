using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Luna.Assets;
using Luna.Types;

namespace Luna {
    static class ChunkHandler {
        public delegate void KVP(Int32 _base);
        private static void HandleKVP(Game _game, BinaryReader _reader, BinaryWriter _writer, Chunk _chunk, KVP _handler) {
            Int32 _keyCount = _reader.ReadInt32();
            for(Int32 i = 0; i < _keyCount; i++) {
                Int32 _keyOffset = _reader.ReadInt32(), _keyBase = (Int32)_reader.BaseStream.Position;
                _reader.BaseStream.Seek(_keyOffset, SeekOrigin.Begin);
                _handler(_keyOffset);
                _reader.BaseStream.Seek(_keyBase, SeekOrigin.Begin);
            }
        }

        public static void STRG(Game _game, BinaryReader _reader, BinaryWriter _writer, Chunk _chunk) {
            HandleKVP(_game, _reader, _writer, _chunk, delegate (Int32 _offset) {
                LString _stringGet = new LString(_reader, _offset);
                _game.Strings.Add(_stringGet.Offset, _stringGet);
                _game.StringMapping.Add(_stringGet);
            });
#if (DEBUG == true)
            Console.WriteLine("Read {0} strings", _game.Strings.Count);
#endif
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
            HandleKVP(_game, _reader, _writer, _chunk, delegate (Int32 _offset) {
                LRoom _roomGet = new LRoom(_game, _reader);
                _game.Rooms.Add(_roomGet.Name, _roomGet);
            });
        }

        public static void CODE(Game _game, BinaryReader _reader, BinaryWriter _writer, Chunk _chunk) {
            HandleKVP(_game, _reader, _writer, _chunk, delegate (Int32 _offset) {
                LCode _codeGet = new LCode(_game, _reader);
                _game.Code.Add(_codeGet.Name, _codeGet);
            });
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
            foreach (KeyValuePair<int, int> _varMap in _game.VariableMapping) {
                Console.WriteLine("{0} => {1}.{2}", _varMap.Key, _game.Variables[_varMap.Value].Type, _game.Variables[_varMap.Value].Name);
            }
#endif
        }

        public static void FUNC(Game _game, BinaryReader _reader, BinaryWriter _writer, Chunk _chunk) {
            Int32 _funcCount = _reader.ReadInt32();
#if (DEBUG == true)
            Console.WriteLine("Function Count: {0}", _funcCount);
#endif
            for (Int32 i = 0; i < _funcCount; i++) {
                LFunction _funcGet = new LFunction(_game, _reader);
                if (_funcGet.Count > 0) {
                    _game.FunctionMapping[_funcGet.Offset + 4] = _game.Functions.Count;
                    for(Int32 j = 0; j < _funcGet.Count - 1; j++) {
                        if (j > 0) _game.FunctionMapping[(Int32)_reader.BaseStream.Position] = _game.Functions.Count;
                        _reader.BaseStream.Seek(_funcGet.Offset + 4, SeekOrigin.Begin);
                        _funcGet.Offset += _reader.ReadInt32() & 0xFFFF;
                    }
                    _reader.BaseStream.Seek(_funcGet.Base, SeekOrigin.Begin);
                }
                _game.Functions.Add(_funcGet);
            }

#if (DEBUG == true)
            foreach (KeyValuePair<int, int> _funcMap in _game.FunctionMapping) {
                Console.WriteLine("Call {0} at {1}", _game.Functions[_funcMap.Value].Name, _funcMap.Key);
            }
#endif

            Int32 _localCount = _reader.ReadInt32();
#if (DEBUG == true)
            Console.WriteLine("Local Count: {0}", _localCount);
#endif
            for (Int32 i = 0; i < _localCount; i++) {
                Int32 _localUses = _reader.ReadInt32();
                string _codeName = _game.GetString(_reader.ReadInt32());
                for(Int32 j = 0; j < _localUses; j++) {
                    Int32 _localInd = _reader.ReadInt32();
                    string _localName = _game.GetString(_reader.ReadInt32());
#if (DEBUG == true)
                    Console.WriteLine("{0} in {1} ({2}/{3})", _localName, _codeName, j + 1, _localUses);
#endif
                }
            }
        }
    }
}
