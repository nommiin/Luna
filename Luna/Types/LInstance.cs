using System.Collections.Generic;
using System.Linq;
using Luna.Assets;
using Luna.Runner;

namespace Luna.Types {
    class LInstance {
        public double ID;
        public static double IDStart = 100000;
        public static double IDCount = LInstance.IDStart;

        public LObject Object;
        public Domain Environment;
        public Dictionary<string, LValue> Variables;

        public LCode RoomPreCreate = null;
        public LCode RoomCreate = null;
        public LCode PreCreate = null;
        public LCode Create = null;
        public LCode BeginStep = null;
        public LCode Step = null;
        public LCode EndStep = null;
        public LCode Draw = null;
        public LCode Destroy = null;
        public LCode CleanUp = null;

        public LInstance(Game _assets, LObject _object, bool _include=false, double _x=0, double _y=0) {
            this.Object = _object;
            this.ID = LInstance.IDCount++;
            this.Environment = new Domain(this);
            this.Variables = new Dictionary<string, LValue>() {
                ["x"] = new LValue(LType.Number, _x),
                ["y"] = new LValue(LType.Number, _y),
                ["xprevious"] = new LValue(LType.Number, _x),
                ["yprevious"] = new LValue(LType.Number, _y),

                ["id"] = new LValue(LType.Number, ID),
                ["solid"] = new LValue(LType.Number, (double)(_object.Solid ? 1 : 0)),
                ["visible"] = new LValue(LType.Number, (double)(_object.Visible ? 1 : 0)),
                ["persistent"] = new LValue(LType.Number, (double)(_object.Persistent ? 1 : 0)),
                ["depth"] = new LValue(LType.Number, (double)_object.Depth),
                ["alarm"] = LValue.Values(Enumerable.Repeat(LValue.Real(0),8).ToArray()),//better than writing 8 LValue.Real(0)
                ["object_index"] = new LValue(LType.Number, (double)_object.Index),

                ["sprite_index"] = new LValue(LType.Number, (double)(_object.Sprite != null ? _object.Sprite.Index : -1)),
                ["sprite_width"] = new LValue(LType.Number, (double)(_object.Sprite != null ? _object.Sprite.Width : 0)),
                ["sprite_height"] = new LValue(LType.Number, (double)(_object.Sprite != null ? _object.Sprite.Height : 0)),
                ["sprite_xoffset"] = new LValue(LType.Number, (double)(_object.Sprite != null ? _object.Sprite.OriginX : 0)),
                ["sprite_yoffset"] = new LValue(LType.Number, (double)(_object.Sprite != null ? _object.Sprite.OriginY : 0)),
                ["image_alpha"] = new LValue(LType.Number, (double)1),
                ["image_angle"] = new LValue(LType.Number, (double)0),
                ["image_blend"] = new LValue(LType.Number, (double)0xFFFFFF),
                ["image_index"] = new LValue(LType.Number, (double)0),
                ["image_number"] = new LValue(LType.Number, (double)(_object.Sprite != null ? _object.Sprite.FrameCount : 0)),
                ["image_speed"] = new LValue(LType.Number, (double)(_object.Sprite != null ? _object.Sprite.PlaybackSpeed : 1)),
                ["image_xscale"] = new LValue(LType.Number, (double)1),
                ["image_yscale"] = new LValue(LType.Number, (double)1),

                ["mask_index"] = new LValue(LType.Number, (double)(_object.Mask != null ? _object.Mask.Index : -1)),
                ["bbox_left"] = new LValue(LType.Number, _object.Mask != null ? _x + _object.Mask.BoundsLeft : _x),
                ["bbox_right"] = new LValue(LType.Number, _object.Mask != null ? _x + _object.Mask.BoundsRight : _x),
                ["bbox_top"] = new LValue(LType.Number, _object.Mask != null ? _y + _object.Mask.BoundsTop : _y),
                ["bbox_bottom"] = new LValue(LType.Number, _object.Mask != null ? _y + _object.Mask.BoundsBottom : _y),
            };

            if (_include == true) {
                _assets.InstanceList.Add(this);
                _assets.InstanceMapping[this.ID] = this;
            }

            this.PreCreate = _object.PreCreate;
            this.Create = _object.Create;
            this.BeginStep = _object.BeginStep;
            this.Step = _object.Step;
            this.EndStep = _object.EndStep;
            this.Draw = _object.Draw;
            this.Destroy = _object.Destroy;
            this.CleanUp = _object.CleanUp;
        }

        public LInstance(Game _assets, double _id, bool _include=true) {
            this.ID = _id;
            this.Environment = new Domain(this);
            this.Variables = new Dictionary<string, LValue>();
            if (_include == true) {
                _assets.InstanceMapping[_id] = this;
            }
        }

        public void Remove(Game _assets, bool _destroy) {
            if (_destroy == true) {
                if (this.Destroy != null) this.Environment.ExecuteCode(_assets, this.Destroy);
            }
            if (this.CleanUp != null) this.Environment.ExecuteCode(_assets, this.CleanUp);
            _assets.InstanceList.Remove(this);
            _assets.InstanceMapping.Remove(this.ID);
        }

        public static LInstance Find(Game _assets, double _id, bool _internal=true) {
            if (_id >= 0 && _id < LInstance.IDStart) {
                for(int i = 0; i < _assets.InstanceList.Count; i++) {
                    LInstance _instGet = _assets.InstanceList[i];
                    if (_instGet.Object.Index == _id) return _instGet;
                }
            } else if (_internal == true || _id >= 0) {
                if (_assets.InstanceMapping.ContainsKey(_id) == true) {
                    return _assets.InstanceMapping[_id];
                }
            }
            return null;
        }
    }
}
