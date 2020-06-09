using System.IO;

namespace Luna.Assets
{
    class LRoomView
    {
        public bool Enabled;
        public int X;
        public int Y;
        public int Width;
        public int Height;
        public int PortX;
        public int PortY;
        public int PortWidth;
        public int PortHeight;
        public uint BorderX;
        public uint BorderY;
        public int SpeedX;
        public int SpeedY;
        public LObject Target;
        public LRoomView(Game _assets, BinaryReader _reader){
            Enabled = _reader.ReadBoolean();
            X = _reader.ReadInt32();
            Y = _reader.ReadInt32();
            Width = _reader.ReadInt32();
            Height = _reader.ReadInt32();
            PortX = _reader.ReadInt32();
            PortY = _reader.ReadInt32();
            PortWidth = _reader.ReadInt32();
            PortHeight = _reader.ReadInt32();
            BorderX = _reader.ReadUInt32();
            BorderY = _reader.ReadUInt32();
            SpeedX = _reader.ReadInt32();
            SpeedY = _reader.ReadInt32();
            Target = _assets.ObjectMapping[_reader.ReadInt32()];
        }
    }
}