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
using System.Linq;
using Xunit;

namespace NLog.UnitTests.LayoutRenderers
{
    using System;

    public class AppDomainLayoutRendererTests : NLogTestBase
    {
        private int id = AppDomain.CurrentDomain.Id;
        private string friendlyname = AppDomain.CurrentDomain.FriendlyName;

        [Fact]
        public void AppDomainTest()
        {
            //example: 0003: NLog.UnitTests
            AssertLayoutRendererOutput("${appdomain}", $"{id:0000}:{friendlyname}");
        }

        [Fact]
        public void AppDomainShortFormatTest()
        {
            //example: 03
            AssertLayoutRendererOutput("${appdomain:format=short}", string.Format("{0:00}", id, friendlyname));
        }

        [Fact]
        public void AppDomainTestLongFormatTest()
        {
            //example: 0003: NLog.UnitTests
            AssertLayoutRendererOutput("${appdomain:format=long}", $"{id:0000}:{friendlyname}");
        }

        [Fact]
        public void AppDomainCustomFormatTest()
        {
            //example: 0003: NLog.UnitTests
            AssertLayoutRendererOutput("${appdomain:format={1\\}{0\\}}", string.Format("{1}{0}", id, friendlyname));
        }


    }
}