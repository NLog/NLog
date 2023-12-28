// 
// Copyright (c) 2004-2021 Jaroslaw Kowalski <jaak@jkowalski.net>, Kim Christensen, Julian Verdurmen
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
    using NLog.Targets;
    using System.Collections.Generic;
    using Xunit;

    public class ConfigurationItemFactoryTests : NLogTestBase
    {
        [Fact]
        public void ConfigurationItemFactoryTargetTest()
        {
            var itemFactory = new ConfigurationItemFactory();
            itemFactory.TargetFactory.RegisterType<MemoryTarget>(nameof(MemoryTarget));
            itemFactory.TargetFactory.TryCreateInstance(nameof(MemoryTarget), out var result);
            Assert.IsType<MemoryTarget>(result);
        }

        [Fact]
        [Obsolete("Instead use LogManager.Setup().SetupExtensions(). Marked obsolete with NLog v5.2")]
        public void ConfigurationItemFactoryFailsTest()
        {
            var itemFactory = new ConfigurationItemFactory();
            var ex = Assert.ThrowsAny<Exception>(() => itemFactory.GetTargetFactory().CreateInstance("Memory-Target") as MemoryTarget);
            Assert.Contains("Memory-Target", ex.Message);

            itemFactory.GetTargetFactory().RegisterDefinition(nameof(MemoryTarget), typeof(MemoryTarget));
            var result = itemFactory.GetTargetFactory().CreateInstance("Memory-Target");
            Assert.IsType<MemoryTarget>(result);
        }

        [Fact]
        public void ConfigurationItemFactorySimpleTest()
        {
            var itemFactory = new ConfigurationItemFactory();
            itemFactory.RegisterType<DebugTarget>();
            itemFactory.TargetFactory.TryCreateInstance("Debug", out var result);
            Assert.NotNull(result);
        }

        [Fact]
        [Obsolete("Instead use LogManager.Setup().SetupExtensions(). Marked obsolete with NLog v5.2")]
        public void ConfigurationItemFactoryDefaultTest()
        {
            var itemFactory = new ConfigurationItemFactory();
            Assert.IsType<DebugTarget>(itemFactory.CreateInstance(typeof(DebugTarget)));
        }

        [Fact]
        [Obsolete("Instead use LogManager.Setup().SetupExtensions(). Marked obsolete with NLog v5.2")]
        public void ConfigurationItemFactoryUsesSuppliedDelegateToResolveObject()
        {
            var itemFactory = new ConfigurationItemFactory();
            itemFactory.RegisterType(typeof(DebugTarget), string.Empty);
            List<Type> resolvedTypes = new List<Type>();
            itemFactory.CreateInstance = t => { resolvedTypes.Add(t); return Activator.CreateInstance(t); };
            itemFactory.TargetFactory.TryCreateInstance("Debug", out var target);
            Assert.NotNull(target);
            Assert.Single(resolvedTypes);
            Assert.Equal(typeof(DebugTarget), resolvedTypes[0]);
        }
    }
}
