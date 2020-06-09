using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Luna.Types;

namespace Luna.Assets {
    public enum SpriteType {
        Bitmap,
        Flash,
        Spine,
        Vector
    }

    public enum CollisionType {
        AlignedRectangle,
        Precise,
        RotatedRectangle
    }

    public enum PlaybackType {
        Loop,
        PingPong
    }

    class LSprite {
        public long Index;
        public string Name;
        public Int32 Width;
        public Int32 Height;
        public Int32 BoundsLeft;
        public Int32 BoundsRight;
        public Int32 BoundsBottom;
        public Int32 BoundsTop;
        public bool Transparent;
        public bool Smooth;
        public bool Preload;
        public Int32 BoundsType;
        public CollisionType CollisionType;
        public Int32 OriginX;
        public Int32 OriginY;
        public SpriteType SpriteType;
        public PlaybackType PlaybackType;
        public float PlaybackSpeed;
        public Int32 FrameCount;
        // public LTexturePage[] TextureEntry;

        public LSprite(Game _assets, BinaryReader _reader) {
            Name = _assets.GetString(_reader.ReadInt32());
            Width = _reader.ReadInt32();
            Height = _reader.ReadInt32();
            BoundsLeft = _reader.ReadInt32();
            BoundsRight = _reader.ReadInt32();
            BoundsBottom = _reader.ReadInt32();
            BoundsTop = _reader.ReadInt32();
            Transparent = _reader.ReadInt32() == 1 ? true : false;
            Smooth = _reader.ReadInt32() == 1 ? true : false;
            Preload = _reader.ReadInt32() == 1 ? true : false;
            BoundsType = _reader.ReadInt32();
            CollisionType = (CollisionType)_reader.ReadInt32();
            OriginX = _reader.ReadInt32();
            OriginY = _reader.ReadInt32();
            _reader.BaseStream.Seek(sizeof(Int32) * 2, SeekOrigin.Current);
            SpriteType = (SpriteType)_reader.ReadInt32();
            PlaybackType = (PlaybackType)_reader.ReadInt32();
            PlaybackSpeed = _reader.ReadSingle();
            _reader.ReadInt32(); // TODO: Read sequence data @ position
            switch (SpriteType) {
                case SpriteType.Bitmap: {
                    FrameCount = _reader.ReadInt32();
                    for(int i = 0; i < FrameCount; i++) {
                        _reader.ReadInt32();
                        // TODO: Read texture page indices
                    }
                    break;
                }

                default: {
                    throw new Exception(String.Format("Could not read unsupported sprite type: {0}", SpriteType));
                }
            }
        }
    }
}
