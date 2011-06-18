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

namespace NLog.UnitTests
{
    using System;
    using System.Text;
    using NUnit.Framework;

#if !NUNIT
    using SetUp = Microsoft.VisualStudio.TestTools.UnitTesting.TestInitializeAttribute;
    using TestFixture = Microsoft.VisualStudio.TestTools.UnitTesting.TestClassAttribute;
    using Test = Microsoft.VisualStudio.TestTools.UnitTesting.TestMethodAttribute;
    using TearDown =  Microsoft.VisualStudio.TestTools.UnitTesting.TestCleanupAttribute;
#endif
    using System.Collections.Generic;
    using System.Reflection;
    using NLog.Config;

    [TestFixture]
    public class ApiTests : NLogTestBase
    {
        private Type[] allTypes;
        private Assembly nlogAssembly = typeof(LogManager).Assembly;
        private readonly Dictionary<Type, int> typeUsageCount = new Dictionary<Type, int>();

        [SetUp]
        public void Initialize()
        {
            allTypes = typeof(LogManager).Assembly.GetTypes();
        }

        [Test]
        public void PublicEnumsTest()
        {
            foreach (Type type in allTypes)
            {
                if (!type.IsPublic)
                {
                    continue;
                }

                if (type.IsEnum || type.IsInterface)
                {
                    this.typeUsageCount[type] = 0;
                }
            }

            this.typeUsageCount[typeof(IInstallable)] = 1;

            foreach (Type type in allTypes)
            {
                if (type.IsGenericTypeDefinition)
                {
                    continue;
                }

                if (type.BaseType != null)
                {
                    this.IncrementUsageCount(type.BaseType);
                }

                foreach (var iface in type.GetInterfaces())
                {
                    this.IncrementUsageCount(iface);
                }

                foreach (var method in type.GetMethods())
                {
                    if (method.IsGenericMethodDefinition)
                    {
                        continue;
                    }

                    // Console.WriteLine("  {0}", method.Name);
                    try
                    {
                        this.IncrementUsageCount(method.ReturnType);

                        foreach (var p in method.GetParameters())
                        {
                            this.IncrementUsageCount(p.ParameterType);
                        }
                    }
                    catch (Exception ex)
                    {
                        // this sometimes throws on .NET Compact Framework, but is not fatal
                        Console.WriteLine("EXCEPTION {0}", ex);
                    }
                }
            }

            var unusedTypes = new List<Type>();
            StringBuilder sb = new StringBuilder();

            foreach (var kvp in this.typeUsageCount)
            {
                if (kvp.Value == 0)
                {
                    Console.WriteLine("Type '{0}' is not used.", kvp.Key);
                    unusedTypes.Add(kvp.Key);
                    sb.Append(kvp.Key.FullName).Append("\n");
                }
            }

            Assert.AreEqual(0, unusedTypes.Count, "There are unused public types. " + sb);
        }

        private void IncrementUsageCount(Type type)
        {
            if (type.IsArray)
            {
                type = type.GetElementType();
            }

            if (type.IsGenericType && !type.IsGenericTypeDefinition)
            {
                this.IncrementUsageCount(type.GetGenericTypeDefinition());
                foreach (var parm in type.GetGenericArguments())
                {
                    this.IncrementUsageCount(parm);
                }
                return;
            }

            if (type.Assembly != nlogAssembly)
            {
                return;
            }

            if (typeUsageCount.ContainsKey(type))
            {
                typeUsageCount[type]++;
            }
        }
    }
}