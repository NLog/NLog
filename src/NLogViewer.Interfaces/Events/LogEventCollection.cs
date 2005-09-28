using System;

namespace NLogViewer.Events
{
    /// <summary>
    /// A collection of elements of type LogEvent
    /// </summary>
    public class LogEventCollection: System.Collections.CollectionBase
    {
        /// <summary>
        /// Initializes a new empty instance of the LogEventCollection class.
        /// </summary>
        public LogEventCollection()
        {
            // empty
        }

        /// <summary>
        /// Initializes a new instance of the LogEventCollection class, containing elements
        /// copied from an array.
        /// </summary>
        /// <param name="items">
        /// The array whose elements are to be added to the new LogEventCollection.
        /// </param>
        public LogEventCollection(LogEvent[] items)
        {
            this.AddRange(items);
        }

        /// <summary>
        /// Initializes a new instance of the LogEventCollection class, containing elements
        /// copied from another instance of LogEventCollection
        /// </summary>
        /// <param name="items">
        /// The LogEventCollection whose elements are to be added to the new LogEventCollection.
        /// </param>
        public LogEventCollection(LogEventCollection items)
        {
            this.AddRange(items);
        }

        /// <summary>
        /// Adds the elements of an array to the end of this LogEventCollection.
        /// </summary>
        /// <param name="items">
        /// The array whose elements are to be added to the end of this LogEventCollection.
        /// </param>
        public virtual void AddRange(LogEvent[] items)
        {
            foreach (LogEvent item in items)
            {
                this.List.Add(item);
            }
        }

        /// <summary>
        /// Adds the elements of another LogEventCollection to the end of this LogEventCollection.
        /// </summary>
        /// <param name="items">
        /// The LogEventCollection whose elements are to be added to the end of this LogEventCollection.
        /// </param>
        public virtual void AddRange(LogEventCollection items)
        {
            foreach (LogEvent item in items)
            {
                this.List.Add(item);
            }
        }

        /// <summary>
        /// Adds an instance of type LogEvent to the end of this LogEventCollection.
        /// </summary>
        /// <param name="value">
        /// The LogEvent to be added to the end of this LogEventCollection.
        /// </param>
        public virtual void Add(LogEvent value)
        {
            this.List.Add(value);
        }

        /// <summary>
        /// Determines whether a specfic LogEvent value is in this LogEventCollection.
        /// </summary>
        /// <param name="value">
        /// The LogEvent value to locate in this LogEventCollection.
        /// </param>
        /// <returns>
        /// true if value is found in this LogEventCollection;
        /// false otherwise.
        /// </returns>
        public virtual bool Contains(LogEvent value)
        {
            return this.List.Contains(value);
        }

        /// <summary>
        /// Return the zero-based index of the first occurrence of a specific value
        /// in this LogEventCollection
        /// </summary>
        /// <param name="value">
        /// The LogEvent value to locate in the LogEventCollection.
        /// </param>
        /// <returns>
        /// The zero-based index of the first occurrence of the _ELEMENT value if found;
        /// -1 otherwise.
        /// </returns>
        public virtual int IndexOf(LogEvent value)
        {
            return this.List.IndexOf(value);
        }

        /// <summary>
        /// Inserts an element into the LogEventCollection at the specified index
        /// </summary>
        /// <param name="index">
        /// The index at which the LogEvent is to be inserted.
        /// </param>
        /// <param name="value">
        /// The LogEvent to insert.
        /// </param>
        public virtual void Insert(int index, LogEvent value)
        {
            this.List.Insert(index, value);
        }

        /// <summary>
        /// Gets or sets the LogEvent at the given index in this LogEventCollection.
        /// </summary>
        public virtual LogEvent this[int index]
        {
            get
            {
                return (LogEvent) this.List[index];
            }
            set
            {
                this.List[index] = value;
            }
        }

        /// <summary>
        /// Removes the first occurrence of a specific LogEvent from this LogEventCollection.
        /// </summary>
        /// <param name="value">
        /// The LogEvent value to remove from this LogEventCollection.
        /// </param>
        public virtual void Remove(LogEvent value)
        {
            this.List.Remove(value);
        }

        /// <summary>
        /// Type-specific enumeration class, used by LogEventCollection.GetEnumerator.
        /// </summary>
        public class Enumerator: System.Collections.IEnumerator
        {
            private System.Collections.IEnumerator wrapped;

            public Enumerator(LogEventCollection collection)
            {
                this.wrapped = ((System.Collections.CollectionBase)collection).GetEnumerator();
            }

            public LogEvent Current
            {
                get
                {
                    return (LogEvent) (this.wrapped.Current);
                }
            }

            object System.Collections.IEnumerator.Current
            {
                get
                {
                    return (LogEvent) (this.wrapped.Current);
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
        /// Returns an enumerator that can iterate through the elements of this LogEventCollection.
        /// </summary>
        /// <returns>
        /// An object that implements System.Collections.IEnumerator.
        /// </returns>        
        public new virtual LogEventCollection.Enumerator GetEnumerator()
        {
            return new LogEventCollection.Enumerator(this);
        }
    }
}
