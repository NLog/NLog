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

using System;
using System.Globalization;
using System.Xml;
using NUnit.Framework;

#if !NUNIT
    using SetUp = Microsoft.VisualStudio.TestTools.UnitTesting.TestInitializeAttribute;
    using TestFixture = Microsoft.VisualStudio.TestTools.UnitTesting.TestClassAttribute;
    using Test = Microsoft.VisualStudio.TestTools.UnitTesting.TestMethodAttribute;
    using TearDown =  Microsoft.VisualStudio.TestTools.UnitTesting.TestCleanupAttribute;
#endif
using NLog.Config;

namespace NLog.UnitTests
{
    [TestFixture]
    public class LoggerTests : NLogTestBase
    {
        [Test]
        public void TraceTest()
        {
            // test all possible overloads of the Trace() method

            for (int enabled = 0; enabled < 2; ++enabled)
            {
                if (enabled == 0)
                {
                    LogManager.Configuration = CreateConfigurationFromString(@"
                <nlog>
                    <targets><target name='debug' type='Debug' layout='${message}' /></targets>
                    <rules>
                        <logger name='*' levels='' writeTo='debug' />
                    </rules>
                </nlog>");
                }
                else
                {
                    LogManager.Configuration = CreateConfigurationFromString(@"
                <nlog>
                    <targets><target name='debug' type='Debug' layout='${message}' /></targets>
                    <rules>
                        <logger name='*' levels='Trace' writeTo='debug' />
                    </rules>
                </nlog>");
                }

                Logger logger = LogManager.GetLogger("A");

                logger.Trace("message");
                if (enabled == 1) AssertDebugLastMessage("debug", "message");

                logger.Trace("message{0}", (ulong)1);
                if (enabled == 1) AssertDebugLastMessage("debug", "message1");

                logger.Trace(CultureInfo.InvariantCulture, "message{0}", (ulong)2);
                if (enabled == 1) AssertDebugLastMessage("debug", "message2");

                logger.Trace("message{0}", (long)1);
                if (enabled == 1) AssertDebugLastMessage("debug", "message1");

                logger.Trace(CultureInfo.InvariantCulture, "message{0}", (long)2);
                if (enabled == 1) AssertDebugLastMessage("debug", "message2");

                logger.Trace("message{0}", (uint)1);
                if (enabled == 1) AssertDebugLastMessage("debug", "message1");

                logger.Trace(CultureInfo.InvariantCulture, "message{0}", (uint)2);
                if (enabled == 1) AssertDebugLastMessage("debug", "message2");

                logger.Trace("message{0}", (int)1);
                if (enabled == 1) AssertDebugLastMessage("debug", "message1");

                logger.Trace(CultureInfo.InvariantCulture, "message{0}", (int)2);
                if (enabled == 1) AssertDebugLastMessage("debug", "message2");

                logger.Trace("message{0}", (ushort)1);
                if (enabled == 1) AssertDebugLastMessage("debug", "message1");

                logger.Trace(CultureInfo.InvariantCulture, "message{0}", (ushort)2);
                if (enabled == 1) AssertDebugLastMessage("debug", "message2");

                logger.Trace("message{0}", (sbyte)1);
                if (enabled == 1) AssertDebugLastMessage("debug", "message1");

                logger.Trace(CultureInfo.InvariantCulture, "message{0}", (sbyte)2);
                if (enabled == 1) AssertDebugLastMessage("debug", "message2");

                logger.Trace("message{0}", this);
                if (enabled == 1) AssertDebugLastMessage("debug", "messageobject-to-string");

                logger.Trace(CultureInfo.InvariantCulture, "message{0}", this);
                if (enabled == 1) AssertDebugLastMessage("debug", "messageobject-to-string");

                logger.Trace("message{0}", (short)1);
                if (enabled == 1) AssertDebugLastMessage("debug", "message1");

                logger.Trace(CultureInfo.InvariantCulture, "message{0}", (short)2);
                if (enabled == 1) AssertDebugLastMessage("debug", "message2");

                logger.Trace("message{0}", (byte)1);
                if (enabled == 1) AssertDebugLastMessage("debug", "message1");

                logger.Trace(CultureInfo.InvariantCulture, "message{0}", (byte)2);
                if (enabled == 1) AssertDebugLastMessage("debug", "message2");

                logger.Trace("message{0}", 'c');
                if (enabled == 1) AssertDebugLastMessage("debug", "messagec");

                logger.Trace(CultureInfo.InvariantCulture, "message{0}", 'd');
                if (enabled == 1) AssertDebugLastMessage("debug", "messaged");

                logger.Trace("message{0}", "ddd");
                if (enabled == 1) AssertDebugLastMessage("debug", "messageddd");

                logger.Trace(CultureInfo.InvariantCulture, "message{0}", "eee");
                if (enabled == 1) AssertDebugLastMessage("debug", "messageeee");

                logger.Trace("message{0}{1}", "ddd", 1);
                if (enabled == 1) AssertDebugLastMessage("debug", "messageddd1");

                logger.Trace(CultureInfo.InvariantCulture, "message{0}{1}", "eee", 2);
                if (enabled == 1) AssertDebugLastMessage("debug", "messageeee2");

                logger.Trace("message{0}{1}{2}", "ddd", 1, "eee");
                if (enabled == 1) AssertDebugLastMessage("debug", "messageddd1eee");

                logger.Trace(CultureInfo.InvariantCulture, "message{0}{1}{2}", "eee", 2, "fff");
                if (enabled == 1) AssertDebugLastMessage("debug", "messageeee2fff");

                logger.Trace("message{0}{1}{2}{3}", "eee", 2, "fff", "ggg");
                if (enabled == 1) AssertDebugLastMessage("debug", "messageeee2fffggg");

                logger.Trace("message{0}", true);
                if (enabled == 1) AssertDebugLastMessage("debug", "messageTrue");

                logger.Trace(CultureInfo.InvariantCulture, "message{0}", false);
                if (enabled == 1) AssertDebugLastMessage("debug", "messageFalse");

                logger.Trace(CultureInfo.InvariantCulture, "message{0}", (float)2.5);
                if (enabled == 1) AssertDebugLastMessage("debug", "message2.5");

                logger.Trace(CultureInfo.InvariantCulture, "message{0}", (double)2.5);
                if (enabled == 1) AssertDebugLastMessage("debug", "message2.5");

#if !NET_CF
                CultureInfo oldCulture = System.Threading.Thread.CurrentThread.CurrentCulture;
                System.Threading.Thread.CurrentThread.CurrentCulture = new CultureInfo("pl-PL");

                logger.Trace("message{0}", (float)1.5);
                if (enabled == 1) AssertDebugLastMessage("debug", "message1,5");

                logger.Trace((double)1.5);
                if (enabled == 1) AssertDebugLastMessage("debug", "1,5");

                logger.Trace(CultureInfo.InvariantCulture, (double)1.5);
                if (enabled == 1) AssertDebugLastMessage("debug", "1.5");

                logger.Trace("message{0}", (double)1.5);
                if (enabled == 1) AssertDebugLastMessage("debug", "message1,5");

                logger.Trace("message{0}", (decimal)1.5);
                if (enabled == 1) AssertDebugLastMessage("debug", "message1,5");
                
                System.Threading.Thread.CurrentThread.CurrentCulture = oldCulture;
#endif

                logger.Trace(CultureInfo.InvariantCulture, "message{0}", (decimal)2.5);
                if (enabled == 1) AssertDebugLastMessage("debug", "message2.5");

                logger.TraceException("message", new Exception("test"));
                if (enabled == 1) AssertDebugLastMessage("debug", "message");

                logger.Trace(delegate { return "message from lambda"; });
                if (enabled == 1) AssertDebugLastMessage("debug", "message from lambda");

                if (enabled == 0)
                    AssertDebugCounter("debug", 0);
            }
        }

        [Test]
        public void DebugTest()
        {
            // test all possible overloads of the Debug() method

            for (int enabled = 0; enabled < 2; ++enabled)
            {
                if (enabled == 0)
                {
                    LogManager.Configuration = CreateConfigurationFromString(@"
                <nlog>
                    <targets><target name='debug' type='Debug' layout='${message}' /></targets>
                    <rules>
                        <logger name='*' levels='' writeTo='debug' />
                    </rules>
                </nlog>");
                }
                else
                {
                    LogManager.Configuration = CreateConfigurationFromString(@"
                <nlog>
                    <targets><target name='debug' type='Debug' layout='${message}' /></targets>
                    <rules>
                        <logger name='*' levels='Debug' writeTo='debug' />
                    </rules>
                </nlog>");
                }

                Logger logger = LogManager.GetLogger("A");

                logger.Debug("message");
                if (enabled == 1) AssertDebugLastMessage("debug", "message");

                logger.Debug("message{0}", (ulong)1);
                if (enabled == 1) AssertDebugLastMessage("debug", "message1");

                logger.Debug(CultureInfo.InvariantCulture, "message{0}", (ulong)2);
                if (enabled == 1) AssertDebugLastMessage("debug", "message2");

                logger.Debug("message{0}", (long)1);
                if (enabled == 1) AssertDebugLastMessage("debug", "message1");

                logger.Debug(CultureInfo.InvariantCulture, "message{0}", (long)2);
                if (enabled == 1) AssertDebugLastMessage("debug", "message2");

                logger.Debug("message{0}", (uint)1);
                if (enabled == 1) AssertDebugLastMessage("debug", "message1");

                logger.Debug(CultureInfo.InvariantCulture, "message{0}", (uint)2);
                if (enabled == 1) AssertDebugLastMessage("debug", "message2");

                logger.Debug("message{0}", (int)1);
                if (enabled == 1) AssertDebugLastMessage("debug", "message1");

                logger.Debug(CultureInfo.InvariantCulture, "message{0}", (int)2);
                if (enabled == 1) AssertDebugLastMessage("debug", "message2");

                logger.Debug("message{0}", (ushort)1);
                if (enabled == 1) AssertDebugLastMessage("debug", "message1");

                logger.Debug(CultureInfo.InvariantCulture, "message{0}", (ushort)2);
                if (enabled == 1) AssertDebugLastMessage("debug", "message2");

                logger.Debug("message{0}", (sbyte)1);
                if (enabled == 1) AssertDebugLastMessage("debug", "message1");

                logger.Debug(CultureInfo.InvariantCulture, "message{0}", (sbyte)2);
                if (enabled == 1) AssertDebugLastMessage("debug", "message2");

                logger.Debug("message{0}", this);
                if (enabled == 1) AssertDebugLastMessage("debug", "messageobject-to-string");

                logger.Debug(CultureInfo.InvariantCulture, "message{0}", this);
                if (enabled == 1) AssertDebugLastMessage("debug", "messageobject-to-string");

                logger.Debug("message{0}", (short)1);
                if (enabled == 1) AssertDebugLastMessage("debug", "message1");

                logger.Debug(CultureInfo.InvariantCulture, "message{0}", (short)2);
                if (enabled == 1) AssertDebugLastMessage("debug", "message2");

                logger.Debug("message{0}", (byte)1);
                if (enabled == 1) AssertDebugLastMessage("debug", "message1");

                logger.Debug(CultureInfo.InvariantCulture, "message{0}", (byte)2);
                if (enabled == 1) AssertDebugLastMessage("debug", "message2");

                logger.Debug("message{0}", 'c');
                if (enabled == 1) AssertDebugLastMessage("debug", "messagec");

                logger.Debug(CultureInfo.InvariantCulture, "message{0}", 'd');
                if (enabled == 1) AssertDebugLastMessage("debug", "messaged");

                logger.Debug("message{0}", "ddd");
                if (enabled == 1) AssertDebugLastMessage("debug", "messageddd");

                logger.Debug(CultureInfo.InvariantCulture, "message{0}", "eee");
                if (enabled == 1) AssertDebugLastMessage("debug", "messageeee");

                logger.Debug("message{0}{1}", "ddd", 1);
                if (enabled == 1) AssertDebugLastMessage("debug", "messageddd1");

                logger.Debug(CultureInfo.InvariantCulture, "message{0}{1}", "eee", 2);
                if (enabled == 1) AssertDebugLastMessage("debug", "messageeee2");

                logger.Debug("message{0}{1}{2}", "ddd", 1, "eee");
                if (enabled == 1) AssertDebugLastMessage("debug", "messageddd1eee");

                logger.Debug(CultureInfo.InvariantCulture, "message{0}{1}{2}", "eee", 2, "fff");
                if (enabled == 1) AssertDebugLastMessage("debug", "messageeee2fff");

                logger.Debug("message{0}{1}{2}{3}", "eee", 2, "fff", "ggg");
                if (enabled == 1) AssertDebugLastMessage("debug", "messageeee2fffggg");

                logger.Debug("message{0}", true);
                if (enabled == 1) AssertDebugLastMessage("debug", "messageTrue");

                logger.Debug(CultureInfo.InvariantCulture, "message{0}", false);
                if (enabled == 1) AssertDebugLastMessage("debug", "messageFalse");

                logger.Debug(CultureInfo.InvariantCulture, "message{0}", (float)2.5);
                if (enabled == 1) AssertDebugLastMessage("debug", "message2.5");

                logger.Debug(CultureInfo.InvariantCulture, "message{0}", (double)2.5);
                if (enabled == 1) AssertDebugLastMessage("debug", "message2.5");

#if !NET_CF
                CultureInfo oldCulture = System.Threading.Thread.CurrentThread.CurrentCulture;
                System.Threading.Thread.CurrentThread.CurrentCulture = new CultureInfo("pl-PL");

                logger.Debug("message{0}", (float)1.5);
                if (enabled == 1) AssertDebugLastMessage("debug", "message1,5");

                logger.Debug((double)1.5);
                if (enabled == 1) AssertDebugLastMessage("debug", "1,5");

                logger.Debug(CultureInfo.InvariantCulture, (double)1.5);
                if (enabled == 1) AssertDebugLastMessage("debug", "1.5");

                logger.Debug("message{0}", (double)1.5);
                if (enabled == 1) AssertDebugLastMessage("debug", "message1,5");

                logger.Debug("message{0}", (decimal)1.5);
                if (enabled == 1) AssertDebugLastMessage("debug", "message1,5");

                System.Threading.Thread.CurrentThread.CurrentCulture = oldCulture;
#endif
                logger.Debug(CultureInfo.InvariantCulture, "message{0}", (decimal)2.5);
                if (enabled == 1) AssertDebugLastMessage("debug", "message2.5");

                logger.DebugException("message", new Exception("test"));
                if (enabled == 1) AssertDebugLastMessage("debug", "message");

                logger.Debug(delegate { return "message from lambda"; });
                if (enabled == 1) AssertDebugLastMessage("debug", "message from lambda");

                if (enabled == 0)
                    AssertDebugCounter("debug", 0);
            }
        }

        [Test]
        public void InfoTest()
        {
            // test all possible overloads of the Info() method

            for (int enabled = 0; enabled < 2; ++enabled)
            {
                if (enabled == 0)
                {
                    LogManager.Configuration = CreateConfigurationFromString(@"
                <nlog>
                    <targets><target name='debug' type='Debug' layout='${message}' /></targets>
                    <rules>
                        <logger name='*' levels='' writeTo='debug' />
                    </rules>
                </nlog>");
                }
                else
                {
                    LogManager.Configuration = CreateConfigurationFromString(@"
                <nlog>
                    <targets><target name='debug' type='Debug' layout='${message}' /></targets>
                    <rules>
                        <logger name='*' levels='Info' writeTo='debug' />
                    </rules>
                </nlog>");
                }

                Logger logger = LogManager.GetLogger("A");

                logger.Info("message");
                if (enabled == 1) AssertDebugLastMessage("debug", "message");

                logger.Info("message{0}", (ulong)1);
                if (enabled == 1) AssertDebugLastMessage("debug", "message1");

                logger.Info(CultureInfo.InvariantCulture, "message{0}", (ulong)2);
                if (enabled == 1) AssertDebugLastMessage("debug", "message2");

                logger.Info("message{0}", (long)1);
                if (enabled == 1) AssertDebugLastMessage("debug", "message1");

                logger.Info(CultureInfo.InvariantCulture, "message{0}", (long)2);
                if (enabled == 1) AssertDebugLastMessage("debug", "message2");

                logger.Info("message{0}", (uint)1);
                if (enabled == 1) AssertDebugLastMessage("debug", "message1");

                logger.Info(CultureInfo.InvariantCulture, "message{0}", (uint)2);
                if (enabled == 1) AssertDebugLastMessage("debug", "message2");

                logger.Info("message{0}", (int)1);
                if (enabled == 1) AssertDebugLastMessage("debug", "message1");

                logger.Info(CultureInfo.InvariantCulture, "message{0}", (int)2);
                if (enabled == 1) AssertDebugLastMessage("debug", "message2");

                logger.Info("message{0}", (ushort)1);
                if (enabled == 1) AssertDebugLastMessage("debug", "message1");

                logger.Info(CultureInfo.InvariantCulture, "message{0}", (ushort)2);
                if (enabled == 1) AssertDebugLastMessage("debug", "message2");

                logger.Info("message{0}", (sbyte)1);
                if (enabled == 1) AssertDebugLastMessage("debug", "message1");

                logger.Info(CultureInfo.InvariantCulture, "message{0}", (sbyte)2);
                if (enabled == 1) AssertDebugLastMessage("debug", "message2");

                logger.Info("message{0}", this);
                if (enabled == 1) AssertDebugLastMessage("debug", "messageobject-to-string");

                logger.Info(CultureInfo.InvariantCulture, "message{0}", this);
                if (enabled == 1) AssertDebugLastMessage("debug", "messageobject-to-string");

                logger.Info("message{0}", (short)1);
                if (enabled == 1) AssertDebugLastMessage("debug", "message1");

                logger.Info(CultureInfo.InvariantCulture, "message{0}", (short)2);
                if (enabled == 1) AssertDebugLastMessage("debug", "message2");

                logger.Info("message{0}", (byte)1);
                if (enabled == 1) AssertDebugLastMessage("debug", "message1");

                logger.Info(CultureInfo.InvariantCulture, "message{0}", (byte)2);
                if (enabled == 1) AssertDebugLastMessage("debug", "message2");

                logger.Info("message{0}", 'c');
                if (enabled == 1) AssertDebugLastMessage("debug", "messagec");

                logger.Info(CultureInfo.InvariantCulture, "message{0}", 'd');
                if (enabled == 1) AssertDebugLastMessage("debug", "messaged");

                logger.Info("message{0}", "ddd");
                if (enabled == 1) AssertDebugLastMessage("debug", "messageddd");

                logger.Info(CultureInfo.InvariantCulture, "message{0}", "eee");
                if (enabled == 1) AssertDebugLastMessage("debug", "messageeee");

                logger.Info("message{0}{1}", "ddd", 1);
                if (enabled == 1) AssertDebugLastMessage("debug", "messageddd1");

                logger.Info(CultureInfo.InvariantCulture, "message{0}{1}", "eee", 2);
                if (enabled == 1) AssertDebugLastMessage("debug", "messageeee2");

                logger.Info("message{0}{1}{2}", "ddd", 1, "eee");
                if (enabled == 1) AssertDebugLastMessage("debug", "messageddd1eee");

                logger.Info(CultureInfo.InvariantCulture, "message{0}{1}{2}", "eee", 2, "fff");
                if (enabled == 1) AssertDebugLastMessage("debug", "messageeee2fff");

                logger.Info("message{0}{1}{2}{3}", "eee", 2, "fff", "ggg");
                if (enabled == 1) AssertDebugLastMessage("debug", "messageeee2fffggg");

                logger.Info("message{0}", true);
                if (enabled == 1) AssertDebugLastMessage("debug", "messageTrue");

                logger.Info(CultureInfo.InvariantCulture, "message{0}", false);
                if (enabled == 1) AssertDebugLastMessage("debug", "messageFalse");

                logger.Info(CultureInfo.InvariantCulture, "message{0}", (float)2.5);
                if (enabled == 1) AssertDebugLastMessage("debug", "message2.5");

                logger.Info(CultureInfo.InvariantCulture, "message{0}", (double)2.5);
                if (enabled == 1) AssertDebugLastMessage("debug", "message2.5");

#if !NET_CF
                CultureInfo oldCulture = System.Threading.Thread.CurrentThread.CurrentCulture;
                System.Threading.Thread.CurrentThread.CurrentCulture = new CultureInfo("pl-PL");

                logger.Info("message{0}", (float)1.5);
                if (enabled == 1) AssertDebugLastMessage("debug", "message1,5");

                logger.Info((double)1.5);
                if (enabled == 1) AssertDebugLastMessage("debug", "1,5");

                logger.Info(CultureInfo.InvariantCulture, (double)1.5);
                if (enabled == 1) AssertDebugLastMessage("debug", "1.5");

                logger.Info("message{0}", (double)1.5);
                if (enabled == 1) AssertDebugLastMessage("debug", "message1,5");

                logger.Info("message{0}", (decimal)1.5);
                if (enabled == 1) AssertDebugLastMessage("debug", "message1,5");

                System.Threading.Thread.CurrentThread.CurrentCulture = oldCulture;
#endif

                logger.Info(CultureInfo.InvariantCulture, "message{0}", (decimal)2.5);
                if (enabled == 1) AssertDebugLastMessage("debug", "message2.5");

                logger.InfoException("message", new Exception("test"));
                if (enabled == 1) AssertDebugLastMessage("debug", "message");

                logger.Info(delegate { return "message from lambda"; });
                if (enabled == 1) AssertDebugLastMessage("debug", "message from lambda");

                if (enabled == 0)
                    AssertDebugCounter("debug", 0);
            }
        }

        [Test]
        public void WarnTest()
        {
            // test all possible overloads of the Warn() method

            for (int enabled = 0; enabled < 2; ++enabled)
            {
                if (enabled == 0)
                {
                    LogManager.Configuration = CreateConfigurationFromString(@"
                <nlog>
                    <targets><target name='debug' type='Debug' layout='${message}' /></targets>
                    <rules>
                        <logger name='*' levels='' writeTo='debug' />
                    </rules>
                </nlog>");
                }
                else
                {
                    LogManager.Configuration = CreateConfigurationFromString(@"
                <nlog>
                    <targets><target name='debug' type='Debug' layout='${message}' /></targets>
                    <rules>
                        <logger name='*' levels='Warn' writeTo='debug' />
                    </rules>
                </nlog>");
                }

                Logger logger = LogManager.GetLogger("A");

                logger.Warn("message");
                if (enabled == 1) AssertDebugLastMessage("debug", "message");

                logger.Warn("message{0}", (ulong)1);
                if (enabled == 1) AssertDebugLastMessage("debug", "message1");

                logger.Warn(CultureInfo.InvariantCulture, "message{0}", (ulong)2);
                if (enabled == 1) AssertDebugLastMessage("debug", "message2");

                logger.Warn("message{0}", (long)1);
                if (enabled == 1) AssertDebugLastMessage("debug", "message1");

                logger.Warn(CultureInfo.InvariantCulture, "message{0}", (long)2);
                if (enabled == 1) AssertDebugLastMessage("debug", "message2");

                logger.Warn("message{0}", (uint)1);
                if (enabled == 1) AssertDebugLastMessage("debug", "message1");

                logger.Warn(CultureInfo.InvariantCulture, "message{0}", (uint)2);
                if (enabled == 1) AssertDebugLastMessage("debug", "message2");

                logger.Warn("message{0}", (int)1);
                if (enabled == 1) AssertDebugLastMessage("debug", "message1");

                logger.Warn(CultureInfo.InvariantCulture, "message{0}", (int)2);
                if (enabled == 1) AssertDebugLastMessage("debug", "message2");

                logger.Warn("message{0}", (ushort)1);
                if (enabled == 1) AssertDebugLastMessage("debug", "message1");

                logger.Warn(CultureInfo.InvariantCulture, "message{0}", (ushort)2);
                if (enabled == 1) AssertDebugLastMessage("debug", "message2");

                logger.Warn("message{0}", (sbyte)1);
                if (enabled == 1) AssertDebugLastMessage("debug", "message1");

                logger.Warn(CultureInfo.InvariantCulture, "message{0}", (sbyte)2);
                if (enabled == 1) AssertDebugLastMessage("debug", "message2");

                logger.Warn("message{0}", this);
                if (enabled == 1) AssertDebugLastMessage("debug", "messageobject-to-string");

                logger.Warn(CultureInfo.InvariantCulture, "message{0}", this);
                if (enabled == 1) AssertDebugLastMessage("debug", "messageobject-to-string");

                logger.Warn("message{0}", (short)1);
                if (enabled == 1) AssertDebugLastMessage("debug", "message1");

                logger.Warn(CultureInfo.InvariantCulture, "message{0}", (short)2);
                if (enabled == 1) AssertDebugLastMessage("debug", "message2");

                logger.Warn("message{0}", (byte)1);
                if (enabled == 1) AssertDebugLastMessage("debug", "message1");

                logger.Warn(CultureInfo.InvariantCulture, "message{0}", (byte)2);
                if (enabled == 1) AssertDebugLastMessage("debug", "message2");

                logger.Warn("message{0}", 'c');
                if (enabled == 1) AssertDebugLastMessage("debug", "messagec");

                logger.Warn(CultureInfo.InvariantCulture, "message{0}", 'd');
                if (enabled == 1) AssertDebugLastMessage("debug", "messaged");

                logger.Warn("message{0}", "ddd");
                if (enabled == 1) AssertDebugLastMessage("debug", "messageddd");

                logger.Warn(CultureInfo.InvariantCulture, "message{0}", "eee");
                if (enabled == 1) AssertDebugLastMessage("debug", "messageeee");

                logger.Warn("message{0}{1}", "ddd", 1);
                if (enabled == 1) AssertDebugLastMessage("debug", "messageddd1");

                logger.Warn(CultureInfo.InvariantCulture, "message{0}{1}", "eee", 2);
                if (enabled == 1) AssertDebugLastMessage("debug", "messageeee2");

                logger.Warn("message{0}{1}{2}", "ddd", 1, "eee");
                if (enabled == 1) AssertDebugLastMessage("debug", "messageddd1eee");

                logger.Warn(CultureInfo.InvariantCulture, "message{0}{1}{2}", "eee", 2, "fff");
                if (enabled == 1) AssertDebugLastMessage("debug", "messageeee2fff");

                logger.Warn("message{0}{1}{2}{3}", "eee", 2, "fff", "ggg");
                if (enabled == 1) AssertDebugLastMessage("debug", "messageeee2fffggg");

                logger.Warn("message{0}", true);
                if (enabled == 1) AssertDebugLastMessage("debug", "messageTrue");

                logger.Warn(CultureInfo.InvariantCulture, "message{0}", false);
                if (enabled == 1) AssertDebugLastMessage("debug", "messageFalse");

                logger.Warn(CultureInfo.InvariantCulture, "message{0}", (float)2.5);
                if (enabled == 1) AssertDebugLastMessage("debug", "message2.5");

                logger.Warn(CultureInfo.InvariantCulture, "message{0}", (double)2.5);
                if (enabled == 1) AssertDebugLastMessage("debug", "message2.5");

#if !NET_CF
                CultureInfo oldCulture = System.Threading.Thread.CurrentThread.CurrentCulture;
                System.Threading.Thread.CurrentThread.CurrentCulture = new CultureInfo("pl-PL");

                logger.Warn("message{0}", (float)1.5);
                if (enabled == 1) AssertDebugLastMessage("debug", "message1,5");

                logger.Warn((double)1.5);
                if (enabled == 1) AssertDebugLastMessage("debug", "1,5");

                logger.Warn(CultureInfo.InvariantCulture, (double)1.5);
                if (enabled == 1) AssertDebugLastMessage("debug", "1.5");

                logger.Warn("message{0}", (double)1.5);
                if (enabled == 1) AssertDebugLastMessage("debug", "message1,5");

                logger.Warn("message{0}", (decimal)1.5);
                if (enabled == 1) AssertDebugLastMessage("debug", "message1,5");

                System.Threading.Thread.CurrentThread.CurrentCulture = oldCulture;
#endif

                logger.Warn(CultureInfo.InvariantCulture, "message{0}", (decimal)2.5);
                if (enabled == 1) AssertDebugLastMessage("debug", "message2.5");

                logger.WarnException("message", new Exception("test"));
                if (enabled == 1) AssertDebugLastMessage("debug", "message");

                logger.Warn(delegate { return "message from lambda"; });
                if (enabled == 1) AssertDebugLastMessage("debug", "message from lambda");

                if (enabled == 0)
                    AssertDebugCounter("debug", 0);
            }
        }

        [Test]
        public void ErrorTest()
        {
            // test all possible overloads of the Error() method

            for (int enabled = 0; enabled < 2; ++enabled)
            {
                if (enabled == 0)
                {
                    LogManager.Configuration = CreateConfigurationFromString(@"
                <nlog>
                    <targets><target name='debug' type='Debug' layout='${message}' /></targets>
                    <rules>
                        <logger name='*' levels='' writeTo='debug' />
                    </rules>
                </nlog>");
                }
                else
                {
                    LogManager.Configuration = CreateConfigurationFromString(@"
                <nlog>
                    <targets><target name='debug' type='Debug' layout='${message}' /></targets>
                    <rules>
                        <logger name='*' levels='Error' writeTo='debug' />
                    </rules>
                </nlog>");
                }

                Logger logger = LogManager.GetLogger("A");

                logger.Error("message");
                if (enabled == 1) AssertDebugLastMessage("debug", "message");

                logger.Error("message{0}", (ulong)1);
                if (enabled == 1) AssertDebugLastMessage("debug", "message1");

                logger.Error(CultureInfo.InvariantCulture, "message{0}", (ulong)2);
                if (enabled == 1) AssertDebugLastMessage("debug", "message2");

                logger.Error("message{0}", (long)1);
                if (enabled == 1) AssertDebugLastMessage("debug", "message1");

                logger.Error(CultureInfo.InvariantCulture, "message{0}", (long)2);
                if (enabled == 1) AssertDebugLastMessage("debug", "message2");

                logger.Error("message{0}", (uint)1);
                if (enabled == 1) AssertDebugLastMessage("debug", "message1");

                logger.Error(CultureInfo.InvariantCulture, "message{0}", (uint)2);
                if (enabled == 1) AssertDebugLastMessage("debug", "message2");

                logger.Error("message{0}", (int)1);
                if (enabled == 1) AssertDebugLastMessage("debug", "message1");

                logger.Error(CultureInfo.InvariantCulture, "message{0}", (int)2);
                if (enabled == 1) AssertDebugLastMessage("debug", "message2");

                logger.Error("message{0}", (ushort)1);
                if (enabled == 1) AssertDebugLastMessage("debug", "message1");

                logger.Error(CultureInfo.InvariantCulture, "message{0}", (ushort)2);
                if (enabled == 1) AssertDebugLastMessage("debug", "message2");

                logger.Error("message{0}", (sbyte)1);
                if (enabled == 1) AssertDebugLastMessage("debug", "message1");

                logger.Error(CultureInfo.InvariantCulture, "message{0}", (sbyte)2);
                if (enabled == 1) AssertDebugLastMessage("debug", "message2");

                logger.Error("message{0}", this);
                if (enabled == 1) AssertDebugLastMessage("debug", "messageobject-to-string");

                logger.Error(CultureInfo.InvariantCulture, "message{0}", this);
                if (enabled == 1) AssertDebugLastMessage("debug", "messageobject-to-string");

                logger.Error("message{0}", (short)1);
                if (enabled == 1) AssertDebugLastMessage("debug", "message1");

                logger.Error(CultureInfo.InvariantCulture, "message{0}", (short)2);
                if (enabled == 1) AssertDebugLastMessage("debug", "message2");

                logger.Error("message{0}", (byte)1);
                if (enabled == 1) AssertDebugLastMessage("debug", "message1");

                logger.Error(CultureInfo.InvariantCulture, "message{0}", (byte)2);
                if (enabled == 1) AssertDebugLastMessage("debug", "message2");

                logger.Error("message{0}", 'c');
                if (enabled == 1) AssertDebugLastMessage("debug", "messagec");

                logger.Error(CultureInfo.InvariantCulture, "message{0}", 'd');
                if (enabled == 1) AssertDebugLastMessage("debug", "messaged");

                logger.Error("message{0}", "ddd");
                if (enabled == 1) AssertDebugLastMessage("debug", "messageddd");

                logger.Error(CultureInfo.InvariantCulture, "message{0}", "eee");
                if (enabled == 1) AssertDebugLastMessage("debug", "messageeee");

                logger.Error("message{0}{1}", "ddd", 1);
                if (enabled == 1) AssertDebugLastMessage("debug", "messageddd1");

                logger.Error(CultureInfo.InvariantCulture, "message{0}{1}", "eee", 2);
                if (enabled == 1) AssertDebugLastMessage("debug", "messageeee2");

                logger.Error("message{0}{1}{2}", "ddd", 1, "eee");
                if (enabled == 1) AssertDebugLastMessage("debug", "messageddd1eee");

                logger.Error(CultureInfo.InvariantCulture, "message{0}{1}{2}", "eee", 2, "fff");
                if (enabled == 1) AssertDebugLastMessage("debug", "messageeee2fff");

                logger.Error("message{0}{1}{2}{3}", "eee", 2, "fff", "ggg");
                if (enabled == 1) AssertDebugLastMessage("debug", "messageeee2fffggg");

                logger.Error("message{0}", true);
                if (enabled == 1) AssertDebugLastMessage("debug", "messageTrue");

                logger.Error(CultureInfo.InvariantCulture, "message{0}", false);
                if (enabled == 1) AssertDebugLastMessage("debug", "messageFalse");

                logger.Error(CultureInfo.InvariantCulture, "message{0}", (float)2.5);
                if (enabled == 1) AssertDebugLastMessage("debug", "message2.5");

                logger.Error(CultureInfo.InvariantCulture, "message{0}", (double)2.5);
                if (enabled == 1) AssertDebugLastMessage("debug", "message2.5");

#if !NET_CF
                CultureInfo oldCulture = System.Threading.Thread.CurrentThread.CurrentCulture;
                System.Threading.Thread.CurrentThread.CurrentCulture = new CultureInfo("pl-PL");

                logger.Error("message{0}", (float)1.5);
                if (enabled == 1) AssertDebugLastMessage("debug", "message1,5");

                logger.Error((double)1.5);
                if (enabled == 1) AssertDebugLastMessage("debug", "1,5");

                logger.Error(CultureInfo.InvariantCulture, (double)1.5);
                if (enabled == 1) AssertDebugLastMessage("debug", "1.5");

                logger.Error("message{0}", (double)1.5);
                if (enabled == 1) AssertDebugLastMessage("debug", "message1,5");

                System.Threading.Thread.CurrentThread.CurrentCulture = oldCulture;
#endif

                logger.Error(CultureInfo.InvariantCulture, "message{0}", (decimal)2.5);
                if (enabled == 1) AssertDebugLastMessage("debug", "message2.5");

                logger.ErrorException("message", new Exception("test"));
                if (enabled == 1) AssertDebugLastMessage("debug", "message");

                logger.Error(delegate { return "message from lambda"; });
                if (enabled == 1) AssertDebugLastMessage("debug", "message from lambda");

                if (enabled == 0)
                    AssertDebugCounter("debug", 0);
            }
        }

        [Test]
        public void FatalTest()
        {
            // test all possible overloads of the Fatal() method

            for (int enabled = 0; enabled < 2; ++enabled)
            {
                if (enabled == 0)
                {
                    LogManager.Configuration = CreateConfigurationFromString(@"
                <nlog>
                    <targets><target name='debug' type='Debug' layout='${message}' /></targets>
                    <rules>
                        <logger name='*' levels='' writeTo='debug' />
                    </rules>
                </nlog>");
                }
                else
                {
                    LogManager.Configuration = CreateConfigurationFromString(@"
                <nlog>
                    <targets><target name='debug' type='Debug' layout='${message}' /></targets>
                    <rules>
                        <logger name='*' levels='Fatal' writeTo='debug' />
                    </rules>
                </nlog>");
                }

                Logger logger = LogManager.GetLogger("A");

                logger.Fatal("message");
                if (enabled == 1) AssertDebugLastMessage("debug", "message");

                logger.Fatal("message{0}", (ulong)1);
                if (enabled == 1) AssertDebugLastMessage("debug", "message1");

                logger.Fatal(CultureInfo.InvariantCulture, "message{0}", (ulong)2);
                if (enabled == 1) AssertDebugLastMessage("debug", "message2");

                logger.Fatal("message{0}", (long)1);
                if (enabled == 1) AssertDebugLastMessage("debug", "message1");

                logger.Fatal(CultureInfo.InvariantCulture, "message{0}", (long)2);
                if (enabled == 1) AssertDebugLastMessage("debug", "message2");

                logger.Fatal("message{0}", (uint)1);
                if (enabled == 1) AssertDebugLastMessage("debug", "message1");

                logger.Fatal(CultureInfo.InvariantCulture, "message{0}", (uint)2);
                if (enabled == 1) AssertDebugLastMessage("debug", "message2");

                logger.Fatal("message{0}", (int)1);
                if (enabled == 1) AssertDebugLastMessage("debug", "message1");

                logger.Fatal(CultureInfo.InvariantCulture, "message{0}", (int)2);
                if (enabled == 1) AssertDebugLastMessage("debug", "message2");

                logger.Fatal("message{0}", (ushort)1);
                if (enabled == 1) AssertDebugLastMessage("debug", "message1");

                logger.Fatal(CultureInfo.InvariantCulture, "message{0}", (ushort)2);
                if (enabled == 1) AssertDebugLastMessage("debug", "message2");

                logger.Fatal("message{0}", (sbyte)1);
                if (enabled == 1) AssertDebugLastMessage("debug", "message1");

                logger.Fatal(CultureInfo.InvariantCulture, "message{0}", (sbyte)2);
                if (enabled == 1) AssertDebugLastMessage("debug", "message2");

                logger.Fatal("message{0}", this);
                if (enabled == 1) AssertDebugLastMessage("debug", "messageobject-to-string");

                logger.Fatal(CultureInfo.InvariantCulture, "message{0}", this);
                if (enabled == 1) AssertDebugLastMessage("debug", "messageobject-to-string");

                logger.Fatal("message{0}", (short)1);
                if (enabled == 1) AssertDebugLastMessage("debug", "message1");

                logger.Fatal(CultureInfo.InvariantCulture, "message{0}", (short)2);
                if (enabled == 1) AssertDebugLastMessage("debug", "message2");

                logger.Fatal("message{0}", (byte)1);
                if (enabled == 1) AssertDebugLastMessage("debug", "message1");

                logger.Fatal(CultureInfo.InvariantCulture, "message{0}", (byte)2);
                if (enabled == 1) AssertDebugLastMessage("debug", "message2");

                logger.Fatal("message{0}", 'c');
                if (enabled == 1) AssertDebugLastMessage("debug", "messagec");

                logger.Fatal(CultureInfo.InvariantCulture, "message{0}", 'd');
                if (enabled == 1) AssertDebugLastMessage("debug", "messaged");

                logger.Fatal("message{0}", "ddd");
                if (enabled == 1) AssertDebugLastMessage("debug", "messageddd");

                logger.Fatal(CultureInfo.InvariantCulture, "message{0}", "eee");
                if (enabled == 1) AssertDebugLastMessage("debug", "messageeee");

                logger.Fatal("message{0}{1}", "ddd", 1);
                if (enabled == 1) AssertDebugLastMessage("debug", "messageddd1");

                logger.Fatal(CultureInfo.InvariantCulture, "message{0}{1}", "eee", 2);
                if (enabled == 1) AssertDebugLastMessage("debug", "messageeee2");

                logger.Fatal("message{0}{1}{2}", "ddd", 1, "eee");
                if (enabled == 1) AssertDebugLastMessage("debug", "messageddd1eee");

                logger.Fatal(CultureInfo.InvariantCulture, "message{0}{1}{2}", "eee", 2, "fff");
                if (enabled == 1) AssertDebugLastMessage("debug", "messageeee2fff");

                logger.Fatal("message{0}{1}{2}{3}", "eee", 2, "fff", "ggg");
                if (enabled == 1) AssertDebugLastMessage("debug", "messageeee2fffggg");

                logger.Fatal("message{0}", true);
                if (enabled == 1) AssertDebugLastMessage("debug", "messageTrue");

                logger.Fatal(CultureInfo.InvariantCulture, "message{0}", false);
                if (enabled == 1) AssertDebugLastMessage("debug", "messageFalse");

                logger.Fatal(CultureInfo.InvariantCulture, "message{0}", (float)2.5);
                if (enabled == 1) AssertDebugLastMessage("debug", "message2.5");

                logger.Fatal(CultureInfo.InvariantCulture, "message{0}", (double)2.5);
                if (enabled == 1) AssertDebugLastMessage("debug", "message2.5");

#if !NET_CF
                CultureInfo oldCulture = System.Threading.Thread.CurrentThread.CurrentCulture;
                System.Threading.Thread.CurrentThread.CurrentCulture = new CultureInfo("pl-PL");

                logger.Fatal("message{0}", (float)1.5);
                if (enabled == 1) AssertDebugLastMessage("debug", "message1,5");

                logger.Fatal((double)1.5);
                if (enabled == 1) AssertDebugLastMessage("debug", "1,5");

                logger.Fatal(CultureInfo.InvariantCulture, (double)1.5);
                if (enabled == 1) AssertDebugLastMessage("debug", "1.5");

                logger.Fatal("message{0}", (double)1.5);
                if (enabled == 1) AssertDebugLastMessage("debug", "message1,5");

                System.Threading.Thread.CurrentThread.CurrentCulture = oldCulture;
#endif

                logger.Fatal(CultureInfo.InvariantCulture, "message{0}", (decimal)2.5);
                if (enabled == 1) AssertDebugLastMessage("debug", "message2.5");

                logger.FatalException("message", new Exception("test"));
                if (enabled == 1) AssertDebugLastMessage("debug", "message");

                logger.Fatal(delegate { return "message from lambda"; });
                if (enabled == 1) AssertDebugLastMessage("debug", "message from lambda");

                if (enabled == 0)
                    AssertDebugCounter("debug", 0);
            }
        }


        [Test]
        public void LogTest()
        {
            // test all possible overloads of the Log(level) method

            foreach (LogLevel level in new LogLevel[] { LogLevel.Debug, LogLevel.Info, LogLevel.Warn, LogLevel.Error, LogLevel.Fatal })
            {

                for (int enabled = 0; enabled < 2; ++enabled)
                {
                    if (enabled == 0)
                    {
                        LogManager.Configuration = CreateConfigurationFromString(@"
                <nlog>
                    <targets><target name='debug' type='Debug' layout='${message}' /></targets>
                    <rules>
                        <logger name='*' levels='' writeTo='debug' />
                    </rules>
                </nlog>");
                    }
                    else
                    {
                        LogManager.Configuration = CreateConfigurationFromString(@"
                <nlog>
                    <targets><target name='debug' type='Debug' layout='${message}' /></targets>
                    <rules>
                        <logger name='*' levels='" + level.Name + @"' writeTo='debug' />
                            </rules>
                                  </nlog>");
                    }

                    Logger logger = LogManager.GetLogger("A");

                    logger.Log(level, "message");
                    if (enabled == 1) AssertDebugLastMessage("debug", "message");

                    logger.Log(level, "message{0}", (ulong)1);
                    if (enabled == 1) AssertDebugLastMessage("debug", "message1");

                    logger.Log(level, CultureInfo.InvariantCulture, "message{0}", (ulong)2);
                    if (enabled == 1) AssertDebugLastMessage("debug", "message2");

                    logger.Log(level, "message{0}", (long)1);
                    if (enabled == 1) AssertDebugLastMessage("debug", "message1");

                    logger.Log(level, CultureInfo.InvariantCulture, "message{0}", (long)2);
                    if (enabled == 1) AssertDebugLastMessage("debug", "message2");

                    logger.Log(level, "message{0}", (uint)1);
                    if (enabled == 1) AssertDebugLastMessage("debug", "message1");

                    logger.Log(level, CultureInfo.InvariantCulture, "message{0}", (uint)2);
                    if (enabled == 1) AssertDebugLastMessage("debug", "message2");

                    logger.Log(level, "message{0}", (int)1);
                    if (enabled == 1) AssertDebugLastMessage("debug", "message1");

                    logger.Log(level, CultureInfo.InvariantCulture, "message{0}", (int)2);
                    if (enabled == 1) AssertDebugLastMessage("debug", "message2");

                    logger.Log(level, "message{0}", (ushort)1);
                    if (enabled == 1) AssertDebugLastMessage("debug", "message1");

                    logger.Log(level, CultureInfo.InvariantCulture, "message{0}", (ushort)2);
                    if (enabled == 1) AssertDebugLastMessage("debug", "message2");

                    logger.Log(level, "message{0}", (sbyte)1);
                    if (enabled == 1) AssertDebugLastMessage("debug", "message1");

                    logger.Log(level, CultureInfo.InvariantCulture, "message{0}", (sbyte)2);
                    if (enabled == 1) AssertDebugLastMessage("debug", "message2");

                    logger.Log(level, "message{0}", this);
                    if (enabled == 1) AssertDebugLastMessage("debug", "messageobject-to-string");

                    logger.Log(level, CultureInfo.InvariantCulture, "message{0}", this);
                    if (enabled == 1) AssertDebugLastMessage("debug", "messageobject-to-string");

                    logger.Log(level, "message{0}", (short)1);
                    if (enabled == 1) AssertDebugLastMessage("debug", "message1");

                    logger.Log(level, CultureInfo.InvariantCulture, "message{0}", (short)2);
                    if (enabled == 1) AssertDebugLastMessage("debug", "message2");

                    logger.Log(level, "message{0}", (byte)1);
                    if (enabled == 1) AssertDebugLastMessage("debug", "message1");

                    logger.Log(level, CultureInfo.InvariantCulture, "message{0}", (byte)2);
                    if (enabled == 1) AssertDebugLastMessage("debug", "message2");

                    logger.Log(level, "message{0}", 'c');
                    if (enabled == 1) AssertDebugLastMessage("debug", "messagec");

                    logger.Log(level, CultureInfo.InvariantCulture, "message{0}", 'd');
                    if (enabled == 1) AssertDebugLastMessage("debug", "messaged");

                    logger.Log(level, "message{0}", "ddd");
                    if (enabled == 1) AssertDebugLastMessage("debug", "messageddd");

                    logger.Log(level, CultureInfo.InvariantCulture, "message{0}", "eee");
                    if (enabled == 1) AssertDebugLastMessage("debug", "messageeee");

                    logger.Log(level, "message{0}{1}", "ddd", 1);
                    if (enabled == 1) AssertDebugLastMessage("debug", "messageddd1");

                    logger.Log(level, CultureInfo.InvariantCulture, "message{0}{1}", "eee", 2);
                    if (enabled == 1) AssertDebugLastMessage("debug", "messageeee2");

                    logger.Log(level, "message{0}{1}{2}", "ddd", 1, "eee");
                    if (enabled == 1) AssertDebugLastMessage("debug", "messageddd1eee");

                    logger.Log(level, CultureInfo.InvariantCulture, "message{0}{1}{2}", "eee", 2, "fff");
                    if (enabled == 1) AssertDebugLastMessage("debug", "messageeee2fff");

                    logger.Log(level, "message{0}{1}{2}{3}", "eee", 2, "fff", "ggg");
                    if (enabled == 1) AssertDebugLastMessage("debug", "messageeee2fffggg");

                    logger.Log(level, "message{0}", true);
                    if (enabled == 1) AssertDebugLastMessage("debug", "messageTrue");

                    logger.Log(level, CultureInfo.InvariantCulture, "message{0}", false);
                    if (enabled == 1) AssertDebugLastMessage("debug", "messageFalse");

                    logger.Log(level, CultureInfo.InvariantCulture, "message{0}", (float)2.5);
                    if (enabled == 1) AssertDebugLastMessage("debug", "message2.5");

                    logger.Log(level, CultureInfo.InvariantCulture, "message{0}", (double)2.5);
                    if (enabled == 1) AssertDebugLastMessage("debug", "message2.5");

#if !NET_CF
                    CultureInfo oldCulture = System.Threading.Thread.CurrentThread.CurrentCulture;
                    System.Threading.Thread.CurrentThread.CurrentCulture = new CultureInfo("pl-PL");

                    logger.Log(level, "message{0}", (float)1.5);
                    if (enabled == 1) AssertDebugLastMessage("debug", "message1,5");

                    logger.Log(level, (double)1.5);
                    if (enabled == 1) AssertDebugLastMessage("debug", "1,5");

                    logger.Log(level, CultureInfo.InvariantCulture, (double)1.5);
                    if (enabled == 1) AssertDebugLastMessage("debug", "1.5");

                    logger.Log(level, "message{0}", (double)1.5);
                    if (enabled == 1) AssertDebugLastMessage("debug", "message1,5");

                    logger.Log(level, "message{0}", (decimal)1.5);
                    if (enabled == 1) AssertDebugLastMessage("debug", "message1,5");

                    System.Threading.Thread.CurrentThread.CurrentCulture = oldCulture;
#endif

                    logger.Log(level, CultureInfo.InvariantCulture, "message{0}", (decimal)2.5);
                    if (enabled == 1) AssertDebugLastMessage("debug", "message2.5");

                    logger.LogException(level, "message", new Exception("test"));
                    if (enabled == 1) AssertDebugLastMessage("debug", "message");

                    logger.Log(level, delegate { return "message from lambda"; });
                    if (enabled == 1) AssertDebugLastMessage("debug", "message from lambda");

                    if (enabled == 0)
                        AssertDebugCounter("debug", 0);
                }
            }
        }

        [Test]
        public void StringFormatWillNotCauseExceptions()
        {
            LogManager.Configuration = CreateConfigurationFromString(@"
                    <nlog throwExceptions='true'>
                        <targets><target name='debug' type='Debug' layout='${message}' /></targets>
                        <rules>
                            <logger name='*' minLevel='Info' writeTo='debug' />
                        </rules>
                    </nlog>");

            Logger l = LogManager.GetLogger("StringFormatWillNotCauseExceptions");

            // invalid format string
            l.Info("aaaa {0");
            AssertDebugLastMessage("debug", "aaaa {0");
        }

        public override string ToString()
        {
            return "object-to-string";
        }

    }
}
