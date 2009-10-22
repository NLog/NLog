using System;
using System.IO;

namespace SilverlightConsoleRunner
{
    class Program
    {
        [STAThread]
        static int Main(string[] args)
        {
            try
            {
                string xapFile = args[0];
                var runner = new ConsoleRunner()
                                 {
                                     XapFile = xapFile
                                 };

                using (StreamWriter log = new StreamWriter("Test.log"))
                {
                    runner.LogWriter = log;
                    runner.Run();
                }

                return runner.FailedCount + runner.OtherCount;
            }
            catch (Exception ex)
            {
                Console.WriteLine("ERROR: {0}", ex);
                return 1;
            }
        }

    }
}
