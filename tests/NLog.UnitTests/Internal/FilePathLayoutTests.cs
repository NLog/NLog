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

using NLog.Layouts;

namespace NLog.UnitTests.Internal
{
    using NLog.Targets;

    using Xunit;
    using Xunit.Extensions;
    using NLog.Internal;

    public class FilePathLayoutTests // Not needed as not using NLog-Core -> : NLogTestBase
    {
        [Theory]
        [InlineData(@"", FilePathKind.Unknown)]
        [InlineData(@" ", FilePathKind.Unknown)]
        [InlineData(null, FilePathKind.Unknown)]
        [InlineData(@"/ test\a", FilePathKind.Absolute)]

        [InlineData(@"test.log", FilePathKind.Relative)]
        [InlineData(@"test", FilePathKind.Relative)]
        [InlineData(@" test.log ", FilePathKind.Relative)]
        [InlineData(@" a/test.log ", FilePathKind.Relative)]

        [InlineData(@".test.log ", FilePathKind.Relative)]
        [InlineData(@"..test.log ", FilePathKind.Relative)]
        [InlineData(@" .. test.log ", FilePathKind.Relative)]
        [InlineData(@"${basedir}\test.log ", FilePathKind.Absolute)]
        [InlineData(@"${BASEDIR}\test.log ", FilePathKind.Absolute)]
        [InlineData(@"${basedir}\test ", FilePathKind.Absolute)]
        [InlineData(@"${BASEDIR}\test ", FilePathKind.Absolute)]
        [InlineData(@"${level}\test ", FilePathKind.Unknown)]

        [InlineData(@"${basedir}/test.log ", FilePathKind.Absolute)]
        [InlineData(@"${BASEDIR}/test.log ", FilePathKind.Absolute)]
        [InlineData(@"${specialfolder:applicationdata}/test.log ", FilePathKind.Absolute)]
        [InlineData(@"${basedir}/test ", FilePathKind.Absolute)]
        [InlineData(@"${BASEDIR}/test ", FilePathKind.Absolute)]
        [InlineData(@"${level}/test ", FilePathKind.Unknown)]
        [InlineData(@" ${level}/test ", FilePathKind.Unknown)]
        [InlineData(@" 
${level}/test ", FilePathKind.Unknown)]
        [InlineData(@"dir 
${level}/test ", FilePathKind.Relative)]
        [InlineData(@"dir${level}/test ", FilePathKind.Relative)]
        public void DetectFilePathKind(string path, FilePathKind expected)
        {
            Layout layout = path;
            var result = FilePathLayout.DetectFilePathKind(layout);
            Assert.Equal(expected, result);
        }

        [Theory]
        [InlineData(@"d:\test.log", FilePathKind.Absolute)]
        [InlineData(@"d:\test", FilePathKind.Absolute)]
        [InlineData(@" d:\test", FilePathKind.Absolute)]
        [InlineData(@" d:\ test", FilePathKind.Absolute)]
        [InlineData(@" d:\ test\a", FilePathKind.Absolute)]
        [InlineData(@"\\test\a", FilePathKind.Absolute)]
        [InlineData(@"\\test/a", FilePathKind.Absolute)]
        [InlineData(@"\ test\a", FilePathKind.Absolute)]
        [InlineData(@" a\test.log ", FilePathKind.Relative)]
        public void DetectFilePathKindWindowsPath(string path, FilePathKind expected)
        {
            if (System.IO.Path.DirectorySeparatorChar != '\\')
                return; //no backward-slash on linux

            Layout layout = path;
            var result = FilePathLayout.DetectFilePathKind(layout);
            Assert.Equal(expected, result);
        }
    }
}
