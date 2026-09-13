//
// Copyright (c) 2004-2024 Jaroslaw Kowalski <jaak@jkowalski.net>, Kim Christensen, Julian Verdurmen
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

        private sealed class ExcludedClass : IExcludedInterface
        {
            public string ExcludedString { get; set; }
            public override string ToString()
            {
                return "Skipped";
            }
        }

        private sealed class IncludedClass
        {
            public string IncludedString { get; set; }
        }

        private sealed class ContainerClass
        {
            public string S { get; set; }
            public ExcludedClass Excluded { get; set; }
            public IncludedClass Included { get; set; }
        }

        private sealed class ClassWithPublicField
        {
            public string Name { get; set; }
            public int Age;
        }

        private sealed class ClassWithOnlyFields
        {
            public string Name;
            public int Age;
        }

        private sealed class ClassWithNestedField
        {
            public IncludedClass Nested;
            public string Skipped = null;   // A null field must be left out of the JSON
        }

        private sealed class ClassWithMixedVisibility
        {
            public string Public;
            internal string Internal;
            private string _private = "hidden";
            public string ReadPrivate() => _private;
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
        public void SimpleValue_RegistersSerializeAsToString_ConvertsValue()
        {
            var logFactory = new LogFactory();
            logFactory.Setup().SetupSerialization(s => s.RegisterObjectTransformation<System.IO.MemoryStream>(o => o.Capacity));

            var testObject = new System.IO.MemoryStream(42);

            var sb = new StringBuilder();
            var options = new JsonSerializeOptions();
            var jsonSerializer = new DefaultJsonSerializer(logFactory.ServiceRepository);
            jsonSerializer.SerializeObject(testObject, sb, options);

            Assert.Equal($"{testObject.Capacity}", sb.ToString());
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
                @"{""S"":""sample"",""Excluded"":""Skipped"",""Included"":{""IncludedString"":""serialized""}}";
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
                @"{""S"":""sample"",""Excluded"":""Skipped"",""Included"":{""IncludedString"":""serialized""}}";
            Assert.Equal(expectedValue, sb.ToString());
        }

        [Fact]
        public void IncludePublicFields_Enabled_SerializesPublicFieldsAlongsideProperties()
        {
            var testObject = new ClassWithPublicField { Name = "John", Age = 42 };

            var sb = new StringBuilder();
            var options = new JsonSerializeOptions { IncludePublicFields = true };

            var jsonSerializer = new DefaultJsonSerializer(null);
            jsonSerializer.SerializeObject(testObject, sb, options);

            Assert.Equal(@"{""Name"":""John"",""Age"":42}", sb.ToString());
        }

        [Fact]
        public void IncludePublicFields_Disabled_SkipsPublicFields()
        {
            var testObject = new ClassWithPublicField { Name = "John", Age = 42 };

            var sb = new StringBuilder();
            var options = new JsonSerializeOptions { IncludePublicFields = false };

            var jsonSerializer = new DefaultJsonSerializer(null);
            jsonSerializer.SerializeObject(testObject, sb, options);

            Assert.Equal(@"{""Name"":""John""}", sb.ToString());
        }

        [Fact]
        public void IncludePublicFields_ClassWithoutProperties_SerializesFields()
        {
            var json = SerializeWithFields(new ClassWithOnlyFields { Name = "John", Age = 42 });
            Assert.Equal(@"{""Name"":""John"",""Age"":42}", json);
        }

        [Fact]
        public void IncludePublicFields_NullFieldValue_IsSkipped()
        {
            var json = SerializeWithFields(new ClassWithNestedField { Nested = new IncludedClass { IncludedString = "abc" } });
            Assert.Equal(@"{""Nested"":{""IncludedString"":""abc""}}", json);
        }

        [Fact]
        public void IncludePublicFields_NonPublicFields_AreNotSerialized()
        {
            var testObject = new ClassWithMixedVisibility { Public = "yes", Internal = "no" };
            Assert.Equal("hidden", testObject.ReadPrivate());
            Assert.Equal(@"{""Public"":""yes""}", SerializeWithFields(testObject));
        }

        [Fact]
        public void IncludePublicFields_WithoutSuppressSpaces_SeparatesWithSpace()
        {
            var sb = new StringBuilder();
            var options = new JsonSerializeOptions { IncludePublicFields = true, SuppressSpaces = false };
            new DefaultJsonSerializer(null).SerializeObject(new ClassWithOnlyFields { Name = "John", Age = 42 }, sb, options);
            Assert.Equal(@"{""Name"":""John"", ""Age"":42}", sb.ToString());
        }

        [Fact]
        public void IncludePublicFields_SameTypeWithAndWithoutOption_DoesNotShareCachedMembers()
        {
            var testObject = new ClassWithPublicField { Name = "John", Age = 42 };
            var jsonSerializer = new DefaultJsonSerializer(null);

            var withFields = new StringBuilder();
            jsonSerializer.SerializeObject(testObject, withFields, new JsonSerializeOptions { IncludePublicFields = true });
            var withoutFields = new StringBuilder();
            jsonSerializer.SerializeObject(testObject, withoutFields, new JsonSerializeOptions { IncludePublicFields = false });
            var withFieldsAgain = new StringBuilder();
            jsonSerializer.SerializeObject(testObject, withFieldsAgain, new JsonSerializeOptions { IncludePublicFields = true });

            Assert.Equal(@"{""Name"":""John"",""Age"":42}", withFields.ToString());
            Assert.Equal(@"{""Name"":""John""}", withoutFields.ToString());
            Assert.Equal(@"{""Name"":""John"",""Age"":42}", withFieldsAgain.ToString());
        }

        [Fact]
        public void IncludePublicFields_ExpandoObject_StillEnumeratesEntries()
        {
            var testObject = new System.Collections.Generic.Dictionary<string, object> { { "Name", "John" }, { "Age", 42 } };
            Assert.Equal(@"{""Name"":""John"",""Age"":42}", SerializeWithFields(testObject));
        }

        [Fact]
        public void IncludePublicFields_Exception_KeepsArtificialTypeProperty()
        {
            var json = SerializeWithFields(new System.InvalidOperationException("Oops"));
            Assert.Contains(@"""Type"":""System.InvalidOperationException""", json);
            Assert.Contains(@"""Message"":""Oops""", json);
        }

        [Fact]
        public void IncludePublicFields_TypeWithoutFields_IsStableAcrossCalls()
        {
            var testObject = new IncludedClass { IncludedString = "abc" };
            Assert.Equal(@"{""IncludedString"":""abc""}", SerializeWithFields(testObject));
            Assert.Equal(@"{""IncludedString"":""abc""}", SerializeWithFields(testObject));
        }

        private class BaseWithProperty
        {
            public string Shadowed { get; set; }
        }

        private sealed class DerivedWithField : BaseWithProperty
        {
            public new string Shadowed;
        }

        private sealed class ExceptionWithTypeField : System.Exception
        {
            public string Type = "collides";
        }
        [Fact]
        public void IncludePublicFields_FieldShadowingInheritedProperty_EmitsMemberOnce()
        {
            var testObject = new DerivedWithField { Shadowed = "field" };
            ((BaseWithProperty)testObject).Shadowed = "property";

            Assert.Equal(@"{""Shadowed"":""property""}", SerializeWithFields(testObject));
        }

        [Fact]
        public void IncludePublicFields_ExceptionWithTypeField_KeepsArtificialTypeOnly()
        {
            var json = SerializeWithFields(new ExceptionWithTypeField());

            Assert.Contains(@"""Type"":""" + typeof(ExceptionWithTypeField).ToString() + @"""", json);
            Assert.DoesNotContain(@"""Type"":""collides""", json);
        }


        private static string SerializeWithFields(object value)
        {
            var sb = new StringBuilder();
            var options = new JsonSerializeOptions { IncludePublicFields = true, SuppressSpaces = true };
            new DefaultJsonSerializer(null).SerializeObject(value, sb, options);
            return sb.ToString();
        }
    }
}
