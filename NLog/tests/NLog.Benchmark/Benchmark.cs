// 
// Copyright (c) 2004-2006 Jaroslaw Kowalski <jaak@jkowalski.net>
// 
// All rights reserved.
// 
// Redistribution and use in source and binary forms, with or without 
// modification, are permitted provided that the following conditions 
// are met:
// 
// * Redistributions of source code must retain the above copyright notice, 
//   this list of conditions and the following disclaimer. 
// 
// * Redistributions in binary form must reproduce the above copyright notice,
//   this list of conditions and the following disclaimer in the documentation
//   and/or other materials provided with the distribution. 
// 
// * Neither the name of Jaroslaw Kowalski nor the names of its 
//   contributors may be used to endorse or promote products derived from this
//   software without specific prior written permission. 
// 
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS"
// AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE 
// IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE 
// ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT OWNER OR CONTRIBUTORS BE 
// LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR 
// CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF
// SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS 
// INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN 
// CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) 
// ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF 
// THE POSSIBILITY OF SUCH DAMAGE.
// 

using System;
using System.Threading;
using System.Globalization;
using System.IO;
using System.Xml;
using System.Runtime.InteropServices;

#if LOG4NET || LOG4NET_WITH_FASTLOGGER
using log4net;
#elif ENTLIB
using System.Diagnostics;
using Microsoft.Practices.EnterpriseLibrary.Logging;
#else
using NLog;
#endif

#if LOG4NET || LOG4NET_WITH_FASTLOGGER

using log4net.Core;
using log4net.Appender;

public class NullAppender : AppenderSkeleton
{
    override protected void Append(LoggingEvent loggingEvent) 
    {
    }
}

public class NullAppenderWithLayout : AppenderSkeleton
{
    override protected void Append(LoggingEvent loggingEvent) 
    {
        string s = RenderLoggingEvent(loggingEvent);
        // ignore s
    }
}

#endif

class Bench
{
    private const int warmup = 10;
    private const int PRIORITY_DEBUG = 2;
    private const int UNROLL_COUNT = 1;
    private static int _repeat;
    private static string _currentTestName;
    private static XmlTextWriter _output;
    private static StopWatch _stopWatch = new StopWatch();

    public static void BeginTest(string name)
    {
        if (_output != null)
        {
            _output.WriteStartElement("timing");
            _output.WriteAttributeString("name", name);
        }
        _currentTestName = name;
        Console.Write("{0} ", name);
        _stopWatch.Start();
    }

    public static void EndTest()
    {
        //LogManager.Configuration.FlushAllTargets();
        _stopWatch.Stop();
        if (_output != null)
        {
            _output.WriteAttributeString("totalTime", Convert.ToString(TimeSpan.FromSeconds(_stopWatch.Seconds)));
            _output.WriteAttributeString("repetitions", Convert.ToString(_repeat * UNROLL_COUNT));
            _output.WriteAttributeString("logsPerSecond", Convert.ToString(_repeat * UNROLL_COUNT /_stopWatch.Seconds));
            _output.WriteAttributeString("nanosecondsPerLog", Convert.ToString(_stopWatch.Nanoseconds / (_repeat * UNROLL_COUNT)));
            _output.WriteEndElement();
        }
        //Console.Write("totalTime: {0} ", Convert.ToString(TimeSpan.FromSeconds(_stopWatch.Seconds)));
        //Console.Write("repetitions: {0} ", Convert.ToString(_repeat));
        //Console.Write("logsPerSecond: {0} ", Convert.ToString(_repeat /_stopWatch.Seconds));
        Console.Write("nanosecondsPerLog: {0} ", Convert.ToString(_stopWatch.Nanoseconds / (_repeat * UNROLL_COUNT)));
        Console.WriteLine();
    }

