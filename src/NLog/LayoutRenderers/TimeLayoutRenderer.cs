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

using System.Globalization;

namespace NLog.LayoutRenderers
{
    using System;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Text;

    using NLog.Config;

    /// <summary>
    /// The time in a 24-hour, sortable format HH:mm:ss.mmm.
    /// </summary>
    [LayoutRenderer("time")]
    [ThreadAgnostic]
    public class TimeLayoutRenderer : LayoutRenderer
    {
        /// <summary>
        /// Gets or sets a value indicating whether to output UTC time instead of local time.
        /// </summary>
        /// <docgen category='Rendering Options' order='10' />
        [DefaultValue(false)]
        public bool UniversalTime { get; set; }

        /// <summary>
        /// Renders time in the 24-h format (HH:mm:ss.mmm) and appends it to the specified <see cref="StringBuilder" />.
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
            
            var culture = GetCulture(logEvent);

            string timeSeparator;
            string ticksSeparator;
            if (culture != null)
            {
#if !SILVERLIGHT
                timeSeparator = culture.DateTimeFormat.TimeSeparator;
#else
                timeSeparator = ":";
#endif
                ticksSeparator = culture.NumberFormat.NumberDecimalSeparator;
            }
            else
            {
                timeSeparator = ":";
                ticksSeparator = ".";
            }

            Append2DigitsZeroPadded(builder, dt.Hour);
            builder.Append(timeSeparator);
            Append2DigitsZeroPadded(builder, dt.Minute);
            builder.Append(timeSeparator);
            Append2DigitsZeroPadded(builder, dt.Second);
            builder.Append(ticksSeparator);
            Append4DigitsZeroPadded(builder, (int)(dt.Ticks % 10000000) / 1000);
        }

        private static void Append2DigitsZeroPadded(StringBuilder builder, int number)
        {
            builder.Append((char)((number / 10) + '0'));
            builder.Append((char)((number % 10) + '0'));
        }

        private static void Append4DigitsZeroPadded(StringBuilder builder, int number)
        {
            builder.Append((char)(((number / 1000) % 10) + '0'));
            builder.Append((char)(((number / 100) % 10) + '0'));
            builder.Append((char)(((number / 10) % 10) + '0'));
            builder.Append((char)(((number / 1) % 10) + '0'));
        }
    }
}
