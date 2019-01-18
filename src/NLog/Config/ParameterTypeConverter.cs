// 
// Copyright (c) 2004-2018 Jaroslaw Kowalski <jaak@jkowalski.net>, Kim Christensen, Julian Verdurmen
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

namespace NLog.Config
{
    using System;

    /// <summary>
    /// Default implementation of <see cref="IParameterTypeConverter"/>
    /// </summary>
    class ParameterTypeConverter : IParameterTypeConverter
    {
        /// <inheritdoc/>
        public object Convert(object parameterValue, Type parameterType, string format, IFormatProvider formatProvider)
        {
            if (parameterType == null || parameterValue == null || parameterValue.GetType() == parameterType || parameterType == typeof(object))
            {
                return parameterValue;
            }

            if (parameterValue is string parameterString)
            {
                parameterValue = parameterString = parameterString.Trim();

                if (parameterType == typeof(DateTime))
                {
                    if (!string.IsNullOrEmpty(format))
                        return DateTime.ParseExact(parameterString, format, formatProvider);
                    else
                        return DateTime.Parse(parameterString, formatProvider);
                }
                if (parameterType == typeof(DateTimeOffset))
                {
                    if (!string.IsNullOrEmpty(format))
                        return DateTimeOffset.ParseExact(parameterString, format, formatProvider);
                    else
                        return DateTimeOffset.Parse(parameterString, formatProvider);
                }
                if (parameterType == typeof(TimeSpan))
                {
#if NET3_5
                    return TimeSpan.Parse(parameterString);
#else
                    if (!string.IsNullOrEmpty(format))
                        return TimeSpan.ParseExact(parameterString, format, formatProvider);
                    else
                        return TimeSpan.Parse(parameterString, formatProvider);
#endif
                }
                if (parameterType == typeof(Guid))
                {
#if NET3_5
                    return new Guid(parameterString);
#else
                    return string.IsNullOrEmpty(format) ? Guid.Parse(parameterString) : Guid.ParseExact(parameterString, format);
#endif
                }
            }
            else if (!string.IsNullOrEmpty(format) && parameterValue is IFormattable formattableValue)
            {
                parameterValue = formattableValue.ToString(format, formatProvider);
            }

            return System.Convert.ChangeType(parameterValue, parameterType, formatProvider);
        }
    }
}
