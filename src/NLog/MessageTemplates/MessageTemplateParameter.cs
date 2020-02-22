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

using System;
using JetBrains.Annotations;

namespace NLog.MessageTemplates
{
    /// <summary>
    /// Description of a single parameter extracted from a MessageTemplate
    /// </summary>
    public struct MessageTemplateParameter
    {
        /// <summary>
        /// Parameter Name extracted from <see cref="LogEventInfo.Message"/>
        /// This is everything between "{" and the first of ",:}".
        /// </summary>
        [NotNull]
        public string Name { get; }

        /// <summary>
        /// Parameter Value extracted from the <see cref="LogEventInfo.Parameters"/>-array
        /// </summary>
        [CanBeNull]
        public object Value { get; }

        /// <summary>
        /// Format to render the parameter.
        /// This is everything between ":" and the first unescaped "}"
        /// </summary>
        [CanBeNull]
        public string Format { get; }

        /// <summary>
        /// Parameter method that should be used to render the parameter
        /// See also <see cref="IValueFormatter"/>
        /// </summary>
        public CaptureType CaptureType { get; }

        /// <summary>
        /// Returns index for <see cref="LogEventInfo.Parameters"/>, when <see cref="MessageTemplateParameters.IsPositional"/>
        /// </summary>
        public int? PositionalIndex
        {
            get
            {
                switch (Name)
                {
                    case "0": return 0;
                    case "1": return 1;
                    case "2": return 2;
                    case "3": return 3;
                    case "4": return 4;
                    case "5": return 5;
                    case "6": return 6;
                    case "7": return 7;
                    case "8": return 8;
                    case "9": return 9;
                    default:
                        if (Name?.Length >= 1 && Name[0] >= '0' && Name[0] <= '9' && int.TryParse(Name, out var parameterIndex))
                        {
                            return parameterIndex;
                        }
                        return null;
                }
            }
        }

        /// <summary>
        /// Constructs a single message template parameter
        /// </summary>
        /// <param name="name">Parameter Name</param>
        /// <param name="value">Parameter Value</param>
        /// <param name="format">Parameter Format</param>
        internal MessageTemplateParameter([NotNull] string name, object value, string format)
        {
            Name = name ?? throw new ArgumentNullException(nameof(name));
            Value = value;
            Format = format;
            CaptureType = CaptureType.Normal;
        }

        /// <summary>
        /// Constructs a single message template parameter
        /// </summary>
        /// <param name="name">Parameter Name</param>
        /// <param name="value">Parameter Value</param>
        /// <param name="format">Parameter Format</param>
        /// <param name="captureType">Parameter CaptureType</param>
        public MessageTemplateParameter([NotNull] string name, object value, string format, CaptureType captureType)
        {
            Name = name ?? throw new ArgumentNullException(nameof(name));
            Value = value;
            Format = format;
            CaptureType = captureType;
        }
    }
}