    public static void Main(string[]args)
    {
        int repeat = 1000000;
        int repeat2 = 100000;
        string outputFile = args[0];
        Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;
        if (args.Length > 1)
        {
            if (args[1] == "long")
            {
                repeat *= 10;
                repeat2 *= 10;
            }
            if (args[1] == "short")
            {
                repeat2 /= 10;
            }
            if (args[1] == "veryshort")
            {
                repeat2 /= 100;
            }
            if (args[1] == "veryveryshort")
            {
                repeat /= 100;
                repeat2 /= 100;
            }
            if (args[1] == "verylong")
            {
                repeat *= 100;
                repeat2 *= 10;
            }
        }
#if LOG4NET
        log4net.Config.XmlConfigurator.Configure(new FileInfo("log4net.config"));

        ILog logger1 = LogManager.GetLogger("nonlogger");
        ILog logger2 = LogManager.GetLogger("null1");
        ILog logger3 = LogManager.GetLogger("null2");
        ILog logger4 = LogManager.GetLogger("file1");
        ILog logger5 = LogManager.GetLogger("file2");
        ILog logger6 = LogManager.GetLogger("file3");
#elif LOG4NET_WITH_FASTLOGGER
        log4net.Config.XmlConfigurator.Configure(new FileInfo("log4net.config"));

        FastLogger logger1 = FastLoggerLogManager.GetLogger("nonlogger");
        FastLogger logger2 = FastLoggerLogManager.GetLogger("null1");
        FastLogger logger3 = FastLoggerLogManager.GetLogger("null2");
        FastLogger logger4 = FastLoggerLogManager.GetLogger("file1");
        FastLogger logger5 = FastLoggerLogManager.GetLogger("file2");
        FastLogger logger6 = FastLoggerLogManager.GetLogger("file3");
#elif ENTLIB
        string logger1 = "nonlogger";
        string logger2 = "null1";
        string logger3 = "null2";
        string logger4 = "file1";
        string logger5 = "file2";
        string logger6 = "file3";
#else        
        Logger logger1 = LogManager.GetLogger("nonlogger");
        Logger logger2 = LogManager.GetLogger("null1");
        Logger logger3 = LogManager.GetLogger("null2");
        Logger logger4 = LogManager.GetLogger("file1");
        Logger logger5 = LogManager.GetLogger("file2");
        Logger logger6 = LogManager.GetLogger("file3");
#endif
        // _output = new XmlTextWriter(Console.Out);

        // warm up
        LogTest(logger1, null, 5);
        //LogTest(logger4, null, 5);
        LogTest(logger2, null, 5);
        LogTest(logger3, null, 5);
        LogTest(logger4, null, 5);
        LogTest(logger5, null, 5);
        LogTest(logger6, null, 5);

        _output = new XmlTextWriter(outputFile, System.Text.Encoding.UTF8);
        _output.Formatting = Formatting.Indented;
        _output.WriteStartElement("results");
        LogTest(logger1, "Non-logging", repeat);
        LogTest(logger3, "Null-target without layout", repeat);
        LogTest(logger2, "Null-target with layout rendering", repeat);
#if WITHLONGTESTS
        LogTest(logger4, "File target", repeat2);
        LogTest(logger5, "Async file target", repeat2);
        LogTest(logger6, "Buffered file target", repeat2);
#if NLOG
        LogTest(LogManager.GetLogger("archive1"), "Non-concurrent archiving file target", repeat2);
        LogTest(LogManager.GetLogger("archive2"), "Non-concurrent archiving file target (often)", repeat2);
        LogTest(LogManager.GetLogger("archive3"), "Concurrent archiving file target", repeat2);
        LogTest(LogManager.GetLogger("archive4"), "Concurrent archiving file target (often)", repeat2);
#endif
#endif

        _output.WriteEndElement();
        _output.Close();
    }

