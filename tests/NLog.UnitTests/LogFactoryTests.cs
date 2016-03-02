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

#if !SILVERLIGHT
namespace NLog.UnitTests
{
    using System;
    using System.IO;
    using System.Threading;
    using System.Reflection;

    using Xunit;

    using NLog.Config;

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
        public void SecondaryLogFactoryDoesNotTakePrimaryLogFactoryLock()
        {
            File.WriteAllText("NLog.config", "<nlog />");
            try
            {
                bool threadTerminated;

                var primaryLogFactory = typeof(LogManager).GetField("factory", BindingFlags.Static | BindingFlags.NonPublic).GetValue(null);
                var primaryLogFactoryLock = typeof(LogFactory).GetField("syncRoot", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(primaryLogFactory);
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
        
        [Fact]
        public void EnableAndDisableLogging()
        {
            LogFactory factory = new LogFactory();
#pragma warning disable 618
            // In order Suspend => Resume 
            Assert.True(factory.IsLoggingEnabled());
            factory.DisableLogging();
            Assert.False(factory.IsLoggingEnabled());
            factory.EnableLogging();
            Assert.True(factory.IsLoggingEnabled());
#pragma warning restore 618           
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
    }
}
#endif
