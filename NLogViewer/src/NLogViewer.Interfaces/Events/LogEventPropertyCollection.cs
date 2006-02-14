using System;

namespace NLogViewer.Events
{
    /// <summary>
    /// A collection of elements of type LogEventProperty
    /// </summary>
    public class LogEventPropertyCollection: System.Collections.CollectionBase
    {
        /// <summary>
        /// Initializes a new empty instance of the LogEventPropertyCollection class.
        /// </summary>
        public LogEventPropertyCollection()
        {
            // empty
        }

        /// <summary>
        /// Initializes a new instance of the LogEventPropertyCollection class, containing elements
        /// copied from an array.
        /// </summary>
        /// <param name="items">
        /// The array whose elements are to be added to the new LogEventPropertyCollection.
        /// </param>
        public LogEventPropertyCollection(LogEventProperty[] items)
        {
            this.AddRange(items);
        }

        /// <summary>
        /// Initializes a new instance of the LogEventPropertyCollection class, containing elements
        /// copied from another instance of LogEventPropertyCollection
        /// </summary>
        /// <param name="items">
        /// The LogEventPropertyCollection whose elements are to be added to the new LogEventPropertyCollection.
        /// </param>
        public LogEventPropertyCollection(LogEventPropertyCollection items)
        {
            this.AddRange(items);
        }

        /// <summary>
        /// Adds the elements of an array to the end of this LogEventPropertyCollection.
        /// </summary>
        /// <param name="items">
        /// The array whose elements are to be added to the end of this LogEventPropertyCollection.
        /// </param>
        public virtual void AddRange(LogEventProperty[] items)
        {
            foreach (LogEventProperty item in items)
            {
                this.List.Add(item);
            }
        }

        /// <summary>
        /// Adds the elements of another LogEventPropertyCollection to the end of this LogEventPropertyCollection.
        /// </summary>
        /// <param name="items">
        /// The LogEventPropertyCollection whose elements are to be added to the end of this LogEventPropertyCollection.
        /// </param>
        public virtual void AddRange(LogEventPropertyCollection items)
        {
            foreach (LogEventProperty item in items)
            {
                this.List.Add(item);
            }
        }

        /// <summary>
        /// Adds an instance of type LogEventProperty to the end of this LogEventPropertyCollection.
        /// </summary>
        /// <param name="value">
        /// The LogEventProperty to be added to the end of this LogEventPropertyCollection.
        /// </param>
        public virtual void Add(LogEventProperty value)
        {
            this.List.Add(value);
        }

        /// <summary>
        /// Determines whether a specfic LogEventProperty value is in this LogEventPropertyCollection.
        /// </summary>
        /// <param name="value">
        /// The LogEventProperty value to locate in this LogEventPropertyCollection.
        /// </param>
        /// <returns>
        /// true if value is found in this LogEventPropertyCollection;
        /// false otherwise.
        /// </returns>
        public virtual bool Contains(LogEventProperty value)
        {
            return this.List.Contains(value);
        }

        /// <summary>
        /// Return the zero-based index of the first occurrence of a specific value
        /// in this LogEventPropertyCollection
        /// </summary>
        /// <param name="value">
        /// The LogEventProperty value to locate in the LogEventPropertyCollection.
        /// </param>
        /// <returns>
        /// The zero-based index of the first occurrence of the _ELEMENT value if found;
        /// -1 otherwise.
        /// </returns>
        public virtual int IndexOf(LogEventProperty value)
        {
            return this.List.IndexOf(value);
        }

        /// <summary>
        /// Inserts an element into the LogEventPropertyCollection at the specified index
        /// </summary>
        /// <param name="index">
        /// The index at which the LogEventProperty is to be inserted.
        /// </param>
        /// <param name="value">
        /// The LogEventProperty to insert.
        /// </param>
        public virtual void Insert(int index, LogEventProperty value)
        {
            this.List.Insert(index, value);
        }

        /// <summary>
        /// Gets or sets the LogEventProperty at the given index in this LogEventPropertyCollection.
        /// </summary>
        public virtual LogEventProperty this[int index]
        {
            get
            {
                return (LogEventProperty) this.List[index];
            }
            set
            {
                this.List[index] = value;
            }
        }

        /// <summary>
        /// Removes the first occurrence of a specific LogEventProperty from this LogEventPropertyCollection.
        /// </summary>
        /// <param name="value">
        /// The LogEventProperty value to remove from this LogEventPropertyCollection.
        /// </param>
        public virtual void Remove(LogEventProperty value)
        {
            this.List.Remove(value);
        }

        /// <summary>
        /// Type-specific enumeration class, used by LogEventPropertyCollection.GetEnumerator.
        /// </summary>
        public class Enumerator: System.Collections.IEnumerator
        {
            private System.Collections.IEnumerator wrapped;

            public Enumerator(LogEventPropertyCollection collection)
            {
                this.wrapped = ((System.Collections.CollectionBase)collection).GetEnumerator();
            }

            public LogEventProperty Current
            {
                get
                {
                    return (LogEventProperty) (this.wrapped.Current);
                }
            }

            object System.Collections.IEnumerator.Current
            {
                get
                {
                    return (LogEventProperty) (this.wrapped.Current);
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
        /// Returns an enumerator that can iterate through the elements of this LogEventPropertyCollection.
        /// </summary>
        /// <returns>
        /// An object that implements System.Collections.IEnumerator.
        /// </returns>        
        public new virtual LogEventPropertyCollection.Enumerator GetEnumerator()
        {
            return new LogEventPropertyCollection.Enumerator(this);
        }
    }
}
