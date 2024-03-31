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
    using System.Text;
    using NLog.Config;
    using NLog.Internal;

    /// <summary>
    /// The date and time in a long, sortable format yyyy-MM-dd HH:mm:ss.ffff.
    /// </summary>
    /// <remarks>
    /// <a href="https://github.com/NLog/NLog/wiki/LongDate-Layout-Renderer">See NLog Wiki</a>
    /// </remarks>
    /// <seealso href="https://github.com/NLog/NLog/wiki/LongDate-Layout-Renderer">Documentation on NLog Wiki</seealso>
    [LayoutRenderer("longdate")]
    [ThreadAgnostic]
    public class LongDateLayoutRenderer : LayoutRenderer, IRawValue
    {
        /// <summary>
        /// Gets or sets a value indicating whether to output UTC time instead of local time.
        /// </summary>
        /// <docgen category='Layout Options' order='10' />
        public bool UniversalTime { get => _universalTime ?? false; set => _universalTime = value; }
        private bool? _universalTime;

        /// <inheritdoc/>
        protected override void Append(StringBuilder builder, LogEventInfo logEvent)
        {
            DateTime dt = GetValue(logEvent);

            //no culture according to specs

            builder.Append4DigitsZeroPadded(dt.Year);
            builder.Append('-');
            builder.Append2DigitsZeroPadded(dt.Month);
            builder.Append('-');
            builder.Append2DigitsZeroPadded(dt.Day);
            builder.Append(' ');
            builder.Append2DigitsZeroPadded(dt.Hour);
            builder.Append(':');
            builder.Append2DigitsZeroPadded(dt.Minute);
            builder.Append(':');
            builder.Append2DigitsZeroPadded(dt.Second);
            builder.Append('.');
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
