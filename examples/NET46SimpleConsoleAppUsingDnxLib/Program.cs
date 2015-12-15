using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NET46SimpleConsoleAppUsingDnxLib
{
    class Program
    {
        private static readonly ILogger logger = LogManager.GetCurrentClassLogger();
        private static readonly ILogger myLogger = LogManager.GetLogger("MyLogger");

        static void Main(string[] args)
        {
            logger.Debug("Hello world !");

            myLogger.Info("Hello from MyLogger");
        }
    }
}
