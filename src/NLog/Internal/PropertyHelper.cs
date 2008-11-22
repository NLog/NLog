// 
// Copyright (c) 2004-2006 Jaroslaw Kowalski <jaak@jkowalski.net>
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
using System.Collections;
using System.Reflection;
using System.Globalization;
using System.Collections.Specialized;
using System.Xml;

using NLog.Internal;
using NLog.Config;
using System.ComponentModel;
using System.Collections.Generic;
using NLog.Layouts;
using NLog.Targets;
using System.Text;

namespace NLog.Internal
{
    internal sealed class PropertyHelper
    {
        private static Dictionary<Type,Dictionary<string,PropertyInfo>> _parameterInfoCache = new Dictionary<Type,Dictionary<string,PropertyInfo>>();

        private PropertyHelper(){}

        public static string ExpandVariables(string input, IDictionary<string, string> variables)
        {
            if (variables == null || variables.Count == 0)
                return input;

            string output = input;

            // TODO - make this case-insensitive, will probably require a different
            // approach

            foreach (string s in variables.Keys)
            {
                output = output.Replace("${" + s + "}", variables[s]);
            }

            return output;
        }

        private static bool TryImplicitConversion(Type resultType, string value, out object result)
        {
            MethodInfo opImplicitMethod = resultType.GetMethod("op_Implicit", BindingFlags.Public | BindingFlags.Static, null, new Type[] { typeof(string) }, null);
            if (opImplicitMethod == null)
            {
                result = null;
                return false;
            }
            result = opImplicitMethod.Invoke(null, new object[] { value });
            return true;

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
                    union |= Convert.ToUInt64(enumField.GetValue(null));
                }
                result = Convert.ChangeType(union, Enum.GetUnderlyingType(resultType), CultureInfo.InvariantCulture);
                return true;
            }
            else
            {
                FieldInfo enumField = resultType.GetField(value, BindingFlags.IgnoreCase | BindingFlags.Static | BindingFlags.FlattenHierarchy | BindingFlags.Public);
                result = enumField.GetValue(null);
                return true;
            }
        }

        public static bool SetPropertyFromString(object o, string name, string value0, IDictionary<string, string> variables)
        {
            string value = ExpandVariables(value0, variables);

            InternalLogger.Debug("Setting '{0}.{1}' to '{2}'", o.GetType().Name, name, value);

            try
            {
                PropertyInfo propInfo;

                if (!TryGetPropertyInfo(o, name, out propInfo))
                {
                    throw new NotSupportedException("Parameter " + name + " not supported on " + o.GetType().Name);
                }

                if (propInfo.IsDefined(typeof(ArrayParameterAttribute), false))
                {
                    throw new NotSupportedException("Parameter " + name + " of " + o.GetType().Name + " is an array and cannot be assigned a scalar value.");
                }

                object newValue;

                if (!TryGetEnumValue(propInfo.PropertyType, value, out newValue))
                {
                    if (!TryImplicitConversion(propInfo.PropertyType, value, out newValue))
                    {
                        if (!TrySpecialConversion(propInfo.PropertyType, value, out newValue))
                        {
                            newValue = Convert.ChangeType(value, propInfo.PropertyType, CultureInfo.InvariantCulture);
                        }
                    }
                }
                propInfo.SetValue(o, newValue, null);
                return true;
            }
            catch (Exception ex)
            {
                InternalLogger.Error(ex.ToString());
                return false;
            }
        }

        private static bool TrySpecialConversion(Type type, string value, out object newValue)
        {
            if (type == typeof(Encoding) && value is string)
            {
                newValue = Encoding.GetEncoding(value);
                return true;
            }
            else
            {
                newValue = null;
                return false;
            }
        }

        internal static bool TryGetPropertyInfo(object o, string propertyName, out PropertyInfo result)
        {
            PropertyInfo propInfo = o.GetType().GetProperty(propertyName, BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);
            if (propInfo != null)
            {
                result = propInfo;
                return true;
            }

            lock (_parameterInfoCache)
            {
                Type targetType = o.GetType();
                Dictionary<string, PropertyInfo> cache;

                if (!_parameterInfoCache.TryGetValue(targetType, out cache))
                {
                    cache = BuildPropertyInfoDictionary(targetType);
                    _parameterInfoCache[targetType] = cache;
                }
                return cache.TryGetValue(propertyName.ToLower(), out result);
            }
        }

        private static bool TryGetPropertyInfo(Type targetType, string propertyName, out PropertyInfo result)
        {
            if (propertyName != "")
            {
                PropertyInfo propInfo = targetType.GetProperty(propertyName, BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);
                if (propInfo != null)
                {
                    result = propInfo;
                    return true;
                }
            }

            lock (_parameterInfoCache)
            {
                Dictionary<string, PropertyInfo> cache;

                if (!_parameterInfoCache.TryGetValue(targetType, out cache))
                {
                    cache = BuildPropertyInfoDictionary(targetType);
                    _parameterInfoCache[targetType] = cache;
                }
                return cache.TryGetValue(propertyName.ToLower(), out result);
            }
        }

        private static Dictionary<string, PropertyInfo> BuildPropertyInfoDictionary(Type t)
        {
            Dictionary<string, PropertyInfo> retVal = new Dictionary<string, PropertyInfo>();
            foreach (PropertyInfo propInfo in t.GetProperties())
            {
                if (propInfo.IsDefined(typeof(ArrayParameterAttribute), false))
                {
                    ArrayParameterAttribute[] attributes = (ArrayParameterAttribute[])propInfo.GetCustomAttributes(typeof(ArrayParameterAttribute), false);

                    retVal[attributes[0].ElementName.ToLower()] = propInfo;
                }
                else
                {
                    retVal[propInfo.Name.ToLower()] = propInfo;
                }
                if (propInfo.IsDefined(typeof(DefaultParameterAttribute), false))
                    retVal[""] = propInfo;
            }
            return retVal;
        }

        internal static Type GetArrayItemType(PropertyInfo propInfo)
        {
            if (propInfo.IsDefined(typeof(ArrayParameterAttribute), false))
            {
                ArrayParameterAttribute[] attributes = (ArrayParameterAttribute[])propInfo.GetCustomAttributes(typeof(ArrayParameterAttribute), false);

                return attributes[0].ItemType;
            }
            else
            {
                return null;
            }
        }

        public static bool IsArrayProperty(Type t, string name)
        {
            PropertyInfo propInfo;

            if (!TryGetPropertyInfo(t, name, out propInfo))
                throw new NotSupportedException("Parameter " + name + " not supported on " + t.Name);

            if (!propInfo.IsDefined(typeof(ArrayParameterAttribute), false))
            {
                return false;
            }
            else
            {
                return true;
            }
        }

        public static bool IsLayoutProperty(Type t, string name)
        {
            PropertyInfo propInfo;

            if (!TryGetPropertyInfo(t, name, out propInfo))
                throw new NotSupportedException("Parameter " + name + " not supported on " + t.Name);

            if (typeof(Layout).IsAssignableFrom(propInfo.PropertyType))
                return true;

            if (0 == String.Compare(name, "layout", StringComparison.InvariantCultureIgnoreCase) && typeof(TargetWithLayout).IsAssignableFrom(propInfo.DeclaringType))
                return true;

            return false;
        }
    }
}
