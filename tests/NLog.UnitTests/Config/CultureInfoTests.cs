// 
// Copyright (c) 2004-2016 Jaroslaw Kowalski <jaak@jkowalski.net>, Kim Christensen, Julian Verdurmen
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

using NLog.LayoutRenderers;
using NLog.Targets;
#if !SILVERLIGHT
using Xunit.Extensions;
#endif

namespace NLog.UnitTests.Config
{
    using System;
    using System.Globalization;
    using System.Threading;
    using Xunit;

    using NLog.Config;

    public class CultureInfoTests : NLogTestBase
    {
        [Fact]
        public void WhenInvariantCultureDefinedThenDefaultCultureIsInvariantCulture()
        {
            var configuration = CreateConfigurationFromString("<nlog useInvariantCulture='true'></nlog>");

            Assert.Equal(CultureInfo.InvariantCulture, configuration.DefaultCultureInfo);
        }

        [Fact]
        public void DifferentConfigurations_UseDifferentDefaultCulture()
        {
            var currentCulture = CultureInfo.CurrentCulture;
            try
            {
                // set the current thread culture to be definitely different from the InvariantCulture
                Thread.CurrentThread.CurrentCulture = new CultureInfo("de-DE");

                var configurationTemplate = @"<nlog useInvariantCulture='{0}'>
<targets>
    <target name='debug' type='Debug' layout='${{message}}' />
</targets>
<rules>
    <logger name='*' writeTo='debug'/>
</rules>
</nlog>";


                // configuration with current culture
                var configuration1 = CreateConfigurationFromString(string.Format(configurationTemplate, false));
                Assert.Equal(null, configuration1.DefaultCultureInfo);

                // configuration with invariant culture
                var configuration2 = CreateConfigurationFromString(string.Format(configurationTemplate, true));
                Assert.Equal(CultureInfo.InvariantCulture, configuration2.DefaultCultureInfo);

                Assert.NotEqual(configuration1.DefaultCultureInfo, configuration2.DefaultCultureInfo);

                var testNumber = 3.14;
                var testDate = DateTime.Now;
                const string formatString = "{0},{1:d}";

                AssertMessageFormattedWithCulture(configuration1, CultureInfo.CurrentCulture, formatString, testNumber, testDate);
                AssertMessageFormattedWithCulture(configuration2, CultureInfo.InvariantCulture, formatString, testNumber, testDate);

            }
            finally
            {
                // restore current thread culture
                Thread.CurrentThread.CurrentCulture = currentCulture;
            }
        }

        private void AssertMessageFormattedWithCulture(LoggingConfiguration configuration, CultureInfo culture, string formatString, params object[] parameters)
        {
            var expected = string.Format(culture, formatString, parameters);
            using (var logFactory = new LogFactory(configuration))
            {
                var logger = logFactory.GetLogger("test");
                logger.Debug(formatString, parameters);
                Assert.Equal(expected, GetDebugLastMessage("debug", configuration));
            }
        }
#if !SILVERLIGHT

        [Fact]
        public void EventPropRendererCultureTest()
        {
            string cultureName = "de-DE";
            string expected = "1,23";   // with decimal comma

            var logEventInfo = CreateLogEventInfo(cultureName);
            logEventInfo.Properties["ADouble"] = 1.23;

            var renderer = new EventPropertiesLayoutRenderer();
            renderer.Item = "ADouble";
            string output = renderer.Render(logEventInfo);

            Assert.Equal(expected, output);
        }

        [Fact]
        public void EventContextRendererCultureTest()
        {
            string cultureName = "de-DE";
            string expected = "1,23";   // with decimal comma

            var logEventInfo = CreateLogEventInfo(cultureName);
            logEventInfo.Properties["ADouble"] = 1.23;

#pragma warning disable 618
            var renderer = new EventContextLayoutRenderer();
#pragma warning restore 618
            renderer.Item = "ADouble";
            string output = renderer.Render(logEventInfo);

            Assert.Equal(expected, output);
        }


#if !MONO
        [Fact(Skip = "TimeSpan tostring isn't culture aware in .NET?")]
        public void ProcessInfoLayoutRendererCultureTest()
        {
            string cultureName = "de-DE";
            string expected = ",";   // decimal comma as separator for ticks

            var logEventInfo = CreateLogEventInfo(cultureName);

            var renderer = new ProcessInfoLayoutRenderer();
            renderer.Property = ProcessInfoProperty.TotalProcessorTime;
            string output = renderer.Render(logEventInfo);

            Assert.Contains(expected, output);
            Assert.DoesNotContain(".", output);
        }
#endif


        [Fact]
        public void AllEventPropRendererCultureTest()
        {
            string cultureName = "de-DE";
            string expected = "ADouble=1,23";   // with decimal comma

            var logEventInfo = CreateLogEventInfo(cultureName);
            logEventInfo.Properties["ADouble"] = 1.23;

            var renderer = new AllEventPropertiesLayoutRenderer();
            string output = renderer.Render(logEventInfo);

            Assert.Equal(expected, output);
        }

        [Theory]
        [InlineData(typeof(TimeLayoutRenderer))]
        [InlineData(typeof(ProcessTimeLayoutRenderer))]
        public void DateTimeCultureTest(Type rendererType)
        {
            string cultureName = "de-DE";
            string expected = ",";   // decimal comma as separator for ticks

            var logEventInfo = CreateLogEventInfo(cultureName);

            var renderer = Activator.CreateInstance(rendererType) as LayoutRenderer;
            Assert.NotNull(renderer);
            string output = renderer.Render(logEventInfo);

            Assert.Contains(expected, output);
            Assert.DoesNotContain(".", output);
        }

        private static LogEventInfo CreateLogEventInfo(string cultureName)
        {
            var logEventInfo = new LogEventInfo(
                LogLevel.Info,
                "SomeName",
                CultureInfo.GetCultureInfo(cultureName),
                "SomeMessage",
                null);
            return logEventInfo;
        }

        /// <summary>
        /// expected: exactly the same exception message + stack trace regardless of the CurrentUICulture
        /// </summary>
        [Fact]
        public void ExceptionTest()
        {
            var target = new MemoryTarget { Layout = @"${exception:format=tostring}" };
            SimpleConfigurator.ConfigureForTargetLogging(target);
            var logger = LogManager.GetCurrentClassLogger();

            try
            {
                throw new InvalidOperationException();
            }
            catch (Exception ex)
            {
                Thread.CurrentThread.CurrentUICulture = new CultureInfo("en-US");
                Thread.CurrentThread.CurrentCulture = new CultureInfo("en-US");
                logger.Error(ex, "");

                Thread.CurrentThread.CurrentUICulture = new CultureInfo("de-DE");
                Thread.CurrentThread.CurrentCulture = new CultureInfo("de-DE");
                logger.Error(ex, "");

                Assert.Equal(2, target.Logs.Count);
                Assert.NotNull(target.Logs[0]);
                Assert.NotNull(target.Logs[1]);
                Assert.Equal(target.Logs[0], target.Logs[1]);
            }
        }
#endif
    }
}