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
using NLog.Config;
using NLog.Internal;

namespace NLog.Common
{
    /// <summary>
    /// Convert between types
    /// </summary>
    internal class ValueConverter : IPropertyTypeConverter
    {
        /// <summary>
        /// Singleton instance of the serializer.
        /// </summary>
        public static ValueConverter Instance { get; } = new ValueConverter();

        /// <inheritdoc/>
        public object Convert(object propertyValue, Type propertyType, string format, IFormatProvider formatProvider)
        {
            if (!NeedToConvert(propertyValue, propertyType))
            {
                return propertyValue;
            }

            var nullableType = Nullable.GetUnderlyingType(propertyType);
            var type = nullableType ?? propertyType;
            if (propertyValue is string propertyString)
            {
                propertyValue = propertyString = propertyString.Trim();

                if (nullableType != null && StringHelpers.IsNullOrWhiteSpace(propertyString))
                {
                    return null;
                }
                
                if (type == typeof(DateTime))
                {
                    return ConvertDateTime(format, formatProvider, propertyString);
                }
                if (type == typeof(DateTimeOffset))
                {
                    return ConvertDateTimeOffset(format, formatProvider, propertyString);
                }
                if (type == typeof(TimeSpan))
                {
                    return ConvertTimeSpan(format, formatProvider, propertyString);
                }
                if (type == typeof(Guid))
                {
                    return ConvertGuid(format, propertyString);
                }
            }
            else if (!string.IsNullOrEmpty(format) && propertyValue is IFormattable formattableValue)
            {
                propertyValue = formattableValue.ToString(format, formatProvider);
            }

            var newValue = System.Convert.ChangeType(propertyValue, type, formatProvider);
            return newValue;
        }

        private static bool NeedToConvert(object propertyValue, Type propertyType)
        {
            return propertyType != null && propertyValue != null && propertyValue.GetType() != propertyType && propertyType != typeof(object);
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
