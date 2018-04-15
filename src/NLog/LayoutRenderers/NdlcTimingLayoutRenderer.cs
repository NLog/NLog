// 
// Copyright (c) 2004-2018 Jaroslaw Kowalski <jaak@jkowalski.net>, Kim Christensen, Julian Verdurmen
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
#if !SILVERLIGHT
    using System;
    using System.Text;
    using NLog.Config;

    /// <summary>
    /// <see cref="NestedDiagnosticsLogicalContext"/> Timing Renderer (Async scope)
    /// </summary>
    [LayoutRenderer("ndlctiming")]
    [ThreadSafe]
    public class NdlcTimingLayoutRenderer : LayoutRenderer
    {
        /// <summary>
        /// Gets or sets whether to only include the duration of the last scope created
        /// </summary>
        /// <docgen category='Rendering Options' order='10' />
        public bool CurrentScope { get; set; }

        /// <summary>
        /// Gets or sets whether to just display the scope creation time, and not the duration
        /// </summary>
        /// <docgen category='Rendering Options' order='10' />
        public bool ScopeBeginTime { get; set; }

        /// <summary>
        /// Gets or sets the TimeSpan format. Can be any argument accepted by TimeSpan.ToString(format).
        /// </summary>
        /// <docgen category='Rendering Options' order='10' />
        public string Format { get; set; }

        /// <summary>
        /// Renders the timing details of the Nested Logical Context item and appends it to the specified <see cref="StringBuilder" />.
        /// </summary>
        /// <param name="builder">The <see cref="StringBuilder"/> to append the rendered data to.</param>
        /// <param name="logEvent">Logging event.</param>
        protected override void Append(StringBuilder builder, LogEventInfo logEvent)
        {
            DateTime scopeBegin = CurrentScope ? NestedDiagnosticsLogicalContext.PeekTopScopeBeginTime() : NestedDiagnosticsLogicalContext.PeekBottomScopeBeginTime();
            if (scopeBegin != DateTime.MinValue)
            {
                if (ScopeBeginTime)
                {
                    var formatProvider = GetFormatProvider(logEvent, null);
                    scopeBegin = Time.TimeSource.Current.FromSystemTime(scopeBegin);
                    builder.Append(scopeBegin.ToString(Format, formatProvider));
                }
                else
                {
                    TimeSpan duration = scopeBegin != DateTime.MinValue ? DateTime.UtcNow - scopeBegin : TimeSpan.Zero;
                    if (duration < TimeSpan.Zero)
                        duration = TimeSpan.Zero;
#if !NET3_5
                    var formatProvider = GetFormatProvider(logEvent, null);
                    builder.Append(duration.ToString(Format, formatProvider));
#else
                    builder.Append(duration.ToString());
#endif
                }
            }
        }
    }
#endif
}