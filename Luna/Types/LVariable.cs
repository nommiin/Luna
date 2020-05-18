using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace Luna.Types {
    class LVariable {
        public static Int32 Length = 20;
        public string Name;
        public LVariableType Type;
        public Int32 String;
        public Int32 Count;
        public Int32 Offset;
        public long Base;

        public LVariable(Game _game, BinaryReader _reader) {
            this.Name = _game.GetString(_reader.ReadInt32());
            this.Type = (LVariableType)_reader.ReadInt32();
            this.String = _reader.ReadInt32(); // UNKNOWN
            this.Count = _reader.ReadInt32();
            this.Offset = _reader.ReadInt32();
            this.Base = _reader.BaseStream.Position;
#if (DEBUG == true)
            Console.WriteLine("Variable: {0}, Type: {1}, String Index: {2}, Count: {3}, Offset: {4}, Base: {5}", this.Name, this.Type, this.String, this.Count, this.Offset, this.Base);
#endif
        }

        public LVariable(string _name, LVariableType _type) {
            this.Name = _name;
            this.Type = _type;
        }
    }
}
