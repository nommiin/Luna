using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using Luna.Types;
using Luna.Assets;
using Luna.Runner;

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
        public List<Int32> RoomIndices = new List<int>();
        public List<LRoom> RoomOrder = new List<LRoom>();
        public float GameSpeed;
        public bool AllowStats;
        public Guid GUID;

        // Assets
        public Dictionary<long, LString> Strings = new Dictionary<long, LString>();
        public Dictionary<string, LCode> Code = new Dictionary<string, LCode>();
        public Dictionary<string, LScript> Scripts = new Dictionary<string, LScript>();
        public Dictionary<string, LSprite> Sprites = new Dictionary<string, LSprite>();
        public Dictionary<string, LObject> Objects = new Dictionary<string, LObject>();
        public Dictionary<string, LRoom> Rooms = new Dictionary<string, LRoom>();

        public List<LString> StringMapping = new List<LString>();
        public List<LCode> CodeMapping = new List<LCode>();
        public List<LScript> ScriptMapping = new List<LScript>();
        public List<LSprite> SpriteMapping = new List<LSprite>();
        public List<LObject> ObjectMapping = new List<LObject>();
        public List<LRoom> RoomMapping = new List<LRoom>();

        // Code
        public Int32 LocalVariables = 0;
        public Int32 InstanceVariables = 0;
        public Int32 GlobalVariables = 0;
        public List<LCode> GlobalScripts = new List<LCode>();
        public List<LVariable> Variables = new List<LVariable>();
        public List<LFunction> Functions = new List<LFunction>();
        public Dictionary<Int32, Int32> FunctionMapping = new Dictionary<int, int>();
        public Dictionary<Int32, Int32> VariableMapping = new Dictionary<Int32, Int32>();

        // Runner
        public GameWindow Window;
        public Interpreter Runner;
        public Dictionary<double, LInstance> Instances = new Dictionary<double, LInstance>();
        public List<LInstance> InstanceList = new List<LInstance>();
        public LInstance GlobalScope;
        public LInstance StaticScope;
        public Random RandomGen;

        // Built-in Variables
        public LRoom CurrentRoom;
        
        // Rendering
        public int CirclePrecision;

        // Special
        public Dictionary<string, Chunk> Chunks;
        public List<System.Threading.Thread> Threads = new List<System.Threading.Thread>();

        public void Initalize(bool _headless) {
            // Game
            this.Runner = new Interpreter(this);
            this.Instances.Add((double)LVariableScope.Global, new LInstance((double)LVariableScope.Global));
            this.GlobalScope = this.Instances[(Int32)LVariableScope.Global];
            this.Instances.Add((double)LVariableScope.Static, new LInstance((double)LVariableScope.Static));
            this.StaticScope = this.Instances[(Int32)LVariableScope.Static];
            this.RandomGen = new Random();

            // Window
            if (_headless == false) {
                this.Window = new GameWindow(this.RoomWidth, this.RoomHeight);
                this.Window.Title = this.DisplayName;
                this.Window.Load += OnLoad;
                this.Window.Closing += OnClose;
                this.Window.UpdateFrame += OnUpdate;
                this.Window.RenderFrame += OnRender;
                Input.Initalize(this);
                this.Window.Run();
            } else {
                OnLoad(null, null);
            }
        }

        public void LoadRoom(LRoom _room) {
            this.CurrentRoom = _room;
            for(int i = 0; i < _room.Instances.Count; i++) {
                LRoomInstance _instGet = _room.Instances[i];
                LInstance _instCreate = new LInstance(this.InstanceList, _instGet.Index, _instGet.X, _instGet.Y);
                _instCreate.RoomPreCreate = _instGet.PreCreate;
                _instCreate.RoomCreate = _instGet.CreationCode;
                _instCreate.Variables["image_xscale"] = new LValue(LType.Number, (double)_instGet.ScaleX);
                _instCreate.Variables["image_yscale"] = new LValue(LType.Number, (double)_instGet.ScaleY);
                _instCreate.Variables["image_speed"] = new LValue(LType.Number, (double)_instGet.ImageSpeed);
                _instCreate.Variables["image_index"] = new LValue(LType.Number, (double)_instGet.ImageIndex);
                _instCreate.Variables["image_blend"] = new LValue(LType.Number, (double)_instGet.ImageBlend);
                _instCreate.Variables["image_angle"] = new LValue(LType.Number, (double)_instGet.Rotation);
                this.Instances.Add((double)_instCreate.ID, _instCreate);
                if (_instCreate.PreCreate != null) _instCreate.Environment.ExecuteCode(this, _instCreate.PreCreate);
                if (_instCreate.Create != null) _instCreate.Environment.ExecuteCode(this, _instCreate.Create);
                if (_instCreate.RoomPreCreate != null) _instCreate.Environment.ExecuteCode(this, _instCreate.RoomPreCreate);
                if (_instCreate.RoomCreate != null) _instCreate.Environment.ExecuteCode(this, _instCreate.RoomCreate);
            }
            if (_room.CreationCode != null) {
                Domain _roomEnvironment = new Domain(new LInstance(-100));
                _roomEnvironment.ExecuteCode(this, _room.CreationCode);
            }
        }

        public string GetString(Int32 _offset) {
            if (_offset == 0) return "";
            if (this.Strings.ContainsKey(_offset) == true) {
                return this.Strings[_offset].Value;
            }
            throw new Exception(String.Format("Could not find string at {0}", _offset));
        }

        private void OnLoad(object sender, EventArgs e) {
            for(int i = 0; i < this.GlobalScripts.Count; i++) {
                this.GlobalScope.Environment.ExecuteCode(this, this.GlobalScripts[i]);
            }
            LoadRoom(this.RoomOrder[0]);
        }

        private void OnClose(object sender, System.ComponentModel.CancelEventArgs e) {

        }

        private void OnUpdate(object sender, FrameEventArgs e) {
            Input.OnKeyUpdate();
            for (int i = 0; i < this.InstanceList.Count; i++) {
                LInstance _instGet = this.InstanceList[i];
                if (_instGet.Step != null) {
                    _instGet.Environment.ExecuteCode(this, _instGet.Step);
                }
                _instGet.Variables["xprevious"] = _instGet.Variables["x"];
                _instGet.Variables["yprevious"] = _instGet.Variables["y"];
            }
        }

        private void OnRender(object sender, FrameEventArgs e) {
            GL.Clear(ClearBufferMask.ColorBufferBit);
            GL.MatrixMode(MatrixMode.Projection);
            GL.LoadIdentity();
            GL.Ortho(0, 0, this.RoomWidth, this.RoomHeight, 0f, 1.0f);

            for (int i = 0; i < this.InstanceList.Count; i++) {
                LInstance _instGet = this.InstanceList[i];
                if (_instGet.Draw != null) {
                    _instGet.Environment.ExecuteCode(this, _instGet.Draw);
                }
            }

            GL.Flush();
            Window.SwapBuffers();
        }
    }
}
