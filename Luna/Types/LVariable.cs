using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace Luna.Types {
    public enum LVariableScope {
        Global = -5,
        Instance = -1,
        Local = -7,
        Static = -16,
        Unknown = -6
    }

    class LVariable {
        public static Int32 Length = 20;
        public string Name;
        public LVariableScope Scope;
        public Int32 String;
        public Int32 Count;
        public Int32 Offset;
        public long Base;

        public LVariable(Game _game, BinaryReader _reader) {
            Name = _game.GetString(_reader.ReadInt32());
            Scope = (LVariableScope)_reader.ReadInt32();
            String = _reader.ReadInt32();
            Count = _reader.ReadInt32();
            Offset = _reader.ReadInt32();
            Base = _reader.BaseStream.Position;
        }

        public LVariable(string _name, LVariableScope _type) {
            Name = _name;
            Scope = _type;
        }

        public override string ToString() {
            return $"Variable: {Name}, Type: {Scope}, String: {String}, Count: {Count}, Offset: {Offset}";
        }
    }
}
