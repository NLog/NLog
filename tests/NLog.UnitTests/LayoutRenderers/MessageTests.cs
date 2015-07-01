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

namespace NLog.UnitTests.LayoutRenderers
{
    using System;
    using System.Globalization;
    using Xunit;

    public class MessageTests : NLogTestBase
    {
        [Fact]
        public void MessageWithoutPaddingTest()
        {
            LogManager.Configuration = CreateConfigurationFromString(@"
            <nlog>
                <targets><target name='debug' type='Debug' layout='${message}' /></targets>
                <rules>
                    <logger name='*' minlevel='Debug' writeTo='debug' />
                </rules>
            </nlog>");

            ILogger logger = LogManager.GetLogger("A");
            logger.Debug("a");
            AssertDebugLastMessage("debug", "a");
            logger.Debug("a{0}", 1);
            AssertDebugLastMessage("debug", "a1");
            logger.Debug("a{0}{1}", 1, "2");
            AssertDebugLastMessage("debug", "a12");
            logger.Debug(CultureInfo.InvariantCulture, "a{0}", new DateTime(2005, 1, 1));
            AssertDebugLastMessage("debug", "a01/01/2005 00:00:00");
        }

        [Fact]
        public void MessageRightPaddingTest()
        {
            LogManager.Configuration = CreateConfigurationFromString(@"
            <nlog>
                <targets><target name='debug' type='Debug' layout='${message:padding=3}' /></targets>
                <rules>
                    <logger name='*' minlevel='Debug' writeTo='debug' />
                </rules>
            </nlog>");

            ILogger logger = LogManager.GetLogger("A");
            logger.Debug("a");
            AssertDebugLastMessage("debug", "  a");
            logger.Debug("a{0}", 1);
            AssertDebugLastMessage("debug", " a1");
            logger.Debug("a{0}{1}", 1, "2");
            AssertDebugLastMessage("debug", "a12");
            logger.Debug(CultureInfo.InvariantCulture, "a{0}", new DateTime(2005, 1, 1));
            AssertDebugLastMessage("debug", "a01/01/2005 00:00:00");
        }


        [Fact]
        public void MessageFixedLengthRightPaddingLeftAlignmentTest()
        {
            LogManager.Configuration = CreateConfigurationFromString(@"
            <nlog>
                <targets><target name='debug' type='Debug' layout='${message:padding=3:fixedlength=true}' /></targets>
                <rules>
                    <logger name='*' minlevel='Debug' writeTo='debug' />
                </rules>
            </nlog>");

            ILogger logger = LogManager.GetLogger("A");
            logger.Debug("a");
            AssertDebugLastMessage("debug", "  a");
            logger.Debug("a{0}", 1);
            AssertDebugLastMessage("debug", " a1");
            logger.Debug("a{0}{1}", 1, "2");
            AssertDebugLastMessage("debug", "a12");
            logger.Debug(CultureInfo.InvariantCulture, "a{0}", new DateTime(2005, 1, 1));
            AssertDebugLastMessage("debug", "a01");
        }

        [Fact]
        public void MessageFixedLengthRightPaddingRightAlignmentTest()
        {
            LogManager.Configuration = CreateConfigurationFromString(@"
            <nlog>
                <targets><target name='debug' type='Debug' layout='${message:padding=3:fixedlength=true:alignmentOnTruncation=right}' /></targets>
                <rules>
                    <logger name='*' minlevel='Debug' writeTo='debug' />
                </rules>
            </nlog>");

            ILogger logger = LogManager.GetLogger("A");
            logger.Debug("a");
            AssertDebugLastMessage("debug", "  a");
            logger.Debug("a{0}", 1);
            AssertDebugLastMessage("debug", " a1");
            logger.Debug("a{0}{1}", 1, "2");
            AssertDebugLastMessage("debug", "a12");
            logger.Debug(CultureInfo.InvariantCulture, "a{0}", new DateTime(2005, 1, 1));
            AssertDebugLastMessage("debug", ":00");
        }

        [Fact]
        public void MessageLeftPaddingTest()
        {
            LogManager.Configuration = CreateConfigurationFromString(@"
            <nlog>
                <targets><target name='debug' type='Debug' layout='${message:padding=-3:padcharacter=x}' /></targets>
                <rules>
                    <logger name='*' minlevel='Debug' writeTo='debug' />
                </rules>
            </nlog>");

            ILogger logger = LogManager.GetLogger("A");
            logger.Debug("a");
            AssertDebugLastMessage("debug", "axx");
            logger.Debug("a{0}", 1);
            AssertDebugLastMessage("debug", "a1x");
            logger.Debug("a{0}{1}", 1, "2");
            AssertDebugLastMessage("debug", "a12");
            logger.Debug(CultureInfo.InvariantCulture, "a{0}", new DateTime(2005, 1, 1));
            AssertDebugLastMessage("debug", "a01/01/2005 00:00:00");
        }

        [Fact]
        public void MessageFixedLengthLeftPaddingLeftAlignmentTest()
        {
            LogManager.Configuration = CreateConfigurationFromString(@"
            <nlog>
                <targets><target name='debug' type='Debug' layout='${message:padding=-3:padcharacter=x:fixedlength=true}' /></targets>
                <rules>
                    <logger name='*' minlevel='Debug' writeTo='debug' />
                </rules>
            </nlog>");

            ILogger logger = LogManager.GetLogger("A");
            logger.Debug("a");
            AssertDebugLastMessage("debug", "axx");
            logger.Debug("a{0}", 1);
            AssertDebugLastMessage("debug", "a1x");
            logger.Debug("a{0}{1}", 1, "2");
            AssertDebugLastMessage("debug", "a12");
            logger.Debug(CultureInfo.InvariantCulture, "a{0}", new DateTime(2005, 1, 1));
            AssertDebugLastMessage("debug", "a01");
        }

        [Fact]
        public void MessageFixedLengthLeftPaddingRightAlignmentTest()
        {
            LogManager.Configuration = CreateConfigurationFromString(@"
            <nlog>
                <targets><target name='debug' type='Debug' layout='${message:padding=-3:padcharacter=x:fixedlength=true:alignmentOnTruncation=right}' /></targets>
                <rules>
                    <logger name='*' minlevel='Debug' writeTo='debug' />
                </rules>
            </nlog>");

            ILogger logger = LogManager.GetLogger("A");
            logger.Debug("a");
            AssertDebugLastMessage("debug", "axx");
            logger.Debug("a{0}", 1);
            AssertDebugLastMessage("debug", "a1x");
            logger.Debug("a{0}{1}", 1, "2");
            AssertDebugLastMessage("debug", "a12");
            logger.Debug(CultureInfo.InvariantCulture, "a{0}", new DateTime(2005, 1, 1));
            AssertDebugLastMessage("debug", ":00");
        }

        [Fact]
        public void MessageWithExceptionTest()
        {
            LogManager.Configuration = CreateConfigurationFromString(@"
            <nlog exceptionLoggingOldStyle='true'>
                <targets><target name='debug' type='Debug' layout='${message:withException=true}' /></targets>
                <rules>
                    <logger name='*' minlevel='Debug' writeTo='debug' />
                </rules>
            </nlog>");

            ILogger logger = LogManager.GetLogger("A");
            logger.Debug("a");
            AssertDebugLastMessage("debug", "a");

            var ex = new InvalidOperationException("Exception message.");
            
#if !SILVERLIGHT
            string newline = Environment.NewLine;
#else
            string newline = "\r\n";
#endif

#pragma warning disable 0618
            // Obsolete method requires testing until completely removed.
            logger.DebugException("Foo", ex);
            AssertDebugLastMessage("debug", "Foo" + newline + ex.ToString());
#pragma warning restore 0618

            logger.Debug(ex, "Foo");
            AssertDebugLastMessage("debug", "Foo" + newline + ex.ToString());

            logger.Debug( "Foo", ex);
            AssertDebugLastMessage("debug", "Foo" + newline + ex.ToString());
        }

        [Fact]
        public void MessageWithExceptionAndCustomSeparatorTest()
        {
            LogManager.Configuration = CreateConfigurationFromString(@"
            <nlog>
                <targets><target name='debug' type='Debug' layout='${message:withException=true:exceptionSeparator=,}' /></targets>
                <rules>
                    <logger name='*' minlevel='Debug' writeTo='debug' />
                </rules>
            </nlog>");

            ILogger logger = LogManager.GetLogger("A");
            logger.Debug("a");
            AssertDebugLastMessage("debug", "a");

            var ex = new InvalidOperationException("Exception message.");
#pragma warning disable 0618
            // Obsolete method requires testing until completely removed.
            logger.DebugException("Foo", ex);
            AssertDebugLastMessage("debug", "Foo," + ex.ToString());
#pragma warning restore 0618

            logger.Debug(ex, "Foo");
            AssertDebugLastMessage("debug", "Foo," + ex.ToString());
        }
    }
}