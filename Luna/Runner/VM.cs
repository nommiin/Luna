using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Luna.Runner;
using Luna.Types;
using Luna.Assets;

namespace Luna {
    static class VM {
        public static void LoadRoom(Game _assets, LRoom _room) {
            _assets.CurrentRoom = _room;
            for (int i = 0; i < _room.Instances.Count; i++) {
                LRoomInstance _instGet = _room.Instances[i];
                LInstance _instCreate = new LInstance(_assets, _instGet.Index, _instGet.X, _instGet.Y);
                _instCreate.RoomPreCreate = _instGet.PreCreate;
                _instCreate.RoomCreate = _instGet.CreationCode;
                _instCreate.Variables["image_xscale"] = new LValue(LType.Number, (double)_instGet.ScaleX);
                _instCreate.Variables["image_yscale"] = new LValue(LType.Number, (double)_instGet.ScaleY);
                _instCreate.Variables["image_speed"] = new LValue(LType.Number, (double)_instGet.ImageSpeed);
                _instCreate.Variables["image_index"] = new LValue(LType.Number, (double)_instGet.ImageIndex);
                _instCreate.Variables["image_blend"] = new LValue(LType.Number, (double)_instGet.ImageBlend);
                _instCreate.Variables["image_angle"] = new LValue(LType.Number, (double)_instGet.Rotation);
                _assets.Instances.Add((double)_instCreate.ID, _instCreate); // TODO: this is probably unneeded
                if (_instCreate.PreCreate != null) _instCreate.Environment.ExecuteCode(_assets, _instCreate.PreCreate);
                if (_instCreate.Create != null) _instCreate.Environment.ExecuteCode(_assets, _instCreate.Create);
                if (_instCreate.RoomPreCreate != null) _instCreate.Environment.ExecuteCode(_assets, _instCreate.RoomPreCreate);
                if (_instCreate.RoomCreate != null) _instCreate.Environment.ExecuteCode(_assets, _instCreate.RoomCreate);
            }

            if (_room.CreationCode != null) {
                Domain _roomEnvironment = new Domain(new LInstance(-100));
                _roomEnvironment.ExecuteCode(_assets, _room.CreationCode);
            }
        }
    }

    /// <summary>
    /// Implementation/transplant of the Well512a RNG algorithm in C#
    /// </summary>
    /// <remarks>
    /// Original C code at http://www.iro.umontreal.ca/~panneton/well/WELL512a.c
    /// </remarks>
    static class WellGenerator
    {
        private static readonly int W = 32;
        private static readonly int R = 16;
        private static readonly int P = 0;
        private static readonly int M1 = 13;
        private static readonly int M2 = 9;
        private static readonly int M3 = 5;
        private static readonly double FACT = 2.32830643653869628906e-10;
        
        private static int state_i;
        private static List<uint> State = Enumerable.Repeat(0u,R).ToList();
        private static uint seed;//should never be 0 ever, that's why i set it to a funny number

        public static uint Seed
        {
            get => seed;
            set
            {
                seed = value;
                State = Enumerable.Repeat(value,R).ToList();
            }
        }

        static WellGenerator()
        {
            Seed = 8008132;//i know, total comedian aren't i
        }

        private static uint z0, z1, z2;

        private static uint Mat0Pos(int t, uint v) => v ^ v >> t;
        private static uint Mat0Neg(int t, uint v) => v ^ v << -t;
        private static uint Mat3Neg(int t, uint v) => v << -t;
        private static uint Mat4Neg(int t, uint b, uint v) => v ^ v << -t & b;
        public static double Next()
        {
            z0 = State[(state_i + 15) & 15];
            z1 = Mat0Neg(-16,State[state_i]) ^ Mat0Neg(-15, State[(state_i + M1) & 15]);
            z2 = Mat0Pos(11, State[(state_i + M2) & 15]);
            uint newV1 = State[state_i] = z1 ^ z2;
            State[(state_i + 15) & 15] = Mat0Neg(-2,z0) ^ Mat0Neg(-18,z1) ^ Mat3Neg(-28,z2) ^ Mat4Neg(-5,0xda442d24U, newV1) ; 
            state_i = (state_i + 15) & 15;
            return State[state_i] * FACT;
        }

        public static int ENext(int min, int max)
        {
            min = (int) Math.Ceiling((double) min);
            max = (int) Math.Floor((double) max);
            return (int) (Math.Floor(Next()*(max - min))+min);
        }

        public static int ENext(int max)
        {
            return ENext(0,max);
        }

        public static int INext(int min, int max)
        {
            min = (int) Math.Ceiling((double) min);
            max = (int) Math.Floor((double) max);
            return (int) (Math.Floor(Next()*(max - min+1))+min);
        }

        public static int INext(int max)
        {
            return INext(0, max);
        }
    }
}
