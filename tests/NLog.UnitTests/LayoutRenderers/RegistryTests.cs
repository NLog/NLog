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

namespace NLog.UnitTests.LayoutRenderers
{
    using System;
    using Microsoft.Win32;
    using Xunit;

    public class RegistryTests : NLogTestBase, IDisposable
    {
        private const string TestKey = @"Software\NLogTest";

        public RegistryTests()
        {
            var key = Registry.CurrentUser.CreateSubKey(TestKey);
            key.SetValue("Foo", "FooValue");
            key.SetValue(null, "UnnamedValue");

#if !NET3_5

            //different keys because in 32bit the 64bits uses the 32
            RegistryKey.OpenBaseKey(RegistryHive.CurrentUser, RegistryView.Registry32).CreateSubKey("Software\\NLogTest").SetValue("view32", "reg32");
            RegistryKey.OpenBaseKey(RegistryHive.CurrentUser, RegistryView.Registry64).CreateSubKey("Software\\NLogTest").SetValue("view64", "reg64");
#endif
        }

        public void Dispose()
        {

#if !NET3_5

            //different keys because in 32bit the 64bits uses the 32
            try
            {
                RegistryKey.OpenBaseKey(RegistryHive.CurrentUser, RegistryView.Registry32).DeleteSubKey("Software\\NLogTest");
            }
            catch (Exception)
            {
            }

            try
            {
                RegistryKey.OpenBaseKey(RegistryHive.CurrentUser, RegistryView.Registry64).DeleteSubKey("Software\\NLogTest");
            }
            catch (Exception)
            {
            }
#endif
            try
            {
                Registry.CurrentUser.DeleteSubKey(TestKey);
            }
            catch (Exception)
            {
            }
        }

