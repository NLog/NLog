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

#if !SILVERLIGHT

namespace NLog.UnitTests.Targets
{
    using System;
    using System.IO;
    using System.Linq;
    using NLog.Targets;
    using System.Collections.Generic;
    using Xunit;
    using System.Reflection;
    using System.Text;

    public class TargetsContructorsTests : NLogTestBase
    {
        private const string ExampleTargetName = "TargetName";

        [Fact]
        public void NamedCtorExists()
        {
            var allTargetTypes = getAllTargetTypes();

            var errorString = new StringBuilder();
            var allCtors = allTargetTypes.Select(p => new TargetCtorEntry(p)).ToList();
            if(allCtors.Any(p => p.NamedConstructor == null))
            {
                errorString.Append("Following target types don't have a constructor like 'ctor(string name)': ")
                    .Append(string.Join(", ", allCtors.Where(p => p.NamedConstructor == null).Select(p => p.TargetType.Name)));
            }

            Assert.True(errorString.Length == 0, errorString.ToString());

            foreach(var ctor in allCtors)
            {
                try
                {
                    ctor.NamedConstructor.Invoke(new object[] { ExampleTargetName });
                }
                catch (TargetInvocationException ex)
                {
                    errorString.AppendFormat("Named constructor of {0} failed with {1}.", ctor.TargetType.Name, ex.GetType().Name).AppendLine();
                }
                catch (Exception ex)
                {
                    errorString.AppendFormat("Named constructor of {0} failed with {1}.", ctor.TargetType.Name, ex.GetType().Name).AppendLine();
                }
            }

            Assert.True(errorString.Length == 0, errorString.ToString());
        }

        [Fact]
        public void DefaultCtorExists()
        {
            var allTargetTypes = getAllTargetTypes();

            var errorString = new StringBuilder();
            var allCtors = allTargetTypes.Select(p => new TargetCtorEntry(p)).ToList();
            if (allCtors.Any(p => p.DefaultConstructor == null))
            {
                errorString.Append("Following target types don't have a default constructor: ")
                    .Append(string.Join(", ", allCtors.Where(p => p.DefaultConstructor == null).Select(p => p.TargetType.Name)));
            }

            Assert.True(errorString.Length == 0, errorString.ToString());


            foreach (var ctor in allCtors)
            {
                try
                {
                    ctor.DefaultConstructor.Invoke(new object[0]);
                }
                catch (TargetInvocationException ex)
                {
                    errorString.AppendFormat("Default constructor of {0} failed with {1}.", ctor.TargetType.Name, ex.GetType().Name).AppendLine();
                }
                catch (Exception ex)
                {
                    errorString.AppendFormat("Default constructor of {0} failed with {1}.", ctor.TargetType.Name, ex.GetType().Name).AppendLine();
                }
            }

            Assert.True(errorString.Length == 0, errorString.ToString());
        }

        [Fact]
        public void CtorsConstructEqualObjects()
        {
            var allTargetTypes = getAllTargetTypes();
            var allCtors = allTargetTypes.Select(p => new TargetCtorEntry(p)).ToList();
            var sb = new StringBuilder();

            foreach (var entry in allCtors)
            {
                var def = entry.DefaultConstructor.Invoke(new object[0]) as Target;
                def.Name = ExampleTargetName;
                var named = entry.NamedConstructor.Invoke(new object[] { ExampleTargetName }) as Target;

                Assert.Equal<Type>(def.GetType(), named.GetType());


                var properties = def.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.GetProperty);

                foreach(var property in properties)
                {
                    var defPropValue = property.GetValue(def);
                    var namedPropValue = property.GetValue(named);

                    if(Equals(defPropValue, namedPropValue) == false)
                    {
                        sb.AppendFormat("Constructors of {0} didn't yield same results for property '{1}'.", entry.TargetType.Name, property.Name).AppendLine();
                    }
                }

            }

            Assert.True(sb.Length == 0, sb.ToString());
        }


        private static IList<Type> getAllTargetTypes()
        {
            return typeof(Target).Assembly.GetTypes().Where(p => p.IsSubclassOf(typeof(Target)) && !p.IsAbstract).ToList();
        }

        private class TargetCtorEntry
        {
            public Type TargetType { get; private set; }

            public ConstructorInfo DefaultConstructor { get; private set; }

            public ConstructorInfo NamedConstructor { get; private set; }

            public bool HasBothConstructors
            {
                get { return DefaultConstructor != null && NamedConstructor != null; }
            }

            public TargetCtorEntry(Type targetType)
            {
                TargetType = targetType;
                var allCtors = targetType.GetConstructors();
                DefaultConstructor = allCtors.FirstOrDefault(p => p.GetParameters().Length == 0 && p.IsPublic);
                NamedConstructor = allCtors.FirstOrDefault(p => isNamedCtor(p));
            }

            private static bool isNamedCtor(ConstructorInfo p)
            {
                var parms = p.GetParameters();
                return parms.Length == 1 && parms[0].ParameterType == typeof(string) && parms[0].Name == "name" && p.IsPublic;
            }
        }
    }
}

#endif