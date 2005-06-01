// 
// Copyright (c) 2004 Jaroslaw Kowalski <jkowalski@users.sourceforge.net>
// 
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
// * Neither the name of the Jaroslaw Kowalski nor the names of its 
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

#if LOG4NET
using log4net;
#else
using NLog;
#endif

#if LOG4NET

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

class StopWatch
{
    private long _startTime;
    private long _stopTime;
    private static long _overhead = 0;
    private static long _frequency;

    static StopWatch()
    {
        QueryPerformanceFrequency(out _frequency);
        StopWatch callibration = new StopWatch();
        long totalOverhead = 0;
        int loopCount = 0;
        for (int i = 0; i < 1000000; ++i)
        {
            callibration.Start();
            callibration.Stop();
            totalOverhead += callibration.Ticks;
            loopCount++;
        }
        _overhead = totalOverhead / loopCount;
        Console.WriteLine("Callibrating StopWatch: overhead {0}", _overhead);
    }
    
    public void Start()
    {
        QueryPerformanceCounter(out _startTime);
    }

    public void Stop()
    {
        QueryPerformanceCounter(out _stopTime);
    }

    public long Ticks
    {
        get { return _stopTime - _startTime - _overhead; }
    }

    public double Seconds
    {
        get { return (double)(_stopTime - _startTime - _overhead) / _frequency; }
    }

    public double Nanoseconds
    {
        get { return (double)1000000000 * (_stopTime - _startTime - _overhead) / _frequency; }
    }

    [DllImport("kernel32.dll")]
    static extern bool QueryPerformanceCounter(out long val);

    [DllImport("kernel32.dll")]
    static extern bool QueryPerformanceFrequency(out long val);
}

class Bench
{
    private const int warmup = 10;
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
        _stopWatch.Start();
    }

    public static void EndTest()
    {
        _stopWatch.Stop();
        if (_output != null)
        {
            _output.WriteAttributeString("totalTime", Convert.ToString(TimeSpan.FromSeconds(_stopWatch.Seconds)));
            _output.WriteAttributeString("repetitions", Convert.ToString(_repeat));
            _output.WriteAttributeString("logsPerSecond", Convert.ToString(_repeat /_stopWatch.Seconds));
            _output.WriteAttributeString("nanosecondsPerLog", Convert.ToString(_stopWatch.Nanoseconds / _repeat));
            _output.WriteEndElement();
        }
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
#else
        Logger logger1 = LogManager.GetLogger("nonlogger");
        Logger logger2 = LogManager.GetLogger("null1");
        Logger logger3 = LogManager.GetLogger("null2");
        Logger logger4 = LogManager.GetLogger("file1");
#endif
        // _output = new XmlTextWriter(Console.Out);
        for (int i = 0; i < warmup; ++i)
        {
            logger1.Debug("Warm up");
            logger2.Debug("Warm up");
            logger3.Debug("Warm up");
            logger1.Info("Warm up");
            logger2.Info("Warm up");
            logger3.Info("Warm up");
        }

        // warm up
        LogTest(logger1, null, 5);
        //LogTest(logger4, null, 5);
        LogTest(logger3, null, 5);
        LogTest(logger2, null, 5);

        _output = new XmlTextWriter(outputFile, System.Text.Encoding.UTF8);
        _output.Formatting = Formatting.Indented;
        _output.WriteStartElement("results");
        LogTest(logger1, "Non-logging", repeat);
        //LogTest(logger4, "File appender", repeat2);
        LogTest(logger3, "Null-appender without layout", repeat);
        LogTest(logger2, "Null-appender with layout rendering", repeat);

        _output.WriteEndElement();
        _output.Close();
    }

    private static void LogTest(
#if LOG4NET
            ILog logger, 
#else
            Logger logger, 
#endif
            string loggerDesc, int repeat)
    {
        _repeat = repeat;

        if (_output != null)
        {
            _output.WriteStartElement("test");
            _output.WriteAttributeString("logger", loggerDesc);
            _output.WriteAttributeString("repetitions", repeat.ToString());
        }

        BeginTest("No formatting");
        for (int i = 0; i < repeat; ++i)
        {
            // System.Diagnostics.Debugger.Break();
            logger.Debug("This is a message without formatting.");
            // System.Diagnostics.Debugger.Break();
        }
        EndTest();
        BeginTest("1 format parameter");
        for (int i = 0; i < repeat; ++i)
        {
#if LOG4NET
#if LOG4NETWITHFORMAT
            logger.DebugFormat("This is a message with {0} format parameter", 1);
#else
            logger.Debug(String.Format("This is a message with {0} format parameter", 1));
#endif
#else
            logger.Debug("This is a message with {0} format parameter", 1);
#endif
        }
        EndTest();
        BeginTest("2 format parameters");
        for (int i = 0; i < repeat; ++i)
        {
#if LOG4NET

#if LOG4NETWITHFORMAT
            logger.DebugFormat("This is a message with {0}{1} parameters", 2, "o");
#else            
            logger.Debug(String.Format("This is a message with {0}{1} parameters", 2, "o"));
#endif

#else
            logger.Debug("This is a message with {0}{1} parameters", 2, "o");
#endif
        }
        EndTest();
        BeginTest("3 format parameters");
        for (int i = 0; i < repeat; ++i)
        {
#if LOG4NET

#if LOG4NETWITHFORMAT
            logger.DebugFormat("This is a  {0}{1}{2} parameters", "thr", 3, 3);
#else
            logger.Debug(String.Format("This is a  {0}{1}{2} parameters", "thr", 3, 3));
#endif
            
#else
            logger.Debug("This is a  {0}{1}{2} parameters", "thr", 3, 3);
#endif
        }
        EndTest();
        BeginTest("No formatting, using a guard");
        for (int i = 0; i < repeat; ++i)
        {
            if (logger.IsDebugEnabled)
            {
                logger.Debug("This is a  no parameters");
            }
        }
        EndTest();
        BeginTest("1 format parameter, using a guard");
        for (int i = 0; i < repeat; ++i)
        {
            if (logger.IsDebugEnabled)
            {
#if LOG4NET

#if LOG4NETWITHFORMAT
                logger.DebugFormat("This is a  {0} parameter", 1);
#else
                logger.Debug(String.Format("This is a  {0} parameter", 1));
#endif
                
#else
                logger.Debug("This is a  {0} parameter", 1);
#endif
            }
        }
        EndTest();
        BeginTest("2 format parameters, using a guard");
        for (int i = 0; i < repeat; ++i)
        {
            if (logger.IsDebugEnabled)
            {
#if LOG4NET
#if LOG4NETWITHFORMAT
                logger.DebugFormat("This is a  {0}{1} parameters", 2, "o");
#else
                logger.Debug(String.Format("This is a  {0}{1} parameters", 2, "o"));
#endif
                
#else
                logger.Debug("This is a  {0}{1} parameters", 2, "o");
#endif
            }
        }
        EndTest();
        BeginTest("3 format parameters, using a guard");
        for (int i = 0; i < repeat; ++i)
        {
            if (logger.IsDebugEnabled)
            {
#if LOG4NET
#if LOG4NETWITHFORMAT
                logger.DebugFormat("This is a  {0}{1}{2} parameters", "thr", 3, 3);
#else
                logger.Debug(String.Format("This is a  {0}{1}{2} parameters", "thr", 3, 3));
#endif
#else
                logger.Debug("This is a  {0}{1}{2} parameters", "thr", 3, 3);
#endif
            }
        }
        EndTest();
        if (_output != null)
        {
            _output.WriteEndElement();
        }
    }
}
