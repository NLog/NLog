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

using System.Text;
using NLog.Targets;
using Xunit;

namespace NLog.UnitTests.Targets
{
    /// <summary>
    ///     Test via <see cref="IJsonConverter" /> path
    /// </summary>
    public class DefaultJsonSerializerClassTests : NLogTestBase
    {
        private static readonly object _testSyncObject = new object();

        private interface IExcludedInterface
        {
        }

        private class ExcludedClass : IExcludedInterface
        {
            public string ExcludedString { get; set; }
            public override string ToString()
            {
                return "Skipped"; 
            }
        }

        private class IncludedClass
        {
            public string IncludedString { get; set; }
        }

        private class ContainerClass
        {
            public string S { get; set; }
            public ExcludedClass Excluded { get; set; }
            public IncludedClass Included { get; set; }
        }

        private static ContainerClass BuildSampleObject()
        {
            var testObject = new ContainerClass
            {
                S = "sample",
                Excluded = new ExcludedClass { ExcludedString = "shouldn't be serialized" },
                Included = new IncludedClass { IncludedString = "serialized" }
            };
            return testObject;
        }

        [Fact]
        public void IExcludedInterfaceSerializer_RegistersSerializeAsToString_InvokesToString()
        {
            var testObject = BuildSampleObject();

            var sb = new StringBuilder();
            var options = new JsonSerializeOptions();

            var logFactory = new LogFactory();
            logFactory.Setup().SetupSerialization(s => s.RegisterObjectTransformation<IExcludedInterface>(o => o.ToString()));

            var jsonSerializer = new DefaultJsonSerializer(logFactory.ServiceRepository);
            jsonSerializer.SerializeObject(testObject, sb, options);
            const string expectedValue =
                @"{""S"":""sample"", ""Excluded"":""Skipped"", ""Included"":{""IncludedString"":""serialized""}}";
            Assert.Equal(expectedValue, sb.ToString());
        }

        [Fact]
        public void ExcludedClassSerializer_RegistersSerializeAsToString_InvokesToString()
        {
            var testObject = BuildSampleObject();

            var sb = new StringBuilder();
            var options = new JsonSerializeOptions();

            var logFactory = new LogFactory();
            logFactory.Setup().SetupSerialization(s => s.RegisterObjectTransformation(typeof(ExcludedClass), o => o.ToString()));

            var jsonSerializer = new DefaultJsonSerializer(logFactory.ServiceRepository);
            jsonSerializer.SerializeObject(testObject, sb, options);
            const string expectedValue =
                @"{""S"":""sample"", ""Excluded"":""Skipped"", ""Included"":{""IncludedString"":""serialized""}}";
            Assert.Equal(expectedValue, sb.ToString());
        }
    }
}