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

namespace NLog.WindowsRegistry.Tests
{
    using System;
    using Microsoft.Win32;
    using NLog.Config;
    using Xunit;

    public sealed class RegistryTests : IDisposable
    {
        private const string TestKey = @"Software\NLogTest";

        public RegistryTests()
        {
            LogManager.ThrowExceptions = true;

            var key = Registry.CurrentUser.CreateSubKey(TestKey);
            key.SetValue("Foo", "FooValue");
            key.SetValue(null, "UnnamedValue");

            //different keys because in 32bit the 64bits uses the 32
            RegistryKey.OpenBaseKey(RegistryHive.CurrentUser, RegistryView.Registry32).CreateSubKey("Software\\NLogTest").SetValue("view32", "reg32");
            RegistryKey.OpenBaseKey(RegistryHive.CurrentUser, RegistryView.Registry64).CreateSubKey("Software\\NLogTest").SetValue("view64", "reg64");
        }

        public void Dispose()
        {
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
            AssertLayoutRendererResult("FooValue",
            @"<nlog>
                <targets><target name='debug' type='Debug' layout='${registry:key=HKCU\\Software\\NLogTest:value=Foo}' /></targets>
                <rules>
                    <logger name='*' minlevel='Debug' writeTo='debug' />
                </rules>
            </nlog>");
        }

#if !NET35

        [Fact]
        public void RegistryNamedValueTest_hive32()
        {
            AssertLayoutRendererResult("reg32",
            @"<nlog>
                <targets><target name='debug' type='Debug' layout='${registry:key=HKCU\\Software\\NLogTest:value=view32:view=Registry32}' /></targets>
                <rules>
                    <logger name='*' minlevel='Debug' writeTo='debug' />
                </rules>
            </nlog>");
        }

        [Fact]
        public void RegistryNamedValueTest_hive64()
        {
            AssertLayoutRendererResult("reg64",
            @"<nlog>
                <targets><target name='debug' type='Debug' layout='${registry:key=HKCU\\Software\\NLogTest:value=view64:view=Registry64}' /></targets>
                <rules>
                    <logger name='*' minlevel='Debug' writeTo='debug' />
                </rules>
            </nlog>");
        }

#endif

        [Fact]
        public void RegistryNamedValueTest_forward_slash()
        {
            AssertLayoutRendererResult("FooValue",
            @"<nlog>
                <targets><target name='debug' type='Debug' layout='${registry:key=HKCU/Software/NLogTest:value=Foo}' /></targets>
                <rules>
                    <logger name='*' minlevel='Debug' writeTo='debug' />
                </rules>
            </nlog>");
        }

        [Fact]
        public void RegistryUnnamedValueTest()
        {
            AssertLayoutRendererResult("UnnamedValue",
            @"<nlog>
                <targets><target name='debug' type='Debug' layout='${registry:key=HKCU\\Software\\NLogTest}' /></targets>
                <rules>
                    <logger name='*' minlevel='Debug' writeTo='debug' />
                </rules>
            </nlog>");
        }

        [Fact]
        public void RegistryUnnamedValueTest_forward_slash()
        {
            AssertLayoutRendererResult("UnnamedValue",
            @"<nlog>
                <targets><target name='debug' type='Debug' layout='${registry:key=HKCU/Software/NLogTest}' /></targets>
                <rules>
                    <logger name='*' minlevel='Debug' writeTo='debug' />
                </rules>
            </nlog>");
        }

        [Fact]
        public void RegistryKeyNotFoundTest()
        {
            AssertLayoutRendererResult("xyz",
            @"<nlog>
                <targets><target name='debug' type='Debug' layout='${registry:key=HKCU\\Software\\NoSuchKey:defaultValue=xyz}' /></targets>
                <rules>
                    <logger name='*' minlevel='Debug' writeTo='debug' />
                </rules>
            </nlog>");
        }

        [Fact]
        public void RegistryKeyNotFoundTest_forward_slash()
        {
            AssertLayoutRendererResult("xyz",
            @"<nlog>
                <targets><target name='debug' type='Debug' layout='${registry:key=HKCU/Software/NoSuchKey:defaultValue=xyz}' /></targets>
                <rules>
                    <logger name='*' minlevel='Debug' writeTo='debug' />
                </rules>
            </nlog>");
        }

        [Fact]
        public void RegistryValueNotFoundTest()
        {
            AssertLayoutRendererResult("xyz",
            @"<nlog>
                <targets><target name='debug' type='Debug' layout='${registry:key=HKCU\\Software\\NLogTest:value=NoSuchValue:defaultValue=xyz}' /></targets>
                <rules>
                    <logger name='*' minlevel='Debug' writeTo='debug' />
                </rules>
            </nlog>");
        }

        [Fact]
        public void RegistryDefaultValueTest()
        {
            AssertLayoutRendererResult("logdefaultvalue",
            @"<nlog>
                <targets><target name='debug' type='Debug' layout='${registry:value=NOT_EXISTENT:key=HKLM/NOT_EXISTENT:defaultValue=logdefaultvalue}' /></targets>
                <rules>
                    <logger name='*' minlevel='Debug' writeTo='debug' />
                </rules>
            </nlog>");
        }

        [Fact]
        public void RegistryDefaultValueTest_with_colon()
        {
            AssertLayoutRendererResult("C:temp",
            @"<nlog>
                <targets><target name='debug' type='Debug' layout='${registry:value=NOT_EXISTENT:key=HKLM/NOT_EXISTENT:defaultValue=C\:temp}' /></targets>
                <rules>
                    <logger name='*' minlevel='Debug' writeTo='debug' />
                </rules>
            </nlog>");
        }

        [Fact]
        public void RegistryDefaultValueTest_with_slash()
        {
            AssertLayoutRendererResult("C/temp",
            @"<nlog>
                <targets><target name='debug' type='Debug' layout='${registry:value=NOT_EXISTENT:key=HKLM/NOT_EXISTENT:defaultValue=C/temp}' /></targets>
                <rules>
                    <logger name='*' minlevel='Debug' writeTo='debug' />
                </rules>
            </nlog>");
        }

        [Fact]
        public void RegistryDefaultValueTest_with_foward_slash()
        {
            AssertLayoutRendererResult("C\\temp",
            @"<nlog>
                <targets><target name='debug' type='Debug' layout='${registry:value=NOT_EXISTENT:key=HKLM/NOT_EXISTENT:defaultValue=C\\temp}' /></targets>
                <rules>
                    <logger name='*' minlevel='Debug' writeTo='debug' />
                </rules>
            </nlog>");
        }


        [Fact]
        public void RegistryDefaultValueTest_with_foward_slash2()
        {
            AssertLayoutRendererResult("C\\temp",
            @"<nlog>
                <targets><target name='debug' type='Debug' layout='${registry:value=NOT_EXISTENT:key=HKLM/NOT_EXISTENT:defaultValue=C\temp:requireEscapingSlashesInDefaultValue=false}' /></targets>
                <rules>
                    <logger name='*' minlevel='Debug' writeTo='debug' />
                </rules>
            </nlog>");
        }

        [Fact]
        public void Registry_nosubky()
        {
            AssertLayoutRendererResult("",
            @"<nlog>
                <targets><target name='debug' type='Debug' layout='${registry:key=HKEY_CURRENT_CONFIG}' /></targets>
                <rules>
                    <logger name='*' minlevel='Debug' writeTo='debug' />
                </rules>
            </nlog>");
        }

        [Fact]
        public void RegistryDefaultValueNull()
        {
            AssertLayoutRendererResult("",
            @"<nlog>
                <targets><target name='debug' type='Debug' layout='${registry:value=NOT_EXISTENT:key=HKLM/NOT_EXISTENT}' /></targets>
                <rules>
                    <logger name='*' minlevel='Debug' writeTo='debug' />
                </rules>
            </nlog>");
        }

        [Fact]
        public void RegistryTestWrongKey_no_ex()
        {
            try
            {
                LogManager.ThrowExceptions = false;
                AssertLayoutRendererResult("",
                @"<nlog>
                    <targets><target name='debug' type='Debug' layout='${registry:value=NOT_EXISTENT:key=garabageHKLM/NOT_EXISTENT:defaultValue=empty}' /></targets>
                    <rules>
                        <logger name='*' minlevel='Debug' writeTo='debug' />
                    </rules>
                </nlog>");
            }
            finally
            {
                LogManager.ThrowExceptions = true;
            }
        }

        [Fact]
        public void RegistryTestWrongKey_ex()
        {
            try
            {
                LogManager.ThrowExceptions = false;
                AssertLayoutRendererResult("",
                @"<nlog>
                    <targets><target name='debug' type='Debug' layout='${registry:value=NOT_EXISTENT:key=garabageHKLM/NOT_EXISTENT:defaultValue=empty}' /></targets>
                    <rules>
                        <logger name='*' minlevel='Debug' writeTo='debug' />
                    </rules>
                </nlog>");
            }
            finally
            {
                LogManager.ThrowExceptions = true;
            }
        }

        private static void AssertLayoutRendererResult(string expectedOuput, string xmlConfig)
        {
            var logFactory = new LogFactory().Setup().SetupExtensions(ext => ext.RegisterLayoutRenderer<NLog.LayoutRenderers.RegistryLayoutRenderer>("registry")).
                LoadConfigurationFromXml(xmlConfig).LogFactory;
            var debugTarget = logFactory.Configuration.FindTargetByName<NLog.Targets.DebugTarget>("debug");

            logFactory.GetCurrentClassLogger().Debug("zzz");

            Assert.Equal(expectedOuput, debugTarget.LastMessage);
        }
    }
}