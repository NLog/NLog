using System.Collections.Generic;
using System.Text;
using NLog.Config;
using NLog.Layouts;

namespace NLog.Targets
{
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

        /// <inheritdoc cref="SyslogLayout.SyslogSeverity"/>
        public Layout<SyslogSeverity> SyslogSeverity { get => _syslogLayout.SyslogSeverity; set => _syslogLayout.SyslogSeverity = value; }

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
        /// Initializes a new instance of the <see cref="NetworkTarget" /> class.
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

            if (address?.StartsWith("udp", System.StringComparison.OrdinalIgnoreCase) == true)
                return null;

            var octetCount = payload.Length;
            return Encoding.ASCII.GetBytes($"{octetCount} ");
        }

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
