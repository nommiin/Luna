using System;
using System.IO;

namespace Luna.Types {
    class LFunction {
        public string Name;
        public Int32 Count;
        public Int32 Offset;
        public long Base;

        public LFunction(Game _game, BinaryReader _reader) {
            this.Name = _game.GetString(_reader.ReadInt32());
            this.Count = _reader.ReadInt32();
            this.Offset = _reader.ReadInt32();
            this.Base = _reader.BaseStream.Position;
        }

        public override string ToString() {
            return $"Function: {this.Name}, Uses: {this.Count}, Offset: {this.Offset}";
        }
    }
}
