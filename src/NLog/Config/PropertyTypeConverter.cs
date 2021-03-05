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

namespace NLog.Config
{
    using System;
    using System.Collections.Generic;
    using System.Reflection;
    using NLog.Internal;

    /// <summary>
    /// Default implementation of <see cref="IPropertyTypeConverter"/>
    /// </summary>
    internal class PropertyTypeConverter : IPropertyTypeConverter
    {
        /// <summary>
        /// Singleton instance of the serializer.
        /// </summary>
        public static PropertyTypeConverter Instance { get; } = new PropertyTypeConverter();

        private static Dictionary<Type, Func<string, string, IFormatProvider, object>> StringConverterLookup => _stringConverters ?? (_stringConverters = BuildStringConverterLookup());
        private static Dictionary<Type, Func<string, string, IFormatProvider, object>> _stringConverters;

        private static Dictionary<Type, Func<string, string, IFormatProvider, object>> BuildStringConverterLookup()
        {
            return new Dictionary<Type, Func<string, string, IFormatProvider, object>>()
            {
                { typeof(System.Text.Encoding), (stringvalue, format, formatProvider) => ConvertToEncoding(stringvalue) },
                { typeof(System.Globalization.CultureInfo), (stringvalue, format, formatProvider) => new System.Globalization.CultureInfo(stringvalue) },
                { typeof(Type), (stringvalue, format, formatProvider) => Type.GetType(stringvalue, true) },
                { typeof(NLog.Targets.LineEndingMode), (stringvalue, format, formatProvider) => NLog.Targets.LineEndingMode.FromString(stringvalue) },
                { typeof(Uri), (stringvalue, format, formatProvider) => new Uri(stringvalue) },
                { typeof(DateTime), (stringvalue, format, formatProvider) => ConvertToDateTime(format, formatProvider, stringvalue) },
                { typeof(DateTimeOffset), (stringvalue, format, formatProvider) => ConvertToDateTimeOffset(format, formatProvider, stringvalue) },
                { typeof(TimeSpan), (stringvalue, format, formatProvider) => ConvertToTimeSpan(format, formatProvider, stringvalue) },
                { typeof(Guid), (stringvalue, format, formatProvider) => ConvertGuid(format, stringvalue) }
            };
        }

        internal static bool IsComplexType(Type type)
        {
            return !type.IsValueType() && !typeof(IConvertible).IsAssignableFrom(type) && !StringConverterLookup.ContainsKey(type) && type.GetFirstCustomAttribute<System.ComponentModel.TypeConverterAttribute>() == null;
        }

        /// <inheritdoc/>
        public object Convert(object propertyValue, Type propertyType, string format, IFormatProvider formatProvider)
        {
            if (!NeedToConvert(propertyValue, propertyType))
            {
                return propertyValue;
            }

            var nullableType = Nullable.GetUnderlyingType(propertyType);
            propertyType = nullableType ?? propertyType;
            if (propertyValue is string propertyString)
            {
                propertyValue = propertyString = propertyString.Trim();

                if (nullableType != null && StringHelpers.IsNullOrWhiteSpace(propertyString))
                {
                    return null;
                }

                if (StringConverterLookup.TryGetValue(propertyType, out var converter))
                {
                    return converter.Invoke(propertyString, format, formatProvider);
                }

                if (propertyType.IsEnum() && NLog.Common.ConversionHelpers.TryParseEnum(propertyString, propertyType, out var enumValue))
                {
                    return enumValue;
                }

                if (!typeof(IConvertible).IsAssignableFrom(propertyType) && PropertyHelper.TryTypeConverterConversion(propertyType, propertyString, out var convertedValue))
                {
                    return convertedValue;
                }
            }
            else if (!string.IsNullOrEmpty(format) && propertyValue is IFormattable formattableValue)
            {
                propertyValue = formattableValue.ToString(format, formatProvider);
            }

#if !NETSTANDARD1_3 && !NETSTANDARD1_5
            if (propertyValue is IConvertible convertibleValue)
            {
                var typeCode = convertibleValue.GetTypeCode();
                if (typeCode == TypeCode.DBNull)
                    return propertyValue;
            }
#endif
            return System.Convert.ChangeType(propertyValue, propertyType, formatProvider);
        }

        private static bool NeedToConvert(object propertyValue, Type propertyType)
        {
            return propertyType != null && propertyValue != null && propertyValue.GetType() != propertyType && propertyType != typeof(object);
        }

        private static object ConvertGuid(string format, string propertyString)
        {
#if !NET35
            return string.IsNullOrEmpty(format) ? Guid.Parse(propertyString) : Guid.ParseExact(propertyString, format);
#else
            return new Guid(propertyString);       
#endif
        }

        private static object ConvertToEncoding(string stringValue)
        {
            stringValue = stringValue.Trim();
            if (string.Equals(stringValue, nameof(System.Text.Encoding.UTF8), StringComparison.OrdinalIgnoreCase))
                stringValue = System.Text.Encoding.UTF8.WebName;  // Support utf8 without hyphen (And not just Utf-8)
            return System.Text.Encoding.GetEncoding(stringValue);
        }

        private static object ConvertToTimeSpan(string format, IFormatProvider formatProvider, string propertyString)
        {
#if !NET35
            if (!string.IsNullOrEmpty(format))
                return TimeSpan.ParseExact(propertyString, format, formatProvider);
            return TimeSpan.Parse(propertyString, formatProvider);
#else
            return TimeSpan.Parse(propertyString);
#endif
        }

        private static object ConvertToDateTimeOffset(string format, IFormatProvider formatProvider, string propertyString)
        {
            if (!string.IsNullOrEmpty(format))
                return DateTimeOffset.ParseExact(propertyString, format, formatProvider);
            return DateTimeOffset.Parse(propertyString, formatProvider);
        }

        private static object ConvertToDateTime(string format, IFormatProvider formatProvider, string propertyString)
        {
            if (!string.IsNullOrEmpty(format))
                return DateTime.ParseExact(propertyString, format, formatProvider);
            return DateTime.Parse(propertyString, formatProvider);
        }
    }
}
