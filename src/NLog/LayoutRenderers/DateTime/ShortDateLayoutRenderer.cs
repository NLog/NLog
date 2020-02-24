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
    /// The short date in a sortable format yyyy-MM-dd.
    /// </summary>
    [LayoutRenderer("shortdate")]
    [ThreadAgnostic]
    [ThreadSafe]
    public class ShortDateLayoutRenderer : LayoutRenderer, IRawValue, IStringValueRenderer
    {
        /// <summary>
        /// Gets or sets a value indicating whether to output UTC time instead of local time.
        /// </summary>
        /// <docgen category='Rendering Options' order='10' />
        [DefaultValue(false)]
        public bool UniversalTime { get; set; }

        private CachedDateFormatted _cachedDateFormatted = new CachedDateFormatted(DateTime.MaxValue, string.Empty);

        /// <summary>
        /// Renders the current short date string (yyyy-MM-dd) and appends it to the specified <see cref="StringBuilder" />.
        /// </summary>
        /// <param name="builder">The <see cref="StringBuilder"/> to append the rendered data to.</param>
        /// <param name="logEvent">Logging event.</param>
        protected override void Append(StringBuilder builder, LogEventInfo logEvent)
        {
            string formattedDate = GetStringValue(logEvent);
            builder.Append(formattedDate);
        }

        private string GetStringValue(LogEventInfo logEvent)
        {
            DateTime timestamp = GetValue(logEvent);

            var cachedDateFormatted = _cachedDateFormatted;
            if (cachedDateFormatted.Date != timestamp.Date)
            {
                var formatTime = timestamp.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
                _cachedDateFormatted = cachedDateFormatted = new CachedDateFormatted(timestamp.Date, formatTime);
            }

            return cachedDateFormatted.FormattedDate;
        }

        private DateTime GetValue(LogEventInfo logEvent)
        {
            var timestamp = logEvent.TimeStamp;
            if (UniversalTime)
            {
                timestamp = timestamp.ToUniversalTime();
            }
            return timestamp;
        }

        bool IRawValue.TryGetRawValue(LogEventInfo logEvent, out object value)
        {
            value = GetValue(logEvent).Date;    // Only Date-part
            return true;
        }

        string IStringValueRenderer.GetFormattedString(LogEventInfo logEvent) => GetStringValue(logEvent);

        private class CachedDateFormatted
        {
            public CachedDateFormatted(DateTime date, string formattedDate)
            {
                Date = date;
                FormattedDate = formattedDate;
            }

            public readonly DateTime Date;
            public readonly string FormattedDate;
        }
    }
}