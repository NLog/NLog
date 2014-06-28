using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NLog.Fluent;
using Xunit;

namespace NLog.UnitTests.Fluent
{
    public class LogBuilderTests
    {
        private static readonly NLog.Logger _logger = NLog.LogManager.GetCurrentClassLogger();

        [Fact]
        public void TraceWrite()
        {
            _logger.Trace()
                .Message("This is a test fluent message.")
                .Property("Test", "TraceWrite")
                .Write();

            _logger.Trace()
                .Message("This is a test fluent message '{0}'.", DateTime.Now.Ticks)
                .Property("Test", "TraceWrite")
                .Write();
        }

        [Fact]
        public void TraceIfWrite()
        {
            _logger.Trace()
                .Message("This is a test fluent message.")
                .Property("Test", "TraceWrite")
                .Write();

            int v = 1;
            _logger.Trace()
                .Message("This is a test fluent WriteIf message '{0}'.", DateTime.Now.Ticks)
                .Property("Test", "TraceWrite")
                .WriteIf(() => v == 1);

            _logger.Trace()
                .Message("This is a test fluent WriteIf message '{0}'.", DateTime.Now.Ticks)
                .Property("Test", "TraceWrite")
                .WriteIf(v == 1);

            _logger.Trace()
                .Message("Should Not WriteIf message '{0}'.", DateTime.Now.Ticks)
                .Property("Test", "TraceWrite")
                .WriteIf(v > 1);

        }

        [Fact]
        public void InfoWrite()
        {
            _logger.Info()
                .Message("This is a test fluent message.")
                .Property("Test", "InfoWrite")
                .Write();

            _logger.Info()
                .Message("This is a test fluent message '{0}'.", DateTime.Now.Ticks)
                .Property("Test", "InfoWrite")
                .Write();
        }

        [Fact]
        public void ErrorWrite()
        {
            string path = "blah.txt";
            try
            {
                string text = File.ReadAllText(path);
            }
            catch (Exception ex)
            {
                _logger.Error()
                    .Message("Error reading file '{0}'.", path)
                    .Exception(ex)
                    .Property("Test", "ErrorWrite")
                    .Write();
            }

            _logger.Error()
                .Message("This is a test fluent message.")
                .Property("Test", "ErrorWrite")
                .Write();

            _logger.Error()
                .Message("This is a test fluent message '{0}'.", DateTime.Now.Ticks)
                .Property("Test", "ErrorWrite")
                .Write();
        }

    }
}
