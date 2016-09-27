// 
// Copyright (c) 2004-2016 Jaroslaw Kowalski <jaak@jkowalski.net>, Kim Christensen, Julian Verdurmen
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

using System.Text;
using NLog.Internal.PoolFactory;
using Xunit;

namespace NLog.UnitTests.Internal.PoolFactory
{
    public class StringBuilderTargetTests : NLogTestBase
    {
        [Fact]
        public void TestEmptyCopyTo()
        {
            StringBuilder sb = new StringBuilder();
            ReusableStringBuilder target = new ReusableStringBuilder(new StringBuilder());
            sb.Append("Old");
            target.CopyTo(sb);
            target.Clear();
            Assert.Equal(0, target.Result.Length);
            Assert.Equal("Old", sb.ToString());
        }

        [Fact]
        public void TestSmallCopyTo()
        {
            StringBuilder sb = new StringBuilder();
            ReusableStringBuilder target = new ReusableStringBuilder(new StringBuilder());
            sb.Append("Old");
            target.Result.Append("New");
            target.CopyTo(sb);
            target.Clear();
            Assert.Equal(0, target.Result.Length);
            Assert.Equal("OldNew", sb.ToString());
        }

        [Fact]
        public void TestLargeCopyTo()
        {
            StringBuilder sb = new StringBuilder();
            ReusableStringBuilder target = new ReusableStringBuilder(new StringBuilder());
            for (int i = 0; i < 256; ++i)
                target.Result.Append("Test");
            target.CopyTo(sb);
            Assert.Equal(256 * "Test".Length, sb.Length);
            Assert.Equal(target.ToString(), sb.ToString());
            target.Clear();
            Assert.Equal(0, target.Result.Length);
        }
    }
}
