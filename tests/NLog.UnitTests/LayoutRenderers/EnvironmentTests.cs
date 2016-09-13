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

#if !SILVERLIGHT

namespace NLog.UnitTests.LayoutRenderers
{
    using System;
    using Xunit;

    public class EnvironmentTests : NLogTestBase
    {
        [Fact]
        public void EnvironmentTest()
        {
            AssertLayoutRendererOutput("${environment:variable=PATH}", System.Environment.GetEnvironmentVariable("PATH"));
        }

        [Fact]
        public void EnvironmentSimpleTest()
        {
            AssertLayoutRendererOutput("${environment:PATH}", System.Environment.GetEnvironmentVariable("PATH"));
        }

        [Fact]
        public void Environment_WhenVariableIsLayout_ShouldBeWrittenAsLayout()
        {
 	        Environment.SetEnvironmentVariable("NLOGTEST", "${level}");
	        AssertLayoutRendererOutput("${environment:variable=NLOGTEST}", "Info");
 	        AssertLayoutRendererOutput("${environment:NLOGTEST}", "Info");
        }
	
        [Fact]
        public void Environment_WhenVariableExists_DoNothing()
        {
	        Environment.SetEnvironmentVariable("NLOGTEST", "ABC1234");           
	        // Test default value with different variations on variable parameter syntax. 
	        AssertLayoutRendererOutput("${environment:variable=NLOGTEST:default=5678}", "ABC1234");
	        AssertLayoutRendererOutput("${environment:NLOGTEST:default=5678}", "ABC1234");
        }

        [Fact]
        public void Environment_empty()
        {
            AssertLayoutRendererOutput("${environment}", "");
            AssertLayoutRendererOutput("${environment:noDefault}", "");
        }

        [Fact]
        public void Environment_WhenVariableIsLayoutAndExists_DoNothing()
        {
	        Environment.SetEnvironmentVariable("NLOGTEST", "${level}");
	        AssertLayoutRendererOutput("${environment:NLOGTEST:default=5678}", "Info");
        }
	
        [Fact]
        public void Environment_WhenVariableDoesNotExists_UseDefault()
        {
	        if (Environment.GetEnvironmentVariable("NLOGTEST") != null)
	        {
	            Environment.SetEnvironmentVariable("NLOGTEST", null);
	        }
	
	        // Test default value with different variations on variable parameter syntax. 
	        AssertLayoutRendererOutput("${environment:variable=NLOGTEST:default=1234}", "1234");
	        AssertLayoutRendererOutput("${environment:NLOGTEST:default=5678}", "5678");
        }
	
        [Fact]
        public void Environment_WhenDefaultEmpty_EmptyString()
        {
	        if (Environment.GetEnvironmentVariable("NLOGTEST") != null)
	        {
	            Environment.SetEnvironmentVariable("NLOGTEST", null);
	        }
	
	        // Test default value with different variations on variable parameter syntax. 
	        AssertLayoutRendererOutput("${environment:variable=NLOGTEST:default=}", "");
	        AssertLayoutRendererOutput("${environment:NLOGTEST:default=}", "");
        }
    }
}

#endif