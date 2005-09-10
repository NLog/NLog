using System;

namespace NLogViewer
{
    /// <summary>
    /// A collection of elements of type LogInstance
    /// </summary>
    public class LogInstanceCollection: System.Collections.CollectionBase
    {
        /// <summary>
        /// Initializes a new empty instance of the LogInstanceCollection class.
        /// </summary>
        public LogInstanceCollection()
        {
            // empty
        }

        /// <summary>
        /// Initializes a new instance of the LogInstanceCollection class, containing elements
        /// copied from an array.
        /// </summary>
        /// <param name="items">
        /// The array whose elements are to be added to the new LogInstanceCollection.
        /// </param>
        public LogInstanceCollection(LogInstance[] items)
        {
            this.AddRange(items);
        }

        /// <summary>
        /// Initializes a new instance of the LogInstanceCollection class, containing elements
        /// copied from another instance of LogInstanceCollection
        /// </summary>
        /// <param name="items">
        /// The LogInstanceCollection whose elements are to be added to the new LogInstanceCollection.
        /// </param>
        public LogInstanceCollection(LogInstanceCollection items)
        {
            this.AddRange(items);
        }

        /// <summary>
        /// Adds the elements of an array to the end of this LogInstanceCollection.
        /// </summary>
        /// <param name="items">
        /// The array whose elements are to be added to the end of this LogInstanceCollection.
        /// </param>
        public virtual void AddRange(LogInstance[] items)
        {
            foreach (LogInstance item in items)
            {
                this.List.Add(item);
            }
        }

        /// <summary>
        /// Adds the elements of another LogInstanceCollection to the end of this LogInstanceCollection.
        /// </summary>
        /// <param name="items">
        /// The LogInstanceCollection whose elements are to be added to the end of this LogInstanceCollection.
        /// </param>
        public virtual void AddRange(LogInstanceCollection items)
        {
            foreach (LogInstance item in items)
            {
                this.List.Add(item);
            }
        }

        /// <summary>
        /// Adds an instance of type LogInstance to the end of this LogInstanceCollection.
        /// </summary>
        /// <param name="value">
        /// The LogInstance to be added to the end of this LogInstanceCollection.
        /// </param>
        public virtual void Add(LogInstance value)
        {
            this.List.Add(value);
        }

        /// <summary>
        /// Determines whether a specfic LogInstance value is in this LogInstanceCollection.
        /// </summary>
        /// <param name="value">
        /// The LogInstance value to locate in this LogInstanceCollection.
        /// </param>
        /// <returns>
        /// true if value is found in this LogInstanceCollection;
        /// false otherwise.
        /// </returns>
        public virtual bool Contains(LogInstance value)
        {
            return this.List.Contains(value);
        }

        /// <summary>
        /// Return the zero-based index of the first occurrence of a specific value
        /// in this LogInstanceCollection
        /// </summary>
        /// <param name="value">
        /// The LogInstance value to locate in the LogInstanceCollection.
        /// </param>
        /// <returns>
        /// The zero-based index of the first occurrence of the _ELEMENT value if found;
        /// -1 otherwise.
        /// </returns>
        public virtual int IndexOf(LogInstance value)
        {
            return this.List.IndexOf(value);
        }

        /// <summary>
        /// Inserts an element into the LogInstanceCollection at the specified index
        /// </summary>
        /// <param name="index">
        /// The index at which the LogInstance is to be inserted.
        /// </param>
        /// <param name="value">
        /// The LogInstance to insert.
        /// </param>
        public virtual void Insert(int index, LogInstance value)
        {
            this.List.Insert(index, value);
        }

        /// <summary>
        /// Gets or sets the LogInstance at the given index in this LogInstanceCollection.
        /// </summary>
        public virtual LogInstance this[int index]
        {
            get
            {
                return (LogInstance) this.List[index];
            }
            set
            {
                this.List[index] = value;
            }
        }

        /// <summary>
        /// Removes the first occurrence of a specific LogInstance from this LogInstanceCollection.
        /// </summary>
        /// <param name="value">
        /// The LogInstance value to remove from this LogInstanceCollection.
        /// </param>
        public virtual void Remove(LogInstance value)
        {
            this.List.Remove(value);
        }

        /// <summary>
        /// Type-specific enumeration class, used by LogInstanceCollection.GetEnumerator.
        /// </summary>
        public class Enumerator: System.Collections.IEnumerator
        {
            private System.Collections.IEnumerator wrapped;

            public Enumerator(LogInstanceCollection collection)
            {
                this.wrapped = ((System.Collections.CollectionBase)collection).GetEnumerator();
            }

            public LogInstance Current
            {
                get
                {
                    return (LogInstance) (this.wrapped.Current);
                }
            }

            object System.Collections.IEnumerator.Current
            {
                get
                {
                    return (LogInstance) (this.wrapped.Current);
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
        /// Returns an enumerator that can iterate through the elements of this LogInstanceCollection.
        /// </summary>
        /// <returns>
        /// An object that implements System.Collections.IEnumerator.
        /// </returns>        
        public new virtual LogInstanceCollection.Enumerator GetEnumerator()
        {
            return new LogInstanceCollection.Enumerator(this);
        }
    }
}
