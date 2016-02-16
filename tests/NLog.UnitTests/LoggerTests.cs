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
    using System.Reflection;
    using NLog.Targets;
    using System;
    using System.Globalization;
    using NLog.Config;
#if ASYNC_SUPPORTED
    using System.Threading.Tasks;
#endif
    using Xunit;
    using System.Threading;
    public class LoggerTests : NLogTestBase
    {
        [Fact]
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

                ILogger logger = LogManager.GetLogger("A");

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

                logger.Trace("message{0}", 1);
                if (enabled == 1) AssertDebugLastMessage("debug", "message1");

                logger.Trace(CultureInfo.InvariantCulture, "message{0}", 2);
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

                logger.Trace(CultureInfo.InvariantCulture, "message{0}", 2.5);
                if (enabled == 1) AssertDebugLastMessage("debug", "message2.5");

                logger.Trace(CultureInfo.InvariantCulture, "message{0}", (decimal)2.5);
                if (enabled == 1) AssertDebugLastMessage("debug", "message2.5");

#pragma warning disable 0618
                // Obsolete method requires testing until removed.
                logger.TraceException("message", new Exception("test"));
                if (enabled == 1) AssertDebugLastMessage("debug", "message");
#pragma warning restore 0618

                logger.Trace(new Exception("test"), "message");
                if (enabled == 1) AssertDebugLastMessage("debug", "message");

                logger.Trace(delegate { return "message from lambda"; });
                if (enabled == 1) AssertDebugLastMessage("debug", "message from lambda");

                if (enabled == 0)
                    AssertDebugCounter("debug", 0);
            }
        }

        [Fact]
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

                ILogger logger = LogManager.GetLogger("A");

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

                logger.Debug("message{0}", 1);
                if (enabled == 1) AssertDebugLastMessage("debug", "message1");

                logger.Debug(CultureInfo.InvariantCulture, "message{0}", 2);
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

                logger.Debug(CultureInfo.InvariantCulture, "message{0}", 2.5);
                if (enabled == 1) AssertDebugLastMessage("debug", "message2.5");

                logger.Debug(CultureInfo.InvariantCulture, "message{0}", (decimal)2.5);
                if (enabled == 1) AssertDebugLastMessage("debug", "message2.5");

#pragma warning disable 0618
                // Obsolete method requires testing until completely removed.
                logger.DebugException("message", new Exception("test"));
                if (enabled == 1) AssertDebugLastMessage("debug", "message");
#pragma warning restore 0618

                logger.Debug(delegate { return "message from lambda"; });
                if (enabled == 1) AssertDebugLastMessage("debug", "message from lambda");

                if (enabled == 0)
                    AssertDebugCounter("debug", 0);
            }
        }

        [Fact]
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

                ILogger logger = LogManager.GetLogger("A");

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

                logger.Info("message{0}", 1);
                if (enabled == 1) AssertDebugLastMessage("debug", "message1");

                logger.Info(CultureInfo.InvariantCulture, "message{0}", 2);
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

                logger.Info(CultureInfo.InvariantCulture, "message{0}", 2.5);
                if (enabled == 1) AssertDebugLastMessage("debug", "message2.5");

                logger.Info(CultureInfo.InvariantCulture, "message{0}", (decimal)2.5);
                if (enabled == 1) AssertDebugLastMessage("debug", "message2.5");

#pragma warning disable 0618
                // Obsolete method requires testing until removed.
                logger.InfoException("message", new Exception("test"));
                if (enabled == 1) AssertDebugLastMessage("debug", "message");
#pragma warning restore 0618

                logger.Info(new Exception("test"), "message");
                if (enabled == 1) AssertDebugLastMessage("debug", "message");

                logger.Info(delegate { return "message from lambda"; });
                if (enabled == 1) AssertDebugLastMessage("debug", "message from lambda");

                if (enabled == 0)
                    AssertDebugCounter("debug", 0);
            }
        }

        [Fact]
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

                ILogger logger = LogManager.GetLogger("A");

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

                logger.Warn("message{0}", 1);
                if (enabled == 1) AssertDebugLastMessage("debug", "message1");

                logger.Warn(CultureInfo.InvariantCulture, "message{0}", 2);
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

                logger.Warn(CultureInfo.InvariantCulture, "message{0}", 2.5);
                if (enabled == 1) AssertDebugLastMessage("debug", "message2.5");

                logger.Warn(CultureInfo.InvariantCulture, "message{0}", (decimal)2.5);
                if (enabled == 1) AssertDebugLastMessage("debug", "message2.5");

