using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK;
using OpenTK.Input;

namespace Luna.Runner {
    class Input {
        private static List<Key> InputDown;
        private static List<Key> InputDownPrevious;
        public static Dictionary<double, Key> InputMapping = new Dictionary<double, Key>() {
            [32] = Key.Space,
            [37] = Key.Left,
            [38] = Key.Up,
            [39] = Key.Right,
            [40] = Key.Down
        };

        public static void Initalize(Game _game) {
            InputDown = new List<Key>();
            InputDownPrevious = new List<Key>();
            _game.Window.KeyUp += OnKeyUp;
            _game.Window.KeyDown += OnKeyDown;
            //code to allow for key remapping, but fill default values
            Enum.GetValues(typeof(Key)).Cast<Key>().ToList().ForEach(_key =>
            {
                if (!InputMapping.ContainsKey((double)_key))InputMapping.Add((double) _key, _key);
            });
        }

        public static void OnKeyUp(object sender, KeyboardKeyEventArgs e) {
            while (InputDown.Contains(e.Key)) {
                InputDown.Remove(e.Key);
            }
        }

        public static void OnKeyDown(object sender, KeyboardKeyEventArgs e) {
            InputDown.Add(e.Key);
        }

        public static void OnKeyUpdate() {
            InputDownPrevious = new List<Key>(InputDown);
        }

        public static bool KeyPressed(double _key) {
            Key _keyMapping = InputMapping[_key];
            Console.WriteLine("{0} was pressed", _keyMapping);
            return InputDown.Contains(_keyMapping) && InputDownPrevious.Contains(_keyMapping) == false;
        }

        public static bool KeyReleased(double _key) {
            Key _keyMapping = InputMapping[_key];
            return InputDownPrevious.Contains(_keyMapping) && InputDown.Contains(_keyMapping) == false;
        }

        public static bool KeyCheck(double _key)
        {
            Key _keyMapping = InputMapping[_key];
            return InputDown.Contains(_keyMapping);
        }
    }
}
