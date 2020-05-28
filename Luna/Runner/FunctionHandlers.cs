using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Luna.Types;

namespace Luna.Runner {
    static class FunctionHandlers {
        public static void show_debug_message(Stack<LValue> _stack) {
            Console.WriteLine(_stack.Pop().Value);
            _stack.Push(new LValue(LType.Number, 0));
        }

        public static void _string(Stack<LValue> _stack) {
            _stack.Push(_stack.Pop().Convert(LType.String));
        }

        public static void real(Stack<LValue> _stack) {
            _stack.Push(_stack.Pop().Convert(LType.String));
        }

        public static void ds_map_create(Stack<LValue> _stack) {
            _stack.Push(new LValue(LType.Number, new LMap().Index));
        }

        public static void ds_map_set(Stack<LValue> _stack) {
            double _mapIndex = _stack.Pop().Value;
            dynamic _propertyKey = _stack.Pop().Value;
            LValue _propertyValue = _stack.Pop();
            LMap.Registry[(int)_mapIndex].Data.Add(_propertyKey, _propertyValue);
            _stack.Push(new LValue(LType.Number, 0));
        }

        public static void ds_map_find_value(Stack<LValue> _stack) {
            double _mapIndex = _stack.Pop().Value;
            dynamic _propertyKey = _stack.Pop().Value;
            LMap _mapGet = LMap.Registry[(int)_mapIndex];
            if (_mapGet.Data.ContainsKey(_propertyKey) == true) {
                _stack.Push(_mapGet.Data[_propertyKey]);
            } else {
                _stack.Push(new LValue(LType.Undefined));
            }
        }
    }
}
