using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Luna.Runner;

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
            this.Name = _game.GetString(_reader.ReadInt32());
            this.Scope = (LVariableScope)_reader.ReadInt32();
            this.String = _reader.ReadInt32();
            this.Count = _reader.ReadInt32();
            this.Offset = _reader.ReadInt32();
            this.Base = _reader.BaseStream.Position;
#if (DEBUG == true)
            Console.WriteLine(this);
#endif
        }

        public LVariable(string _name, LVariableScope _type) {
            this.Name = _name;
            this.Scope = _type;
        }

        public override string ToString() {
            return $"Variable: {this.Name}, Type: {this.Scope}, String: {this.String}, Count: {this.Count}, Offset: {this.Offset}";
        }
    }
}
