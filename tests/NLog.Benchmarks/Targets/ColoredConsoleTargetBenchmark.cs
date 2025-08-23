namespace NLog.Benchmarks
{
    using System;
    using System.IO;

    using BenchmarkDotNet.Attributes;

    using NLog.Config;
    using NLog.Targets;

    public class ColoredConsoleTargetBenchmark
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        private static readonly string TestMessage = "The cat sat at the bar.";
        private readonly MemoryStream _memoryStream = new MemoryStream();
        private StreamWriter _streamWriter;
        private TextWriter _oldConsoleOutWriter;

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

            _streamWriter = new StreamWriter(_memoryStream);
            _oldConsoleOutWriter = Console.Out;
            Console.SetOut(_streamWriter);
        }

        [GlobalCleanup]
        public void Cleanup()
        {
            Console.SetOut(_oldConsoleOutWriter);
            _streamWriter.Dispose();
            _memoryStream.Dispose();
        }

        [Benchmark]
        public void WordHighlightingTextIgnoreCase()
        {
            _memoryStream.SetLength(0); // Clear out the previous run
            for (int i = 0; i < 1000; ++i)
            {
                Logger.Debug(TestMessage);
            }
        }
    }
}
