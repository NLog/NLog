// 
// Copyright (c) 2004-2019 Jaroslaw Kowalski <jaak@jkowalski.net>, Kim Christensen, Julian Verdurmen
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

using NLog.LayoutRenderers;
using NLog.LayoutRenderers.Wrappers;
using NLog.Layouts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace NLog.UnitTests.Layouts
{
    public class LayoutParserTests : NLogTestBase
    {
        /// <summary>
        /// https://github.com/NLog/NLog/issues/3193
        /// </summary>
        /// <param name="input"></param>
        [Theory]
        [InlineData(@"                                    ${appdomain:format={0\} {1\}}")]
        [InlineData(@"                           ${cached:${appdomain:format={0\} {1\}}}")]
        [InlineData(@"                  ${cached:${cached:${appdomain:format={0\} {1\}}}}")]
        [InlineData(@"         ${cached:${cached:${cached:${appdomain:format={0\} {1\}}}}}")]
        [InlineData(@"${cached:${cached:${cached:${cached:${appdomain:format={0\} {1\}}}}}}")]
        [InlineData(@"                                    ${literal:text={0\} {1\}}")]
        [InlineData(@"                           ${cached:${literal:text={0\} {1\}}}")]
        [InlineData(@"                  ${cached:${cached:${literal:text={0\} {1\}}}}")]
        [InlineData(@"         ${cached:${cached:${cached:${literal:text={0\} {1\}}}}}")]
        [InlineData(@"${cached:${cached:${cached:${cached:${literal:text={0\} {1\}}}}}}")]
        public void Issue_3193_Nested_Сlosing_Braces(string input)
        {
            var reader = new NLog.Internal.SimpleStringReader(input.Trim());
            var factory = NLog.Config.ConfigurationItemFactory.Default;
            IReadOnlyCollection<LayoutRenderer> renderers = LayoutParser.CompileLayout(factory, reader, isNested: true, text: out var text);

            while (true)
            {
                Assert.Equal(1, renderers.Count);
                var singleRender = renderers.Single();
                if (singleRender is WrapperLayoutRendererBase wrapperRender)
                {
                    renderers = ((SimpleLayout)wrapperRender.Inner).Renderers;
                }
                else
                {
                    break;
                }
            }
            var textRenderer = (LiteralLayoutRenderer)renderers.Single();
            if (input.IndexOf("literal") >= 0)
            {
                Assert.Equal("{0} {1}", textRenderer.Text);
            }
            else if (input.IndexOf("appdomain") >= 0)
            {
                var appDomain = LogFactory.CurrentAppDomain;
                Assert.Equal($"{appDomain.Id} {appDomain.FriendlyName}", textRenderer.Text);
            }
            else
            {
                throw new Xunit.Sdk.XunitException("NOT SUPPORTED");
            }
        }
    }
}
