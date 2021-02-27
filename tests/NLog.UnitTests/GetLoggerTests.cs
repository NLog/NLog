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

using NLog.Config;

namespace NLog.UnitTests
{
    using System;
    using Xunit;

    public class GetLoggerTests : NLogTestBase
    {
        [Fact]
        public void GetCurrentClassLoggerTest()
        {
            ILogger logger = LogManager.GetCurrentClassLogger();
            Assert.Equal("NLog.UnitTests.GetLoggerTests", logger.Name);
        }

        [Fact]
        public void GetCurrentClassLoggerLambdaTest()
        {
            System.Linq.Expressions.Expression<Func<ILogger>> sum = () => LogManager.GetCurrentClassLogger();
            ILogger logger = sum.Compile().Invoke();
            Assert.Equal("NLog.UnitTests.GetLoggerTests", logger.Name);
        }

        [Fact]
        public void TypedGetLoggerTest()
        {
            LogFactory lf = new LogFactory();

            MyLogger l1 = (MyLogger)lf.GetLogger("AAA", typeof(MyLogger));
            MyLogger l2 = lf.GetLogger<MyLogger>("AAA");
            ILogger l3 = lf.GetLogger("AAA", typeof(Logger));
            ILogger l5 = lf.GetLogger("AAA");
            ILogger l6 = lf.GetLogger("AAA");

            Assert.Same(l1, l2);
            Assert.Same(l5, l6);
            Assert.Same(l3, l5);

            Assert.NotSame(l1, l3);

            Assert.Equal("AAA", l1.Name);
            Assert.Equal("AAA", l3.Name);
        }

        [Fact]
        public void TypedGetCurrentClassLoggerTest()
        {
            LogFactory lf = new LogFactory();

            MyLogger l1 = (MyLogger)lf.GetCurrentClassLogger(typeof(MyLogger));
            MyLogger l2 = lf.GetCurrentClassLogger<MyLogger>();
            ILogger l3 = lf.GetCurrentClassLogger(typeof(Logger));
     
            ILogger l5 = lf.GetCurrentClassLogger();
            ILogger l6 = lf.GetCurrentClassLogger();

            Assert.Same(l1, l2);
            Assert.Same(l5, l6);
            Assert.Same(l3, l5);

            Assert.NotSame(l1, l3);

            Assert.Equal("NLog.UnitTests.GetLoggerTests", l1.Name);
            Assert.Equal("NLog.UnitTests.GetLoggerTests", l3.Name);
        }

        [Fact]
        public void GenericGetLoggerTest()
        {
            LogFactory<MyLogger> lf = new LogFactory<MyLogger>();

            MyLogger l1 = lf.GetLogger("AAA");
            MyLogger l2 = lf.GetLogger("AAA");
            MyLogger l3 = lf.GetLogger("BBB");

            Assert.Same(l1, l2);
            Assert.NotSame(l1, l3);

            Assert.Equal("AAA", l1.Name);
            Assert.Equal("BBB", l3.Name);
        }

        [Fact]
        public void GenericGetCurrentClassLoggerTest()
        {
            LogFactory<MyLogger> lf = new LogFactory<MyLogger>();

            MyLogger l1 = lf.GetCurrentClassLogger();
            MyLogger l2 = lf.GetCurrentClassLogger();

            Assert.Same(l1, l2);
            Assert.Equal("NLog.UnitTests.GetLoggerTests", l1.Name);
        }

        public class InvalidLogger
        {
            private InvalidLogger()
            {
            }
        }


        [Fact]
        public void InvalidLoggerConfiguration_NotThrowsThrowExceptions_NotThrows()
        {
            using (new NoThrowNLogExceptions())
            {
                LogManager.GetCurrentClassLogger(typeof(InvalidLogger));
            }
        }

        [Fact]
        public void InvalidLoggerConfiguration_ThrowsThrowExceptions_Throws()
        {
            LogManager.ThrowExceptions = true;
            InvalidLoggerConfiguration_ThrowsNLogResolveException();
        }

        private void InvalidLoggerConfiguration_ThrowsNLogResolveException()
        {
            Assert.Throws<NLogDependencyResolveException>(() =>
            {
                LogManager.GetCurrentClassLogger(typeof(InvalidLogger));
            });

        }

        public class MyLogger : Logger
        {
            public MyLogger()
            {
            }

            public void LogWithEventID(int eventID, string message, object[] par)
            {
                LogEventInfo lei = LogEventInfo.Create(LogLevel.Info, Name, null, message, par);
                lei.Properties["EventId"] = eventID;
                Log(typeof(MyLogger), lei);
            }
        }
    }
}
