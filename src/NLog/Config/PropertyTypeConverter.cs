//
// Copyright (c) 2004-2024 Jaroslaw Kowalski <jaak@jkowalski.net>, Kim Christensen, Julian Verdurmen
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
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;
    using NLog.Common;
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


        [UnconditionalSuppressMessage("Trimming - Allow converting option-values from config", "IL2057")]
        internal static Type ConvertToType(string stringvalue, bool throwOnError)
        {
            return Type.GetType(stringvalue, throwOnError);
        }

        internal static bool IsComplexType(Type type)
        {
            return !type.IsValueType && !typeof(IConvertible).IsAssignableFrom(type) && !HasConvertFromStringSupport(type);
        }

        /// <inheritdoc/>
        public object? Convert(object? propertyValue, Type propertyType, string? format, IFormatProvider? formatProvider)
        {
            if (propertyValue is null || propertyType is null || propertyType.Equals(typeof(object)))
            {
                return propertyValue;   // Type conversion not possible
            }

            var propertyValueType = propertyValue.GetType();
            if (propertyType.IsAssignableFrom(propertyValueType))
            {
                return propertyValue;   // Type is matching
            }

            var nullableType = Nullable.GetUnderlyingType(propertyType);
            if (nullableType != null)
            {
                if (nullableType.IsAssignableFrom(propertyValueType))
                {
                    return propertyValue;   // Type is matching
                }

                if (propertyValue is string propertyString && StringHelpers.IsNullOrWhiteSpace(propertyString))
                {
                    return null;
                }

                propertyType = nullableType;
            }

            return ChangeObjectType(propertyValue, propertyType, format, formatProvider);
        }

        /// <summary>
        /// Remember to align with <see cref="TryConvertFromString"/>
        /// </summary>
        private static bool HasConvertFromStringSupport(Type type)
        {
            if (type == typeof(System.Text.Encoding))
                return true;
            if (type == typeof(System.Globalization.CultureInfo))
                return true;
            if (type == typeof(Type))
                return true;
            if (type == typeof(NLog.Targets.LineEndingMode))
                return true;
            if (type == typeof(LogLevel))
                return true;
            if (type == typeof(Uri))
                return true;
            if (type == typeof(DateTime))
                return true;
            if (type == typeof(DateTimeOffset))
                return true;
            if (type == typeof(TimeSpan))
                return true;
            if (type == typeof(Guid))
                return true;
            if (type.IsEnum)
                return true;
            return false;
        }

        /// <summary>
        /// Remember to align with <see cref="HasConvertFromStringSupport"/>
        /// </summary>
        internal static bool TryConvertFromString(string propertyString, Type propertyType, string? format, IFormatProvider? formatProvider, out object? propertyValue)
        {
            propertyValue = propertyString = propertyString.Trim();

            if (propertyType == typeof(System.Text.Encoding))
            {
                propertyValue = ConvertToEncoding(propertyString);
                return true;
            }
            if (propertyType == typeof(CultureInfo))
            {
                propertyValue = ConvertToCultureInfo(propertyString);
                return true;
            }
            if (propertyType == typeof(Type))
            {
                propertyValue = ConvertToType(propertyString, true);
                return true;
            }
            if (propertyType == typeof(NLog.Targets.LineEndingMode))
            {
                propertyValue = NLog.Targets.LineEndingMode.FromString(propertyString);
                return true;
            }
            if (propertyType == typeof(LogLevel))
            {
                propertyValue = LogLevel.FromString(propertyString);
                return true;
            }
            if (propertyType == typeof(Uri))
            {
                propertyValue = new Uri(propertyString);
                return true;
            }
            if (propertyType == typeof(DateTime))
            {
                propertyValue = ConvertToDateTime(format, formatProvider, propertyString);
                return true;
            }
            if (propertyType == typeof(DateTimeOffset))
            {
                propertyValue = ConvertToDateTimeOffset(format, formatProvider, propertyString);
                return true;
            }
            if (propertyType == typeof(TimeSpan))
            {
                propertyValue = ConvertToTimeSpan(format, formatProvider, propertyString);
                return true;
            }
            if (propertyType == typeof(Guid))
            {
                propertyValue = ConvertGuid(format, propertyString);
                return true;
            }

            if (propertyType.IsEnum)
            {
                if (!NLog.Common.ConversionHelpers.TryParseEnum(propertyString, propertyType, out propertyValue))
                {
                    throw new ArgumentException($"Failed parsing Enum {propertyType.Name} from value: {propertyString}");
                }
                return true;
            }

            if (!typeof(IConvertible).IsAssignableFrom(propertyType) && !propertyType.IsAssignableFrom(typeof(string)))
            {
                if (PropertyHelper.TryTypeConverterConversion(propertyType, propertyString, out var convertedValue))
                {
                    propertyValue = convertedValue;
                    return true;
                }
            }

            return false;
        }

        private static object? ChangeObjectType(object propertyValue, Type propertyType, string? format, IFormatProvider? formatProvider)
        {
            if (propertyValue is string propertyString)
            {
                if (TryConvertFromString(propertyString, propertyType, format, formatProvider, out var fromStringValue))
                    return fromStringValue;
            }
            else if ((!string.IsNullOrEmpty(format) || typeof(string) == propertyType) && propertyValue is IFormattable formattableValue)
            {
                var stringValue = formattableValue.ToString(format, formatProvider);
                if (TryConvertFromString(stringValue, propertyType, format, formatProvider, out var fromStringValue))
                    return fromStringValue;

                propertyValue = stringValue;
            }
            else if (propertyValue is IConvertible convertibleValue)
            {
                var typeCode = convertibleValue.GetTypeCode();
                if (typeCode == TypeCode.DBNull)
                    return convertibleValue;
                if (typeCode == TypeCode.Empty)
                    return null;
                if (typeCode == TypeCode.DateTime && typeof(DateTimeOffset) == propertyType)
                    return new DateTimeOffset(convertibleValue.ToDateTime(formatProvider));
            }
            else
            {
                if (TryConvertToType(propertyValue, propertyType, out var convertedValue))
                    return convertedValue;
            }

            return System.Convert.ChangeType(propertyValue, propertyType, formatProvider);
        }

        //[RequiresDynamicCode("TypeDescriptor requires dynamic code")]
        [UnconditionalSuppressMessage("Trimming - Allow converting option-values from config", "IL2026")]
        [UnconditionalSuppressMessage("Trimming - Allow converting option-values from config", "IL2067")]
        [UnconditionalSuppressMessage("Trimming - Allow converting option-values from config", "IL2072")]
        private static bool TryConvertToType(object propertyValue, Type propertyType, out object? convertedValue)
        {
#if NETSTANDARD2_1_OR_GREATER || NET
            if (!System.Runtime.CompilerServices.RuntimeFeature.IsDynamicCodeSupported)
#else
            if (propertyValue is null || propertyType.IsAssignableFrom(propertyValue.GetType()))
#endif
            {
                convertedValue = null;
                return false;
            }

            try
            {
                var typeConverter = System.ComponentModel.TypeDescriptor.GetConverter(propertyValue.GetType());
                if (typeConverter != null && typeConverter.CanConvertTo(propertyType))
                {
                    convertedValue = typeConverter.ConvertTo(propertyValue, propertyType);
                    return true;
                }
            }
            catch (Exception ex)
            {
                InternalLogger.Error(ex, "Error in lookup of TypeDescriptor for type={0} to convert value '{1}'", propertyValue.GetType(), propertyValue);
                convertedValue = null;
                return false;
            }

            convertedValue = null;
            return false;
        }

        private static Guid ConvertGuid(string? format, string propertyString)
        {
#if !NET35
            return string.IsNullOrEmpty(format) ? Guid.Parse(propertyString) : Guid.ParseExact(propertyString, format);
#else
            return new Guid(propertyString);
#endif
        }

        internal static CultureInfo? ConvertToCultureInfo(string? stringValue)
        {
            if (stringValue is null || StringHelpers.IsNullOrWhiteSpace(stringValue))
                return null;
            if (nameof(CultureInfo.InvariantCulture).Equals(stringValue, StringComparison.OrdinalIgnoreCase))
                return CultureInfo.InvariantCulture;
            if (nameof(CultureInfo.CurrentCulture).Equals(stringValue, StringComparison.OrdinalIgnoreCase))
                return CultureInfo.CurrentCulture;
            return new CultureInfo(stringValue);
        }

        internal static System.Text.Encoding ConvertToEncoding(string stringValue)
        {
            stringValue = stringValue.Trim();
            if (string.Equals(stringValue, nameof(System.Text.Encoding.UTF8), StringComparison.OrdinalIgnoreCase))
                stringValue = System.Text.Encoding.UTF8.WebName;  // Support utf8 without hyphen (And not just Utf-8)
            return System.Text.Encoding.GetEncoding(stringValue);
        }

        private static TimeSpan ConvertToTimeSpan(string? format, IFormatProvider? formatProvider, string propertyString)
        {
#if !NET35
            if (!string.IsNullOrEmpty(format))
                return TimeSpan.ParseExact(propertyString, format, formatProvider);
            return TimeSpan.Parse(propertyString, formatProvider);
#else
            return TimeSpan.Parse(propertyString);
#endif
        }

        private static DateTimeOffset ConvertToDateTimeOffset(string? format, IFormatProvider? formatProvider, string propertyString)
        {
            if (!string.IsNullOrEmpty(format))
                return DateTimeOffset.ParseExact(propertyString, format, formatProvider);
            return DateTimeOffset.Parse(propertyString, formatProvider);
        }

        private static DateTime ConvertToDateTime(string? format, IFormatProvider? formatProvider, string propertyString)
        {
            if (!string.IsNullOrEmpty(format))
                return DateTime.ParseExact(propertyString, format, formatProvider);
            return DateTime.Parse(propertyString, formatProvider);
        }
    }
}
