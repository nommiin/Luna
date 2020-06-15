using System;
using System.Drawing;
using System.IO;
using Luna.Assets;
using Luna.Types;

namespace Luna {
    static class ChunkHandler {
        public delegate void ChunkKVP(Int32 _offset);
        public static void HandleKVP(Game _assets, BinaryReader _reader, ChunkKVP _handler) {
            Int32 _keyCount = _reader.ReadInt32();
            for(Int32 i = 0; i < _keyCount; i++) {
                Int32 _keyOffset = _reader.ReadInt32(), _keyBase = (Int32)_reader.BaseStream.Position;
                _reader.BaseStream.Seek(_keyOffset, SeekOrigin.Begin);
                _handler(_keyOffset);
                _reader.BaseStream.Seek(_keyBase, SeekOrigin.Begin);
            }
        }

        public delegate void ChunkList(Int32 _offset);
        public static void HandleList(Game _assets, BinaryReader _reader, ChunkList _handler) {
            Int32 _listOffset = _reader.ReadInt32(), _listBase = (Int32)_reader.BaseStream.Position;
            _reader.BaseStream.Seek(_listOffset, SeekOrigin.Begin);
            Int32 _itemCount = _reader.ReadInt32();
            for(Int32 i = 0; i < _itemCount; i++) {
                Int32 _itemOffset = _reader.ReadInt32(), _itemBase = (Int32)_reader.BaseStream.Position;
                _reader.BaseStream.Seek(_itemOffset, SeekOrigin.Begin);
                _handler(_itemOffset);
                _reader.BaseStream.Seek(_itemBase, SeekOrigin.Begin);
            }
            _reader.BaseStream.Seek(_listBase, SeekOrigin.Begin);
        }
        
        //gonna have to be serious got no idea if this works
        public delegate void ChunkZeus();
        public static void HandleZeus(Game _assets, BinaryReader _reader, ChunkZeus _handler) {
            int _count = _reader.ReadInt32();
            for (int i = 0; i < _count; i++) {
                _handler();
            }
        }

        public static void STRG(Game _assets, BinaryReader _reader, Chunk _chunk) {
            HandleKVP(_assets, _reader, delegate (Int32 _offset) {
                LString _stringGet = new LString(_reader, _offset);
                _assets.Strings.Add(_stringGet.Offset, _stringGet);
                _assets.StringMapping.Add(_stringGet);
            });
        }

        public static void GEN8(Game _assets, BinaryReader _reader, Chunk _chunk) {
            _assets.DebugEnabled = ((_reader.ReadInt32() - 4352) == 0 ? true : false);
            _assets.Name = _assets.GetString(_reader.ReadInt32());
            _assets.Configuration = _assets.GetString(_reader.ReadInt32());
            _assets.RoomMax = _reader.ReadInt32();
            _assets.RoomMaxTile = _reader.ReadInt32();
            _assets.ID = _reader.ReadInt32();
            _reader.BaseStream.Seek(sizeof(Int32) * 4, SeekOrigin.Current);
            _assets.GameName = _assets.GetString(_reader.ReadInt32());
            _assets.Build = new Version(_reader.ReadInt32(), _reader.ReadInt32(), _reader.ReadInt32(), _reader.ReadInt32());
            _assets.RoomWidth = _reader.ReadInt32();
            _assets.RoomHeight = _reader.ReadInt32();
            Int32 _gameFlags = _reader.ReadInt32();
            _assets.Flags.Fullscreen = (_gameFlags & 1) == 1 ? true : false;
            _assets.Flags.Interpolate = ((_gameFlags >> 3) & 1) == 1 ? true : false;
            _assets.Flags.Scale = ((_gameFlags >> 4) & 1) == 1 ? true : false;
            _assets.Flags.ShowCursor = ((_gameFlags >> 5) & 1) == 1 ? true : false;
            _assets.Flags.Resizable = ((_gameFlags >> 6) & 1) == 1 ? true : false;
            _assets.Flags.Screenshot = ((_gameFlags >> 7) & 1) == 1 ? true : false;
            _assets.Flags.IsSteam = ((_gameFlags >> 13) & 1) == 1 ? true : false;
            _assets.Flags.IsPlayer = _assets.Flags.IsSteam;
            _assets.Flags.Borderless = ((_gameFlags >> 14) & 1) == 1 ? true : false;
            _assets.Flags.IsJavaScript = ((_gameFlags >> 15) & 1) == 1 ? true : false;
            _assets.CRC = _reader.ReadInt32();
            _assets.MD5 = _reader.ReadBytes(16);
            _assets.Timestamp = _reader.ReadInt64();
            _assets.DisplayName = _assets.GetString(_reader.ReadInt32());
            _assets.Flags.Targets = _reader.ReadInt64();
            _assets.Classifications = _reader.ReadInt64();
            _assets.AppID = _reader.ReadInt32();
            _assets.DebugPort = _reader.ReadInt32();
            _assets.RoomCount = _reader.ReadInt32();
            for (Int32 i = 0; i < _assets.RoomCount; i++) {
                _assets.RoomIndices.Add(_reader.ReadInt32());
            }

            if (_assets.Build.Major >= 2) {
                _reader.BaseStream.Seek(sizeof(Int64) * 5, SeekOrigin.Current);
                _assets.GameSpeed = _reader.ReadSingle();
                _assets.AllowStats = _reader.ReadLBoolean();
                _assets.GUID = new Guid(_reader.ReadBytes(16));
            }
        }

