// 
// Copyright (c) 2004-2021 Jaroslaw Kowalski <jaak@jkowalski.net>, Kim Christensen, Julian Verdurmen
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

namespace NLog.UnitTests.LayoutRenderers
{
    using System;
    using System.IO;
    using System.Linq;
    using NLog.Layouts;
    using Xunit;

    public class BaseDirTests : NLogTestBase
    {
        private string baseDir = AppDomain.CurrentDomain.BaseDirectory;

        [Fact]
        public void BaseDirTest()
        {
            AssertLayoutRendererOutput("${basedir}", baseDir);
        }

        [Fact]
        public void BaseDir_FixTempDir_NotRequired()
        {
            AssertLayoutRendererOutput("${basedir:fixtempdir=true}", baseDir);
        }

        [Fact]
        public void BaseDirCombineTest()
        {
            AssertLayoutRendererOutput("${basedir:dir=aaa}", Path.Combine(baseDir, "aaa"));
        }

        [Fact]
        public void BaseDirFileCombineTest()
        {
            AssertLayoutRendererOutput("${basedir:file=aaa.txt}", Path.Combine(baseDir, "aaa.txt"));
        }

        [Fact]
        public void BaseDirCurrentProcessTest()
        {
            Layout l = "${basedir:processdir=true}";
            var dir = l.Render(LogEventInfo.CreateNullEvent());

            Assert.NotNull(dir);
            Assert.True(Directory.Exists(dir), $"dir '{dir}' doesn't exists");
            Assert.Equal(Path.GetDirectoryName(CurrentProcessPath), dir);
        }

        [Fact]
        public void BaseDirDirFileCombineTest()
        {
            AssertLayoutRendererOutput("${basedir:dir=aaa:file=bbb.txt}", Path.Combine(baseDir, "aaa", "bbb.txt"));
        }

        [Fact]
        [Obsolete("For unit testing only. Marked obsolete on NLog 5.0")]
        public void InjectBaseDirAndCheckConfigPathsTest()
        {
            string fakeBaseDir = @"y:\root\";
            var old = LogFactory.CurrentAppDomain;
            try
            {
                var currentAppDomain = new Mocks.AppDomainMock(fakeBaseDir);
                LogFactory.CurrentAppDomain = currentAppDomain;

                //test 1 
                AssertLayoutRendererOutput("${basedir}", fakeBaseDir);

                //test 2
                var paths = LogManager.LogFactory.GetCandidateConfigFilePaths().ToList();
                var count = paths.Count(p => p.StartsWith(fakeBaseDir));

                Assert.True(count > 0, $"At least one path should start with '{fakeBaseDir}'");
            }
            finally
            {
                //restore
                LogFactory.CurrentAppDomain = old;
            }
        }

        [Fact]
        public void BaseDir_FixTempDir_ChoosesProcessDir()
        {
            var tempDir = System.IO.Path.GetTempPath();
            var processPath = CurrentProcessPath;

            var appEnvironment = new Mocks.AppEnvironmentMock(null, null);
            appEnvironment.AppDomainBaseDirectory = tempDir;
            appEnvironment.UserTempFilePath = tempDir;
            appEnvironment.CurrentProcessFilePath = processPath;
            var baseLayoutRenderer = new NLog.LayoutRenderers.BaseDirLayoutRenderer(appEnvironment);

            // test1
            Assert.Equal(tempDir, baseLayoutRenderer.Render(LogEventInfo.CreateNullEvent()));

            // test2
            baseLayoutRenderer.FixTempDir = true;
            Assert.Equal(Path.GetDirectoryName(processPath), baseLayoutRenderer.Render(LogEventInfo.CreateNullEvent()));
        }
    }
}