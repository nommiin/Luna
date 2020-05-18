using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace Luna {
    class Program {
        public static string Path = @"E:\Luna\Sample\Project3.win";
        static void Main(string[] args) {
            IFF WAD = new IFF(Path, new Game());
            

            WAD.Parse();
            Console.ReadKey();
        }
    }
}
