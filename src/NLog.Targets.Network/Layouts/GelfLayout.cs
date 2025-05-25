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
    /// GELF (Graylog Extended Log Format) is a JSON-based, structured log format for Graylog Log Management.
    /// </summary>
    /// <remarks>
    /// <a href="https://github.com/NLog/NLog/wiki/GelfLayout">See NLog Wiki</a>
    /// </remarks>
    /// <seealso href="https://github.com/NLog/NLog/wiki/GelfLayout">Documentation on NLog Wiki</seealso>
    /// <example><para>
    /// {
    ///  "version": "1.1",
    ///  "host": "example.org",
    ///  "short_message": "A short message that helps you identify what is going on",
    ///  "full_message": "Backtrace here\n\nmore stuff",
    ///  "timestamp": 1385053862.3072,
    ///  "level": 1,
    ///  "_user_id": 9001,
    ///  "_some_info": "foo",
    ///  "_some_env_var": "bar"
    /// }
    /// </para></example>
    [Layout("GelfLayout")]
    [ThreadAgnostic]
    [AppDomainFixedOutput]
    public class GelfLayout : CompoundLayout
    {
        private IJsonConverter JsonConverter => _jsonConverter ?? (_jsonConverter = ResolveService<IJsonConverter>());
        private IJsonConverter? _jsonConverter;

        /// <summary>
        /// Gets or sets Graylog Message Host-field
        /// </summary>
        public Layout GelfHostName
        {
            get => _gelfHostName;
            set
            {
                _gelfHostName = value;
                _gelfHostNameString = null;
            }
        }
        private Layout _gelfHostName;
        private string? _gelfHostNameString;

        /// <summary>
        /// Gets or sets the Graylog Message Short-Message-field
        /// </summary>
        /// <remarks>Will truncate when longer than 250 chars</remarks>
        public Layout GelfShortMessage { get; set; }

        /// <summary>
        /// Gets or sets the Graylog Message Full-Message-field
        /// </summary>
        public Layout GelfFullMessage { get; set; }

        /// <summary>
        /// Gets or sets whether to activate the legacy Graylog Message Facility-field
        /// </summary>
        /// <remarks>
        /// Activated legacy GELF v1.0 format
        /// </remarks>
        public Layout GelfFacility
        {
            get => _gelfFacility;
            set
            {
                _gelfFacility = value;
                _gelfFacilityString = null;
            }
        }
        private Layout _gelfFacility = Layout.Empty;
        private string? _gelfFacilityString;

        /// <summary>
        /// Gets or sets GELF additional fields
        /// </summary>
        [ArrayParameter(typeof(TargetPropertyWithContext), "GelfField")]
        public List<TargetPropertyWithContext> GelfFields { get; } = new List<TargetPropertyWithContext>();

        /// <summary>
        /// Gets or sets the option to include all properties from the log events
        /// </summary>
        public bool IncludeEventProperties { get; set; }

        /// <summary>
        /// Gets or sets whether to include the contents of the <see cref="ScopeContext"/> properties-dictionary.
        /// </summary>
        public bool IncludeScopeProperties { get; set; }

        /// <summary>
        /// Gets or sets the option to exclude null/empty properties from the log event (as JSON)
        /// </summary>
        public bool ExcludeEmptyProperties { get; set; }

        /// <summary>
        /// List of property names to exclude when <see cref="IncludeEventProperties"/> is true
        /// </summary>
#if NET35
        public HashSet<string> ExcludeProperties { get; set; } = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
#else
        public ISet<string> ExcludeProperties { get; set; } = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
#endif

        /// <summary>
        /// List of property names to include when <see cref="IncludeEventProperties"/> is true
        /// </summary>
#if NET35
        public HashSet<string> IncludeProperties { get; set; } = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
#else
        public ISet<string> IncludeProperties { get; set; } = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
#endif

        /// <summary>
        /// Disables <see cref="ThreadAgnosticAttribute"/> to capture ScopeContext-properties from active thread context
        /// </summary>
        public LayoutRenderer? DisableThreadAgnostic => IncludeScopeProperties ? _disableThreadAgnostic : (IncludeEventProperties ? _enableThreadAgnosticImmutable : null);
        private static readonly LayoutRenderer _disableThreadAgnostic = new FuncLayoutRenderer(string.Empty, (evt, cfg) => string.Empty);
        private static readonly LayoutRenderer _enableThreadAgnosticImmutable = new ExceptionDataLayoutRenderer() { Item = " " };

        /// <summary>
        /// Initializes a new instance of the <see cref="GelfLayout" /> class.
        /// </summary>
        public GelfLayout()
        {
            _gelfHostName = GelfHostName = "${hostname}";
            GelfShortMessage = GelfFullMessage = "${message}";
        }

        /// <inheritdoc/>
        protected override void InitializeLayout()
        {
            if (GelfFields.Count == 0)
            {
                GelfFields.Add(new TargetPropertyWithContext("_logLevel", "${level}"));
                GelfFields.Add(new TargetPropertyWithContext("_logger", "${logger}"));
                GelfFields.Add(new TargetPropertyWithContext("_exceptionType", "${exception:Format=Type}") { IncludeEmptyValue = false });
                GelfFields.Add(new TargetPropertyWithContext("_exceptionMessage", "${exception:Format=Message}") { IncludeEmptyValue = false });
                GelfFields.Add(new TargetPropertyWithContext("_stackTrace", "${exception:Format=ToString}") { IncludeEmptyValue = false });
            }

            // CompoundLayout includes optimization, so only doing precalculate/caching of relevant Layouts (instead of the entire GELF-message)
            Layouts.Clear();
            if (!IncludeEventProperties && !IncludeScopeProperties)
            {
                if (GelfFacility != null)
                    Layouts.Add(GelfFacility);
                if (GelfHostName != null)
                    Layouts.Add(GelfHostName);
                if (GelfShortMessage != null)
                    Layouts.Add(GelfShortMessage);
                if (GelfFullMessage != null)
                    Layouts.Add(GelfFullMessage);
                for (int i = 0; i < GelfFields.Count; ++i)
                    Layouts.Add(GelfFields[i].Layout);
            }

            base.InitializeLayout();

            _gelfHostNameString = ResolveJsonFixedString(_gelfHostName);
            _gelfFacilityString = ResolveJsonFixedString(_gelfFacility);
        }

        /// <inheritdoc/>
        protected override void CloseLayout()
        {
            base.CloseLayout();
            _jsonConverter = null;
        }

        private string? ResolveJsonFixedString(Layout layout)
        {
            if (layout is SimpleLayout simpleLayout && simpleLayout.IsFixedText && !ReferenceEquals(layout, Layout.Empty))
            {
                var stringBuilder = new StringBuilder();
                JsonConverter.SerializeObject(simpleLayout.FixedText, stringBuilder);
                return stringBuilder.ToString();
            }
            return null;
        }

        /// <inheritdoc/>
        protected override string GetFormattedMessage(LogEventInfo logEvent)
        {
            var stringBuilder = new StringBuilder();
            RenderFormattedMessage(logEvent, stringBuilder);
            return stringBuilder.ToString();
        }

        /// <inheritdoc/>
        protected override void RenderFormattedMessage(LogEventInfo logEvent, StringBuilder target)
        {
            // GELF ver. 1.1
            target.Append(_beginJsonMessage);
            target.Append("version");
            target.Append(_completeJsonPropertyName);
            target.Append('"');
            target.Append(GelfVersion11);
            target.Append('"');

            target.Append(_beginJsonPropertyName);
            target.Append("host");
            target.Append(_completeJsonPropertyName);
            if (_gelfHostNameString is null)
                JsonConverter.SerializeObject(GelfHostName?.Render(logEvent) ?? string.Empty, target);
            else
                target.Append(_gelfHostNameString);

            var shortMessage = GelfShortMessage?.Render(logEvent) ?? string.Empty;
            target.Append(_beginJsonPropertyName);
            target.Append("short_message");
            target.Append(_completeJsonPropertyName);
            JsonConverter.SerializeObject(shortMessage.Length > ShortMessageMaxLength ? shortMessage.Substring(0, ShortMessageMaxLength) : shortMessage, target);

            var fullMessage = GelfFullMessage?.Render(logEvent) ?? string.Empty;
            if (string.IsNullOrEmpty(fullMessage) && shortMessage.Length > ShortMessageMaxLength)
                fullMessage = shortMessage;
            else if (fullMessage.Length < ShortMessageMaxLength && string.Equals(fullMessage, shortMessage, StringComparison.Ordinal))
                fullMessage = string.Empty;
            if (!string.IsNullOrEmpty(fullMessage))
            {
                target.Append(_beginJsonPropertyName);
                target.Append("full_message");
                target.Append(_completeJsonPropertyName);
                JsonConverter.SerializeObject(fullMessage.Length > FullMessageMaxLength ? fullMessage.Substring(0, FullMessageMaxLength) : fullMessage, target);
            }

            var unixTimestamp = ToUnixTimeStamp(logEvent.TimeStamp);
            target.Append(_beginJsonPropertyName);
            target.Append("timestamp");
            target.Append(_completeJsonPropertyName);
            target.AppendFormat(System.Globalization.CultureInfo.InvariantCulture, "{0}", unixTimestamp);

            var gelfSeverity = ToSyslogLevel(logEvent.Level);
            target.Append(_beginJsonPropertyName);
            target.Append("level");
            target.Append(_completeJsonPropertyName);
            target.Append((int)gelfSeverity);

            if (_gelfFacility != null && !ReferenceEquals(_gelfFacility, Layout.Empty))
            {
                target.Append(_beginJsonPropertyName);
                target.Append("facility");
                target.Append(_completeJsonPropertyName);
                if (_gelfFacilityString is null)
                    JsonConverter.SerializeObject(_gelfFacility?.Render(logEvent) ?? string.Empty, target);
                else
                    target.Append(_gelfFacilityString);

                var callerLineNumber = logEvent.CallerLineNumber;
                if (callerLineNumber > 0)
                {
                    target.Append(_beginJsonPropertyName);
                    target.Append("line");
                    target.Append(_completeJsonPropertyName);
                    target.Append(callerLineNumber);
                }

                var callerFileName = logEvent.CallerFilePath ?? logEvent.CallerClassName ?? logEvent.Exception?.TargetSite?.ToString() ?? logEvent.LoggerName;
                if (!string.IsNullOrEmpty(callerFileName))
                {
                    target.Append(_beginJsonPropertyName);
                    target.Append("file");
                    target.Append(_completeJsonPropertyName);
                    JsonConverter.SerializeObject(callerFileName, target);
                }
            }

            var eventProperties = IncludeEventProperties ? ResolveEventProperties(logEvent) : null;
            var scopePropertyList = IncludeScopeProperties ? ResolveScopePropertyList() : null;
            bool filterGelfFields = eventProperties?.Count > 0 || scopePropertyList?.Count > 0;

            foreach (var field in GelfFields)
            {
                var fieldName = field.Name;
                if (string.IsNullOrEmpty(fieldName))
                    continue;

                if (filterGelfFields && ExcludeGelfField(fieldName, eventProperties, scopePropertyList))
                    continue;

                var fieldValue = field.RenderValue(logEvent);
                if (!field.IncludeEmptyValue && (fieldValue is null || ReferenceEquals(string.Empty, fieldValue)))
                    continue;

                BeginJsonProperty(target, fieldName, fieldValue);
            }

            if (scopePropertyList?.Count > 0)
            {
                var filterScopeProperties = eventProperties?.Count > 0 || ExcludeProperties?.Count > 0;
                for (int i = 0; i < scopePropertyList.Count; ++i)
                {
                    var scopeProperty = scopePropertyList[i];
                    if (ExcludeEmptyProperties && (scopeProperty.Value is null || ReferenceEquals(scopeProperty.Value, string.Empty)))
                        continue;

                    if (filterScopeProperties && ExcludeScopeProperty(scopeProperty.Key, eventProperties, ExcludeProperties))
                        continue;

                    BeginJsonProperty(target, scopeProperty.Key, scopeProperty.Value);
                }
            }

            if (eventProperties?.Count > 0)
            {
                var excludeProperties = ExcludeProperties?.Count > 0 ? ExcludeProperties : null;
                foreach (var eventProperty in eventProperties)
                {
                    if (ExcludeEmptyProperties && (eventProperty.Value is null || ReferenceEquals(eventProperty.Value, string.Empty)))
                        continue;

                    var eventPropertyName = eventProperty.Key?.ToString() ?? string.Empty;
                    if (excludeProperties?.Contains(eventPropertyName) == true)
                        continue;

                    BeginJsonProperty(target, eventPropertyName, eventProperty.Value);
                }
            }

            target.Append(_completeJsonMessage);
        }

#if NET35
        private static bool ExcludeScopeProperty(string propertyName, IDictionary<object, object?>? eventProperties, HashSet<string>? excludeProperties)
#else
        private static bool ExcludeScopeProperty(string propertyName, IDictionary<object, object?>? eventProperties, ISet<string>? excludeProperties)
#endif
        {
            if (excludeProperties?.Contains(propertyName) == true)
                return true;
            if (eventProperties?.ContainsKey(propertyName) == true)
                return true;
            return false;
        }

        private static bool ExcludeGelfField(string gelfFieldName, IDictionary<object, object?>? eventProperties, IList<KeyValuePair<string, object?>>? scopeProperties)
        {
            if (string.IsNullOrEmpty(gelfFieldName))
                return true;

            if (eventProperties?.ContainsKey(gelfFieldName) == true)
                return true;

            var normalPropertyName = gelfFieldName[0] == '_' ? gelfFieldName.Substring(1) : null;
            if (normalPropertyName != null && eventProperties?.ContainsKey(normalPropertyName) == true)
                return true;

            if (scopeProperties?.Count > 0)
            {
                for (int i = 0; i < scopeProperties.Count; ++i)
                {
                    if (gelfFieldName.Equals(scopeProperties[i].Key, StringComparison.OrdinalIgnoreCase))
                        return true;
                    if (normalPropertyName?.Equals(scopeProperties[i].Key, StringComparison.OrdinalIgnoreCase) == true)
                        return true;
                }
            }

            return false;
        }

        private IDictionary<object, object?>? ResolveEventProperties(LogEventInfo logEvent)
        {
            if (!logEvent.HasProperties)
                return null;

            var eventProperties = logEvent.Properties;
            if (IncludeProperties?.Count > 0)
            {
                bool filterProperty = false;
                bool foundProperty = false;
                foreach (var eventProperty in logEvent.Properties)
                {
                    if (IncludeProperties.Contains(eventProperty.Key?.ToString() ?? string.Empty))
                        foundProperty = true;
                    else
                        filterProperty = true;

                    if (filterProperty && foundProperty)
                        break;
                }

                if (filterProperty)
                    return foundProperty ? eventProperties.Where(p => IncludeProperties.Contains(p.Key?.ToString() ?? string.Empty)).ToDictionary(p => p.Key, p => p.Value) : null;
            }

            return eventProperties;
        }

        private IList<KeyValuePair<string, object?>>? ResolveScopePropertyList()
        {
            var scopeProperties = ScopeContext.GetAllProperties();
            if (scopeProperties is IList<KeyValuePair<string, object?>> scopePropertyList)
            {
                if (scopePropertyList.Count == 0)
                    return null;

                if (IncludeProperties?.Count > 0)
                {
                    bool filterProperty = false;
                    bool foundProperty = false;
                    for (int i = 0; i < scopePropertyList.Count; ++i)
                    {
                        if (!IncludeProperties.Contains(scopePropertyList[i].Key))
                            filterProperty = true;
                        else
                            foundProperty = true;

                        if (filterProperty && foundProperty)
                            break;
                    }

                    if (filterProperty)
                        return foundProperty ? scopePropertyList.Where(p => IncludeProperties.Contains(p.Key)).ToList() : null;
                }

                return scopePropertyList;
            }

            var scopePropertyCollection = IncludeProperties?.Count > 0
                ? scopeProperties?.Where(p => IncludeProperties.Contains(p.Key)).ToList()
                : scopeProperties?.ToList();
            return scopePropertyCollection?.Count > 0 ? scopePropertyCollection : null;
        }

        private void BeginJsonProperty(StringBuilder sb, string propName, object? propertyValue)
        {
            if (ExcludeEmptyProperties && (propertyValue is null || ReferenceEquals(propertyValue, string.Empty)))
                return;

            var initialLength = sb.Length;

            sb.Append(_beginJsonPropertyName);

            sb.Append(EscapePropertyName(propName));

            sb.Append(_completeJsonPropertyName);

            if (!JsonConverter.SerializeObject(propertyValue, sb))
            {
                sb.Length = initialLength;
            }

            if (ExcludeEmptyProperties && sb[sb.Length - 1] == '"' && sb[sb.Length - 2] == '"' && sb[sb.Length - 3] != '\\')
            {
                sb.Length = initialLength;
            }
        }

        internal static decimal ToUnixTimeStamp(DateTime timeStamp)
        {
            return Convert.ToDecimal(timeStamp.ToUniversalTime().Subtract(UnixDateStart).TotalSeconds);
        }

        internal static SyslogLevel ToSyslogLevel(LogLevel logLevel)
        {
            try
            {
                return _logLevelMapping[logLevel.Ordinal];
            }
            catch (IndexOutOfRangeException)
            {
                return SyslogLevel.Emergency;
            }
        }

        private static readonly SyslogLevel[] _logLevelMapping = new []
        {
            NLog.Layouts.SyslogLevel.Debug,
            NLog.Layouts.SyslogLevel.Debug,
            NLog.Layouts.SyslogLevel.Informational,
            NLog.Layouts.SyslogLevel.Warning,
            NLog.Layouts.SyslogLevel.Error,
            NLog.Layouts.SyslogLevel.Emergency,
            NLog.Layouts.SyslogLevel.Emergency,
        };

        private readonly string _beginJsonMessage = "{\"";
        private readonly string _completeJsonMessage = "}";
        private readonly string _beginJsonPropertyName = ",\"";
        private readonly string _completeJsonPropertyName = "\":";
        private const string GelfVersion11 = "1.1";
        private static DateTime UnixDateStart = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        private const int ShortMessageMaxLength = 250;
        private const int FullMessageMaxLength = 16383; // Truncate due to: https://github.com/Graylog2/graylog2-server/issues/873

        private static string EscapePropertyName(string propertyName)
        {
            if (string.IsNullOrEmpty(propertyName))
                return string.Empty;

            foreach (var chr in propertyName)
            {
                if (RequiresJsonEscape(chr, true) || char.IsWhiteSpace(chr))
                {
                    propertyName = new string(propertyName.Select(c => char.IsWhiteSpace(c) || RequiresJsonEscape(c, true) ? '_' : c).ToArray()).Replace("__", "_").Trim('_');
                    return string.IsNullOrEmpty(propertyName) ? string.Empty : $"_{propertyName}";
                }
            }

            if (propertyName[0] == '_')
                return propertyName;
            else
                return $"_{propertyName}";
        }

        private static bool RequiresJsonEscape(char ch, bool escapeUnicode)
        {
            if (ch < 32)
                return true;
            if (ch > 127)
                return escapeUnicode;
            return ch == '"' || ch == '\\';
        }
    }
}
