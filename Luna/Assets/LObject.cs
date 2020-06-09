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
        public LCode Destroy = null;

        public LObject(Game _assets, BinaryReader _reader) {
            Name = _assets.GetString(_reader.ReadInt32());
            Int32 _spriteIndex = _reader.ReadInt32();
            if (_spriteIndex != -1) {
                Sprite = _assets.SpriteMapping[_spriteIndex];
            } else Sprite = null;
            Visible = _reader.ReadInt32() == 1 ? true : false;
            Solid = _reader.ReadInt32() == 1 ? true : false;
            Depth = _reader.ReadInt32();
            Persistent = _reader.ReadInt32() == 1 ? true : false;
            Int32 _parentIndex = _reader.ReadInt32();
            if (_parentIndex != -100) {
                Parent = _assets.ObjectMapping[_parentIndex];
            } else Parent = null;
            Int32 _maskIndex = _reader.ReadInt32();
            if (_maskIndex != -1) {
                Mask = _assets.SpriteMapping[_maskIndex];
            } else Mask = Sprite;
            PhysObject = _reader.ReadInt32() == 1 ? true : false;
            PhysSensor = _reader.ReadInt32() == 1 ? true : false;
            PhysShape = _reader.ReadInt32();
            PhysDensity = _reader.ReadSingle();
            PhysResitution = _reader.ReadSingle();
            PhysGroup = _reader.ReadInt32();
            PhysLinearDamping = _reader.ReadSingle();
            PhysAngularDamping = _reader.ReadSingle();
            PhysVertCount = _reader.ReadInt32();
            PhysFriction = _reader.ReadSingle();
            PhysAwake = _reader.ReadInt32() == 1 ? true : false;
            PhysKinematic = _reader.ReadInt32() == 1 ? true : false;
            _reader.BaseStream.Seek(sizeof(Single) * PhysVertCount, SeekOrigin.Current);
            ChunkHandler.HandleKVP(_assets, _reader, delegate (Int32 _eventOffset) {
                ChunkHandler.HandleKVP(_assets, _reader, delegate (Int32 _typeOffset) {
                    Int32 _eventSubtype = _reader.ReadInt32();
                    ChunkHandler.HandleKVP(_assets, _reader, delegate (Int32 _actionOffset) {
                        Events.Add(new LEvent(this, _assets, _reader, this, _eventSubtype));
                    });
                });
            });
        }
    }
}
