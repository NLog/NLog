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

#pragma warning disable 0618

namespace NLog.UnitTests.Contexts
{
    using Xunit;

    public class GlobalDiagnosticsContextTests
    {
        [Fact]
        public void GDCTest1()
        {
            GlobalDiagnosticsContext.Clear();
            Assert.False(GlobalDiagnosticsContext.Contains("foo"));
            Assert.Equal(string.Empty, GlobalDiagnosticsContext.Get("foo"));
            Assert.False(GlobalDiagnosticsContext.Contains("foo2"));
            Assert.Equal(string.Empty, GlobalDiagnosticsContext.Get("foo2"));

            GlobalDiagnosticsContext.Set("foo", "bar");
            GlobalDiagnosticsContext.Set("foo2", "bar2");

            Assert.True(GlobalDiagnosticsContext.Contains("foo"));
            Assert.Equal("bar", GlobalDiagnosticsContext.Get("foo"));

            GlobalDiagnosticsContext.Remove("foo");
            Assert.False(GlobalDiagnosticsContext.Contains("foo"));
            Assert.Equal(string.Empty, GlobalDiagnosticsContext.Get("foo"));

            Assert.True(GlobalDiagnosticsContext.Contains("foo2"));
            Assert.Equal("bar2", GlobalDiagnosticsContext.Get("foo2"));
        }

        [Fact]
        public void GDCTest2()
        {
            GDC.Clear();
            Assert.False(GDC.Contains("foo"));
            Assert.Equal(string.Empty, GDC.Get("foo"));
            Assert.False(GDC.Contains("foo2"));
            Assert.Equal(string.Empty, GDC.Get("foo2"));

            GDC.Set("foo", "bar");
            GDC.Set("foo2", "bar2");

            Assert.True(GDC.Contains("foo"));
            Assert.Equal("bar", GDC.Get("foo"));

            GDC.Remove("foo");
            Assert.False(GDC.Contains("foo"));
            Assert.Equal(string.Empty, GDC.Get("foo"));

            Assert.True(GDC.Contains("foo2"));
            Assert.Equal("bar2", GDC.Get("foo2"));
        }
    }
}