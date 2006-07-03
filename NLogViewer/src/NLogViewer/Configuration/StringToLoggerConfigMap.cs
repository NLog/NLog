using System;

namespace NLogViewer.Configuration
{
    /// <summary>
    /// A dictionary with keys of type String and values of type LoggerConfig
    /// </summary>
    [Serializable]
    public class StringToLoggerConfigMap : System.Collections.DictionaryBase
    {
        /// <summary>
        /// Initializes a new empty instance of the StringToLoggerConfigMap class
        /// </summary>
        public StringToLoggerConfigMap()
        {
            // empty
        }

        /// <summary>
        /// Gets or sets the LoggerConfig associated with the given String
        /// </summary>
        /// <param name="key">
        /// The String whose value to get or set.
        /// </param>
        public virtual LoggerConfig this[String key]
        {
            get
            {
                return (LoggerConfig) this.Dictionary[key];
            }
            set
            {
                this.Dictionary[key] = value;
            }
        }

        /// <summary>
        /// Adds an element with the specified key and value to this StringToLoggerConfigMap.
        /// </summary>
        /// <param name="key">
        /// The String key of the element to add.
        /// </param>
        /// <param name="value">
        /// The LoggerConfig value of the element to add.
        /// </param>
        public virtual void Add(String key, LoggerConfig value)
        {
            this.Dictionary.Add(key, value);
        }

        /// <summary>
        /// Determines whether this StringToLoggerConfigMap contains a specific key.
        /// </summary>
        /// <param name="key">
        /// The String key to locate in this StringToLoggerConfigMap.
        /// </param>
        /// <returns>
        /// true if this StringToLoggerConfigMap contains an element with the specified key;
        /// otherwise, false.
        /// </returns>
        public virtual bool Contains(String key)
        {
            return this.Dictionary.Contains(key);
        }

        /// <summary>
        /// Determines whether this StringToLoggerConfigMap contains a specific key.
        /// </summary>
        /// <param name="key">
        /// The String key to locate in this StringToLoggerConfigMap.
        /// </param>
        /// <returns>
        /// true if this StringToLoggerConfigMap contains an element with the specified key;
        /// otherwise, false.
        /// </returns>
        public virtual bool ContainsKey(String key)
        {
            return this.Dictionary.Contains(key);
        }

        /// <summary>
        /// Determines whether this StringToLoggerConfigMap contains a specific value.
        /// </summary>
        /// <param name="value">
        /// The LoggerConfig value to locate in this StringToLoggerConfigMap.
        /// </param>
        /// <returns>
        /// true if this StringToLoggerConfigMap contains an element with the specified value;
        /// otherwise, false.
        /// </returns>
        public virtual bool ContainsValue(LoggerConfig value)
        {
            foreach (LoggerConfig item in this.Dictionary.Values)
            {
                if (item == value)
                    return true;
            }
            return false;
        }

        /// <summary>
        /// Removes the element with the specified key from this StringToLoggerConfigMap.
        /// </summary>
        /// <param name="key">
        /// The String key of the element to remove.
        /// </param>
        public virtual void Remove(String key)
        {
            this.Dictionary.Remove(key);
        }

        /// <summary>
        /// Gets a collection containing the keys in this StringToLoggerConfigMap.
        /// </summary>
        public virtual System.Collections.ICollection Keys
        {
            get
            {
                return this.Dictionary.Keys;
            }
        }

        /// <summary>
        /// Gets a collection containing the values in this StringToLoggerConfigMap.
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
