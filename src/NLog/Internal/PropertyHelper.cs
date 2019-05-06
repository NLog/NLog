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

namespace NLog.Internal
{
    using System;
    using System.Collections;
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

#pragma warning disable S1144 // Unused private types or members should be removed. BUT they help CoreRT to provide config through reflection
        private static readonly RequiredParameterAttribute _requiredParameterAttribute = new RequiredParameterAttribute();
        private static readonly ArrayParameterAttribute _arrayParameterAttribute = new ArrayParameterAttribute(null, string.Empty);
        private static readonly DefaultValueAttribute _defaultValueAttribute = new DefaultValueAttribute(string.Empty);
        private static readonly AdvancedAttribute _advancedAttribute = new AdvancedAttribute();
        private static readonly DefaultParameterAttribute _defaultParameterAttribute = new DefaultParameterAttribute();
        private static readonly FlagsAttribute _flagsAttribute = new FlagsAttribute();
#pragma warning restore S1144 // Unused private types or members should be removed

        /// <summary>
        /// Set value parsed from string.
        /// </summary>
        /// <param name="obj">object instance to set with property <paramref name="propertyName"/></param>
        /// <param name="propertyName">name of the property on <paramref name="obj"/></param>
        /// <param name="value">The value to be parsed.</param>
        /// <param name="configurationItemFactory"></param>
        internal static void SetPropertyFromString(object obj, string propertyName, string value, ConfigurationItemFactory configurationItemFactory)
        {
            InternalLogger.Debug("Setting '{0}.{1}' to '{2}'", obj.GetType().Name, propertyName, value);

            PropertyInfo propInfo;

            if (!TryGetPropertyInfo(obj, propertyName, out propInfo))
            {
                throw new NotSupportedException($"Parameter {propertyName} not supported on {obj.GetType().Name}");
            }

            try
            {
                if (propInfo.IsDefined(_arrayParameterAttribute.GetType(), false))
                {
                    throw new NotSupportedException($"Parameter {propertyName} of {obj.GetType().Name} is an array and cannot be assigned a scalar value.");
                }

                object newValue;

                Type propertyType = propInfo.PropertyType;

                propertyType = Nullable.GetUnderlyingType(propertyType) ?? propertyType;

                if (!(TryNLogSpecificConversion(propertyType, value, out newValue, configurationItemFactory)
                    || TryGetEnumValue(propertyType, value, out newValue, true)
                    || TrySpecialConversion(propertyType, value, out newValue)
                    || TryImplicitConversion(propertyType, value, out newValue)
                    || TryFlatListConversion(propertyType, value, out newValue)
                    || TryTypeConverterConversion(propertyType, value, out newValue)))
                    newValue = Convert.ChangeType(value, propertyType, CultureInfo.InvariantCulture);

                propInfo.SetValue(obj, newValue, null);
            }
            catch (TargetInvocationException ex)
            {
                throw new NLogConfigurationException($"Error when setting property '{propInfo.Name}' on {obj}", ex.InnerException);
            }
            catch (Exception exception)
            {
                InternalLogger.Warn(exception, "Error when setting property '{0}' on '{1}'", propInfo.Name, obj);

                if (exception.MustBeRethrownImmediately())
                {
                    throw;
                }

                throw new NLogConfigurationException($"Error when setting property '{propInfo.Name}' on {obj}", exception);
            }
        }

        /// <summary>
        /// Is the property of array-type?
        /// </summary>
        /// <param name="t">Type which has the property <paramref name="propertyName"/></param>
        /// <param name="propertyName">name of the property.</param>
        /// <returns></returns>
        internal static bool IsArrayProperty(Type t, string propertyName)
        {
            PropertyInfo propInfo;

            if (!TryGetPropertyInfo(t, propertyName, out propInfo))
            {
                throw new NotSupportedException($"Parameter {propertyName} not supported on {t.Name}");
            }

            return propInfo.IsDefined(_arrayParameterAttribute.GetType(), false);
        }

        /// <summary>
        /// Get propertyinfo
        /// </summary>
        /// <param name="obj">object which could have property <paramref name="propertyName"/></param>
        /// <param name="propertyName">propertyname on <paramref name="obj"/></param>
        /// <param name="result">result when success.</param>
        /// <returns>success.</returns>
        internal static bool TryGetPropertyInfo(object obj, string propertyName, out PropertyInfo result)
        {
            PropertyInfo propInfo = obj.GetType().GetProperty(propertyName, BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);
            if (propInfo != null)
            {
                result = propInfo;
                return true;
            }

            lock (parameterInfoCache)
            {
                Type targetType = obj.GetType();
                Dictionary<string, PropertyInfo> cache;

                if (!parameterInfoCache.TryGetValue(targetType, out cache))
                {
                    cache = BuildPropertyInfoDictionary(targetType);
                    parameterInfoCache[targetType] = cache;
                }

                return cache.TryGetValue(propertyName, out result);
            }
        }

        internal static Type GetArrayItemType(PropertyInfo propInfo)
        {
            var arrayParameterAttribute = propInfo.GetFirstCustomAttribute<ArrayParameterAttribute>();
            return arrayParameterAttribute?.ItemType;
        }

        internal static PropertyInfo[] GetAllReadableProperties(Type type)
        {
            return type.GetProperties(BindingFlags.Public | BindingFlags.Instance);
        }

