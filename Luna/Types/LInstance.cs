using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Luna.Assets;
using Luna.Runner;

namespace Luna.Types {
    class LInstance {
        public static double Counter = 100000;
        public double ID;

        public LObject Object;
        public Domain Environment;
        public Dictionary<string, LValue> Variables;

        public LCode RoomPreCreate = null;
        public LCode RoomCreate = null;
        public LCode PreCreate = null;
        public LCode Create = null;
        public LCode Step = null;
        public LCode Draw = null;

        public LInstance(List<LInstance> _instances, LObject _object, double _x=0, double _y=0) {
            _instances.Add(this);
            Console.WriteLine("{0} created at {1}, {2}", _object.Name, _x, _y);
            Object = _object;
            ID = Counter++;
            Environment = new Domain(this);
            Variables = new Dictionary<string, LValue>() {
                ["x"] = new LValue(LType.Number, _x),
                ["y"] = new LValue(LType.Number, _y),
                ["xprevious"] = new LValue(LType.Number, _x),
                ["yprevious"] = new LValue(LType.Number, _y),

                ["id"] = new LValue(LType.Number, ID),
                ["solid"] = new LValue(LType.Number, (double)(_object.Solid ? 1 : 0)),
                ["visible"] = new LValue(LType.Number, (double)(_object.Visible ? 1 : 0)),
                ["persistent"] = new LValue(LType.Number, (double)(_object.Persistent ? 1 : 0)),
                ["depth"] = new LValue(LType.Number, (double)_object.Depth),
                // ["alarm"] = new LValue(LType.Array, ???),
                ["object_index"] = new LValue(LType.Number, (double)_object.Index),

                ["sprite_index"] = new LValue(LType.Number, (double)(_object.Sprite != null ? _object.Sprite.Index : -1)),
                ["sprite_width"] = new LValue(LType.Number, (double)(_object.Sprite != null ? _object.Sprite.Width : 0)),
                ["sprite_height"] = new LValue(LType.Number, (double)(_object.Sprite != null ? _object.Sprite.Height : 0)),
                ["sprite_xoffset"] = new LValue(LType.Number, (double)(_object.Sprite != null ? _object.Sprite.OriginX : 0)),
                ["sprite_yoffset"] = new LValue(LType.Number, (double)(_object.Sprite != null ? _object.Sprite.OriginY : 0)),
                ["image_alpha"] = new LValue(LType.Number, (double)1),
                ["image_angle"] = new LValue(LType.Number, (double)0),
                ["image_blend"] = new LValue(LType.Number, (double)16777215),
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

            PreCreate = _object.PreCreate;
            Create = _object.Create;
            Step = _object.Step;
            Draw = _object.Draw;
        }

        public LInstance(double _id) {
            ID = _id;
            Environment = new Domain(this);
            Variables = new Dictionary<string, LValue>();
        }
    }
}
