// 
// Copyright (c) 2004-2019 Jaroslaw Kowalski <jaak@jkowalski.net>, Kim Christensen, Julian Verdurmen
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

namespace NLog.Internal
{
    using System;
    using System.Globalization;
    using System.Text;
    using MessageTemplates;

    internal sealed class LogMessageTemplateFormatter : ILogMessageFormatter
    {
        public static readonly LogMessageTemplateFormatter DefaultAuto = new LogMessageTemplateFormatter(false, false);
        public static readonly LogMessageTemplateFormatter Default = new LogMessageTemplateFormatter(true, false);
        public static readonly LogMessageTemplateFormatter DefaultAutoSingleTarget = new LogMessageTemplateFormatter(false, true);
        private static readonly StringBuilderPool _builderPool = new StringBuilderPool(Environment.ProcessorCount * 2);

        /// <summary>
        /// When true: Do not fallback to StringBuilder.Format for positional templates
        /// </summary>
        private readonly bool _forceTemplateRenderer;
        private readonly bool _singleTargetOnly;

        /// <summary>
        /// New formatter
        /// </summary>
        /// <param name="forceTemplateRenderer">When true: Do not fallback to StringBuilder.Format for positional templates</param>
        /// <param name="singleTargetOnly"></param>
        private LogMessageTemplateFormatter(bool forceTemplateRenderer, bool singleTargetOnly)
        {
            _forceTemplateRenderer = forceTemplateRenderer;
            _singleTargetOnly = singleTargetOnly;
            MessageFormatter = FormatMessage;
        }

        /// <summary>
        /// The MessageFormatter delegate
        /// </summary>
        public LogMessageFormatter MessageFormatter { get; }

        /// <inheritDoc/>
        public bool HasProperties(LogEventInfo logEvent)
        {
            if (!HasParameters(logEvent))
                return false;

            if (_singleTargetOnly)
            {
                // Perform quick check for valid message template parameter names (No support for rewind if mixed message-template)
                TemplateEnumerator holeEnumerator = new TemplateEnumerator(logEvent.Message);
                if (holeEnumerator.MoveNext() && holeEnumerator.Current.MaybePositionalTemplate)
                {
                    return false;   // Skip allocation of PropertiesDictionary
                }
            }

            return true;    // Parse message template and allocate PropertiesDictionary
        }

        private bool HasParameters(LogEventInfo logEvent)
        {
            //if message is empty, there no parameters
            //null check cheapest, so in-front
            return logEvent.Parameters != null && !string.IsNullOrEmpty(logEvent.Message) && logEvent.Parameters.Length > 0;
        }

        public void AppendFormattedMessage(LogEventInfo logEvent, StringBuilder builder)
        {
            if (!HasParameters(logEvent))
            {
                builder.Append(logEvent.Message ?? string.Empty);
            }
            else
            {
                logEvent.Message.Render(logEvent.FormatProvider ?? CultureInfo.CurrentCulture, logEvent.Parameters, _forceTemplateRenderer, builder, out _);
            }
        }

        public string FormatMessage(LogEventInfo logEvent)
        {
            if (!HasParameters(logEvent))
            {
                return logEvent.Message;
            }
            using (var builder = _builderPool.Acquire())
            {
                AppendToBuilder(logEvent, builder.Item);
                return builder.Item.ToString();
            }
        }

        private void AppendToBuilder(LogEventInfo logEvent, StringBuilder builder)
        {
            logEvent.Message.Render(logEvent.FormatProvider ?? CultureInfo.CurrentCulture, logEvent.Parameters, _forceTemplateRenderer, builder, out var messageTemplateParameterList);
            logEvent.CreateOrUpdatePropertiesInternal(false, messageTemplateParameterList ?? ArrayHelper.Empty<MessageTemplateParameter>());
        }
    }
}
