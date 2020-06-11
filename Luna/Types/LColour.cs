using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK.Graphics;

namespace Luna.Types {
    class LColour {
        public byte Red;
        public byte Green;
        public byte Blue;
        public byte Alpha;

        public LColour(Int32 _colour) {
            this.Red = (byte)(_colour & 0xFF);
            this.Green = (byte)((_colour >> 8) & 0xFF);
            this.Blue = (byte)((_colour >> 16) & 0xFF);
            this.Alpha = (byte)((_colour >> 24) & 0xFF);
        }

        public LColour(byte _r, byte _g, byte _b, byte _a)
        {
            //is this ever gonna get used?
            //stubbing, implement when needed
            throw new NotImplementedException("LColour's RGBA constructor is not implemented.");
        }

        public static implicit operator double(LColour _val) => (_val.Alpha << 24) & (_val.Blue << 16) & (_val.Green << 8) & _val.Red;
        public static implicit operator Color4(LColour _val) => new Color4(_val.Red, _val.Green, _val.Blue, _val.Alpha);

        public override string ToString() {
            return $"rgba({this.Red}, {this.Green}, {this.Blue}, {this.Alpha})";
        }

        public static LColour FromColor4(Color4 _color4) {
            return new LColour(_color4.ToArgb());
        }
    }
}
