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

namespace NLog.Targets
{
    using System.Collections.Generic;
    using NLog.Layouts;

    /// <summary>
    /// Sends log messages to Splunk server using either TCP or UDP with Splunk-JSON-format
    /// </summary>
    /// <remarks>
    /// <a href="https://github.com/nlog/nlog/wiki/Splunk-target">See NLog Wiki</a>
    /// </remarks>
    /// <seealso href="https://github.com/nlog/nlog/wiki/Splunk-target">Documentation on NLog Wiki</seealso>
    [Target("Splunk")]
    public class SplunkTarget : NetworkTarget
    {
        private readonly SplunkLayout _splunkLayout = new SplunkLayout();

        /// <inheritdoc cref="SplunkLayout.SplunkHostName"/>
        public Layout SplunkHostName { get => _splunkLayout.SplunkHostName; set => _splunkLayout.SplunkHostName = value; }

        /// <inheritdoc cref="SplunkLayout.SplunkSourceName"/>
        public Layout SplunkSourceName { get => _splunkLayout.SplunkSourceName; set => _splunkLayout.SplunkSourceName = value; }

        /// <inheritdoc cref="SplunkLayout.SplunkSourceType"/>
        public Layout SplunkSourceType { get => _splunkLayout.SplunkSourceType; set => _splunkLayout.SplunkSourceType = value; }

        /// <inheritdoc cref="SplunkLayout.SplunkIndex"/>
        public Layout SplunkIndex { get => _splunkLayout.SplunkIndex; set => _splunkLayout.SplunkIndex = value; }

        /// <inheritdoc cref="SplunkLayout.IncludeEventProperties"/>
        public bool IncludeEventProperties { get => _splunkLayout.IncludeEventProperties; set => _splunkLayout.IncludeEventProperties = value; }

        /// <inheritdoc cref="SplunkLayout.IncludeScopeProperties"/>
        public bool IncludeScopeProperties { get => _splunkLayout.IncludeScopeProperties; set => _splunkLayout.IncludeScopeProperties = value; }

        /// <inheritdoc cref="SplunkLayout.ExcludeEmptyProperties"/>
        public bool ExcludeEmptyProperties { get => _splunkLayout.ExcludeEmptyProperties; set => _splunkLayout.ExcludeEmptyProperties = value; }

        /// <inheritdoc cref="SplunkLayout.ExcludeProperties"/>
#if NET35
        public HashSet<string>? ExcludeProperties { get => _splunkLayout.ExcludeProperties; set => _splunkLayout.ExcludeProperties = value; }
#else
        public ISet<string>? ExcludeProperties { get => _splunkLayout.ExcludeProperties; set => _splunkLayout.ExcludeProperties = value; }
#endif

        /// <inheritdoc/>
        public override Layout Layout
        {
            get => _splunkLayout;
            set { /* Fixed SplunkLayout  */ } // NOSONAR
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SplunkTarget" /> class.
        /// </summary>
        public SplunkTarget()
        {
            LineEnding = LineEndingMode.LF;
            Layout = _splunkLayout;
        }
    }
}
