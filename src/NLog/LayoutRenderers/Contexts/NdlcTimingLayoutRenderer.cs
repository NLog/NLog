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

    /// <summary>
    /// Obsolete and replaced by <see cref="ScopeContextTimingLayoutRenderer"/> with NLog v5.
    /// Render Nested Diagnostic Context (NDLC) timings from <see cref="NestedDiagnosticsLogicalContext"/>
    /// </summary>
    /// <remarks>
    /// <a href="https://github.com/NLog/NLog/wiki/NdlcTiming-Layout-Renderer">See NLog Wiki</a>
    /// </remarks>
    /// <seealso href="https://github.com/NLog/NLog/wiki/NdlcTiming-Layout-Renderer">Documentation on NLog Wiki</seealso>
    [LayoutRenderer("ndlctiming")]
    [Obsolete("Replaced by ScopeContextTimingLayoutRenderer ${scopetiming}. Marked obsolete on NLog 5.0")]
    public class NdlcTimingLayoutRenderer : LayoutRenderer
    {
        /// <summary>
        /// Gets or sets whether to only include the duration of the last scope created
        /// </summary>
        /// <docgen category='Layout Options' order='10' />
        public bool CurrentScope { get; set; }

        /// <summary>
        /// Gets or sets whether to just display the scope creation time, and not the duration
        /// </summary>
        /// <docgen category='Layout Options' order='10' />
        public bool ScopeBeginTime { get; set; }

        /// <summary>
        /// Gets or sets the TimeSpan format. Can be any argument accepted by TimeSpan.ToString(format).
        /// </summary>
        /// <docgen category='Layout Options' order='10' />
        public string Format { get; set; }

        /// <inheritdoc/>
        protected override void Append(StringBuilder builder, LogEventInfo logEvent)
        {
            TimeSpan? scopeDuration = CurrentScope ? ScopeContext.PeekInnerNestedDuration() : ScopeContext.PeekOuterNestedDuration();
            if (scopeDuration.HasValue)
            {
                if (scopeDuration.Value < TimeSpan.Zero)
                    scopeDuration = TimeSpan.Zero;

                if (ScopeBeginTime)
                {
                    var formatProvider = GetFormatProvider(logEvent, null);
                    var scopeBegin = Time.TimeSource.Current.Time.Subtract(scopeDuration.Value);
                    builder.Append(scopeBegin.ToString(Format, formatProvider));
                }
                else
                {
#if !NET35
                    var formatProvider = GetFormatProvider(logEvent, null);
                    builder.Append(scopeDuration.Value.ToString(Format, formatProvider));
#else
                    builder.Append(scopeDuration.Value.ToString());
#endif
                }
            }
        }
    }
}