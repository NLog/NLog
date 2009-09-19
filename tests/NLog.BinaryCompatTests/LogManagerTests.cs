using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using NLog.Config;
using NLog.Targets;

namespace NLog.BinaryCompatTests
{
    public static class LogManagerTests
    {
        public class MyLogger : Logger
        {
        }

        public static void GetLoggerTest1()
        {
            LogManager.GetLogger("AAA");
        }

        public static void GetLoggerTest2()
        {
            LogManager.GetLogger("AAA", typeof(MyLogger));
        }
    }
}
