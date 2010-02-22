using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Linq;

namespace DumpTestResultSummary
{
    class Program
    {
        static XNamespace mstest2006Namespace = "http://microsoft.com/schemas/VisualStudio/TeamTest/2006";

        static int Main(string[] args)
        {
            var oldColor = Console.ForegroundColor;

            try
            {
                if (args.Length < 2)
                {
                    Usage();
                    return 1;
                }

                string trxFileName = args[1];
                if (!File.Exists(trxFileName))
                {
                    Console.Write(args[0].PadRight(35));
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("TRX file not found!");
                    return 0;
                }

                XElement element = XElement.Load(trxFileName);
                var analyzer = new MSTestResultsFileAnalyzer(mstest2006Namespace);
                analyzer.Label = args[0];
                analyzer.DumpSummary(element);

                return 0;
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("ERROR: {0}", ex);
                return 1;
            }
            finally
            {
                Console.ForegroundColor = oldColor;
            }
        }

        private static void Usage()
        {
            Console.WriteLine("Usage: DumpTestResultSummary label filename.trx [detailed]");
        }
    }
}
