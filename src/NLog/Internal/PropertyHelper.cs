// 
// Copyright (c) 2004,2005 Jaroslaw Kowalski <jkowalski@users.sourceforge.net>
// 
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
// * Neither the name of the Jaroslaw Kowalski nor the names of its 
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
using System.Xml;

using NLog.Internal;
using NLog.Config;

namespace NLog.Internal
{
    internal sealed class PropertyHelper
    {
        private static TypeToPropertyInfoDictionaryAssociation _parameterInfoCache = new TypeToPropertyInfoDictionaryAssociation();

        private PropertyHelper(){}

        public static bool SetPropertyFromString(object o, string name, string value)
        {
            InternalLogger.Info("Setting '{0}.{1}' to '{2}'", o.GetType().Name, name, value);

            try
            {
                PropertyInfo propInfo = GetPropertyInfo(o, name);
                if (propInfo == null)
                {
                    throw new NotSupportedException("Parameter " + name + " not supported on " + o.GetType().Name);
                }

                if (propInfo.IsDefined(typeof(ArrayParameterAttribute), false))
                {
                    throw new NotSupportedException("Parameter " + name + " of " + o.GetType().Name + " is an array and cannot be assigned a scalar value.");
                }

                object newValue = Convert.ChangeType(value, propInfo.PropertyType, CultureInfo.InvariantCulture);

                propInfo.SetValue(o, newValue, null);
                return true;
            }
            catch (Exception ex)
            {
                InternalLogger.Error(ex.ToString());
                return false;
            }
        }

        public static void AddArrayItemFromElement(object o, XmlElement el)
        {
            string name = el.Name;
            PropertyInfo propInfo = GetPropertyInfo(o, name);
            if (propInfo == null)
                throw new NotSupportedException("Parameter " + name + " not supported on " + o.GetType().Name);

            IList propertyValue = (IList)propInfo.GetValue(o, null);
            Type elementType = GetArrayItemType(propInfo);
            object arrayItem = FactoryHelper.CreateInstance(elementType);

            foreach (XmlAttribute attrib in el.Attributes)
            {
                string childName = attrib.LocalName;
                string childValue = attrib.InnerText;

                PropertyHelper.SetPropertyFromString(arrayItem, childName, childValue);
            }

            foreach (XmlNode node in el.ChildNodes)
            {
                if (node is XmlElement)
                {
                    XmlElement el2 = (XmlElement)node;
                    string childName = el2.Name;

                    if (IsArrayProperty(elementType, childName))
                    {
                        PropertyHelper.AddArrayItemFromElement(arrayItem, el2);
                    }
                    else
                    {
                        string childValue = el2.InnerXml;

                        PropertyHelper.SetPropertyFromString(arrayItem, childName, childValue);
                    }
                }
            }

            propertyValue.Add(arrayItem);
        }

        private static PropertyInfo GetPropertyInfo(object o, string propertyName)
        {
            PropertyInfo propInfo = o.GetType().GetProperty(propertyName, BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);
            if (propInfo != null)
                return propInfo;

            lock(_parameterInfoCache)
            {
                Type targetType = o.GetType();
                PropertyInfoDictionary cache = _parameterInfoCache[targetType];
                if (cache == null)
                {
                    cache = BuildPropertyInfoDictionary(targetType);
                    _parameterInfoCache[targetType] = cache;
                }
                return cache[propertyName.ToLower()];
            }
        }

        private static PropertyInfo GetPropertyInfo(Type targetType, string propertyName)
        {
            PropertyInfo propInfo = targetType.GetProperty(propertyName, BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);
            if (propInfo != null)
                return propInfo;

            lock(_parameterInfoCache)
            {
                PropertyInfoDictionary cache = _parameterInfoCache[targetType];
                if (cache == null)
                {
                    cache = BuildPropertyInfoDictionary(targetType);
                    _parameterInfoCache[targetType] = cache;
                }
                return cache[propertyName.ToLower()];
            }
        }

        private static PropertyInfoDictionary BuildPropertyInfoDictionary(Type t)
        {
            PropertyInfoDictionary retVal = new PropertyInfoDictionary();
            foreach (PropertyInfo propInfo in t.GetProperties())
            {
                if (propInfo.IsDefined(typeof(ArrayParameterAttribute), false))
                {
                    ArrayParameterAttribute[]attributes = (ArrayParameterAttribute[])propInfo.GetCustomAttributes(typeof(ArrayParameterAttribute), false);

                    retVal[attributes[0].ElementName.ToLower()] = propInfo;
                }
                else
                {
                    retVal[propInfo.Name.ToLower()] = propInfo;
                }
            }
            return retVal;
        }

        private static Type GetArrayItemType(PropertyInfo propInfo)
        {
            if (propInfo.IsDefined(typeof(ArrayParameterAttribute), false))
            {
                ArrayParameterAttribute[]attributes = (ArrayParameterAttribute[])propInfo.GetCustomAttributes(typeof(ArrayParameterAttribute), false);

                return attributes[0].ItemType;
            }
            else
            {
                return null;
            }
        }

        public static bool IsArrayProperty(Type t, string name)
        {
            PropertyInfo propInfo = GetPropertyInfo(t, name);
            if (propInfo == null)
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
    }
}
