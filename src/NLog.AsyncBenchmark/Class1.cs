// 
// Copyright (c) 2004,2005 Jaroslaw Kowalski <jkowalski@users.sourceforge.net>
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
using System.IO;

using NLog;

namespace NLog.AsyncBenchmark
{
	class Timing
	{
        private Logger _logger;
        private int _repeatCount;
        private Thread _thread;
        private StopWatch _timeToWrite = new StopWatch();

        public Timing(Logger logger, int repeatCount)
        {
            _logger = logger;
            _repeatCount = repeatCount;

            _thread = new Thread(new ThreadStart(ThreadProc));
        }

		/// <summary>
		/// The main entry point for the application.
		/// </summary>
		[STAThread]
		static void Main(string[] args)
		{
            foreach (string s in Directory.GetFiles(".", "*.log"))
            {
                File.Delete(s);
            }
            TimeLogger(LogManager.GetLogger("async_file_none"));
            TimeLogger(LogManager.GetLogger("async_file_discard"));
            TimeLogger(LogManager.GetLogger("async_file_block"));
            TimeLogger(LogManager.GetLogger("async_file_none_10000"));
            TimeLogger(LogManager.GetLogger("async_file_discard_10000"));
            TimeLogger(LogManager.GetLogger("async_file_block_10000"));
            TimeLogger(LogManager.GetLogger("async_multifile_none"));
            TimeLogger(LogManager.GetLogger("async_multifile_discard"));
            TimeLogger(LogManager.GetLogger("async_multifile_block"));
            TimeLogger(LogManager.GetLogger("async_multifile_none_10000"));
            TimeLogger(LogManager.GetLogger("async_multifile_discard_10000"));
            TimeLogger(LogManager.GetLogger("async_multifile_block_10000"));

            TimeLogger(LogManager.GetLogger("sync_file"));
            TimeLogger(LogManager.GetLogger("sync_file_keepopen"));
            TimeLogger(LogManager.GetLogger("async_multifile"));
            TimeLogger(LogManager.GetLogger("sync_multifile"));
            TimeLogger(LogManager.GetLogger("sync_multifile_keepopen"));
            //TimeLogger(LogManager.GetLogger("async_network"));
            //TimeLogger(LogManager.GetLogger("sync_network"));
        }

        static void TimeLogger(Logger logger)
        {
            Console.WriteLine("Timing {0}", logger.Name);
            StopWatch _loggerTime = new StopWatch();
            _loggerTime.Start();

            using (StreamWriter output = new StreamWriter(logger.Name + ".csv", false, System.Text.Encoding.ASCII))
            {
                for (int repetitions = 1; repetitions <= 10000; repetitions *= 10)
                {
                    for (int threadCount = 1; threadCount <= 5; ++threadCount)
                    {
                        Console.WriteLine("threads: {0} repetitions: {1}", threadCount, repetitions);
                        System.GC.Collect();
                        Timing[] timings = new Timing[threadCount];
                        StopWatch _timeToFlush = new StopWatch();
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
                        long maxThreadTime = timings[0].ThreadTime;
                        long minThreadTime = timings[0].ThreadTime;
                        long sumThreadTime = timings[0].ThreadTime;

                        for (int i = 1; i < threadCount; ++i)
                        {
                            maxThreadTime = Math.Max(maxThreadTime, timings[i].ThreadTime);
                            minThreadTime = Math.Min(minThreadTime, timings[i].ThreadTime);
                            sumThreadTime += timings[i].ThreadTime;
                        }

                        maxThreadTime /= repetitions;
                        minThreadTime /= repetitions;
                        sumThreadTime /= repetitions;

                        long avgThreadTime = sumThreadTime / threadCount;

                        LogManager.Flush();
                        _timeToFlush.Stop();
                        output.WriteLine("{0};{1};{2};{3};{4};{5};{6}", logger.Name, threadCount, repetitions, _timeToFlush.Ticks, minThreadTime, maxThreadTime, avgThreadTime);
                        output.Flush();
                    }
                }
                output.Flush();
            }
            _loggerTime.Stop();
            Console.WriteLine("Time: {0}", _loggerTime.Seconds);
        }

        void ThreadProc()
        {
            _timeToWrite.Start();
            for (int i = 0; i < _repeatCount; ++i)
            {
                _logger.Debug("Message {0}", i);
            }
            _timeToWrite.Stop();
        }

        void Run()
        {
            _thread.Start();
        }

        void Join()
        {
            _thread.Join();
        }

        public long ThreadTime
        {
            get { return _timeToWrite.Ticks; }
        }
	}
}
