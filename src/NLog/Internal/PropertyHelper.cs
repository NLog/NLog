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

namespace NLog.Internal
{
    internal sealed class PropertyHelper
    {
        private static TypeToPropertyInfoDictionaryAssociation _parameterInfoCache = new TypeToPropertyInfoDictionaryAssociation();

        private PropertyHelper(){}

        public static string ExpandVariables(string input, NameValueCollection variables)
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

        private static object GetEnumValue(Type enumType, string value)
        {
            if (enumType.IsDefined(typeof(FlagsAttribute), false))
            {
                ulong union = 0;

                foreach (string v in value.Split(','))
                {
                    FieldInfo enumField = enumType.GetField(v.Trim(), BindingFlags.IgnoreCase | BindingFlags.Static | BindingFlags.FlattenHierarchy | BindingFlags.Public);
                    union |= Convert.ToUInt64(enumField.GetValue(null));
                }
                object retval = Convert.ChangeType(union, Enum.GetUnderlyingType(enumType), CultureInfo.InvariantCulture);
                return retval;
            }
            else
            {
                FieldInfo enumField = enumType.GetField(value, BindingFlags.IgnoreCase | BindingFlags.Static | BindingFlags.FlattenHierarchy | BindingFlags.Public);
                return enumField.GetValue(null);
            }
        }

        public static bool SetPropertyFromElement(object o, XmlElement el, NameValueCollection variables)
        {
            if (AddArrayItemFromElement(o, el, variables))
                return true;

            if (SetLayoutFromElement(o, el, variables))
                return true;

            return SetPropertyFromString(o, el.LocalName, el.InnerText, variables);
        }