#pragma warning disable 0618
                // Obsolete method requires testing until removed.
                logger.WarnException("message", new Exception("test"));
                if (enabled == 1) AssertDebugLastMessage("debug", "message");
#pragma warning restore 0618

                logger.Warn(new Exception("test"), "message");
                if (enabled == 1) AssertDebugLastMessage("debug", "message");

                logger.Warn(delegate { return "message from lambda"; });
                if (enabled == 1) AssertDebugLastMessage("debug", "message from lambda");

                if (enabled == 0)
                    AssertDebugCounter("debug", 0);
            }
        }

        [Fact]
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

                ILogger logger = LogManager.GetLogger("A");

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

                logger.Error("message{0}", 1);
                if (enabled == 1) AssertDebugLastMessage("debug", "message1");

                logger.Error(CultureInfo.InvariantCulture, "message{0}", 2);
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

                logger.Error(CultureInfo.InvariantCulture, "message{0}", 2.5);
                if (enabled == 1) AssertDebugLastMessage("debug", "message2.5");

                logger.Error(CultureInfo.InvariantCulture, "message{0}", (decimal)2.5);
                if (enabled == 1) AssertDebugLastMessage("debug", "message2.5");

#pragma warning disable 0618
                // Obsolete method requires testing until completely removed.
                logger.ErrorException("message", new Exception("test"));
                if (enabled == 1) AssertDebugLastMessage("debug", "message");
#pragma warning restore 0618

                logger.Error(new Exception("test"), "message");
                if (enabled == 1) AssertDebugLastMessage("debug", "message");

                logger.Error(delegate { return "message from lambda"; });
                if (enabled == 1) AssertDebugLastMessage("debug", "message from lambda");

                if (enabled == 0)
                    AssertDebugCounter("debug", 0);
            }
        }

        [Fact]
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

                ILogger logger = LogManager.GetLogger("A");

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

                logger.Fatal("message{0}", 1);
                if (enabled == 1) AssertDebugLastMessage("debug", "message1");

                logger.Fatal(CultureInfo.InvariantCulture, "message{0}", 2);
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

                logger.Fatal(CultureInfo.InvariantCulture, "message{0}", 2.5);
                if (enabled == 1) AssertDebugLastMessage("debug", "message2.5");

                logger.Fatal(CultureInfo.InvariantCulture, "message{0}", (decimal)2.5);
                if (enabled == 1) AssertDebugLastMessage("debug", "message2.5");

#pragma warning disable 0618
                // Obsolete method requires testing until removed.
                logger.FatalException("message", new Exception("test"));
                if (enabled == 1) AssertDebugLastMessage("debug", "message");
#pragma warning restore 0618

                logger.Fatal(new Exception("test"), "message");
                if (enabled == 1) AssertDebugLastMessage("debug", "message");

                logger.Fatal(delegate { return "message from lambda"; });
                if (enabled == 1) AssertDebugLastMessage("debug", "message from lambda");

                if (enabled == 0)
                    AssertDebugCounter("debug", 0);
            }
        }

        [Fact]
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

                    ILogger logger = LogManager.GetLogger("A");

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

                    logger.Log(level, CultureInfo.InvariantCulture, "message{0}", (decimal)2.5);
                    if (enabled == 1) AssertDebugLastMessage("debug", "message2.5");

                    logger.Log(level, new Exception("test"), "message");
                    if (enabled == 1) AssertDebugLastMessage("debug", "message");

#pragma warning disable 0618
                    // Obsolete method requires testing until removed.
                    logger.LogException(level, "message", new Exception("test"));
                    if (enabled == 1) AssertDebugLastMessage("debug", "message");
#pragma warning restore 0618

                    logger.Log(level, delegate { return "message from lambda"; });
                    if (enabled == 1) AssertDebugLastMessage("debug", "message from lambda");

                    if (enabled == 0)
                        AssertDebugCounter("debug", 0);
                }
            }
        }

        #region Conditional Logger
