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
    internal sealed class PropertiesDictionary : IDictionary<object, object?>
    {
        private
#if !NETFRAMEWORK
        readonly
#endif
        struct PropertyValue
        {
            /// <summary>
            /// Value of the property
            /// </summary>
            public readonly object? Value;

            /// <summary>
            /// Has property been captured from message-template ?
            /// </summary>
            public readonly bool IsMessageProperty;

            public PropertyValue(object? value, bool isMessageProperty)
            {
                Value = value;
                IsMessageProperty = isMessageProperty;
            }
        }

        /// <summary>
        /// The properties of the logEvent
        /// </summary>
        private Dictionary<object, PropertyValue>? _eventProperties;
        /// <summary>
        /// The properties extracted from the message-template
        /// </summary>
        private IList<MessageTemplateParameter>? _messageProperties;

        /// <summary>
        /// Wraps the list of message-template-parameters as IDictionary-interface
        /// </summary>
        /// <param name="messageParameters">Message-template-parameters</param>
        public PropertiesDictionary(IList<MessageTemplateParameter>? messageParameters = null)
        {
            if (messageParameters?.Count > 0)
            {
                _messageProperties = SetMessageProperties(messageParameters, null);
            }
        }

        /// <summary>
        /// Transforms the list of event-properties into IDictionary-interface
        /// </summary>
        public PropertiesDictionary(int initialCapacity)
        {
            if (initialCapacity > 3)
            {
                _eventProperties = new Dictionary<object, PropertyValue>(initialCapacity, PropertyKeyComparer.Default);
            }
        }

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
        }

        public void ResetMessageProperties(IList<MessageTemplateParameter>? newMessageProperties = null)
        {
            _messageProperties = SetMessageProperties(newMessageProperties, _messageProperties);
        }

        private IList<MessageTemplateParameter>? SetMessageProperties(IList<MessageTemplateParameter>? newMessageProperties, IList<MessageTemplateParameter>? oldMessageProperties)
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

        private static Dictionary<object, PropertyValue> BuildEventProperties(IList<MessageTemplateParameter>? messageProperties)
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
        public object? this[object key]
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
        public ICollection<object> Keys => IsEmpty ? EmptyKeyCollection : new KeyCollection(this);

        /// <inheritDoc/>
        public ICollection<object?> Values => IsEmpty ? EmptyValueCollection : new ValueCollection(this);


        private static readonly KeyCollection EmptyKeyCollection = new KeyCollection(new PropertiesDictionary());
        private static readonly ValueCollection EmptyValueCollection = new ValueCollection(new PropertiesDictionary());

        /// <inheritDoc/>
        public int Count => (_eventProperties?.Count) ?? (_messageProperties?.Count) ?? 0;

        /// <inheritDoc/>
        public bool IsReadOnly => false;

        /// <inheritDoc/>
        public void Add(object key, object? value)
        {
            EventProperties.Add(key, new PropertyValue(value, false));
        }

        /// <inheritDoc/>
        public void Add(KeyValuePair<object, object?> item)
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
        public bool Contains(KeyValuePair<object, object?> item)
        {
            if (!IsEmpty && (_eventProperties != null || ContainsKey(item.Key)))
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
        public void CopyTo(KeyValuePair<object, object?>[] array, int arrayIndex)
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

        internal PropertyDictionaryEnumerator GetPropertyEnumerator()
        {
            return new PropertyDictionaryEnumerator(this);
        }

        /// <inheritDoc/>
        public IEnumerator<KeyValuePair<object, object?>> GetEnumerator()
        {
            return IsEmpty ? System.Linq.Enumerable.Empty<KeyValuePair<object, object?>>().GetEnumerator() : new PropertyDictionaryEnumerator(this);
        }

        /// <inheritDoc/>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        /// <inheritDoc/>
        public bool Remove(object key)
        {
            if (!IsEmpty && (_eventProperties != null || ContainsKey(key)))
            {
                return EventProperties.Remove(key);
            }
            return false;
        }

        /// <inheritDoc/>
        public bool Remove(KeyValuePair<object, object?> item)
        {
            if (!IsEmpty && (_eventProperties != null || ContainsKey(item.Key)))
            {
                if (((ICollection<KeyValuePair<object, PropertyValue>>)EventProperties).Remove(new KeyValuePair<object, PropertyValue>(item.Key, new PropertyValue(item.Value, false))))
                    return true;

                if (((ICollection<KeyValuePair<object, PropertyValue>>)EventProperties).Remove(new KeyValuePair<object, PropertyValue>(item.Key, new PropertyValue(item.Value, true))))
                    return true;
            }

            return false;
        }

        /// <inheritDoc/>
        public bool TryGetValue(object key, out object? value)
        {
            if (!IsEmpty)
            {
                if (_eventProperties is null)
                {
                    return TryLookupMessagePropertyValue(key, out value);
                }

                if (EventProperties.TryGetValue(key, out var eventProperty))
                {
                    value = eventProperty.Value;
                    return true;
                }
            }

            value = null;
            return false;
        }

        private bool TryLookupMessagePropertyValue(object key, out object? propertyValue)
        {
            if (_messageProperties is null || _messageProperties.Count == 0)
            {
                propertyValue = null;
                return false;
            }

            if (_messageProperties.Count > 10)
            {
                if (EventProperties.TryGetValue(key, out var eventProperty))
                {
                    propertyValue = eventProperty.Value;
                    return true;
                }
            }
            else if (key is string keyString)
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
        private static bool VerifyUniqueMessageTemplateParametersFast(IList<MessageTemplateParameter>? parameterList)
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
                var currentName = parameterList[i].Name;
                for (int j = i + 1; j < parameterCount; ++j)
                {
                    if (currentName == parameterList[j].Name)
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

        public struct PropertyDictionaryEnumerator : IEnumerator<KeyValuePair<object, object?>>
        {
            private readonly PropertiesDictionary _dictionary;
            private Dictionary<object, PropertyValue>.Enumerator _eventEnumerator;
            private int? _messagePropertiesIndex;

            public PropertyDictionaryEnumerator(PropertiesDictionary dictionary)
            {
                _dictionary = dictionary;
                _eventEnumerator = dictionary._eventProperties?.GetEnumerator() ?? default(Dictionary<object, PropertyValue>.Enumerator);
                _messagePropertiesIndex = dictionary._messageProperties?.Count > 0 ? -1 : default(int?);
            }

            public KeyValuePair<object, object?> Current
            {
                get
                {
                    if (_messagePropertiesIndex.HasValue)
                    {
                        var property = _dictionary.MessageProperties[_messagePropertiesIndex.Value];
                        return new KeyValuePair<object, object?>(property.Name, property.Value);
                    }
                    if (_dictionary._eventProperties != null)
                    {
                        return new KeyValuePair<object, object?>(_eventEnumerator.Current.Key, _eventEnumerator.Current.Value.Value);
                    }
                    throw new InvalidOperationException();
                }
            }

            public MessageTemplateParameter CurrentParameter
            {
                get
                {
                    if (_messagePropertiesIndex.HasValue)
                    {
                        return _dictionary.MessageProperties[_messagePropertiesIndex.Value];
                    }
                    if (_dictionary._eventProperties != null)
                    {
                        string parameterName = XmlHelper.XmlConvertToString(_eventEnumerator.Current.Key ?? string.Empty) ?? string.Empty;
                        return new MessageTemplateParameter(parameterName, _eventEnumerator.Current.Value.Value, null, CaptureType.Unknown);
                    }
                    throw new InvalidOperationException();
                }
            }

            public KeyValuePair<string, object?> CurrentProperty
            {
                get
                {
                    if (_messagePropertiesIndex.HasValue)
                    {
                        var property = _dictionary.MessageProperties[_messagePropertiesIndex.Value];
                        return new KeyValuePair<string, object?>(property.Name, property.Value);
                    }
                    if (_dictionary._eventProperties != null)
                    {
                        string propertyName = XmlHelper.XmlConvertToString(_eventEnumerator.Current.Key ?? string.Empty) ?? string.Empty;
                        return new KeyValuePair<string, object?>(propertyName, _eventEnumerator.Current.Value.Value);
                    }
                    throw new InvalidOperationException();
                }
            }

            object IEnumerator.Current => Current;

            public bool MoveNext()
            {
                if (_messagePropertiesIndex.HasValue && MoveNextValidMessageParameter())
                {
                    return true;
                }

                if (_dictionary._eventProperties != null)
                {
                    return MoveNextValidEventProperty();
                }

                return false;
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

            private bool MoveNextValidMessageParameter()
            {
                var messageProperties = _dictionary.MessageProperties;
                if (_messagePropertiesIndex.HasValue && messageProperties != null && _messagePropertiesIndex.Value + 1 < messageProperties.Count)
                {
                    var eventProperties = _dictionary._eventProperties;
                    if (eventProperties is null)
                    {
                        _messagePropertiesIndex = _messagePropertiesIndex.Value + 1;
                        return true;
                    }

                    for (int i = _messagePropertiesIndex.Value + 1; i < messageProperties.Count; ++i)
                    {
                        if (eventProperties.TryGetValue(messageProperties[i].Name, out var valueItem) && valueItem.IsMessageProperty)
                        {
                            _messagePropertiesIndex = i;
                            return true;
                        }
                    }
                }

                _messagePropertiesIndex = null;
                return false;
            }

            public void Dispose()
            {
                // Nothing to do
            }

            public void Reset()
            {
                _messagePropertiesIndex = _dictionary._messageProperties?.Count > 0 ? -1 : default(int?);
                _eventEnumerator = default(Dictionary<object, PropertyValue>.Enumerator);
            }
        }

        [DebuggerDisplay("Count = {Count}")]
        private sealed class ValueCollection : ICollection<object?>
        {
            private readonly PropertiesDictionary _dictionary;

            public ValueCollection(PropertiesDictionary dictionary)
            {
                _dictionary = dictionary;
            }

            /// <inheritDoc/>
            public int Count => _dictionary.Count;

            /// <inheritDoc/>
            public bool IsReadOnly => true;

            /// <summary>Will always throw, as collection is readonly</summary>
            public void Add(object? item) { throw new NotSupportedException(); }

            /// <summary>Will always throw, as collection is readonly</summary>
            public void Clear() { throw new NotSupportedException(); }

            /// <summary>Will always throw, as collection is readonly</summary>
            public bool Remove(object? item) { throw new NotSupportedException(); }

            /// <inheritDoc/>
            public bool Contains(object? item)
            {
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
            public void CopyTo(object?[] array, int arrayIndex)
            {
                Guard.ThrowIfNull(array);
                if (arrayIndex < 0)
                    throw new ArgumentOutOfRangeException(nameof(arrayIndex));

                if (!_dictionary.IsEmpty)
                {
                    foreach (var propertyItem in _dictionary)
                    {
                        array[arrayIndex++] = propertyItem.Value;
                    }
                }
            }

            /// <inheritDoc/>
            public IEnumerator<object?> GetEnumerator()
            {
                return new ValueCollectionEnumerator(_dictionary);
            }

            /// <inheritDoc/>
            IEnumerator IEnumerable.GetEnumerator()
            {
                return GetEnumerator();
            }

            private sealed class ValueCollectionEnumerator : IEnumerator<object?>
            {
                PropertyDictionaryEnumerator _enumerator;

                public ValueCollectionEnumerator(PropertiesDictionary dictionary)
                {
                    _enumerator = dictionary.GetPropertyEnumerator();
                }

                /// <inheritDoc/>
                public object? Current => _enumerator.Current.Value;
                public void Dispose() => _enumerator.Dispose();
                public bool MoveNext() => _enumerator.MoveNext();
                public void Reset() => _enumerator.Reset();
            }
        }

        [DebuggerDisplay("Count = {Count}")]
        private sealed class KeyCollection : ICollection<object>
        {
            private readonly PropertiesDictionary _dictionary;

            public KeyCollection(PropertiesDictionary dictionary)
            {
                _dictionary = dictionary;
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
                return _dictionary.ContainsKey(item);
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
                        array[arrayIndex++] = propertyItem.Key;
                    }
                }
            }

            /// <inheritDoc/>
            public IEnumerator<object> GetEnumerator()
            {
                return new KeyCollectionEnumerator(_dictionary);
            }

            /// <inheritDoc/>
            IEnumerator IEnumerable.GetEnumerator()
            {
                return GetEnumerator();
            }

            private sealed class KeyCollectionEnumerator : IEnumerator<object>
            {
                PropertyDictionaryEnumerator _enumerator;

                public KeyCollectionEnumerator(PropertiesDictionary dictionary)
                {
                    _enumerator = dictionary.GetPropertyEnumerator();
                }

                /// <inheritDoc/>
                public object Current => _enumerator.Current.Key;
                public void Dispose() => _enumerator.Dispose();
                public bool MoveNext() => _enumerator.MoveNext();
                public void Reset() => _enumerator.Reset();
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
