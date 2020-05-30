using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Luna.Types;

namespace Luna.Runner {
    [AttributeUsage(AttributeTargets.Method)]
    public class FunctionDefinition : Attribute {
        public string Name;

        public FunctionDefinition(string _name) {
            this.Name = _name;
        }
    }

    static class FunctionHandlers {
        [FunctionDefinition("show_debug_message")]
        public static void show_debug_message(Interpreter _vm, Int32 _count, Stack<LValue> _stack) {
            Console.WriteLine(_stack.Pop().Value);
            _stack.Push(new LValue(LType.Number, 0));
        }

        [FunctionDefinition("show_message")]
        public static void show_message(Interpreter _vm, Int32 _count, Stack<LValue> _stack) {
            MessageBox.Show((string)_stack.Pop().Convert(LType.String).Value, _vm.Data.DisplayName, MessageBoxButtons.OK);
            _stack.Push(new LValue(LType.Number, 0));
        }

        [FunctionDefinition("real")]
        public static void real(Interpreter _vm, Int32 _count, Stack<LValue> _stack) {
            _stack.Push(_stack.Pop().Convert(LType.String));
        }

        [FunctionDefinition("ds_map_create")]
        public static void ds_map_create(Interpreter _vm, Int32 _count, Stack<LValue> _stack) {
            _stack.Push(new LValue(LType.Number, new LMap().Index));
        }

        [FunctionDefinition("ds_map_set")]
        public static void ds_map_set(Interpreter _vm, Int32 _count, Stack<LValue> _stack) {
            double _mapIndex = (double)_stack.Pop().Value;
            object _propertyKey = _stack.Pop().Value;
            LValue _propertyValue = _stack.Pop();
            LMap.Registry[(int)_mapIndex].Data.Add(_propertyKey, _propertyValue);
            _stack.Push(new LValue(LType.Number, 0));
        }

        [FunctionDefinition("ds_map_find_value")]
        public static void ds_map_find_value(Interpreter _vm, Int32 _count, Stack<LValue> _stack) {
            double _mapIndex = (double)_stack.Pop().Value;
            object _propertyKey = _stack.Pop().Value;
            LMap _mapGet = LMap.Registry[(int)_mapIndex];
            if (_mapGet.Data.ContainsKey(_propertyKey) == true) {
                _stack.Push(_mapGet.Data[_propertyKey]);
            } else {
                _stack.Push(new LValue(LType.Undefined));
            }
        }

        [FunctionDefinition("string")]
        public static void _string(Interpreter _vm, Int32 _count, Stack<LValue> _stack) {
            _stack.Push(_stack.Pop().Convert(LType.String));
        }

        [FunctionDefinition("@@NewGMLArray@@")]
        public static void NewGMLArray(Interpreter _vm, Int32 _count, Stack<LValue> _stack) {
            LValue _valArray = new LValue(LType.Array, new List<LValue>());
            for (Int32 i = 0; i < _count; i++) _valArray.Array.Add(_stack.Pop());
            _stack.Push(_valArray);
            //_stack.Push(new LValue(LType.Array))
        }

    }
}
