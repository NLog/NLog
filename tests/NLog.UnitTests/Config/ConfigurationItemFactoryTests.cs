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

namespace NLog.UnitTests.Config
{
    using System;
    using NLog.Config;
    using NLog.Internal;
    using NLog.Targets;
    using System.Collections.Generic;
    using Xunit;

    public class ConfigurationItemFactoryTests : NLogTestBase
    {
        [Fact]
        public void ConfigurationItemFactoryDefaultTest()
        {
            var cif = new ConfigurationItemFactory();
            Assert.IsType(typeof(DebugTarget), cif.CreateInstance(typeof(DebugTarget)));
        }

        [Fact]
        public void ConfigurationItemFactorySimpleTest()
        {
            var cif = new ConfigurationItemFactory();
            cif.RegisterType(typeof(DebugTarget), string.Empty);
            var target = cif.Targets.CreateInstance("Debug") as DebugTarget;
            Assert.NotNull(target);
        }

        [Fact]
        public void ConfigurationItemFactoryUsesSuppliedDelegateToResolveObject()
        {
            var cif = new ConfigurationItemFactory();
            cif.RegisterType(typeof(DebugTarget), string.Empty);
            List<Type> resolvedTypes = new List<Type>();
            cif.CreateInstance = t => { resolvedTypes.Add(t); return FactoryHelper.CreateInstance(t); };
            Target target = cif.Targets.CreateInstance("Debug");
            Assert.NotNull(target);
            Assert.Equal(1, resolvedTypes.Count);
            Assert.Equal(typeof(DebugTarget), resolvedTypes[0]);
        }

#if !MONO_2_0
        // this is just to force reference to NLog.Extended.dll
        public Type ForceExtendedReference = typeof(MessageQueueTarget).DeclaringType;

        [Fact]
        public void ExtendedTargetTest()
        {
            var targets = ConfigurationItemFactory.Default.Targets;

            AssertInstance(targets, "MSMQ", "MessageQueueTarget");
        }

        [Fact]
        public void ExtendedLayoutRendererTest()
        {
            var layoutRenderers = ConfigurationItemFactory.Default.LayoutRenderers;

            AssertInstance(layoutRenderers, "appsetting", "AppSettingLayoutRenderer");
        }

        private static void AssertInstance<T1, T2>(INamedItemFactory<T1, T2> targets, string itemName, string expectedTypeName)
            where T1 : class
        {
            Assert.Equal(expectedTypeName, targets.CreateInstance(itemName).GetType().Name);
        }
#endif
    }


}