        public static bool SetPropertyFromString(object o, string name, string value0, NameValueCollection variables)
        {
            string value = ExpandVariables(value0, variables);

            InternalLogger.Debug("Setting '{0}.{1}' to '{2}'", o.GetType().Name, name, value);

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

                object newValue;

                if (propInfo.PropertyType.IsEnum)
                {
                    newValue = GetEnumValue(propInfo.PropertyType, value);
                }
                else
                {
                    newValue = Convert.ChangeType(value, propInfo.PropertyType, CultureInfo.InvariantCulture);
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

        public static bool AddArrayItemFromElement(object o, XmlElement el, NameValueCollection variables)
        {
            string name = el.Name;
            if (!IsArrayProperty(o.GetType(), name))
                return false;
            PropertyInfo propInfo = GetPropertyInfo(o, name);
            if (propInfo == null)
                throw new NotSupportedException("Parameter " + name + " not supported on " + o.GetType().Name);

            IList propertyValue = (IList)propInfo.GetValue(o, null);
            Type elementType = GetArrayItemType(propInfo);
            object arrayItem = FactoryHelper.CreateInstance(elementType);
            ConfigureObjectFromAttributes(arrayItem, el.Attributes, variables, true);
            ConfigureObjectFromElement(arrayItem, el, variables);
            propertyValue.Add(arrayItem);
            return true;
        }

        internal static PropertyInfo GetPropertyInfo(object o, string propertyName)
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
            if (propertyName != "")
            {
                PropertyInfo propInfo = targetType.GetProperty(propertyName, BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);
                if (propInfo != null)
                    return propInfo;
            }

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
                if (propInfo.IsDefined(typeof(DefaultParameterAttribute), false))
                    retVal[""] = propInfo;
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

        public static bool IsLayoutProperty(Type t, string name)
        {
            PropertyInfo propInfo = GetPropertyInfo(t, name);
            if (propInfo == null)
                throw new NotSupportedException("Parameter " + name + " not supported on " + t.Name);

            if (typeof(ILayout).IsAssignableFrom(propInfo.PropertyType))
                return true;

            if (0 == String.Compare(name, "layout", true) && typeof(TargetWithLayout).IsAssignableFrom(propInfo.DeclaringType))
                return true;

            return false;
        }

        public static bool EqualsCI(string p1, string p2)
        {
            return String.Compare(p1, p2, true) == 0;
        }

        public static string GetCaseInsensitiveAttribute(XmlElement element, string name, NameValueCollection variables)
        {
            // first try a case-sensitive match
            string s = element.GetAttribute(name);
            if (s != null && s != "")
                return PropertyHelper.ExpandVariables(s, variables);

            // then look through all attributes and do a case-insensitive compare
            // this isn't very fast, but we don't need ultra speed here

            foreach (XmlAttribute a in element.Attributes)
            {
                if (EqualsCI(a.LocalName, name))
                    return PropertyHelper.ExpandVariables(a.Value, variables);
            }

            return null;
        }

        public static bool HasCaseInsensitiveAttribute(XmlElement element, string name)
        {
            // first try a case-sensitive match
            if (element.HasAttribute(name))
                return true;

            // then look through all attributes and do a case-insensitive compare
            // this isn't very fast, but we don't need ultra speed here because usually we have about
            // 3 attributes per element

            foreach (XmlAttribute a in element.Attributes)
            {
                if (EqualsCI(a.LocalName, name))
                    return true;
            }

            return false;
        }

        public static bool SetLayoutFromElement(object o, XmlElement el, NameValueCollection variables)
        {
            string name = el.LocalName;
            if (!IsLayoutProperty(o.GetType(), name))
                return false;

            PropertyInfo targetPropertyInfo = o.GetType().GetProperty(name, BindingFlags.Public | BindingFlags.Instance | BindingFlags.FlattenHierarchy | BindingFlags.IgnoreCase);

            if (targetPropertyInfo != null && typeof(ILayout).IsAssignableFrom(targetPropertyInfo.PropertyType))
            {
                ILayout layout = LayoutFactory.CreateLayout(GetCaseInsensitiveAttribute(el, "type", variables));
                ConfigureObjectFromAttributes(layout, el.Attributes, variables, true);
                ConfigureObjectFromElement(layout, el, variables);
                targetPropertyInfo.SetValue(o, layout, null);
                return true;
            }

            if (name == "layout" && (o is TargetWithLayout))
            {
                if (HasCaseInsensitiveAttribute(el, "type"))
                {
                    ILayout layout = LayoutFactory.CreateLayout(GetCaseInsensitiveAttribute(el, "type", variables));
                    ConfigureObjectFromAttributes(layout, el.Attributes, variables, true);
                    ConfigureObjectFromElement(layout, el, variables);
                    ((TargetWithLayout)o).CompiledLayout = layout;
                }
                else
                {
                    ((TargetWithLayout)o).Layout = el.InnerText;
                }
                return true;
            }

            return false;
        }

        public static void ConfigureObjectFromAttributes(object targetObject, XmlAttributeCollection attrs, NameValueCollection variables, bool ignoreType)
        {
            foreach (XmlAttribute attrib in attrs)
            {
                string childName = attrib.LocalName;
                string childValue = attrib.InnerText;

                if (ignoreType && 0 == String.Compare(childName, "type", true))
                    continue;

                PropertyHelper.SetPropertyFromString(targetObject, childName, childValue, variables);
            }
        }

        public static void ConfigureObjectFromElement(object targetObject, XmlElement el, NameValueCollection variables)
        {
            foreach (XmlElement el2 in GetChildElements(el))
            {
                SetPropertyFromElement(targetObject, el2, variables);
            }
        }

        internal static XmlNode[] GetChildElements(XmlElement element)
        {
            ArrayList list = new ArrayList();
            foreach (XmlNode n in element.ChildNodes)
            {
                if (n is XmlElement)
                    list.Add(n);
            }
            return (XmlElement[])list.ToArray(typeof(XmlElement));
        }

        internal static XmlNode[] GetChildElements(XmlElement element, string localName)
        {
            ArrayList list = new ArrayList();
            foreach (XmlNode n in element.ChildNodes)
            {
                if (0 != String.Compare(localName, n.LocalName, true))
                    continue;

                if (n is XmlElement)
                    list.Add(n);
            }
            return (XmlElement[])list.ToArray(typeof(XmlElement));
        }
    }
}
