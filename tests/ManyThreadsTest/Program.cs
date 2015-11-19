using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using NLog;
using NLogPerfTest;

namespace ParallelTask
{
    class Program
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();
        private static int processed = 0;
        private static CpuUsage cpu = new CpuUsage();
        private static long logged;
        static void TestProducerConsumer(int threadCount, int period, int count = 10000)
        {
            Interlocked.Exchange(ref logged, 0);
            var tcs = period == 0 ? new CancellationTokenSource() : new CancellationTokenSource(TimeSpan.FromSeconds(period));
            var tf = new TaskFactory(tcs.Token, TaskCreationOptions.LongRunning, TaskContinuationOptions.None, null);
            var tasks = new List<Task>();
            var cpuUsageTotal = 0.0;
            var cpuUsageCount = 0;
            var sw = Stopwatch.StartNew();
            tasks.AddRange(Enumerable.Range(1, threadCount).Select(i => tf.StartNew(() =>
            {
                while (!tcs.IsCancellationRequested && processed < count)
                try
                {
                    Log4WithSleep(i, threadCount);
                    Interlocked.Increment(ref processed);
                }
                catch (OperationCanceledException ex)
                {
                }
            })));
            var t = new Timer(state =>
            {
                var cpuz = cpu.GetUsage();
                if (cpuz < 0) return;
                cpuUsageTotal += cpuz;
                cpuUsageCount++;
                Console.WriteLine("Tasks={0} Processed={1} CPU%={2:F}, CPUTime={3}, AvgCpu={4:F}, logged={5}", tasks.Count, processed,
                    cpu.GetUsage(),
                    System.Diagnostics.Process.GetCurrentProcess().TotalProcessorTime, cpuUsageTotal / cpuUsageCount, logged);
                Interlocked.Exchange(ref processed, 0);
            }, null, 0, 1000);
            if (period == 0)
            {
                Console.ReadLine();
                tcs.Cancel();
            }
            Task.WaitAll(tasks.ToArray());
            t.Dispose();
            logger.Info("Tasks={0} TotalCpu={1} countCpu={2} AvgCpu={3} logged rows={4}, time={5}", tasks.Count, cpuUsageTotal, cpuUsageCount, cpuUsageTotal / cpuUsageCount, logged, sw.Elapsed);
            Console.WriteLine("Finished");
        }

        private const int sleep = 80;
        /// <summary>
        /// Выполняет логирование N раз с имитацией обращения к внешней системе после каждого логирования
        /// </summary>
        /// <param name="index"></param>
        /// <param name="total"></param>
        static void Log4WithSleep(int index, int total)
        {
            for (int i = 0; i < 4; i++)
            {
                Interlocked.Increment(ref logged);
                logger.Info("TL_ID={0}", 0);
                Thread.Sleep(sleep);
            }
        }

        static void Main(string[] args)
        {
            int period = 0;
            var threadCount = 900;
            Console.WriteLine("TaskCount={0}", threadCount);
            TestProducerConsumer(threadCount, period, 10000);
        }
    }
}
