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

using NLog.Internal;

using NUnit.Framework;

#if !NUNIT
using SetUp = Microsoft.VisualStudio.TestTools.UnitTesting.TestInitializeAttribute;
using TestFixture = Microsoft.VisualStudio.TestTools.UnitTesting.TestClassAttribute;
using Test = Microsoft.VisualStudio.TestTools.UnitTesting.TestMethodAttribute;
using TearDown = Microsoft.VisualStudio.TestTools.UnitTesting.TestCleanupAttribute;
#endif

namespace NLog.UnitTests
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;

#if !SILVERLIGHT && !NET2_0 && !MONO && !NET_CF
    using System.IO.Abstractions;
    using FakeItEasy;
#endif

    [TestFixture]
    public class LogFactoryTests : NLogTestBase
    {
#if !SILVERLIGHT && !NET2_0 && !MONO && !NET_CF
        [Test]
        public void Configuration_PrivateBinPathIsNull_DoesNotThrowWhen()
        {
            AppDomainHelper.PrivateBinPath = () => null;
            var fakeFileSystem = A.Fake<IFileSystem>();
            var factory = new LogFactory(fakeFileSystem);

            var loggingConfiguration = factory.Configuration;
        }
        
        [Test]
        public void Configuration_WithPrivateBinPath_CheckIfConfigFileExistsInPrivateBinPath()
        {
            const string AnyDirectory = "C:\\any\\";
            AppDomainHelper.PrivateBinPath = () => AnyDirectory;
            var fakeFileSystem = A.Fake<IFileSystem>();
            var factory = new LogFactory(fakeFileSystem);

            var loggingConfiguration = factory.Configuration;

            A.CallTo(() => fakeFileSystem.File.Exists(Path.Combine(AnyDirectory, "NLog.config"))).MustHaveHappened();
        }
        
        [Test]
        public void Configuration_WithMultiplePrivateBinPath_CheckIfConfigFileExistsInPrivateBinPaths()
        {
            const string AnyDirectory = "C:\\any\\";
            const string SomethingDirectory = "C:\\something\\";
            AppDomainHelper.PrivateBinPath = () => string.Join(";", new[] { AnyDirectory, SomethingDirectory });
            var fakeFileSystem = A.Fake<IFileSystem>();
            var factory = new LogFactory(fakeFileSystem);

            var loggingConfiguration = factory.Configuration;

            A.CallTo(() => fakeFileSystem.File.Exists(Path.Combine(AnyDirectory, "NLog.config"))).MustHaveHappened();
            A.CallTo(() => fakeFileSystem.File.Exists(Path.Combine(SomethingDirectory, "NLog.config"))).MustHaveHappened();
        }
#endif

        [Test]
        public void Flush_DoNotThrowExceptionsAndTimeout_DoesNotThrow()
        {
            LogManager.Configuration = CreateConfigurationFromString(@"
            <nlog throwExceptions='false'>
                <targets><target type='MethodCall' name='test' methodName='Throws' className='NLog.UnitTests.LogFactoryTests, NLog.UnitTests.netfx40' /></targets>
                <rules>
                    <logger name='*' minlevel='Debug' writeto='test'></logger>
                </rules>
            </nlog>");

            Logger logger = LogManager.GetCurrentClassLogger();
            logger.Factory.Flush(TimeSpan.FromMilliseconds(1));
        }
        
        [Test]
        public void Flush_DoNotThrowExceptionsAndTimeout_WritesToInternalLog()
        {
            LogManager.Configuration = CreateConfigurationFromString(@"
            <nlog internalLogToConsole='true' throwExceptions='false'>
                <targets><target type='MethodCall' name='test' methodName='Throws' className='NLog.UnitTests.LogFactoryTests, NLog.UnitTests.netfx40' /></targets>
                <rules>
                    <logger name='*' minlevel='Debug' writeto='test'></logger>
                </rules>
            </nlog>");

            var writer = new StringWriter();
            Console.SetOut(writer);
            Logger logger = LogManager.GetCurrentClassLogger();
            logger.Factory.Flush(TimeSpan.FromMilliseconds(1));

            Assert.IsTrue(writer.ToString().Contains("Error"));
        }
        
        [Test]
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
            catch(Exception e)
            {
                ExceptionThrown = true;
            }
            
            Assert.IsFalse(ExceptionThrown);
            
        }
        
        [Test]
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
            catch(Exception e)
            {
                ExceptionThrown = true;
            }
            
            Assert.IsTrue(ExceptionThrown);
            
        }
        

        public static void Throws()
        {
            throw new Exception();
        }
    }
}