#if DEBUG

        [Fact]
        public void ConditionalTraceTest()
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

                var logger = LogManager.GetLogger("A");

                logger.ConditionalTrace("message");
                if (enabled == 1) AssertDebugLastMessage("debug", "message");

                logger.ConditionalTrace("message{0}", (ulong)1);
                if (enabled == 1) AssertDebugLastMessage("debug", "message1");

                logger.ConditionalTrace(CultureInfo.InvariantCulture, "message{0}", (ulong)2);
                if (enabled == 1) AssertDebugLastMessage("debug", "message2");

                logger.ConditionalTrace("message{0}", (long)1);
                if (enabled == 1) AssertDebugLastMessage("debug", "message1");

                logger.ConditionalTrace(CultureInfo.InvariantCulture, "message{0}", (long)2);
                if (enabled == 1) AssertDebugLastMessage("debug", "message2");

                logger.ConditionalTrace("message{0}", (uint)1);
                if (enabled == 1) AssertDebugLastMessage("debug", "message1");

                logger.ConditionalTrace(CultureInfo.InvariantCulture, "message{0}", (uint)2);
                if (enabled == 1) AssertDebugLastMessage("debug", "message2");

                logger.ConditionalTrace("message{0}", 1);
                if (enabled == 1) AssertDebugLastMessage("debug", "message1");

                logger.ConditionalTrace(CultureInfo.InvariantCulture, "message{0}", 2);
                if (enabled == 1) AssertDebugLastMessage("debug", "message2");

                logger.ConditionalTrace("message{0}", (ushort)1);
                if (enabled == 1) AssertDebugLastMessage("debug", "message1");

                logger.ConditionalTrace(CultureInfo.InvariantCulture, "message{0}", (ushort)2);
                if (enabled == 1) AssertDebugLastMessage("debug", "message2");

                logger.ConditionalTrace("message{0}", (sbyte)1);
                if (enabled == 1) AssertDebugLastMessage("debug", "message1");

                logger.ConditionalTrace(CultureInfo.InvariantCulture, "message{0}", (sbyte)2);
                if (enabled == 1) AssertDebugLastMessage("debug", "message2");

                logger.ConditionalTrace("message{0}", this);
                if (enabled == 1) AssertDebugLastMessage("debug", "messageobject-to-string");

                logger.ConditionalTrace(CultureInfo.InvariantCulture, "message{0}", this);
                if (enabled == 1) AssertDebugLastMessage("debug", "messageobject-to-string");

                logger.ConditionalTrace("message{0}", (short)1);
                if (enabled == 1) AssertDebugLastMessage("debug", "message1");

                logger.ConditionalTrace(CultureInfo.InvariantCulture, "message{0}", (short)2);
                if (enabled == 1) AssertDebugLastMessage("debug", "message2");

                logger.ConditionalTrace("message{0}", (byte)1);
                if (enabled == 1) AssertDebugLastMessage("debug", "message1");

                logger.ConditionalTrace(CultureInfo.InvariantCulture, "message{0}", (byte)2);
                if (enabled == 1) AssertDebugLastMessage("debug", "message2");

                logger.ConditionalTrace("message{0}", 'c');
                if (enabled == 1) AssertDebugLastMessage("debug", "messagec");

                logger.ConditionalTrace(CultureInfo.InvariantCulture, "message{0}", 'd');
                if (enabled == 1) AssertDebugLastMessage("debug", "messaged");

                logger.ConditionalTrace("message{0}", "ddd");
                if (enabled == 1) AssertDebugLastMessage("debug", "messageddd");

                logger.ConditionalTrace(CultureInfo.InvariantCulture, "message{0}", "eee");
                if (enabled == 1) AssertDebugLastMessage("debug", "messageeee");

                logger.ConditionalTrace("message{0}{1}", "ddd", 1);
                if (enabled == 1) AssertDebugLastMessage("debug", "messageddd1");

                logger.ConditionalTrace(CultureInfo.InvariantCulture, "message{0}{1}", "eee", 2);
                if (enabled == 1) AssertDebugLastMessage("debug", "messageeee2");

                logger.ConditionalTrace("message{0}{1}{2}", "ddd", 1, "eee");
                if (enabled == 1) AssertDebugLastMessage("debug", "messageddd1eee");

                logger.ConditionalTrace(CultureInfo.InvariantCulture, "message{0}{1}{2}", "eee", 2, "fff");
                if (enabled == 1) AssertDebugLastMessage("debug", "messageeee2fff");

                logger.ConditionalTrace("message{0}{1}{2}{3}", "eee", 2, "fff", "ggg");
                if (enabled == 1) AssertDebugLastMessage("debug", "messageeee2fffggg");

                logger.ConditionalTrace("message{0}", true);
                if (enabled == 1) AssertDebugLastMessage("debug", "messageTrue");

                logger.ConditionalTrace(CultureInfo.InvariantCulture, "message{0}", false);
                if (enabled == 1) AssertDebugLastMessage("debug", "messageFalse");

                logger.ConditionalTrace(CultureInfo.InvariantCulture, "message{0}", (float)2.5);
                if (enabled == 1) AssertDebugLastMessage("debug", "message2.5");

                logger.ConditionalTrace(CultureInfo.InvariantCulture, "message{0}", 2.5);
                if (enabled == 1) AssertDebugLastMessage("debug", "message2.5");

                logger.ConditionalTrace(CultureInfo.InvariantCulture, "message{0}", (decimal)2.5);
                if (enabled == 1) AssertDebugLastMessage("debug", "message2.5");

                logger.ConditionalTrace(new Exception("test"), "message");
                if (enabled == 1) AssertDebugLastMessage("debug", "message");

                logger.ConditionalTrace(delegate { return "message from lambda"; });
                if (enabled == 1) AssertDebugLastMessage("debug", "message from lambda");

                if (enabled == 0)
                    AssertDebugCounter("debug", 0);
            }
        }

        [Fact]
        public void ConditionalDebugTest()
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

                var logger = LogManager.GetLogger("A");

                logger.ConditionalDebug("message");
                if (enabled == 1) AssertDebugLastMessage("debug", "message");

                logger.ConditionalDebug("message{0}", (ulong)1);
                if (enabled == 1) AssertDebugLastMessage("debug", "message1");

                logger.ConditionalDebug(CultureInfo.InvariantCulture, "message{0}", (ulong)2);
                if (enabled == 1) AssertDebugLastMessage("debug", "message2");

                logger.ConditionalDebug("message{0}", (long)1);
                if (enabled == 1) AssertDebugLastMessage("debug", "message1");

                logger.ConditionalDebug(CultureInfo.InvariantCulture, "message{0}", (long)2);
                if (enabled == 1) AssertDebugLastMessage("debug", "message2");

                logger.ConditionalDebug("message{0}", (uint)1);
                if (enabled == 1) AssertDebugLastMessage("debug", "message1");

                logger.ConditionalDebug(CultureInfo.InvariantCulture, "message{0}", (uint)2);
                if (enabled == 1) AssertDebugLastMessage("debug", "message2");

                logger.ConditionalDebug("message{0}", 1);
                if (enabled == 1) AssertDebugLastMessage("debug", "message1");

                logger.ConditionalDebug(CultureInfo.InvariantCulture, "message{0}", 2);
                if (enabled == 1) AssertDebugLastMessage("debug", "message2");

                logger.ConditionalDebug("message{0}", (ushort)1);
                if (enabled == 1) AssertDebugLastMessage("debug", "message1");

                logger.ConditionalDebug(CultureInfo.InvariantCulture, "message{0}", (ushort)2);
                if (enabled == 1) AssertDebugLastMessage("debug", "message2");

                logger.ConditionalDebug("message{0}", (sbyte)1);
                if (enabled == 1) AssertDebugLastMessage("debug", "message1");

                logger.ConditionalDebug(CultureInfo.InvariantCulture, "message{0}", (sbyte)2);
                if (enabled == 1) AssertDebugLastMessage("debug", "message2");

                logger.ConditionalDebug("message{0}", this);
                if (enabled == 1) AssertDebugLastMessage("debug", "messageobject-to-string");

                logger.ConditionalDebug(CultureInfo.InvariantCulture, "message{0}", this);
                if (enabled == 1) AssertDebugLastMessage("debug", "messageobject-to-string");

                logger.ConditionalDebug("message{0}", (short)1);
                if (enabled == 1) AssertDebugLastMessage("debug", "message1");

                logger.ConditionalDebug(CultureInfo.InvariantCulture, "message{0}", (short)2);
                if (enabled == 1) AssertDebugLastMessage("debug", "message2");

                logger.ConditionalDebug("message{0}", (byte)1);
                if (enabled == 1) AssertDebugLastMessage("debug", "message1");

                logger.ConditionalDebug(CultureInfo.InvariantCulture, "message{0}", (byte)2);
                if (enabled == 1) AssertDebugLastMessage("debug", "message2");

                logger.ConditionalDebug("message{0}", 'c');
                if (enabled == 1) AssertDebugLastMessage("debug", "messagec");

                logger.ConditionalDebug(CultureInfo.InvariantCulture, "message{0}", 'd');
                if (enabled == 1) AssertDebugLastMessage("debug", "messaged");

                logger.ConditionalDebug("message{0}", "ddd");
                if (enabled == 1) AssertDebugLastMessage("debug", "messageddd");

                logger.ConditionalDebug(CultureInfo.InvariantCulture, "message{0}", "eee");
                if (enabled == 1) AssertDebugLastMessage("debug", "messageeee");

                logger.ConditionalDebug("message{0}{1}", "ddd", 1);
                if (enabled == 1) AssertDebugLastMessage("debug", "messageddd1");

                logger.ConditionalDebug(CultureInfo.InvariantCulture, "message{0}{1}", "eee", 2);
                if (enabled == 1) AssertDebugLastMessage("debug", "messageeee2");

                logger.ConditionalDebug("message{0}{1}{2}", "ddd", 1, "eee");
                if (enabled == 1) AssertDebugLastMessage("debug", "messageddd1eee");

                logger.ConditionalDebug(CultureInfo.InvariantCulture, "message{0}{1}{2}", "eee", 2, "fff");
                if (enabled == 1) AssertDebugLastMessage("debug", "messageeee2fff");

                logger.ConditionalDebug("message{0}{1}{2}{3}", "eee", 2, "fff", "ggg");
                if (enabled == 1) AssertDebugLastMessage("debug", "messageeee2fffggg");

                logger.ConditionalDebug("message{0}", true);
                if (enabled == 1) AssertDebugLastMessage("debug", "messageTrue");

                logger.ConditionalDebug(CultureInfo.InvariantCulture, "message{0}", false);
                if (enabled == 1) AssertDebugLastMessage("debug", "messageFalse");

                logger.ConditionalDebug(CultureInfo.InvariantCulture, "message{0}", (float)2.5);
                if (enabled == 1) AssertDebugLastMessage("debug", "message2.5");

                logger.ConditionalDebug(CultureInfo.InvariantCulture, "message{0}", 2.5);
                if (enabled == 1) AssertDebugLastMessage("debug", "message2.5");

                logger.ConditionalDebug(CultureInfo.InvariantCulture, "message{0}", (decimal)2.5);
                if (enabled == 1) AssertDebugLastMessage("debug", "message2.5");

                logger.ConditionalDebug(delegate { return "message from lambda"; });
                if (enabled == 1) AssertDebugLastMessage("debug", "message from lambda");

                if (enabled == 0)
                    AssertDebugCounter("debug", 0);
            }
        }

