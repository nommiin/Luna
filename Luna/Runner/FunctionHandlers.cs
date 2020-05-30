using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
        public static void show_debug_message(Stack<LValue> _stack) {
            Console.WriteLine(_stack.Pop().Value);
            _stack.Push(new LValue(LType.Number, 0));
        }

        [FunctionDefinition("string")]
        public static void _string(Stack<LValue> _stack) {
            _stack.Push(_stack.Pop().Convert(LType.String));
        }

        [FunctionDefinition("real")]
        public static void real(Stack<LValue> _stack) {
            _stack.Push(_stack.Pop().Convert(LType.String));
        }

        [FunctionDefinition("ds_map_create")]
        public static void ds_map_create(Stack<LValue> _stack) {
            _stack.Push(new LValue(LType.Number, new LMap().Index));
        }

        [FunctionDefinition("ds_map_set")]
        public static void ds_map_set(Stack<LValue> _stack) {
            double _mapIndex = (double)_stack.Pop().Value;
            object _propertyKey = _stack.Pop().Value;
            LValue _propertyValue = _stack.Pop();
            LMap.Registry[(int)_mapIndex].Data.Add(_propertyKey, _propertyValue);
            _stack.Push(new LValue(LType.Number, 0));
        }

        [FunctionDefinition("ds_map_find_value")]
        public static void ds_map_find_value(Stack<LValue> _stack) {
            double _mapIndex = (double)_stack.Pop().Value;
            object _propertyKey = _stack.Pop().Value;
            LMap _mapGet = LMap.Registry[(int)_mapIndex];
            if (_mapGet.Data.ContainsKey(_propertyKey) == true) {
                _stack.Push(_mapGet.Data[_propertyKey]);
            } else {
                _stack.Push(new LValue(LType.Undefined));
            }
        }

        [FunctionDefinition("@@NewGMLArray@@")]
        public static void NewGMLArray(Stack<LValue> _stack) {

        }

    }
}
