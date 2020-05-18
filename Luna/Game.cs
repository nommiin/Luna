using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Luna.Assets;

namespace Luna {
    class Game {
        // GEN8
        public bool DebugEnabled;
        public string Name;
        public string Configuration;
        public Int32 RoomMax;
        public Int32 RoomMaxTile;
        public Int32 ID;
        public string GameName;
        public Version Build;
        public Int32 RoomWidth;
        public Int32 RoomHeight;
        public Options Flags = new Options();
        public class Options {
            public bool Fullscreen;
            public Int32 SyncMode;
            public bool Interpolate;
            public bool Scale;
            public bool ShowCursor;
            public bool Resizable;
            public bool Screenshot;
            public Int32 StudioEdition;
            public bool IsSteam;
            public bool IsPlayer;
            public bool Borderless;
            public bool IsJavaScript;
            public long Targets;
        }
        public Int32 CRC;
        public byte[] MD5;
        public long Timestamp;
        public string DisplayName;
        public long Classifications;
        public Int32 AppID;
        public Int32 DebugPort;
        public Int32 RoomCount;
        public List<Int32> RoomOrder = new List<int>();
        public float GameSpeed;
        public bool AllowStats;
        public Guid GUID;

        // Assets
        public Dictionary<string, LRoom> Rooms = new Dictionary<string, LRoom>();
        public Dictionary<long, LString> Strings = new Dictionary<long, LString>();

        // Code
        public Int32 LocalVariables = 0;
        public Int32 InstanceVariables = 0;
        public Int32 GlobalVariables = 0;

        public List<LVariable> Variables = new List<LVariable>();

        // Special
        public Dictionary<string, Chunk> Chunks;

        public string GetString(Int32 _offset) {
            if (_offset == 0) return "";
            if (this.Strings.ContainsKey(_offset) == true) {
                return this.Strings[_offset].Value;
            } else {
                Console.WriteLine("[WARNING] Could not find string at {0}, returning \"???\" instead.", _offset);
                return "???";
            }
            
        }
    }

    class LString {
        public string Value;
        public Int32 Offset;
        public long Base;

        public LString(BinaryReader _reader) {
            this.Offset = _reader.ReadInt32();
            this.Base = _reader.BaseStream.Position;
            _reader.BaseStream.Seek(this.Offset, SeekOrigin.Begin);
            this.Value = ASCIIEncoding.ASCII.GetString(_reader.ReadBytes(_reader.ReadInt32()));
            _reader.BaseStream.Seek(this.Base, SeekOrigin.Begin);
            this.Offset += 4;
            Console.WriteLine("String: {0}, Offset: {1}", this.Value, this.Offset);
        }

        public LString(string _value) {
            this.Value = _value;
            this.Offset = -1;
            this.Base = -1;
        }

        public override string ToString() {
            return this.Value;
        }
    }

    class LBoolean {
        public bool Value;

        public LBoolean(Int32 _val) {
            this.Value = (_val >= 0.5);
        }

        public static implicit operator bool(LBoolean b) => b.Value;
        public static explicit operator LBoolean(bool b) => new LBoolean(1);
    }

    class LVariable {
        public string Name;
        public Int32 Type;
        public Int32 Magic;
        public Int32 Count;
        public Int32 Offset;
        public long Base;

        public LVariable(Game _game, BinaryReader _reader, BinaryWriter _writer) {
            this.Name = _game.GetString(_reader.ReadInt32());
            this.Type = _reader.ReadInt32();
            this.Magic = _reader.ReadInt32(); // UNKNOWN
            this.Count = _reader.ReadInt32();
            this.Offset = _reader.ReadInt32();
            this.Base = _reader.BaseStream.Position;
            Console.WriteLine("Variable: {0}, Type: {1}, Magic: {2}, Count: {3}, Base: {4}", this.Name, this.Type, this.Magic, this.Count, this.Base);
        }

        public LVariable(string _name, Int32 _type) {
            this.Name = _name;
            this.Type = _type;
        }
    }
}
