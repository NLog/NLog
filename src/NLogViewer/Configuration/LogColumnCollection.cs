using System;

namespace NLogViewer.Configuration
{
    /// <summary>
    /// A collection of elements of type LogColumn
    /// </summary>
    public class LogColumnCollection: System.Collections.CollectionBase
    {
        /// <summary>
        /// Initializes a new empty instance of the LogColumnCollection class.
        /// </summary>
        public LogColumnCollection()
        {
            // empty
        }

        /// <summary>
        /// Initializes a new instance of the LogColumnCollection class, containing elements
        /// copied from an array.
        /// </summary>
        /// <param name="items">
        /// The array whose elements are to be added to the new LogColumnCollection.
        /// </param>
        public LogColumnCollection(LogColumn[] items)
        {
            this.AddRange(items);
        }

        /// <summary>
        /// Initializes a new instance of the LogColumnCollection class, containing elements
        /// copied from another instance of LogColumnCollection
        /// </summary>
        /// <param name="items">
        /// The LogColumnCollection whose elements are to be added to the new LogColumnCollection.
        /// </param>
        public LogColumnCollection(LogColumnCollection items)
        {
            this.AddRange(items);
        }

        /// <summary>
        /// Adds the elements of an array to the end of this LogColumnCollection.
        /// </summary>
        /// <param name="items">
        /// The array whose elements are to be added to the end of this LogColumnCollection.
        /// </param>
        public virtual void AddRange(LogColumn[] items)
        {
            foreach (LogColumn item in items)
            {
                this.List.Add(item);
            }
        }

        /// <summary>
        /// Adds the elements of another LogColumnCollection to the end of this LogColumnCollection.
        /// </summary>
        /// <param name="items">
        /// The LogColumnCollection whose elements are to be added to the end of this LogColumnCollection.
        /// </param>
        public virtual void AddRange(LogColumnCollection items)
        {
            foreach (LogColumn item in items)
            {
                this.List.Add(item);
            }
        }

        /// <summary>
        /// Adds an instance of type LogColumn to the end of this LogColumnCollection.
        /// </summary>
        /// <param name="value">
        /// The LogColumn to be added to the end of this LogColumnCollection.
        /// </param>
        public virtual void Add(LogColumn value)
        {
            this.List.Add(value);
        }

        /// <summary>
        /// Determines whether a specfic LogColumn value is in this LogColumnCollection.
        /// </summary>
        /// <param name="value">
        /// The LogColumn value to locate in this LogColumnCollection.
        /// </param>
        /// <returns>
        /// true if value is found in this LogColumnCollection;
        /// false otherwise.
        /// </returns>
        public virtual bool Contains(LogColumn value)
        {
            return this.List.Contains(value);
        }

        /// <summary>
        /// Return the zero-based index of the first occurrence of a specific value
        /// in this LogColumnCollection
        /// </summary>
        /// <param name="value">
        /// The LogColumn value to locate in the LogColumnCollection.
        /// </param>
        /// <returns>
        /// The zero-based index of the first occurrence of the _ELEMENT value if found;
        /// -1 otherwise.
        /// </returns>
        public virtual int IndexOf(LogColumn value)
        {
            return this.List.IndexOf(value);
        }

        /// <summary>
        /// Inserts an element into the LogColumnCollection at the specified index
        /// </summary>
        /// <param name="index">
        /// The index at which the LogColumn is to be inserted.
        /// </param>
        /// <param name="value">
        /// The LogColumn to insert.
        /// </param>
        public virtual void Insert(int index, LogColumn value)
        {
            this.List.Insert(index, value);
        }

        /// <summary>
        /// Gets or sets the LogColumn at the given index in this LogColumnCollection.
        /// </summary>
        public virtual LogColumn this[int index]
        {
            get
            {
                return (LogColumn) this.List[index];
            }
            set
            {
                this.List[index] = value;
            }
        }

        /// <summary>
        /// Removes the first occurrence of a specific LogColumn from this LogColumnCollection.
        /// </summary>
        /// <param name="value">
        /// The LogColumn value to remove from this LogColumnCollection.
        /// </param>
        public virtual void Remove(LogColumn value)
        {
            this.List.Remove(value);
        }

        /// <summary>
        /// Type-specific enumeration class, used by LogColumnCollection.GetEnumerator.
        /// </summary>
        public class Enumerator: System.Collections.IEnumerator
        {
            private System.Collections.IEnumerator wrapped;

            public Enumerator(LogColumnCollection collection)
            {
                this.wrapped = ((System.Collections.CollectionBase)collection).GetEnumerator();
            }

            public LogColumn Current
            {
                get
                {
                    return (LogColumn) (this.wrapped.Current);
                }
            }

            object System.Collections.IEnumerator.Current
            {
                get
                {
                    return (LogColumn) (this.wrapped.Current);
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
        /// Returns an enumerator that can iterate through the elements of this LogColumnCollection.
        /// </summary>
        /// <returns>
        /// An object that implements System.Collections.IEnumerator.
        /// </returns>        
        public new virtual LogColumnCollection.Enumerator GetEnumerator()
        {
            return new LogColumnCollection.Enumerator(this);
        }
    }
}
