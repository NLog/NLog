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
    internal sealed class PropertiesDictionary : IDictionary<object, object?>, IList<MessageTemplateParameter>
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
        private MessageTemplateParameter[]? _messageProperties;
        private int _messagePropertiesCount;

        /// <summary>
        /// Wraps the list of message-template-parameters as IDictionary-interface
        /// </summary>
        /// <param name="messageParameters">Message-template-parameters</param>
        public PropertiesDictionary(MessageTemplateParameter[]? messageParameters = null)
        {
            if (messageParameters?.Length > 0)
            {
                ResetMessageProperties(messageParameters, messageParameters.Length);
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

        private bool IsEmpty => (_eventProperties is null || _eventProperties.Count == 0) && (_messageProperties is null || _messagePropertiesCount == 0);

        private Dictionary<object, PropertyValue> GetEventProperties(bool prepareForInsert = false)
        {
            if (_eventProperties is null)
            {
                System.Threading.Interlocked.CompareExchange(ref _eventProperties, InitializeEventPropertiesDictionary(prepareForInsert, _messageProperties, _messagePropertiesCount, out var resetMessageProperties), null);
                if (resetMessageProperties)
                {
                    _messageProperties = null;
                    _messagePropertiesCount = 0;
                }
            }
            return _eventProperties;
        }

        public IList<MessageTemplateParameter> MessageProperties => this;

        public void ResetMessageProperties(MessageTemplateParameter[]? newMessageProperties = null, int newMessagePropertiesCount = 0)
        {
            var eventProperties = _eventProperties;
            var oldMessageProperties = _messageProperties;
            var oldMessagePropertiesCount = _messagePropertiesCount;
            if (eventProperties != null || !VerifyUniqueMessageTemplateParametersFast(newMessageProperties) || oldMessagePropertiesCount > 0)
            {
                if (eventProperties is null)
                {
                    eventProperties = _eventProperties = oldMessagePropertiesCount == 0 ?
                        new Dictionary<object, PropertyValue>(newMessagePropertiesCount, PropertyKeyComparer.Default) :
                        InitializeEventPropertiesDictionary(false, oldMessageProperties, oldMessagePropertiesCount, out var _);
                }

                if (oldMessageProperties != null && eventProperties.Count > 0)
                {
                    RemoveOldMessageProperties(eventProperties, oldMessageProperties, oldMessagePropertiesCount);
                }

                if (newMessageProperties != null)
                {
                    InsertMessagePropertiesIntoEmptyDictionary(eventProperties, newMessageProperties, newMessagePropertiesCount, out _);
                }
            }

            _messageProperties = newMessageProperties;
            _messagePropertiesCount = newMessagePropertiesCount;
        }

        private static void RemoveOldMessageProperties(Dictionary<object, PropertyValue> eventProperties, IList<MessageTemplateParameter> oldMessageProperties, int oldMessagePropertiesCount)
        {
            for (int i = 0; i < oldMessagePropertiesCount; ++i)
            {
                if (eventProperties.TryGetValue(oldMessageProperties[i].Name, out var propertyValue) && propertyValue.IsMessageProperty)
                {
                    eventProperties.Remove(oldMessageProperties[i].Name);
                }
            }
        }

        private static Dictionary<object, PropertyValue> InitializeEventPropertiesDictionary(bool prepareForInsert, MessageTemplateParameter[]? messageProperties, int messagePropertiesCount, out bool resetMessageProperties)
        {
            if (messageProperties != null && messagePropertiesCount > 0)
            {
                var dictionaryCapacity = prepareForInsert ? (messagePropertiesCount + 2) : messagePropertiesCount;
                var eventProperties = new Dictionary<object, PropertyValue>(dictionaryCapacity, PropertyKeyComparer.Default);
                InsertMessagePropertiesIntoEmptyDictionary(eventProperties, messageProperties, messagePropertiesCount, out resetMessageProperties);
                return eventProperties;
            }
            else
            {
                resetMessageProperties = false;
                return new Dictionary<object, PropertyValue>(PropertyKeyComparer.Default);
            }
        }

        /// <inheritDoc/>
        public ICollection<object> Keys => IsEmpty ? EmptyKeyCollection : new KeyCollection(this);

        /// <inheritDoc/>
        public ICollection<object?> Values => IsEmpty ? EmptyValueCollection : new ValueCollection(this);


        private static readonly KeyCollection EmptyKeyCollection = new KeyCollection(new PropertiesDictionary());
        private static readonly ValueCollection EmptyValueCollection = new ValueCollection(new PropertiesDictionary());

        /// <inheritDoc/>
        public int Count => _eventProperties?.Count ?? _messagePropertiesCount;

        /// <inheritDoc/>
        public bool IsReadOnly => false;

        int ICollection<MessageTemplateParameter>.Count => _messagePropertiesCount;

        bool ICollection<MessageTemplateParameter>.IsReadOnly => true;

        MessageTemplateParameter IList<MessageTemplateParameter>.this[int index]
        {
            get
            {
                if (index >= _messagePropertiesCount || index < 0 || _messageProperties is null)
                    throw new ArgumentOutOfRangeException(nameof(index));
                return _messageProperties[index];
            }
            set
            {
                if (index >= _messagePropertiesCount || index < 0 || _messageProperties is null)
                    throw new ArgumentOutOfRangeException(nameof(index));
                _messageProperties[index] = value;
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
            set
            {
                if (SkipDictionaryAllocation() && key is string propertyName && propertyName.Length > 0)
                {
                    var messageProperties = _messageProperties ?? (_messageProperties = new MessageTemplateParameter[3]);
                    for (int i = 0; i < _messagePropertiesCount; ++i)
                    {
                        if (messageProperties[i].Name.Equals(propertyName))
                        {
                            messageProperties[i] = new MessageTemplateParameter(messageProperties[i].Name, value, messageProperties[i].Format, messageProperties[i].CaptureType);
                            return;
                        }
                    }
                    messageProperties[_messagePropertiesCount++] = new MessageTemplateParameter(propertyName, value, null, CaptureType.Unknown);
                    return;
                }

                GetEventProperties(true)[key] = new PropertyValue(value, false);
            }
        }

        /// <inheritDoc/>
        public void Add(object key, object? value)
        {
            if (SkipDictionaryAllocation() && key is string propertyName && propertyName.Length > 0)
            {
                var messageProperties = _messageProperties ?? (_messageProperties = new MessageTemplateParameter[3]);
                for (int i = 0; i < _messagePropertiesCount; ++i)
                {
                    if (messageProperties[i].Name.Equals(propertyName))
                    {
                        throw new ArgumentException($"An item with the same key {propertyName} has already been added.", nameof(key));
                    }
                }
                messageProperties[_messagePropertiesCount++] = new MessageTemplateParameter(propertyName, value, null, CaptureType.Unknown);
                return;
            }

            GetEventProperties(true)[key] = new PropertyValue(value, false);
        }

        private bool SkipDictionaryAllocation()
        {
            return _eventProperties is null && (_messageProperties is null || _messagePropertiesCount < _messageProperties.Length);
        }

        /// <inheritDoc/>
        public void Add(KeyValuePair<object, object?> item)
        {
            Add(item.Key, item.Value);
        }

        /// <inheritDoc/>
        public void Clear()
        {
            _eventProperties = null;
            _messageProperties = null;
            _messagePropertiesCount = 0;
        }

        /// <inheritDoc/>
        public bool Contains(KeyValuePair<object, object?> item)
        {
            if (!IsEmpty && (_eventProperties != null || ContainsKey(item.Key)))
            {
                IDictionary<object, PropertyValue> eventProperties = GetEventProperties();
                if (eventProperties.Contains(new KeyValuePair<object, PropertyValue>(item.Key, new PropertyValue(item.Value, false))))
                    return true;

                if (eventProperties.Contains(new KeyValuePair<object, PropertyValue>(item.Key, new PropertyValue(item.Value, true))))
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
                return GetEventProperties().Remove(key);
            }
            return false;
        }

        /// <inheritDoc/>
        public bool Remove(KeyValuePair<object, object?> item)
        {
            if (!IsEmpty && (_eventProperties != null || ContainsKey(item.Key)))
            {
                ICollection<KeyValuePair<object, PropertyValue>> eventProperties = GetEventProperties();
                if (eventProperties.Remove(new KeyValuePair<object, PropertyValue>(item.Key, new PropertyValue(item.Value, false))))
                    return true;

                if (eventProperties.Remove(new KeyValuePair<object, PropertyValue>(item.Key, new PropertyValue(item.Value, true))))
                    return true;
            }

            return false;
        }

        /// <inheritDoc/>
        public bool TryGetValue(object key, out object? value)
        {
            if (_eventProperties is null)
            {
                return TryLookupMessagePropertyValue(key, out value);
            }

            if (_eventProperties.TryGetValue(key, out var eventProperty))
            {
                value = eventProperty.Value;
                return true;
            }

            value = null;
            return false;
        }

        private bool TryLookupMessagePropertyValue(object key, out object? propertyValue)
        {
            if (_messageProperties is null || _messagePropertiesCount == 0)
            {
                propertyValue = null;
                return false;
            }

            if (_messagePropertiesCount > 10)
            {
                if (GetEventProperties().TryGetValue(key, out var eventProperty))
                {
                    propertyValue = eventProperty.Value;
                    return true;
                }
            }
            else if (key is string keyString)
            {
                for (int i = 0; i < _messagePropertiesCount; ++i)
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
                for (int i = 0; i < _messagePropertiesCount; ++i)
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
        private static bool VerifyUniqueMessageTemplateParametersFast(MessageTemplateParameter[]? parameterList)
        {
            if (parameterList is null)
                return true;

            var parameterCount = parameterList.Length;
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
        private static void InsertMessagePropertiesIntoEmptyDictionary(Dictionary<object, PropertyValue> eventProperties, MessageTemplateParameter[] messageProperties, int messagePropertiesCount, out bool resetMessageProperties)
        {
            resetMessageProperties = messagePropertiesCount > 0 && messageProperties[0].CaptureType == CaptureType.Unknown;
            if (resetMessageProperties)
            {
                for (int i = 1; i < messagePropertiesCount; ++i)
                {
                    if (messageProperties[i].CaptureType != CaptureType.Unknown)
                    {
                        resetMessageProperties = false;
                        break;
                    }
                }
            }
            
            for (int i = 0; i < messagePropertiesCount; ++i)
            {
                var messageProperty = messageProperties[i];

                try
                {
                    eventProperties.Add(messageProperty.Name, new PropertyValue(messageProperty.Value, !resetMessageProperties));
                }
                catch (ArgumentException)
                {
                    if (eventProperties.TryGetValue(messageProperty.Name, out var propertyValue) && propertyValue.IsMessageProperty)
                    {
                        var uniqueName = GenerateUniquePropertyName(messageProperty.Name, eventProperties, (newkey, props) => props.ContainsKey(newkey));
                        eventProperties.Add(uniqueName, new PropertyValue(messageProperty.Value, !resetMessageProperties));
                        messageProperties[i] = new MessageTemplateParameter(uniqueName, messageProperty.Value, messageProperty.Format, messageProperty.CaptureType);
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

        int IList<MessageTemplateParameter>.IndexOf(MessageTemplateParameter item)
        {
            if (_messageProperties != null && _messagePropertiesCount > 0)
            {
                for (int i = 0; i < _messagePropertiesCount; ++i)
                {
                    if (_messageProperties[i].Equals(item))
                        return i;
                }
            }
            return -1;
        }

        bool ICollection<MessageTemplateParameter>.Contains(MessageTemplateParameter item)
        {
            return ((IList<MessageTemplateParameter>)this).IndexOf(item) >= 0;
        }

        void ICollection<MessageTemplateParameter>.CopyTo(MessageTemplateParameter[] array, int arrayIndex)
        {
            if (_messageProperties != null && _messagePropertiesCount > 0)
            {
                Array.Copy(_messageProperties, 0, array, arrayIndex, _messagePropertiesCount);
            }
        }

        void IList<MessageTemplateParameter>.Insert(int index, MessageTemplateParameter item)
        {
            throw new NotSupportedException("MessageTemplateParameters array is read-only");
        }

        void IList<MessageTemplateParameter>.RemoveAt(int index)
        {
            throw new NotSupportedException("MessageTemplateParameters array is read-only");
        }

        void ICollection<MessageTemplateParameter>.Add(MessageTemplateParameter item)
        {
            throw new NotSupportedException("MessageTemplateParameters array is read-only");
        }

        void ICollection<MessageTemplateParameter>.Clear()
        {
            throw new NotSupportedException("MessageTemplateParameters array is read-only");
        }

        bool ICollection<MessageTemplateParameter>.Remove(MessageTemplateParameter item)
        {
            throw new NotSupportedException("MessageTemplateParameters array is read-only");
        }

        IEnumerator<MessageTemplateParameter> IEnumerable<MessageTemplateParameter>.GetEnumerator()
        {
            if (_messageProperties is null)
                return System.Linq.Enumerable.Empty<MessageTemplateParameter>().GetEnumerator();
            else
                return ((IList<MessageTemplateParameter>)_messageProperties).GetEnumerator();
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
                _messagePropertiesIndex = dictionary._messagePropertiesCount > 0 ? -1 : default(int?);
            }

            public KeyValuePair<object, object?> Current
            {
                get
                {
                    if (_messagePropertiesIndex.HasValue)
                    {
                        var property = (_dictionary._messageProperties ?? ArrayHelper.Empty<MessageTemplateParameter>())[_messagePropertiesIndex.Value];
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
                        return (_dictionary._messageProperties ?? ArrayHelper.Empty<MessageTemplateParameter>())[_messagePropertiesIndex.Value];
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
                        var property = (_dictionary._messageProperties ?? ArrayHelper.Empty<MessageTemplateParameter>())[_messagePropertiesIndex.Value];
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
                var messagePropertiesCount = _dictionary._messagePropertiesCount;
                if (_messagePropertiesIndex.HasValue && _messagePropertiesIndex.Value + 1 < messagePropertiesCount)
                {
                    var eventProperties = _dictionary._eventProperties;
                    var messageProperties = _dictionary._messageProperties;
                    if (eventProperties is null || messageProperties is null)
                    {
                        _messagePropertiesIndex = _messagePropertiesIndex.Value + 1;
                        return true;
                    }

                    for (int i = _messagePropertiesIndex.Value + 1; i < messagePropertiesCount; ++i)
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
                _messagePropertiesIndex = _dictionary._messagePropertiesCount > 0 ? -1 : default(int?);
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
                    var eventProperties = _dictionary.GetEventProperties();
                    if (eventProperties.ContainsValue(new PropertyValue(item, false)))
                        return true;

                    if (eventProperties.ContainsValue(new PropertyValue(item, true)))
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
