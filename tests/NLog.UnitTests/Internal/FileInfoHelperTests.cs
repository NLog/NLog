//
// Copyright (c) 2004-2024 Jaroslaw Kowalski <jaak@jkowalski.net>, Kim Christensen, Julian Verdurmen
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

namespace NLog.UnitTests.Internal
{
    using NLog.Internal;
    using Xunit;

    public class FileInfoHelperTests // Not needed as not using NLog-Core -> : NLogTestBase
    {
        [Theory]
        [InlineData(@"", false)]
        [InlineData(@" ", false)]
        [InlineData(null, false)]
        [InlineData(@"/ test\a", false)]
        [InlineData(@"test.log", true)]
        [InlineData(@"test", true)]
        [InlineData(@" test.log ", true)]
        [InlineData(@" a/test.log ", true)]
        [InlineData(@".test.log ", true)]
        [InlineData(@"..test.log ", true)]
        [InlineData(@" .. test.log ", true)]
        [InlineData(@"dir$/test ", true)]
        public void DetectFilePathKind(string path, bool expected)
        {
            var result = FileInfoHelper.IsRelativeFilePath(path);
            Assert.Equal(expected, result);
        }

        [Theory]
        [InlineData(@"d:\test.log", false)]
        [InlineData(@"d:\test", false)]
        [InlineData(@" d:\test", false)]
        [InlineData(@" d:\ test", false)]
        [InlineData(@" d:\ test\a", false)]
        [InlineData(@"\\test\a", false)]
        [InlineData(@"\\test/a", false)]
        [InlineData(@"\ test\a", false)]
        [InlineData(@" a\test.log ", true)]
        public void DetectFilePathKindWindowsPath(string path, bool expected)
        {
            if (System.IO.Path.DirectorySeparatorChar != '\\')
                return; //no backward-slash on linux

            var result = FileInfoHelper.IsRelativeFilePath(path);
            Assert.Equal(expected, result);
        }
    }
}