        internal static void CheckRequiredParameters(object o)
        {
            foreach (PropertyInfo propInfo in GetAllReadableProperties(o.GetType()))
            {
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

        private static bool TryImplicitConversion(Type resultType, string value, out object result)
        {
            try
            {
#if !NETSTANDARD1_3
                if (Type.GetTypeCode(resultType) != TypeCode.Object)
#else
                if (resultType.IsPrimitive() || resultType == typeof(string))
#endif
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

        private static bool TryNLogSpecificConversion(Type propertyType, string value, out object newValue, ConfigurationItemFactory configurationItemFactory)
        {
            if (propertyType == typeof(Layout) || propertyType == typeof(SimpleLayout))
            {
                newValue = new SimpleLayout(value, configurationItemFactory);
                return true;
            }

            if (propertyType == typeof(ConditionExpression))
            {
                newValue = ConditionParser.ParseExpression(value, configurationItemFactory);
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

        private static bool TrySpecialConversion(Type type, string value, out object newValue)
        {
            if (type == typeof(Encoding))
            {
                value = value.Trim();
                if (string.Equals(value, nameof(Encoding.UTF8), StringComparison.OrdinalIgnoreCase))
                    value = Encoding.UTF8.WebName;  // Support utf8 without hyphen (And not just Utf-8)
                newValue = Encoding.GetEncoding(value);
                return true;
            }

            if (type == typeof(CultureInfo))
            {
                value = value.Trim();
                newValue = new CultureInfo(value);
                return true;
            }

            if (type == typeof(Type))
            {
                value = value.Trim();
                newValue = Type.GetType(value, true);
                return true;
            }

            newValue = null;
            return false;
        }

        /// <summary>
        /// Try parse of string to (Generic) list, comma separated.
        /// </summary>
        /// <remarks>
        /// If there is a comma in the value, then (single) quote the value. For single quotes, use the backslash as escape
        /// </remarks>
        /// <param name="type"></param>
        /// <param name="valueRaw"></param>
        /// <param name="newValue"></param>
        /// <returns></returns>
        private static bool TryFlatListConversion(Type type, string valueRaw, out object newValue)
        {
            if (type.IsGenericType() && TryCreateCollectionObject(type, valueRaw, out var newList, out var collectionAddMethod, out var propertyType))
            {
                var values = valueRaw.SplitQuoted(',', '\'', '\\');
                foreach (var value in values)
                {
                    if (!(TryGetEnumValue(propertyType, value, out newValue, false)
                          || TrySpecialConversion(propertyType, value, out newValue)
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

        private static bool TryCreateCollectionObject(Type collectionType, string valueRaw, out object collectionObject, out MethodInfo collectionAddMethod, out Type collectionItemType)
        {
            var typeDefinition = collectionType.GetGenericTypeDefinition();
#if NET3_5
            var isSet = typeDefinition == typeof(HashSet<>);
#else
            var isSet = typeDefinition == typeof(ISet<>) || typeDefinition == typeof(HashSet<>);
#endif
            //not checking "implements" interface as we are creating HashSet<T> or List<T> and also those checks are expensive
            if (isSet || typeDefinition == typeof(List<>) || typeDefinition == typeof(IList<>) || typeDefinition == typeof(IEnumerable<>)) //set or list/array etc
            {
                //note: type.GenericTypeArguments is .NET 4.5+ 
                collectionItemType = collectionType.GetGenericArguments()[0];

                var listType = isSet ? typeof(HashSet<>) : typeof(List<>);
                var genericArgs = collectionItemType;
                var concreteType = listType.MakeGenericType(genericArgs);
                collectionObject = Activator.CreateInstance(concreteType);
                //no support for array
                if (collectionObject == null)
                {
                    throw new NLogConfigurationException("Cannot create instance of {0} for value {1}", collectionType.ToString(), valueRaw);
                }

                collectionAddMethod = concreteType.GetMethod("Add", BindingFlags.Instance | BindingFlags.Public);
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

        private static bool TryTypeConverterConversion(Type type, string value, out object newValue)
        {
#if !SILVERLIGHT && !NETSTANDARD1_3
            var converter = TypeDescriptor.GetConverter(type);
            if (converter.CanConvertFrom(typeof(string)))
            {
                newValue = converter.ConvertFromInvariantString(value);
                return true;
            }
#else
            if (type == typeof(LineEndingMode))
            {
                newValue = LineEndingMode.FromString(value);
                return true;
            }
            else if (type == typeof(Uri))
            {
                newValue = new Uri(value);
                return true;
            }
#endif

            newValue = null;
            return false;
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

            lock (parameterInfoCache)
            {
                Dictionary<string, PropertyInfo> cache;

                if (!parameterInfoCache.TryGetValue(targetType, out cache))
                {
                    cache = BuildPropertyInfoDictionary(targetType);
                    parameterInfoCache[targetType] = cache;
                }

                return cache.TryGetValue(propertyName, out result);
            }
        }

        private static Dictionary<string, PropertyInfo> BuildPropertyInfoDictionary(Type t)
        {
            var retVal = new Dictionary<string, PropertyInfo>(StringComparer.OrdinalIgnoreCase);
            foreach (PropertyInfo propInfo in GetAllReadableProperties(t))
            {
                var arrayParameterAttribute = propInfo.GetFirstCustomAttribute<ArrayParameterAttribute>();

                if (arrayParameterAttribute != null)
                {
                    retVal[arrayParameterAttribute.ElementName] = propInfo;
                }
                else
                {
                    retVal[propInfo.Name] = propInfo;
                }

                if (propInfo.IsDefined(_defaultParameterAttribute.GetType(), false))
                {
                    // define a property with empty name
                    retVal[string.Empty] = propInfo;
                }
            }

            return retVal;
        }
    }
}