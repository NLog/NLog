//
// Copyright (c) 2004-2024 Jaroslaw Kowalski <jaak@jkowalski.net>, Kim Christensen, Julian Verdurmen
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

namespace NLog.Benchmarks
{
    using System;
    using System.IO;

    using BenchmarkDotNet.Attributes;

    using NLog.Targets;

    public class ColoredConsoleTargetBenchmark
    {
        private static readonly string TestMessage = "The cat sat at the bar.";
        private readonly StreamWriter _streamWriter = new StreamWriter(new MemoryStream());
        private readonly LogFactory _logFactory = new LogFactory();
        private Logger _logger;

        [GlobalSetup]
        public void Setup()
        {
            var target = new ColoredConsoleTarget(stdErr => _streamWriter)
            {
                Layout = "${logger} ${message}",
                WordHighlightingRules = {
                    new ConsoleWordHighlightingRule
                    {
                        ForegroundColor = ConsoleOutputColor.Red,
                        Text = "at",
                        IgnoreCase = true
                    }
                }
            };
            _logger = _logFactory.Setup().LoadConfiguration(cfg => cfg.ForLogger().WriteTo(target)).GetCurrentClassLogger();
        }

        [GlobalCleanup]
        public void Cleanup()
        {
            _logFactory.Dispose();
            _streamWriter.Dispose();
        }

        [Benchmark]
        public void WordHighlightingTextIgnoreCase()
        {
            _streamWriter.BaseStream.Position = 0;
            _streamWriter.BaseStream.SetLength(0); // Clear out the previous run
            for (int i = 0; i < 1000; ++i)
            {
                _logger.Debug(TestMessage);
            }
            _streamWriter.Flush();
        }
    }
}
