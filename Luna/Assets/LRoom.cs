using System;
using System.Collections.Generic;
using System.IO;

namespace Luna.Assets {
    class LRoom {
        public long Index;
        public string Name;
        public string Caption;
        public Int32 Width;
        public Int32 Height;
        public Int32 Speed;
        public bool Persistent;
        public Int32 Colour;
        public bool ShowColour;
        public LCode CreationCode;
        public bool EnableViews;
        public bool ClearViewport;
        public bool ClearBuffer;
        public bool PhysWorld;
        public Int32 PhysWorldTop;
        public Int32 PhysWorldLeft;
        public Int32 PhysWorldRight;
        public Int32 PhysWorldBottom;
        public Single PhysGravityX;
        public Single PhysGravityY;
        public Single PhysConversion;
        public List<LRoomInstance> Instances = new List<LRoomInstance>();
        public LRoomView[] Views = new LRoomView[8];

        public LRoom(Game _game, BinaryReader _reader) {
            this.Name = _game.GetString(_reader.ReadInt32());
            this.Caption = _game.GetString(_reader.ReadInt32());
            this.Width = _reader.ReadInt32();
            this.Height = _reader.ReadInt32();
            this.Speed = _reader.ReadInt32();
            this.Persistent = _reader.ReadLBoolean();
            this.Colour = _reader.ReadInt32();
            this.ShowColour = _reader.ReadLBoolean();
            Int32 _codeIndex = _reader.ReadInt32();
            if (_codeIndex != -1) {
                this.CreationCode = _game.CodeMapping[_codeIndex];
            } else this.CreationCode = null;
            Int32 _flagGet = _reader.ReadInt32() & 0xF;
            this.EnableViews = ((_flagGet & 1) == 1 ? true : false);
            this.ClearViewport = (((_flagGet >> 1) & 1) == 1 ? true : false);
            this.ClearBuffer = (((_flagGet >> 2) & 1) == 1 ? true : false);
            _reader.ReadInt32(); // Backgrounds
            int _i = 0;
            ChunkHandler.HandleList(_game, _reader, delegate(Int32 _offset) {
                Views[_i++] = new LRoomView(_game,_reader);
            });
            ChunkHandler.HandleList(_game, _reader, delegate (Int32 _offset) {
                this.Instances.Add(new LRoomInstance(_game, _reader));
            });
            _reader.ReadInt32(); // TILES
            this.PhysWorld = _reader.ReadLBoolean();
            this.PhysWorldTop = _reader.ReadInt32();
            this.PhysWorldLeft = _reader.ReadInt32();
            this.PhysWorldRight = _reader.ReadInt32();
            this.PhysWorldBottom = _reader.ReadInt32();
            this.PhysGravityX = _reader.ReadSingle();
            this.PhysGravityY = _reader.ReadSingle();
            this.PhysConversion = _reader.ReadSingle();
            _reader.ReadInt32(); // LAYERS
            _reader.ReadInt32(); // SEQUENCES
            // TODO: Parse remaining data, such as phyiscs and layer data.
        }

        public override string ToString() {
            return $"Room: {this.Name}, Size: {this.Width}x{this.Height}, Creation Code: {this.CreationCode}";
        }
    }
}
