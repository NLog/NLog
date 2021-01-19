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
    using System.Collections;
    using System.Collections.Generic;
    using System.Diagnostics;
    using MessageTemplates;

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
            /// Is this a property of the message?
            /// </summary>
            public readonly bool IsMessageProperty;

            /// <summary>
            /// 
            /// </summary>
            /// <param name="value">Value of the property</param>
            /// <param name="isMessageProperty">Is this a property of the message?</param>
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
        /// The properties extracted from the message
        /// </summary>
        private IList<MessageTemplateParameter> _messageProperties;
        private DictionaryCollection _keyCollection;
        private DictionaryCollection _valueCollection;

        /// <summary>
        /// Injects the list of message-template-parameter into the IDictionary-interface
        /// </summary>
        /// <param name="parameterList">Message-template-parameters</param>
        public PropertiesDictionary(IList<MessageTemplateParameter> parameterList = null)
        {
            if (parameterList?.Count > 0)
            {
                MessageProperties = parameterList;
            }
        }

        private bool IsEmpty => (_eventProperties == null || _eventProperties.Count == 0) && (_messageProperties == null || _messageProperties.Count == 0);

        private Dictionary<object, PropertyValue> EventProperties
        {
            get
            {
                if (_eventProperties == null)
                {
                    if (_messageProperties != null && _messageProperties.Count > 0)
                    {
                        _eventProperties = new Dictionary<object, PropertyValue>(_messageProperties.Count);
                        if (!InsertMessagePropertiesIntoEmptyDictionary(_messageProperties, _eventProperties))
                        {
                            _messageProperties = CreateUniqueMessagePropertiesListSlow(_messageProperties, _eventProperties);
                        }
                    }
                    else
                    {
                        _eventProperties = new Dictionary<object, PropertyValue>();
                    }
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
            if (_eventProperties == null && VerifyUniqueMessageTemplateParametersFast(newMessageProperties))
            {
                return newMessageProperties;
            }
            else
            {
                if (_eventProperties == null)
                {
                    _eventProperties = new Dictionary<object, PropertyValue>(newMessageProperties.Count);
                }

                if (oldMessageProperties != null && _eventProperties.Count > 0)
                {
                    RemoveOldMessageProperties(oldMessageProperties);
                }

                if (newMessageProperties != null && (_eventProperties.Count > 0 || !InsertMessagePropertiesIntoEmptyDictionary(newMessageProperties, _eventProperties)))
                {
                    return CreateUniqueMessagePropertiesListSlow(newMessageProperties, _eventProperties);
                }
                else
                {
                    return newMessageProperties;
                }
            }
        }

        private void RemoveOldMessageProperties(IList<MessageTemplateParameter> oldMessageProperties)
        {
            for (int i = 0; i < oldMessageProperties.Count; ++i)
            {
                if (_eventProperties.TryGetValue(oldMessageProperties[i].Name, out var propertyValue) && propertyValue.IsMessageProperty)
                {
                    _eventProperties.Remove(oldMessageProperties[i].Name);
                }
            }
        }

        /// <inheritDoc/>
        public object this[object key]
        {
            get
            {
                if (!IsEmpty && EventProperties.TryGetValue(key, out var valueItem))
                {
                    return valueItem.Value;
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
            _eventProperties?.Clear();
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
            if (!IsEmpty)
            {
                return EventProperties.ContainsKey(key);
            }
            return false;
        }

        /// <inheritDoc/>
        public void CopyTo(KeyValuePair<object, object>[] array, int arrayIndex)
        {
            if (array == null)
                throw new ArgumentNullException(nameof(array));
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
            if (!IsEmpty && EventProperties.TryGetValue(key, out var valueItem))
            {
                value = valueItem.Value;
                return true;
            }

            value = null;
            return false;
        }

        /// <summary>
        /// Check if the message-template-parameters can be used directly without allocating a dictionary
        /// </summary>
        /// <param name="parameterList">Message-template-parameters</param>
        /// <returns>Are all parameter names unique (true / false)</returns>
        private static bool VerifyUniqueMessageTemplateParametersFast(IList<MessageTemplateParameter> parameterList)
        {
            if (parameterList == null || parameterList.Count == 0)
                return true;

            if (parameterList.Count > 10)
                return false;

            for (int i = 0; i < parameterList.Count - 1; ++i)
            {
                for (int j = i + 1; j < parameterList.Count; ++j)
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
        /// <param name="eventProperties">The initially empty dictionary</param>
        /// <returns>Message-template-parameters was inserted into dictionary without trouble (true/false)</returns>
        private static bool InsertMessagePropertiesIntoEmptyDictionary(IList<MessageTemplateParameter> messageProperties, Dictionary<object, PropertyValue> eventProperties)
        {
            try
            {
                for (int i = 0; i < messageProperties.Count; ++i)
                {
                    eventProperties.Add(messageProperties[i].Name, new PropertyValue(messageProperties[i].Value, true));
                }
                return true; // We are done
            }
            catch (ArgumentException)
            {
                // Duplicate keys found, lets try again
                for (int i = 0; i < messageProperties.Count; ++i)
                {
                    //remove the duplicates
                    eventProperties.Remove(messageProperties[i].Name);
                }
                return false;
            }
        }

        /// <summary>
        /// Attempt to override the existing dictionary values using the message-template-parameters 
        /// </summary>
        /// <param name="messageProperties">Message-template-parameters</param>
        /// <param name="eventProperties">The already filled dictionary</param>
        /// <returns>List of unique message-template-parameters</returns>
        private static IList<MessageTemplateParameter> CreateUniqueMessagePropertiesListSlow(IList<MessageTemplateParameter> messageProperties, Dictionary<object, PropertyValue> eventProperties)
        {
            List<MessageTemplateParameter> messagePropertiesUnique = null;
            for (int i = 0; i < messageProperties.Count; ++i)
            {
                if (eventProperties.TryGetValue(messageProperties[i].Name, out var valueItem) && valueItem.IsMessageProperty)
                {
                    if (messagePropertiesUnique == null)
                    {
                        messagePropertiesUnique = new List<MessageTemplateParameter>(messageProperties.Count);
                        for (int j = 0; j < i; ++j)
                        {
                            messagePropertiesUnique.Add(messageProperties[j]);
                        }
                    }
                    continue;   // Skip already exists
                }

                eventProperties[messageProperties[i].Name] = new PropertyValue(messageProperties[i].Value, true);
                messagePropertiesUnique?.Add(messageProperties[i]);
            }

            return messagePropertiesUnique ?? messageProperties;
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
                        // Move forward to a key that is not overriden
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
                    // Move forward to a key that is not overriden
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
                return propertiesDictionary._messageProperties != null && propertiesDictionary._messageProperties.Count > 0;
            }

            private static bool HasEventProperties(PropertiesDictionary propertiesDictionary)
            {
                return propertiesDictionary._eventProperties != null && propertiesDictionary._eventProperties.Count > 0;
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
                if (_dictionary._eventProperties == null)
                    return startIndex;

                for (int i = startIndex; i < _dictionary._messageProperties.Count; ++i)
                {
                    if (_dictionary._eventProperties.TryGetValue(_dictionary._messageProperties[i].Name, out var valueItem) && valueItem.IsMessageProperty)
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

        private class ParameterEnumerator : DictionaryEnumeratorBase, IEnumerator<MessageTemplateParameter>
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

        private class DictionaryEnumerator : DictionaryEnumeratorBase, IEnumerator<KeyValuePair<object, object>>
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
        private class DictionaryCollection : ICollection<object>
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
                if (array == null)
                    throw new ArgumentNullException(nameof(array));
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

            private class DictionaryCollectionEnumerator : DictionaryEnumeratorBase, IEnumerator<object>
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
    }
}
