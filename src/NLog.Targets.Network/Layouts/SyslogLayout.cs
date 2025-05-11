using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NLog.Config;
using NLog.LayoutRenderers;
using NLog.Targets;

namespace NLog.Layouts
{
    /// <summary>
    /// A specialized layout that renders Syslog-formatted events in format Rfc3164 / Rfc5424
    /// </summary>
    [Layout("SyslogLayout")]
    [ThreadAgnostic]
    [AppDomainFixedOutput]
    public class SyslogLayout : CompoundLayout
    {
        private const string Rfc5424DefaultVersion = "1";
        private const string Rfc3164TimestampFormat = "{0:MMM} {0,11:d HH:mm:ss}";
        private const string NilValue = "-";
        private const int HostNameMaxLength = 255;
        private const int AppNameMaxLength = 48;
        private const int ProcessIdMaxLength = 128;

        private IValueFormatter ValueFormatter
        {
            get => _valueFormatter ?? (_valueFormatter = ResolveService<IValueFormatter>());
            set => _valueFormatter = value;
        }
        private IValueFormatter _valueFormatter;

        /// <summary>
        /// Gets or sets a value indicating whether to use RFC 3164 for Syslog Format
        /// </summary>
        /// <remarks><see href="https://datatracker.ietf.org/doc/html/rfc3164"/></remarks>
        public bool Rfc3164 { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to use RFC 5424 for Syslog Format
        /// </summary>
        /// <remarks><see href="https://datatracker.ietf.org/doc/html/rfc5424"/></remarks>
        public bool Rfc5424 { get; set; }

        /// <summary>
        /// Gets or sets a value indicating what DateTime format should be used when <see cref="Rfc5424"/> = true
        /// </summary>
        public Layout SyslogTimestamp { get; set; } = "${date:format=o}";

        /// <summary>
        /// The FQDN or IPv4 address or IPv6 address or hostname of the sender machine (Optional)
        /// </summary>
        /// <remarks>
        /// RFC 5424 - NILVALUE or 1 to 255 PRINTUSASCII
        /// </remarks>
        public Layout SyslogHostName
        {
            get => _hostName;
            set
            {
                _hostName = value;
                _hostNameString = _hostName is SimpleLayout simpleLayout && simpleLayout.IsFixedText ? EscapePropertyName(simpleLayout.FixedText, HostNameMaxLength) : null;
            }
        }
        private Layout _hostName;
        private string _hostNameString;

        /// <summary>
        /// Name of the device / application / process sending the Syslog-message (Optional)
        /// </summary>
        /// <remarks>
        /// <para>RFC 3164 - Tag-Name - Alphanumeric string not exceeding 32 characters</para>
        /// <para>RFC 5424 - NILVALUE or 1 to 48 PRINTUSASCII</para>
        /// </remarks>
        public Layout SyslogAppName
        {
            get => _appName;
            set
            {
                _appName = value;
                _appNameString = _appName is SimpleLayout simpleLayout && simpleLayout.IsFixedText ? EscapePropertyName(simpleLayout.FixedText, AppNameMaxLength) : null;
            }
        }
        private Layout _appName;
        private string _appNameString;

        /// <summary>
        /// Process Id or Process Name or Logger Name (Optional)
        /// </summary>
        /// <remarks>
        /// RFC 5424 - NILVALUE or 1 to 128 PRINTUSASCII
        /// </remarks>
        public Layout SyslogProcessId
        {
            get => _processId;
            set
            {
                _processId = value;
                _processIdString = _processId is SimpleLayout simpleLayout && simpleLayout.IsFixedText ? EscapePropertyName(simpleLayout.FixedText, ProcessIdMaxLength) : null;
            }
        }
        private Layout _processId;
        private string _processIdString;

        /// <summary>
        /// The type of message that should be the same for events with the same semantics. Ex ${event-properties:EventId} (Optional)
        /// </summary>
        /// <remarks>
        /// RFC 5424 - NILVALUE or 1 to 32 PRINTUSASCII
        /// </remarks>
        public Layout SyslogMessageId { get; set; }

        /// <summary>
        /// Mesage Payload
        /// </summary>
        public Layout SyslogMessage { get; set; } = "${message}${onexception:|}${exception:format=shortType,message}";

        /// <summary>
        /// Message Severity
        /// </summary>
        public Layout<SyslogLevel> SyslogLevel { get; set; } = Layout<SyslogLevel>.FromMethod(l => ToSyslogLevel(l.Level), LayoutRenderOptions.ThreadAgnostic);

        /// <summary>
        /// Device Facility
        /// </summary>
        public SyslogFacility SyslogFacility { get; set; } = SyslogFacility.User;

        /// <summary>
        /// Gets or sets the prefix for StructuredData when <see cref="Rfc5424"/> = true
        /// </summary>
        public Layout StructuredDataId { get; set; } = "meta";

        /// <summary>
        /// Gets or sets a value indicating whether LogEvent Properties should be included for StructuredData when <see cref="Rfc5424"/> = true
        /// </summary>
        public bool IncludeEventProperties { get; set; }

        /// <summary>
        /// List of StructuredData Parameters to include when <see cref="Rfc5424"/> = true
        /// </summary>
        [ArrayParameter(typeof(TargetPropertyWithContext), "StructuredDataParam")]
        public List<TargetPropertyWithContext> StructuredDataParams { get; } = new List<TargetPropertyWithContext>();

        private KeyValuePair<SyslogFacility, Dictionary<SyslogLevel, string>> _priValueMapping;

        /// <summary>
        /// Disables <see cref="ThreadAgnosticAttribute"/> to capture volatile LogEvent-properties from active thread context
        /// </summary>
        public LayoutRenderer DisableThreadAgnostic => IncludeEventProperties ? _enableThreadAgnosticImmutable : null;
        private static readonly LayoutRenderer _enableThreadAgnosticImmutable = new ExceptionDataLayoutRenderer() { Item = " " };

        /// <summary>
        /// Initializes a new instance of the <see cref="SyslogLayout" /> class.
        /// </summary>
        public SyslogLayout()
        {
            SyslogHostName = "${hostname}";
            SyslogAppName = "${processname}";
            SyslogProcessId = "${processid}";
        }

        /// <inheritdoc/>
        protected override void InitializeLayout()
        {
            // CompoundLayout includes optimization, so only doing precalculate/caching of relevant Layouts (instead of the entire SysLog-message)
            Layouts.Clear();
            if (!IncludeEventProperties)
            {
                if (SyslogTimestamp != null)
                    Layouts.Add(SyslogTimestamp);
                if (SyslogHostName != null)
                    Layouts.Add(SyslogHostName);
                if (SyslogAppName != null)
                    Layouts.Add(SyslogAppName);
                if (SyslogProcessId != null)
                    Layouts.Add(SyslogProcessId);
                if (SyslogMessageId != null)
                    Layouts.Add(SyslogMessageId);
                if (SyslogMessage != null)
                    Layouts.Add(SyslogMessage);
                if (SyslogLevel != null)
                    Layouts.Add(SyslogLevel);
                if (StructuredDataId != null)
                    Layouts.Add(StructuredDataId);                
                for (int i = 0; i < StructuredDataParams.Count; ++i)
                    Layouts.Add(StructuredDataParams[i].Layout);
            }

            base.InitializeLayout();
        }

        /// <inheritdoc/>
        protected override void CloseLayout()
        {
            ValueFormatter = null;
        }

        /// <inheritdoc/>
        protected override string GetFormattedMessage(LogEventInfo logEvent)
        {
            var stringBuilder = new StringBuilder(128);
            RenderFormattedMessage(logEvent, stringBuilder);
            return stringBuilder.ToString();
        }

        /// <inheritdoc/>
        protected override void RenderFormattedMessage(LogEventInfo logEvent, StringBuilder target)
        {
            var syslogLevel = SyslogLevel.RenderValue(logEvent);
            var facility = SyslogFacility;

            var priValueMapper = _priValueMapping;
            if (priValueMapper.Key != facility || priValueMapper.Value is null)
            {
                priValueMapper = new KeyValuePair<SyslogFacility, Dictionary<SyslogLevel, string>>(facility, ResolveFacilityMapper(facility));
                _priValueMapping = priValueMapper;
            }

            var priValue = priValueMapper.Value[syslogLevel];
            var hostName = _hostNameString ?? EscapePropertyName(SyslogHostName?.Render(logEvent) ?? string.Empty, HostNameMaxLength);
            var appName = _appNameString ?? EscapePropertyName(SyslogAppName?.Render(logEvent) ?? string.Empty, AppNameMaxLength);
            var processId = _processIdString ?? EscapePropertyName(SyslogProcessId?.Render(logEvent) ?? string.Empty, ProcessIdMaxLength);

            if (Rfc3164 || !Rfc5424)
            {
                Render_Rfc3164(logEvent, target, priValue, hostName, appName, processId);
            }
            else
            {
                Render_Rfc5424(logEvent, target, priValue, hostName, appName, processId);
            }
        }

        private void Render_Rfc3164(LogEventInfo logEvent, StringBuilder target, string priValue, string hostName, string appName, string processId)
        {
            target.Append(priValue);
            target.AppendFormat(System.Globalization.CultureInfo.InvariantCulture, Rfc3164TimestampFormat, logEvent.TimeStamp);
            target.Append(' ');
            target.Append(string.IsNullOrEmpty(hostName) ? NilValue : hostName);
            target.Append(' ');
            if (appName.Length > 32)
                target.Append(appName, 0, 32);  // Rfc3164 TAG
            else
                target.Append(string.IsNullOrEmpty(appName) ? NilValue : appName);  // Rfc3164 TAG

            if (!string.IsNullOrEmpty(processId))
            {
                target.Append('[');
                target.Append(processId);
                target.Append(']');
            }
            target.Append(':');
            target.Append(' ');

            int startIndex = target.Length;
            SyslogMessage.Render(logEvent, target);

            bool removeDuplicates = false;

            for (int i = startIndex; i < target.Length; ++i)
            {
                var chr = target[i];
                if (ToAscii(chr) != chr)
                {
                    target[i] = ToAscii(chr);
                    removeDuplicates = true;
                }
            }

            if (removeDuplicates)
            {
                var fixedString = target.ToString(startIndex, target.Length - startIndex);
                fixedString = fixedString.Replace("??", "?").Replace("  ", " ");
                if (fixedString.Length >= 1000)
                    fixedString = fixedString.Trim(' ', '?', '_');
                target.Length = startIndex;
                target.Append(fixedString);
            }
        }

        private void Render_Rfc5424(LogEventInfo logEvent, StringBuilder target, string priValue, string hostName, string appName, string processId)
        {
            var msgId = EscapePropertyName(SyslogMessageId?.Render(logEvent) ?? string.Empty, 32);
            if (msgId?.IndexOf(' ') >= 0)
                msgId = msgId.Trim().Replace(' ', '_');

            target.Append(priValue);
            target.Append(Rfc5424DefaultVersion);
            target.Append(' ');
            SyslogTimestamp.Render(logEvent, target);
            target.Append(' ');
            target.Append(string.IsNullOrEmpty(hostName) ? NilValue : hostName);
            target.Append(' ');
            target.Append(string.IsNullOrEmpty(appName) ? NilValue : appName);
            target.Append(' ');
            target.Append(string.IsNullOrEmpty(processId) ? NilValue : processId);
            target.Append(' ');
            target.Append(string.IsNullOrEmpty(msgId) ? NilValue : msgId);

            var structuredDataId = EscapePropertyName(StructuredDataId?.Render(logEvent) ?? string.Empty);
            if (!string.IsNullOrEmpty(structuredDataId))
            {
                if (IncludeEventProperties && logEvent.HasProperties)
                {
                    foreach (var eventProperty in logEvent.Properties)
                    {
                        var propertyName = EscapePropertyName(eventProperty.Key?.ToString() ?? string.Empty, 32);
                        if (string.IsNullOrEmpty(propertyName))
                            continue;

                        structuredDataId = AppendPropertyName(target, structuredDataId, propertyName);

                        var propertyValue = eventProperty.Value;
                        AppendPropertyValue(target, propertyValue);
                    }
                }

                foreach (var sdParam in StructuredDataParams)
                {
                    var sdParamName = EscapePropertyName(sdParam.Name ?? string.Empty, 32);
                    if (string.IsNullOrEmpty(sdParamName))
                        continue;

                    var sdParamValue = sdParam.RenderValue(logEvent);
                    if (!sdParam.IncludeEmptyValue && (sdParamValue is null || string.Empty.Equals(sdParamValue)))
                        continue;

                    structuredDataId = AppendPropertyName(target, structuredDataId, sdParamName);
                    AppendPropertyValue(target, sdParamValue);
                }

                if (string.IsNullOrEmpty(structuredDataId))
                {
                    target.Append(']');
                }
            }

            target.Append(' ');
            SyslogMessage.Render(logEvent, target);
        }

        private static string AppendPropertyName(StringBuilder target, string structuredDataId, string propertyName)
        {
            if (!string.IsNullOrEmpty(structuredDataId))
            {
                target.Append(' ');
                target.Append('[');
                target.Append(structuredDataId);
                structuredDataId = string.Empty;
            }
            target.Append(' ');
            target.Append(propertyName);
            target.Append('=');
            return structuredDataId;
        }

        private void AppendPropertyValue(StringBuilder target, object propertyValue)
        {
            target.Append('"');

            if (propertyValue is IConvertible convertPropertyValue)
            {
                propertyValue = EscapePropertyValue(convertPropertyValue);
                ValueFormatter.FormatValue(propertyValue, null, MessageTemplates.CaptureType.Unknown, System.Globalization.CultureInfo.InvariantCulture, target);
            }
            else
            {
                var propertyStartIndex = target.Length;
                ValueFormatter.FormatValue(propertyValue, null, MessageTemplates.CaptureType.Unknown, System.Globalization.CultureInfo.InvariantCulture, target);
                for (int i = propertyStartIndex; i < target.Length; ++i)
                {
                    var chr = target[i];
                    if (IsSpecialChar(chr))
                    {
                        var stringValue = target.ToString(propertyStartIndex, target.Length - propertyStartIndex);
                        target.Length = propertyStartIndex;
                        target.Append(EscapePropertyValue(stringValue).ToString());
                        break;
                    }
                }
            }

            target.Append('"');
        }

        private static IConvertible EscapePropertyValue(IConvertible propertyValue)
        {
            var typeCode = propertyValue.GetTypeCode();
            switch (typeCode)
            {
                case TypeCode.Char:
                    {
                        var charValue = propertyValue.ToChar(System.Globalization.CultureInfo.CurrentCulture);
                        if (IsSpecialChar(charValue))
                            return char.IsWhiteSpace(charValue) ? " " : $"\\{charValue}";
                        else
                            return propertyValue;
                    }
                case TypeCode.String:
                    {
                        var stringValue = propertyValue.ToString();
                        if (stringValue.IndexOfAny(SpecialChars) >= 0)
                            return stringValue.Replace("\\", "\\\\").Replace("[", "\\[").Replace("]", "\\]").Replace("=", "\\=").Replace("\"", "\\\"").Replace("\n", " ").Replace("\r", "");
                        else
                            return stringValue;
                    }
                case TypeCode.DBNull: return string.Empty;
                case TypeCode.Empty: return string.Empty;
                default:
                    return propertyValue;
            }
        }

        private static string EscapePropertyName(string unicodeValue, int maxLength = 0)
        {
            if (string.IsNullOrEmpty(unicodeValue))
                return string.Empty;

            foreach (var chr in unicodeValue)
            {
                if (ToAscii(chr) == chr && !IsSpecialChar(chr) && chr != ' ')
                    continue;

                unicodeValue = new string(unicodeValue.Select(c => ToAsciiFixWhiteSpace(c)).Where(c => !IsSpecialChar(c)).ToArray()).Replace("??", "?").Trim('_').Replace("__", "_");
                break;
            }

            if (maxLength > 0 && unicodeValue.Length > maxLength)
            {
                unicodeValue = unicodeValue.Substring(0, maxLength);
            }

            return unicodeValue;
        }

        private static readonly char[] SpecialChars = new[] { '[', ']', '"', '\\', '=', '\r', '\n' };

        private static bool IsSpecialChar(char c)
        {
            switch (c)
            {
                case '[':
                case ']':
                case '"':
                case '\'':
                case '=':
                case '\r':
                case '\n':
                    return true;
                default:
                    return false;
            }
        }

        private static char ToAsciiFixWhiteSpace(char c)
        {
            c = ToAscii(c);
            return c == ' ' ? '_' : c;
        }

        private static char ToAscii(char c)
        {
            if (char.IsWhiteSpace(c))
                return ' '; // newlines are also whitespace
            if (!IsAscii(c))
                return '?';
            if (c <= 31)
                return '?'; // control characters
            return c;
        }

        /// <remarks>
        /// Per http://www.unicode.org/glossary/#ASCII, ASCII is only U+0000..U+007F.
        /// </remarks>
        private static bool IsAscii(char c) => (uint)c <= '\x007f';

        private static readonly SyslogLevel[] _loglevelMappings = new []
        {
            NLog.Layouts.SyslogLevel.Debug,
            NLog.Layouts.SyslogLevel.Debug,
            NLog.Layouts.SyslogLevel.Informational,
            NLog.Layouts.SyslogLevel.Warning,
            NLog.Layouts.SyslogLevel.Error,
            NLog.Layouts.SyslogLevel.Emergency,
            NLog.Layouts.SyslogLevel.Emergency,
        };

        private static SyslogLevel ToSyslogLevel(LogLevel logLevel)
        {
            try
            {
                return _loglevelMappings[logLevel.Ordinal];
            }
            catch (IndexOutOfRangeException)
            {
                return NLog.Layouts.SyslogLevel.Emergency;
            }
        }

        private static Dictionary<SyslogLevel, string> ResolveFacilityMapper(SyslogFacility facility)
        {
            return (new SyslogLevel[]
            {
                    NLog.Layouts.SyslogLevel.Emergency,
                    NLog.Layouts.SyslogLevel.Alert,
                    NLog.Layouts.SyslogLevel.Critical,
                    NLog.Layouts.SyslogLevel.Error,
                    NLog.Layouts.SyslogLevel.Warning,
                    NLog.Layouts.SyslogLevel.Notice,
                    NLog.Layouts.SyslogLevel.Informational,
                    NLog.Layouts.SyslogLevel.Debug
            }).ToDictionary(s => s, s => ResolvePriHeader(facility, s));
        }

        private static string ResolvePriHeader(SyslogFacility facility, SyslogLevel severity)
        {
            var priVal = (int)facility * 8 + (int)severity;
            return $"<{priVal}>";
        }
    }
}
