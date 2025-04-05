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
    using NLog.Config;
    using NLog.Layouts;

    /// <summary>
    /// Sends log messages to GrayLog server using either TCP or UDP with GELF-format
    /// </summary>
    [Target("Gelf")]
    public class GelfTarget : NetworkTarget
    {
        private readonly GelfLayout _gelfLayout = new GelfLayout();

        /// <inheritdoc cref="GelfLayout.GelfHostName"/>
        public Layout GelfHostName { get => _gelfLayout.GelfHostName; set => _gelfLayout.GelfHostName = value; }

        /// <inheritdoc cref="GelfLayout.GelfShortMessage"/>
        public Layout GelfShortMessage { get => _gelfLayout.GelfShortMessage; set => _gelfLayout.GelfShortMessage = value; }

        /// <inheritdoc cref="GelfLayout.GelfFullMessage"/>
        public Layout GelfFullMessage { get => _gelfLayout.GelfFullMessage; set => _gelfLayout.GelfFullMessage = value; }

        /// <inheritdoc cref="GelfLayout.GelfFacility"/>
        public Layout GelfFacility { get => _gelfLayout.GelfFacility; set => _gelfLayout.GelfFacility = value; }

        /// <inheritdoc cref="GelfLayout.GelfFields"/>
        [ArrayParameter(typeof(TargetPropertyWithContext), "GelfField")]
        public List<TargetPropertyWithContext> GelfFields { get => _gelfLayout.GelfFields; }

        /// <inheritdoc cref="GelfLayout.IncludeEventProperties"/>
        public bool IncludeEventProperties { get => _gelfLayout.IncludeEventProperties; set => _gelfLayout.IncludeEventProperties = value; }

        /// <inheritdoc cref="GelfLayout.IncludeScopeProperties"/>
        public bool IncludeScopeProperties { get => _gelfLayout.IncludeScopeProperties; set => _gelfLayout.IncludeScopeProperties = value; }

        /// <inheritdoc cref="GelfLayout.ExcludeEmptyProperties"/>
        public bool ExcludeEmptyProperties { get => _gelfLayout.ExcludeEmptyProperties; set => _gelfLayout.ExcludeEmptyProperties = value; }

        /// <inheritdoc cref="GelfLayout.ExcludeProperties"/>
        public ISet<string> ExcludeProperties { get => _gelfLayout.ExcludeProperties; }

        /// <inheritdoc cref="GelfLayout.IncludeProperties"/>
        public ISet<string> IncludeProperties { get => _gelfLayout.IncludeProperties; }

        /// <inheritdoc/>
        public override Layout Layout
        {
            get
            {
                return _gelfLayout;
            }
            set
            {
                // Fixed GelfLayout
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="GelfTarget" /> class.
        /// </summary>
        public GelfTarget()
        {
            LineEnding = LineEndingMode.Null; // Graylog Server oftens uses NUL-byte as message-delimiter for TCP (but prevents using compression)
            NewLine = false;    // LineEnding must be explicit enabled when using TCP
            Layout = _gelfLayout;
        }
    }
}
