namespace NLog.BinaryCompatTests
{
    using System;
    using System.Reflection;

    public class Program
    {
        private static int PassedCount = 0;
        private static int FailedCount = 0;

        private static int Main(string[] args)
        {
            ConsoleColor oldColor = Console.ForegroundColor;

            try
            {
                RunAll(typeof(LogManagerTests));
                RunAll(typeof(LoggerTests));
                RunAll(typeof(LogEventInfoTests));
                RunAll(typeof(LogFactoryTests));
                if (FailedCount == 0)
                {
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine("All binary compatibility tests have passed.");
                }
                else
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("{0} tests have failed.", FailedCount);
                }

                return 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return 1;
            }
            finally
            {
                Console.ForegroundColor = oldColor;
            }
        }

        private static void RunAll(Type testClass)
        {
            Console.WriteLine("Running {0}:", testClass.Name);
            foreach (MethodInfo mi in testClass.GetMethods(BindingFlags.Static | BindingFlags.Public))
            {
                ConsoleColor oldColor = Console.ForegroundColor;
                try
                {
                    mi.Invoke(null, null);
                    PassedCount++;
                    // Console.ForegroundColor = ConsoleColor.Green;
                    // Console.Write("    OK ");
                    // Console.ForegroundColor = oldColor;
                    // Console.WriteLine("{0}", mi.Name);
                }
                catch (TargetInvocationException ex)
                {
                    FailedCount++;
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.Write("FAILED ");
                    Console.WriteLine("{0}", mi.Name);
                    Console.WriteLine("  {0}", ex.InnerException);
                }
                finally
                {
                    Console.ForegroundColor = oldColor;
                }
            }
        }
    }
}