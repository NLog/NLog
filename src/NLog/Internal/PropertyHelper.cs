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

namespace NLog.Internal
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;
    using System.Reflection;
    using System.Text;
    using NLog.Common;
    using NLog.Conditions;
    using NLog.Config;
    using NLog.Layouts;
    using NLog.Targets;

    /// <summary>
    /// Reflection helpers for accessing properties.
    /// </summary>
    internal static class PropertyHelper
    {
        private static readonly Dictionary<Type, Dictionary<string, PropertyInfo>> _parameterInfoCache = new Dictionary<Type, Dictionary<string, PropertyInfo>>();
        private static readonly Dictionary<Type, Func<string, ConfigurationItemFactory, object>> _propertyConversionMapper = BuildPropertyConversionMapper();

#pragma warning disable S1144 // Unused private types or members should be removed. BUT they help CoreRT to provide config through reflection
        private static readonly RequiredParameterAttribute _requiredParameterAttribute = new RequiredParameterAttribute();
        private static readonly ArrayParameterAttribute _arrayParameterAttribute = new ArrayParameterAttribute(null, string.Empty);
        private static readonly DefaultParameterAttribute _defaultParameterAttribute = new DefaultParameterAttribute();
        private static readonly NLogConfigurationIgnorePropertyAttribute _ignorePropertyAttribute = new NLogConfigurationIgnorePropertyAttribute();
        private static readonly NLogConfigurationItemAttribute _configPropertyAttribute = new NLogConfigurationItemAttribute();
        private static readonly FlagsAttribute _flagsAttribute = new FlagsAttribute();
#pragma warning restore S1144 // Unused private types or members should be removed

        private static Dictionary<Type, Func<string, ConfigurationItemFactory, object>> BuildPropertyConversionMapper()
        {
            return new Dictionary<Type, Func<string, ConfigurationItemFactory, object>>()
            {
                { typeof(Layout), TryParseLayoutValue },
                { typeof(SimpleLayout), TryParseLayoutValue },
                { typeof(ConditionExpression), TryParseConditionValue },
                { typeof(Encoding), TryParseEncodingValue },
                { typeof(string), (stringvalue, factory) => stringvalue },
                { typeof(int), (stringvalue, factory) => Convert.ChangeType(stringvalue.Trim(), TypeCode.Int32, CultureInfo.InvariantCulture) },
                { typeof(bool), (stringvalue, factory) => Convert.ChangeType(stringvalue.Trim(), TypeCode.Boolean, CultureInfo.InvariantCulture) },
                { typeof(CultureInfo), (stringvalue, factory) =>  TryParseCultureInfo(stringvalue) },
                { typeof(Type),  (stringvalue, factory) => PropertyTypeConverter.ConvertToType(stringvalue.Trim(), true) },
                { typeof(LineEndingMode), (stringvalue, factory) => LineEndingMode.FromString(stringvalue.Trim()) },
                { typeof(Uri), (stringvalue, factory) => new Uri(stringvalue.Trim()) }
            };
        }

        internal static void SetPropertyFromString(object targetObject, PropertyInfo propInfo, string stringValue, ConfigurationItemFactory configurationItemFactory)
        {
            object propertyValue = null;

            try
            {
                var propertyType = Nullable.GetUnderlyingType(propInfo.PropertyType) ?? propInfo.PropertyType;
                if (ReferenceEquals(propertyType, propInfo.PropertyType) || !StringHelpers.IsNullOrWhiteSpace(stringValue))
                {
                    if (!TryNLogSpecificConversion(propertyType, stringValue, configurationItemFactory, out propertyValue))
                    {
                        if (propInfo.IsDefined(_arrayParameterAttribute.GetType(), false))
                        {
                            throw new NotSupportedException($"'{targetObject?.GetType()?.Name}' cannot assign property '{propInfo.Name}', because property of type array and not scalar value: '{stringValue}'.");
                        }

                        if (!(TryGetEnumValue(propertyType, stringValue, out propertyValue)
                            || TryImplicitConversion(propertyType, stringValue, out propertyValue)
                            || TryFlatListConversion(targetObject, propInfo, stringValue, configurationItemFactory, out propertyValue)
                            || TryTypeConverterConversion(propertyType, stringValue, out propertyValue)))
                            propertyValue = Convert.ChangeType(stringValue, propertyType, CultureInfo.InvariantCulture);
                    }
                }
            }
            catch (Exception ex)
            {
                if (ex.MustBeRethrownImmediately())
                {
                    throw;
                }

                throw new NLogConfigurationException($"'{targetObject?.GetType()?.Name}' cannot assign property '{propInfo.Name}'='{stringValue}'. Error: {ex.Message}", ex);
            }

            SetPropertyValueForObject(targetObject, propertyValue, propInfo);
        }

        internal static void SetPropertyValueForObject(object targetObject, object value, PropertyInfo propInfo)
        {
            try
            {
                propInfo.SetValue(targetObject, value, null);
            }
            catch (TargetInvocationException ex)
            {
                throw new NLogConfigurationException($"'{targetObject?.GetType()?.Name}' cannot assign property '{propInfo.Name}'", ex.InnerException ?? ex);
            }
            catch (Exception ex)
            {
                if (ex.MustBeRethrownImmediately())
                {
                    throw;
                }

                throw new NLogConfigurationException($"'{targetObject?.GetType()?.Name}' cannot assign property '{propInfo.Name}'. Error={ex.Message}", ex);
            }
        }

        /// <summary>
        /// Get property info
        /// </summary>
        /// <param name="configFactory">Configuration Reflection Helper</param>
        /// <param name="obj">object which could have property <paramref name="propertyName"/></param>
        /// <param name="propertyName">property name on <paramref name="obj"/></param>
        /// <param name="result">result when success.</param>
        /// <returns>success.</returns>
        internal static bool TryGetPropertyInfo(ConfigurationItemFactory configFactory, object obj, string propertyName, out PropertyInfo result)
        {
            var configProperties = TryLookupConfigItemProperties(configFactory, obj.GetType());
            if (configProperties is null)
            {
                if (!string.IsNullOrEmpty(propertyName))
                {
#pragma warning disable CS0618 // Type or member is obsolete
                    return TryGetPropertyInfo(obj, propertyName, out result);
#pragma warning restore CS0618 // Type or member is obsolete
                }

                result = null;
                return false;
            }

            return configProperties.TryGetValue(propertyName, out result);
        }

        [UnconditionalSuppressMessage("Trimming - Ignore since obsolete", "IL2075")]
        [Obsolete("Instead use RegisterType<T>, as dynamic Assembly loading will be moved out. Marked obsolete with NLog v5.2")]
        private static bool TryGetPropertyInfo(object obj, string propertyName, out PropertyInfo result)
        {
            InternalLogger.Debug("Object reflection needed to configure instance of type: {0} (Lookup property={1})", obj.GetType(), propertyName);

            PropertyInfo propInfo = obj.GetType().GetProperty(propertyName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.IgnoreCase);
            if (propInfo != null)
            {
                result = propInfo;
                return true;
            }

            result = null;
            return false;
        }

        internal static Type GetArrayItemType(PropertyInfo propInfo)
        {
            var arrayParameterAttribute = propInfo.GetFirstCustomAttribute<ArrayParameterAttribute>();
            return arrayParameterAttribute?.ItemType;
        }

        internal static bool IsConfigurationItemType(ConfigurationItemFactory configFactory, Type type)
        {
            if (type is null || IsSimplePropertyType(type))
                return false;

            if (typeof(ISupportsInitialize).IsAssignableFrom(type))
                return true;    // Target, Layout, LayoutRenderer

            if (typeof(IEnumerable).IsAssignableFrom(type))
                return true;

            return TryLookupConfigItemProperties(configFactory, type) != null;
        }

        internal static Dictionary<string, PropertyInfo> GetAllConfigItemProperties(ConfigurationItemFactory configFactory, Type type)
        {
            return TryLookupConfigItemProperties(configFactory, type) ?? new Dictionary<string, PropertyInfo>();
        }

        private static Dictionary<string, PropertyInfo> TryLookupConfigItemProperties(ConfigurationItemFactory configFactory, Type type)
        {
            lock (_parameterInfoCache)
            {
                if (!_parameterInfoCache.TryGetValue(type, out var cache))
                {
                    if (TryCreatePropertyInfoDictionary(configFactory, type, out cache))
                    {
                        _parameterInfoCache[type] = cache;
                    }
                    else
                    {
                        _parameterInfoCache[type] = null;    // Not config item type
                    }
                }

                return cache;
            }
        }

        internal static void CheckRequiredParameters(ConfigurationItemFactory configFactory, object o)
        {
            foreach (var configProp in GetAllConfigItemProperties(configFactory, o.GetType()))
            {
                var propInfo = configProp.Value;
                var propertyType = propInfo.PropertyType;
                if (propertyType != null && (propertyType.IsClass || Nullable.GetUnderlyingType(propertyType) != null))
                {
                    if (propInfo.IsDefined(_requiredParameterAttribute.GetType(), false))
                    {
                        object value = propInfo.GetValue(o, null);
                        if (value is null)
                        {
                            throw new NLogConfigurationException(
                                $"Required parameter '{propInfo.Name}' on '{o}' was not specified.");
                        }
                    }
                }
            }
        }

        internal static bool IsSimplePropertyType(Type type)
        {
            if (Type.GetTypeCode(type) != TypeCode.Object)
                return true;

            if (type == typeof(CultureInfo))
                return true;

            if (type == typeof(Type))
                return true;

            if (type == typeof(Encoding))
                return true;

            if (type == typeof(LogLevel))
                return true;

            return false;
        }

        [UnconditionalSuppressMessage("Trimming - Allow converting option-values from config", "IL2070")]
        private static bool TryImplicitConversion(Type resultType, string value, out object result)
        {
            try
            {
                if (IsSimplePropertyType(resultType))
                {
                    result = null;
                    return false;
                }

                MethodInfo operatorImplicitMethod = resultType.GetMethod("op_Implicit", BindingFlags.Public | BindingFlags.Static, null, new Type[] { value.GetType() }, null);
                if (operatorImplicitMethod is null || !resultType.IsAssignableFrom(operatorImplicitMethod.ReturnType))
                {
                    result = null;
                    return false;
                }

                result = operatorImplicitMethod.Invoke(null, new object[] { value });
                return true;
            }
            catch (Exception ex)
            {
                InternalLogger.Warn(ex, "Implicit Conversion Failed of {0} to {1}", value, resultType);
            }
            result = null;
            return false;
        }

        [UnconditionalSuppressMessage("Trimming - Allow converting option-values from config", "IL2067")]
        private static bool TryNLogSpecificConversion(Type propertyType, string value, ConfigurationItemFactory configurationItemFactory, out object newValue)
        {
            if (_propertyConversionMapper.TryGetValue(propertyType, out var objectConverter))
            {
                newValue = objectConverter.Invoke(value, configurationItemFactory);
                return true;
            }

            if (propertyType.IsGenericType && propertyType.GetGenericTypeDefinition() == typeof(Layout<>))
            {
                var simpleLayout = new SimpleLayout(value, configurationItemFactory);
                newValue = Activator.CreateInstance(propertyType, BindingFlags.Instance | BindingFlags.Public, null, new object[] { simpleLayout }, null);
                return true;
            }

            newValue = null;
            return false;
        }

        private static bool TryGetEnumValue(Type resultType, string value, out object result)
        {
            if (!resultType.IsEnum)
            {
                result = null;
                return false;
            }

            if (!StringHelpers.IsNullOrWhiteSpace(value))
            {
                // Note: .NET Standard 2.1 added a public Enum.TryParse(Type)
                try
                {
                    result = (Enum)Enum.Parse(resultType, value, true);
                    return true;
                }
                catch (ArgumentException ex)
                {
                    throw new ArgumentException($"Failed parsing Enum {resultType.Name} from value: {value}", ex);
                }
            }
            else
            {
                result = null;
                return false;
            }
        }

        private static object TryParseCultureInfo(string stringValue)
        {
            stringValue = stringValue?.Trim();
            if (string.IsNullOrEmpty(stringValue))
                return CultureInfo.InvariantCulture;
            else
                return new CultureInfo(stringValue);
        }

        private static object TryParseEncodingValue(string stringValue, ConfigurationItemFactory configurationItemFactory)
        {
            _ = configurationItemFactory;   // Discard unreferenced parameter
            stringValue = stringValue.Trim();
            if (string.Equals(stringValue, nameof(Encoding.UTF8), StringComparison.OrdinalIgnoreCase))
                stringValue = Encoding.UTF8.WebName;  // Support utf8 without hyphen (And not just Utf-8)
            return Encoding.GetEncoding(stringValue);
        }

        private static object TryParseLayoutValue(string stringValue, ConfigurationItemFactory configurationItemFactory)
        {
            return new SimpleLayout(stringValue, configurationItemFactory);
        }

        private static object TryParseConditionValue(string stringValue, ConfigurationItemFactory configurationItemFactory)
        {
            try
            {
                return ConditionParser.ParseExpression(stringValue, configurationItemFactory);
            }
            catch (ConditionParseException ex)
            {
                throw new NLogConfigurationException($"Cannot parse ConditionExpression '{stringValue}'. Error: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Try parse of string to (Generic) list, comma separated.
        /// </summary>
        /// <remarks>
        /// If there is a comma in the value, then (single) quote the value. For single quotes, use the backslash as escape
        /// </remarks>
        private static bool TryFlatListConversion(object obj, PropertyInfo propInfo, string valueRaw, ConfigurationItemFactory configurationItemFactory, out object newValue)
        {
            var collectionType = propInfo.PropertyType;
            if (!collectionType.IsGenericType || !typeof(IEnumerable).IsAssignableFrom(collectionType))
            {
                newValue = null;
                return false;
            }

            try
            {
                if (TryCreateCollectionObject(obj, propInfo, out var newList, out var collectionAddMethod, out var propertyType))
                {
                    var values = valueRaw.SplitQuoted(',', '\'', '\\');
                    foreach (var value in values)
                    {
                        if (!(TryGetEnumValue(propertyType, value, out newValue)
                              || TryNLogSpecificConversion(propertyType, value, configurationItemFactory, out newValue)
                              || TryImplicitConversion(propertyType, value, out newValue)
                              || TryTypeConverterConversion(propertyType, value, out newValue)))
                        {
                            newValue = Convert.ChangeType(value, propertyType, CultureInfo.InvariantCulture);
                        }

                        collectionAddMethod.Invoke(newList, new object[] { newValue });
                    }

                    newValue = newList;
                    return true;
                }

                newValue = null;
                return false;
            }
            catch (Exception ex)
            {
                throw new NLogConfigurationException($"Failed to parse collection for property '{propInfo.Name}' on {obj.GetType()} with value '{valueRaw}'", ex);
            }
        }

        [UnconditionalSuppressMessage("Trimming - Allow converting option-values from config", "IL2072")]
        [UnconditionalSuppressMessage("Trimming - Allow converting option-values from config", "IL2075")]
        private static bool TryCreateCollectionObject(object obj, PropertyInfo propInfo, out object collectionObject, out MethodInfo collectionAddMethod, out Type collectionItemType)
        {
            collectionObject = null;
            collectionAddMethod = null;
            collectionItemType = null;

            var collectionType = propInfo.PropertyType;
            if (TryCreateListCollection<string>(collectionType, out collectionObject, out collectionAddMethod, out collectionItemType))
                return true;

            if (TryCreateListCollection<int>(collectionType, out collectionObject, out collectionAddMethod, out collectionItemType))
                return true;

            var typeDefinition = collectionType.GetGenericTypeDefinition();
            if (typeDefinition == typeof(List<>))
            {
                collectionObject = Activator.CreateInstance(collectionType);
                collectionAddMethod = collectionType.GetMethod("Add", BindingFlags.Instance | BindingFlags.Public);
                collectionItemType = collectionType.GetGenericArguments()[0];
                return true;
            }

            if (typeDefinition != typeof(IList<>) && typeDefinition != typeof(IEnumerable<>) && typeDefinition != typeof(HashSet<>)
#if !NET35
                && typeDefinition != typeof(ISet<>)
#endif
                )
                return false;

            var existingValue = propInfo.IsValidPublicProperty() ? propInfo.GetPropertyValue(obj) : null;
            if (existingValue?.GetType().IsGenericType == true)
            {
                if (existingValue.GetType().GetGenericTypeDefinition() == typeof(List<>))
                {
                    var existingType = existingValue.GetType();
                    collectionObject = Activator.CreateInstance(existingType);
                    collectionAddMethod = existingType.GetMethod("Add", BindingFlags.Instance | BindingFlags.Public);
                    collectionItemType = existingType.GetGenericArguments()[0];
                    return true;
                }

                if (existingValue.GetType().GetGenericTypeDefinition() == typeof(HashSet<>))
                {
                    object hashSetComparer = null;
                    var existingType = existingValue.GetType();
                    var comparerPropInfo = existingType.GetProperty("Comparer", BindingFlags.Public | BindingFlags.Instance);
                    if (comparerPropInfo.IsValidPublicProperty())
                    {
                        hashSetComparer = comparerPropInfo.GetPropertyValue(existingValue);
                    }
                    if (hashSetComparer != null)
                    {
                        var constructor = existingType.GetConstructor(new[] { hashSetComparer.GetType() });
                        if (constructor != null)
                            collectionObject = constructor.Invoke(new[] { hashSetComparer });
                        else
                            collectionObject = Activator.CreateInstance(existingType);
                    }
                    else
                    {
                        collectionObject = Activator.CreateInstance(existingType);
                    }
                    collectionAddMethod = existingType.GetMethod("Add", BindingFlags.Instance | BindingFlags.Public);
                    collectionItemType = existingType.GetGenericArguments()[0];
                    return true;
                }
            }

            if (TryCreateHashSetCollection<string>(collectionType, out collectionObject, out collectionAddMethod, out collectionItemType))
                return true;

            if (TryCreateHashSetCollection<int>(collectionType, out collectionObject, out collectionAddMethod, out collectionItemType))
                return true;

            if (typeDefinition == typeof(HashSet<>))
            {
                collectionObject = Activator.CreateInstance(collectionType);
                collectionAddMethod = collectionType.GetMethod("Add", BindingFlags.Instance | BindingFlags.Public);
                collectionItemType = collectionType.GetGenericArguments()[0];
                return true;
            }

            InternalLogger.Debug("Object type reflection needed to configure instance of type: {0}", collectionType);
            return TryCreateTypeCollection(collectionType, out collectionObject, out collectionAddMethod, out collectionItemType);
         }

        private static bool TryCreateListCollection<T>(Type collectionType, out object collectionObject, out MethodInfo collectionAddMethod, out Type collectionItemType)
        {
            if (collectionType.IsAssignableFrom(typeof(List<T>)))
            {
                collectionObject = new List<T>();
                collectionAddMethod = typeof(List<T>).GetMethod("Add", BindingFlags.Instance | BindingFlags.Public);
                collectionItemType = typeof(T);
                return true;
            }

            collectionObject = null;
            collectionAddMethod = null;
            collectionItemType = null;
            return false;
        }

        private static bool TryCreateHashSetCollection<T>(Type collectionType, out object collectionObject, out MethodInfo collectionAddMethod, out Type collectionItemType)
        {
            if (collectionType.IsAssignableFrom(typeof(HashSet<T>)))
            {
                collectionObject = new HashSet<T>();
                collectionAddMethod = typeof(HashSet<T>).GetMethod("Add", BindingFlags.Instance | BindingFlags.Public);
                collectionItemType = typeof(T);
                return true;
            }

            collectionObject = null;
            collectionAddMethod = null;
            collectionItemType = null;
            return false;
        }

        [UnconditionalSuppressMessage("Trimming - Allow converting option-values from config", "IL3050")]
        private static bool TryCreateTypeCollection(Type propertyType, out object collectionObject, out MethodInfo collectionAddMethod, out Type collectionItemType)
        {
#if !NET35
            var typeDefinition = propertyType.GetGenericTypeDefinition();
            if (typeDefinition == typeof(ISet<>))
            {
                var setType = typeof(HashSet<>).MakeGenericType(propertyType.GetGenericArguments());
                collectionAddMethod = setType.GetMethod("Add", BindingFlags.Instance | BindingFlags.Public);
                collectionItemType = propertyType.GetGenericArguments()[0];
                collectionObject = Activator.CreateInstance(setType);
                return true;
            }
#endif

            var listType = typeof(List<>).MakeGenericType(propertyType.GetGenericArguments());
            collectionAddMethod = listType.GetMethod("Add", BindingFlags.Instance | BindingFlags.Public);
            collectionItemType = propertyType.GetGenericArguments()[0];
            collectionObject = Activator.CreateInstance(listType);
            return true;
        }

        [UnconditionalSuppressMessage("Trimming - Allow converting option-values from config", "IL2026")]
        [UnconditionalSuppressMessage("Trimming - Allow converting option-values from config", "IL2067")]
        [UnconditionalSuppressMessage("Trimming - Allow converting option-values from config", "IL2072")]
        internal static bool TryTypeConverterConversion(Type type, string value, out object newValue)
        {
            if (typeof(IConvertible).IsAssignableFrom(type) || type.IsAssignableFrom(typeof(string)))
            {
                newValue = null;
                return false;
            }

            try
            {
                InternalLogger.Debug("Object reflection needed for creating external type: {0} from string-value: {1}", type, value);

                var converter = TypeDescriptor.GetConverter(type);
                if (converter.CanConvertFrom(typeof(string)))
                {
                    newValue = converter.ConvertFromInvariantString(value);
                    return true;
                }

                newValue = null;
                return false;
            }
            catch (MissingMethodException ex)
            {
                InternalLogger.Error(ex, "Error in lookup of TypeDescriptor for type={0} to convert value '{1}'", type, value);
                newValue = null;
                return false;
            }
        }

        private static bool TryCreatePropertyInfoDictionary(ConfigurationItemFactory configFactory, Type objectType, out Dictionary<string, PropertyInfo> result)
        {
            result = null;

            try
            {
                if (!typeof(ISupportsInitialize).IsAssignableFrom(objectType) && !objectType.IsDefined(_configPropertyAttribute.GetType(), true))
                {
                    return false;
                }

                var properties = configFactory.TryGetTypeProperties(objectType);
                if (properties is null)
                {
                    return false;
                }

                if (properties.Count == 0)
                {
                    result = properties;
                    return true;
                }

                if (!HasCustomConfigurationProperties(objectType, properties))
                {
                    result = properties;
                    return true;
                }

                bool checkDefaultValue = typeof(LayoutRenderers.LayoutRenderer).IsAssignableFrom(objectType);

                result = new Dictionary<string, PropertyInfo>(properties.Count + 4, StringComparer.OrdinalIgnoreCase);
                foreach (var property in properties)
                {
                    var propInfo = property.Value;

                    if (!IncludeConfigurationPropertyInfo(objectType, propInfo, checkDefaultValue, out var overridePropertyName))
                        continue;

                    result[propInfo.Name] = propInfo;
                    if (overridePropertyName is null)
                        continue;

                    result[overridePropertyName] = propInfo;
                }
            }
            catch (Exception ex)
            {
                InternalLogger.Debug(ex, "Type reflection not possible for type {0}. Maybe because of .NET Native.", objectType);
            }

            return result != null;
        }

        private static bool HasCustomConfigurationProperties(Type objectType, Dictionary<string, PropertyInfo> objectProperties)
        {
            bool checkDefaultValue = typeof(LayoutRenderers.LayoutRenderer).IsAssignableFrom(objectType);

            foreach (var property in objectProperties)
            {
                if (IncludeConfigurationPropertyInfo(objectType, property.Value, checkDefaultValue, out var overridePropertyName) && overridePropertyName is null)
                    continue;

                return true;
            }

            return false;
        }

        private static bool IncludeConfigurationPropertyInfo(Type objectType, PropertyInfo propInfo, bool checkDefaultValue, out string overridePropertyName)
        {
            overridePropertyName = null;

            try
            {
                var propertyType = propInfo?.PropertyType;
                if (propertyType is null)
                    return false;

                if (checkDefaultValue && propInfo.IsDefined(_defaultParameterAttribute.GetType(), false))
                {
                    overridePropertyName = string.Empty;
                    return true;
                }

                if (IsSimplePropertyType(propertyType))
                    return true;

                if (typeof(ISupportsInitialize).IsAssignableFrom(propertyType))
                    return true;    // Target, Layout, LayoutRenderer

                if (propInfo.IsDefined(_ignorePropertyAttribute.GetType(), false))
                    return false;   // NLog will ignore all properties marked with NLogConfigurationIgnorePropertyAttribute

                if (typeof(IEnumerable).IsAssignableFrom(propertyType))
                {
                    var arrayParameterAttribute = propInfo.GetFirstCustomAttribute<ArrayParameterAttribute>();
                    if (arrayParameterAttribute != null)
                    {
                        overridePropertyName = arrayParameterAttribute.ElementName;
                        return true;
                    }
                }

                return true;
            }
            catch (Exception ex)
            {
                InternalLogger.Debug(ex, "Type reflection not possible for property {0} on type {1}. Maybe because of .NET Native.", propInfo.Name, objectType);
                return false;
            }
        }
    }
}
