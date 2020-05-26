﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using OpenTK;
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
        public List<Int32> RoomOrder = new List<int>();
        public float GameSpeed;
        public bool AllowStats;
        public Guid GUID;

        // Assets
        public Dictionary<string, LCode> Code = new Dictionary<string, LCode>();
        public Dictionary<string, LRoom> Rooms = new Dictionary<string, LRoom>();
        public Dictionary<long, LString> Strings = new Dictionary<long, LString>();
        public List<LString> StringMapping = new List<LString>();

        // Code
        public Int32 LocalVariables = 0;
        public Int32 InstanceVariables = 0;
        public Int32 GlobalVariables = 0;
        public List<LVariable> Variables = new List<LVariable>();
        public Dictionary<Int32, Int32> VariableMapping = new Dictionary<Int32, Int32>();
        public List<LFunction> Functions = new List<LFunction>();
        public Dictionary<Int32, Int32> FunctionMapping = new Dictionary<int, int>();

        // Runner
        public bool Headless;
        public GameWindow Window;
        public Interpreter Runner;

        // Special
        public Dictionary<string, Chunk> Chunks;

        public void Initalize(bool _headless) {
            this.Headless = _headless;
            this.Runner = new Interpreter(this);

            // Window
            this.Runner.Timer.Start();
            if (this.Headless == false) {
                this.Window = new GameWindow(this.RoomWidth, this.RoomHeight);
                this.Window.Title = this.DisplayName;
                this.Window.Load += OnLoad;
                this.Window.Closing += OnClose;
                this.Window.UpdateFrame += OnUpdate;
                this.Window.RenderFrame += OnRender;
                this.Window.Run();
            } else {
                OnLoad(null, null);
            }
        }

        private void OnLoad(object sender, EventArgs e) {
            this.Runner.ExecuteScript("gml_GlobalScript_Script2");
        }

        private void OnClose(object sender, System.ComponentModel.CancelEventArgs e) {

        }

        private void OnUpdate(object sender, FrameEventArgs e) {

        }

        private void OnRender(object sender, FrameEventArgs e) {

        }

        public string GetString(Int32 _offset) {
            if (_offset == 0) return "";
            if (this.Strings.ContainsKey(_offset) == true) {
                return this.Strings[_offset].Value;
            }
            throw new Exception(String.Format("Could not find string at {0}", _offset));
        }
    }
}
