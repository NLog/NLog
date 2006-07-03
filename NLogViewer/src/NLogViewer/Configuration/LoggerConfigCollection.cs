using System;

namespace NLogViewer.Configuration
{
    /// <summary>
    /// A collection of elements of type LoggerConfig
    /// </summary>
    [Serializable]
    public class LoggerConfigCollection: System.Collections.CollectionBase
    {
        /// <summary>
        /// Initializes a new empty instance of the LoggerConfigCollection class.
        /// </summary>
        public LoggerConfigCollection()
        {
            // empty
        }

        /// <summary>
        /// Initializes a new instance of the LoggerConfigCollection class, containing elements
        /// copied from an array.
        /// </summary>
        /// <param name="items">
        /// The array whose elements are to be added to the new LoggerConfigCollection.
        /// </param>
        public LoggerConfigCollection(LoggerConfig[] items)
        {
            this.AddRange(items);
        }

        /// <summary>
        /// Initializes a new instance of the LoggerConfigCollection class, containing elements
        /// copied from another instance of LoggerConfigCollection
        /// </summary>
        /// <param name="items">
        /// The LoggerConfigCollection whose elements are to be added to the new LoggerConfigCollection.
        /// </param>
        public LoggerConfigCollection(LoggerConfigCollection items)
        {
            this.AddRange(items);
        }

        /// <summary>
        /// Adds the elements of an array to the end of this LoggerConfigCollection.
        /// </summary>
        /// <param name="items">
        /// The array whose elements are to be added to the end of this LoggerConfigCollection.
        /// </param>
        public virtual void AddRange(LoggerConfig[] items)
        {
            foreach (LoggerConfig item in items)
            {
                this.List.Add(item);
            }
        }

        /// <summary>
        /// Adds the elements of another LoggerConfigCollection to the end of this LoggerConfigCollection.
        /// </summary>
        /// <param name="items">
        /// The LoggerConfigCollection whose elements are to be added to the end of this LoggerConfigCollection.
        /// </param>
        public virtual void AddRange(LoggerConfigCollection items)
        {
            foreach (LoggerConfig item in items)
            {
                this.List.Add(item);
            }
        }

        /// <summary>
        /// Adds an instance of type LoggerConfig to the end of this LoggerConfigCollection.
        /// </summary>
        /// <param name="value">
        /// The LoggerConfig to be added to the end of this LoggerConfigCollection.
        /// </param>
        public virtual void Add(LoggerConfig value)
        {
            this.List.Add(value);
        }

        /// <summary>
        /// Determines whether a specfic LoggerConfig value is in this LoggerConfigCollection.
        /// </summary>
        /// <param name="value">
        /// The LoggerConfig value to locate in this LoggerConfigCollection.
        /// </param>
        /// <returns>
        /// true if value is found in this LoggerConfigCollection;
        /// false otherwise.
        /// </returns>
        public virtual bool Contains(LoggerConfig value)
        {
            return this.List.Contains(value);
        }

        /// <summary>
        /// Return the zero-based index of the first occurrence of a specific value
        /// in this LoggerConfigCollection
        /// </summary>
        /// <param name="value">
        /// The LoggerConfig value to locate in the LoggerConfigCollection.
        /// </param>
        /// <returns>
        /// The zero-based index of the first occurrence of the _ELEMENT value if found;
        /// -1 otherwise.
        /// </returns>
        public virtual int IndexOf(LoggerConfig value)
        {
            return this.List.IndexOf(value);
        }

        /// <summary>
        /// Inserts an element into the LoggerConfigCollection at the specified index
        /// </summary>
        /// <param name="index">
        /// The index at which the LoggerConfig is to be inserted.
        /// </param>
        /// <param name="value">
        /// The LoggerConfig to insert.
        /// </param>
        public virtual void Insert(int index, LoggerConfig value)
        {
            this.List.Insert(index, value);
        }

        /// <summary>
        /// Gets or sets the LoggerConfig at the given index in this LoggerConfigCollection.
        /// </summary>
        public virtual LoggerConfig this[int index]
        {
            get
            {
                return (LoggerConfig) this.List[index];
            }
            set
            {
                this.List[index] = value;
            }
        }

        /// <summary>
        /// Removes the first occurrence of a specific LoggerConfig from this LoggerConfigCollection.
        /// </summary>
        /// <param name="value">
        /// The LoggerConfig value to remove from this LoggerConfigCollection.
        /// </param>
        public virtual void Remove(LoggerConfig value)
        {
            this.List.Remove(value);
        }

        /// <summary>
        /// Type-specific enumeration class, used by LoggerConfigCollection.GetEnumerator.
        /// </summary>
        public class Enumerator: System.Collections.IEnumerator
        {
            private System.Collections.IEnumerator wrapped;

            public Enumerator(LoggerConfigCollection collection)
            {
                this.wrapped = ((System.Collections.CollectionBase)collection).GetEnumerator();
            }

            public LoggerConfig Current
            {
                get
                {
                    return (LoggerConfig) (this.wrapped.Current);
                }
            }

            object System.Collections.IEnumerator.Current
            {
                get
                {
                    return (LoggerConfig) (this.wrapped.Current);
                }
            }

            public bool MoveNext()
            {
                return this.wrapped.MoveNext();
            }

            public void Reset()
            {
                this.wrapped.Reset();
            }
        }

        /// <summary>
        /// Returns an enumerator that can iterate through the elements of this LoggerConfigCollection.
        /// </summary>
        /// <returns>
        /// An object that implements System.Collections.IEnumerator.
        /// </returns>        
        public new virtual LoggerConfigCollection.Enumerator GetEnumerator()
        {
            return new LoggerConfigCollection.Enumerator(this);
        }
    }
}
