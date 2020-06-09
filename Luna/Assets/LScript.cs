using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Luna.Assets;
using System.IO;

namespace Luna.Assets {
    class LScript {
        public string Name;
        public long Index;

        public LScript(Game _game, BinaryReader _reader) {
            Name = _game.GetString(_reader.ReadInt32());
            Index = _reader.ReadInt32();
        }
    }
}
