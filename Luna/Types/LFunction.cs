using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace Luna.Types {
    class LFunction {
        public string Name;
        public Int32 Count;
        public Int32 Offset;
        public long Base;

        public LFunction(Game _game, BinaryReader _reader) {
            Name = _game.GetString(_reader.ReadInt32());
            Count = _reader.ReadInt32();
            Offset = _reader.ReadInt32();
            Base = _reader.BaseStream.Position;
#if (DEBUG == true)
            //Console.WriteLine(this);
#endif
        }

        public override string ToString() {
            return $"Function: {Name}, Uses: {Count}, Offset: {Offset}";
        }
    }
}