    private static void LogTest(
#if LOG4NET
            ILog logger, 
#elif LOG4NET_WITH_FASTLOGGER
            FastLogger logger,
#elif ENTLIB
            string category,
#else
            Logger logger, 
#endif
            string loggerDesc, int repeat)
    {
        _repeat = repeat;

#if ENTLIB
        LogEntry entry;
#endif

        if (_output != null)
        {
            _output.WriteStartElement("test");
            _output.WriteAttributeString("logger", loggerDesc);
            _output.WriteAttributeString("repetitions", repeat.ToString());
        }

        Console.WriteLine("{0}:", loggerDesc);

        BeginTest("No formatting");
        for (int i = 0; i < repeat; ++i)
        {
// BEGIN_UNROLL            
            // System.Diagnostics.Debugger.Break();
#if ENTLIB
            Logger.Write("This is a message without formatting.", category, PRIORITY_DEBUG, 1, TraceEventType.Verbose);
#else
            logger.Debug("This is a message without formatting.");
#endif
            // System.Diagnostics.Debugger.Break();
// END_UNROLL
        }
        EndTest();
        BeginTest("1 format parameter");
        for (int i = 0; i < repeat; ++i)
        {
// BEGIN_UNROLL            
#if LOG4NETWITHFORMAT || LOG4NET_WITH_FASTLOGGER
            logger.DebugFormat("This is a message with {0} format parameter", 1);
#elif LOG4NET
            logger.Debug(String.Format("This is a message with {0} format parameter", 1));
#elif ENTLIB
            Logger.Write(String.Format("This is a message with {0} format parameter", 1), category, PRIORITY_DEBUG, 1, TraceEventType.Verbose);
#else
            logger.Debug("This is a message with {0} format parameter", 1);
#endif
// END_UNROLL
        }
        EndTest();
        BeginTest("2 format parameters");
        for (int i = 0; i < repeat; ++i)
        {
// BEGIN_UNROLL            
#if LOG4NETWITHFORMAT || LOG4NET_WITH_FASTLOGGER
            logger.DebugFormat("This is a message with {0}{1} parameters", 2, "o");
#elif LOG4NET
            logger.Debug(String.Format("This is a message with {0}{1} parameters", 2, "o"));
#elif ENTLIB
            Logger.Write(String.Format("This is a message with {0}{1} format parameters", 2, "o"), category, PRIORITY_DEBUG, 1, TraceEventType.Verbose);
#else
            logger.Debug("This is a message with {0}{1} parameters", 2, "o");
#endif
// END_UNROLL
        }
        EndTest();
        BeginTest("3 format parameters");
        for (int i = 0; i < repeat; ++i)
        {
// BEGIN_UNROLL            
#if LOG4NETWITHFORMAT || LOG4NET_WITH_FASTLOGGER
            logger.DebugFormat("This is a message with {0}{1}{2} parameters", "thr", 3, 3);
#elif LOG4NET
            logger.Debug(String.Format("This is a message with {0}{1}{2} parameters", "thr", 3, 3));
#elif ENTLIB
            Logger.Write(String.Format("This is a message with {0}{1}{2} format parameters", "thr", 3, 3), category, PRIORITY_DEBUG);
#else
            logger.Debug("This is a message with {0}{1}{2} parameters", "thr", 3, 3);
#endif
// END_UNROLL
        }
        EndTest();
        BeginTest("No formatting, using a guard");
        for (int i = 0; i < repeat; ++i)
        {
// BEGIN_UNROLL            
#if ENTLIB
            entry = new LogEntry();
            entry.Message = "This is a message with no parameters.";
            entry.Categories.Add(category);
            entry.Priority = PRIORITY_DEBUG;
            entry.Severity = TraceEventType.Verbose;
            if (Logger.ShouldLog(entry))
            {
                Logger.Write(entry);
            }
#else
            if (logger.IsDebugEnabled)
            {
                logger.Debug("This is a message with no parameters");
            }
#endif
// END_UNROLL
        }
        EndTest();
        BeginTest("1 format parameter, using a guard");
        for (int i = 0; i < repeat; ++i)
        {
// BEGIN_UNROLL            
#if ENTLIB
            entry = new LogEntry();
            entry.Message = String.Format("This is a message with {0} parameters.", 1);
            entry.Categories.Add(category);
            entry.Priority = PRIORITY_DEBUG;
            entry.Severity = TraceEventType.Verbose;
            if (Logger.ShouldLog(entry))
            {
                Logger.Write(entry);
            }
#else
            if (logger.IsDebugEnabled)
            {
#if LOG4NETWITHFORMAT || LOG4NET_WITH_FASTLOGGER
                logger.DebugFormat("This is a message with {0} parameter", 1);
#elif LOG4NET
                logger.Debug(String.Format("This is a message with {0} parameter", 1));
#else
                logger.Debug("This is a message with {0} parameter", 1);
#endif
            }
#endif
// END_UNROLL
        }
        EndTest();
        BeginTest("2 format parameters, using a guard");
        for (int i = 0; i < repeat; ++i)
        {
// BEGIN_UNROLL
#if ENTLIB
            entry = new LogEntry();
            entry.Message = String.Format("This is a message with {0}{1} parameters.", 2, "o");
            entry.Categories.Add(category);
            entry.Priority = PRIORITY_DEBUG;
            entry.Severity = TraceEventType.Verbose;
            if (Logger.ShouldLog(entry))
            {
                Logger.Write(entry);
            }
#else
            if (logger.IsDebugEnabled)
            {
#if LOG4NETWITHFORMAT || LOG4NET_WITH_FASTLOGGER
                logger.DebugFormat("This is a message with {0}{1} parameters", 2, "o");
#elif LOG4NET
                logger.Debug(String.Format("This is a message with {0}{1} parameters", 2, "o"));
#else
                logger.Debug("This is a message with {0}{1} parameters", 2, "o");
#endif
            }
#endif
// END_UNROLL
        }
        EndTest();
        BeginTest("3 format parameters, using a guard");
        for (int i = 0; i < repeat; ++i)
        {
// BEGIN_UNROLL
#if ENTLIB
            entry = new LogEntry();
            entry.Message = String.Format("This is a message with {0}{1}{2} parameters.", "thr", 3, 3);
            entry.Categories.Add(category);
            entry.Priority = PRIORITY_DEBUG;
            entry.Severity = TraceEventType.Verbose;
            if (Logger.ShouldLog(entry))
            {
                Logger.Write(entry);
            }
#else
            if (logger.IsDebugEnabled)
            {
#if LOG4NETWITHFORMAT || LOG4NET_WITH_FASTLOGGER
                logger.DebugFormat("This is a message with {0}{1}{2} parameters", "thr", 3, 3);
#elif LOG4NET
                logger.Debug(String.Format("This is a message with {0}{1}{2} parameters", "thr", 3, 3));
#else
                logger.Debug("This is a message with {0}{1}{2} parameters", "thr", 3, 3);
#endif
            }
#endif
// END_UNROLL
        }
        EndTest();
        if (_output != null)
        {
            _output.WriteEndElement();
            _output.Flush();
        }
    }
}
