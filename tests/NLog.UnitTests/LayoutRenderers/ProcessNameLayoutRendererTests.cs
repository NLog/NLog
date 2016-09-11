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

//no silverlight because of xUnit needed
#if !SILVERLIGHT && !__IOS__

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NLog.LayoutRenderers;
using Xunit;
using Xunit.Extensions;
using NLog.Layouts;

namespace NLog.UnitTests.LayoutRenderers
{
    public class ProcessNameLayoutRendererTests : NLogTestBase
    {
        [Fact]
        public void RenderProcessNameLayoutRenderer()
        {
            Layout layout = "${processname}";

            layout.Initialize(null);
            string actual = layout.Render(LogEventInfo.CreateNullEvent());
            layout.Close();


            Assert.NotNull(actual);
            Assert.True(actual.Length > 0, "actual.Length > 0");
            var lower = actual.ToLower();

            //lowercase
            var allowedProcessNames = new List<string> {"vstest.executionengine", "xunit", " mono-sgen"};
            
            Assert.True(allowedProcessNames.Any(p => lower.Contains(p)), string.Format("validating processname failed. Please add (if correct) '{0}' to 'allowedProcessNames'", actual));
        }

        [Fact]
        public void RenderProcessNameLayoutRenderer_fullname()
        {
            Layout layout = "${processname:fullname=true}";

            layout.Initialize(null);
            string actual = layout.Render(LogEventInfo.CreateNullEvent());
            layout.Close();


            Assert.NotNull(actual);
            Assert.True(actual.Length > 0, "actual.Length > 0");
            Assert.True(File.Exists(actual), "process not found");
        }
    }
}
#endif