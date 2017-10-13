// 
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

namespace NLog.MessageTemplates
{
    using System.Collections;
    using System.Collections.Generic;

    /// <summary>
    /// Internal implementation of the interface for returning MessageTemplate parameters
    /// </summary>
    internal class MessageTemplateParameters : IMessageTemplateParameters
    {
        readonly IList<MessageTemplateParameter> _parameters;

        /// <inheritDoc/>
        public MessageTemplateParameter this[int index] => _parameters[index];

        /// <inheritDoc/>
        public int Count => _parameters.Count;

        /// <inheritDoc/>
        public IEnumerator<MessageTemplateParameter> GetEnumerator() { return _parameters.GetEnumerator(); }

        /// <inheritDoc/>
        IEnumerator IEnumerable.GetEnumerator() { return _parameters.GetEnumerator(); }

        /// <inheritDoc/>
        public bool IsPositional { get; }

        /// <summary>
        /// Constructore for positional parameters
        /// </summary>
        public MessageTemplateParameters(object[] parameters)
        {
            var hasParameters = parameters != null && parameters.Length > 0;
            if (hasParameters)
            {
                IsPositional = true;
            }

            _parameters = CreateParameters(parameters, hasParameters);
        }

        /// <summary>
        /// Constructor for named parameters
        /// </summary>
        public MessageTemplateParameters(IList<MessageTemplateParameter> parameters)
        {
            _parameters = parameters ?? Internal.ArrayHelper.Empty<MessageTemplateParameter>();
        }

        /// <summary>
        /// Create MessageTemplateParameter from <paramref name="parameters"/>
        /// </summary>
        /// <param name="parameters"></param>
        /// <param name="hasParameters">is <paramref name="parameters"/> filled? (parameter for performance)</param>
        /// <returns></returns>
        private MessageTemplateParameter[] CreateParameters(object[] parameters, bool hasParameters)
        {
            if (hasParameters)
            {
                var templateParameters = new MessageTemplateParameter[parameters.Length];
                for (int i = 0; i < parameters.Length; ++i)
                {
                    string parameterName;
                    switch (i)
                    {
                        //prevent creating a string (int.ToString())
                        case 0:
                            parameterName = "0";
                            break;
                        case 1:
                            parameterName = "1";
                            break;
                        case 2:
                            parameterName = "2";
                            break;
                        case 3:
                            parameterName = "3";
                            break;
                        case 4:
                            parameterName = "4";
                            break;
                        case 5:
                            parameterName = "5";
                            break;
                        case 6:
                            parameterName = "6";
                            break;
                        case 7:
                            parameterName = "7";
                            break;
                        case 8:
                            parameterName = "8";
                            break;
                        case 9:
                            parameterName = "9";
                            break;
                        default:
                            parameterName = i.ToString(System.Globalization.CultureInfo.InvariantCulture);
                            break;
                    }
                    templateParameters[i] = new MessageTemplateParameter(parameterName, parameters[i], null);
                }
                return templateParameters;
            }

            return Internal.ArrayHelper.Empty<MessageTemplateParameter>();
        }
    }
}
