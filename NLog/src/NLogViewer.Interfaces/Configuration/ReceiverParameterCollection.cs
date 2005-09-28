using System;

namespace NLogViewer.Configuration
{
    /// <summary>
    /// A collection of elements of type ReceiverParameter
    /// </summary>
    public class ReceiverParameterCollection: System.Collections.CollectionBase
    {
        /// <summary>
        /// Initializes a new empty instance of the ReceiverParameterCollection class.
        /// </summary>
        public ReceiverParameterCollection()
        {
            // empty
        }

        /// <summary>
        /// Initializes a new instance of the ReceiverParameterCollection class, containing elements
        /// copied from an array.
        /// </summary>
        /// <param name="items">
        /// The array whose elements are to be added to the new ReceiverParameterCollection.
        /// </param>
        public ReceiverParameterCollection(ReceiverParameter[] items)
        {
            this.AddRange(items);
        }

        /// <summary>
        /// Initializes a new instance of the ReceiverParameterCollection class, containing elements
        /// copied from another instance of ReceiverParameterCollection
        /// </summary>
        /// <param name="items">
        /// The ReceiverParameterCollection whose elements are to be added to the new ReceiverParameterCollection.
        /// </param>
        public ReceiverParameterCollection(ReceiverParameterCollection items)
        {
            this.AddRange(items);
        }

        /// <summary>
        /// Adds the elements of an array to the end of this ReceiverParameterCollection.
        /// </summary>
        /// <param name="items">
        /// The array whose elements are to be added to the end of this ReceiverParameterCollection.
        /// </param>
        public virtual void AddRange(ReceiverParameter[] items)
        {
            foreach (ReceiverParameter item in items)
            {
                this.List.Add(item);
            }
        }

        /// <summary>
        /// Adds the elements of another ReceiverParameterCollection to the end of this ReceiverParameterCollection.
        /// </summary>
        /// <param name="items">
        /// The ReceiverParameterCollection whose elements are to be added to the end of this ReceiverParameterCollection.
        /// </param>
        public virtual void AddRange(ReceiverParameterCollection items)
        {
            foreach (ReceiverParameter item in items)
            {
                this.List.Add(item);
            }
        }

        /// <summary>
        /// Adds an instance of type ReceiverParameter to the end of this ReceiverParameterCollection.
        /// </summary>
        /// <param name="value">
        /// The ReceiverParameter to be added to the end of this ReceiverParameterCollection.
        /// </param>
        public virtual void Add(ReceiverParameter value)
        {
            this.List.Add(value);
        }

        /// <summary>
        /// Determines whether a specfic ReceiverParameter value is in this ReceiverParameterCollection.
        /// </summary>
        /// <param name="value">
        /// The ReceiverParameter value to locate in this ReceiverParameterCollection.
        /// </param>
        /// <returns>
        /// true if value is found in this ReceiverParameterCollection;
        /// false otherwise.
        /// </returns>
        public virtual bool Contains(ReceiverParameter value)
        {
            return this.List.Contains(value);
        }

        /// <summary>
        /// Return the zero-based index of the first occurrence of a specific value
        /// in this ReceiverParameterCollection
        /// </summary>
        /// <param name="value">
        /// The ReceiverParameter value to locate in the ReceiverParameterCollection.
        /// </param>
        /// <returns>
        /// The zero-based index of the first occurrence of the _ELEMENT value if found;
        /// -1 otherwise.
        /// </returns>
        public virtual int IndexOf(ReceiverParameter value)
        {
            return this.List.IndexOf(value);
        }

        /// <summary>
        /// Inserts an element into the ReceiverParameterCollection at the specified index
        /// </summary>
        /// <param name="index">
        /// The index at which the ReceiverParameter is to be inserted.
        /// </param>
        /// <param name="value">
        /// The ReceiverParameter to insert.
        /// </param>
        public virtual void Insert(int index, ReceiverParameter value)
        {
            this.List.Insert(index, value);
        }

        /// <summary>
        /// Gets or sets the ReceiverParameter at the given index in this ReceiverParameterCollection.
        /// </summary>
        public virtual ReceiverParameter this[int index]
        {
            get
            {
                return (ReceiverParameter) this.List[index];
            }
            set
            {
                this.List[index] = value;
            }
        }

        /// <summary>
        /// Removes the first occurrence of a specific ReceiverParameter from this ReceiverParameterCollection.
        /// </summary>
        /// <param name="value">
        /// The ReceiverParameter value to remove from this ReceiverParameterCollection.
        /// </param>
        public virtual void Remove(ReceiverParameter value)
        {
            this.List.Remove(value);
        }

        /// <summary>
        /// Type-specific enumeration class, used by ReceiverParameterCollection.GetEnumerator.
        /// </summary>
        public class Enumerator: System.Collections.IEnumerator
        {
            private System.Collections.IEnumerator wrapped;

            /// <summary>
            /// 
            /// </summary>
            /// <param name="collection"></param>
            public Enumerator(ReceiverParameterCollection collection)
            {
                this.wrapped = ((System.Collections.CollectionBase)collection).GetEnumerator();
            }

            /// <summary>
            /// 
            /// </summary>
            public ReceiverParameter Current
            {
                get
                {
                    return (ReceiverParameter) (this.wrapped.Current);
                }
            }

            /// <summary>
            /// 
            /// </summary>
            object System.Collections.IEnumerator.Current
            {
                get
                {
                    return (ReceiverParameter) (this.wrapped.Current);
                }
            }

            /// <summary>
            /// 
            /// </summary>
            /// <returns></returns>
            public bool MoveNext()
            {
                return this.wrapped.MoveNext();
            }

            /// <summary>
            /// 
            /// </summary>
            public void Reset()
            {
                this.wrapped.Reset();
            }
        }

        /// <summary>
        /// Returns an enumerator that can iterate through the elements of this ReceiverParameterCollection.
        /// </summary>
        /// <returns>
        /// An object that implements System.Collections.IEnumerator.
        /// </returns>        
        public new virtual ReceiverParameterCollection.Enumerator GetEnumerator()
        {
            return new ReceiverParameterCollection.Enumerator(this);
        }
    }
}