        [Fact]
        public void RegistryNamedValueTest()
        {
            LogManager.Configuration = CreateConfigurationFromString(@"
            <nlog>
                <targets><target name='debug' type='Debug' layout='${registry:key=HKCU\\Software\\NLogTest:value=Foo}' /></targets>
                <rules>
                    <logger name='*' minlevel='Debug' writeTo='debug' />
                </rules>
            </nlog>");

            LogManager.GetLogger("d").Debug("zzz");
            AssertDebugLastMessage("debug", "FooValue");
        }

#if !NET3_5

        [Fact]
        public void RegistryNamedValueTest_hive32()
        {
            LogManager.Configuration = CreateConfigurationFromString(@"
            <nlog>
                <targets><target name='debug' type='Debug' layout='${registry:key=HKCU\\Software\\NLogTest:value=view32:view=Registry32}' /></targets>
                <rules>
                    <logger name='*' minlevel='Debug' writeTo='debug' />
                </rules>
            </nlog>");

            LogManager.GetLogger("d").Debug("zzz");
            AssertDebugLastMessage("debug", "reg32");
        }

        [Fact]
        public void RegistryNamedValueTest_hive64()
        {
            LogManager.Configuration = CreateConfigurationFromString(@"
            <nlog>
                <targets><target name='debug' type='Debug' layout='${registry:key=HKCU\\Software\\NLogTest:value=view64:view=Registry64}' /></targets>
                <rules>
                    <logger name='*' minlevel='Debug' writeTo='debug' />
                </rules>
            </nlog>");

            LogManager.GetLogger("d").Debug("zzz");
            AssertDebugLastMessage("debug", "reg64");
        }

#endif

        [Fact]
        public void RegistryNamedValueTest_forward_slash()
        {
            LogManager.Configuration = CreateConfigurationFromString(@"
            <nlog>
                <targets><target name='debug' type='Debug' layout='${registry:key=HKCU/Software/NLogTest:value=Foo}' /></targets>
                <rules>
                    <logger name='*' minlevel='Debug' writeTo='debug' />
                </rules>
            </nlog>");

            LogManager.GetLogger("d").Debug("zzz");
            AssertDebugLastMessage("debug", "FooValue");
        }

        [Fact]
        public void RegistryUnnamedValueTest()
        {
            LogManager.Configuration = CreateConfigurationFromString(@"
            <nlog>
                <targets><target name='debug' type='Debug' layout='${registry:key=HKCU\\Software\\NLogTest}' /></targets>
                <rules>
                    <logger name='*' minlevel='Debug' writeTo='debug' />
                </rules>
            </nlog>");

            LogManager.GetLogger("d").Debug("zzz");
            AssertDebugLastMessage("debug", "UnnamedValue");

        }


        [Fact]
        public void RegistryUnnamedValueTest_forward_slash()
        {
            LogManager.Configuration = CreateConfigurationFromString(@"
            <nlog>
                <targets><target name='debug' type='Debug' layout='${registry:key=HKCU/Software/NLogTest}' /></targets>
                <rules>
                    <logger name='*' minlevel='Debug' writeTo='debug' />
                </rules>
            </nlog>");

            LogManager.GetLogger("d").Debug("zzz");
            AssertDebugLastMessage("debug", "UnnamedValue");

        }

        [Fact]
        public void RegistryKeyNotFoundTest()
        {
            LogManager.Configuration = CreateConfigurationFromString(@"
            <nlog>
                <targets><target name='debug' type='Debug' layout='${registry:key=HKCU\\Software\\NoSuchKey:defaultValue=xyz}' /></targets>
                <rules>
                    <logger name='*' minlevel='Debug' writeTo='debug' />
                </rules>
            </nlog>");

            LogManager.GetLogger("d").Debug("zzz");
            AssertDebugLastMessage("debug", "xyz");
        }

        [Fact]
        public void RegistryKeyNotFoundTest_forward_slash()
        {
            LogManager.Configuration = CreateConfigurationFromString(@"
            <nlog>
                <targets><target name='debug' type='Debug' layout='${registry:key=HKCU/Software/NoSuchKey:defaultValue=xyz}' /></targets>
                <rules>
                    <logger name='*' minlevel='Debug' writeTo='debug' />
                </rules>
            </nlog>");

            LogManager.GetLogger("d").Debug("zzz");
            AssertDebugLastMessage("debug", "xyz");
        }

        [Fact]
        public void RegistryValueNotFoundTest()
        {
            LogManager.Configuration = CreateConfigurationFromString(@"
            <nlog>
                <targets><target name='debug' type='Debug' layout='${registry:key=HKCU\\Software\\NLogTest:value=NoSuchValue:defaultValue=xyz}' /></targets>
                <rules>
                    <logger name='*' minlevel='Debug' writeTo='debug' />
                </rules>
            </nlog>");

            LogManager.GetLogger("d").Debug("zzz");
            AssertDebugLastMessage("debug", "xyz");
        }


        [Fact]
        public void RegistryDefaultValueTest()
        {
            //example: 0003: NLog.UnitTests
            AssertLayoutRendererOutput("${registry:value=NOT_EXISTENT:key=HKLM/NOT_EXISTENT:defaultValue=logdefaultvalue}",
                "logdefaultvalue");
        }

        [Fact]
        public void RegistryDefaultValueTest_with_colon()
        {
            //example: 0003: NLog.UnitTests
            AssertLayoutRendererOutput("${registry:value=NOT_EXISTENT:key=HKLM/NOT_EXISTENT:defaultValue=C\\:temp}",
                "C:temp");
        }


        [Fact]
        public void RegistryDefaultValueTest_with_slash()
        {
            //example: 0003: NLog.UnitTests
            AssertLayoutRendererOutput("${registry:value=NOT_EXISTENT:key=HKLM/NOT_EXISTENT:defaultValue=C/temp}",
                "C/temp");
        }

        [Fact]
        public void RegistryDefaultValueTest_with_foward_slash()
        {
            //example: 0003: NLog.UnitTests
            AssertLayoutRendererOutput("${registry:value=NOT_EXISTENT:key=HKLM/NOT_EXISTENT:defaultValue=C\\\\temp}",
                "C\\temp");
        }


        [Fact]
        public void RegistryDefaultValueTest_with_foward_slash2()
        {
            //example: 0003: NLog.UnitTests
            AssertLayoutRendererOutput("${registry:value=NOT_EXISTENT:key=HKLM/NOT_EXISTENT:defaultValue=C\\temp:requireEscapingSlashesInDefaultValue=false}",
                "C\\temp");
        }

        [Fact]
        public void Registry_nosubky()
        {
            //example: 0003: NLog.UnitTests
            AssertLayoutRendererOutput("${registry:key=HKEY_CURRENT_CONFIG}", "");
        }

        [Fact]
        public void RegistryDefaultValueNull()
        {
            //example: 0003: NLog.UnitTests
            AssertLayoutRendererOutput("${registry:value=NOT_EXISTENT:key=HKLM/NOT_EXISTENT}", "");
        }

        [Fact]
        public void RegistryTestWrongKey_no_ex()
        {
            LogManager.ThrowExceptions = false;

            AssertLayoutRendererOutput("${registry:value=NOT_EXISTENT:key=garabageHKLM/NOT_EXISTENT:defaultValue=empty}", "");
        }

        [Fact(Skip = "SimpleLayout.GetFormattedMessage catches exception. Will be fixed in the future")]

        public void RegistryTestWrongKey_ex()
        {
            LogManager.ThrowExceptions = true;

            Assert.Throws<ArgumentException>(
                () => { AssertLayoutRendererOutput("${registry:value=NOT_EXISTENT:key=garabageHKLM/NOT_EXISTENT:defaultValue=empty}", ""); });


        }
    }
}

#endif