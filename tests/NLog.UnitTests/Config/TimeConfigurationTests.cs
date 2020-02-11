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

namespace NLog.UnitTests.Config
{
    using System;
    using Time;
    using Xunit;
    
    public class TimeConfigurationTests : NLogTestBase, IDisposable
    {
        public void Dispose()
        {
            TimeSource.Current = new FastLocalTimeSource();
        }

        [Fact]
        public void DefaultTimeSourceTest()
        {
            Assert.IsType<FastLocalTimeSource>(TimeSource.Current);
            XmlLoggingConfiguration.CreateFromXmlString("<nlog />");
            Assert.IsType<FastLocalTimeSource>(TimeSource.Current);
        }

        [Fact]
        public void AccurateLocalTest()
        {
            TestTimeSourceConfiguration<AccurateLocalTimeSource>("AccurateLocal");
        }

        [Fact]
        public void AccurateUtcTest()
        {
            TestTimeSourceConfiguration<AccurateUtcTimeSource>("AccurateUTC");
        }

        [Fact]
        public void FastLocalTest()
        {
            TestTimeSourceConfiguration<FastLocalTimeSource>("FastLocal");
        }

        [Fact]
        public void FastUtcTest()
        {
            TestTimeSourceConfiguration<FastUtcTimeSource>("FastUTC");
        }

        void TestTimeSourceConfiguration<T>(string type)
            where T : TimeSource
        {
            Assert.IsType<FastLocalTimeSource>(TimeSource.Current);
            XmlLoggingConfiguration.CreateFromXmlString(@"
                <nlog>
                    <time type='" + type + @"' />
                </nlog>");
            Assert.IsType<T>(TimeSource.Current);
        }
    }
}
