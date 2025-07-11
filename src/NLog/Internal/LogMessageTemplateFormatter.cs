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

namespace NLog.Internal
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Text;
    using MessageTemplates;
    using NLog.Config;

    internal sealed class LogMessageTemplateFormatter : ILogMessageFormatter
    {
        private static readonly StringBuilderPool _builderPool = new StringBuilderPool(Environment.ProcessorCount * 2);
        private readonly LogFactory _logFactory;

        private IValueFormatter ValueFormatter => _valueFormatter ?? (_valueFormatter = _logFactory.ServiceRepository.GetService<IValueFormatter>());
        private IValueFormatter? _valueFormatter;

        /// <summary>
        /// When <see langword="true"/> Do not fallback to StringBuilder.Format for positional templates
        /// </summary>
        private readonly bool _forceMessageTemplateRenderer;
        private readonly bool _singleTargetOnly;

        /// <summary>
        /// New formatter
        /// </summary>
        /// <param name="logFactory"></param>
        /// <param name="forceMessageTemplateRenderer">When <see langword="true"/> Do not fallback to StringBuilder.Format for positional templates</param>
        /// <param name="singleTargetOnly"></param>
        public LogMessageTemplateFormatter(LogFactory logFactory, bool forceMessageTemplateRenderer, bool singleTargetOnly)
        {
            _logFactory = logFactory;
            _forceMessageTemplateRenderer = forceMessageTemplateRenderer;
            _singleTargetOnly = singleTargetOnly;
            MessageFormatter = FormatMessage;
        }

        /// <summary>
        /// The MessageFormatter delegate
        /// </summary>
        public LogMessageFormatter MessageFormatter { get; }

        public bool? EnableMessageTemplateParser => _forceMessageTemplateRenderer ? true : default(bool?);

        /// <inheritDoc/>
        public bool HasProperties(LogEventInfo logEvent)
        {
            if (!LogMessageStringFormatter.HasParameters(logEvent))
                return false;

            if (_singleTargetOnly)
            {
                // Perform quick check for valid message template parameter names (No support for rewind if mixed message-template)
                TemplateEnumerator holeEnumerator = new TemplateEnumerator(logEvent.Message);
                if (!holeEnumerator.MoveNext() || holeEnumerator.Current.MaybePositionalTemplate)
                {
                    return false;   // Skip allocation of PropertiesDictionary
                }
            }

            return true;    // Parse message template and allocate PropertiesDictionary
        }

        public void AppendFormattedMessage(LogEventInfo logEvent, StringBuilder builder)
        {
            if (_singleTargetOnly && logEvent.Parameters?.Length > 0)
            {
                Render(logEvent.Message, logEvent.FormatProvider ?? _logFactory.DefaultCultureInfo ?? CultureInfo.CurrentCulture, logEvent.Parameters, builder, out _);
            }
            else
            {
                builder.Append(logEvent.FormattedMessage);
            }
        }

        public string FormatMessage(LogEventInfo logEvent)
        {
            var parameters = logEvent.Parameters;
            if (parameters?.Length > 0 && !string.IsNullOrEmpty(logEvent.Message))
            {
                using (var builder = _builderPool.Acquire())
                {
                    AppendToBuilder(logEvent, parameters, builder.Item);
                    return builder.Item.ToString();
                }
            }

            return logEvent.Message;
        }

        private void AppendToBuilder(LogEventInfo logEvent, object?[] parameters, StringBuilder builder)
        {
            Render(logEvent.Message, logEvent.FormatProvider ?? _logFactory.DefaultCultureInfo ?? CultureInfo.CurrentCulture, parameters, builder, out var messageTemplateParameterList);
            logEvent.TryCreatePropertiesInternal(messageTemplateParameterList ?? ArrayHelper.Empty<MessageTemplateParameter>());
        }

        /// <summary>
        /// Render a template to a string.
        /// </summary>
        /// <param name="template">The template.</param>
        /// <param name="formatProvider">Culture.</param>
        /// <param name="parameters">Parameters for the holes.</param>
        /// <param name="sb">The String Builder destination.</param>
        /// <param name="messageTemplateParameters">Parameters for the holes.</param>
        private void Render(string template, IFormatProvider? formatProvider, object?[] parameters, StringBuilder sb, out IList<MessageTemplateParameter>? messageTemplateParameters)
        {
            messageTemplateParameters = null;

            TemplateEnumerator templateEnumerator = new TemplateEnumerator(template);
            if (!templateEnumerator.MoveNext() || (templateEnumerator.Current.MaybePositionalTemplate && !_forceMessageTemplateRenderer))
            {
                // string.Format when not message-template for structured logging
                sb.AppendFormat(formatProvider, template, parameters);
                return;
            }

            // Handle message-template-format or string-format or mixed-format
            int pos = 0;
            int holeIndex = 0;
            int holeStartPosition = 0;
            int originalLength = sb.Length;

            do
            {
                var literal = templateEnumerator.Current.Literal;
                sb.Append(template, pos, literal.Print);
                pos += literal.Print;
                if (literal.Skip == 0)
                {
                    pos++;
                    continue;
                }

                pos += literal.Skip;
                var hole = templateEnumerator.Current.Hole;
                if (hole.Alignment != 0)
                    holeStartPosition = sb.Length;
                if (hole.Index != -1 && messageTemplateParameters is null)
                {
                    holeIndex++;
                    RenderHolePositional(sb, hole, formatProvider, parameters[hole.Index]);
                }
                else
                {
                    var holeParameter = parameters[holeIndex];
                    if (messageTemplateParameters is null)
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
            } while (templateEnumerator.MoveNext());

            messageTemplateParameters = VerifyMessageTemplateParameters(messageTemplateParameters, holeIndex);
        }

#if NETSTANDARD2_1_OR_GREATER || NET9_0_OR_GREATER
        internal string Render(ref TemplateEnumerator templateEnumerator, IFormatProvider? formatProvider, in ReadOnlySpan<object?> parameters, out IList<MessageTemplateParameter>? messageTemplateParameters)
        {
            // Handle message-template-format or string-format or mixed-format
            messageTemplateParameters = null;

            using (var builder = _builderPool.Acquire())
            {
                var sb = builder.Item;

                string template = templateEnumerator.Template;
                int pos = 0;
                int holeStartPosition = 0;
                int holeIndex = 0;

                do
                {
                    var literal = templateEnumerator.Current.Literal;
                    sb.Append(template, pos, literal.Print);
                    pos += literal.Print;
                    if (literal.Skip == 0)
                    {
                        pos++;
                        continue;
                    }

                    pos += literal.Skip;
                    var hole = templateEnumerator.Current.Hole;

                    if (hole.Alignment != 0)
                        holeStartPosition = sb.Length;
                    if (hole.Index != -1 && messageTemplateParameters is null)
                    {
                        holeIndex++;
                        RenderHolePositional(sb, hole, formatProvider, parameters[hole.Index]);
                    }
                    else
                    {
                        var holeParameter = parameters[holeIndex];
                        if (messageTemplateParameters is null)
                        {
                            messageTemplateParameters = new MessageTemplateParameter[parameters.Length];
                            if (holeIndex != 0)
                            {
                                // rewind and try again
                                templateEnumerator = new TemplateEnumerator(template);
                                sb.ClearBuilder();
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
                } while (templateEnumerator.MoveNext());

                messageTemplateParameters = VerifyMessageTemplateParameters(messageTemplateParameters, holeIndex);
                return sb.ToString();
            }
        }
#endif

        private static IList<MessageTemplateParameter>? VerifyMessageTemplateParameters(IList<MessageTemplateParameter>? messageTemplateParameters, int holeIndex)
        {
            if (messageTemplateParameters != null && holeIndex != messageTemplateParameters.Count)
            {
                var truncateParameters = new MessageTemplateParameter[holeIndex];
                for (int i = 0; i < truncateParameters.Length; ++i)
                    truncateParameters[i] = messageTemplateParameters[i];
                messageTemplateParameters = truncateParameters;
            }

            return messageTemplateParameters;
        }

        private void RenderHolePositional(StringBuilder sb, in Hole hole, IFormatProvider? formatProvider, object? value)
        {
            if (value is null)
            {
                sb.Append("NULL");
            }
            else if (hole.CaptureType == CaptureType.Normal)
            {
                MessageTemplates.ValueFormatter.FormatToString(value, hole.Format, formatProvider, sb);
            }
            else
            {
                RenderHole(sb, hole.CaptureType, hole.Format, formatProvider, value);
            }
        }

        private void RenderHole(StringBuilder sb, in Hole hole, IFormatProvider? formatProvider, object? value)
        {
            RenderHole(sb, hole.CaptureType, hole.Format, formatProvider, value);
        }

        private void RenderHole(StringBuilder sb, CaptureType captureType, string? holeFormat, IFormatProvider? formatProvider, object? value)
        {
            if (value is null)
            {
                sb.Append("NULL");
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
