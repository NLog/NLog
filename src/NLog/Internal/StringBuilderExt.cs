// 
// Copyright (c) 2004-2016 Jaroslaw Kowalski <jaak@jkowalski.net>, Kim Christensen, Julian Verdurmen
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
using NLog.Config;

namespace NLog.Internal
{
    /// <summary>
    /// Helpers for <see cref="StringBuilder"/>, which is used in e.g. layout renderers.
    /// </summary>
    internal static class StringBuilderExt
    {
        /// <summary>
        /// Append a value and use formatProvider of <paramref name="logEvent"/> or <paramref name="configuration"/> to convert to string.
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="o">value to append.</param>
        /// <param name="logEvent">current logEvent for FormatProvider.</param>
        /// <param name="configuration">Configuration for DefaultCultureInfo</param>
        public static void Append(this StringBuilder builder, object o, LogEventInfo logEvent, LoggingConfiguration configuration)
        {
            var formatProvider = logEvent.FormatProvider;
            if (formatProvider == null && configuration != null)
            {
                formatProvider = configuration.DefaultCultureInfo;
            }
            builder.Append(Convert.ToString(o, formatProvider));
        }

        /// <summary>
        /// Copies content of the builder and appends it to target
        /// </summary>
        /// <param name="source">Source of the content</param>
        /// <param name="target">Destination of the content</param>
        /// <param name="workBuffer">Helper buffer to perform the copy [optional]</param>
        public static void CopyToBuilder(this StringBuilder source, StringBuilder target, char[] workBuffer = null)
        {
#if !SILVERLIGHT
            if (workBuffer == null)
                workBuffer = new char[1024];
            for (int i = 0; i < source.Length; i += workBuffer.Length)
            {
                int charCount = Math.Min(source.Length - i, workBuffer.Length);
                source.CopyTo(i, workBuffer, 0, charCount);
                target.Append(workBuffer, 0, charCount);
            }
#else
            target.Append(source.ToString());
#endif
        }

        /// <summary>
        /// Clears the provider StringBuilder
        /// </summary>
        /// <param name="builder"></param>
        public static void ClearBuilder(this StringBuilder builder)
        {
#if NET4 || NET4_5
            builder.Clear();
#else
            builder.Length = 0;
#endif
        }
    }
}
