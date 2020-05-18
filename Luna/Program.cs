#define DEBUG

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace Luna {
    class Program {
        static void Main(string[] args) {
            IFF WAD = new IFF(@"E:\Luna\Sample\Project3.win", new Game());
            
            WAD.Parse();

            Console.ReadKey();
        }
    }
}
