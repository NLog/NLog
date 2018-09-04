﻿// 
// Copyright (c) 2004-2017 Jaroslaw Kowalski <jaak@jkowalski.net>, Kim Christensen, Julian Verdurmen
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
            messageTemplateParameters = null;

            TemplateEnumerator holeEnumerator = new TemplateEnumerator(template);
            while (holeEnumerator.MoveNext())
            {
                var literal = holeEnumerator.Current.Literal;
                if (holeIndex == 0 && !forceTemplateRenderer && sb.Length == 0 && literal.Skip != 0 && holeEnumerator.Current.Hole.Index != -1)
                {
                    // Not a template
                    sb.AppendFormat(formatProvider, template, parameters);
                    return;
                }

                sb.Append(template, pos, literal.Print);
                pos += literal.Print;
                if (literal.Skip == 0)
                {
                    pos++;
                }
                else
                {
                    pos += literal.Skip;
                    var hole = holeEnumerator.Current.Hole;
                    if (hole.Index != -1)
                    {
                        RenderHole(sb, hole, formatProvider, parameters[hole.Index], true);
                    }
                    else
                    {
                        var holeParameter = parameters[holeIndex];
                        if (messageTemplateParameters == null)
                        {
                            messageTemplateParameters = new MessageTemplateParameter[parameters.Length];
                        }
                        messageTemplateParameters[holeIndex++] = new MessageTemplateParameter(hole.Name, holeParameter, hole.Format, hole.CaptureType);
                        RenderHole(sb, hole, formatProvider, holeParameter);
                    }
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
                    if (template.IsPositional)
                    {
                        Hole hole = template.Holes[holeIndex++];
                        RenderHole(sb, hole, formatProvider, parameters[hole.Index], true);
                    }
                    else
                    {
                        RenderHole(sb, template.Holes[holeIndex], formatProvider, parameters[holeIndex++]);
                    }
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

            switch (captureType)
            {
                case CaptureType.Stringify:
                    ValueSerializer.Instance.StringifyObject(value, holeFormat, formatProvider, sb);
                    break;
                case CaptureType.Serialize:
                    ValueSerializer.Instance.SerializeObject(value, holeFormat, formatProvider, sb);
                    break;
                default:
                    if (legacy)
                    {
                        ValueSerializer.FormatToString(value, holeFormat, formatProvider, sb);
                    }
                    else
                    {
                        ValueSerializer.Instance.FormatObject(value, holeFormat, formatProvider, sb);
                    }
                    break;
            }
        }
    }
}
