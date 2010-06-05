using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BuildDocPages
{
    class Program
    {
        static int Main(string[] args)
        {
            try
            {
                if (args.Length != 6)
                {
                    Usage();
                    return 1;
                }

                DocPageGenerator generator = new DocPageGenerator();
                generator.InputFile = args[0];
                generator.Stylesheet = args[1];
                generator.OutputDirectory = args[2];
                generator.BaseDirectory = args[3];
                generator.FileSuffix = args[4];
                generator.Mode = args[5];
                generator.Generate();
                return 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine("ERROR: {0}", ex);
                return 1;
            }
        }

        private static void Usage()
        {
            Console.WriteLine("Usage: BuildDocPages nlog.api output_directory base_directory_for_sources file_suffix mode");
        }
    }
}
