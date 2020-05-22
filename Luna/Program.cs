using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace Luna {
    class Program {
        static void Main(string[] args) {
            IFF WAD = new IFF(@"E:\Luna\Sample\LunaMath.win", new Game());
            WAD.Parse();

            Interpreter _int = new Interpreter(WAD.Data);
            _int.ExecuteScript("gml_GlobalScript_Script2");
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
