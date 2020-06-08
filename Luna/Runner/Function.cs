using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using Luna.Types;
using OpenTK;
using OpenTK.Input;
using OpenTK.Graphics.OpenGL;

namespace Luna.Runner {
    [AttributeUsage(AttributeTargets.Method)]
    public class FunctionDefinition : Attribute {
        public string Name;

        public FunctionDefinition(string _name) {
            this.Name = _name;
        }

        public static void Initalize() {
            MethodInfo[] _functionGet = typeof(Function).GetMethods(BindingFlags.Public | BindingFlags.Static);
            foreach(MethodInfo _functionHandler in _functionGet) {
                Function.Mapping.Add(_functionHandler.GetCustomAttribute<FunctionDefinition>().Name,
                    (Function.Handler)Delegate.CreateDelegate(typeof(Function.Handler), _functionHandler)
                );
            }
        }
    }

    static class Function {
        public delegate LValue Handler(Game _assets, Domain _environment, LValue[] _arguments, Int32 _count, Stack<LValue> _stack);
        public static Dictionary<string, Handler> Mapping = new Dictionary<string, Handler>();

        [FunctionDefinition("show_debug_message")]
        public static LValue show_debug_message(Game _assets, Domain _environment, LValue[] _arguments, Int32 _count, Stack<LValue> _stack) {
            Console.WriteLine(_arguments[0].Value);
            return new LValue(LType.Number, (double)0);
        }

        [FunctionDefinition("room_goto")]
        public static LValue room_goto(Game _assets, Domain _environment, LValue[] _arguments, Int32 _count, Stack<LValue> _stack) {
            return new LValue(LType.Number, (double)0);
        }

        [FunctionDefinition("event_inherited")]
        public static LValue event_inherited(Game _assets, Domain _environment, LValue[] _arguments, Int32 _count, Stack<LValue> _stack) {
            //
            return new LValue(LType.Number, (double)0);
        }

        #region Instances
        [FunctionDefinition("instance_create_depth")]
        public static LValue instance_create_depth(Game _assets, Domain _environment, LValue[] _arguments, Int32 _count, Stack<LValue> _stack) {
            LInstance _instCreate = new LInstance(_assets.InstanceList, _assets.ObjectMapping[(int)(double)_arguments[2].Value], (double)_arguments[0].Value, (double)_arguments[1].Value);
            if (_instCreate.PreCreate != null) _instCreate.Environment.ExecuteCode(_assets, _instCreate.PreCreate);
            if (_instCreate.Create != null) _instCreate.Environment.ExecuteCode(_assets, _instCreate.Create);
            return new LValue(LType.Number, _instCreate.ID);
        }

        [FunctionDefinition("instance_destroy")]
        public static LValue instance_destroy(Game _assets, Domain _environment, LValue[] _arguments, Int32 _count, Stack<LValue> _stack) {
            _assets.InstanceList.Remove(_environment.Instance);
            return new LValue(LType.Number, (double)0);
        }
        #endregion

        #region Input
        [FunctionDefinition("keyboard_check")]
        public static LValue keyboard_check(Game _assets, Domain _environment, LValue[] _arguments, Int32 _count, Stack<LValue> _stack) {
            return new LValue(LType.Number, (double)(Input.KeyCheck(_arguments[0]) == true ? 1 : 0));
        }
        #endregion

        #region Rendering
        [FunctionDefinition("draw_circle")]
        public static LValue draw_circle(Game _assets, Domain _environment, LValue[] _arguments, Int32 _count, Stack<LValue> _stack) {
            GL.Begin(PrimitiveType.TriangleFan);

            double _x = ((double)_arguments[0].Value / (_assets.RoomWidth / 2));
            double _y = -((double)_arguments[1].Value / (_assets.RoomHeight / 2));
            double _r = (double)_arguments[2].Value / 360;// (double)_arguments[2].Value;

            GL.Vertex2(_x, _y);
            for(int i = 0; i < 360; i++) {
                GL.Vertex2(_x + (Math.Sin(i) * _r), _y + (Math.Cos(i) * _r));
            }

            /*
            GL.Vertex2(_arguments[0], _arguments[1]);
            for(int i = 0; i <= 360; i += 360 / 16) {
                GL.Vertex2((double)_arguments[0].Value + Math.Cos(i) * (double)_arguments[2].Value, (double)_arguments[1].Value + Math.Sin(i) * (double)_arguments[2].Value);
            }
            */


            GL.End();
            return new LValue(LType.Number, (double)0);
        }
        #endregion

        #region Math
        [FunctionDefinition("max")]
        public static LValue max(Game _assets, Domain _environment, LValue[] _arguments, Int32 _count, Stack<LValue> _stack) {
            double[] _val = new double[_count];
            for (int i = 0; i < _count; i++) _val[i] = (double)_arguments[i];
            return new LValue(LType.Number, _val.Max());
            /*double[] _val = new double[_count];
            for (int i = 0; i < _count; i++) _val[i] = _stack.Pop();
            _stack.Push(new LValue(LType.Number, _val.Max()));*/
        }

        [FunctionDefinition("min")]
        public static LValue min(Game _assets, Domain _environment, LValue[] _arguments, Int32 _count, Stack<LValue> _stack) {
            double[] _val = new double[_count];
            for (int i = 0; i < _count; i++) _val[i] = (double)_arguments[i];
            return new LValue(LType.Number, _val.Min());
        }

        [FunctionDefinition("lengthdir_x")]
        public static LValue lengthdir_x(Game _assets, Domain _environment, LValue[] _arguments, Int32 _count, Stack<LValue> _stack) {
            double _len = (double)_arguments[0].Value;
            double _dir = (double)_arguments[1].Value;

            double _Px = (_len * Math.Cos(_dir * Math.PI / 180));
            double _o81 = Math.Round(_Px);
            double _F6 = _Px - _o81;
            if (Math.Abs(_F6) < 0.0001) {
                return new LValue(LType.Number, _o81);
            } else {
                return new LValue(LType.Number, _Px);
            }
            
        }

        [FunctionDefinition("lengthdir_y")]
        public static LValue lengthdir_y(Game _assets, Domain _environment, LValue[] _arguments, Int32 _count, Stack<LValue> _stack) {
            double _len = (double)_arguments[0].Value;
            double _dir = (double)_arguments[1].Value;

            double _Px = (_len * Math.Sin(_dir * Math.PI / 180));
            double _o81 = Math.Round(_Px);
            double _F6 = _Px - _o81;
            if (Math.Abs(_F6) < 0.0001) {
                return new LValue(LType.Number, _o81);
            } else {
                return new LValue(LType.Number, _Px);
            }
        }

        [FunctionDefinition("irandom")]
        public static LValue irandom(Game _assets, Domain _environment, LValue[] _arguments, Int32 _count, Stack<LValue> _stack) {
            return new LValue(LType.Number, (double)_assets.RandomGen.Next(0, (int)(double)_arguments[0].Value));
        }
        #endregion
        }
}
