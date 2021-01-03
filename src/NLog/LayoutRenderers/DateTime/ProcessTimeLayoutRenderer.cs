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

namespace NLog.LayoutRenderers
{
    using System;
    using System.ComponentModel;
    using System.Globalization;
    using System.Text;
    using NLog.Config;
    using NLog.Internal;

    /// <summary>
    /// The process time in format HH:mm:ss.mmm.
    /// </summary>
    [LayoutRenderer("processtime")]
    [ThreadAgnostic]
    [ThreadSafe]
    public class ProcessTimeLayoutRenderer : LayoutRenderer, IRawValue
    {
        /// <summary>
        /// Gets or sets a value indicating whether to output in culture invariant format
        /// </summary>
        /// <docgen category='Rendering Options' order='10' />
        [DefaultValue(false)]
        public bool Invariant { get; set; }

        /// <inheritdoc />
        protected override void Append(StringBuilder builder, LogEventInfo logEvent)
        {
            var ts = GetValue(logEvent);
            var culture = Invariant ? null : GetCulture(logEvent);
            WritetTimestamp(builder, ts, culture);
        }

        /// <inheritdoc />
        bool IRawValue.TryGetRawValue(LogEventInfo logEvent, out object value)
        {
            value = GetValue(logEvent);
            return true;
        }

        /// <summary>
        /// Write timestamp to builder with format hh:mm:ss:fff
        /// </summary>
        internal static void WritetTimestamp(StringBuilder builder, TimeSpan ts, CultureInfo culture)
        {
            string timeSeparator = ":";
            string ticksSeparator = ".";
            if (culture != null)
            {
#if !NETSTANDARD1_3 && !NETSTANDARD1_5
                timeSeparator = culture.DateTimeFormat.TimeSeparator;
#endif
                ticksSeparator = culture.NumberFormat.NumberDecimalSeparator;
            }

            builder.Append2DigitsZeroPadded(ts.Hours);
            builder.Append(timeSeparator);
            builder.Append2DigitsZeroPadded(ts.Minutes);
            builder.Append(timeSeparator);
            builder.Append2DigitsZeroPadded(ts.Seconds);
            builder.Append(ticksSeparator);
            int milliseconds = ts.Milliseconds;
            if (milliseconds < 100)
            {
                builder.Append('0');

                if (milliseconds < 10)
                {
                    builder.Append('0');

                    if (milliseconds < 0)
                    {
                        //don't write negative times. This is probably an accuracy problem (accuracy is by default 16ms, see https://github.com/NLog/NLog/wiki/Time-Source)
                        builder.Append('0');
                        return;
                    }
                }
            }

            builder.AppendInvariant(milliseconds);
        }

        private static TimeSpan GetValue(LogEventInfo logEvent)
        {
            TimeSpan ts = logEvent.TimeStamp.ToUniversalTime() - LogEventInfo.ZeroDate;
            return ts;
        }
    }
}
