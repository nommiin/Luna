using System;
using System.Collections.Generic;
using System.IO;

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
        public Int32 XOrigin;
        public Int32 YOrigin;
        public SpriteType SpriteType;
        public PlaybackType PlaybackType;
        public float PlaybackSpeed;
        public Int32 FrameCount;
        public List<LTexturePageEntry> TextureEntries;

        public LSprite(Game _assets, BinaryReader _reader) {
            this.Name = _assets.GetString(_reader.ReadInt32());
            this.Width = _reader.ReadInt32();
            this.Height = _reader.ReadInt32();
            this.BoundsLeft = _reader.ReadInt32();
            this.BoundsRight = _reader.ReadInt32();
            this.BoundsBottom = _reader.ReadInt32();
            this.BoundsTop = _reader.ReadInt32();
            this.Transparent = _reader.ReadLBoolean();
            this.Smooth = _reader.ReadLBoolean();
            this.Preload = _reader.ReadLBoolean();
            this.BoundsType = _reader.ReadInt32();
            this.CollisionType = (CollisionType)_reader.ReadInt32();
            this.XOrigin = _reader.ReadInt32();
            this.YOrigin = _reader.ReadInt32();
            _reader.BaseStream.Seek(sizeof(Int32) * 2, SeekOrigin.Current);
            this.SpriteType = (SpriteType)_reader.ReadInt32();
            this.PlaybackType = (PlaybackType)_reader.ReadInt32();
            this.PlaybackSpeed = _reader.ReadSingle();
            _reader.ReadInt32(); // TODO: Read sequence data @ position
            switch (this.SpriteType) {
                case SpriteType.Bitmap: {
                    this.TextureEntries = new List<LTexturePageEntry>();
                    ChunkHandler.HandleKVP(_assets, _reader, delegate(Int32 _offset) {
                        this.TextureEntries.Add(_assets.TextureEntries.Find(tpe=>tpe.Base == _offset));
                    });
                    break;
                }

                default: {
                    throw new Exception(String.Format("Could not read unsupported sprite type: {0}", this.SpriteType));
                }
            }
        }
    }
}
