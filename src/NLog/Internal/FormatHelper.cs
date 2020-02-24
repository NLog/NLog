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

namespace NLog.Internal
{
    using System;
    using NLog.Config;

    internal static class FormatHelper
    {
        /// <summary>
        /// Convert object to string
        /// </summary>
        /// <param name="o">value</param>
        /// <param name="formatProvider">format for conversion.</param>
        /// <returns></returns>
        /// <remarks>
        /// If <paramref name="formatProvider"/> is <c>null</c> and <paramref name="o"/> isn't a <see cref="string"/> already, then the <see cref="LogFactory"/> will get a locked by <see cref="LogManager.Configuration"/>
        /// </remarks>
        internal static string ConvertToString(object o, IFormatProvider formatProvider)
        {
            // if no IFormatProvider is specified, use the Configuration.DefaultCultureInfo value.
            if (formatProvider == null)
            {
                if (SkipFormattableToString(o))
                    return o?.ToString() ?? string.Empty;

                if (o is IFormattable)
                {
                    // Lookup / loading global configuration can be dangerous and can lead to deadlocks
                    var loggingConfiguration = LogManager.Configuration;
                    if (loggingConfiguration != null)
                        formatProvider = loggingConfiguration.DefaultCultureInfo;
                }
            }

            return Convert.ToString(o, formatProvider);
        }

        private static bool SkipFormattableToString(object o)
        {
            switch (Convert.GetTypeCode(o))
            {
                case TypeCode.String:   return true;
                case TypeCode.Empty:    return true;
                default:                return false;
            }
        }

        internal static string TryFormatToString(object value, string format, IFormatProvider formatProvider)
        {
            if (SkipFormattableToString(value))
                return value?.ToString() ?? string.Empty;

            if (value is IFormattable formattable)
            {
                return formattable.ToString(format, formatProvider);
            }
            else if (value is System.Collections.IEnumerable)
            {
                return null;
            }
            else
            {
                return value.ToString() ?? string.Empty;
            }
        }
    }
}