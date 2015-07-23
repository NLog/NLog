// 
// Copyright (c) 2004-2011 Jaroslaw Kowalski <jaak@jkowalski.net>
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

using System.IO;
using System.Linq;
using System.Threading;
using NLog.Targets;

#if !SILVERLIGHT
namespace NLog.UnitTests
{
    using System;
    using NLog.Config;
    using Xunit;

    public class LogFactoryTests : NLogTestBase
    {
        [Fact]
        public void Flush_DoNotThrowExceptionsAndTimeout_DoesNotThrow()
        {
            LogManager.Configuration = CreateConfigurationFromString(@"
            <nlog throwExceptions='false'>
                <targets><target type='MethodCall' name='test' methodName='Throws' className='NLog.UnitTests.LogFactoryTests, NLog.UnitTests.netfx40' /></targets>
                <rules>
                    <logger name='*' minlevel='Debug' writeto='test'></logger>
                </rules>
            </nlog>");

            ILogger logger = LogManager.GetCurrentClassLogger();
            logger.Factory.Flush(_ => { }, TimeSpan.FromMilliseconds(1));
        }

        [Fact]
        public void InvalidXMLConfiguration_DoesNotThrowErrorWhen_ThrowExceptionFlagIsNotSet()
        {
            Boolean ExceptionThrown = false;
            try
            {
                LogManager.ThrowExceptions = false;

                LogManager.Configuration = CreateConfigurationFromString(@"
            <nlog internalLogToConsole='IamNotBooleanValue'>
                <targets><target type='MethodCall' name='test' methodName='Throws' className='NLog.UnitTests.LogFactoryTests, NLog.UnitTests.netfx40' /></targets>
                <rules>
                    <logger name='*' minlevel='Debug' writeto='test'></logger>
                </rules>
            </nlog>");
            }
            catch (Exception)
            {
                ExceptionThrown = true;
            }

            Assert.False(ExceptionThrown);

        }

        [Fact]
        public void InvalidXMLConfiguration_ThrowErrorWhen_ThrowExceptionFlagIsSet()
        {
            Boolean ExceptionThrown = false;
            try
            {
                LogManager.ThrowExceptions = true;

                LogManager.Configuration = CreateConfigurationFromString(@"
            <nlog internalLogToConsole='IamNotBooleanValue'>
                <targets><target type='MethodCall' name='test' methodName='Throws' className='NLog.UnitTests.LogFactoryTests, NLog.UnitTests.netfx40' /></targets>
                <rules>
                    <logger name='*' minlevel='Debug' writeto='test'></logger>
                </rules>
            </nlog>");
            }
            catch (Exception)
            {
                ExceptionThrown = true;
            }

            Assert.True(ExceptionThrown);

        }

        [Fact]
        public void ReloadConfigOnTimer_DoesNotThrowConfigException_IfConfigChangedInBetween()
        {
            var loggingConfiguration = new LoggingConfiguration();
            LogManager.Configuration = loggingConfiguration;
            var logFactory = new LogFactory(loggingConfiguration);
            var differentConfiguration = new LoggingConfiguration();

            Assert.DoesNotThrow(() => logFactory.ReloadConfigOnTimer(differentConfiguration));
        }

        private class ReloadNullConfiguration : LoggingConfiguration
        {
            public override LoggingConfiguration Reload()
            {
                return null;
            }
        }

        [Fact]
        public void ReloadConfigOnTimer_DoesNotThrowConfigException_IfConfigReloadReturnsNull()
        {
            var loggingConfiguration = new ReloadNullConfiguration();
            LogManager.Configuration = loggingConfiguration;
            var logFactory = new LogFactory(loggingConfiguration);

            Assert.DoesNotThrow(() => logFactory.ReloadConfigOnTimer(loggingConfiguration));
        }

        [Fact]
        public void ReloadConfigOnTimer_Raises_ConfigurationReloadedEvent()
        {
            var called = false;
            var loggingConfiguration = new LoggingConfiguration();
            LogManager.Configuration = loggingConfiguration;
            var logFactory = new LogFactory(loggingConfiguration);
            logFactory.ConfigurationReloaded += (sender, args) => { called = true; };

            logFactory.ReloadConfigOnTimer(loggingConfiguration);

            Assert.True(called);
        }

        [Fact]
        public void ReloadConfigOnTimer_When_No_Exception_Raises_ConfigurationReloadedEvent_With_Correct_Sender()
        {
            object calledBy = null;
            var loggingConfiguration = new LoggingConfiguration();
            LogManager.Configuration = loggingConfiguration;
            var logFactory = new LogFactory(loggingConfiguration);
            logFactory.ConfigurationReloaded += (sender, args) => { calledBy = sender; };

            logFactory.ReloadConfigOnTimer(loggingConfiguration);

            Assert.Same(calledBy, logFactory);
        }

        [Fact]
        public void ReloadConfigOnTimer_When_No_Exception_Raises_ConfigurationReloadedEvent_With_Argument_Indicating_Success()
        {
            LoggingConfigurationReloadedEventArgs arguments = null;
            var loggingConfiguration = new LoggingConfiguration();
            LogManager.Configuration = loggingConfiguration;
            var logFactory = new LogFactory(loggingConfiguration);
            logFactory.ConfigurationReloaded += (sender, args) => { arguments = args; };

            logFactory.ReloadConfigOnTimer(loggingConfiguration);

            Assert.True(arguments.Succeeded);
        }

        public static void Throws()
        {
            throw new Exception();
        }

        /// <summary>
        /// We should be forward compatible so that we can add easily attributes in the future.
        /// </summary>
        [Fact]
        public void NewAttrOnNLogLevelShouldNotThrowError()
        {
            LogManager.Configuration = CreateConfigurationFromString(@"
            <nlog throwExceptions='true' imAnewAttribute='noError'>
                <targets><target type='file' name='f1' filename='test.log' /></targets>
                <rules>
                    <logger name='*' minlevel='Debug' writeto='f1'></logger>
                </rules>
            </nlog>");


        }

        /// <summary>
        /// Rename file, do edits, and then rename back. The auto reload should work.
        /// </summary>
        [Fact]
        public void Auto_Reload_after_rename()
        {
            try
            {
                var tempFolder = Guid.NewGuid().ToString();

                var newFileName = "other.config";
                string tempPath = Path.Combine(Path.GetTempPath(), tempFolder);
                Directory.CreateDirectory(tempPath);

                var originalFileName = "main.nlog";
                var originalFilePath = Path.Combine(tempPath, originalFileName);
                var newFilePath = Path.Combine(tempPath, newFileName);

                //delete old stuff
                new FileInfo(originalFilePath).Delete();
                new FileInfo(newFilePath).Delete();

                WriteToFile(GetValidXml(), originalFilePath);

                //event for async testing
                var counterEvent = new CountdownEvent(1);

                var xmlLoggingConfiguration = new XmlLoggingConfiguration(originalFilePath);
                LogManager.Configuration = xmlLoggingConfiguration;

                LogManager.ConfigurationReloaded += SignalCounterEvent1(counterEvent);

                //"move"
                var fileInfo = new FileInfo(originalFilePath);
                fileInfo.CopyTo(newFilePath);
                fileInfo.Delete();
                Thread.Sleep(1000);

                //write to new file
                WriteToFile(GetValidXml(@"c:\temp\log2.txt"), newFilePath);

                //"move" back.
                var fileInfo2 = new FileInfo(newFilePath);
                fileInfo2.CopyTo(originalFilePath);
                fileInfo2.Delete();

                counterEvent.Wait(5000);
                Thread.Sleep(1000);

                Test_if_reload_success(@"c:\temp\log2.txt");

                if (counterEvent.CurrentCount != 0)
                {
                    throw new Exception("failed to reload");
                }

            }
            finally
            {
                LogManager.Configuration = null;
            }
        }

        /// <summary>
        /// Reload by writing file test
        /// </summary>
        [Fact]
        public void Auto_reload_validxml_test()
        {
            try
            {
                string tempPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
                Directory.CreateDirectory(tempPath);

                var tempPathFile = Path.Combine(tempPath, "main.nlog");

                WriteToFile(GetValidXml(), tempPathFile);

                //event for async testing
                var counterEvent = new CountdownEvent(1);

                var xmlLoggingConfiguration = new XmlLoggingConfiguration(tempPathFile);
                LogManager.Configuration = xmlLoggingConfiguration;

                LogManager.ConfigurationReloaded += (sender, e) =>
                {

                    if (counterEvent.CurrentCount < 1)
                        counterEvent.Signal();
                };

                Test_if_reload_success(@"c:\temp\log.txt");

                WriteToFile(GetValidXml(@"c:\temp\log2.txt"), tempPathFile);

                //test after signal
                counterEvent.Wait(3000);
                //we need some extra time for completion
                Thread.Sleep(1000);

                Test_if_reload_success(@"c:\temp\log2.txt");

            }
            finally
            {
                LogManager.Configuration = null;

            }
        }

        [Fact]
        public void Auto_Reload_invalidxml_test()
        {
            try
            {
                string tempPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
                Directory.CreateDirectory(tempPath);

                var tempPathFile = Path.Combine(tempPath, "main.nlog");

                WriteToFile(GetValidXml(), tempPathFile);

                //event for async testing
                var counterEvent = new CountdownEvent(1);

                var xmlLoggingConfiguration = new XmlLoggingConfiguration(tempPathFile);
                LogManager.Configuration = xmlLoggingConfiguration;

                LogManager.ConfigurationReloaded += SignalCounterEvent1(counterEvent);

                Test_if_reload_success(@"c:\temp\log.txt");


                //set invalid, set valid
                WriteToFile(InvalidXml, tempPathFile);

                counterEvent.Wait(5000);
                //we need some extra time for completion
                Thread.Sleep(1000);

                if (counterEvent.CurrentCount != 0)
                {
                    throw new Exception("failed to reload");
                }



                LogManager.ConfigurationReloaded -= SignalCounterEvent1(counterEvent);

                var counterEvent2 = new CountdownEvent(1);
                LogManager.ConfigurationReloaded += (sender, e) => SignalCounterEvent(counterEvent2);

                WriteToFile(GetValidXml(@"c:\temp\log2.txt"), tempPathFile);

                counterEvent2.Wait(5000);
                //we need some extra time for completion
                Thread.Sleep(1000);

                if (counterEvent2.CurrentCount != 0)
                {
                    throw new Exception("failed to reload - 2");
                }

                Test_if_reload_success(@"c:\temp\log2.txt");

            }
            finally
            {
                LogManager.Configuration = null;
            }
        }

        private static EventHandler<LoggingConfigurationReloadedEventArgs> SignalCounterEvent1(CountdownEvent counterEvent)
        {
            return (sender, e) => SignalCounterEvent(counterEvent);
        }

        private static void SignalCounterEvent(CountdownEvent counterEvent)
        {
            //we get this event sometimes mulitple times for 1 change. So no signal if not needed.
            if (counterEvent.CurrentCount > 0)
            {
                counterEvent.Signal();
            }
        }

        private static string GetValidXml(string fileName = @"c:\temp\log.txt")
        {
            return
                string.Format(@"<?xml version=""1.0"" encoding=""utf-8"" ?>
<nlog xmlns=""http://www.nlog-project.org/schemas/NLog.xsd""
      xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" 
      autoReload=""true"" 
      internalLogLevel=""Info"" 
      throwExceptions=""false"">
  <targets>
    <target name=""file"" xsi:type=""File""   fileName=""{0}"" layout=""${{level}} "" />
  </targets>
  <rules>
    <logger name=""*"" minlevel=""Error"" writeTo=""file"" />
  </rules>
</nlog>
", fileName);
        }

        /// <summary>
        /// Invalid XML missing closing tags.
        /// </summary>
        const string InvalidXml = @"<?xml version=""1.0"" encoding=""utf-8"" ?>
<nlog xmlns=""http://www.nlog-project.org/schemas/NLog.xsd""
      xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" 
      autoReload=""true"" 
      internalLogLevel=""Info"" 
      throwExceptions=""false"">
  <targets>
    <target name=""file"" xsi:type=""File""   fileName=""c:\temp\log.txt"" layout=""${level} "" />
  </targets>
  <rules>
    <logger name=""*"" minlevel=""Error"" writeTo=""file"" />

";

        /// <summary>
        /// Test after reload
        /// </summary>
        /// <param name="filenameTest"></param>
        private static void Test_if_reload_success(string filenameTest)
        {
            var loggingConfiguration = LogManager.Configuration;
            LogManager.Configuration = loggingConfiguration;
            // xmlLoggingConfiguration.Reload();
            Assert.True(((XmlLoggingConfiguration)loggingConfiguration).AutoReload);
            Assert.Equal(1, loggingConfiguration.FileNamesToWatch.Count());
            //   Assert.Equal(1, xmlLoggingConfiguration.AllTargets.Count);

            var target = LogManager.Configuration.FindTargetByName("file") as FileTarget;
            Assert.NotNull(target);
            Assert.Equal(string.Format(@"'{0}'", filenameTest), target.FileName.ToString());
        }

        /// <summary>
        /// Write config to file
        /// </summary>
        /// <param name="configXML"></param>
        /// <param name="path">path to file</param>
        private static void WriteToFile(string configXML, string path)
        {
            using (StreamWriter fs = File.CreateText(path))
            {
                fs.Write(configXML);
                fs.Flush();
            }
        }

     
    }
}
#endif
