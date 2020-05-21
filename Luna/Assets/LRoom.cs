using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace Luna.Assets {
    class LRoom {
        public string Name;
        public string Caption;
        public Int32 Width;
        public Int32 Height;
        public Int32 Speed;
        public bool Persistent;
        public Int32 Colour;
        public bool ShowColour;
        public Int32 CreationCode;

        public LRoom(Game _game, BinaryReader _reader) {
            this.Name = _game.GetString(_reader.ReadInt32());
            this.Caption = _game.GetString(_reader.ReadInt32());
            this.Width = _reader.ReadInt32();
            this.Height = _reader.ReadInt32();
            this.Speed = _reader.ReadInt32();
            this.Persistent = _reader.ReadBoolean();
            this.Colour = _reader.ReadInt32();
            this.ShowColour = _reader.ReadBoolean();
            this.CreationCode = _reader.ReadInt32();
            // TODO: Parse remaining data, such as phyiscs and layer data.
#if (DEBUG == true)
            Console.WriteLine(this);
#endif
        }

        public override string ToString() {
            return $"Room: {this.Name}, Size: {this.Width}x{this.Height}, Creation Code: {this.CreationCode}";
        }
    }
}
