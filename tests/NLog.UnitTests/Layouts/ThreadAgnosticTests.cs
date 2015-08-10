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

namespace NLog.UnitTests.Layouts
{
    using NLog.LayoutRenderers;
    using NLog.Internal;
    using NLog.Config;
    using NLog.LayoutRenderers.Wrappers;
    using NLog.Layouts;
    using Xunit;

    public class ThreadAgnosticTests : NLogTestBase
    {
        [Fact]
        public void ThreadAgnosticAttributeTest()
        {
            foreach (var t in ReflectionHelpers.SafeGetTypes(typeof(Layout).Assembly))
            {
                if (t.Namespace == typeof(WrapperLayoutRendererBase).Namespace)
                {
                    if (t.IsAbstract || t.IsEnum || t.IsNestedPrivate)
                    {
                        // skip non-concrete types, enumerations, and private nested types
                        continue;
                    }

                    Assert.True(t.IsDefined(typeof(ThreadAgnosticAttribute), true), "Type " + t + " is missing [ThreadAgnostic] attribute.");
                }
            }
        }

        [Fact]
        public void ThreadAgnosticTest()
        {
            Layout l = new SimpleLayout("${message}");
            l.Initialize(null);
            Assert.True(l.IsThreadAgnostic);
        }

        [Fact]
        public void NonThreadAgnosticTest()
        {
            Layout l = new SimpleLayout("${threadname}");
            l.Initialize(null);
            Assert.False(l.IsThreadAgnostic);
        }

        [Fact]
        public void AgnosticPlusNonAgnostic()
        {
            Layout l = new SimpleLayout("${message}${threadname}");
            l.Initialize(null);
            Assert.False(l.IsThreadAgnostic);
        }

        [Fact]
        public void AgnosticPlusAgnostic()
        {
            Layout l = new SimpleLayout("${message}${level}${logger}");
            l.Initialize(null);
            Assert.True(l.IsThreadAgnostic);
        }

        [Fact]
        public void WrapperOverAgnostic()
        {
            Layout l = new SimpleLayout("${rot13:${message}}");
            l.Initialize(null);
            Assert.True(l.IsThreadAgnostic);
        }

        [Fact]
        public void DoubleWrapperOverAgnostic()
        {
            Layout l = new SimpleLayout("${lowercase:${rot13:${message}}}");
            l.Initialize(null);
            Assert.True(l.IsThreadAgnostic);
        }

        [Fact]
        public void TripleWrapperOverAgnostic()
        {
            Layout l = new SimpleLayout("${uppercase:${lowercase:${rot13:${message}}}}");
            l.Initialize(null);
            Assert.True(l.IsThreadAgnostic);
        }

        [Fact]
        public void TripleWrapperOverNonAgnostic()
        {
            Layout l = new SimpleLayout("${uppercase:${lowercase:${rot13:${message}${threadname}}}}");
            l.Initialize(null);
            Assert.False(l.IsThreadAgnostic);
        }

        [Fact]
        public void ComplexAgnosticWithCondition()
        {
            Layout l = @"${message:padding=-10:padCharacter=Y:when='${pad:${logger}:padding=10:padCharacter=X}'=='XXXXlogger'}";
            l.Initialize(null);
            Assert.True(l.IsThreadAgnostic);
        }

        [Fact]
        public void ComplexNonAgnosticWithCondition()
        {
            Layout l = @"${message:padding=-10:padCharacter=Y:when='${pad:${threadname}:padding=10:padCharacter=X}'=='XXXXlogger'}";
            l.Initialize(null);
            Assert.False(l.IsThreadAgnostic);
        }

        [Fact]
        public void CsvThreadAgnostic()
        {
            CsvLayout l = new CsvLayout()
            {
                Columns =
                {
                    new CsvColumn("name1", "${message}"),
                    new CsvColumn("name2", "${level}"),
                    new CsvColumn("name3", "${longdate}"),
                },
            };

            l.Initialize(null);
            Assert.True(l.IsThreadAgnostic);
        }

        [Fact]
        public void CsvNonAgnostic()
        {
            CsvLayout l = new CsvLayout()
            {
                Columns =
                {
                    new CsvColumn("name1", "${message}"),
                    new CsvColumn("name2", "${threadname}"),
                    new CsvColumn("name3", "${longdate}"),
                },
            };

            l.Initialize(null);
            Assert.False(l.IsThreadAgnostic);
        }

        [Fact]
        public void CustomNotAgnosticTests()
        {
            var cif = new ConfigurationItemFactory();
            cif.RegisterType(typeof(CustomRendererNonAgnostic), string.Empty);

            Layout l = new SimpleLayout("${customNotAgnostic}", cif);

            l.Initialize(null);
            Assert.False(l.IsThreadAgnostic);
        }

        [Fact]
        public void CustomAgnosticTests()
        {
            var cif = new ConfigurationItemFactory();
            cif.RegisterType(typeof(CustomRendererAgnostic), string.Empty);

            Layout l = new SimpleLayout("${customAgnostic}", cif);

            l.Initialize(null);
            Assert.True(l.IsThreadAgnostic);
        }

        [LayoutRenderer("customNotAgnostic")]
        public class CustomRendererNonAgnostic : LayoutRenderer
        {
            protected override void Append(System.Text.StringBuilder builder, LogEventInfo logEvent)
            {
                builder.Append("custom");
            }
        }

        [LayoutRenderer("customAgnostic")]
        [ThreadAgnostic]
        public class CustomRendererAgnostic : LayoutRenderer
        {
            protected override void Append(System.Text.StringBuilder builder, LogEventInfo logEvent)
            {
                builder.Append("customAgnostic");
            }
        }
    }
}
