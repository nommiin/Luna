using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Luna.Types;

namespace Luna.Assets {
    
    class LRoomInstance {
        public Int32 X;
        public Int32 Y;
        public LObject Index;
        public Int32 ID;
        public LCode CreationCode;
        public Single ScaleX;
        public Single ScaleY;
        public Single ImageSpeed;
        public Int32 ImageIndex;
        public LColour ImageBlend;
        public Single Rotation;
        public LCode PreCreate;

        public LRoomInstance(Game _assets, BinaryReader _reader) {
            X = _reader.ReadInt32();
            Y = _reader.ReadInt32();
            Index = _assets.ObjectMapping[_reader.ReadInt32()];
            ID = _reader.ReadInt32();
            Int32 _creationIndex = _reader.ReadInt32();
            if (_creationIndex != -1) {
                CreationCode = _assets.CodeMapping[_creationIndex];
            } else CreationCode = null;
            ScaleX = _reader.ReadSingle();
            ScaleY = _reader.ReadSingle();
            ImageSpeed = _reader.ReadSingle();
            ImageIndex = _reader.ReadInt32();
            ImageBlend = new LColour(_reader.ReadInt32());
            Rotation = _reader.ReadSingle();
            Int32 _createIndex = _reader.ReadInt32();
            if (_createIndex != -1) {
                PreCreate = _assets.CodeMapping[_createIndex];
            } else PreCreate = null;
        }
    }
}
