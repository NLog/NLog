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
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using NLog.Common;

    /// <summary>
    /// Parameters extracted from parsing <see cref="LogEventInfo.Message"/> as MessageTemplate
    /// </summary>
    public sealed class MessageTemplateParameters : IMessageTemplateParameters
    {
        private readonly IList<MessageTemplateParameter> _parameters;

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
        /// Constructor for positional parameters
        /// </summary>
        /// <param name="message"><see cref="LogEventInfo.Message"/> including any parameter placeholders</param>
        /// <param name="parameters">All <see cref="LogEventInfo.Parameters"/></param>
        public MessageTemplateParameters(string message, object[] parameters)
        {
            var hasParameters = parameters != null && parameters.Length > 0;
            if (hasParameters)
            {
                IsPositional = true;
            }

            _parameters = hasParameters ? CreateParameters(message, parameters) : Internal.ArrayHelper.Empty<MessageTemplateParameter>();
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
        /// <param name="message"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        private IList<MessageTemplateParameter> CreateParameters(string message, object[] parameters)
        {
            try
            {
                List<MessageTemplateParameter> templateParameters = new List<MessageTemplateParameter>(parameters.Length);

                int holeIndex = 0;
                TemplateEnumerator templateEnumerator = new TemplateEnumerator(message);
                while (templateEnumerator.MoveNext())
                {
                    if (templateEnumerator.Current.Literal.Skip != 0)
                    {
                        var hole = templateEnumerator.Current.Hole;
                        if (hole.Index == -1)
                            templateParameters.Add(new MessageTemplateParameter(hole.Name, parameters[holeIndex++], hole.Format, hole.CaptureType));
                        else
                            templateParameters.Add(new MessageTemplateParameter(hole.Name, parameters[hole.Index], hole.Format, hole.CaptureType));
                    }
                }
                return templateParameters;
            }
            catch (Exception ex)
            {
                InternalLogger.Warn(ex, "Error when parsing a message.");
                return Internal.ArrayHelper.Empty<MessageTemplateParameter>();
            }
        }
    }
}
