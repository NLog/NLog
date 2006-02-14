using System;

namespace NLogViewer.Receivers
{
    /// <summary>
    /// A collection of elements of type LogEventReceiverInfo
    /// </summary>
    public class LogEventReceiverInfoCollection: System.Collections.CollectionBase
    {
        /// <summary>
        /// Initializes a new empty instance of the LogEventReceiverInfoCollection class.
        /// </summary>
        public LogEventReceiverInfoCollection()
        {
            // empty
        }

        /// <summary>
        /// Initializes a new instance of the LogEventReceiverInfoCollection class, containing elements
        /// copied from an array.
        /// </summary>
        /// <param name="items">
        /// The array whose elements are to be added to the new LogEventReceiverInfoCollection.
        /// </param>
        public LogEventReceiverInfoCollection(LogEventReceiverInfo[] items)
        {
            this.AddRange(items);
        }

        /// <summary>
        /// Initializes a new instance of the LogEventReceiverInfoCollection class, containing elements
        /// copied from another instance of LogEventReceiverInfoCollection
        /// </summary>
        /// <param name="items">
        /// The LogEventReceiverInfoCollection whose elements are to be added to the new LogEventReceiverInfoCollection.
        /// </param>
        public LogEventReceiverInfoCollection(LogEventReceiverInfoCollection items)
        {
            this.AddRange(items);
        }

        /// <summary>
        /// Adds the elements of an array to the end of this LogEventReceiverInfoCollection.
        /// </summary>
        /// <param name="items">
        /// The array whose elements are to be added to the end of this LogEventReceiverInfoCollection.
        /// </param>
        public virtual void AddRange(LogEventReceiverInfo[] items)
        {
            foreach (LogEventReceiverInfo item in items)
            {
                this.List.Add(item);
            }
        }

        /// <summary>
        /// Adds the elements of another LogEventReceiverInfoCollection to the end of this LogEventReceiverInfoCollection.
        /// </summary>
        /// <param name="items">
        /// The LogEventReceiverInfoCollection whose elements are to be added to the end of this LogEventReceiverInfoCollection.
        /// </param>
        public virtual void AddRange(LogEventReceiverInfoCollection items)
        {
            foreach (LogEventReceiverInfo item in items)
            {
                this.List.Add(item);
            }
        }

        /// <summary>
        /// Adds an instance of type LogEventReceiverInfo to the end of this LogEventReceiverInfoCollection.
        /// </summary>
        /// <param name="value">
        /// The LogEventReceiverInfo to be added to the end of this LogEventReceiverInfoCollection.
        /// </param>
        public virtual void Add(LogEventReceiverInfo value)
        {
            this.List.Add(value);
        }

        /// <summary>
        /// Determines whether a specfic LogEventReceiverInfo value is in this LogEventReceiverInfoCollection.
        /// </summary>
        /// <param name="value">
        /// The LogEventReceiverInfo value to locate in this LogEventReceiverInfoCollection.
        /// </param>
        /// <returns>
        /// true if value is found in this LogEventReceiverInfoCollection;
        /// false otherwise.
        /// </returns>
        public virtual bool Contains(LogEventReceiverInfo value)
        {
            return this.List.Contains(value);
        }

        /// <summary>
        /// Return the zero-based index of the first occurrence of a specific value
        /// in this LogEventReceiverInfoCollection
        /// </summary>
        /// <param name="value">
        /// The LogEventReceiverInfo value to locate in the LogEventReceiverInfoCollection.
        /// </param>
        /// <returns>
        /// The zero-based index of the first occurrence of the _ELEMENT value if found;
        /// -1 otherwise.
        /// </returns>
        public virtual int IndexOf(LogEventReceiverInfo value)
        {
            return this.List.IndexOf(value);
        }

        /// <summary>
        /// Inserts an element into the LogEventReceiverInfoCollection at the specified index
        /// </summary>
        /// <param name="index">
        /// The index at which the LogEventReceiverInfo is to be inserted.
        /// </param>
        /// <param name="value">
        /// The LogEventReceiverInfo to insert.
        /// </param>
        public virtual void Insert(int index, LogEventReceiverInfo value)
        {
            this.List.Insert(index, value);
        }

        /// <summary>
        /// Gets or sets the LogEventReceiverInfo at the given index in this LogEventReceiverInfoCollection.
        /// </summary>
        public virtual LogEventReceiverInfo this[int index]
        {
            get
            {
                return (LogEventReceiverInfo) this.List[index];
            }
            set
            {
                this.List[index] = value;
            }
        }

        /// <summary>
        /// Removes the first occurrence of a specific LogEventReceiverInfo from this LogEventReceiverInfoCollection.
        /// </summary>
        /// <param name="value">
        /// The LogEventReceiverInfo value to remove from this LogEventReceiverInfoCollection.
        /// </param>
        public virtual void Remove(LogEventReceiverInfo value)
        {
            this.List.Remove(value);
        }

        /// <summary>
        /// Type-specific enumeration class, used by LogEventReceiverInfoCollection.GetEnumerator.
        /// </summary>
        public class Enumerator: System.Collections.IEnumerator
        {
            private System.Collections.IEnumerator wrapped;

            public Enumerator(LogEventReceiverInfoCollection collection)
            {
                this.wrapped = ((System.Collections.CollectionBase)collection).GetEnumerator();
            }

            public LogEventReceiverInfo Current
            {
                get
                {
                    return (LogEventReceiverInfo) (this.wrapped.Current);
                }
            }

            object System.Collections.IEnumerator.Current
            {
                get
                {
                    return (LogEventReceiverInfo) (this.wrapped.Current);
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
        /// Returns an enumerator that can iterate through the elements of this LogEventReceiverInfoCollection.
        /// </summary>
        /// <returns>
        /// An object that implements System.Collections.IEnumerator.
        /// </returns>        
        public new virtual LogEventReceiverInfoCollection.Enumerator GetEnumerator()
        {
            return new LogEventReceiverInfoCollection.Enumerator(this);
        }
    }
}
