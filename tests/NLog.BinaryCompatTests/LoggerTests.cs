namespace NLog.BinaryCompatTests
{
    using System;
    using System.Globalization;

    public static class LoggerTests
    {
        public static void DebugTest()
        {
            Logger logger = LogManager.GetLogger("A");

            bool isEnabled = logger.IsDebugEnabled;
            logger.Debug("message");
            logger.Debug("message{0}", (ulong)1);
            logger.Debug(CultureInfo.InvariantCulture, "message{0}", (ulong)2);
            logger.Debug("message{0}", (long)1);
            logger.Debug(CultureInfo.InvariantCulture, "message{0}", (long)2);
            logger.Debug("message{0}", (uint)1);
            logger.Debug(CultureInfo.InvariantCulture, "message{0}", (uint)2);
            logger.Debug("message{0}", (int)1);
            logger.Debug(CultureInfo.InvariantCulture, "message{0}", (int)2);
            logger.Debug("message{0}", (ushort)1);
            logger.Debug(CultureInfo.InvariantCulture, "message{0}", (ushort)2);
            logger.Debug("message{0}", (sbyte)1);
            logger.Debug(CultureInfo.InvariantCulture, "message{0}", (sbyte)2);
            logger.Debug("message{0}", new object());
            logger.Debug(CultureInfo.InvariantCulture, "message{0}", new object());
            logger.Debug("message{0}", (short)1);
            logger.Debug(CultureInfo.InvariantCulture, "message{0}", (short)2);
            logger.Debug("message{0}", (byte)1);
            logger.Debug(CultureInfo.InvariantCulture, "message{0}", (byte)2);
            logger.Debug("message{0}", 'c');
            logger.Debug(CultureInfo.InvariantCulture, "message{0}", 'd');
            logger.Debug("message{0}", "ddd");
            logger.Debug(CultureInfo.InvariantCulture, "message{0}", "eee");
            logger.Debug("message{0}{1}", "ddd", 1);
            logger.Debug(CultureInfo.InvariantCulture, "message{0}{1}", "eee", 2);
            logger.Debug("message{0}{1}{2}", "ddd", 1, "eee");
            logger.Debug(CultureInfo.InvariantCulture, "message{0}{1}{2}", "eee", 2, "fff");
            logger.Debug("message{0}{1}{2}{3}", "eee", 2, "fff", "ggg");
            logger.Debug("message{0}", true);
            logger.Debug(CultureInfo.InvariantCulture, "message{0}", false);
            logger.Debug(CultureInfo.InvariantCulture, "message{0}", (float)2.5);
            logger.Debug((double)1.5);
            logger.Debug(CultureInfo.InvariantCulture, (double)1.5);
            logger.Debug("message{0}", (double)1.5);
            logger.Debug(CultureInfo.InvariantCulture, "message{0}", (double)2.5);
            logger.Debug("message{0}", (decimal)1.5);
            logger.Debug(CultureInfo.InvariantCulture, "message{0}", (decimal)2.5);
            logger.DebugException("message", new Exception("test"));
        }

        public static void ErrorTest()
        {
            Logger logger = LogManager.GetLogger("A");

            bool isEnabled = logger.IsErrorEnabled;
            logger.Error("message");
            logger.Error("message{0}", (ulong)1);
            logger.Error(CultureInfo.InvariantCulture, "message{0}", (ulong)2);
            logger.Error("message{0}", (long)1);
            logger.Error(CultureInfo.InvariantCulture, "message{0}", (long)2);
            logger.Error("message{0}", (uint)1);
            logger.Error(CultureInfo.InvariantCulture, "message{0}", (uint)2);
            logger.Error("message{0}", (int)1);
            logger.Error(CultureInfo.InvariantCulture, "message{0}", (int)2);
            logger.Error("message{0}", (ushort)1);
            logger.Error(CultureInfo.InvariantCulture, "message{0}", (ushort)2);
            logger.Error("message{0}", (sbyte)1);
            logger.Error(CultureInfo.InvariantCulture, "message{0}", (sbyte)2);
            logger.Error("message{0}", new object());
            logger.Error(CultureInfo.InvariantCulture, "message{0}", new object());
            logger.Error("message{0}", (short)1);
            logger.Error(CultureInfo.InvariantCulture, "message{0}", (short)2);
            logger.Error("message{0}", (byte)1);
            logger.Error(CultureInfo.InvariantCulture, "message{0}", (byte)2);
            logger.Error("message{0}", 'c');
            logger.Error(CultureInfo.InvariantCulture, "message{0}", 'd');
            logger.Error("message{0}", "ddd");
            logger.Error(CultureInfo.InvariantCulture, "message{0}", "eee");
            logger.Error("message{0}{1}", "ddd", 1);
            logger.Error(CultureInfo.InvariantCulture, "message{0}{1}", "eee", 2);
            logger.Error("message{0}{1}{2}", "ddd", 1, "eee");
            logger.Error(CultureInfo.InvariantCulture, "message{0}{1}{2}", "eee", 2, "fff");
            logger.Error("message{0}{1}{2}{3}", "eee", 2, "fff", "ggg");
            logger.Error("message{0}", true);
            logger.Error(CultureInfo.InvariantCulture, "message{0}", false);
            logger.Error(CultureInfo.InvariantCulture, "message{0}", (float)2.5);
            logger.Error((double)1.5);
            logger.Error(CultureInfo.InvariantCulture, (double)1.5);
            logger.Error("message{0}", (double)1.5);
            logger.Error(CultureInfo.InvariantCulture, "message{0}", (double)2.5);
            logger.Error("message{0}", (decimal)1.5);
            logger.Error(CultureInfo.InvariantCulture, "message{0}", (decimal)2.5);
            logger.ErrorException("message", new Exception("test"));
        }

        public static void FatalTest()
        {
            Logger logger = LogManager.GetLogger("A");

            bool isEnabled = logger.IsFatalEnabled;
            logger.Fatal("message");
            logger.Fatal("message{0}", (ulong)1);
            logger.Fatal(CultureInfo.InvariantCulture, "message{0}", (ulong)2);
            logger.Fatal("message{0}", (long)1);
            logger.Fatal(CultureInfo.InvariantCulture, "message{0}", (long)2);
            logger.Fatal("message{0}", (uint)1);
            logger.Fatal(CultureInfo.InvariantCulture, "message{0}", (uint)2);
            logger.Fatal("message{0}", (int)1);
            logger.Fatal(CultureInfo.InvariantCulture, "message{0}", (int)2);
            logger.Fatal("message{0}", (ushort)1);
            logger.Fatal(CultureInfo.InvariantCulture, "message{0}", (ushort)2);
            logger.Fatal("message{0}", (sbyte)1);
            logger.Fatal(CultureInfo.InvariantCulture, "message{0}", (sbyte)2);
            logger.Fatal("message{0}", new object());
            logger.Fatal(CultureInfo.InvariantCulture, "message{0}", new object());
            logger.Fatal("message{0}", (short)1);
            logger.Fatal(CultureInfo.InvariantCulture, "message{0}", (short)2);
            logger.Fatal("message{0}", (byte)1);
            logger.Fatal(CultureInfo.InvariantCulture, "message{0}", (byte)2);
            logger.Fatal("message{0}", 'c');
            logger.Fatal(CultureInfo.InvariantCulture, "message{0}", 'd');
            logger.Fatal("message{0}", "ddd");
            logger.Fatal(CultureInfo.InvariantCulture, "message{0}", "eee");
            logger.Fatal("message{0}{1}", "ddd", 1);
            logger.Fatal(CultureInfo.InvariantCulture, "message{0}{1}", "eee", 2);
            logger.Fatal("message{0}{1}{2}", "ddd", 1, "eee");
            logger.Fatal(CultureInfo.InvariantCulture, "message{0}{1}{2}", "eee", 2, "fff");
            logger.Fatal("message{0}{1}{2}{3}", "eee", 2, "fff", "ggg");
            logger.Fatal("message{0}", true);
            logger.Fatal(CultureInfo.InvariantCulture, "message{0}", false);
            logger.Fatal(CultureInfo.InvariantCulture, "message{0}", (float)2.5);
            logger.Fatal((double)1.5);
            logger.Fatal(CultureInfo.InvariantCulture, (double)1.5);
            logger.Fatal("message{0}", (double)1.5);
            logger.Fatal(CultureInfo.InvariantCulture, "message{0}", (double)2.5);
            logger.Fatal("message{0}", (decimal)1.5);
            logger.Fatal(CultureInfo.InvariantCulture, "message{0}", (decimal)2.5);
            logger.FatalException("message", new Exception("test"));
        }

        public static void InfoTest()
        {
            Logger logger = LogManager.GetLogger("A");

            bool isEnabled = logger.IsInfoEnabled;
            logger.Info("message");
            logger.Info("message{0}", (ulong)1);
            logger.Info(CultureInfo.InvariantCulture, "message{0}", (ulong)2);
            logger.Info("message{0}", (long)1);
            logger.Info(CultureInfo.InvariantCulture, "message{0}", (long)2);
            logger.Info("message{0}", (uint)1);
            logger.Info(CultureInfo.InvariantCulture, "message{0}", (uint)2);
            logger.Info("message{0}", (int)1);
            logger.Info(CultureInfo.InvariantCulture, "message{0}", (int)2);
            logger.Info("message{0}", (ushort)1);
            logger.Info(CultureInfo.InvariantCulture, "message{0}", (ushort)2);
            logger.Info("message{0}", (sbyte)1);
            logger.Info(CultureInfo.InvariantCulture, "message{0}", (sbyte)2);
            logger.Info("message{0}", new object());
            logger.Info(CultureInfo.InvariantCulture, "message{0}", new object());
            logger.Info("message{0}", (short)1);
            logger.Info(CultureInfo.InvariantCulture, "message{0}", (short)2);
            logger.Info("message{0}", (byte)1);
            logger.Info(CultureInfo.InvariantCulture, "message{0}", (byte)2);
            logger.Info("message{0}", 'c');
            logger.Info(CultureInfo.InvariantCulture, "message{0}", 'd');
            logger.Info("message{0}", "ddd");
            logger.Info(CultureInfo.InvariantCulture, "message{0}", "eee");
            logger.Info("message{0}{1}", "ddd", 1);
            logger.Info(CultureInfo.InvariantCulture, "message{0}{1}", "eee", 2);
            logger.Info("message{0}{1}{2}", "ddd", 1, "eee");
            logger.Info(CultureInfo.InvariantCulture, "message{0}{1}{2}", "eee", 2, "fff");
            logger.Info("message{0}{1}{2}{3}", "eee", 2, "fff", "ggg");
            logger.Info("message{0}", true);
            logger.Info(CultureInfo.InvariantCulture, "message{0}", false);
            logger.Info(CultureInfo.InvariantCulture, "message{0}", (float)2.5);
            logger.Info((double)1.5);
            logger.Info(CultureInfo.InvariantCulture, (double)1.5);
            logger.Info("message{0}", (double)1.5);
            logger.Info(CultureInfo.InvariantCulture, "message{0}", (double)2.5);
            logger.Info("message{0}", (decimal)1.5);
            logger.Info(CultureInfo.InvariantCulture, "message{0}", (decimal)2.5);
            logger.InfoException("message", new Exception("test"));
        }

        public static void LogTest()
        {
            Logger logger = LogManager.GetLogger("A");

            logger.Log(LogLevel.Trace, "message");
            logger.Log(LogLevel.Trace, "message{0}", (ulong)1);
            logger.Log(LogLevel.Trace, CultureInfo.InvariantCulture, "message{0}", (ulong)2);
            logger.Log(LogLevel.Trace, "message{0}", (long)1);
            logger.Log(LogLevel.Trace, CultureInfo.InvariantCulture, "message{0}", (long)2);
            logger.Log(LogLevel.Trace, "message{0}", (uint)1);
            logger.Log(LogLevel.Trace, CultureInfo.InvariantCulture, "message{0}", (uint)2);
            logger.Log(LogLevel.Trace, "message{0}", (int)1);
            logger.Log(LogLevel.Trace, CultureInfo.InvariantCulture, "message{0}", (int)2);
            logger.Log(LogLevel.Trace, "message{0}", (ushort)1);
            logger.Log(LogLevel.Trace, CultureInfo.InvariantCulture, "message{0}", (ushort)2);
            logger.Log(LogLevel.Trace, "message{0}", (sbyte)1);
            logger.Log(LogLevel.Trace, CultureInfo.InvariantCulture, "message{0}", (sbyte)2);
            logger.Log(LogLevel.Trace, "message{0}", new object());
            logger.Log(LogLevel.Trace, CultureInfo.InvariantCulture, "message{0}", new object());
            logger.Log(LogLevel.Trace, "message{0}", (short)1);
            logger.Log(LogLevel.Trace, CultureInfo.InvariantCulture, "message{0}", (short)2);
            logger.Log(LogLevel.Trace, "message{0}", (byte)1);
            logger.Log(LogLevel.Trace, CultureInfo.InvariantCulture, "message{0}", (byte)2);
            logger.Log(LogLevel.Trace, "message{0}", 'c');
            logger.Log(LogLevel.Trace, CultureInfo.InvariantCulture, "message{0}", 'd');
            logger.Log(LogLevel.Trace, "message{0}", "ddd");
            logger.Log(LogLevel.Trace, CultureInfo.InvariantCulture, "message{0}", "eee");
            logger.Log(LogLevel.Trace, "message{0}{1}", "ddd", 1);
            logger.Log(LogLevel.Trace, CultureInfo.InvariantCulture, "message{0}{1}", "eee", 2);
            logger.Log(LogLevel.Trace, "message{0}{1}{2}", "ddd", 1, "eee");
            logger.Log(LogLevel.Trace, CultureInfo.InvariantCulture, "message{0}{1}{2}", "eee", 2, "fff");
            logger.Log(LogLevel.Trace, "message{0}{1}{2}{3}", "eee", 2, "fff", "ggg");
            logger.Log(LogLevel.Trace, "message{0}", true);
            logger.Log(LogLevel.Trace, CultureInfo.InvariantCulture, "message{0}", false);
            logger.Log(LogLevel.Trace, CultureInfo.InvariantCulture, "message{0}", (float)2.5);
            logger.Log(LogLevel.Trace, (double)1.5);
            logger.Log(LogLevel.Trace, CultureInfo.InvariantCulture, (double)1.5);
            logger.Log(LogLevel.Trace, "message{0}", (double)1.5);
            logger.Log(LogLevel.Trace, CultureInfo.InvariantCulture, "message{0}", (double)2.5);
            logger.Log(LogLevel.Trace, "message{0}", (decimal)1.5);
            logger.Log(LogLevel.Trace, CultureInfo.InvariantCulture, "message{0}", (decimal)2.5);
            logger.LogException(LogLevel.Trace, "message", new Exception("test"));
        }

        public static void TraceTest()
        {
            Logger logger = LogManager.GetLogger("A");

            bool isEnabled = logger.IsTraceEnabled;
            logger.Trace("message");
            logger.Trace("message{0}", (ulong)1);
            logger.Trace(CultureInfo.InvariantCulture, "message{0}", (ulong)2);
            logger.Trace("message{0}", (long)1);
            logger.Trace(CultureInfo.InvariantCulture, "message{0}", (long)2);
            logger.Trace("message{0}", (uint)1);
            logger.Trace(CultureInfo.InvariantCulture, "message{0}", (uint)2);
            logger.Trace("message{0}", (int)1);
            logger.Trace(CultureInfo.InvariantCulture, "message{0}", (int)2);
            logger.Trace("message{0}", (ushort)1);
            logger.Trace(CultureInfo.InvariantCulture, "message{0}", (ushort)2);
            logger.Trace("message{0}", (sbyte)1);
            logger.Trace(CultureInfo.InvariantCulture, "message{0}", (sbyte)2);
            logger.Trace("message{0}", new object());
            logger.Trace(CultureInfo.InvariantCulture, "message{0}", new object());
            logger.Trace("message{0}", (short)1);
            logger.Trace(CultureInfo.InvariantCulture, "message{0}", (short)2);
            logger.Trace("message{0}", (byte)1);
            logger.Trace(CultureInfo.InvariantCulture, "message{0}", (byte)2);
            logger.Trace("message{0}", 'c');
            logger.Trace(CultureInfo.InvariantCulture, "message{0}", 'd');
            logger.Trace("message{0}", "ddd");
            logger.Trace(CultureInfo.InvariantCulture, "message{0}", "eee");
            logger.Trace("message{0}{1}", "ddd", 1);
            logger.Trace(CultureInfo.InvariantCulture, "message{0}{1}", "eee", 2);
            logger.Trace("message{0}{1}{2}", "ddd", 1, "eee");
            logger.Trace(CultureInfo.InvariantCulture, "message{0}{1}{2}", "eee", 2, "fff");
            logger.Trace("message{0}{1}{2}{3}", "eee", 2, "fff", "ggg");
            logger.Trace("message{0}", true);
            logger.Trace(CultureInfo.InvariantCulture, "message{0}", false);
            logger.Trace(CultureInfo.InvariantCulture, "message{0}", (float)2.5);
            logger.Trace((double)1.5);
            logger.Trace(CultureInfo.InvariantCulture, (double)1.5);
            logger.Trace("message{0}", (double)1.5);
            logger.Trace(CultureInfo.InvariantCulture, "message{0}", (double)2.5);
            logger.Trace("message{0}", (decimal)1.5);
            logger.Trace(CultureInfo.InvariantCulture, "message{0}", (decimal)2.5);
            logger.TraceException("message", new Exception("test"));
        }

        public static void WarnTest()
        {
            Logger logger = LogManager.GetLogger("A");

            bool isEnabled = logger.IsWarnEnabled;
            logger.Warn("message");
            logger.Warn("message{0}", (ulong)1);
            logger.Warn(CultureInfo.InvariantCulture, "message{0}", (ulong)2);
            logger.Warn("message{0}", (long)1);
            logger.Warn(CultureInfo.InvariantCulture, "message{0}", (long)2);
            logger.Warn("message{0}", (uint)1);
            logger.Warn(CultureInfo.InvariantCulture, "message{0}", (uint)2);
            logger.Warn("message{0}", (int)1);
            logger.Warn(CultureInfo.InvariantCulture, "message{0}", (int)2);
            logger.Warn("message{0}", (ushort)1);
            logger.Warn(CultureInfo.InvariantCulture, "message{0}", (ushort)2);
            logger.Warn("message{0}", (sbyte)1);
            logger.Warn(CultureInfo.InvariantCulture, "message{0}", (sbyte)2);
            logger.Warn("message{0}", new object());
            logger.Warn(CultureInfo.InvariantCulture, "message{0}", new object());
            logger.Warn("message{0}", (short)1);
            logger.Warn(CultureInfo.InvariantCulture, "message{0}", (short)2);
            logger.Warn("message{0}", (byte)1);
            logger.Warn(CultureInfo.InvariantCulture, "message{0}", (byte)2);
            logger.Warn("message{0}", 'c');
            logger.Warn(CultureInfo.InvariantCulture, "message{0}", 'd');
            logger.Warn("message{0}", "ddd");
            logger.Warn(CultureInfo.InvariantCulture, "message{0}", "eee");
            logger.Warn("message{0}{1}", "ddd", 1);
            logger.Warn(CultureInfo.InvariantCulture, "message{0}{1}", "eee", 2);
            logger.Warn("message{0}{1}{2}", "ddd", 1, "eee");
            logger.Warn(CultureInfo.InvariantCulture, "message{0}{1}{2}", "eee", 2, "fff");
            logger.Warn("message{0}{1}{2}{3}", "eee", 2, "fff", "ggg");
            logger.Warn("message{0}", true);
            logger.Warn(CultureInfo.InvariantCulture, "message{0}", false);
            logger.Warn(CultureInfo.InvariantCulture, "message{0}", (float)2.5);
            logger.Warn((double)1.5);
            logger.Warn(CultureInfo.InvariantCulture, (double)1.5);
            logger.Warn("message{0}", (double)1.5);
            logger.Warn(CultureInfo.InvariantCulture, "message{0}", (double)2.5);
            logger.Warn("message{0}", (decimal)1.5);
            logger.Warn(CultureInfo.InvariantCulture, "message{0}", (decimal)2.5);
            logger.WarnException("message", new Exception("test"));
        }
    }
}