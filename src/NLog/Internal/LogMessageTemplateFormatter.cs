// 
// Copyright (c) 2004-2020 Jaroslaw Kowalski <jaak@jkowalski.net>, Kim Christensen, Julian Verdurmen
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
    using System.Collections.Generic;
    using System.Globalization;
    using System.Text;
    using JetBrains.Annotations;
    using MessageTemplates;
    using NLog.Config;

    internal sealed class LogMessageTemplateFormatter : ILogMessageFormatter
    {
        private static readonly StringBuilderPool _builderPool = new StringBuilderPool(Environment.ProcessorCount * 2);
        private readonly IServiceProvider _serviceProvider;

        private IValueFormatter ValueFormatter => _valueFormatter ?? (_valueFormatter = _serviceProvider.GetService<IValueFormatter>());
        private IValueFormatter _valueFormatter;

        /// <summary>
        /// When true: Do not fallback to StringBuilder.Format for positional templates
        /// </summary>
        private readonly bool _forceTemplateRenderer;
        private readonly bool _singleTargetOnly;

        /// <summary>
        /// New formatter
        /// </summary>
        /// <param name="serviceProvider"></param>
        /// <param name="forceTemplateRenderer">When true: Do not fallback to StringBuilder.Format for positional templates</param>
        /// <param name="singleTargetOnly"></param>
        public LogMessageTemplateFormatter([NotNull] IServiceProvider serviceProvider, bool forceTemplateRenderer, bool singleTargetOnly)
        {
            _serviceProvider = serviceProvider;
            _forceTemplateRenderer = forceTemplateRenderer;
            _singleTargetOnly = singleTargetOnly;
            MessageFormatter = FormatMessage;
        }

        /// <summary>
        /// The MessageFormatter delegate
        /// </summary>
        public LogMessageFormatter MessageFormatter { get; }

        public bool ForceTemplateRenderer => _forceTemplateRenderer;

        /// <inheritDoc/>
        public bool HasProperties(LogEventInfo logEvent)
        {
            if (!LogMessageStringFormatter.HasParameters(logEvent))
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

        public void AppendFormattedMessage(LogEventInfo logEvent, StringBuilder builder)
        {
            if (_singleTargetOnly)
            {
                Render(logEvent.Message, logEvent.FormatProvider ?? CultureInfo.CurrentCulture, logEvent.Parameters, builder, out _);
            }
            else
            {
                builder.Append(logEvent.FormattedMessage);
            }
        }

        public string FormatMessage(LogEventInfo logEvent)
        {
            if (LogMessageStringFormatter.HasParameters(logEvent))
            {
                using (var builder = _builderPool.Acquire())
                {
                    AppendToBuilder(logEvent, builder.Item);
                    return builder.Item.ToString();
                }
            }
            
            return logEvent.Message;
        }

        private void AppendToBuilder(LogEventInfo logEvent, StringBuilder builder)
        {
            Render(logEvent.Message, logEvent.FormatProvider ?? CultureInfo.CurrentCulture, logEvent.Parameters, builder, out var messageTemplateParameterList);
            logEvent.CreateOrUpdatePropertiesInternal(false, messageTemplateParameterList ?? ArrayHelper.Empty<MessageTemplateParameter>());
        }

        /// <summary>
        /// Render a template to a string.
        /// </summary>
        /// <param name="template">The template.</param>
        /// <param name="formatProvider">Culture.</param>
        /// <param name="parameters">Parameters for the holes.</param>
        /// <param name="sb">The String Builder destination.</param>
        /// <param name="messageTemplateParameters">Parameters for the holes.</param>
        private void Render(string template, IFormatProvider formatProvider, object[] parameters, StringBuilder sb, out IList<MessageTemplateParameter> messageTemplateParameters)
        {
            int pos = 0;
            int holeIndex = 0;
            int holeStartPosition = 0;
            messageTemplateParameters = null;
            int originalLength = sb.Length;

            TemplateEnumerator templateEnumerator = new TemplateEnumerator(template);
            while (templateEnumerator.MoveNext())
            {
                if (holeIndex == 0 && !_forceTemplateRenderer && templateEnumerator.Current.MaybePositionalTemplate && sb.Length == originalLength)
                {
                    // Not a structured template
                    sb.AppendFormat(formatProvider, template, parameters);
                    return;
                }

                var literal = templateEnumerator.Current.Literal;
                sb.Append(template, pos, literal.Print);
                pos += literal.Print;
                if (literal.Skip == 0)
                {
                    pos++;
                }
                else
                {
                    pos += literal.Skip;
                    var hole = templateEnumerator.Current.Hole;
                    if (hole.Alignment != 0)
                        holeStartPosition = sb.Length;
                    if (hole.Index != -1 && messageTemplateParameters == null)
                    {
                        holeIndex++;
                        RenderHole(sb, hole, formatProvider, parameters[hole.Index], true);
                    }
                    else
                    {
                        var holeParameter = parameters[holeIndex];
                        if (messageTemplateParameters == null)
                        {
                            messageTemplateParameters = new MessageTemplateParameter[parameters.Length];
                            if (holeIndex != 0)
                            {
                                // rewind and try again
                                templateEnumerator = new TemplateEnumerator(template);
                                sb.Length = originalLength;
                                holeIndex = 0;
                                pos = 0;
                                continue;
                            }
                        }
                        messageTemplateParameters[holeIndex++] = new MessageTemplateParameter(hole.Name, holeParameter, hole.Format, hole.CaptureType);
                        RenderHole(sb, hole, formatProvider, holeParameter);
                    }
                    if (hole.Alignment != 0)
                        RenderPadding(sb, hole.Alignment, holeStartPosition);
                }
            }

            if (messageTemplateParameters != null && holeIndex != messageTemplateParameters.Count)
            {
                var truncateParameters = new MessageTemplateParameter[holeIndex];
                for (int i = 0; i < truncateParameters.Length; ++i)
                    truncateParameters[i] = messageTemplateParameters[i];
                messageTemplateParameters = truncateParameters;
            }
        }

        private void RenderHole(StringBuilder sb, Hole hole, IFormatProvider formatProvider, object value, bool legacy = false)
        {
            RenderHole(sb, hole.CaptureType, hole.Format, formatProvider, value, legacy);
        }

        private void RenderHole(StringBuilder sb, CaptureType captureType, string holeFormat, IFormatProvider formatProvider, object value, bool legacy = false)
        {
            if (value == null)
            {
                sb.Append("NULL");
                return;
            }

            if (captureType == CaptureType.Normal && legacy)
            {
                MessageTemplates.ValueFormatter.FormatToString(value, holeFormat, formatProvider, sb);
            }
            else
            {
                ValueFormatter.FormatValue(value, holeFormat, captureType, formatProvider, sb);
            }
        }

        private static void RenderPadding(StringBuilder sb, int holeAlignment, int holeStartPosition)
        {
            int holeWidth = sb.Length - holeStartPosition;
            int holePadding = Math.Abs(holeAlignment) - holeWidth;
            if (holePadding > 0)
            {
                if (holeAlignment < 0 || holeWidth == 0)
                {
                    sb.Append(' ', holePadding);
                }
                else
                {
                    string holeFormatVaue = sb.ToString(holeStartPosition, holeWidth);
                    sb.Length = holeStartPosition;
                    sb.Append(' ', holePadding);
                    sb.Append(holeFormatVaue);
                }
            }
        }
    }
}
