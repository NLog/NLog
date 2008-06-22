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
using System.IO;
using System.Diagnostics;
using System.Reflection;

using NLog;
using NLog.Targets;

namespace NLog.AsyncBenchmark
{
	class Timing
	{
        private Logger _logger;
        private int _repeatCount;
        private Thread _thread;
        private StopWatch _timeToWrite = new StopWatch();
        private static Mutex _syncStartMutex = new Mutex(false, "NLOG-SYNCSTART");
        private Exception _exception;

        public Timing(Logger logger, int repeatCount)
        {
            _logger = logger;
            _repeatCount = repeatCount;
            _exception = null;

            _thread = new Thread(new ThreadStart(ThreadProc));
        }

		/// <summary>
		/// The main entry point for the application.
		/// </summary>
		[STAThread]
		static int Main(string[] args)
		{
            LogManager.ThrowExceptions = true;
            if (args.Length > 0 && args[0] == "/child")
            {
                Console.Error.WriteLine("AAA");
                int repetitions = Convert.ToInt32(args[1]);
                int threads = Convert.ToInt32(args[2]);
                string targetName = args[3];

                Console.Error.WriteLine("Child #{0} running {1} threads, {2} repetitions on {3}", System.Threading.Thread.CurrentThread.ManagedThreadId, threads, repetitions, targetName);
                Target t = LogManager.Configuration.FindTargetByName(targetName);

                double minTime, maxTime, avgTime, timeToFlush;
                bool gotException;

                // warmup
                Console.Error.WriteLine("Warming up...");
                TimeTarget(t, 1, 1, out minTime, out maxTime, out avgTime, out timeToFlush, out gotException);

                // wait on mutex and release immediately
                // the processes are started with mutex held
                // so they all wait here.

                Console.Error.WriteLine("Ready and waiting...");
                _syncStartMutex.WaitOne();
                _syncStartMutex.ReleaseMutex();
                Console.Error.WriteLine("Go!");

                TimeTarget(t, threads, repetitions, out minTime, out maxTime, out avgTime, out timeToFlush, out gotException);
                Console.WriteLine("{0} {1} {2} {3} {4}", minTime, maxTime, avgTime, timeToFlush, gotException ? "1" : "0");
                Console.Error.WriteLine("Child #{0} finished.", System.Threading.Thread.CurrentThread.ManagedThreadId);
                return 0;
            }

            Internal.InternalLogger.LogLevel = LogLevel.Warn;
            Internal.InternalLogger.LogToConsoleError = true;

            foreach (string s in Directory.GetFiles(".", "*.log"))
            {
                File.Delete(s);
            }

            NLog.Config.LoggingConfiguration defaultConfig = LogManager.Configuration;

            TextWriter output = new StreamWriter("results.csv", false, System.Text.Encoding.ASCII);
            string separator = Thread.CurrentThread.CurrentCulture.TextInfo.ListSeparator;
            output.Write("targetName");
            output.Write(separator);
            output.Write("processCount");
            output.Write(separator);
            output.Write("threadCount");
            output.Write(separator);
            output.Write("repetitions");
            output.Write(separator);
            output.Write("avgTime");
            output.Write(separator);
            output.Write("ttf");
            output.Write(separator);
            output.Write("gotException");
            output.WriteLine();
            output.Flush();
            foreach (Target t in defaultConfig.GetConfiguredNamedTargets())
            {
                TimeTarget(output, t);
            }
            output.Close();
            return 0;
        }

