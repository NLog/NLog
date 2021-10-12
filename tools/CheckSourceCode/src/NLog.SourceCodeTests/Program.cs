using System;
using System.Linq;

namespace NLog.SourceCodeTests
{
    class Program
    {
        static int Main(string[] args)
        {
            var tests = new SourceCodeTests();
            var success = tests.VerifyFileHeaders();
            success = success & tests.VerifyNamespacesAndClassNames();

            var noInteractive = args.FirstOrDefault() == "no-interactive";

            if (success)
            {
                Console.WriteLine("YESS everything OK");
            }

            if (!noInteractive)
            {
                Console.WriteLine("press any key");
                Console.ReadKey();
            }

            if (success)
            {
                //error
                return 0;
            }
            return 1;
        }
    }
}
