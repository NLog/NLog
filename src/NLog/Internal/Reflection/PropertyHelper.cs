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
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Globalization;
    using System.Linq;
    using System.Reflection;
    using System.Text;
    using NLog.Common;
    using NLog.Conditions;
    using NLog.Config;
    using NLog.Internal;
    using NLog.Layouts;
    using NLog.Targets;

    /// <summary>
    /// Reflection helpers for accessing properties.
    /// </summary>
    internal static class PropertyHelper
    {
        private static Dictionary<Type, Dictionary<string, PropertyInfo>> parameterInfoCache = new Dictionary<Type, Dictionary<string, PropertyInfo>>();
        private static Dictionary<Type, Func<string, ConfigurationItemFactory, object>> DefaultPropertyConversionMapper = BuildPropertyConversionMapper();

#pragma warning disable S1144 // Unused private types or members should be removed. BUT they help CoreRT to provide config through reflection
        private static readonly RequiredParameterAttribute _requiredParameterAttribute = new RequiredParameterAttribute();
        private static readonly ArrayParameterAttribute _arrayParameterAttribute = new ArrayParameterAttribute(null, string.Empty);
        private static readonly DefaultValueAttribute _defaultValueAttribute = new DefaultValueAttribute(string.Empty);
        private static readonly AdvancedAttribute _advancedAttribute = new AdvancedAttribute();
        private static readonly DefaultParameterAttribute _defaultParameterAttribute = new DefaultParameterAttribute();
        private static readonly NLogConfigurationIgnorePropertyAttribute _ignorePropertyAttribute = new NLogConfigurationIgnorePropertyAttribute();
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
                { typeof(CultureInfo), (stringvalue, factory) => new CultureInfo(stringvalue.Trim()) },
                { typeof(Type),  (stringvalue, factory) => Type.GetType(stringvalue.Trim(), true) },
                { typeof(LineEndingMode), (stringvalue, factory) => LineEndingMode.FromString(stringvalue.Trim()) },
                { typeof(Uri), (stringvalue, factory) => new Uri(stringvalue.Trim()) }
            };
        }

        /// <summary>
        /// Set value parsed from string.
        /// </summary>
        /// <param name="obj">object instance to set with property <paramref name="propertyName"/></param>
        /// <param name="propertyName">name of the property on <paramref name="obj"/></param>
        /// <param name="value">The value to be parsed.</param>
        /// <param name="configurationItemFactory"></param>
        internal static void SetPropertyFromString(object obj, string propertyName, string value, ConfigurationItemFactory configurationItemFactory)
        {
            var objType = obj.GetType();
            InternalLogger.Debug("Setting '{0}.{1}' to '{2}'", objType, propertyName, value);

            if (!TryGetPropertyInfo(objType, propertyName, out var propInfo))
            {
                throw new NLogConfigurationException($"Unknown property '{propertyName}'='{value}' for '{objType.Name}'");
            }

            try
            {
                Type propertyType = propInfo.PropertyType;

                if (!TryNLogSpecificConversion(propertyType, value, configurationItemFactory, out var newValue))
                {
                    if (propInfo.IsDefined(_arrayParameterAttribute.GetType(), false))
                    {
                        throw new NotSupportedException($"Property {propertyName} on {objType.Name} is an array, and cannot be assigned a scalar value: '{value}'.");
                    }

                    propertyType = Nullable.GetUnderlyingType(propertyType) ?? propertyType;

                    if (!(TryGetEnumValue(propertyType, value, out newValue, true)
                        || TryImplicitConversion(propertyType, value, out newValue)
                        || TryFlatListConversion(obj, propInfo, value, configurationItemFactory, out newValue)
                        || TryTypeConverterConversion(propertyType, value, out newValue)))
                        newValue = Convert.ChangeType(value, propertyType, CultureInfo.InvariantCulture);
                }

                propInfo.SetValue(obj, newValue, null);
            }
            catch (TargetInvocationException ex)
            {
                throw new NLogConfigurationException($"Error when setting property '{propInfo.Name}'='{value}' on {objType.Name}", ex.InnerException);
            }
            catch (Exception exception)
            {
                if (exception.MustBeRethrownImmediately())
                {
                    throw;
                }

                throw new NLogConfigurationException($"Error when setting property '{propInfo.Name}'='{value}' on {objType.Name}", exception);
            }
        }

        /// <summary>
        /// Get property info
        /// </summary>
        /// <param name="obj">object which could have property <paramref name="propertyName"/></param>
        /// <param name="propertyName">property name on <paramref name="obj"/></param>
        /// <param name="result">result when success.</param>
        /// <returns>success.</returns>
        internal static bool TryGetPropertyInfo(object obj, string propertyName, out PropertyInfo result)
        {
            return TryGetPropertyInfo(obj.GetType(), propertyName, out result);
        }

        internal static Type GetArrayItemType(PropertyInfo propInfo)
        {
            var arrayParameterAttribute = propInfo.GetFirstCustomAttribute<ArrayParameterAttribute>();
            return arrayParameterAttribute?.ItemType;
        }

        internal static bool IsConfigurationItemType(Type type)
        {
            if (type == null || IsSimplePropertyType(type))
                return false;

            if (typeof(LayoutRenderers.LayoutRenderer).IsAssignableFrom(type))
                return true;

            if (typeof(Layout).IsAssignableFrom(type))
                return true;

            // NLog will register no properties for types that are not marked with NLogConfigurationItemAttribute
            return TryLookupConfigItemProperties(type) != null;
        }

        internal static Dictionary<string, PropertyInfo> GetAllConfigItemProperties(Type type)
        {
            // NLog will ignore all properties marked with NLogConfigurationIgnorePropertyAttribute
            return TryLookupConfigItemProperties(type) ?? new Dictionary<string, PropertyInfo>();
        }

        private static Dictionary<string, PropertyInfo> TryLookupConfigItemProperties(Type type)
        {
            lock (parameterInfoCache)
            {
                // NLog will ignore all properties marked with NLogConfigurationIgnorePropertyAttribute
                if (!parameterInfoCache.TryGetValue(type, out var cache))
                {
                    if (TryCreatePropertyInfoDictionary(type, out cache))
                    {
                        parameterInfoCache[type] = cache;
                    }
                    else
                    {
                        parameterInfoCache[type] = null;    // Not config item type
                    }
                }

                return cache;
            }
        }

        internal static void CheckRequiredParameters(object o)
        {
            foreach (var configProp in GetAllConfigItemProperties(o.GetType()))
            {
                var propInfo = configProp.Value;
                if (propInfo.IsDefined(_requiredParameterAttribute.GetType(), false))
                {
                    object value = propInfo.GetValue(o, null);
                    if (value == null)
                    {
                        throw new NLogConfigurationException(
                            $"Required parameter '{propInfo.Name}' on '{o}' was not specified.");
                    }
                }
            }
        }

        internal static bool IsSimplePropertyType(Type type)
        {
#if !NETSTANDARD1_3
            if (Type.GetTypeCode(type) != TypeCode.Object)
#else
            if (type.IsPrimitive() || type == typeof(string))
#endif
                return true;

            if (type == typeof(CultureInfo))
                return true;

            if (type == typeof(Type))
                return true;

            if (type == typeof(Encoding))
                return true;

            return false;
        }

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
                if (operatorImplicitMethod == null || !resultType.IsAssignableFrom(operatorImplicitMethod.ReturnType))
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

        private static bool TryNLogSpecificConversion(Type propertyType, string value, ConfigurationItemFactory configurationItemFactory, out object newValue)
        {
            if (DefaultPropertyConversionMapper.TryGetValue(propertyType, out var objectConverter))
            {
                newValue = objectConverter.Invoke(value, configurationItemFactory);
                return true;
            }

            if (propertyType.IsGenericType() && propertyType.GetGenericTypeDefinition() == typeof(Layout<>))
            {
                var simpleLayout = new SimpleLayout(value, configurationItemFactory);
                var concreteType = typeof(Layout<>).MakeGenericType(propertyType.GetGenericArguments());
                newValue = Activator.CreateInstance(concreteType, BindingFlags.Instance | BindingFlags.Public, null, new object[] { simpleLayout }, null);
                return true;
            }

            newValue = null;
            return false;
        }

        private static bool TryGetEnumValue(Type resultType, string value, out object result, bool flagsEnumAllowed)
        {
            if (!resultType.IsEnum())
            {
                result = null;
                return false;
            }

            if (flagsEnumAllowed && resultType.IsDefined(_flagsAttribute.GetType(), false))
            {
                ulong union = 0;

                foreach (string v in value.SplitAndTrimTokens(','))
                {
                    FieldInfo enumField = resultType.GetField(v, BindingFlags.IgnoreCase | BindingFlags.Static | BindingFlags.FlattenHierarchy | BindingFlags.Public);
                    if (enumField == null)
                    {
                        throw new NLogConfigurationException($"Invalid enumeration value '{value}'.");
                    }

                    union |= Convert.ToUInt64(enumField.GetValue(null), CultureInfo.InvariantCulture);
                }

                result = Convert.ChangeType(union, Enum.GetUnderlyingType(resultType), CultureInfo.InvariantCulture);
                result = Enum.ToObject(resultType, result);

                return true;
            }
            else
            {
                FieldInfo enumField = resultType.GetField(value, BindingFlags.IgnoreCase | BindingFlags.Static | BindingFlags.FlattenHierarchy | BindingFlags.Public);
                if (enumField == null)
                {
                    throw new NLogConfigurationException($"Invalid enumeration value '{value}'.");
                }

                result = enumField.GetValue(null);
                return true;
            }
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
            return ConditionParser.ParseExpression(stringValue, configurationItemFactory);
        }

        /// <summary>
        /// Try parse of string to (Generic) list, comma separated.
        /// </summary>
        /// <remarks>
        /// If there is a comma in the value, then (single) quote the value. For single quotes, use the backslash as escape
        /// </remarks>
        private static bool TryFlatListConversion(object obj, PropertyInfo propInfo, string valueRaw, ConfigurationItemFactory configurationItemFactory, out object newValue)
        {
            if (propInfo.PropertyType.IsGenericType() && TryCreateCollectionObject(obj, propInfo, valueRaw, out var newList, out var collectionAddMethod, out var propertyType))
            {
                var values = valueRaw.SplitQuoted(',', '\'', '\\');
                foreach (var value in values)
                {
                    if (!(TryGetEnumValue(propertyType, value, out newValue, false)
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

        private static bool TryCreateCollectionObject(object obj, PropertyInfo propInfo, string valueRaw, out object collectionObject, out MethodInfo collectionAddMethod, out Type collectionItemType)
        {
            var collectionType = propInfo.PropertyType;
            var typeDefinition = collectionType.GetGenericTypeDefinition();
#if !NET35
            var isSet = typeDefinition == typeof(ISet<>) || typeDefinition == typeof(HashSet<>);
#else
            var isSet = typeDefinition == typeof(HashSet<>);       
#endif
            //not checking "implements" interface as we are creating HashSet<T> or List<T> and also those checks are expensive
            if (isSet || typeDefinition == typeof(List<>) || typeDefinition == typeof(IList<>) || typeDefinition == typeof(IEnumerable<>)) //set or list/array etc
            {
                object hashsetComparer = isSet ? ExtractHashSetComparer(obj, propInfo) : null;

                //note: type.GenericTypeArguments is .NET 4.5+ 
                collectionItemType = collectionType.GetGenericArguments()[0];
                collectionObject = CreateCollectionObjectInstance(isSet ? typeof(HashSet<>) : typeof(List<>), collectionItemType, hashsetComparer);
                //no support for array
                if (collectionObject == null)
                {
                    throw new NLogConfigurationException("Cannot create instance of {0} for value {1}", collectionType.ToString(), valueRaw);
                }

                collectionAddMethod = collectionObject.GetType().GetMethod("Add", BindingFlags.Instance | BindingFlags.Public);
                if (collectionAddMethod == null)
                {
                    throw new NLogConfigurationException("Add method on type {0} for value {1} not found", collectionType.ToString(), valueRaw);
                }

                return true;
            }

            collectionObject = null;
            collectionAddMethod = null;
            collectionItemType = null;
            return false;
        }

        private static object CreateCollectionObjectInstance(Type collectionType, Type collectionItemType, object hashSetComparer)
        {
            var concreteType = collectionType.MakeGenericType(collectionItemType);
            if (hashSetComparer != null)
            {
                var constructor = concreteType.GetConstructor(new[] { hashSetComparer.GetType() });
                if (constructor != null)
                    return constructor.Invoke(new[] { hashSetComparer });
            }
            return Activator.CreateInstance(concreteType);
        }

        /// <summary>
        /// Attempt to reuse the HashSet.Comparer from the original HashSet-object (Ex. StringComparer.OrdinalIgnoreCase)
        /// </summary>
        private static object ExtractHashSetComparer(object obj, PropertyInfo propInfo)
        {
            var exitingValue = propInfo.IsValidPublicProperty() ? propInfo.GetPropertyValue(obj) : null;
            if (exitingValue != null)
            {
                // Found original HashSet-object. See if we can extract the Comparer
                var comparerPropInfo = exitingValue.GetType().GetProperty("Comparer", BindingFlags.Instance | BindingFlags.Public);
                if (comparerPropInfo.IsValidPublicProperty())
                {
                    return comparerPropInfo.GetPropertyValue(exitingValue);
                }
            }

            return null;
        }

        internal static bool TryTypeConverterConversion(Type type, string value, out object newValue)
        {
            try
            {
#if !NETSTANDARD1_3
                var converter = TypeDescriptor.GetConverter(type);
                if (converter.CanConvertFrom(typeof(string)))
                {
                    newValue = converter.ConvertFromInvariantString(value);
                    return true;
                }
#endif
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

        private static bool TryGetPropertyInfo(Type targetType, string propertyName, out PropertyInfo result)
        {
            if (!string.IsNullOrEmpty(propertyName))
            {
                PropertyInfo propInfo = targetType.GetProperty(propertyName, BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);
                if (propInfo != null)
                {
                    result = propInfo;
                    return true;
                }
            }

            // NLog has special property-lookup handling for default-parameters (and array-properties)
            var configProperties = GetAllConfigItemProperties(targetType);
            return configProperties.TryGetValue(propertyName, out result);
        }

        private static bool TryCreatePropertyInfoDictionary(Type t, out Dictionary<string, PropertyInfo> result)
        {
            result = null;

            try
            {
                if (!t.IsDefined(typeof(NLogConfigurationItemAttribute), true))
                {
                    return false;
                }

                result = new Dictionary<string, PropertyInfo>(StringComparer.OrdinalIgnoreCase);
                foreach (PropertyInfo propInfo in t.GetProperties(BindingFlags.Public | BindingFlags.Instance))
                {
                    try
                    {
                        var parameterName = LookupPropertySymbolName(propInfo);
                        if (string.IsNullOrEmpty(parameterName))
                        {
                            continue;
                        }

                        result[parameterName] = propInfo;

                        if (propInfo.IsDefined(_defaultParameterAttribute.GetType(), false))
                        {
                            // define a property with empty name (Default property name)
                            result[string.Empty] = propInfo;
                        }
                    }
                    catch (Exception ex)
                    {
                        InternalLogger.Debug(ex, "Type reflection not possible for property {0} on type {1}. Maybe because of .NET Native.", propInfo.Name, t);
                    }
                }
            }
            catch (Exception ex)
            {
                InternalLogger.Debug(ex, "Type reflection not possible for type {0}. Maybe because of .NET Native.", t);
            }

            return result != null;
        }

        private static string LookupPropertySymbolName(PropertyInfo propInfo)
        {
            if (propInfo.PropertyType == null)
                return null;

            if (IsSimplePropertyType(propInfo.PropertyType))
                return propInfo.Name;

            if (typeof(LayoutRenderers.LayoutRenderer).IsAssignableFrom(propInfo.PropertyType))
                return propInfo.Name;

            if (typeof(Layout).IsAssignableFrom(propInfo.PropertyType))
                return propInfo.Name;

            if (propInfo.IsDefined(_ignorePropertyAttribute.GetType(), false))
                return null;

            var arrayParameterAttribute = propInfo.GetFirstCustomAttribute<ArrayParameterAttribute>();
            if (arrayParameterAttribute != null)
            {
                return arrayParameterAttribute.ElementName;
            }

            return propInfo.Name;
        }
    }
}