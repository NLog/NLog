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
    using System.Globalization;
    using System.Text;

    using NLog.Config;

    /// <summary>
    /// The short date in a sortable format yyyy-MM-dd.
    /// </summary>
    [LayoutRenderer("shortdate")]
    [ThreadAgnostic]
    public class ShortDateLayoutRenderer : LayoutRenderer
    {

        private static readonly DateData CachedUtcDate = new DateData();
        private static readonly DateData CachedLocalDate = new DateData();

        /// <summary>
        /// Gets or sets a value indicating whether to output UTC time instead of local time.
        /// </summary>
        /// <docgen category='Rendering Options' order='10' />
        [DefaultValue(false)]
        public bool UniversalTime { get; set; }

        /// <summary>
        /// Renders the current short date string (yyyy-MM-dd) and appends it to the specified <see cref="StringBuilder" />.
        /// </summary>
        /// <param name="builder">The <see cref="StringBuilder"/> to append the rendered data to.</param>
        /// <param name="logEvent">Logging event.</param>
        protected override void Append(StringBuilder builder, LogEventInfo logEvent)
        {
            var timestamp = logEvent.TimeStamp;

            if (this.UniversalTime)
            {
                timestamp = timestamp.ToUniversalTime();
                CachedUtcDate.AppendDate(builder, timestamp);
            }
            else
            {
                CachedLocalDate.AppendDate(builder, timestamp);
            }
        }

        private class DateData
        {
            private DateTime date;
            private string formattedDate;

            /// <summary>
            /// Appends a date in format yyyy-MM-dd to the StringBuilder.
            /// The DateTime.ToString() result is cached for future uses
            /// since it only changes once a day. This optimization yields a
            /// performance boost of 40% and makes the renderer allocation-free
            /// in must cases.
            /// </summary>
            /// <param name="builder">The <see cref="StringBuilder"/> to append the date to</param>
            /// <param name="timestamp">The date to append</param>
            public void AppendDate(StringBuilder builder, DateTime timestamp)
            {
                if (formattedDate == null || date.Day != timestamp.Day || date.Month != timestamp.Month || date.Year != timestamp.Year)
                {
                    date = timestamp;
                    formattedDate = timestamp.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
                }
                builder.Append(formattedDate);
            }
        }
    }
}