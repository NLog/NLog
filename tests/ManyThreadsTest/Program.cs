using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using NLog;
using NLogPerfTest;

namespace ParallelTask
{
    public enum Status
    {
        St1 = 0,
        St2 = 2,
        St3 = 3
    }

    class Program
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();
        private static List<Logger> loggers;

        private static BlockingCollection<int> col;
        private const int maxSize = 8000;
        private const int batchSize = maxSize / 3;
        private static int processed = 0;
        private static CpuUsage cpu = new CpuUsage();
        private static long logged;
        static void TestProducerConsumer(int threadCount, Action<int, int> collectionConsumerAction, int period, int count)
        {
            Interlocked.Exchange(ref logged, 0);
            col = new BlockingCollection<int>();
            var tcs = period == 0 ? new CancellationTokenSource() : new CancellationTokenSource(TimeSpan.FromSeconds(period));
            var tf = new TaskFactory(tcs.Token, TaskCreationOptions.LongRunning, TaskContinuationOptions.None, null);
            var tasks = new List<Task>();
            var cpuUsageTotal = 0.0;
            var cpuUsageCount = 0;
            var sw = Stopwatch.StartNew();
            tasks.Add(tf.StartNew(() =>
            {
                if (count > 0)
                {
                    for (int i = 0; i <= count; i++)
                        col.TryAdd(i);
                    col.CompleteAdding();
                }
                else
                    while (!col.IsAddingCompleted)
                        if (col.Count > maxSize - batchSize)
                        {
                            if (tf.CancellationToken.WaitHandle.WaitOne(1000))
                                break;
                        }
                        else
                            for (int i = 0; i < batchSize; i++)
                                try
                                {
                                    col.TryAdd(i);
                                }
                                catch (InvalidOperationException)
                                {
                                    break;
                                }
            }));
            tasks.AddRange(Enumerable.Range(1, threadCount).Select(i => tf.StartNew(() =>
            {
                try
                {
                    foreach (var о in col.GetConsumingEnumerable(tf.CancellationToken))
                    {
                        collectionConsumerAction(i, threadCount);
                        Interlocked.Increment(ref processed);
                    }
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
                Console.WriteLine("Tasks={0} Processed={1} CPU%={2:F}, CPUTime={3}, QueueLength={4}, AvgCpu={5:F}, logged={6}", tasks.Count, processed,
                    cpu.GetUsage(),
                    System.Diagnostics.Process.GetCurrentProcess().TotalProcessorTime, col.Count, cpuUsageTotal / cpuUsageCount, logged);
                Interlocked.Exchange(ref processed, 0);
            }, null, 0, 1000);
            if (period == 0)
            {
                Console.ReadLine();
                tcs.Cancel();
                col.CompleteAdding();
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
                LogFormattedString();
                Thread.Sleep(sleep);
            }
        }

        /// <summary>
        /// Выполняет логирование N раз с имитацией обращения к внешней системе после каждого логирования
        /// </summary>
        /// <param name="index"></param>
        /// <param name="total"></param>
        static void Log4WithSleepNdc(int index, int total)
        {
            using (NestedDiagnosticsContext.Push("NDC" + index))
                for (int i = 0; i < 4; i++)
                {
                    LogFormattedStringLess();
                    Thread.Sleep(sleep);
                }
        }

        /// <summary>
        /// Выполняет логирование N раз с имитацией обращения к внешней системе после каждого логирования
        /// </summary>
        /// <param name="index"></param>
        /// <param name="total"></param>
        static void Log4WithSleepMdc(int index, int total)
        {
            MappedDiagnosticsContext.Set("tl", "MDC" + index);
            for (int i = 0; i < 4; i++)
            {
                LogFormattedStringLess();
                Thread.Sleep(sleep);
            }
        }

        public class Payment
        {
            public long Id { get; set; }
            public int GoodsId { get; set; }
            public int SourceOfMoneyId { get; set; }
            public int StoreId { get; set; }
            public int Channel { get; set; }
            public long? UsrId { get; set; }
            public DateTime Date { get; set; }
            public string ServiceNumber { get; set; }
            public string MoreInfoSm { get; set; }
            public string MoreInfoSt { get; set; }
            public string IdSm { get; set; }
            public string IdSt { get; set; }
            public string IdMerchant { get; set; }
            public decimal Price { get; set; }
            public decimal Amount { get; set; }
            public string SourceTxt { get; set; }
            public string Info { get; set; }
            public string Extension { get; set; }
            public string AocResult { get; set; }
            public int ErrorCode { get; set; }

            public string Url { get; set; }
            public string PublicKey { get; set; }
            public string PublicKeySerial { get; set; }
            public string XmlViewParam { get; set; }


            /// <summary>
            /// Дополнительные параметры платежа.
            /// </summary>
            public string Params { get; set; }

            /// <summary>
            /// Шаблон для трансформации дополнительных паратетров платежа.
            /// </summary>
            public string ParamsXslt { get; set; }

            /// <summary>
            /// Строка с дополнительными параметрами, специфичными для протокола.
            /// </summary>
            public string ProtocolParams { get; set; }
        }

        private static Payment payment = new Payment()
        {
            IdSt = "3487864",
            IdSm = "bmbsdfhkshf",
            MoreInfoSm = "3745754382",
            SourceTxt = "SourceTxt"
        };

        static void LogFormattedString()
        {
            Interlocked.Increment(ref logged);
            var transformedParams = 5;
            var action = "string";
            logger.Info("TL_ID={0} action={1} date={2} moreInfoSt={3} moreInfoSm={4} idSt={5} idSm={6} price={7} amount={8} sourceTxt={9} serviceNumber={10} sourceOfMoneyId={11} goodsId={12} {13}",
                payment.Id, action, payment.Date.ToString("yyyy-MM-ddTHH:mm:ss.fffzzzz"), payment.MoreInfoSt ?? string.Empty, payment.MoreInfoSm ?? string.Empty,
                payment.IdSt ?? string.Empty, payment.IdSm ?? string.Empty, payment.Price, payment.Amount, payment.SourceTxt ?? string.Empty,
                payment.ServiceNumber ?? string.Empty, payment.SourceOfMoneyId, payment.GoodsId, transformedParams);
        }

        static void LogFormattedStringLess()
        {
            Interlocked.Increment(ref logged);
            var transformedParams = 5;
            var action = "строка";
            logger.Info("TL_ID={0} action={1} date={2} moreInfoSt={3} moreInfoSm={4} idSt={5} idSm={6} price={7} amount={8} sourceTxt={9} serviceNumber={10} sourceOfMoneyId={11} goodsId={12}",
                transformedParams, action, payment.Date.ToString("yyyy-MM-ddTHH:mm:ss.fffzzzz"), payment.MoreInfoSt ?? string.Empty, payment.MoreInfoSm ?? string.Empty,
                payment.IdSt ?? string.Empty, payment.IdSm ?? string.Empty, payment.Price, payment.Amount, payment.SourceTxt ?? string.Empty,
                payment.ServiceNumber ?? string.Empty, payment.SourceOfMoneyId, payment.GoodsId);
        }

        static void Main(string[] args)
        {
            int period = 0;
            var rowsPerSec = 1800;
            var method = 1;
            const int count = 100000;
            var threadCount = args.Length>0?100:900;
            Console.WriteLine("TaskCount={0} Method={1}", threadCount, method);
            Action<int, int> action = Log4WithSleep;
            switch (method)
            {
                case 1: action = Log4WithSleep; break;
                case 2: action = Log4WithSleepMdc; break;
                case 3: action = Log4WithSleepNdc; break;
            }
            TestProducerConsumer(threadCount, action, period, count);
        }
    }
}
