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
    using System.Text;
    using NLog.Config;
    using NLog.Internal;

    /// <summary>
    /// The log level.
    /// </summary>
    /// <remarks>
    /// <a href="https://github.com/NLog/NLog/wiki/Level-Layout-Renderer">See NLog Wiki</a>
    /// </remarks>
    /// <seealso href="https://github.com/NLog/NLog/wiki/Level-Layout-Renderer">Documentation on NLog Wiki</seealso>
    [LayoutRenderer("level")]
    [LayoutRenderer("loglevel")]
    [ThreadAgnostic]
    public class LevelLayoutRenderer : LayoutRenderer, IRawValue, INoAllocationStringValueRenderer
    {
        private readonly static string[] _defaultNames = GenerateLevelNames(LevelFormat.Name, false);
        private readonly static string[] _defaultUppercaseNames = GenerateLevelNames(LevelFormat.Name, true);
        private string[] _levelNames = _defaultNames;

        /// <summary>
        /// Gets or sets a value indicating the output format of the level.
        /// </summary>
        /// <remarks>Default: <see cref="LevelFormat.Name"/></remarks>
        /// <docgen category='Layout Options' order='10' />
        public LevelFormat Format
        {
            get => _format;
            set
            {
                if (_format != value)
                {
                    _format = value;
                    _levelNames = GenerateLevelNames(_format, _uppercase);
                }
            }
        }
        private LevelFormat _format = LevelFormat.Name;

        /// <summary>
        /// Gets or sets a value indicating whether upper case conversion should be applied.
        /// </summary>
        /// <remarks>Default: <see langword="false"/></remarks>
        /// <docgen category='Layout Options' order='10' />
        public bool Uppercase
        {
            get => _uppercase;
            set
            {
                if (_uppercase != value)
                {
                    _uppercase = value;
                    if (_format == LevelFormat.Name)
                        _levelNames = _uppercase ? _defaultUppercaseNames : _defaultNames;
                    else
                        _levelNames = GenerateLevelNames(_format, _uppercase);
                }
            }
        }
        private bool _uppercase;

        /// <inheritdoc/>
        protected override void Append(StringBuilder builder, LogEventInfo logEvent)
        {
            builder.Append(GetLogLevelStringValue(logEvent));
        }

        private string GetLogLevelStringValue(LogEventInfo logEvent)
        {
            var logLevel = GetValue(logEvent) ?? LogLevel.Trace;
            var ordinal = logLevel.Ordinal;
            return (ordinal >= 0 && ordinal < _levelNames.Length) ? _levelNames[ordinal] : GetFormattedLevelName(logLevel, _format, _uppercase);
        }

        private static string[] GenerateLevelNames(LevelFormat format, bool upperCase)
        {
            string[] newLevelNames = new string[LogLevel.MaxLevel.Ordinal + 2];
            newLevelNames[LogLevel.Trace.Ordinal] = GetFormattedLevelName(LogLevel.Trace, format, upperCase);
            newLevelNames[LogLevel.Debug.Ordinal] = GetFormattedLevelName(LogLevel.Debug, format, upperCase);
            newLevelNames[LogLevel.Info.Ordinal] = GetFormattedLevelName(LogLevel.Info, format, upperCase);
            newLevelNames[LogLevel.Warn.Ordinal] = GetFormattedLevelName(LogLevel.Warn, format, upperCase);
            newLevelNames[LogLevel.Error.Ordinal] = GetFormattedLevelName(LogLevel.Error, format, upperCase);
            newLevelNames[LogLevel.Fatal.Ordinal] = GetFormattedLevelName(LogLevel.Fatal, format, upperCase);
            newLevelNames[LogLevel.Off.Ordinal] = GetFormattedLevelName(LogLevel.Off, format, upperCase);
            return newLevelNames;
        }

        private static string GetFormattedLevelName(LogLevel logLevel, LevelFormat format, bool upperCase)
        {
            switch (format)
            {
                case LevelFormat.FirstCharacter:
                    return logLevel.ToString()[0].ToString();
                case LevelFormat.Ordinal:
                    return logLevel.Ordinal.ToString(System.Globalization.CultureInfo.InvariantCulture);
                case LevelFormat.FullName:
                    return upperCase ? GetFullNameString(logLevel).ToUpperInvariant() : GetFullNameString(logLevel);
                case LevelFormat.TriLetter:
                    return upperCase ? GetTriLetterString(logLevel).ToUpperInvariant() : GetTriLetterString(logLevel);
                default:
                    return upperCase ? logLevel.ToString().ToUpperInvariant() : logLevel.ToString();
            }
        }

        private static string GetFullNameString(LogLevel logLevel)
        {
            if (logLevel == LogLevel.Info)
                return "Information";
            else if (logLevel == LogLevel.Warn)
                return "Warning";
            else
                return logLevel.ToString();
        }

        private static string GetTriLetterString(LogLevel level)
        {
            if (level == LogLevel.Trace)
                return "Trc";
            if (level == LogLevel.Debug)
                return "Dbg";
            if (level == LogLevel.Info)
                return "Inf";
            if (level == LogLevel.Warn)
                return "Wrn";
            if (level == LogLevel.Error)
                return "Err";
            if (level == LogLevel.Fatal)
                return "Ftl";
            return level.ToString();
        }

        bool IRawValue.TryGetRawValue(LogEventInfo logEvent, out object value)
        {
            value = GetValue(logEvent);
            return true;
        }

        string? IStringValueRenderer.GetFormattedString(LogEventInfo logEvent) => GetLogLevelStringValue(logEvent);

        string? INoAllocationStringValueRenderer.GetFormattedStringNoAllocation(LogEventInfo logEvent) => GetLogLevelStringValue(logEvent);

        private static LogLevel GetValue(LogEventInfo logEvent)
        {
            return logEvent.Level;
        }
    }
}