        public static void VARI(Game _assets, BinaryReader _reader, Chunk _chunk) {
            _assets.LocalVariables = _reader.ReadInt32();
            _assets.InstanceVariables = _reader.ReadInt32();
            _assets.GlobalVariables = _reader.ReadInt32();
            while (_reader.BaseStream.Position < _chunk.Base + _chunk.Length) {
                LVariable _varGet = new LVariable(_assets, _reader);
                if (_varGet.Count > 0) {
                    _assets.VariableMapping[_varGet.Offset] = _assets.Variables.Count;
                    for (Int32 i = 0; i < _varGet.Count; i++) {
                        if (i > 0) _assets.VariableMapping[_varGet.Offset] = _assets.Variables.Count;
                        _reader.BaseStream.Seek(_varGet.Offset + 4, SeekOrigin.Begin);
                        _varGet.Offset += _reader.ReadInt32() & 0xFFFF;
                    }
                }
                _assets.Variables.Add(_varGet);
                _reader.BaseStream.Seek(_varGet.Base, SeekOrigin.Begin);
                if ((_chunk.Base + _chunk.Length) - _reader.BaseStream.Position < LVariable.Length) break;
            }
        }

        public static void FUNC(Game _assets, BinaryReader _reader, Chunk _chunk) {
            Int32 _funcCount = _reader.ReadInt32();
            for (Int32 i = 0; i < _funcCount; i++) {
                LFunction _funcGet = new LFunction(_assets, _reader);
                if (_funcGet.Count > 0) {
                    _assets.FunctionMapping[_funcGet.Offset] = _assets.Functions.Count;
                    for(Int32 j = 0; j < _funcGet.Count; j++) {
                        if (j > 0) _assets.FunctionMapping[_funcGet.Offset] = _assets.Functions.Count;
                        _reader.BaseStream.Seek(_funcGet.Offset, SeekOrigin.Begin);
                        _funcGet.Offset += _reader.ReadInt32() & 0xFFFF;
                    }
                    _reader.BaseStream.Seek(_funcGet.Base, SeekOrigin.Begin);
                }
                _assets.Functions.Add(_funcGet);
            }

            /*
            Int32 _localCount = _reader.ReadInt32();
            for (Int32 i = 0; i < _localCount; i++) {
                Int32 _localUses = _reader.ReadInt32();
                string _codeName = _assets.GetString(_reader.ReadInt32());
                for(Int32 j = 0; j < _localUses; j++) {
                    Int32 _localInd = _reader.ReadInt32();
                    string _localName = _assets.GetString(_reader.ReadInt32());
                }
            }
            */
        }

        public static void CODE(Game _assets, BinaryReader _reader, Chunk _chunk) {
            HandleKVP(_assets, _reader, delegate (Int32 _offset) {
                LCode _codeGet = new LCode(_assets, _reader);
                _assets.Code.Add(_codeGet.Name, _codeGet);
                _assets.CodeMapping.Add(_codeGet);
            });
        }

        public static void SCPT(Game _assets, BinaryReader _reader, Chunk _chunk) {
            HandleKVP(_assets, _reader, delegate (Int32 _offset) {
                LScript _scriptGet = new LScript(_assets, _reader);
                _assets.Scripts.Add(_scriptGet.Name, _scriptGet);
                _assets.ScriptMapping.Insert((Int32)_scriptGet.Index, _scriptGet);
            });
        }

        public static void GLOB(Game _assets, BinaryReader _reader, Chunk _chunk) {
            for(Int32 i = 0, _i = _reader.ReadInt32(); i < _i; i++) {
                _assets.GlobalScripts.Add(_assets.CodeMapping[_reader.ReadInt32()]);
            }
        }

        public static void ROOM(Game _assets, BinaryReader _reader, Chunk _chunk) {
            HandleKVP(_assets, _reader, delegate (Int32 _offset) {
                LRoom _roomGet = new LRoom(_assets, _reader);
                _assets.Rooms.Add(_roomGet.Name, _roomGet);
                _roomGet.Index = _assets.RoomMapping.Count;
                _assets.RoomMapping.Add(_roomGet);
            });

            for(Int32 i = 0; i < _assets.RoomIndices.Count; i++) {
                _assets.RoomOrder.Insert(i, _assets.RoomMapping[_assets.RoomIndices[i]]);
            }
        }

        public static void TXTR(Game _assets, BinaryReader _reader, Chunk _chunk)
        {
            HandleList(_assets, _reader, delegate(int _offset){
                LEmbTexture _tex = new LEmbTexture(_assets, _reader);
                _tex.Texture.Save(_offset+".png");
            });
            Environment.Exit(0);
        }

        public static void SPRT(Game _assets, BinaryReader _reader, Chunk _chunk) {
            HandleKVP(_assets, _reader, delegate (Int32 _offset) {
                LSprite _spriteGet = new LSprite(_assets, _reader);
                _assets.Sprites.Add(_spriteGet.Name, _spriteGet);
                _spriteGet.Index = _assets.SpriteMapping.Count;
                _assets.SpriteMapping.Add(_spriteGet);
            });
        }

        public static void OBJT(Game _assets, BinaryReader _reader, Chunk _chunk) {
            HandleKVP(_assets, _reader, delegate (Int32 _offset) {
                LObject _objectGet = new LObject(_assets, _reader);
                _assets.Objects.Add(_objectGet.Name, _objectGet);
                _objectGet.Index = _assets.ObjectMapping.Count;
                _assets.ObjectMapping.Add(_objectGet);
            });
        }

        public static void SEQN(Game _assets, BinaryReader _reader, Chunk _chunk) {
            HandleKVP(_assets, _reader, delegate(int _offset) {
                LSequence _seqGet = new LSequence(_assets, _reader);
                _assets.Sequences.Add(_seqGet.Name,_seqGet);
                _seqGet.Index = _assets.SequenceMapping.Count;
                _assets.SequenceMapping.Add(_seqGet);
            });
        }
    }
}
