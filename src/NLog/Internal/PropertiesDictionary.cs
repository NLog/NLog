// 
// Copyright (c) 2004-2017 Jaroslaw Kowalski <jaak@jkowalski.net>, Kim Christensen, Julian Verdurmen
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
    using NLog.MessageTemplates;

    /// <summary>
    /// Dictionary that combines the standard <see cref="LogEventInfo.Properties" /> with the
    /// MessageTemplate-properties extracted from the <see cref="LogEventInfo.Message" />.
    /// 
    /// The <see cref="MessageProperties" /> are returned as the first items
    /// in the collection, and in positional order.
    /// </summary>
    internal sealed class PropertiesDictionary : IDictionary<object, object>
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
        private IDictionary _eventContextAdapter;

        /// <summary>
        /// Injects the list of message-template-parameter into the IDictionary-interface
        /// </summary>
        /// <param name="parameterList">Message-template-parameters</param>
        public PropertiesDictionary(IList<MessageTemplateParameter> parameterList = null)
        {
            if (parameterList != null && parameterList.Count > 0)
            {
                var messageProperties = new MessageTemplateParameter[parameterList.Count];
                for (int i = 0; i < parameterList.Count; ++i)
                    messageProperties[i] = parameterList[i];
                MessageProperties = messageProperties;
            }
        }

        private bool IsEmpty => (_eventProperties == null || _eventProperties.Count == 0) && (_messageProperties == null || _messageProperties.Count == 0);

        public IDictionary EventContext => _eventContextAdapter ?? (_eventContextAdapter = new DictionaryAdapter<object, object>(this));

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
            get
            {
                return _messageProperties ?? ArrayHelper.Empty<MessageTemplateParameter>();
            }
            internal set
            {
                if (_eventProperties == null && VerifyUniqueMessageTemplateParametersFast(value))
                {
                    _messageProperties = value;
                }
                else
                {
                    if (_eventProperties == null)
                    {
                        _eventProperties = new Dictionary<object, PropertyValue>(value.Count);
                    }

                    if (_messageProperties != null && _eventProperties.Count > 0)
                    {
                        PropertyValue propertyValue;
                        for (int i = 0; i < _messageProperties.Count; ++i)
                        {
                            if (_eventProperties.TryGetValue(_messageProperties[i].Name, out propertyValue) && propertyValue.IsMessageProperty)
                            {
                                _eventProperties.Remove(_messageProperties[i].Name);
                            }
                        }
                    }

                    if (value != null && (_eventProperties.Count != 0 || !InsertMessagePropertiesIntoEmptyDictionary(value, _eventProperties)))
                    {
                        _messageProperties = CreateUniqueMessagePropertiesListSlow(value, _eventProperties);
                    }
                    else
                    {
                        _messageProperties = value;
                    }
                }
            }
        }

        /// <inheritDoc/>
        public object this[object key]
        {
            get
            {
                if (!IsEmpty)
                {
                    if (EventProperties.TryGetValue(key, out var valueItem))
                    {
                        return valueItem.Value;
                    }
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
            if (!IsEmpty)
            {
                if (EventProperties.TryGetValue(key, out var valueItem))
                {
                    value = valueItem.Value;
                    return true;
                }
            }

            value = null;
            return false;
        }

        /// <summary>
        /// Attempt to use the message-template-parameters without allocating a dictionary
        /// </summary>
        /// <param name="parameterList">Message-template-parameters</param>
        /// <returns>List of message-template-parameters if succesful (else null)</returns>
        private static IList<MessageTemplateParameter> CreateUniqueMessagePropertiesListFast(IList<MessageTemplateParameter> parameterList)
        {
            if (parameterList.Count <= 10)
            {
                bool uniqueMessageProperties = VerifyUniqueMessageTemplateParametersFast(parameterList);
                if (uniqueMessageProperties)
                {
                    var messageProperties = new MessageTemplateParameter[parameterList.Count];
                    for (int i = 0; i < parameterList.Count; ++i)
                        messageProperties[i] = parameterList[i];
                    return messageProperties;
                }
            }

            return null;
        }

        private static bool VerifyUniqueMessageTemplateParametersFast(IList<MessageTemplateParameter> parameterList)
        {
            if (parameterList == null || parameterList.Count == 0)
                return true;

            if (parameterList.Count > 10)
                return false;

            bool uniqueMessageProperties = true;
            for (int i = 0; i < parameterList.Count - 1; ++i)
            {
                for (int j = i + 1; j < parameterList.Count; ++j)
                {
                    if (parameterList[i].Name == parameterList[j].Name)
                    {
                        uniqueMessageProperties = false;
                        break;
                    }
                }
            }

            return uniqueMessageProperties;
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
                if (eventProperties.TryGetValue(messageProperties[i].Name, out var valueItem))
                {
                    if (valueItem.IsMessageProperty)
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
                }

                eventProperties[messageProperties[i].Name] = new PropertyValue(messageProperties[i].Value, true);
                messagePropertiesUnique?.Add(messageProperties[i]);
            }

            return messagePropertiesUnique ?? messageProperties;
        }

        private class DictionaryEnumeratorBase
        {
            private readonly PropertiesDictionary _dictionary;
            private int? _messagePropertiesEnumerator;
            private bool _eventEnumeratorCreated;
            private Dictionary<object, PropertyValue>.Enumerator _eventEnumerator;

            protected DictionaryEnumeratorBase(PropertiesDictionary dictionary)
            {
                _dictionary = dictionary;
            }

            protected KeyValuePair<object, object> CurrentPair
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

                    if (_dictionary._eventProperties != null && _dictionary._eventProperties.Count > 0)
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
                if (_dictionary._messageProperties != null && _dictionary._messageProperties.Count > 0)
                {
                    // Move forward to a key that is not overriden
                    _messagePropertiesEnumerator = FindNextValidMessagePropertyIndex(0);
                    if (_messagePropertiesEnumerator.HasValue)
                    {
                        return true;
                    }
                }

                if (_dictionary._eventProperties != null && _dictionary._eventProperties.Count > 0)
                {
                    _eventEnumerator = _dictionary._eventProperties.GetEnumerator();
                    _eventEnumeratorCreated = true;
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

        private class DictionaryEnumerator : DictionaryEnumeratorBase, IEnumerator<KeyValuePair<object, object>>
        {
            /// <inheritDoc/>
            public KeyValuePair<object, object> Current => CurrentPair;

            /// <inheritDoc/>
            object IEnumerator.Current => CurrentPair;

            public DictionaryEnumerator(PropertiesDictionary dictionary)
                : base(dictionary)
            {
            }
        }

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
                public object Current => _keyCollection ? CurrentPair.Key : CurrentPair.Value;
            }
        }
    }
}
