using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

        public static implicit operator double(LColour _val) => ((_val.Alpha << 24) & (_val.Blue << 16) & (_val.Green << 8) & _val.Red);

        public override string ToString() {
            return $"rgba({this.Red}, {this.Green}, {this.Blue}, {this.Alpha})";
        }
    }
}
