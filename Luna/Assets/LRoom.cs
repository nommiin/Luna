using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
            Name = _game.GetString(_reader.ReadInt32());
            Caption = _game.GetString(_reader.ReadInt32());
            Width = _reader.ReadInt32();
            Height = _reader.ReadInt32();
            Speed = _reader.ReadInt32();
            Persistent = _reader.ReadInt32() == 1 ? true : false;
            Colour = _reader.ReadInt32();
            ShowColour = _reader.ReadInt32() == 1 ? true : false;
            Int32 _codeIndex = _reader.ReadInt32();
            if (_codeIndex != -1) {
                CreationCode = _game.CodeMapping[_codeIndex];
            } else CreationCode = null;
            Int32 _flagGet = _reader.ReadInt32() & 0xF;
            EnableViews = (_flagGet & 1) == 1 ? true : false;
            ClearViewport = ((_flagGet >> 1) & 1) == 1 ? true : false;
            ClearBuffer = ((_flagGet >> 2) & 1) == 1 ? true : false;
            _reader.ReadInt32(); // Backgrounds
            int _i = 0;
            ChunkHandler.HandleList(_game, _reader, delegate(Int32 _offset) {
                Views[_i++] = new LRoomView(_game,_reader);
            });
            ChunkHandler.HandleList(_game, _reader, delegate (Int32 _offset) {
                Instances.Add(new LRoomInstance(_game, _reader));
            });
            _reader.ReadInt32(); // TILES
            PhysWorld = _reader.ReadInt32() == 1 ? true : false;
            PhysWorldTop = _reader.ReadInt32();
            PhysWorldLeft = _reader.ReadInt32();
            PhysWorldRight = _reader.ReadInt32();
            PhysWorldBottom = _reader.ReadInt32();
            PhysGravityX = _reader.ReadSingle();
            PhysGravityY = _reader.ReadSingle();
            PhysConversion = _reader.ReadSingle();
            _reader.ReadInt32(); // LAYERS
            _reader.ReadInt32(); // SEQUENCES
            // TODO: Parse remaining data, such as phyiscs and layer data.
        }

        public override string ToString() {
            return $"Room: {Name}, Size: {Width}x{Height}, Creation Code: {CreationCode}";
        }
    }
}
