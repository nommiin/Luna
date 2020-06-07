using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
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
        public delegate void Handler(Interpreter _vm, Domain _environment, LValue[] _arguments, Int32 _count, Stack<LValue> _stack);
        public static Dictionary<string, Handler> Mapping = new Dictionary<string, Handler>();

        [FunctionDefinition("show_debug_message")]
        public static void show_debug_message(Interpreter _vm, Domain _environment, LValue[] _arguments, Int32 _count, Stack<LValue> _stack) {
            Console.WriteLine((string)_arguments[0]);
            _stack.Push(new LValue(LType.Number, (double)0));
        }

        [FunctionDefinition("room_goto")]
        public static void room_goto(Interpreter _vm, Domain _environment, LValue[] _arguments, Int32 _count, Stack<LValue> _stack) {

        }

        [FunctionDefinition("event_inherited")]
        public static void event_inherited(Interpreter _vm, Domain _environment, LValue[] _arguments, Int32 _count, Stack<LValue> _stack) {
            //
            _stack.Push(new LValue(LType.Number, (double)0));
        }

        #region Input
        [FunctionDefinition("keyboard_check")]
        public static void keyboard_check(Interpreter _vm, Domain _environment, LValue[] _arguments, Int32 _count, Stack<LValue> _stack) {
            _stack.Push(new LValue(LType.Number, (double)(Input.KeyCheck(_arguments[0]) == true ? 1 : 0)));
        }
        #endregion

        #region Rendering
        [FunctionDefinition("draw_circle")]
        public static void draw_circle(Interpreter _vm, Domain _environment, LValue[] _arguments, Int32 _count, Stack<LValue> _stack) {
            GL.Begin(PrimitiveType.TriangleFan);

            double _x = ((double)_arguments[0].Value / (_vm.Assets.RoomWidth / 2));
            double _y = -((double)_arguments[1].Value / (_vm.Assets.RoomHeight / 2));
            double _r = 0.08;// (double)_arguments[2].Value;

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
            _stack.Push(new LValue(LType.Number, (double)0));
        }
        #endregion

        #region Math
        [FunctionDefinition("max")]
        public static void max(Interpreter _vm, Domain _environment, LValue[] _arguments, Int32 _count, Stack<LValue> _stack) {
            double[] _val = new double[_count];
            for (int i = 0; i < _count; i++) _val[i] = (double)_arguments[i];
            _stack.Push(new LValue(LType.Number, _val.Max()));
            /*double[] _val = new double[_count];
            for (int i = 0; i < _count; i++) _val[i] = _stack.Pop();
            _stack.Push(new LValue(LType.Number, _val.Max()));*/
        }

        [FunctionDefinition("min")]
        public static void min(Interpreter _vm, Domain _environment, LValue[] _arguments, Int32 _count, Stack<LValue> _stack) {
            double[] _val = new double[_count];
            for (int i = 0; i < _count; i++) _val[i] = (double)_arguments[i];
            _stack.Push(new LValue(LType.Number, _val.Min()));
        }
        #endregion
    }
}
