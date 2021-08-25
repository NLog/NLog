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
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Text;
    using NLog.Config;
    using NLog.Internal;

    /// <summary>
    /// The log level.
    /// </summary>
    [LayoutRenderer("level")]
    [ThreadAgnostic]
    [ThreadSafe]
    public class LevelLayoutRenderer : LayoutRenderer, IRawValue, IStringValueRenderer
    {
        private static readonly string[] _upperCaseMapper = new string[]
        {
            LogLevel.Trace.ToString().ToUpperInvariant(),
            LogLevel.Debug.ToString().ToUpperInvariant(),
            LogLevel.Info.ToString().ToUpperInvariant(),
            LogLevel.Warn.ToString().ToUpperInvariant(),
            LogLevel.Error.ToString().ToUpperInvariant(),
            LogLevel.Fatal.ToString().ToUpperInvariant(),
            LogLevel.Off.ToString().ToUpperInvariant(),
        };

        /// <summary>
        /// Gets or sets a value indicating the output format of the level.
        /// </summary>
        /// <docgen category='Rendering Options' order='10' />
        [DefaultValue(LevelFormat.Name)]
        public LevelFormat Format { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether upper case conversion should be applied.
        /// </summary>
        /// <value>A value of <c>true</c> if upper case conversion should be applied otherwise, <c>false</c>.</value>
        /// <docgen category='Rendering Options' order='10' />
        [DefaultValue(false)]
        public bool Uppercase { get; set; }

        /// <inheritdoc/>
        protected override void Append(StringBuilder builder, LogEventInfo logEvent)
        {
            LogLevel level = GetValue(logEvent);
            switch (Format)
            {
                case LevelFormat.Name:
                    builder.Append(Uppercase ? GetUpperCaseString(level) : level.ToString());
                    break;
                case LevelFormat.FirstCharacter:
                    builder.Append(level.ToString()[0]);
                    break;
                case LevelFormat.Ordinal:
                    builder.AppendInvariant(level.Ordinal);
                    break;
                case LevelFormat.FullName:
                    if (level == LogLevel.Info)
                        builder.Append(Uppercase ? "INFORMATION" : "Information");
                    else if (level == LogLevel.Warn)
                        builder.Append(Uppercase ? "WARNING" : "Warning");
                    else
                        builder.Append(Uppercase ? GetUpperCaseString(level) : level.ToString());
                    break;
            }
        }

        private string GetUpperCaseString(LogLevel level)
        {
            try
            {
                return _upperCaseMapper[level.Ordinal];
            }
            catch (IndexOutOfRangeException)
            {
                return level.ToString().ToUpperInvariant();
            }
        }

        /// <inheritdoc/>
        bool IRawValue.TryGetRawValue(LogEventInfo logEvent, out object value)
        {
            value = GetValue(logEvent);
            return true;
        }

        /// <inheritdoc/>
        string IStringValueRenderer.GetFormattedString(LogEventInfo logEvent)
        {
            if (Format == LevelFormat.Name)
            {
                var level = GetValue(logEvent);
                return Uppercase ? GetUpperCaseString(level) : level.ToString();
            }
            return null;
        }

        private static LogLevel GetValue(LogEventInfo logEvent)
        {
            return logEvent.Level;
        }
    }
}
