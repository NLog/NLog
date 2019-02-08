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

using System;
using System.Collections.Generic;
using System.Text;

namespace NLog.MessageTemplates
{
    /// <summary>
    /// Render templates
    /// </summary>
    internal static class TemplateRenderer
    {
        /// <summary>
        /// Render a template to a string.
        /// </summary>
        /// <param name="template">The template.</param>
        /// <param name="formatProvider">Culture.</param>
        /// <param name="parameters">Parameters for the holes.</param>
        /// <param name="forceTemplateRenderer">Do not fallback to StringBuilder.Format for positional templates.</param>
        /// <param name="sb">The String Builder destination.</param>
        /// <param name="messageTemplateParameters">Parameters for the holes.</param>
        public static void Render(this string template, IFormatProvider formatProvider, object[] parameters, bool forceTemplateRenderer, StringBuilder sb, out IList<MessageTemplateParameter> messageTemplateParameters)
        {
            int pos = 0;
            int holeIndex = 0;
            int holeStartPosition = 0;
            messageTemplateParameters = null;
            int originalLength = sb.Length;

            TemplateEnumerator templateEnumerator = new TemplateEnumerator(template);
            while (templateEnumerator.MoveNext())
            {
                if (holeIndex == 0 && !forceTemplateRenderer && templateEnumerator.Current.MaybePositionalTemplate && sb.Length == originalLength)
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

        /// <summary>
        /// Render a template to a string.
        /// </summary>
        /// <param name="template">The template.</param>
        /// <param name="sb">The String Builder destination.</param>
        /// <param name="formatProvider">Culture.</param>
        /// <param name="parameters">Parameters for the holes.</param>
        /// <returns>Rendered template, never null.</returns>
        public static void Render(this Template template, StringBuilder sb, IFormatProvider formatProvider, object[] parameters)
        {
            int pos = 0;
            int holeIndex = 0;
            int holeStartPosition = 0;
            foreach (var literal in template.Literals)
            {
                sb.Append(template.Value, pos, literal.Print);
                pos += literal.Print;
                if (literal.Skip == 0)
                {
                    pos++;
                }
                else
                {
                    pos += literal.Skip;

                    var hole = template.Holes[holeIndex];
                    if (hole.Alignment != 0)
                        holeStartPosition = sb.Length;

                    var parameter = template.IsPositional ? parameters[hole.Index] : parameters[holeIndex];
                    ++holeIndex;
                    RenderHole(sb, hole, formatProvider, parameter, template.IsPositional);
                    if (hole.Alignment != 0)
                        RenderPadding(sb, hole.Alignment, holeStartPosition);
                }
            }
        }

        private static void RenderHole(StringBuilder sb, Hole hole, IFormatProvider formatProvider, object value, bool legacy = false)
        {
            RenderHole(sb, hole.CaptureType, hole.Format, formatProvider, value, legacy);
        }

        public static void RenderHole(StringBuilder sb, CaptureType captureType, string holeFormat, IFormatProvider formatProvider, object value, bool legacy = false)
        {
            if (value == null)
            {
                sb.Append("NULL");
                return;
            }

            if (captureType == CaptureType.Normal && legacy)
            {
                ValueFormatter.FormatToString(value, holeFormat, formatProvider, sb);
            }
            else
            {
                ValueFormatter.Instance.FormatValue(value, holeFormat, captureType, formatProvider, sb);
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
