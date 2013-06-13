using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PrettyJunction
{
    public class Helper
    {
        public static void WriteError(string error, params object[] arg)
        {
            var fgcolor = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine(error, arg);
            Console.ForegroundColor = fgcolor;
        }
    }
}
