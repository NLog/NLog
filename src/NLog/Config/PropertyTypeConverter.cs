// 
// Copyright (c) 2004-2021 Jaroslaw Kowalski <jaak@jkowalski.net>, Kim Christensen, Julian Verdurmen
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
    using System.Diagnostics.CodeAnalysis;
    using System.Reflection;
    using NLog.Internal;

    /// <summary>
    /// Default implementation of <see cref="IPropertyTypeConverter"/>
    /// </summary>
    internal sealed class PropertyTypeConverter : IPropertyTypeConverter
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
                { typeof(Type), (stringvalue, format, formatProvider) => ConvertToType(stringvalue, true) },
                { typeof(NLog.Targets.LineEndingMode), (stringvalue, format, formatProvider) => NLog.Targets.LineEndingMode.FromString(stringvalue) },
                { typeof(LogLevel), (stringvalue, format, formatProvider) => LogLevel.FromString(stringvalue) },
                { typeof(Uri), (stringvalue, format, formatProvider) => new Uri(stringvalue) },
                { typeof(DateTime), (stringvalue, format, formatProvider) => ConvertToDateTime(format, formatProvider, stringvalue) },
                { typeof(DateTimeOffset), (stringvalue, format, formatProvider) => ConvertToDateTimeOffset(format, formatProvider, stringvalue) },
                { typeof(TimeSpan), (stringvalue, format, formatProvider) => ConvertToTimeSpan(format, formatProvider, stringvalue) },
                { typeof(Guid), (stringvalue, format, formatProvider) => ConvertGuid(format, stringvalue) },
            };
        }

        [UnconditionalSuppressMessage("Trimming - Allow converting option-values from config", "IL2057")]
        internal static Type ConvertToType(string stringvalue, bool throwOnError)
        {
            return Type.GetType(stringvalue, throwOnError);
        }

        internal static bool IsComplexType(Type type)
        {
            return !type.IsValueType() && !typeof(IConvertible).IsAssignableFrom(type) && !StringConverterLookup.ContainsKey(type) && type.GetFirstCustomAttribute<System.ComponentModel.TypeConverterAttribute>() is null;
        }

        /// <inheritdoc/>
        public object Convert(object propertyValue, Type propertyType, string format, IFormatProvider formatProvider)
        {
            if (propertyValue is null || propertyType is null || propertyType == typeof(object))
            {
                return propertyValue;   // No type conversion required
            }

            var propertyValueType = propertyValue.GetType();
            if (propertyType.Equals(propertyValueType))
            {
                return propertyValue;   // Same type
            }

            var nullableType = Nullable.GetUnderlyingType(propertyType);
            if (nullableType != null)
            {
                if (nullableType.Equals(propertyValueType))
                {
                    return propertyValue;   // Same type
                }

                if (propertyValue is string propertyString && StringHelpers.IsNullOrWhiteSpace(propertyString))
                {
                    return null;
                }

                propertyType = nullableType;
            }

            return ChangeObjectType(propertyValue, propertyType, format, formatProvider);
        }

        private static bool TryConvertFromString(string propertyString, Type propertyType, string format, IFormatProvider formatProvider, out object propertyValue)
        {
            propertyValue = propertyString = propertyString.Trim();

            if (StringConverterLookup.TryGetValue(propertyType, out var converter))
            {
                propertyValue = converter.Invoke(propertyString, format, formatProvider);
                return true;
            }

            if (propertyType.IsEnum())
            {
                return NLog.Common.ConversionHelpers.TryParseEnum(propertyString, propertyType, out propertyValue);
            }

            if (PropertyHelper.TryTypeConverterConversion(propertyType, propertyString, out var convertedValue))
            {
                propertyValue = convertedValue;
                return true;
            }

            return false;
        }

        private static object ChangeObjectType(object propertyValue, Type propertyType, string format, IFormatProvider formatProvider)
        {
            if (propertyValue is string propertyString && TryConvertFromString(propertyString, propertyType, format, formatProvider, out propertyValue))
            {
                return propertyValue;
            }

            if (propertyValue is IConvertible convertibleValue)
            {
                var typeCode = convertibleValue.GetTypeCode();
#if !NETSTANDARD1_3 && !NETSTANDARD1_5
                if (typeCode == TypeCode.DBNull)
                    return convertibleValue;
#endif
                if (typeCode == TypeCode.Empty)
                    return null;
            }
            else if (TryConvertToType(propertyValue, propertyType, out var convertedValue))
            {
                return convertedValue;
            }

            if (!string.IsNullOrEmpty(format) && propertyValue is IFormattable formattableValue)
            {
                propertyValue = formattableValue.ToString(format, formatProvider);
            }

            return System.Convert.ChangeType(propertyValue, propertyType, formatProvider);
        }

        [UnconditionalSuppressMessage("Trimming - Allow converting option-values from config", "IL2026")]
        [UnconditionalSuppressMessage("Trimming - Allow converting option-values from config", "IL2067")]
        [UnconditionalSuppressMessage("Trimming - Allow converting option-values from config", "IL2072")]
        private static bool TryConvertToType(object propertyValue, Type propertyType, out object convertedValue)
        {
            if (propertyValue is null || propertyType.IsAssignableFrom(propertyValue.GetType()))
            {
                convertedValue = null;
                return false;
            }

            var typeConverter = System.ComponentModel.TypeDescriptor.GetConverter(propertyValue.GetType());
            if (typeConverter != null && typeConverter.CanConvertTo(propertyType))
            {
                convertedValue = typeConverter.ConvertTo(propertyValue, propertyType);
                return true;
            }

            convertedValue = null;
            return false;
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
