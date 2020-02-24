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

using NLog.Internal;
using NLog.Layouts;
using Xunit;
using Xunit.Extensions;

namespace NLog.UnitTests.Internal
{
    public class LayoutHelpersTests
    {
        private string _eventPropertiesLayout;

        /// <summary>
        /// int.MaxValue:    2,147,483,647
        /// </summary>
        private const int DefaultInt = int.MinValue;

        /// <summary>
        /// short.MaxValue:  32767
        /// </summary>
        private const short DefaultShort = short.MinValue;


        [Theory]
        [InlineData("4", 4)]
        [InlineData(" 4", 4)] //spaces
        [InlineData("4 ", 4)]
        [InlineData("-4", -4)] //neg
        [InlineData(" -4", -4)]
        [InlineData(" -4 ", -4)]
        [InlineData("4.0 ", DefaultInt)] //no (thousand) separators
        [InlineData("4,000 ", DefaultInt)] //no (thousand) separators
        [InlineData("2147483647", int.MaxValue)]
        [InlineData("2147483648", DefaultInt)] //overflow
        [InlineData("", DefaultInt)]
        [InlineData(null, DefaultInt)]
        public void TestRenderInt(string input, int expected)
        {
            _eventPropertiesLayout = "${event-properties:val}";
            Layout layout = _eventPropertiesLayout;
            var result = layout.RenderInt(CreateLogEvent(input), DefaultInt, "event-properties");
            Assert.Equal(expected, result);
        }

        [Theory]
        [InlineData("4", (short)4)]
        [InlineData(" 4", (short)4)] //spaces
        [InlineData("4 ", (short)4)]
        [InlineData("4.0", DefaultShort)]  //no (thousand) separators
        [InlineData("4,0", DefaultShort)]  //no (thousand) separators
        [InlineData("2147483647", DefaultShort)] //overflow
        [InlineData("2147483648", DefaultShort)] //overflow
        [InlineData("32768", DefaultShort)] //overflow
        [InlineData("32767", short.MaxValue)]
        [InlineData("", DefaultShort)]
        [InlineData(null, DefaultShort)]

        public void TestRenderShort(string input, short expected)
        {
            _eventPropertiesLayout = "${event-properties:val}";
            Layout layout = _eventPropertiesLayout;
            var result = layout.RenderShort(CreateLogEvent(input), DefaultShort, "event-properties");
            Assert.Equal(expected, result);
        }


        /// <summary>
        /// test with both default. True or false, case-insensitive, no numbers
        /// </summary>
        /// <param name="input"></param>
        /// <param name="expected">null = default</param>
        [Theory]
        [InlineData("4", null)]
        [InlineData("1", null)]
        [InlineData("0", null)]
        [InlineData("-1", null)]
        [InlineData("true", true)]
        [InlineData("true ", true)]
        [InlineData(" true ", true)]
        [InlineData(" True ", true)]
        [InlineData(" TRUE ", true)]
        [InlineData("false", false)]
        [InlineData("false ", false)]
        [InlineData(" false ", false)]
        [InlineData(" False ", false)]
        [InlineData(" FALSE ", false)]
        [InlineData(" DALSE ", null)]
        
        public void TestRenderBool(string input, bool? expected)
        {
            _eventPropertiesLayout = "${event-properties:val}";
            Layout layout = _eventPropertiesLayout;
            //test with default=false
            {
                const bool defaultValue = false;
                var result1 = layout.RenderBool(CreateLogEvent(input), defaultValue, "event-properties");
                Assert.Equal(expected ?? defaultValue, result1);
            }
            //test with default=true
            {
                const bool defaultValue = true;
                var result2 = layout.RenderBool(CreateLogEvent(input), defaultValue, "event-properties");
                Assert.Equal(expected ?? defaultValue, result2);
            }
        }

        private static LogEventInfo CreateLogEvent(string input)
        {
            var logEvent = LogEventInfo.CreateNullEvent();
            logEvent.Properties["val"] = input;
            return logEvent;
        }
    }
}
