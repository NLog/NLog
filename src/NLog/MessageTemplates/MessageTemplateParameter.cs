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
        public readonly string Name;

        /// <summary>
        /// Parameter Value extracted from the <see cref="LogEventInfo.Parameters"/>-array
        /// </summary>
        [CanBeNull]
        public readonly object Value;

        /// <summary>
        /// Format to render the parameter.
        /// This is everything between ":" and the first unescaped "}"
        /// </summary>
        [CanBeNull]
        public readonly string Format;

        /// <summary>
        /// Checks if the <see cref="Format"/> contains reserved letters ('@', '$', 'l')
        /// </summary>
        public bool IsReservedFormat
        {
            get
            {
                switch (Format)
                {
                    case "@":
                    case "$":
                        return true;
                    case "l":
                        if (Value is string || Value == null || Value is char)
                            return true;
                        else
                            return false;
                }
                return false;
            }
        }

        /// <summary>
        /// Constructs a single message template parameter
        /// </summary>
        /// <param name="name">Parameter Name</param>
        /// <param name="value">Parameter Value</param>
        /// <param name="format">Parameter Format</param>
        public MessageTemplateParameter([NotNull] string name, object value, string format)
        {
            Name = name ?? throw new ArgumentNullException(nameof(name));
            Value = value;
            Format = format;
        }
    }
}