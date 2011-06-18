// 
// Copyright (c) 2004-2011 Jaroslaw Kowalski <jaak@jkowalski.net>
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
        private static Dictionary<Type, Dictionary<string, PropertyInfo>> parameterInfoCache = new Dictionary<Type, Dictionary<string, PropertyInfo>>();

        internal static void SetPropertyFromString(object o, string name, string value, ConfigurationItemFactory configurationItemFactory)
        {
            InternalLogger.Debug("Setting '{0}.{1}' to '{2}'", o.GetType().Name, name, value);

            PropertyInfo propInfo;

            if (!TryGetPropertyInfo(o, name, out propInfo))
            {
                throw new NotSupportedException("Parameter " + name + " not supported on " + o.GetType().Name);
            }

            try
            {
                if (propInfo.IsDefined(typeof(ArrayParameterAttribute), false))
                {
                    throw new NotSupportedException("Parameter " + name + " of " + o.GetType().Name + " is an array and cannot be assigned a scalar value.");
                }

                object newValue;

                Type propertyType = propInfo.PropertyType;

                propertyType = Nullable.GetUnderlyingType(propertyType) ?? propertyType;

                if (!TryNLogSpecificConversion(propertyType, value, out newValue, configurationItemFactory))
                {
                    if (!TryGetEnumValue(propertyType, value, out newValue))
                    {
                        if (!TryImplicitConversion(propertyType, value, out newValue))
                        {
                            if (!TrySpecialConversion(propertyType, value, out newValue))
                            {
                                newValue = Convert.ChangeType(value, propertyType, CultureInfo.InvariantCulture);
                            }
                        }
                    }
                }

                propInfo.SetValue(o, newValue, null);
            }
            catch (TargetInvocationException ex)
            {
                throw new NLogConfigurationException("Error when setting property '" + propInfo.Name + "' on " + o, ex.InnerException);
            }
            catch (Exception exception)
            {
                if (exception.MustBeRethrown())
                {
                    throw;
                }

                throw new NLogConfigurationException("Error when setting property '" + propInfo.Name + "' on " + o, exception);
            }
        }

        internal static bool IsArrayProperty(Type t, string name)
        {
            PropertyInfo propInfo;

            if (!TryGetPropertyInfo(t, name, out propInfo))
            {
                throw new NotSupportedException("Parameter " + name + " not supported on " + t.Name);
            }

            return propInfo.IsDefined(typeof(ArrayParameterAttribute), false);
        }

        internal static bool TryGetPropertyInfo(object o, string propertyName, out PropertyInfo result)
        {
            PropertyInfo propInfo = o.GetType().GetProperty(propertyName, BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);
            if (propInfo != null)
            {
                result = propInfo;
                return true;
            }

            lock (parameterInfoCache)
            {
                Type targetType = o.GetType();
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
            var arrayParameterAttribute = (ArrayParameterAttribute)Attribute.GetCustomAttribute(propInfo, typeof(ArrayParameterAttribute));
            if (arrayParameterAttribute != null)
            {
                return arrayParameterAttribute.ItemType;
            }

            return null;
        }

        internal static IEnumerable<PropertyInfo> GetAllReadableProperties(Type type)
        {
#if NETCF2_0
            // .NET Compact Framework 2.0 understands 'Public' differently
            // it only returns properties where getter and setter are public

            var allProperties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic);
            var readableProperties = new List<PropertyInfo>();
            foreach (var prop in allProperties)
            {
                if (prop.CanRead)
                {
                    readableProperties.Add(prop);
                }
            }

            return readableProperties;
#else
            // other frameworks don't have this problem
            return type.GetProperties(BindingFlags.Public | BindingFlags.Instance);
#endif
        }

        internal static void CheckRequiredParameters(object o)
        {
            foreach (PropertyInfo propInfo in PropertyHelper.GetAllReadableProperties(o.GetType()))
            {
                if (propInfo.IsDefined(typeof(RequiredParameterAttribute), false))
                {
                    object value = propInfo.GetValue(o, null);
                    if (value == null)
                    {
                        throw new NLogConfigurationException(
                            "Required parameter '" + propInfo.Name + "' on '" + o + "' was not specified.");
                    }
                }
            }
        }

        private static bool TryImplicitConversion(Type resultType, string value, out object result)
        {
            MethodInfo operatorImplicitMethod = resultType.GetMethod("op_Implicit", BindingFlags.Public | BindingFlags.Static, null, new Type[] { typeof(string) }, null);
            if (operatorImplicitMethod == null)
            {
                result = null;
                return false;
            }

            result = operatorImplicitMethod.Invoke(null, new object[] { value });
            return true;
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

        private static bool TryGetEnumValue(Type resultType, string value, out object result)
        {
            if (!resultType.IsEnum)
            {
                result = null;
                return false;
            }

            if (resultType.IsDefined(typeof(FlagsAttribute), false))
            {
                ulong union = 0;

                foreach (string v in value.Split(','))
                {
                    FieldInfo enumField = resultType.GetField(v.Trim(), BindingFlags.IgnoreCase | BindingFlags.Static | BindingFlags.FlattenHierarchy | BindingFlags.Public);
                    if (enumField == null)
                    {
                        throw new NLogConfigurationException("Invalid enumeration value '" + value + "'.");
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
                    throw new NLogConfigurationException("Invalid enumeration value '" + value + "'.");
                }

                result = enumField.GetValue(null);
                return true;
            }
        }

        private static bool TrySpecialConversion(Type type, string value, out object newValue)
        {
            if (type == typeof(Uri))
            {
                newValue = new Uri(value, UriKind.RelativeOrAbsolute);
                return true;
            }

            if (type == typeof(Encoding))
            {
                newValue = Encoding.GetEncoding(value);
                return true;
            }

            if (type == typeof(CultureInfo))
            {
                newValue = new CultureInfo(value);
                return true;
            }

            if (type == typeof(Type))
            {
                newValue = Type.GetType(value, true);
                return true;
            }

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
                var arrayParameterAttribute = (ArrayParameterAttribute)Attribute.GetCustomAttribute(propInfo, typeof(ArrayParameterAttribute));

                if (arrayParameterAttribute != null)
                {
                    retVal[arrayParameterAttribute.ElementName] = propInfo;
                }
                else
                {
                    retVal[propInfo.Name] = propInfo;
                }

                if (propInfo.IsDefined(typeof(DefaultParameterAttribute), false))
                {
                    // define a property with empty name
                    retVal[string.Empty] = propInfo;
                }
            }

            return retVal;
        }
    }
}
