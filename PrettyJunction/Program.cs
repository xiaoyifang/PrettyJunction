using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using NDesk.Options;

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
            string configFile=null;
            string directory=null;
            bool show_help=false;
            var p = new OptionSet() {
			    { "f|config=", "the {CONFIG}",
			      v => configFile=v },
			    { "c|clean=", "clean the {DIRECTORY}",
			      v => directory=v },
			    { "h|help",  "show this message and exit", 
			      v => show_help = v != null },
		    };

            List<string> extra;
            try
            {
                extra = p.Parse(args);
            }
            catch (OptionException e)
            {
                Helper.WriteError(e.Message);
                Console.WriteLine("Try `prettyjunction --help' for more information.");
                return;
            }
            //if(show_help)
            {
                PrintUsage(p);
                //return;
            }

            JunctionManager manager=new JunctionManager();
            if (!string.IsNullOrEmpty(directory))
            {
                manager.CleanDirectory(directory);
            }
            if(!string.IsNullOrEmpty(configFile))
            {
                manager.ProcessFile(configFile);
            }
            Console.WriteLine("Finish processing.");


        }
        private static  void PrintUsage()
        {
            Console.WriteLine(@"
=================================================
Aim to replace junction

=================================================
"
                );
            Console.WriteLine("Try `prettyjunction --help' for more information.");

        }
        static void PrintUsage(OptionSet p)
        {
            PrintUsage();
            Console.WriteLine("Options:");
            p.WriteOptionDescriptions(Console.Out);
            Console.WriteLine();
        }

        
    }
}
