// 
// Copyright (c) 2004-2020 Jaroslaw Kowalski <jaak@jkowalski.net>, Kim Christensen, Julian Verdurmen
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

namespace NLog.UnitTests
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Threading;
    using NLog.Config;
    using Xunit;

    public class LogFactoryTests : NLogTestBase
    {
        [Fact]
        public void Flush_DoNotThrowExceptionsAndTimeout_DoesNotThrow()
        {
            // Arrange
            LogManager.Configuration = XmlLoggingConfiguration.CreateFromXmlString($@"
            <nlog throwExceptions='false'>
                <targets>
                    <target type='BufferingWrapper' name='test'>
                        <target type='MethodCall' name='test_wrapped' methodName='{nameof(TestClass.GenerateTimeout)}' className='{typeof(TestClass).AssemblyQualifiedName}' />
                    </target>
                </targets>
                <rules>
                    <logger name='*' minlevel='Debug' writeto='test'></logger>
                </rules>
            </nlog>");

            Logger logger = LogManager.GetCurrentClassLogger();
            logger.Info("Prepare Timeout");

            Exception timeoutException = null;
            ManualResetEvent manualResetEvent = new ManualResetEvent(false);

            // Act
            logger.Factory.Flush(TimeSpan.FromMilliseconds(1));
            logger.Factory.Flush(ex => { timeoutException = ex; manualResetEvent.Set(); }, TimeSpan.FromMilliseconds(1));

            // Assert
            Assert.True(manualResetEvent.WaitOne(5000));
            Assert.NotNull(timeoutException);
        }

        [Fact]
        public void InvalidXMLConfiguration_DoesNotThrowErrorWhen_ThrowExceptionFlagIsNotSet()
        {
            using (new NoThrowNLogExceptions())
            {
                LogManager.Configuration = XmlLoggingConfiguration.CreateFromXmlString($@"
            <nlog internalLogIncludeTimestamp='IamNotBooleanValue'>
                <targets><target type='Debug' name='test' /></targets>
                <rules>
                    <logger name='*' minlevel='Debug' writeto='test'></logger>
                </rules>
            </nlog>");
            }
        }

        [Fact]
        public void InvalidXMLConfiguration_ThrowErrorWhen_ThrowExceptionFlagIsSet()
        {
            Boolean ExceptionThrown = false;
            try
            {
                LogManager.ThrowExceptions = true;

                LogManager.Configuration = XmlLoggingConfiguration.CreateFromXmlString($@"
            <nlog internalLogIncludeTimestamp='IamNotBooleanValue'>
                <targets><target type='Debug' name='test' /></targets>
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
        public void Configuration_InaccessibleNLog_doesNotThrowException()
        {
            string tempDirectory = null;

            try
            {
                // Arrange
                var logFactory = CreateEmptyNLogFile(out tempDirectory, out var configFile);
                using (OpenStream(configFile))
                {
                    // Act
                    var loggingConfig = logFactory.Configuration;

                    // Assert
                    Assert.Null(loggingConfig);
                }

                // Assert
                Assert.NotNull(logFactory.Configuration);
            }
            finally
            {
                if (tempDirectory != null && Directory.Exists(tempDirectory))
                    Directory.Delete(tempDirectory, true);
            }
        }

        [Fact]
        public void LoadConfiguration_InaccessibleNLog_throwException()
        {
            string tempDirectory = null;

            try
            {
                // Arrange
                var logFactory = CreateEmptyNLogFile(out tempDirectory, out var configFile);
                using (OpenStream(configFile))
                {
                    // Act
                    var ex = Record.Exception(() => logFactory.LoadConfiguration(configFile));

                    // Assert
                    Assert.IsType<FileNotFoundException>(ex);
                }

                // Assert
                Assert.NotNull(logFactory.LoadConfiguration(configFile).Configuration);
            }
            finally
            {
                if (tempDirectory != null && Directory.Exists(tempDirectory))
                    Directory.Delete(tempDirectory, true);
            }
        }

        private static FileStream OpenStream(string configFile)
        {
            return new FileStream(configFile, FileMode.Open, FileAccess.ReadWrite, FileShare.None);
        }

        private static LogFactory CreateEmptyNLogFile(out string tempDirectory, out string filePath)
        {
            tempDirectory = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));
            filePath = Path.Combine(tempDirectory, "NLog.config");
            Directory.CreateDirectory(tempDirectory);
            File.WriteAllText(filePath, "<nlog />");
            LogFactory logFactory = new LogFactory();
            logFactory.SetCandidateConfigFilePaths(new[] { filePath });
            return logFactory;
        }

        [Fact]
        public void SecondaryLogFactoryDoesNotTakePrimaryLogFactoryLock()
        {
            File.WriteAllText("NLog.config", "<nlog />");
            try
            {
                bool threadTerminated;

                var primaryLogFactory = LogManager.factory;
                var primaryLogFactoryLock = primaryLogFactory._syncRoot;
                // Simulate a potential deadlock. 
                // If the creation of the new LogFactory takes the lock of the global LogFactory, the thread will deadlock.
                lock (primaryLogFactoryLock)
                {
                    var thread = new Thread(() =>
                    {
                        (new LogFactory()).GetCurrentClassLogger();
                    });
                    thread.Start();
                    threadTerminated = thread.Join(TimeSpan.FromSeconds(1));
                }

                Assert.True(threadTerminated);
            }
            finally
            {
                try
                {
                    File.Delete("NLog.config");
                }
                catch { }
            }
        }

        [Fact]
        public void ReloadConfigOnTimer_DoesNotThrowConfigException_IfConfigChangedInBetween()
        {
            EventHandler<LoggingConfigurationChangedEventArgs> testChanged = null;

            try
            {
                LogManager.Configuration = null;

                var loggingConfiguration = new LoggingConfiguration();
                LogManager.Configuration = loggingConfiguration;

                var configLoader = new LoggingConfigurationWatchableFileLoader(LogFactory.DefaultAppEnvironment);
                var logFactory = new LogFactory(configLoader);
                logFactory.Configuration = loggingConfiguration;

                var differentConfiguration = new LoggingConfiguration();

                // Verify that the random configuration change is ignored (Only the final reset is reacted upon)
                bool called = false;
                LoggingConfiguration oldConfiguration = null, newConfiguration = null;
                testChanged = (s, e) => { called = true; oldConfiguration = e.DeactivatedConfiguration; newConfiguration = e.ActivatedConfiguration; };
                LogManager.LogFactory.ConfigurationChanged += testChanged;

                var exRecorded = Record.Exception(() => configLoader.ReloadConfigOnTimer(differentConfiguration));
                Assert.Null(exRecorded);

                // Final reset clears the configuration, so it is changed to null
                LogManager.Configuration = null;
                Assert.True(called);
                Assert.Equal(loggingConfiguration, oldConfiguration);
                Assert.Null(newConfiguration);
            }
            finally
            {
                if (testChanged != null)
                    LogManager.LogFactory.ConfigurationChanged -= testChanged;
            }
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
            var configLoader = new LoggingConfigurationWatchableFileLoader(LogFactory.DefaultAppEnvironment);
            var logFactory = new LogFactory(configLoader);
            logFactory.Configuration = loggingConfiguration;

            var exRecorded = Record.Exception(() => configLoader.ReloadConfigOnTimer(loggingConfiguration));
            Assert.Null(exRecorded);
        }

        /// <summary>
        /// We should be forward compatible so that we can add easily attributes in the future.
        /// </summary>
        [Fact]
        public void NewAttrOnNLogLevelShouldNotThrowError()
        {
            using (new NoThrowNLogExceptions())
            {
                LogManager.Configuration = XmlLoggingConfiguration.CreateFromXmlString(@"
                <nlog imAnewAttribute='noError'>
                    <targets><target type='file' name='f1' filename='test.log' /></targets>
                    <rules>
                        <logger name='*' minlevel='Debug' writeto='f1'></logger>
                    </rules>
                </nlog>");
            }
        }

        [Fact]
        public void ValueWithVariableMustNotCauseInfiniteRecursion()
        {
            LogManager.Configuration = null;

            var filename = "NLog.config";
            File.WriteAllText(filename, @"
            <nlog>
                <variable name='dir' value='c:\mylogs' />
                <targets>
                    <target name='f' type='file' fileName='${var:dir}\test.log' />
                </targets>
                <rules>
                    <logger name='*' writeTo='f' />
                </rules>
            </nlog>");
            try
            {
                var x = LogManager.Configuration;
                //2nd call
                var config = new XmlLoggingConfiguration(filename);
            }
            finally
            {
                File.Delete(filename);
            }
        }

        [Fact]
        public void SuspendAndResumeLogging_InOrder()
        {
            LogFactory factory = new LogFactory();

            // In order Suspend => Resume [Case 1]
            Assert.True(factory.IsLoggingEnabled());
            factory.SuspendLogging();
            Assert.False(factory.IsLoggingEnabled());
            factory.ResumeLogging();
            Assert.True(factory.IsLoggingEnabled());

            // In order Suspend => Resume [Case 2]
            using (var factory2 = new LogFactory())
            {
                Assert.True(factory.IsLoggingEnabled());
                factory.SuspendLogging();
                Assert.False(factory.IsLoggingEnabled());
                factory.ResumeLogging();
                Assert.True(factory.IsLoggingEnabled());
            }
        }

        [Fact]
        public void SuspendAndResumeLogging_OutOfOrder()
        {
            LogFactory factory = new LogFactory();

            // Out of order Resume => Suspend => (Suspend => Resume)
            factory.ResumeLogging();
            Assert.True(factory.IsLoggingEnabled());
            factory.SuspendLogging();
            Assert.True(factory.IsLoggingEnabled());
            factory.SuspendLogging();
            Assert.False(factory.IsLoggingEnabled());
            factory.ResumeLogging();
            Assert.True(factory.IsLoggingEnabled());
        }

        [Fact]
        public void LogFactory_GetLoggerWithNull_ShouldThrow()
        {
            LogFactory factory = new LogFactory();
            Assert.Throws<ArgumentNullException>(() => factory.GetLogger(null));
        }

        private class TestClass
        {
            public static void GenerateTimeout()
            {
                Thread.Sleep(5000);
            }
        }
    }
}
