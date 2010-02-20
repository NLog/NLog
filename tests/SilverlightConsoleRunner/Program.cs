namespace SilverlightConsoleRunner
{
    using System;
    using System.IO;

    internal class Program
    {
        [STAThread]
        public static int Main(string[] args)
        {
            try
            {
                if (args.Length < 2)
                {
                    Console.WriteLine("Usage: SilverlightConsoleRunner testfile.xap [SL2|SL3] [logfile.log]");
                    return 1;
                }

                string logfile = "UnitTests.log";
                if (args.Length > 2)
                {
                    logfile = args[2];
                }

                string xapFile = args[0];
                var runner = new ConsoleRunner()
                {
                    XapFile = xapFile,
                    SilverlightVersion = args[1],
                };

                Console.WriteLine("Running tests in '{0}'", Path.GetFullPath(runner.XapFile));
                Console.WriteLine("Silverlight version: '{0}'", runner.SilverlightVersion);
                Console.WriteLine("Log file: '{0}'", Path.GetFullPath(logfile));

                using (var log = new StreamWriter(logfile))
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