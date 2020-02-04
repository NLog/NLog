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
using System.ComponentModel;

namespace NLog.Targets
{
    /// <summary>
    /// Options for JSON serialisation
    /// </summary>
    public class JsonSerializeOptions
    {
        /// <summary>
        /// Add quotes around object keys?
        /// </summary>
        [DefaultValue(true)]
        public bool QuoteKeys { get; set; }

        /// <summary>
        /// Format provider for value
        /// </summary>
        public IFormatProvider FormatProvider { get; set; }

        /// <summary>
        /// Format string for value
        /// </summary>
        public string Format { get; set; }

        /// <summary>
        /// Should non-ascii characters be encoded
        /// </summary>
        [DefaultValue(false)]
        public bool EscapeUnicode { get; set; }

        /// <summary>
        /// Should forward slashes be escaped? If true, / will be converted to \/ 
        /// </summary>
        [DefaultValue(true)]
        public bool EscapeForwardSlash { get; set; } = true; // todo NLog 5, default to false
        
        /// <summary>
        /// Serialize enum as string value
        /// </summary>
        [DefaultValue(false)]
        public bool EnumAsInteger { get; set; }

        /// <summary>
        /// Should dictionary keys be sanitized. All characters must either be letters, numbers or underscore character (_).
        /// 
        /// Any other characters will be converted to underscore character (_)
        /// </summary>
        [DefaultValue(false)]
        public bool SanitizeDictionaryKeys { get; set; }

        /// <summary>
        /// How far down the rabbit hole should the Json Serializer go with object-reflection before stopping
        /// </summary>
        [DefaultValue(10)]
        public int MaxRecursionLimit { get; set; }

        /// <summary>Initializes a new instance of the <see cref="T:System.Object" /> class.</summary>
        public JsonSerializeOptions()
        {
            QuoteKeys = true;
            MaxRecursionLimit = 10;
        }
    }
}
