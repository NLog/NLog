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

namespace NLog.MessageTemplates
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using NLog.Common;
    using NLog.Internal;

    /// <summary>
    /// Parameters extracted from parsing <see cref="LogEventInfo.Message"/> as MessageTemplate
    /// </summary>
    public sealed class MessageTemplateParameters : IEnumerable<MessageTemplateParameter>
    {
        internal static readonly MessageTemplateParameters Empty = new MessageTemplateParameters(string.Empty, ArrayHelper.Empty<object>());

        private readonly IList<MessageTemplateParameter> _parameters;

        /// <inheritDoc/>
        public IEnumerator<MessageTemplateParameter> GetEnumerator() { return _parameters.GetEnumerator(); }

        /// <inheritDoc/>
        IEnumerator IEnumerable.GetEnumerator() { return _parameters.GetEnumerator(); }

        /// <summary>
        /// Gets the parameters at the given index
        /// </summary>
        public MessageTemplateParameter this[int index] => _parameters[index];

        /// <summary>
        /// Number of parameters
        /// </summary>
        public int Count => _parameters.Count;

        /// <summary>Indicates whether the template should be interpreted as positional 
        /// (all holes are numbers) or named.</summary>
        public bool IsPositional { get; }

        /// <summary>
        /// Indicates whether the template was parsed successful, and there are no unmatched parameters
        /// </summary>
        internal bool IsValidTemplate { get; }

        /// <summary>
        /// Constructor for parsing the message template with parameters
        /// </summary>
        /// <param name="message"><see cref="LogEventInfo.Message"/> including any parameter placeholders</param>
        /// <param name="parameters">All <see cref="LogEventInfo.Parameters"/></param>
        internal MessageTemplateParameters(string message, object[] parameters)
        {
            var hasParameters = parameters?.Length > 0;
            bool isPositional = hasParameters;
            bool isValidTemplate = !hasParameters;
            _parameters = hasParameters ? ParseMessageTemplate(message, parameters, out isPositional, out isValidTemplate) : ArrayHelper.Empty<MessageTemplateParameter>();
            IsPositional = isPositional;
            IsValidTemplate = isValidTemplate;
        }

        /// <summary>
        /// Constructor for named parameters that already has been parsed
        /// </summary>
        internal MessageTemplateParameters(IList<MessageTemplateParameter> templateParameters, string message, object[] parameters)
        {
            _parameters = templateParameters ?? ArrayHelper.Empty<MessageTemplateParameter>();
            if (parameters != null && _parameters.Count != parameters.Length)
            {
                IsValidTemplate = false;
            }
        }

        /// <summary>
        /// Create MessageTemplateParameter from <paramref name="parameters"/>
        /// </summary>
        private static IList<MessageTemplateParameter> ParseMessageTemplate(string template, object[] parameters, out bool isPositional, out bool isValidTemplate)
        {
            isPositional = true;
            isValidTemplate = true;

            List<MessageTemplateParameter> templateParameters = new List<MessageTemplateParameter>(parameters.Length);

            try
            {
                short holeIndex = 0;
                TemplateEnumerator templateEnumerator = new TemplateEnumerator(template);
                while (templateEnumerator.MoveNext())
                {
                    if (templateEnumerator.Current.Literal.Skip != 0)
                    {
                        var hole = templateEnumerator.Current.Hole;
                        if (hole.Index != -1 && isPositional)
                        {
                            holeIndex = GetMaxHoleIndex(holeIndex, hole.Index);
                            var value = GetHoleValueSafe(parameters, hole.Index, ref isValidTemplate);
                            templateParameters.Add(new MessageTemplateParameter(hole.Name, value, hole.Format, hole.CaptureType));
                        }
                        else
                        {
                            if (isPositional)
                            {
                                isPositional = false;
                                if (holeIndex != 0)
                                {
                                    // rewind and try again
                                    templateEnumerator = new TemplateEnumerator(template);
                                    holeIndex = 0;
                                    templateParameters.Clear();
                                    continue;
                                }
                            }

                            var value = GetHoleValueSafe(parameters, holeIndex, ref isValidTemplate);
                            templateParameters.Add(new MessageTemplateParameter(hole.Name, value, hole.Format, hole.CaptureType));
                            holeIndex++;
                        }
                    }
                }

                if (isPositional)
                {
                    if (templateParameters.Count < parameters.Length || holeIndex != parameters.Length)
                    {
                        isValidTemplate = false;
                    }
                }
                else
                {
                    if (templateParameters.Count != parameters.Length)
                    {
                        isValidTemplate = false;
                    }
                }

                return templateParameters;
            }
            catch (Exception ex)
            {
                isValidTemplate = false;
                InternalLogger.Warn(ex, "Error when parsing a message.");
                return templateParameters;
            }
        }

        private static short GetMaxHoleIndex(short maxHoleIndex, short holeIndex)
        {
            if (maxHoleIndex == 0)
                maxHoleIndex++;
            if (maxHoleIndex <= holeIndex)
            {
                maxHoleIndex = holeIndex;
                maxHoleIndex++;
            }
            return maxHoleIndex;
        }

        private static object GetHoleValueSafe(object[] parameters, short holeIndex, ref bool isValidTemplate)
        {
            if (parameters.Length > holeIndex)
            {
                return parameters[holeIndex];
            }
            isValidTemplate = false;
            return null;
        }
    }
}
