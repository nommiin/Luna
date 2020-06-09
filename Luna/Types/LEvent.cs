using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Luna.Assets;

namespace Luna.Types {
    public enum ActionType {
        Normal,
        Begin,
        End,
        Else,
        Exit,
        Repeat,
        Variable,
        Code
    }

    public enum ActionExecution {
        None,
        Function,
        Code
    }

    class LEvent {
        public LObject Owner;
        public Int32 Subtype;
        public Int32 LibraryID;
        public Int32 ID;
        public ActionType Type;
        public bool UseRelative;
        public bool IsQuestion;
        public bool ApplyTo;
        public ActionExecution Execution;
        public string Name;
        public LCode Code;
        public Int32 ArgumentCount;
        public Int32 Caller;
        public bool IsRelative;
        public bool IsNot;

        public LEvent(LObject _object, Game _assets, BinaryReader _reader, LObject _owner, Int32 _type) {
            Owner = _owner;
            Subtype = _type;
            LibraryID = _reader.ReadInt32();
            ID = _reader.ReadInt32();
            Type = (ActionType)_reader.ReadInt32();
            UseRelative = _reader.ReadInt32() == 1 ? true : false;
            IsQuestion = _reader.ReadInt32() == 1 ? true : false;
            ApplyTo = _reader.ReadInt32() == 1 ? true : false;
            Execution = (ActionExecution)_reader.ReadInt32();
            Name = _assets.GetString(_reader.ReadInt32());
            Code = _assets.CodeMapping[_reader.ReadInt32()];
            ArgumentCount = _reader.ReadInt32();
            Caller = _reader.ReadInt32();
            IsRelative = _reader.ReadInt32() == 1 ? true : false;
            IsNot = _reader.ReadInt32() == 1 ? true : false;
            //todo: use enums for setting event code objects
            switch (Code.Name.Replace("gml_Object_" + _object.Name + "_", "")) {
                case "PreCreate_0": _object.PreCreate = Code; break;
                case "Create_0": _object.Create = Code; break;
                case "Step_0": _object.Step = Code; break;
                case "Draw_0": _object.Draw = Code; break;
                case "Destroy_0": _object.Destroy = Code; break;
                //case "Other_(game end id goes here)": _object.Other.GameEnd = Code; break;
            }
            //this.
        }
    }

    enum Events
    {
        Create,
        Destroy,
        Alarm,
        Step,
        Collision,
        Keyboard,
        Mouse,
        Other,
        Draw,
        KeyPress,
        KeyRelease,
        Trigger,//does this exist in 2.3? triggers were removed 
        CleanUp,
        PreCreate
    }
}
