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

namespace NLog.LayoutRenderers
{
    using System;
    using System.ComponentModel;
    using System.Text;

    using NLog.Config;
    using System.Runtime.CompilerServices;

    /// <summary>
    /// The date and time in a long, sortable format yyyy-MM-dd HH:mm:ss.mmm.
    /// </summary>
    [LayoutRenderer("longdate")]
    [ThreadAgnostic]
    public class LongDateLayoutRenderer : LayoutRenderer
    {
        /// <summary>
        /// Gets or sets a value indicating whether to output UTC time instead of local time.
        /// </summary>
        /// <docgen category='Rendering Options' order='10' />
        [DefaultValue(false)]
        public bool UniversalTime { get; set; }

        /// <summary>
        /// Renders the date in the long format (yyyy-MM-dd HH:mm:ss.mmm) and appends it to the specified <see cref="StringBuilder" />.
        /// </summary>
        /// <param name="builder">The <see cref="StringBuilder"/> to append the rendered data to.</param>
        /// <param name="logEvent">Logging event.</param>
        protected override void Append(StringBuilder builder, LogEventInfo logEvent)
        {
            DateTime dt = logEvent.TimeStamp;
            if (this.UniversalTime)
            {
                dt = dt.ToUniversalTime();
            }

            long ticks = dt.Ticks;

            var charBuffer = CachedCharBuffer;

            // If the difference in ticks is more than one second (Handle Daylight-Saving-Time)
            if (Math.Abs(ticks - this.cachedCurrentSecondTicks) > TimeSpan.TicksPerSecond)
            {
                int second = dt.Second;
                Append4DigitsZeroPadded(charBuffer, 0, dt.Year);
                Append2DigitsZeroPadded(charBuffer, 5, dt.Month);
                Append2DigitsZeroPadded(charBuffer, 8, dt.Day);
                Append2DigitsZeroPadded(charBuffer, 11, dt.Hour);
                Append2DigitsZeroPadded(charBuffer, 14, dt.Minute);
                Append2DigitsZeroPadded(charBuffer, 17, dt.Second);
                this.cachedCurrentSecondTicks = new DateTime(dt.Year, dt.Month, dt.Day, dt.Hour, dt.Minute, second, 0, dt.Kind).Ticks;
            }

            Append4DigitsZeroPadded(charBuffer, 20, (int)(ticks % 10000000) / 1000);
            builder.Append(charBuffer, 0, 24);
        }

        private char[] cachedCharBuffer;
        private long cachedCurrentSecondTicks;
        private char[] CachedCharBuffer
        {
            get
            {
                if (this.cachedCharBuffer == null)
                {
                    this.cachedCharBuffer = new char[24];
                    this.cachedCharBuffer[4] = '-';
                    this.cachedCharBuffer[7] = '-';
                    this.cachedCharBuffer[10] = ' ';
                    this.cachedCharBuffer[13] = ':';
                    this.cachedCharBuffer[16] = ':';
                    this.cachedCharBuffer[19] = '.';
                }

                return this.cachedCharBuffer;
            }
        }

#if NET4_5
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        private static void Append2DigitsZeroPadded(char[] chars, int index, int number)
        {
            chars[index] = (char)((number / 10) + '0');
            chars[index + 1] = (char)((number % 10) + '0');
        }
#if NET4_5
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        private static void Append4DigitsZeroPadded(char[] chars, int index, int number)
        {
            chars[index] = (char)(((number / 1000) % 10) + '0');
            chars[index + 1] = (char)(((number / 100) % 10) + '0');
            chars[index + 2] = (char)(((number / 10) % 10) + '0');
            chars[index + 3] = (char)(((number / 1) % 10) + '0');
        }
    }
}
