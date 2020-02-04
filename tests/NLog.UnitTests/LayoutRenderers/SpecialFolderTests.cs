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

namespace NLog.UnitTests.LayoutRenderers
{
#if !NETSTANDARD1_5
    using System;
    using System.IO;
    using Xunit;

    /// <summary>
    ///     Provides Unit testing for <see cref="NLog.LayoutRenderers.SpecialFolderLayoutRenderer"/>
    /// </summary>
    public class SpecialFolderTests : NLogTestBase
    {
        private string sysDir = Environment.GetFolderPath(Environment.SpecialFolder.System);
        private const string sysDirString = "System";

        [Fact]
        public void SpecialFolderTest()
        {
            foreach (var specialDirString in Enum.GetNames(typeof(Environment.SpecialFolder))) {
                var folder = (Environment.SpecialFolder)Enum.Parse(typeof(Environment.SpecialFolder), specialDirString);

                AssertLayoutRendererOutput($"${{specialfolder:folder={specialDirString}}}", Environment.GetFolderPath(folder));
            }
        }

        [Fact]
        public void SpecialFolderDirCombineTest()
        {
            AssertLayoutRendererOutput($"${{specialfolder:folder={sysDirString}:dir=aaa}}", Path.Combine(sysDir, "aaa"));
        }

        [Fact]
        public void SpecialFolderFileCombineTest()
        {
            AssertLayoutRendererOutput($"${{specialfolder:folder={sysDirString}:file=aaa.txt}}", Path.Combine(sysDir, "aaa.txt"));
        }

        [Fact]
        public void SpecialFolderDirFileCombineTest()
        {
            AssertLayoutRendererOutput($"${{specialfolder:folder={sysDirString}:dir=aaa:file=bbb.txt}}", Path.Combine(sysDir, "aaa", "bbb.txt"));
        }
    }
#endif
}
