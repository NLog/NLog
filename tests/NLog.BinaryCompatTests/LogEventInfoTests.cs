namespace NLog.BinaryCompatTests
{
    using System;
    using System.Collections;
    using System.Globalization;

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

        public static void LogEventInfoTest3()
        {
            var logEventInfo = new LogEventInfo(LogLevel.Info, "Foo", CultureInfo.InvariantCulture, "message {0}", new object[] { 1 });
            Assert(logEventInfo.Context.Count == 0);
            logEventInfo.Context["AAA"] = "bbb";
            Assert(logEventInfo.Context.Count == 1);

            // NLog v2 uses a wrapper here (and the property is marked as deprecated)
            // so make sure it is functional

            foreach (DictionaryEntry de in logEventInfo.Context)
            {
                Assert("AAA".Equals(de.Key));
                Assert("bbb".Equals(de.Value));
            }

            foreach (object key in logEventInfo.Context.Keys)
            {
                Assert("AAA".Equals(key));
            }

            foreach (object value in logEventInfo.Context.Values)
            {
                Assert("bbb".Equals(value));
            }
        }

        private static void Assert(bool p)
        {
            if (!p)
            {
                throw new InvalidOperationException("Assertion failed.");
            }
        }
    }
}