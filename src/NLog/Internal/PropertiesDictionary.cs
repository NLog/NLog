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

    /// <summary>
    /// Dictionary that combines the standard <see cref="LogEventInfo.Properties" /> with the
    /// MessageTemplate-properties extracted from the <see cref="LogEventInfo.Message" />.
    /// 
    /// The <see cref="MessageProperties" /> are returned as the first items
    /// in the collection, and in positional order.
    /// </summary>
    internal sealed class PropertiesDictionary : IDictionary<object, object>
    {
        struct PropertyValue
        {
            public readonly object Value;
            public readonly bool MessageProperty;

            public PropertyValue(object value, bool messageProperty)
            {
                Value = value;
                MessageProperty = messageProperty;
            }
        }

        private Dictionary<object, PropertyValue> _eventProperties;
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
            MessageProperties = parameterList;
        }

        private bool IsEmpty { get { return (_eventProperties == null || _eventProperties.Count == 0) && (_messageProperties == null || _messageProperties.Count == 0); } }

        public IDictionary EventContext { get { return _eventContextAdapter ?? (_eventContextAdapter = new DictionaryAdapter<object, object>(this)); } }

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
            private set
            {
                if (value != null && value.Count > 0)
                {
                    _messageProperties = _eventProperties == null ? CreateUniqueMessagePropertiesListFast(value) : null;
                    if (_messageProperties == null)
                    {
                        // Dictionary was already allocated, or the message-template-parameters are troublesome
                        var eventProperties = _eventProperties ?? (_eventProperties = new Dictionary<object, PropertyValue>(value.Count));

                        _messageProperties = new List<MessageTemplateParameter>(value.Count);
                        for (int i = 0; i < value.Count; ++i)
                            _messageProperties.Add(value[i]);

                        if (eventProperties.Count != 0 || !InsertMessagePropertiesIntoEmptyDictionary(_messageProperties, eventProperties))
                        {
                            _messageProperties = CreateUniqueMessagePropertiesListSlow(_messageProperties, eventProperties);
                        }
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
                    PropertyValue valueItem;
                    if (EventProperties.TryGetValue(key, out valueItem))
                    {
                        return valueItem.Value;
                    }
                }

                throw new KeyNotFoundException();
            }
            set
            {
                EventProperties[key] = new PropertyValue(value, false);
            }
        }

        /// <inheritDoc/>
        public ICollection<object> Keys { get { return KeyCollection; } }
        /// <inheritDoc/>
        public ICollection<object> Values { get { return ValueCollection; } }

        private DictionaryCollection KeyCollection
        {
            get
            {
                if (_keyCollection != null)
                    return _keyCollection;
                else if (IsEmpty)
                    return EmptyKeyCollection;
                else
                    return _keyCollection ?? (_keyCollection = new DictionaryCollection(this, true));
            }
        }

        private DictionaryCollection ValueCollection
        {
            get
            {
                if (_valueCollection != null)
                    return _valueCollection;
                else if (IsEmpty)
                    return EmptyValueCollection;
                else
                    return _valueCollection ?? (_valueCollection = new DictionaryCollection(this, false));
            }
        }

        private static readonly DictionaryCollection EmptyKeyCollection = new DictionaryCollection(new PropertiesDictionary(), true);
        private static readonly DictionaryCollection EmptyValueCollection = new DictionaryCollection(new PropertiesDictionary(), false);

        /// <inheritDoc/>
        public int Count { get { return (_eventProperties?.Count) ?? (_messageProperties?.Count) ?? 0; } }

        /// <inheritDoc/>
        public bool IsReadOnly { get { return false; } }

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
                _eventProperties.Clear();
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
                throw new ArgumentNullException("array");
            if (arrayIndex < 0)
                throw new ArgumentOutOfRangeException("arrayIndex");

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
            else
                return new DictionaryEnumerator(this);
        }

        /// <inheritDoc/>
        IEnumerator IEnumerable.GetEnumerator()
        {
            if (IsEmpty)
                return ArrayHelper.Empty<KeyValuePair<object, object>>().GetEnumerator();
            else
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
                PropertyValue valueItem;
                if (EventProperties.TryGetValue(key, out valueItem))
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
        static IList<MessageTemplateParameter> CreateUniqueMessagePropertiesListFast(IList<MessageTemplateParameter> parameterList)
        {
            if (parameterList.Count <= 10)
            {
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
            PropertyValue valueItem;
            for (int i = 0; i < messageProperties.Count; ++i)
            {
                if (eventProperties.TryGetValue(messageProperties[i].Name, out valueItem))
                {
                    if (valueItem.MessageProperty)
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
                if (messagePropertiesUnique != null)
                {
                    messagePropertiesUnique.Add(messageProperties[i]);
                }
            }

            return messagePropertiesUnique ?? messageProperties;
        }

        class DictionaryEnumeratorBase
        {
            readonly PropertiesDictionary _dictionary;
            int? _messagePropertiesEnumerator;
            bool _eventEnumeratorCreated;
            Dictionary<object, PropertyValue>.Enumerator _eventEnumerator;

            protected DictionaryEnumeratorBase(PropertiesDictionary dictionary)
            {
                _dictionary = dictionary;
            }

            public KeyValuePair<object, object> CurrentPair
            {
                get
                {
                    if (_messagePropertiesEnumerator.HasValue)
                    {
                        var property = _dictionary._messageProperties[_messagePropertiesEnumerator.Value];
                        return new KeyValuePair<object, object>(property.Name, property.Value);
                    }
                    else if (_eventEnumeratorCreated)
                        return new KeyValuePair<object, object>(_eventEnumerator.Current.Key, _eventEnumerator.Current.Value.Value);
                    else
                        throw new InvalidOperationException();
                }
            }

            public bool MoveNext()
            {
                if (_messagePropertiesEnumerator.HasValue)
                {
                    if ((_messagePropertiesEnumerator.Value + 1) < _dictionary._messageProperties.Count)
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
                else
                {
                    if (_eventEnumeratorCreated)
                    {
                        return MoveNextValidEventProperty();
                    }
                    else
                    {
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
                }
            }

            private bool MoveNextValidEventProperty()
            {
                while (_eventEnumerator.MoveNext())
                {
                    if (!_eventEnumerator.Current.Value.MessageProperty)
                        return true;
                }
                return false;
            }

            private int? FindNextValidMessagePropertyIndex(int startIndex)
            {
                if (_dictionary._eventProperties == null)
                    return startIndex;

                PropertyValue valueItem;
                for (int i = startIndex; i < _dictionary._messageProperties.Count; ++i)
                {
                    if (_dictionary._eventProperties.TryGetValue(_dictionary._messageProperties[i].Name, out valueItem) && valueItem.MessageProperty)
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

        class DictionaryEnumerator : DictionaryEnumeratorBase, IEnumerator<KeyValuePair<object, object>>
        {
            /// <inheritDoc/>
            public KeyValuePair<object, object> Current { get { return CurrentPair; } }

            /// <inheritDoc/>
            object IEnumerator.Current { get { return CurrentPair; } }

            public DictionaryEnumerator(PropertiesDictionary dictionary)
                : base(dictionary)
            {
            }
        }

        class DictionaryCollection : ICollection<object>
        {
            readonly PropertiesDictionary _dictionary;
            readonly bool _keyCollection;

            public DictionaryCollection(PropertiesDictionary dictionary, bool keyCollection)
            {
                _dictionary = dictionary;
                _keyCollection = keyCollection;
            }

            /// <inheritDoc/>
            public int Count { get { return _dictionary.Count; } }

            /// <inheritDoc/>
            public bool IsReadOnly { get { return true; } }


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
                else
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
            }

            /// <inheritDoc/>
            public void CopyTo(object[] array, int arrayIndex)
            {
                if (array == null)
                    throw new ArgumentNullException("array");
                if (arrayIndex < 0)
                    throw new ArgumentOutOfRangeException("arrayIndex");

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
                readonly bool _keyCollection;

                public DictionaryCollectionEnumerator(PropertiesDictionary dictionary, bool keyCollection)
                    : base(dictionary)
                {
                    _keyCollection = keyCollection;
                }

                /// <inheritDoc/>
                public object Current { get { return _keyCollection ? CurrentPair.Key : CurrentPair.Value; } }
            }
        }
    }
}
