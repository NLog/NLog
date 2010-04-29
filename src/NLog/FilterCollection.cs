// 
// Copyright (c) 2004-2006 Jaroslaw Kowalski <jaak@jkowalski.net>
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

using System;
using System.Collections;
using System.Text;

namespace NLog
{
    // CLOVER:OFF
    /// <summary>
    /// A collection of elements of type Filter
    /// </summary>
    public class FilterCollection: System.Collections.CollectionBase
    {
        /// <summary>
        /// Initializes a new empty instance of the FilterCollection class.
        /// </summary>
        public FilterCollection()
        {
            // empty
        }

        /// <summary>
        /// Initializes a new instance of the FilterCollection class, containing elements
        /// copied from an array.
        /// </summary>
        /// <param name="items">
        /// The array whose elements are to be added to the new FilterCollection.
        /// </param>
        public FilterCollection(Filter[]items)
        {
            this.AddRange(items);
        }

        /// <summary>
        /// Initializes a new instance of the FilterCollection class, containing elements
        /// copied from another instance of FilterCollection
        /// </summary>
        /// <param name="items">
        /// The FilterCollection whose elements are to be added to the new FilterCollection.
        /// </param>
        public FilterCollection(FilterCollection items)
        {
            this.AddRange(items);
        }

        /// <summary>
        /// Adds the elements of an array to the end of this FilterCollection.
        /// </summary>
        /// <param name="items">
        /// The array whose elements are to be added to the end of this FilterCollection.
        /// </param>
        public virtual void AddRange(Filter[]items)
        {
            foreach (Filter item in items)
            {
                this.List.Add(item);
            }
        }

        /// <summary>
        /// Adds the elements of another FilterCollection to the end of this FilterCollection.
        /// </summary>
        /// <param name="items">
        /// The FilterCollection whose elements are to be added to the end of this FilterCollection.
        /// </param>
        public virtual void AddRange(FilterCollection items)
        {
            foreach (Filter item in items)
            {
                this.List.Add(item);
            }
        }

        /// <summary>
        /// Adds an instance of type Filter to the end of this FilterCollection.
        /// </summary>
        /// <param name="value">
        /// The Filter to be added to the end of this FilterCollection.
        /// </param>
        public virtual void Add(Filter value)
        {
            this.List.Add(value);
        }

        /// <summary>
        /// Determines whether a specfic Filter value is in this FilterCollection.
        /// </summary>
        /// <param name="value">
        /// The Filter value to locate in this FilterCollection.
        /// </param>
        /// <returns>
        /// true if value is found in this FilterCollection;
        /// false otherwise.
        /// </returns>
        public virtual bool Contains(Filter value)
        {
            return this.List.Contains(value);
        }

        /// <summary>
        /// Return the zero-based index of the first occurrence of a specific value
        /// in this FilterCollection
        /// </summary>
        /// <param name="value">
        /// The Filter value to locate in the FilterCollection.
        /// </param>
        /// <returns>
        /// The zero-based index of the first occurrence of the _ELEMENT value if found;
        /// -1 otherwise.
        /// </returns>
        public virtual int IndexOf(Filter value)
        {
            return this.List.IndexOf(value);
        }

        /// <summary>
        /// Inserts an element into the FilterCollection at the specified index
        /// </summary>
        /// <param name="index">
        /// The index at which the Filter is to be inserted.
        /// </param>
        /// <param name="value">
        /// The Filter to insert.
        /// </param>
        public virtual void Insert(int index, Filter value)
        {
            this.List.Insert(index, value);
        }

        /// <summary>
        /// Gets or sets the Filter at the given index in this FilterCollection.
        /// </summary>
        public virtual Filter this[int index]
        {
            get { return (Filter)this.List[index]; }
            set { this.List[index] = value; }
        }

        /// <summary>
        /// Removes the first occurrence of a specific Filter from this FilterCollection.
        /// </summary>
        /// <param name="value">
        /// The Filter value to remove from this FilterCollection.
        /// </param>
        public virtual void Remove(Filter value)
        {
            this.List.Remove(value);
        }

        /// <summary>
        /// Type-specific enumeration class, used by FilterCollection.GetEnumerator.
        /// </summary>
        public class Enumerator: System.Collections.IEnumerator
        {
            private System.Collections.IEnumerator wrapped;

            /// <summary>
            /// 
            /// </summary>
            /// <param name="collection"></param>
            public Enumerator(FilterCollection collection)
            {
                this.wrapped = ((System.Collections.CollectionBase)collection).GetEnumerator();
            }

            /// <summary>
            /// 
            /// </summary>
            public Filter Current
            {
                get { return (Filter)(this.wrapped.Current); }
            }

            /// <summary>
            /// 
            /// </summary>
            object System.Collections.IEnumerator.Current
            {
                get { return (Filter)(this.wrapped.Current); }
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
        /// Returns an enumerator that can iterate through the elements of this FilterCollection.
        /// </summary>
        /// <returns>
        /// An object that implements System.Collections.IEnumerator.
        /// </returns>        
        public new virtual FilterCollection.Enumerator GetEnumerator()
        {
            return new FilterCollection.Enumerator(this);
        }
    }
}
