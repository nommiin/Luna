using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Luna;
using Luna.Assets;
using Luna.Types;

namespace Luna.Runner {
    /*
        NOTE:
        Methods cannot share names with any types, so for functions that do; an underscore should be added to the start of the function
        When the Interpreter class loads function delegates, the underscore will be trimmed from the name
    */
    static class FunctionHandlers {
        public static void show_debug_message(Interpreter _vm, Int32 _count) {
            Console.WriteLine(_vm.Stack.Pop().Value);
            _vm.Stack.Push(new LValue(LType.Number, 0));
        }

        public static void show_message(Interpreter _vm, Int32 _count) {
            MessageBox.Show(_vm.Stack.Pop().Convert(LType.String).Value, _vm.Data.DisplayName, MessageBoxButtons.OK);
            _vm.Stack.Push(new LValue(LType.Number, 0));
        }

        public static void _string(Interpreter _vm, Int32 _count) {
            _vm.Stack.Push(_vm.Stack.Pop().Convert(LType.String));
        }

        public static void real(Interpreter _vm, Int32 _count) {
            _vm.Stack.Push(_vm.Stack.Pop().Convert(LType.Number));
        }

        public static void ds_map_create(Interpreter _vm, Int32 _count) {
            _vm.Stack.Push(new LValue(LType.Number, new LMap().Index));
        }

        public static void ds_map_set(Interpreter _vm, Int32 _count) {
            double _mapIndex = _vm.Stack.Pop().Value;
            dynamic _propertyKey = _vm.Stack.Pop();
            LValue _propertyValue = _vm.Stack.Pop();
            LMap.Registry[(int)_mapIndex].Data.Add(_propertyKey.Value, _propertyValue);
            _vm.Stack.Push(new LValue(LType.Number, 0));
        }

        public static void ds_map_find_value(Interpreter _vm, Int32 _count) {
            double _mapIndex = _vm.Stack.Pop().Value;
            dynamic _propertyKey = _vm.Stack.Pop().Value;
            LMap _mapGet = LMap.Registry[(int)_mapIndex];
            if (_mapGet.Data.ContainsKey(_propertyKey) == true) {
                _vm.Stack.Push(_mapGet.Data[_propertyKey]);
            } else {
                _vm.Stack.Push(new LValue(LType.Undefined));
            }
        }
    }
}
