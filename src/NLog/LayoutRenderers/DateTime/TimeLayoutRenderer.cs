// 
// Copyright (c) 2004-2021 Jaroslaw Kowalski <jaak@jkowalski.net>, Kim Christensen, Julian Verdurmen
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
    using System.Globalization;
    using System.Text;
    using NLog.Config;
    using NLog.Internal;

    /// <summary>
    /// The time in a 24-hour, sortable format HH:mm:ss.mmmm.
    /// </summary>
    /// <remarks>
    /// <a href="https://github.com/NLog/NLog/wiki/Time-Layout-Renderer">See NLog Wiki</a>
    /// </remarks>
    /// <seealso href="https://github.com/NLog/NLog/wiki/Time-Layout-Renderer">Documentation on NLog Wiki</seealso>
    [LayoutRenderer("time")]
    [ThreadAgnostic]
    public class TimeLayoutRenderer : LayoutRenderer, IRawValue
    {
        /// <summary>
        /// Gets or sets a value indicating whether to output UTC time instead of local time.
        /// </summary>
        /// <docgen category='Layout Options' order='50' />
        public bool UniversalTime { get => _universalTime ?? false; set => _universalTime = value; }
        private bool? _universalTime;

        /// <summary>
        /// Gets or sets a value indicating whether to output in culture invariant format
        /// </summary>
        /// <docgen category='Layout Options' order='100' />
        public bool Invariant { get => ReferenceEquals(Culture, CultureInfo.InvariantCulture); set => Culture = value ? CultureInfo.InvariantCulture : CultureInfo.CurrentCulture; }

        /// <summary>
        /// Gets or sets the culture used for rendering. 
        /// </summary>
        /// <docgen category='Layout Options' order='100' />
        [RequiredParameter]
        public CultureInfo Culture { get; set; } = CultureInfo.InvariantCulture;

        /// <inheritdoc/>
        protected override void Append(StringBuilder builder, LogEventInfo logEvent)
        {
            var dt = GetValue(logEvent);
            var culture = GetCulture(logEvent, Culture);

            string timeSeparator = ":";
            string ticksSeparator = ".";
            if (!ReferenceEquals(culture, CultureInfo.InvariantCulture))
            {
#if !NETSTANDARD1_3 && !NETSTANDARD1_5
                timeSeparator = culture.DateTimeFormat.TimeSeparator;
#endif
                ticksSeparator = culture.NumberFormat.NumberDecimalSeparator;
            }

            builder.Append2DigitsZeroPadded(dt.Hour);
            builder.Append(timeSeparator);
            builder.Append2DigitsZeroPadded(dt.Minute);
            builder.Append(timeSeparator);
            builder.Append2DigitsZeroPadded(dt.Second);
            builder.Append(ticksSeparator);
            builder.Append4DigitsZeroPadded((int)(dt.Ticks % 10000000) / 1000);
        }

        bool IRawValue.TryGetRawValue(LogEventInfo logEvent, out object value)
        {
            value = GetValue(logEvent);
            return true;
        }

        private DateTime GetValue(LogEventInfo logEvent)
        {
            DateTime timestamp = logEvent.TimeStamp;
            if (_universalTime.HasValue)
            {
                if (_universalTime.Value)
                    timestamp = timestamp.ToUniversalTime();
                else
                    timestamp = timestamp.ToLocalTime();
            }
            return timestamp;
        }
    }
}
