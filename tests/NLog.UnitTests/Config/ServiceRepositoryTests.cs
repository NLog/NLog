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

namespace NLog.UnitTests.Config
{
    using System.IO;
    using System.Text;
    using System.Xml;
    using NLog.Targets;
    using Xunit;

    public class ServiceRepositoryTests : NLogTestBase
    {
        [Fact]
        public void SideBySideLogFactoryExternalInterfaceTest()
        {
            // Stage
            LogFactory logFactory1 = new LogFactory();
            logFactory1.ServiceRepository.RegisterType(typeof(IMyPrettyInterface), (t) => new MyPrettyImplementation() { Test = nameof(logFactory1) });

            LogFactory logFactory2 = new LogFactory();
            logFactory2.ServiceRepository.RegisterType(typeof(IMyPrettyInterface), (t) => new MyPrettyImplementation() { Test = nameof(logFactory2) });

            // Act
            var logFactory1service = logFactory1.ServiceRepository.ResolveInstance(typeof(IMyPrettyInterface));
            var logFactory2service = logFactory2.ServiceRepository.ResolveInstance(typeof(IMyPrettyInterface));

            // Assert
            Assert.Equal(nameof(logFactory1), logFactory1service.ToString());
            Assert.Equal(nameof(logFactory2), logFactory2service.ToString());
        }

        [Fact]
        public void SideBySideLogFactoryInternalInterfaceTest()
        {
            // Stage
            LogFactory logFactory1 = new LogFactory();
            InitializeLogFactoryJsonConverter(logFactory1, nameof(logFactory1), out Logger logger1, out DebugTarget target1);

            LogFactory logFactory2 = new LogFactory();
            InitializeLogFactoryJsonConverter(logFactory2, nameof(logFactory2), out Logger logger2, out DebugTarget target2);

            // Act
            logger1.Info("Hello {user}", "Kenny");
            logger2.Info("Hello {user}", "Kenny");

            // Assert
            Assert.Equal("Kenny" + "_" + nameof(logFactory1), target1.LastMessage);
            Assert.Equal("Kenny" + "_" + nameof(logFactory2), target2.LastMessage);
        }

        private static void InitializeLogFactoryJsonConverter(LogFactory logFactory, string jsonOutput, out Logger logger, out DebugTarget target)
        {
            logFactory.ServiceRepository.RegisterType(typeof(IJsonConverter), (t) => new MySimpleJsonConverter() { Test = jsonOutput });
            using (var stringReader = new StringReader(@"<nlog><targets><target type='debug' name='test' layout='${event-properties:user:format=@}'/></targets><rules><logger name='*' minLevel='Debug' writeTo='test'/></rules></nlog>"))
            {
                using (var reader = XmlReader.Create(stringReader))
                {
                    logFactory.Configuration = new NLog.Config.XmlLoggingConfiguration(reader, null, logFactory);
                }
            }
            logger = logFactory.GetLogger(nameof(logFactory));
            target = logFactory.Configuration.FindTargetByName("test") as DebugTarget;
        }

        interface IMyPrettyInterface
        {
            string Test { get; }
        }

        class MyPrettyImplementation : IMyPrettyInterface
        {
            public string Test { get; set; }
            public override string ToString()
            {
                return Test;
            }
        }

        class MySimpleJsonConverter : IJsonConverter
        {
            public string Test { get; set; }

            public bool SerializeObject(object value, StringBuilder builder)
            {
                builder.Append(string.Concat(value.ToString(), "_", Test));
                return true;
            }
        }
    }
}
