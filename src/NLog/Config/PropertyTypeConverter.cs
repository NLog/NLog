// 
// Copyright (c) 2004-2019 Jaroslaw Kowalski <jaak@jkowalski.net>, Kim Christensen, Julian Verdurmen
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
    /// Default implementation of <see cref="IPropertyTypeConverter"/>
    /// </summary>
    class PropertyTypeConverter : IPropertyTypeConverter
    {
        /// <inheritdoc/>
        public object Convert(object propertyValue, Type propertyType, string format, IFormatProvider formatProvider)
        {
            if (propertyType == null || propertyValue == null || propertyValue.GetType() == propertyType || propertyType == typeof(object))
            {
                return propertyValue;
            }

            if (propertyValue is string propertyString)
            {
                propertyValue = propertyString = propertyString.Trim();

                if (propertyType == typeof(DateTime))
                {
                    return ConvertDateTime(format, formatProvider, propertyString);
                }
                if (propertyType == typeof(DateTimeOffset))
                {
                    return ConvertDateTimeOffset(format, formatProvider, propertyString);
                }
                if (propertyType == typeof(TimeSpan))
                {
                    return ConvertTimeSpan(format, formatProvider, propertyString);
                }
                if (propertyType == typeof(Guid))
                {
                    return ConvertGuid(format, propertyString);
                }
            }
            else if (!string.IsNullOrEmpty(format) && propertyValue is IFormattable formattableValue)
            {
                propertyValue = formattableValue.ToString(format, formatProvider);
            }

            return System.Convert.ChangeType(propertyValue, propertyType, formatProvider);
        }

        private static object ConvertGuid(string format, string propertyString)
        {
#if NET3_5
           return new Guid(propertyString);
#else
            return string.IsNullOrEmpty(format) ? Guid.Parse(propertyString) : Guid.ParseExact(propertyString, format);
#endif
        }

        private static object ConvertTimeSpan(string format, IFormatProvider formatProvider, string propertyString)
        {
#if NET3_5
            return TimeSpan.Parse(propertyString);
#else
            if (!string.IsNullOrEmpty(format))
                return TimeSpan.ParseExact(propertyString, format, formatProvider);
            return TimeSpan.Parse(propertyString, formatProvider);
#endif
        }

        private static object ConvertDateTimeOffset(string format, IFormatProvider formatProvider, string propertyString)
        {
            if (!string.IsNullOrEmpty(format))
                return DateTimeOffset.ParseExact(propertyString, format, formatProvider);
            return DateTimeOffset.Parse(propertyString, formatProvider);
        }

        private static object ConvertDateTime(string format, IFormatProvider formatProvider, string propertyString)
        {
            if (!string.IsNullOrEmpty(format))
                return DateTime.ParseExact(propertyString, format, formatProvider);
            return DateTime.Parse(propertyString, formatProvider);
        }
    }
}
