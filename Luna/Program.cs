using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace Luna {
    class Program {
        static void Main(string[] args) {
            IFF _wad = new IFF(@"E:\Luna\Sample\LunaParent.win", new Game());
            _wad.Parse(delegate (Game _game) {
                _game.Initalize(true);
            });
            /*
            LunaTypes
            00000000 : 02000f84                   pushi.e   2
            00000004 : ffff 25 45 030000a0        pop.v.i   $a$
            0000000c : 0000 06 c0 04000000        push.s    "1"
            00000014 : ffff 65 45 050000a0        pop.v.s   $b$
            0000001c : ffff 05 c0 030000a0        push.v    $a$
            00000024 : ffff 05 c0 050000a0        push.v    $b$
            0000002c : 0006 55 15                 set.v.v
            00000030 : 0800 00 b8                 bf        0x00000050

            00000034 : 0000 06 c0 06000000        push.s    "PASS"
            0000003c : 0000 56 07                 conv.s.v
            00000040 : 0100 02 d9 07000000        call.i    $show_debug_message$
            00000048 : 0000 05 9e                 popz.v
            0000004c : 0700 00 b6                 b         0x00000068

            00000050 : 0000 06 c0 08000000        push.s    "FAIL"
            00000058 : 0000 56 07                 conv.s.v
            0000005c : 0100 02 d9 07000000        call.i    $show_debug_message$
            00000064 : 0000 05 9e                 popz.v
            */

            /*
            Luna2Vars
            00000000 : 2000 0f 84                  pushi.e   32
            00000004 : ffff 25 45 030000a0         pop.v.i   $a$
            0000000c : 8000 0f 84                  pushi.e   128
            00000010 : ffff 25 45 040000a0         pop.v.i   $b$
            */

            /*
            LunaConditional
            00000000 : 2000 0f 84                  pushi.e   32
            00000004 : f9ff 25 45 030000a0         pop.v.i   $a$
            0000000c : 0000 06 c0 0b000000         push.s    "Hello World!"
            00000014 : ffff 65 45 040000a0         pop.v.s   $b$
            0000001c : f9ff 05 c1 030000a0         pushl.v   $a$
            00000024 : 2000 0f 84                  pushi.e   32
            00000028 : 0003 52 15                  set.i.v
            0000002c : 0700 00 b8                  bf        0x00000048

            00000030 : 0000 06 c0 0c000000         push.s    "a is equal to 32"
            00000038 : 0000 56 07                  conv.s.v
            0000003c : 0100 02 d9 0d000000         call.i    $show_debug_message$
            00000044 : 0000 05 9e                  popz.v
            */

            Console.ReadKey();
        }
    }
}