        static void TimeTarget(TextWriter output, Target target)
        {
            NLog.Config.SimpleConfigurator.ConfigureForTargetLogging(target, LogLevel.Debug);

            string separator = Thread.CurrentThread.CurrentCulture.TextInfo.ListSeparator;
            for (int processCount = 1; processCount <= 4; processCount *= 2)
            {
                for (int repetitions = 1000; repetitions <= 1000; repetitions *= 10)
                {
                    for (int threadCount = 1; threadCount <= 1; threadCount *= 4)
                    {
                        Console.WriteLine("Timing '{0}' in {1} processes", target.Name, processCount);
                        Process[] processes = new Process[processCount];
                        for (int i = 0; i < processCount; ++i)
                        {
                            Process p = new Process();
                            p.StartInfo.FileName = Assembly.GetEntryAssembly().Location;
                            p.StartInfo.Arguments = String.Format("/child {0} {1} {2}", repetitions, threadCount, target.Name);
                            p.StartInfo.UseShellExecute = false;
                            p.StartInfo.RedirectStandardOutput = true;
                            //Console.WriteLine("{0}", p.StartInfo.FileName);
                            processes[i] = p;
                        }

                        _syncStartMutex.WaitOne();
                        for (int i = 0; i < processCount; ++i)
                        {
                            processes[i].Start();
                        }
                        Console.WriteLine("Ready, steady...");
                        System.Threading.Thread.Sleep(10000);
                        Console.WriteLine("Go!!!!!");
                        _syncStartMutex.ReleaseMutex();

                        double min = Double.MaxValue;
                        double max = Double.MinValue;
                        double sum = 0.0;
                        double ttf = Double.MinValue;
                        bool gotException = false;

                        for (int i = 0; i < processCount; ++i)
                        {
                            processes[i].WaitForExit();
                            string result = processes[i].StandardOutput.ReadToEnd();
                            processes[i].Dispose();

                            string[] results = result.Split(' ');
                            min = Math.Min(min, Convert.ToDouble(results[0]));
                            max = Math.Max(max, Convert.ToDouble(results[1]));
                            sum += Convert.ToDouble(results[2]);
                            ttf = Math.Max(ttf, Convert.ToDouble(results[3]));
                            if (Convert.ToInt32(results[4]) != 0)
                            {
                                gotException = true;
                            }
                        }

                        double avg = sum / processCount; 

                        output.Write(target.Name);
                        output.Write(separator);
                        output.Write(processCount);
                        output.Write(separator);
                        output.Write(threadCount);
                        output.Write(separator);
                        output.Write(repetitions);
                        output.Write(separator);
                        output.Write(Math.Round(avg * 1000.0, 3));
                        output.Write(separator);
                        output.Write(Math.Round(ttf * 1000.0, 3));
                        output.Write(separator);
                        output.Write(gotException);
                        output.WriteLine();
                        output.Flush();
                    }
                }
            }
        }

        static void TimeTarget(Target target, int threadCount, int repetitions, out double minTime, out double maxTime, out double avgTime, out double timeToFlush, out bool gotException)
        {
            NLog.Config.SimpleConfigurator.ConfigureForTargetLogging(target, LogLevel.Debug);

            Timing[] timings = new Timing[threadCount];
            StopWatch _timeToFlush = new StopWatch();
            Logger logger = LogManager.GetLogger("A");
            for (int i = 0; i < threadCount; ++i)
            {
                Timing t = new Timing(logger, repetitions);
                timings[i] = t;
            }
            _timeToFlush.Start();
            for (int i = 0; i < threadCount; ++i)
            {
                timings[i].Run();
            }
            for (int i = 0; i < threadCount; ++i)
            {
                timings[i].Join();
            }

            double maxThreadTime = timings[0].ThreadTime;
            double minThreadTime = timings[0].ThreadTime;
            double sumThreadTime = timings[0].ThreadTime;

            for (int i = 1; i < threadCount; ++i)
            {
                maxThreadTime = Math.Max(maxThreadTime, timings[i].ThreadTime);
                minThreadTime = Math.Min(minThreadTime, timings[i].ThreadTime);
                sumThreadTime += timings[i].ThreadTime;
            }

            gotException = false;
            for (int i = 0; i < threadCount; ++i)
            {
                if (timings[i]._exception != null)
                {
                    gotException = true;
                }
            }

            maxThreadTime /= repetitions;
            minThreadTime /= repetitions;
            sumThreadTime /= repetitions;

            double avgThreadTime = sumThreadTime / threadCount;

            LogManager.Flush();
            _timeToFlush.Stop();

            maxTime = maxThreadTime;
            minTime = minThreadTime;
            avgTime = avgThreadTime;
            timeToFlush = _timeToFlush.Seconds;
        }

        public void ThreadProc()
        {
            _timeToWrite.Start();
            try
            {
                for (int i = 0; i < 10; ++i)
                {
                    _logger.Debug("Warmup {0}", i);
                }

                for (int i = 0; i < _repeatCount; ++i)
                {
                    _logger.Debug("Message {0}", i);
                }
            }
            catch (Exception ex)
            {
                _exception = ex;
            }
            finally
            {
                _timeToWrite.Stop();
            }
        }

        void Run()
        {
            _thread.Start();
        }

        void Join()
        {
            _thread.Join();
        }

        public double ThreadTime
        {
            get { return _timeToWrite.Seconds; }
        }
	}
}