#endif
        #endregion

        [Fact]
        public void SwallowTest()
        {
            LogManager.Configuration = CreateConfigurationFromString(@"
                <nlog>
                    <targets><target name='debug' type='Debug' layout='${message}' /></targets>
                    <rules>
                        <logger name='*' levels='Error' writeTo='debug' />
                    </rules>
                </nlog>");
            ILogger logger = LogManager.GetLogger("A");
            bool warningFix = true;

            bool executed = false;
            logger.Swallow(() => executed = true);
            Assert.True(executed);

            Assert.Equal(1, logger.Swallow(() => 1));
            Assert.Equal(1, logger.Swallow(() => 1, 2));

#if ASYNC_SUPPORTED
            logger.SwallowAsync(Task.WhenAll()).Wait();

            int executions = 0;
            logger.Swallow(Task.Run(() => ++executions));
            logger.SwallowAsync(async () => { await Task.Delay(20); ++executions; }).Wait();
            Assert.True(executions == 2);

            Assert.Equal(1, logger.SwallowAsync(async () => { await Task.Delay(20); return 1; }).Result);
            Assert.Equal(1, logger.SwallowAsync(async () => { await Task.Delay(20); return 1; }, 2).Result);
#endif

            AssertDebugCounter("debug", 0);

            logger.Swallow(() => { throw new InvalidOperationException("Test message 1"); });
            AssertDebugLastMessageContains("debug", "Test message 1");

            Assert.Equal(0, logger.Swallow(() => { if (warningFix) throw new InvalidOperationException("Test message 2"); return 1; }));
            AssertDebugLastMessageContains("debug", "Test message 2");

            Assert.Equal(2, logger.Swallow(() => { if (warningFix) throw new InvalidOperationException("Test message 3"); return 1; }, 2));
            AssertDebugLastMessageContains("debug", "Test message 3");

#if ASYNC_SUPPORTED
            var fireAndFogetCompletion = new TaskCompletionSource<bool>();
            fireAndFogetCompletion.SetException(new InvalidOperationException("Swallow fire and forget test message"));
            logger.Swallow(fireAndFogetCompletion.Task);
            while (!GetDebugLastMessage("debug").Contains("Swallow fire and forget test message"))
                Thread.Sleep(10); // Polls forever since there is nothing to wait on.

            var completion = new TaskCompletionSource<bool>();
            completion.SetException(new InvalidOperationException("Test message 4"));
            logger.SwallowAsync(completion.Task).Wait();
            AssertDebugLastMessageContains("debug", "Test message 4");

            logger.SwallowAsync(async () => { await Task.Delay(20); throw new InvalidOperationException("Test message 5"); }).Wait();
            AssertDebugLastMessageContains("debug", "Test message 5");

            Assert.Equal(0, logger.SwallowAsync(async () => { await Task.Delay(20); if (warningFix) throw new InvalidOperationException("Test message 6"); return 1; }).Result);
            AssertDebugLastMessageContains("debug", "Test message 6");

            Assert.Equal(2, logger.SwallowAsync(async () => { await Task.Delay(20); if (warningFix) throw new InvalidOperationException("Test message 7"); return 1; }, 2).Result);
            AssertDebugLastMessageContains("debug", "Test message 7");
#endif
        }

        [Fact]
        public void StringFormatWillNotCauseExceptions()
        {
            LogManager.Configuration = CreateConfigurationFromString(@"
                    <nlog throwExceptions='true'>
                        <targets><target name='debug' type='Debug' layout='${message}' /></targets>
                        <rules>
                            <logger name='*' minLevel='Info' writeTo='debug' />
                        </rules>
                    </nlog>");

            ILogger l = LogManager.GetLogger("StringFormatWillNotCauseExceptions");

            // invalid format string
            l.Info("aaaa {0");
            AssertDebugLastMessage("debug", "aaaa {0");
        }

        [Fact]
        public void MultipleLoggersWithSameNameShouldBothReceiveMessages()
        {
            LogManager.Configuration = CreateConfigurationFromString(@"
                <nlog>
                    <targets>
                        <target name='first' type='Debug' layout='${message}' />
                        <target name='second' type='Debug' layout='${message}' />
                    </targets>
                    <rules>
                        <logger name='*' minlevel='Debug' writeTo='first' />
                        <logger name='*' minlevel='Debug' writeTo='second' />
                    </rules>
                </nlog>");
            var logger = LogManager.GetLogger("A");

            const string logMessage = "Anything";
            logger.Debug(logMessage);
            AssertDebugLastMessage("first", logMessage);
            AssertDebugLastMessage("second", logMessage);
        }

        [Fact]
        public void When_Logging_LogEvent_Without_Level_Defined_No_Exception_Should_Be_Thrown()
        {
            var config = new LoggingConfiguration();
            var target = new MyTarget();
            config.LoggingRules.Add(new LoggingRule("*", LogLevel.Trace, target));
            LogManager.Configuration = config;
            var logger = LogManager.GetLogger("A");

            Assert.Throws<InvalidOperationException>(() => logger.Log(new LogEventInfo()));
        }

        public abstract class BaseWrapper
        {
            public void Log(string what)
            {
                InternalLog(what);
            }

            protected abstract void InternalLog(string what);
        }

        public class MyWrapper : BaseWrapper
        {
            private readonly ILogger wrapperLogger;

            public MyWrapper()
            {
                wrapperLogger = LogManager.GetLogger("WrappedLogger");
            }

            protected override void InternalLog(string what)
            {
                LogEventInfo info = new LogEventInfo(LogLevel.Warn, wrapperLogger.Name, what);

                // Provide BaseWrapper as wrapper type.
                // Expected: UserStackFrame should point to the method that calls a 
                // method of BaseWrapper.
                wrapperLogger.Log(typeof(BaseWrapper), info);
            }
        }

        public class MyTarget : TargetWithLayout
        {
            public MyTarget()
            {
                // enforce creation of stack trace
                Layout = "${stacktrace}";
            }

            public LogEventInfo LastEvent { get; private set; }

            protected override void Write(LogEventInfo logEvent)
            {
                LastEvent = logEvent;
                base.Write(logEvent);
            }
        }

        public override string ToString()
        {
            return "object-to-string";
        }

    }
}
