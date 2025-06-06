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

namespace NLog.Filters
{
    using System;

    /// <summary>
    /// Matches when the calculated layout does NOT contain the specified substring.
    /// This filter is deprecated in favor of <c>&lt;when /&gt;</c> which is based on <a href="https://github.com/NLog/NLog/wiki/Conditions">conditions</a>.
    /// </summary>
    [Filter("whenNotContains")]
    public class WhenNotContainsFilter : LayoutBasedFilter
    {
        /// <summary>
        /// Gets or sets the substring to be matched.
        /// </summary>
        /// <docgen category='Filtering Options' order='10' />
        public string Substring { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets a value indicating whether to ignore case when comparing strings.
        /// </summary>
        /// <docgen category='Filtering Options' order='10' />
        public bool IgnoreCase { get; set; }

        /// <inheritdoc/>
        protected override FilterResult Check(LogEventInfo logEvent)
        {
            StringComparison comparison = IgnoreCase
                                              ? StringComparison.OrdinalIgnoreCase
                                              : StringComparison.Ordinal;
            string result = Layout.Render(logEvent);
            if (!string.IsNullOrEmpty(Substring) && result.IndexOf(Substring, comparison) < 0)
            {
                return Action;
            }

            return FilterResult.Neutral;
        }
    }
}
