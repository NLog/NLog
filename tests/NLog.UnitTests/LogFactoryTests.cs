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

using FakeItEasy;

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
    using System.Collections.Generic;
    using System.Linq;
    using System.IO.Abstractions;

    [TestFixture]
    public class LogFactoryTest : NLogTestBase
    {
#if !NET_CF
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
    }
}
