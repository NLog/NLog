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
#if DYNAMIC_OBJECT
    using System.Dynamic;
#endif
    using System.Linq;
    using System.Reflection;
    using NLog.Common;

    /// <summary>
    /// Converts object into a List of property-names and -values using reflection
    /// </summary>
    internal class ObjectReflectionCache
    {
        readonly MruCache<Type, ObjectPropertyInfos> _objectTypeCache = new MruCache<Type, ObjectPropertyInfos>(10000);
        static readonly Dictionary<Type, Func<object, IDictionary<string,object>>> _objectTypeOverride = new Dictionary<Type, Func<object, IDictionary<string, object>>>();

        public ObjectPropertyList LookupObjectProperties(object value)
        {
            Type objectType = value.GetType();
            if (_objectTypeCache.TryGetValue(objectType, out var propertyInfos))
            {
                if (!propertyInfos.HasFastLookup)
                {
                    var fastLookup = BuildFastLookup(propertyInfos.Properties, false);
                    propertyInfos = new ObjectPropertyInfos(propertyInfos.Properties, fastLookup);
                    _objectTypeCache.TryAddValue(objectType, propertyInfos);
                }
                return new ObjectPropertyList(value, propertyInfos.Properties, propertyInfos.FastLookup);
            }

            if (_objectTypeOverride.Count > 0)
            {
                // User defined object reflection override
                lock (_objectTypeOverride)
                {
                    if (_objectTypeOverride.TryGetValue(objectType, out var objectReflection))
                    {
                        var objectProperties = objectReflection.Invoke(value);
                        if (objectProperties?.Count > 0)
                            return new ObjectPropertyList(objectProperties);

                        // object.ToString() since no properties
                        propertyInfos = ObjectPropertyInfos.SimpleToString;
                        return new ObjectPropertyList(value, propertyInfos.Properties, propertyInfos.FastLookup);
                    }
                }
            }

#if DYNAMIC_OBJECT
            if (value is DynamicObject d)
            {
                var dictionary = DynamicObjectToDict(d);
                return new ObjectPropertyList(dictionary);
            }
#endif

            propertyInfos = BuildObjectPropertyInfos(value, objectType);
            _objectTypeCache.TryAddValue(objectType, propertyInfos);
            return new ObjectPropertyList(value, propertyInfos.Properties, propertyInfos.FastLookup);
        }

        private static ObjectPropertyInfos BuildObjectPropertyInfos(object value, Type objectType)
        {
            ObjectPropertyInfos propertyInfos;
            if (ConvertSimpleToString(objectType))
            {
                propertyInfos = ObjectPropertyInfos.SimpleToString;
            }
            else
            {
                var properties = GetPublicProperties(objectType);
                if (value is Exception)
                {
                    // Special handling of Exception (Include Exception-Type as artificial first property)
                    var fastLookup = BuildFastLookup(properties, true);
                    propertyInfos = new ObjectPropertyInfos(properties, fastLookup);
                }
                else if (properties.Length == 0)
                {
                    propertyInfos = ObjectPropertyInfos.SimpleToString;
                }
                else
                {
                    propertyInfos = new ObjectPropertyInfos(properties, null);
                }
            }

            return propertyInfos;
        }

        private static bool ConvertSimpleToString(Type objectType)
        {
            if (objectType == typeof(Guid))
                return true;

            if (objectType == typeof(TimeSpan))
                return true;

            if (typeof(Uri).IsAssignableFrom(objectType))
                return true;

            if (typeof(MemberInfo).IsAssignableFrom(objectType))
                return true;

            if (typeof(Assembly).IsAssignableFrom(objectType))
                return true;

            return false;
        }

        private static PropertyInfo[] GetPublicProperties(Type type)
        {
            PropertyInfo[] properties = null;

            try
            {
                properties = type.GetProperties(PublicProperties);
            }
            catch (Exception ex)
            {
                InternalLogger.Warn(ex, "Failed to get object properties for type: {0}", type);
            }

            // Skip Index-Item-Properties (Ex. public string this[int Index])
            if (properties != null)
            {
                foreach (var prop in properties)
                {
                    if (!ValidPublicProperty(prop))
                    {
                        properties = properties.Where(p => ValidPublicProperty(p)).ToArray();
                        break;
                    }
                }
            }

            return properties ?? ArrayHelper.Empty<PropertyInfo>();
        }

        private static bool ValidPublicProperty(PropertyInfo p)
        {
            return p.CanRead && p.GetIndexParameters().Length == 0 && p.GetGetMethod() != null;
        }

        private static FastPropertyLookup[] BuildFastLookup(PropertyInfo[] properties, bool includeType)
        {
            int fastAccessIndex = includeType ? 1 : 0;
            FastPropertyLookup[] fastLookup = new FastPropertyLookup[properties.Length + fastAccessIndex];
            if (includeType)
            {
                fastLookup[0] = new FastPropertyLookup("Type", TypeCode.String, (o, p) => o.GetType().ToString());
            }
            foreach (var prop in properties)
            {
                var getterMethod = prop.GetGetMethod();
                Type propertyType = getterMethod.ReturnType;
                ReflectionHelpers.LateBoundMethod valueLookup = ReflectionHelpers.CreateLateBoundMethod(getterMethod);
#if NETSTANDARD1_3
                TypeCode typeCode = propertyType == typeof(string) ? TypeCode.String : (propertyType == typeof(int) ? TypeCode.Int32 : TypeCode.Object);
#else
                TypeCode typeCode = Type.GetTypeCode(propertyType); // Skip cyclic-reference checks when not TypeCode.Object
#endif
                fastLookup[fastAccessIndex++] = new FastPropertyLookup(prop.Name, typeCode, valueLookup);
            }
            return fastLookup;
        }

        private const BindingFlags PublicProperties = BindingFlags.Instance | BindingFlags.Public;

        public struct ObjectPropertyList : IEnumerable<ObjectPropertyList.PropertyValue>
        {
            internal static readonly StringComparer NameComparer = StringComparer.Ordinal;
            private readonly object _object;
            private readonly PropertyInfo[] _properties;
            private readonly FastPropertyLookup[] _fastLookup;

            public struct PropertyValue
            {
                public readonly string Name;
                public readonly object Value;
                public TypeCode TypeCode
                {
                    get
                    {
                        if (_typecode == TypeCode.Object)
                        {
                            return Convert.GetTypeCode(Value);
                        }

                        return Value == null ? TypeCode.Empty : _typecode;
                    }
                }

                private readonly TypeCode _typecode;

                public PropertyValue(string name, object value, TypeCode typeCode)
                {
                    Name = name;
                    Value = value;
                    _typecode = typeCode;
                }

                public PropertyValue(object owner, PropertyInfo propertyInfo)
                {
                    Name = propertyInfo.Name;
                    Value = propertyInfo.GetValue(owner, null);
                    _typecode = TypeCode.Object;
                }

                public PropertyValue(object owner, FastPropertyLookup fastProperty)
                {
                    Name = fastProperty.Name;
                    Value = fastProperty.ValueLookup(owner, null);
                    _typecode = fastProperty.TypeCode;
                }
            }

            public int Count => _fastLookup?.Length ?? _properties?.Length ?? (_object as ICollection)?.Count ?? (_object as ICollection<KeyValuePair<string, object>>)?.Count ?? 0;

            internal ObjectPropertyList(object value, PropertyInfo[] properties, FastPropertyLookup[] fastLookup)
            {
                _object = value;
                _properties = properties;
                _fastLookup = fastLookup;
            }

            public ObjectPropertyList(IDictionary<string, object> value)
            {
                _object = value;    // Expando objects
                _properties = null;
                _fastLookup = null;
            }

            public bool TryGetPropertyValue(string name, out PropertyValue propertyValue)
            {
                if (_fastLookup != null)
                {
                    int nameHashCode = NameComparer.GetHashCode(name);
                    foreach (var fastProperty in _fastLookup)
                    {
                        if (fastProperty.NameHashCode==nameHashCode && NameComparer.Equals(fastProperty.Name, name))
                        {
                            propertyValue = new PropertyValue(_object, fastProperty);
                            return true;
                        }
                    }
                }
                else if (_properties != null)
                {
                    foreach (var propInfo in _properties)
                    {
                        if (NameComparer.Equals(propInfo.Name, name))
                        {
                            propertyValue = new PropertyValue(_object, propInfo);
                            return true;
                        }
                    }
                }
                else if (_object is IDictionary<string, object> expandoObject && expandoObject.TryGetValue(name, out var objectValue))
                {
                    propertyValue = new PropertyValue(name, objectValue, TypeCode.Object);
                    return true;
                }
                propertyValue = default(PropertyValue);
                return false;
            }

            public override string ToString()
            {
                return _object?.ToString() ?? "null";
            }

            public Enumerator GetEnumerator()
            {
                if (_properties != null)
                    return new Enumerator(_object, _properties, _fastLookup);
                else
                    return new Enumerator((_object as IDictionary<string, object>).GetEnumerator());
            }

            IEnumerator<PropertyValue> IEnumerable<PropertyValue>.GetEnumerator() => GetEnumerator();
            IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

            public struct Enumerator : IEnumerator<PropertyValue>
            {
                private readonly object _owner;
                private readonly PropertyInfo[] _properties;
                private readonly FastPropertyLookup[] _fastLookup;
                private readonly IEnumerator<KeyValuePair<string, object>> _enumerator;
                private int _index;

                internal Enumerator(object owner, PropertyInfo[] properties, FastPropertyLookup[] fastLookup)
                {
                    _owner = owner;
                    _properties = properties;
                    _fastLookup = fastLookup;
                    _index = -1;
                    _enumerator = null;
                }

                internal Enumerator(IEnumerator<KeyValuePair<string, object>> enumerator)
                {
                    _owner = enumerator;
                    _properties = null;
                    _fastLookup = null;
                    _index = 0;
                    _enumerator = enumerator;
                }

                public PropertyValue Current
                {
                    get
                    {
                        try
                        {
                            if (_fastLookup != null)
                                return new PropertyValue(_owner, _fastLookup[_index]);
                            else if (_properties != null)
                                return new PropertyValue(_owner, _properties[_index]);
                            else
                                return new PropertyValue(_enumerator.Current.Key, _enumerator.Current.Value, TypeCode.Object);
                        }
                        catch (Exception ex)
                        {
                            InternalLogger.Warn(ex, "Failed to get property value for object: {0}", _owner);
                            return default(PropertyValue);
                        }
                    }
                }

                object IEnumerator.Current => Current;

                public void Dispose()
                {
                    _enumerator?.Dispose();
                }

                public bool MoveNext()
                {
                    if (_properties != null)
                        return ++_index < (_fastLookup?.Length ?? _properties.Length);
                    else
                        return _enumerator.MoveNext();
                }

                public void Reset()
                {
                    if (_properties != null)
                        _index = -1;
                    else
                        _enumerator.Reset();
                }
            }
        }

        internal struct FastPropertyLookup
        {
            public readonly string Name;
            public readonly ReflectionHelpers.LateBoundMethod ValueLookup;
            public readonly TypeCode TypeCode;
            public readonly int NameHashCode;

            public FastPropertyLookup(string name, TypeCode typeCode, ReflectionHelpers.LateBoundMethod valueLookup)
            {
                Name = name;
                ValueLookup = valueLookup;
                TypeCode = typeCode;
                NameHashCode = ObjectPropertyList.NameComparer.GetHashCode(name);
            }
        }

        private struct ObjectPropertyInfos : IEquatable<ObjectPropertyInfos>
        {
            public readonly PropertyInfo[] Properties;
            public readonly FastPropertyLookup[] FastLookup;

            public static readonly ObjectPropertyInfos SimpleToString = new ObjectPropertyInfos(ArrayHelper.Empty<PropertyInfo>(), ArrayHelper.Empty<FastPropertyLookup>());

            public ObjectPropertyInfos(PropertyInfo[] properties, FastPropertyLookup[] fastLookup)
            {
                Properties = properties;
                FastLookup = fastLookup;
            }

            public bool HasFastLookup => FastLookup != null;

            public bool Equals(ObjectPropertyInfos other)
            {
                return ReferenceEquals(Properties, other.Properties) && FastLookup?.Length == other.FastLookup?.Length;
            }
        }

#if DYNAMIC_OBJECT
        private static Dictionary<string, object> DynamicObjectToDict(DynamicObject d)
        {
            var newVal = new Dictionary<string, object>();
            foreach (var propName in d.GetDynamicMemberNames())
            {
                if (d.TryGetMember(new GetBinderAdapter(propName), out var result))
                {
                    newVal[propName] = result;
                }
            }

            return newVal;
        }

        /// <summary>
        /// Binder for retrieving value of <see cref="DynamicObject"/>
        /// </summary>
        private sealed class GetBinderAdapter : GetMemberBinder
        {
            internal GetBinderAdapter(string name)
                : base(name, false)
            {
            }

            /// <inheritdoc />
            public override DynamicMetaObject FallbackGetMember(DynamicMetaObject target, DynamicMetaObject errorSuggestion)
            {
                return target;
            }
        }
#endif
    }
}
