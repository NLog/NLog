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

using System;
using System.IO;
using System.Reflection;
using System.Threading;
using NLog.Common;
using NLog.Internal;
using Xunit;
using Xunit.Extensions;

namespace NLog.UnitTests.Internal
{
    public class ExceptionHelperTests : NLogTestBase
    {
        [Theory]
#if !NETSTANDARD1_5
        [InlineData(typeof(StackOverflowException), true)]
        [InlineData(typeof(ThreadAbortException), true)]
#endif
        [InlineData(typeof(NLogConfigurationException), false)]
        [InlineData(typeof(Exception), false)]
        [InlineData(typeof(ArgumentException), false)]
#if !DEBUG
        [InlineData(typeof(NullReferenceException), false)]
#endif
        [InlineData(typeof(OutOfMemoryException), true)]
        public void TestMustBeRethrowImmediately(Type t, bool result)
        {
            var ex = CreateException(t);
            Assert.Equal(result, ex.MustBeRethrownImmediately());
        }

        [Theory]
#if !NETSTANDARD1_5
        [InlineData(typeof(StackOverflowException), true, false, false)]
        [InlineData(typeof(StackOverflowException), true, true, false)]
        [InlineData(typeof(ThreadAbortException), true, false, false)]
        [InlineData(typeof(ThreadAbortException), true, true, false)]
#endif
        [InlineData(typeof(NLogConfigurationException), true, true, true)]
        [InlineData(typeof(NLogConfigurationException), false, true, false)]
        [InlineData(typeof(NLogConfigurationException), true, true, null)]
        [InlineData(typeof(NLogConfigurationException), true, false, true)]
        [InlineData(typeof(NLogConfigurationException), false, false, false)]
        [InlineData(typeof(NLogConfigurationException), false, false, null)]
        [InlineData(typeof(Exception), false, false, false)]
        [InlineData(typeof(Exception), false, false, true)]
        [InlineData(typeof(Exception), false, false, null)]
        [InlineData(typeof(Exception), true, true, false)]
        [InlineData(typeof(Exception), true, true, true)]
        [InlineData(typeof(Exception), true, true, null)]
        [InlineData(typeof(ArgumentException), false, false, false)]
        [InlineData(typeof(ArgumentException), false, false, true)]
        [InlineData(typeof(ArgumentException), false, false, null)]
        [InlineData(typeof(ArgumentException), true, true, false)]
        [InlineData(typeof(ArgumentException), true, true, true)]
        [InlineData(typeof(ArgumentException), true, true, null)]
#if !DEBUG
        [InlineData(typeof(NullReferenceException), false, false, false)]
        [InlineData(typeof(NullReferenceException), true, true, false)]
#endif
        [InlineData(typeof(OutOfMemoryException), true, false, false)]
        [InlineData(typeof(OutOfMemoryException), true, true, false)]
        public void MustBeRethrown(Type exceptionType, bool result, bool throwExceptions, bool? throwConfigException)
        {
            LogManager.ThrowExceptions = throwExceptions;
            LogManager.ThrowConfigExceptions = throwConfigException;

            var ex = CreateException(exceptionType);
            Assert.Equal(result, ex.MustBeRethrown());
        }

        [Theory]
        [InlineData("Error has been raised.", typeof(ArgumentException), false, "Error")]
        [InlineData("Error has been raised.", typeof(ArgumentException), true, "Warn")]
        [InlineData("Error has been raised.", typeof(NLogConfigurationException), false, "Warn")]
        [InlineData("Error has been raised.", typeof(NLogConfigurationException), true, "Warn")]
        [InlineData("", typeof(ArgumentException), true, "Warn")]
        [InlineData("", typeof(NLogConfigurationException), true, "Warn")]
        public void MustBeRethrown_ShouldLog_exception_and_only_once(string text, Type exceptionType, bool logFirst, string levelText)
        {
            using (new InternalLoggerScope())
            {

                var level = LogLevel.FromString(levelText);
                InternalLogger.LogLevel = LogLevel.Trace;

                var stringWriter = new StringWriter();
                InternalLogger.LogWriter = stringWriter;

                InternalLogger.IncludeTimestamp = false;

                var ex1 = CreateException(exceptionType);


                //exception should be once 
                const string prefix = " Exception: ";
                string expected =
                    levelText + " " + text + prefix + ex1 + Environment.NewLine;

                // Named (based on LogLevel) public methods.

                if (logFirst)
                    InternalLogger.Log(ex1, level, text);

                ex1.MustBeRethrown();

                stringWriter.Flush();
                var actual = stringWriter.ToString();
                Assert.Equal(expected, actual);
            }


        }

        private static Exception CreateException(Type exceptionType)
        {
            return exceptionType.GetConstructor(BindingFlags.NonPublic | BindingFlags.Default | BindingFlags.Instance | BindingFlags.Public, null, Type.EmptyTypes, null).Invoke(null) as Exception;
        }
    }
}
