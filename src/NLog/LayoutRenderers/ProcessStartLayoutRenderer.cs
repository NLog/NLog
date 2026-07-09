//
// Copyright (c) 2004-2024 Jaroslaw Kowalski <jaak@jkowalski.net>, Kim Christensen, Julian Verdurmen
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
    /// Process start time.
    /// </summary>
    /// <remarks>
    /// <a href="https://github.com/NLog/NLog/wiki/ProcessStart-Layout-Renderer">See NLog Wiki</a>
    /// </remarks>
    /// <seealso href="https://github.com/NLog/NLog/wiki/ProcessStart-Layout-Renderer">Documentation on NLog Wiki</seealso>
    [LayoutRenderer("processstart")]
    [AppDomainFixedOutput]
    [ThreadAgnostic]
    public class ProcessStartLayoutRenderer : LayoutRenderer, IRawValue
    {
        /// <summary>
        /// Gets or sets the culture used for rendering.
        /// </summary>
        /// <remarks>Default: <see cref="CultureInfo.InvariantCulture"/></remarks>
        /// <docgen category='Layout Options' order='100' />
        public CultureInfo Culture { get; set; } = CultureInfo.InvariantCulture;

        /// <summary>
        /// Gets or sets the date format. Can be any argument accepted by DateTime.ToString(format).
        /// </summary>
        /// <remarks>Default: <c>yyyy-MM-dd HH:mm:ss</c></remarks>
        /// <docgen category='Layout Options' order='10' />
        [DefaultParameter]
        public string Format { get; set; } = "yyyy-MM-dd HH:mm:ss";

        /// <summary>
        /// Gets or sets a value indicating whether to output UTC time instead of local time.
        /// </summary>
        /// <remarks>Default: <see langword="false"/></remarks>
        /// <docgen category='Layout Options' order='50' />
        public bool UniversalTime { get; set; }

        /// <inheritdoc/>
        protected override void Append(StringBuilder builder, LogEventInfo logEvent)
        {
            AppendFormattedValue(builder, logEvent, GetValue(), Format, Culture);
        }

        bool IRawValue.TryGetRawValue(LogEventInfo logEvent, out object? value)
        {
            value = GetValue();
            return true;
        }

        private DateTime GetValue()
        {
            return UniversalTime ? NLog.LogEventInfo.ZeroDate.ToUniversalTime() : NLog.LogEventInfo.ZeroDate.ToLocalTime();
        }
    }
}
