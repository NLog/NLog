using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using NLog.Config;
using NLog.Targets;

namespace NLog.BinaryCompatTests
{
    public static class LogEventInfoTests
    {
        public static void LogEventInfoTest1()
        {
            var logEventInfo = new LogEventInfo(LogLevel.Info, "Foo", "message");
        }

        public static void LogEventInfoTest2()
        {
            var logEventInfo = new LogEventInfo(LogLevel.Info, "Foo", CultureInfo.InvariantCulture, "message {0}", new object[] { 1 });
        }
    }
}
