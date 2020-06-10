using System;
using System.IO;

namespace Luna.Assets {
    class LRoomView {
        public bool Enabled;
        public Int32 X;
        public Int32 Y;
        public Int32 Width;
        public Int32 Height;
        public Int32 PortX;
        public Int32 PortY;
        public Int32 PortWidth;
        public Int32 PortHeight;
        public Int32 BorderX;
        public Int32 BorderY;
        public Int32 SpeedX;
        public Int32 SpeedY;
        public LObject Target;

        public LRoomView(Game _assets, BinaryReader _reader) {
            this.Enabled = _reader.ReadLBoolean();
            this.X = _reader.ReadInt32();
            this.Y = _reader.ReadInt32();
            this.Width = _reader.ReadInt32();
            this.Height = _reader.ReadInt32();
            this.PortX = _reader.ReadInt32();
            this.PortY = _reader.ReadInt32();
            this.PortWidth = _reader.ReadInt32();
            this.PortHeight = _reader.ReadInt32();
            this.BorderX = _reader.ReadInt32();
            this.BorderY = _reader.ReadInt32();
            this.SpeedX = _reader.ReadInt32();
            this.SpeedY = _reader.ReadInt32();
            Int32 _viewTarget = _reader.ReadInt32();
            if (_viewTarget != -1) {
                this.Target = _assets.ObjectMapping[_viewTarget];
            } else this.Target = null;
        }
    }
}