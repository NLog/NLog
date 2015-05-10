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

using System;
using System.Text;
using NLog.LayoutRenderers;

namespace NLog.UnitTests.Layouts
{
    using Xunit;

    public class AllEventPropertiesLayoutRendererTests : NLogTestBase
    {
        [Fact]
        public void AllParametersAreSetToDefault()
        {
            var sb = new StringBuilder();
            var renderer = new AllEventPropertiesLayoutRenderer();
            var ev = BuildLogEventWithProperties();
            
            renderer.Render(sb, ev);

            Assert.Equal("a=1, hello=world, 17=100", sb.ToString());
        }

        [Fact]
        public void CustomSeparator()
        {
            var sb = new StringBuilder();
            var renderer = new AllEventPropertiesLayoutRenderer();
            renderer.Separator = " | ";
            var ev = BuildLogEventWithProperties();

            renderer.Render(sb, ev);

            Assert.Equal("a=1 | hello=world | 17=100", sb.ToString());
        }

        [Fact]
        public void CustomFormat()
        {
            var sb = new StringBuilder();
            var renderer = new AllEventPropertiesLayoutRenderer();
            renderer.Format = "[key] is [value]";
            var ev = BuildLogEventWithProperties();

            renderer.Render(sb, ev);

            Assert.Equal("a is 1, hello is world, 17 is 100", sb.ToString());
        }

        [Fact]
        public void NoProperties()
        {
            var sb = new StringBuilder();
            var renderer = new AllEventPropertiesLayoutRenderer();
            var ev = new LogEventInfo();

            renderer.Render(sb, ev);

            Assert.Equal("", sb.ToString());
        }

        [Fact]
        public void TestInvalidCustomFormatWithoutKeyPlaceholder()
        {
            var renderer = new AllEventPropertiesLayoutRenderer();
            var ex = Assert.Throws<ArgumentException>(() => renderer.Format = "[key is [value]");
            Assert.Equal("Invalid format: [key] placeholder is missing.", ex.Message);
        }

        [Fact]
        public void TestInvalidCustomFormatWithoutValuePlaceholder()
        {
            var renderer = new AllEventPropertiesLayoutRenderer();
            var ex = Assert.Throws<ArgumentException>(() => renderer.Format = "[key] is [vlue]");
            Assert.Equal("Invalid format: [value] placeholder is missing.", ex.Message);
        }
        
        private static LogEventInfo BuildLogEventWithProperties()
        {
            var ev = new LogEventInfo();
            ev.Properties["a"] = 1;
            ev.Properties["hello"] = "world";
            ev.Properties[17] = 100;
            return ev;
        }
    }
}