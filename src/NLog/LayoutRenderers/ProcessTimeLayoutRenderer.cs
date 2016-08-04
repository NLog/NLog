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
    using System.Text;

    using NLog.Config;

    /// <summary>
    /// The process time in format HH:mm:ss.mmm.
    /// </summary>
    [LayoutRenderer("processtime")]
    [ThreadAgnostic]
    public class ProcessTimeLayoutRenderer : LayoutRenderer
    {
        /// <summary>
        /// Renders the current process running time and appends it to the specified <see cref="StringBuilder" />.
        /// </summary>
        /// <param name="builder">The <see cref="StringBuilder"/> to append the rendered data to.</param>
        /// <param name="logEvent">Logging event.</param>
        protected override void Append(StringBuilder builder, LogEventInfo logEvent)
        {
            TimeSpan ts = logEvent.TimeStamp.ToUniversalTime() - LogEventInfo.ZeroDate;
            var culture = GetCulture(logEvent);
            WritetTimestamp(builder, ts, culture);
        }

        /// <summary>
        /// Write timestamp to builder with format hh:mm:ss:fff
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="ts"></param>
        /// <param name="culture"></param>
        internal static void WritetTimestamp(StringBuilder builder, TimeSpan ts, CultureInfo culture)
        {
          
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


            if (ts.Hours < 10)
            {
                builder.Append('0');
            }

            builder.Append(ts.Hours);
            
            builder.Append(timeSeparator);
            if (ts.Minutes < 10)
            {
                builder.Append('0');
            }

            builder.Append(ts.Minutes);
            builder.Append(timeSeparator);
            if (ts.Seconds < 10)
            {
                builder.Append('0');
            }

            builder.Append(ts.Seconds);
          
            builder.Append(ticksSeparator);

            if (ts.Milliseconds < 100)
            {
                builder.Append('0');

                if (ts.Milliseconds < 10)
                {
                    builder.Append('0');

                    if (ts.Milliseconds < 0)
                    {
                        //don't write negative times. This is probably an accuracy problem (accuracy is by default 16ms, see https://github.com/NLog/NLog/wiki/Time-Source)
                        builder.Append('0');
                        return;
                        
                    }
                }
            }

            builder.Append(ts.Milliseconds);
        }
    }
}
