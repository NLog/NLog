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

using System;
using System.Text;

namespace NLog
{
    /// <summary>
    /// Render a message template property to a string
    /// 
    /// If stringify is set, <see cref="StringifyObject"/> will be used
    /// If serialize is set, <see cref="SerializeObject"/> will be used
    /// Otherwise <see cref="FormatObject"/> will be used
    /// </summary>
    public interface IValueSerializer
    {
        /// <summary>
        /// Serialization of an object, e.g. JSON and append to <paramref name="builder"/>. Used if Serialize is set.
        /// </summary>
        /// <param name="value">The object to serialize to string.</param>
        /// <param name="format">The format string for the value</param>
        /// <param name="formatProvider">An object that supplies culture-specific formatting information.</param>
        /// <param name="builder">Output destination.</param>
        /// <returns>Serialize succeeded (true/false)</returns>
        bool SerializeObject(object value, string format, IFormatProvider formatProvider, StringBuilder builder);

        /// <summary>
        /// Convert object into into string value and append to <paramref name="builder"/>. Used if Stringify is set.
        /// </summary>
        /// <param name="value">The object to convert to string.</param>
        /// <param name="format">The format string for the value</param>
        /// <param name="formatProvider">An object that supplies culture-specific formatting information.</param>
        /// <param name="builder">Output destination.</param>
        /// <returns>Stringify succeeded (true/false)</returns>
        bool StringifyObject(object value, string format, IFormatProvider formatProvider, StringBuilder builder);

        /// <summary>
        /// Format object into into string value and append to <paramref name="builder"/>. Used for rendering the template properties.
        /// </summary>
        /// <remarks>
        /// Stringify and Serialize isn't set
        /// </remarks>
        /// <remarks>This one is called if Stringify and Serialize isn't set for this property.</remarks>
        /// <param name="value">The object to convert to string.</param>
        /// <param name="format">The format string for the value</param>
        /// <param name="formatProvider">An object that supplies culture-specific formatting information.</param>
        /// <param name="builder">Output destination.</param>
        /// <returns>Formating succeeded (true/false)</returns>
        bool FormatObject(object value, string format, IFormatProvider formatProvider, StringBuilder builder);
    }
}
