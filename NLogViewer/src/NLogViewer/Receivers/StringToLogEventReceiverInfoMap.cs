using System;

namespace NLogViewer.Receivers
{
    /// <summary>
    /// A dictionary with keys of type String and values of type LogEventReceiverInfo
    /// </summary>
    public class StringToLogEventReceiverInfoMap: System.Collections.DictionaryBase
    {
        /// <summary>
        /// Initializes a new empty instance of the StringToLogEventReceiverInfoMap class
        /// </summary>
        public StringToLogEventReceiverInfoMap()
        {
            // empty
        }

        /// <summary>
        /// Gets or sets the LogEventReceiverInfo associated with the given String
        /// </summary>
        /// <param name="key">
        /// The String whose value to get or set.
        /// </param>
        public virtual LogEventReceiverInfo this[String key]
        {
            get
            {
                return (LogEventReceiverInfo) this.Dictionary[key];
            }
            set
            {
                this.Dictionary[key] = value;
            }
        }

        /// <summary>
        /// Adds an element with the specified key and value to this StringToLogEventReceiverInfoMap.
        /// </summary>
        /// <param name="key">
        /// The String key of the element to add.
        /// </param>
        /// <param name="value">
        /// The LogEventReceiverInfo value of the element to add.
        /// </param>
        public virtual void Add(String key, LogEventReceiverInfo value)
        {
            this.Dictionary.Add(key, value);
        }

        /// <summary>
        /// Determines whether this StringToLogEventReceiverInfoMap contains a specific key.
        /// </summary>
        /// <param name="key">
        /// The String key to locate in this StringToLogEventReceiverInfoMap.
        /// </param>
        /// <returns>
        /// true if this StringToLogEventReceiverInfoMap contains an element with the specified key;
        /// otherwise, false.
        /// </returns>
        public virtual bool Contains(String key)
        {
            return this.Dictionary.Contains(key);
        }

        /// <summary>
        /// Determines whether this StringToLogEventReceiverInfoMap contains a specific key.
        /// </summary>
        /// <param name="key">
        /// The String key to locate in this StringToLogEventReceiverInfoMap.
        /// </param>
        /// <returns>
        /// true if this StringToLogEventReceiverInfoMap contains an element with the specified key;
        /// otherwise, false.
        /// </returns>
        public virtual bool ContainsKey(String key)
        {
            return this.Dictionary.Contains(key);
        }

        /// <summary>
        /// Determines whether this StringToLogEventReceiverInfoMap contains a specific value.
        /// </summary>
        /// <param name="value">
        /// The LogEventReceiverInfo value to locate in this StringToLogEventReceiverInfoMap.
        /// </param>
        /// <returns>
        /// true if this StringToLogEventReceiverInfoMap contains an element with the specified value;
        /// otherwise, false.
        /// </returns>
        public virtual bool ContainsValue(LogEventReceiverInfo value)
        {
            foreach (LogEventReceiverInfo item in this.Dictionary.Values)
            {
                if (item == value)
                    return true;
            }
            return false;
        }

        /// <summary>
        /// Removes the element with the specified key from this StringToLogEventReceiverInfoMap.
        /// </summary>
        /// <param name="key">
        /// The String key of the element to remove.
        /// </param>
        public virtual void Remove(String key)
        {
            this.Dictionary.Remove(key);
        }

        /// <summary>
        /// Gets a collection containing the keys in this StringToLogEventReceiverInfoMap.
        /// </summary>
        public virtual System.Collections.ICollection Keys
        {
            get
            {
                return this.Dictionary.Keys;
            }
        }

        /// <summary>
        /// Gets a collection containing the values in this StringToLogEventReceiverInfoMap.
        /// </summary>
        public virtual System.Collections.ICollection Values
        {
            get
            {
                return this.Dictionary.Values;
            }
        }
    }
}
