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
    using System.Text;
    using NLog.Config;
    using NLog.Layouts;

    /// <summary>
    /// Sends log messages to Syslog server using either TCP or UDP with format Rfc3164 or Rfc5424
    /// </summary>
    /// <remarks>
    /// When using TCP then the default message-delimeter is octet-byte-count prefix, but it can be changed
    /// by setting <see cref="NetworkTarget.LineEnding"/> to <see cref="LineEndingMode.LF"/> or <see cref="LineEndingMode.Null"/>
    /// 
    /// <a href="https://github.com/nlog/nlog/wiki/Syslog-target">See NLog Wiki</a>
    /// </remarks>
    /// <seealso href="https://github.com/nlog/nlog/wiki/Syslog-target">Documentation on NLog Wiki</seealso>
    [Target("Syslog")]
    public class SyslogTarget : NetworkTarget
    {
        private readonly SyslogLayout _syslogLayout = new SyslogLayout();

        /// <inheritdoc cref="SyslogLayout.Rfc3164"/>
        public bool Rfc3164 { get => _syslogLayout.Rfc3164; set => _syslogLayout.Rfc3164 = value; }

        /// <inheritdoc cref="SyslogLayout.Rfc5424"/>
        public bool Rfc5424 { get => _syslogLayout.Rfc5424; set => _syslogLayout.Rfc5424 = value; }

        /// <inheritdoc cref="SyslogLayout.SyslogTimestamp"/>
        public Layout SyslogTimestamp { get => _syslogLayout.SyslogTimestamp; set => _syslogLayout.SyslogTimestamp = value; }

        /// <inheritdoc cref="SyslogLayout.SyslogHostName"/>
        public Layout SyslogHostName { get => _syslogLayout.SyslogHostName; set => _syslogLayout.SyslogHostName = value; }

        /// <inheritdoc cref="SyslogLayout.SyslogAppName"/>
        public Layout SyslogAppName { get => _syslogLayout.SyslogAppName; set => _syslogLayout.SyslogAppName = value; }

        /// <inheritdoc cref="SyslogLayout.SyslogProcessId"/>
        public Layout SyslogProcessId { get => _syslogLayout.SyslogProcessId; set => _syslogLayout.SyslogProcessId = value; }

        /// <inheritdoc cref="SyslogLayout.SyslogMessageId"/>
        public Layout SyslogMessageId { get => _syslogLayout.SyslogMessageId; set => _syslogLayout.SyslogMessageId = value; }

        /// <inheritdoc cref="SyslogLayout.SyslogMessage"/>
        public Layout SyslogMessage { get => _syslogLayout.SyslogMessage; set => _syslogLayout.SyslogMessage = value; }

        /// <inheritdoc cref="SyslogLayout.SyslogLevel"/>
        public Layout<SyslogLevel> SyslogLevel { get => _syslogLayout.SyslogLevel; set => _syslogLayout.SyslogLevel = value; }

        /// <inheritdoc cref="SyslogLayout.SyslogFacility"/>
        public SyslogFacility SyslogFacility { get => _syslogLayout.SyslogFacility; set => _syslogLayout.SyslogFacility = value; }

        /// <inheritdoc cref="SyslogLayout.StructuredDataId"/>
        public Layout StructuredDataId { get => _syslogLayout.StructuredDataId; set => _syslogLayout.StructuredDataId = value; }

        /// <inheritdoc cref="SyslogLayout.IncludeEventProperties"/>
        public bool IncludeEventProperties { get => _syslogLayout.IncludeEventProperties; set => _syslogLayout.IncludeEventProperties = value; }

        /// <inheritdoc cref="SyslogLayout.StructuredDataParams"/>
        [ArrayParameter(typeof(TargetPropertyWithContext), "StructuredDataParam")]
        public List<TargetPropertyWithContext> StructuredDataParams => _syslogLayout.StructuredDataParams;

        /// <summary>
        /// Initializes a new instance of the <see cref="SyslogTarget" /> class.
        /// </summary>
        public SyslogTarget()
        {
            LineEnding = LineEndingMode.None;
            Layout = _syslogLayout;
        }

        /// <inheritdoc/>
        protected override byte[] GetHeaderToWrite(LogEventInfo logEvent, string address, byte[] payload)
        {
            if (LineEnding?.NewLineCharacters?.Length > 0)
                return null;

            if (address?.StartsWith("tcp", System.StringComparison.OrdinalIgnoreCase) == true)
            {
                var octetCount = payload.Length;
                return GenerateOctetHeader(octetCount);
            }

            // Skip octet framing for UDP protocols by returning null
            return null;
        }

        private static byte[] GenerateOctetHeader(int octetCount)
        {
            if (octetCount < OctetHeaders.Length)
            {
                var headerBytes = OctetHeaders[octetCount];
                if (headerBytes is null)
                    OctetHeaders[octetCount] = headerBytes = Encoding.ASCII.GetBytes($"{octetCount} ");
                return headerBytes;
            }

            return Encoding.ASCII.GetBytes($"{octetCount} ");
        }
        private static readonly byte[][] OctetHeaders = new byte[4046][];

        /// <inheritdoc/>
        public override Layout Layout
        {
            get
            {
                return _syslogLayout;
            }
            set
            {
                // Fixed SyslogLayout
            }
        }
    }
}
