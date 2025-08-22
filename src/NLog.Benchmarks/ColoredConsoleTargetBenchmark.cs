using System;
using System.IO;

using BenchmarkDotNet.Attributes;

using NLog.Config;
using NLog.Targets;

namespace NLog.Benchmarks
{
    public class ColoredConsoleTargetBenchmark
    {
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();
        private static readonly string message = "The cat sat at the bar.";
        private MemoryStream memoryStream = new MemoryStream();
        private StreamWriter streamWriter;
        private TextWriter oldConsoleOutWriter;

        [GlobalSetup]
        public void Setup()
        {
            var config = new LoggingConfiguration();
            var target = new ColoredConsoleTarget("ColoredConsole")
                { Layout = "${logger} ${message}" };
            target.WordHighlightingRules.Add(
                new ConsoleWordHighlightingRule
                {
                    ForegroundColor = ConsoleOutputColor.Red,
                    Text = "at",
                    IgnoreCase = true
                });

            config.AddTarget(target);
            config.AddRule(LogLevel.Debug, LogLevel.Fatal, target);

            LogManager.Configuration = config;

            streamWriter = new StreamWriter(memoryStream);
            oldConsoleOutWriter = Console.Out;
            Console.SetOut(streamWriter);
        }

        [GlobalCleanup]
        public void Cleanup()
        {
            Console.SetOut(oldConsoleOutWriter);
            streamWriter.Dispose();
            memoryStream.Dispose();
        }

        [Benchmark]
        public void WordHighlightingTextIgnoreCase()
        {
            memoryStream.SetLength(0); // Clear out the previous run
            for (int i = 0; i < 1000; ++i)
            {
                logger.Debug(message);
            }
        }
    }
}
