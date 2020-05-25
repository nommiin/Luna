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
            MessageBox.Show(_vm.Stack.Pop().Value, _vm.Data.DisplayName, MessageBoxButtons.OK);
            _vm.Stack.Push(new LValue(LType.Number, 0));
        }

        public static void _string(Interpreter _vm, Int32 _count) {
            _vm.Stack.Push(_vm.Stack.Pop().Convert(LType.String));
        }

        public static void real(Interpreter _vm, Int32 _count) {
            _vm.Stack.Push(_vm.Stack.Pop().Convert(LType.Number));
        }
    }
}
