using System;
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
            this.X = _reader.ReadInt32();
            this.Y = _reader.ReadInt32();
            this.Index = _assets.ObjectMapping[_reader.ReadInt32()];
            this.ID = _reader.ReadInt32();
            Int32 _creationIndex = _reader.ReadInt32();
            if (_creationIndex != -1) {
                this.CreationCode = _assets.CodeMapping[_creationIndex];
            } else CreationCode = null;
            this.ScaleX = _reader.ReadSingle();
            this.ScaleY = _reader.ReadSingle();
            this.ImageSpeed = _reader.ReadSingle();
            this.ImageIndex = _reader.ReadInt32();
            this.ImageBlend = new LColour(_reader.ReadInt32());
            this.Rotation = _reader.ReadSingle();
            Int32 _createIndex = _reader.ReadInt32();
            if (_createIndex != -1) {
                this.PreCreate = _assets.CodeMapping[_createIndex];
            } else this.PreCreate = null;
        }
    }
}
