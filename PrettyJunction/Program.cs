using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace PrettyJunction
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length == 0)
            {
                PrintUsage();
                return;
            }
            PrintUsage();
            
            //string filename = @"D:\临时代码\TestJunctionFolder\junction.txt";
            JunctionManager manager=new JunctionManager();
            //manager.ProcessFile(filename);
            manager.ProcessFile(args[0]);
            Console.WriteLine("Finish processing.");


        }
        private static  void PrintUsage()
        {
            Console.WriteLine(@"
=================================================
Aim to replace junction
Usages:
    PrettyJunction.exe configFile
=================================================
"
                );
        }

        
    }
}
