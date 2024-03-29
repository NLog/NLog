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

namespace NLog.Internal
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Diagnostics;
    using NLog.MessageTemplates;

    /// <summary>
    /// Dictionary that combines the standard <see cref="LogEventInfo.Properties" /> with the
    /// MessageTemplate-properties extracted from the <see cref="LogEventInfo.Message" />.
    /// 
    /// The <see cref="MessageProperties" /> are returned as the first items
    /// in the collection, and in positional order.
    /// </summary>
    [DebuggerDisplay("Count = {Count}")]
    internal sealed class PropertiesDictionary : IDictionary<object, object>, IEnumerable<MessageTemplateParameter>
    {
        private struct PropertyValue
        {
            /// <summary>
            /// Value of the property 
            /// </summary>
            public readonly object Value;

            /// <summary>
            /// Has property been captured from message-template ?
            /// </summary>
            public readonly bool IsMessageProperty;

            public PropertyValue(object value, bool isMessageProperty)
            {
                Value = value;
                IsMessageProperty = isMessageProperty;
            }
        }

        /// <summary>
        /// The properties of the logEvent
        /// </summary>
        private Dictionary<object, PropertyValue> _eventProperties;
        /// <summary>
        /// The properties extracted from the message-template
        /// </summary>
        private IList<MessageTemplateParameter> _messageProperties;
        private DictionaryCollection _keyCollection;
        private DictionaryCollection _valueCollection;

        /// <summary>
        /// Wraps the list of message-template-parameters as IDictionary-interface
        /// </summary>
        /// <param name="messageParameters">Message-template-parameters</param>
        public PropertiesDictionary(IList<MessageTemplateParameter> messageParameters = null)
        {
            if (messageParameters?.Count > 0)
            {
                MessageProperties = messageParameters;
            }
        }

#if !NET35
        /// <summary>
        /// Transforms the list of event-properties into IDictionary-interface
        /// </summary>
        /// <param name="eventProperties">Message-template-parameters</param>
        public PropertiesDictionary(IReadOnlyList<KeyValuePair<object, object>> eventProperties)
        {
            var propertyCount = eventProperties.Count;
            if (propertyCount > 0)
            {
                _eventProperties = new Dictionary<object, PropertyValue>(propertyCount, PropertyKeyComparer.Default);
                for (int i = 0; i < propertyCount; ++i)
                {
                    var property = eventProperties[i];
                    _eventProperties[property.Key] = new PropertyValue(property.Value, false);
                }
            }
        }
#endif

        private bool IsEmpty => (_eventProperties is null || _eventProperties.Count == 0) && (_messageProperties is null || _messageProperties.Count == 0);

        private Dictionary<object, PropertyValue> EventProperties
        {
            get
            {
                if (_eventProperties is null)
                {
                    System.Threading.Interlocked.CompareExchange(ref _eventProperties, BuildEventProperties(_messageProperties), null);
                }
                return _eventProperties;
            }
        }

        public IList<MessageTemplateParameter> MessageProperties
        {
            get => _messageProperties ?? ArrayHelper.Empty<MessageTemplateParameter>();
            internal set => _messageProperties = SetMessageProperties(value, _messageProperties);
        }

        private IList<MessageTemplateParameter> SetMessageProperties(IList<MessageTemplateParameter> newMessageProperties, IList<MessageTemplateParameter> oldMessageProperties)
        {
            if (_eventProperties is null && VerifyUniqueMessageTemplateParametersFast(newMessageProperties))
            {
                return newMessageProperties;
            }
            else
            {
                var eventProperties = _eventProperties;
                if (eventProperties is null)
                {
                    eventProperties = _eventProperties = new Dictionary<object, PropertyValue>(newMessageProperties?.Count ?? 0, PropertyKeyComparer.Default);
                }

                if (oldMessageProperties != null && eventProperties.Count > 0)
                {
                    RemoveOldMessageProperties(oldMessageProperties, eventProperties);
                }

                if (newMessageProperties != null)
                {
                    InsertMessagePropertiesIntoEmptyDictionary(newMessageProperties, eventProperties);
                }

                return newMessageProperties;
            }
        }

        private static void RemoveOldMessageProperties(IList<MessageTemplateParameter> oldMessageProperties, Dictionary<object, PropertyValue> eventProperties)
        {
            for (int i = 0; i < oldMessageProperties.Count; ++i)
            {
                if (eventProperties.TryGetValue(oldMessageProperties[i].Name, out var propertyValue) && propertyValue.IsMessageProperty)
                {
                    eventProperties.Remove(oldMessageProperties[i].Name);
                }
            }
        }

        private static Dictionary<object, PropertyValue> BuildEventProperties(IList<MessageTemplateParameter> messageProperties)
        {
            if (messageProperties?.Count > 0)
            {
                var eventProperties = new Dictionary<object, PropertyValue>(messageProperties.Count, PropertyKeyComparer.Default);
                InsertMessagePropertiesIntoEmptyDictionary(messageProperties, eventProperties);
                return eventProperties;
            }
            else
            {
                return new Dictionary<object, PropertyValue>(PropertyKeyComparer.Default);
            }
        }

        /// <inheritDoc/>
        public object this[object key]
        {
            get
            {
                if (TryGetValue(key, out var valueItem))
                {
                    return valueItem;
                }

                throw new KeyNotFoundException();
            }
            set => EventProperties[key] = new PropertyValue(value, false);
        }

        /// <inheritDoc/>
        public ICollection<object> Keys => KeyCollection;

        /// <inheritDoc/>
        public ICollection<object> Values => ValueCollection;

        private DictionaryCollection KeyCollection
        {
            get
            {
                if (_keyCollection != null)
                    return _keyCollection;
                if (IsEmpty)
                    return EmptyKeyCollection;
                return _keyCollection ?? (_keyCollection = new DictionaryCollection(this, true));
            }
        }

        private DictionaryCollection ValueCollection
        {
            get
            {
                if (_valueCollection != null)
                    return _valueCollection;

                if (IsEmpty)
                    return EmptyValueCollection;
                return _valueCollection ?? (_valueCollection = new DictionaryCollection(this, false));
            }
        }

        private static readonly DictionaryCollection EmptyKeyCollection = new DictionaryCollection(new PropertiesDictionary(), true);
        private static readonly DictionaryCollection EmptyValueCollection = new DictionaryCollection(new PropertiesDictionary(), false);

        /// <inheritDoc/>
        public int Count => (_eventProperties?.Count) ?? (_messageProperties?.Count) ?? 0;

        /// <inheritDoc/>
        public bool IsReadOnly => false;

        /// <inheritDoc/>
        public void Add(object key, object value)
        {
            EventProperties.Add(key, new PropertyValue(value, false));
        }

        /// <inheritDoc/>
        public void Add(KeyValuePair<object, object> item)
        {
            Add(item.Key, item.Value);
        }

        /// <inheritDoc/>
        public void Clear()
        {
            if (_eventProperties != null)
                _eventProperties = null;
            if (_messageProperties != null)
                _messageProperties = ArrayHelper.Empty<MessageTemplateParameter>();
        }

        /// <inheritDoc/>
        public bool Contains(KeyValuePair<object, object> item)
        {
            if (!IsEmpty)
            {
                if (((IDictionary<object, PropertyValue>)EventProperties).Contains(new KeyValuePair<object, PropertyValue>(item.Key, new PropertyValue(item.Value, false))))
                    return true;

                if (((IDictionary<object, PropertyValue>)EventProperties).Contains(new KeyValuePair<object, PropertyValue>(item.Key, new PropertyValue(item.Value, true))))
                    return true;
            }
            return false;
        }

        /// <inheritDoc/>
        public bool ContainsKey(object key)
        {
            return TryGetValue(key, out var _);
        }

        /// <inheritDoc/>
        public void CopyTo(KeyValuePair<object, object>[] array, int arrayIndex)
        {
            Guard.ThrowIfNull(array);
            if (arrayIndex < 0)
                throw new ArgumentOutOfRangeException(nameof(arrayIndex));

            if (!IsEmpty)
            {
                foreach (var propertyItem in this)
                {
                    array[arrayIndex++] = propertyItem;
                }
            }
        }

        /// <inheritDoc/>
        public IEnumerator<KeyValuePair<object, object>> GetEnumerator()
        {
            if (IsEmpty)
                return System.Linq.Enumerable.Empty<KeyValuePair<object, object>>().GetEnumerator();
            return new DictionaryEnumerator(this);
        }

        /// <inheritDoc/>
        IEnumerator IEnumerable.GetEnumerator()
        {
            if (IsEmpty)
                return ArrayHelper.Empty<KeyValuePair<object, object>>().GetEnumerator();
            return new DictionaryEnumerator(this);
        }

        /// <inheritDoc/>
        public bool Remove(object key)
        {
            if (!IsEmpty)
            {
                return EventProperties.Remove(key);
            }
            return false;
        }

        /// <inheritDoc/>
        public bool Remove(KeyValuePair<object, object> item)
        {
            if (!IsEmpty)
            {
                if (((IDictionary<object, PropertyValue>)EventProperties).Remove(new KeyValuePair<object, PropertyValue>(item.Key, new PropertyValue(item.Value, false))))
                    return true;

                if (((IDictionary<object, PropertyValue>)EventProperties).Remove(new KeyValuePair<object, PropertyValue>(item.Key, new PropertyValue(item.Value, true))))
                    return true;
            }

            return false;
        }

        /// <inheritDoc/>
        public bool TryGetValue(object key, out object value)
        {
            if (!IsEmpty)
            {
                if (_eventProperties is null && _messageProperties?.Count < 5)
                {
                    return TryLookupMessagePropertyValue(key, out value);
                }
                else if (EventProperties.TryGetValue(key, out var valueItem))
                {
                    value = valueItem.Value;
                    return true;
                }
            }

            value = null;
            return false;
        }

        private bool TryLookupMessagePropertyValue(object key, out object propertyValue)
        {
            if (key is string keyString)
            {
                for (int i = 0; i < _messageProperties.Count; ++i)
                {
                    if (keyString.Equals(_messageProperties[i].Name, StringComparison.Ordinal))
                    {
                        propertyValue = _messageProperties[i].Value;
                        return true;
                    }
                }
            }
            else if (key is IgnoreCasePropertyKey keyIgnoreCase)
            {
                for (int i = 0; i < _messageProperties.Count; ++i)
                {
                    if (keyIgnoreCase.Equals(_messageProperties[i].Name))
                    {
                        propertyValue = _messageProperties[i].Value;
                        return true;
                    }
                }
            }

            propertyValue = null;
            return false;
        }

        /// <summary>
        /// Check if the message-template-parameters can be used directly without allocating a dictionary
        /// </summary>
        /// <param name="parameterList">Message-template-parameters</param>
        /// <returns>Are all parameter names unique (true / false)</returns>
        private static bool VerifyUniqueMessageTemplateParametersFast(IList<MessageTemplateParameter> parameterList)
        {
            if (parameterList is null)
                return true;
            
            var parameterCount = parameterList.Count;
            if (parameterCount <= 1)
                return true;

            if (parameterCount > 10)
                return false;

            for (int i = 0; i < parameterCount - 1; ++i)
            {
                for (int j = i + 1; j < parameterCount; ++j)
                {
                    if (parameterList[i].Name == parameterList[j].Name)
                        return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Attempt to insert the message-template-parameters into an empty dictionary
        /// </summary>
        /// <param name="messageProperties">Message-template-parameters</param>
        /// <param name="eventProperties">The dictionary that initially contains no message-template-parameters</param>
        private static void InsertMessagePropertiesIntoEmptyDictionary(IList<MessageTemplateParameter> messageProperties, Dictionary<object, PropertyValue> eventProperties)
        {
            for (int i = 0; i < messageProperties.Count; ++i)
            {
                try
                {
                    eventProperties.Add(messageProperties[i].Name, new PropertyValue(messageProperties[i].Value, true));
                }
                catch (ArgumentException)
                {
                    var duplicateProperty = messageProperties[i];
                    if (eventProperties.TryGetValue(duplicateProperty.Name, out var propertyValue) && propertyValue.IsMessageProperty)
                    {
                        var uniqueName = GenerateUniquePropertyName(duplicateProperty.Name, eventProperties, (newkey, props) => props.ContainsKey(newkey));
                        eventProperties.Add(uniqueName, new PropertyValue(messageProperties[i].Value, true));
                        messageProperties[i] = new MessageTemplateParameter(uniqueName, duplicateProperty.Value, duplicateProperty.Format, duplicateProperty.CaptureType);
                    }
                }
            }
        }

        internal static string GenerateUniquePropertyName<TKey, TValue>(string originalName, IDictionary<TKey, TValue> properties, Func<string, IDictionary<TKey, TValue>, bool> containsKey)
        {
            originalName = originalName ?? string.Empty;

            int newNameIndex = 1;
            var newItemName = string.Concat(originalName, "_1");
            while (containsKey(newItemName, properties))
            {
                newItemName = string.Concat(originalName, "_", (++newNameIndex).ToString());
            }

            return newItemName;
        }

        IEnumerator<MessageTemplateParameter> IEnumerable<MessageTemplateParameter>.GetEnumerator()
        {
            return new ParameterEnumerator(this);
        }

        private abstract class DictionaryEnumeratorBase : IDisposable
        {
            private readonly PropertiesDictionary _dictionary;
            private int? _messagePropertiesEnumerator;
            private bool _eventEnumeratorCreated;
            private Dictionary<object, PropertyValue>.Enumerator _eventEnumerator;

            protected DictionaryEnumeratorBase(PropertiesDictionary dictionary)
            {
                _dictionary = dictionary;
            }

            protected KeyValuePair<object, object> CurrentProperty
            {
                get
                {
                    if (_messagePropertiesEnumerator.HasValue)
                    {
                        var property = _dictionary._messageProperties[_messagePropertiesEnumerator.Value];
                        return new KeyValuePair<object, object>(property.Name, property.Value);
                    }
                    if (_eventEnumeratorCreated)
                        return new KeyValuePair<object, object>(_eventEnumerator.Current.Key, _eventEnumerator.Current.Value.Value);
                    throw new InvalidOperationException();
                }
            }

            protected MessageTemplateParameter CurrentParameter
            {
                get
                {
                    if (_messagePropertiesEnumerator.HasValue)
                    {
                        return _dictionary._messageProperties[_messagePropertiesEnumerator.Value];
                    }
                    if (_eventEnumeratorCreated)
                    {
                        string parameterName = XmlHelper.XmlConvertToString(_eventEnumerator.Current.Key ?? string.Empty) ?? string.Empty;
                        return new MessageTemplateParameter(parameterName, _eventEnumerator.Current.Value.Value, null, CaptureType.Unknown);
                    }
                    throw new InvalidOperationException();
                }
            }

            public bool MoveNext()
            {
                if (_messagePropertiesEnumerator.HasValue)
                {
                    if (_messagePropertiesEnumerator.Value + 1 < _dictionary._messageProperties.Count)
                    {
                        // Move forward to a key that is not overridden
                        _messagePropertiesEnumerator = FindNextValidMessagePropertyIndex(_messagePropertiesEnumerator.Value + 1);
                        if (_messagePropertiesEnumerator.HasValue)
                            return true;

                        _messagePropertiesEnumerator = _dictionary._eventProperties.Count - 1;
                    }

                    if (HasEventProperties(_dictionary))
                    {
                        _messagePropertiesEnumerator = null;
                        _eventEnumerator = _dictionary._eventProperties.GetEnumerator();
                        _eventEnumeratorCreated = true;
                        return MoveNextValidEventProperty();
                    }

                    return false;
                }
                
                if (_eventEnumeratorCreated)
                {
                    return MoveNextValidEventProperty();
                }

                if (HasMessageProperties(_dictionary))
                {
                    // Move forward to a key that is not overridden
                    _messagePropertiesEnumerator = FindNextValidMessagePropertyIndex(0);
                    if (_messagePropertiesEnumerator.HasValue)
                    {
                        return true;
                    }
                }

                if (HasEventProperties(_dictionary))
                {
                    _eventEnumerator = _dictionary._eventProperties.GetEnumerator();
                    _eventEnumeratorCreated = true;
                    return MoveNextValidEventProperty();
                }

                return false;
            }

            private static bool HasMessageProperties(PropertiesDictionary propertiesDictionary)
            {
                return propertiesDictionary._messageProperties?.Count > 0;
            }

            private static bool HasEventProperties(PropertiesDictionary propertiesDictionary)
            {
                return propertiesDictionary._eventProperties?.Count > 0;
            }

            private bool MoveNextValidEventProperty()
            {
                while (_eventEnumerator.MoveNext())
                {
                    if (!_eventEnumerator.Current.Value.IsMessageProperty)
                        return true;
                }
                return false;
            }

            private int? FindNextValidMessagePropertyIndex(int startIndex)
            {
                var eventProperties = _dictionary._eventProperties;
                if (eventProperties is null)
                    return startIndex;

                for (int i = startIndex; i < _dictionary._messageProperties.Count; ++i)
                {
                    if (eventProperties.TryGetValue(_dictionary._messageProperties[i].Name, out var valueItem) && valueItem.IsMessageProperty)
                    {
                        return i;
                    }
                }

                return null;
            }

            public void Dispose()
            {
                // Nothing to do
            }

            public void Reset()
            {
                _messagePropertiesEnumerator = null;
                _eventEnumeratorCreated = false;
                _eventEnumerator = default(Dictionary<object, PropertyValue>.Enumerator);
            }
        }

        private sealed class ParameterEnumerator : DictionaryEnumeratorBase, IEnumerator<MessageTemplateParameter>
        {
            /// <inheritDoc/>
            public MessageTemplateParameter Current => CurrentParameter;

            /// <inheritDoc/>
            object IEnumerator.Current => CurrentParameter;

            public ParameterEnumerator(PropertiesDictionary dictionary)
                : base(dictionary)
            {
            }
        }

        private sealed class DictionaryEnumerator : DictionaryEnumeratorBase, IEnumerator<KeyValuePair<object, object>>
        {
            /// <inheritDoc/>
            public KeyValuePair<object, object> Current => CurrentProperty;

            /// <inheritDoc/>
            object IEnumerator.Current => CurrentProperty;

            public DictionaryEnumerator(PropertiesDictionary dictionary)
                : base(dictionary)
            {
            }
        }

        [DebuggerDisplay("Count = {Count}")]
        private sealed class DictionaryCollection : ICollection<object>
        {
            private readonly PropertiesDictionary _dictionary;
            private readonly bool _keyCollection;

            public DictionaryCollection(PropertiesDictionary dictionary, bool keyCollection)
            {
                _dictionary = dictionary;
                _keyCollection = keyCollection;
            }

            /// <inheritDoc/>
            public int Count => _dictionary.Count;

            /// <inheritDoc/>
            public bool IsReadOnly => true;


            /// <summary>Will always throw, as collection is readonly</summary>
            public void Add(object item) { throw new NotSupportedException(); }

            /// <summary>Will always throw, as collection is readonly</summary>
            public void Clear() { throw new NotSupportedException(); }

            /// <summary>Will always throw, as collection is readonly</summary>
            public bool Remove(object item) { throw new NotSupportedException(); }

            /// <inheritDoc/>
            public bool Contains(object item)
            {
                if (_keyCollection)
                {
                    return _dictionary.ContainsKey(item);
                }
                if (!_dictionary.IsEmpty)
                {
                    if (_dictionary.EventProperties.ContainsValue(new PropertyValue(item, false)))
                        return true;

                    if (_dictionary.EventProperties.ContainsValue(new PropertyValue(item, true)))
                        return true;
                }
                return false;
            }

            /// <inheritDoc/>
            public void CopyTo(object[] array, int arrayIndex)
            {
                Guard.ThrowIfNull(array);
                if (arrayIndex < 0)
                    throw new ArgumentOutOfRangeException(nameof(arrayIndex));

                if (!_dictionary.IsEmpty)
                {
                    foreach (var propertyItem in _dictionary)
                    {
                        array[arrayIndex++] = _keyCollection ? propertyItem.Key : propertyItem.Value;
                    }
                }
            }

            /// <inheritDoc/>
            public IEnumerator<object> GetEnumerator()
            {
                return new DictionaryCollectionEnumerator(_dictionary, _keyCollection);
            }

            /// <inheritDoc/>
            IEnumerator IEnumerable.GetEnumerator()
            {
                return GetEnumerator();
            }

            private sealed class DictionaryCollectionEnumerator : DictionaryEnumeratorBase, IEnumerator<object>
            {
                private readonly bool _keyCollection;

                public DictionaryCollectionEnumerator(PropertiesDictionary dictionary, bool keyCollection)
                    : base(dictionary)
                {
                    _keyCollection = keyCollection;
                }

                /// <inheritDoc/>
                public object Current => _keyCollection ? CurrentProperty.Key : CurrentProperty.Value;
            }
        }

        /// <summary>
        /// Special property-key for lookup without being case-sensitive
        /// </summary>
        internal sealed class IgnoreCasePropertyKey
        {
            private readonly string _propertyName;

            public IgnoreCasePropertyKey(string propertyName)
            {
                _propertyName = propertyName;
            }

            public bool Equals(string propertyName) => Equals(_propertyName, propertyName);

            public override bool Equals(object obj)
            {
                if (obj is string stringObj)
                    return Equals(_propertyName, stringObj);
                else if (obj is IgnoreCasePropertyKey ignoreCase)
                    return Equals(_propertyName, ignoreCase._propertyName);
                else
                    return false;
            }

            public override int GetHashCode()
            {
                return GetHashCode(_propertyName);
            }

            public override string ToString() => _propertyName;

            internal static int GetHashCode(string propertyName)
            {
                return StringComparer.OrdinalIgnoreCase.GetHashCode(propertyName);
            }

            internal static bool Equals(string x, string y)
            {
                return string.Equals(x, y, StringComparison.OrdinalIgnoreCase);
            }
        }

        /// <summary>
        /// Property-Key equality-comparer that uses string-hashcode from OrdinalIgnoreCase
        /// Enables case-insensitive lookup using <see cref="IgnoreCasePropertyKey"/>
        /// </summary>
        private sealed class PropertyKeyComparer : IEqualityComparer<object>
        {
            public static readonly PropertyKeyComparer Default = new PropertyKeyComparer();

            public new bool Equals(object x, object y)
            {
                if (y is IgnoreCasePropertyKey ynocase && x is string xstring)
                {
                    return ynocase.Equals(xstring);
                }
                else if (x is IgnoreCasePropertyKey xnocase && y is string ystring)
                {
                    return xnocase.Equals(ystring);
                }
                else
                {
                    return EqualityComparer<object>.Default.Equals(x, y);
                }
            }

            public int GetHashCode(object obj)
            {
                if (obj is string objstring)
                    return IgnoreCasePropertyKey.GetHashCode(objstring);
                else
                    return EqualityComparer<object>.Default.GetHashCode(obj);
            }
        }
    }
}
