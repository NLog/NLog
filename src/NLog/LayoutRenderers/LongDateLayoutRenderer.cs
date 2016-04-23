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

            var charArray = Chars;
            if (dt.Ticks > this.cachedCurrentYearTicks)
            {
                int year = dt.Year;
                this.cachedCurrentYearTicks = new DateTime(year, 12, 31, 23, 59, 59, 999, dt.Kind).Ticks;
                Append4DigitsZeroPadded(charArray, 0, year);
            }

            if (ticks > this.cachedCurrentMonthTicks)
            {
                int month = dt.Month;
                int year = dt.Year;
                Append2DigitsZeroPadded(charArray, 5, month);
                this.cachedCurrentMonthTicks = new DateTime(year, month, DateTime.DaysInMonth(year, month), 23, 59, 59, 999, dt.Kind).Ticks;
            }

            if (ticks > this.cachedCurrentDayTicks)
            {
                int month = dt.Month;
                int year = dt.Year;
                int day = dt.Day;

                this.cachedCurrentDayTicks = new DateTime(year, month, day, 23, 59, 59, 999, dt.Kind).Ticks;
                Append2DigitsZeroPadded(charArray, 8, dt.Day);
            }

            if (ticks > this.cachedCurrentHourTicks)
            {
                int hour = dt.Hour;
                Append2DigitsZeroPadded(charArray, 11, dt.Hour);
                this.cachedCurrentHourTicks = new DateTime(dt.Year, dt.Month, dt.Day, hour, 59, 59, 999, dt.Kind).Ticks;
            }

            if (ticks > this.cachedCurrentMinuteTicks)
            {
                int minute = dt.Minute;
                Append2DigitsZeroPadded(charArray, 14, minute);
                this.cachedCurrentMinuteTicks = new DateTime(dt.Year, dt.Month, dt.Day, dt.Hour, minute, 59, 999, dt.Kind).Ticks;
            }

            if (ticks > this.cachedCurrentSecondTicks)
            {
                int second = dt.Second;
                Append2DigitsZeroPadded(charArray, 17, second);
                this.cachedCurrentSecondTicks = new DateTime(dt.Year, dt.Month, 1, dt.Hour, dt.Minute, second, 999, dt.Kind).Ticks;
            }

            Append4DigitsZeroPadded(charArray, 20, (int)(ticks % 10000000) / 1000);
            builder.Append(charArray, 0, 24);
        }

        private char[] charArray;
        private long cachedCurrentYearTicks;
        private long cachedCurrentMonthTicks;
        private long cachedCurrentDayTicks;
        private long cachedCurrentHourTicks;
        private long cachedCurrentMinuteTicks;
        private long cachedCurrentSecondTicks;
        private char[] Chars
        {
            get
            {
                if (this.charArray == null)
                {
                    this.charArray = new char[24];
                    this.charArray[4] = '-';
                    this.charArray[7] = '-';
                    this.charArray[10] = ' ';
                    this.charArray[13] = ':';
                    this.charArray[16] = ':';
                    this.charArray[19] = '.';
                }

                return this.charArray;
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
