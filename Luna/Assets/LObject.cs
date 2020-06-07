using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Luna.Types;

namespace Luna.Assets {
    class LObject {
        public long Index;
        public string Name;
        public LSprite Sprite;
        public bool Visible;
        public bool Solid;
        public Int32 Depth;
        public bool Persistent;
        public LObject Parent;
        public LSprite Mask;
        public bool PhysObject;
        public bool PhysSensor;
        public Int32 PhysShape;
        public Single PhysDensity;
        public Single PhysResitution;
        public Int32 PhysGroup;
        public Single PhysLinearDamping;
        public Single PhysAngularDamping;
        public Int32 PhysVertCount;
        public Single PhysFriction;
        public bool PhysAwake;
        public bool PhysKinematic;
        public List<LEvent> Events = new List<LEvent>();
        public LCode PreCreate = null;
        public LCode Create = null;
        public LCode Step = null;
        public LCode Draw = null;
        
        public LObject(Game _assets, BinaryReader _reader) {
            this.Name = _assets.GetString(_reader.ReadInt32());
            Int32 _spriteIndex = _reader.ReadInt32();
            if (_spriteIndex != -1) {
                this.Sprite = _assets.SpriteMapping[_spriteIndex];
            } else this.Sprite = null;
            this.Visible = (_reader.ReadInt32() == 1 ? true : false);
            this.Solid = (_reader.ReadInt32() == 1 ? true : false);
            this.Depth = _reader.ReadInt32();
            this.Persistent = (_reader.ReadInt32() == 1 ? true : false);
            Int32 _parentIndex = _reader.ReadInt32();
            if (_parentIndex != -100) {
                this.Parent = _assets.ObjectMapping[_parentIndex];
            } else this.Parent = null;
            Int32 _maskIndex = _reader.ReadInt32();
            if (_maskIndex != -1) {
                this.Mask = _assets.SpriteMapping[_maskIndex];
            } else this.Mask = this.Sprite;
            this.PhysObject = (_reader.ReadInt32() == 1 ? true : false);
            this.PhysSensor = (_reader.ReadInt32() == 1 ? true : false);
            this.PhysShape = _reader.ReadInt32();
            this.PhysDensity = _reader.ReadSingle();
            this.PhysResitution = _reader.ReadSingle();
            this.PhysGroup = _reader.ReadInt32();
            this.PhysLinearDamping = _reader.ReadSingle();
            this.PhysAngularDamping = _reader.ReadSingle();
            this.PhysVertCount = _reader.ReadInt32();
            this.PhysFriction = _reader.ReadSingle();
            this.PhysAwake = (_reader.ReadInt32() == 1 ? true : false);
            this.PhysKinematic = (_reader.ReadInt32() == 1 ? true : false);
            _reader.BaseStream.Seek(sizeof(Single) * this.PhysVertCount, SeekOrigin.Current);
            ChunkHandler.HandleKVP(_assets, _reader, delegate (Int32 _eventOffset) {
                ChunkHandler.HandleKVP(_assets, _reader, delegate (Int32 _typeOffset) {
                    Int32 _eventSubtype = _reader.ReadInt32();
                    ChunkHandler.HandleKVP(_assets, _reader, delegate (Int32 _actionOffset) {
                        this.Events.Add(new LEvent(this, _assets, _reader, this, _eventSubtype));
                    });
                });
            });
        }
    }
}
