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
using System.Collections.Specialized;
using System.Diagnostics;
using System.Reflection;
using System.Globalization;

using NLog.Config;

namespace NLog.Layouts
{
    // CLOVER:OFF
    /// <summary>
    /// A collection of elements of type CsvFileColumn
    /// </summary>
    public class CsvColumnCollection: System.Collections.CollectionBase
    {
        /// <summary>
        /// Initializes a new empty instance of the CsvFileColumnCollection class.
        /// </summary>
        public CsvColumnCollection()
        {
            // empty
        }

        /// <summary>
        /// Initializes a new instance of the CsvFileColumnCollection class, containing elements
        /// copied from an array.
        /// </summary>
        /// <param name="items">
        /// The array whose elements are to be added to the new CsvFileColumnCollection.
        /// </param>
        public CsvColumnCollection(CsvColumn[]items)
        {
            this.AddRange(items);
        }

        /// <summary>
        /// Initializes a new instance of the CsvFileColumnCollection class, containing elements
        /// copied from another instance of CsvFileColumnCollection
        /// </summary>
        /// <param name="items">
        /// The CsvFileColumnCollection whose elements are to be added to the new CsvFileColumnCollection.
        /// </param>
        public CsvColumnCollection(CsvColumnCollection items)
        {
            this.AddRange(items);
        }

        /// <summary>
        /// Adds the elements of an array to the end of this CsvFileColumnCollection.
        /// </summary>
        /// <param name="items">
        /// The array whose elements are to be added to the end of this CsvFileColumnCollection.
        /// </param>
        public virtual void AddRange(CsvColumn[]items)
        {
            foreach (CsvColumn item in items)
            {
                this.List.Add(item);
            }
        }

        /// <summary>
        /// Adds the elements of another CsvFileColumnCollection to the end of this CsvFileColumnCollection.
        /// </summary>
        /// <param name="items">
        /// The CsvFileColumnCollection whose elements are to be added to the end of this CsvFileColumnCollection.
        /// </param>
        public virtual void AddRange(CsvColumnCollection items)
        {
            foreach (CsvColumn item in items)
            {
                this.List.Add(item);
            }
        }

        /// <summary>
        /// Adds an instance of type CsvFileColumn to the end of this CsvFileColumnCollection.
        /// </summary>
        /// <param name="value">
        /// The CsvFileColumn to be added to the end of this CsvFileColumnCollection.
        /// </param>
        public virtual void Add(CsvColumn value)
        {
            this.List.Add(value);
        }

        /// <summary>
        /// Determines whether a specfic CsvFileColumn value is in this CsvFileColumnCollection.
        /// </summary>
        /// <param name="value">
        /// The CsvFileColumn value to locate in this CsvFileColumnCollection.
        /// </param>
        /// <returns>
        /// true if value is found in this CsvFileColumnCollection;
        /// false otherwise.
        /// </returns>
        public virtual bool Contains(CsvColumn value)
        {
            return this.List.Contains(value);
        }

        /// <summary>
        /// Return the zero-based index of the first occurrence of a specific value
        /// in this CsvFileColumnCollection
        /// </summary>
        /// <param name="value">
        /// The CsvFileColumn value to locate in the CsvFileColumnCollection.
        /// </param>
        /// <returns>
        /// The zero-based index of the first occurrence of the _ELEMENT value if found;
        /// -1 otherwise.
        /// </returns>
        public virtual int IndexOf(CsvColumn value)
        {
            return this.List.IndexOf(value);
        }

        /// <summary>
        /// Inserts an element into the CsvFileColumnCollection at the specified index
        /// </summary>
        /// <param name="index">
        /// The index at which the CsvFileColumn is to be inserted.
        /// </param>
        /// <param name="value">
        /// The CsvFileColumn to insert.
        /// </param>
        public virtual void Insert(int index, CsvColumn value)
        {
            this.List.Insert(index, value);
        }

        /// <summary>
        /// Gets or sets the CsvFileColumn at the given index in this CsvFileColumnCollection.
        /// </summary>
        public virtual CsvColumn this[int index]
        {
            get { return (CsvColumn)this.List[index]; }
            set { this.List[index] = value; }
        }

        /// <summary>
        /// Removes the first occurrence of a specific CsvFileColumn from this CsvFileColumnCollection.
        /// </summary>
        /// <param name="value">
        /// The CsvFileColumn value to remove from this CsvFileColumnCollection.
        /// </param>
        public virtual void Remove(CsvColumn value)
        {
            this.List.Remove(value);
        }

        /// <summary>
        /// Type-specific enumeration class, used by CsvFileColumnCollection.GetEnumerator.
        /// </summary>
        public class Enumerator: System.Collections.IEnumerator
        {
            private System.Collections.IEnumerator wrapped;

            /// <summary>
            /// 
            /// </summary>
            /// <param name="collection"></param>
            public Enumerator(CsvColumnCollection collection)
            {
                this.wrapped = ((System.Collections.CollectionBase)collection).GetEnumerator();
            }

            /// <summary>
            /// 
            /// </summary>
            public CsvColumn Current
            {
                get { return (CsvColumn)(this.wrapped.Current); }
            }

            /// <summary>
            /// 
            /// </summary>
            object System.Collections.IEnumerator.Current
            {
                get { return (CsvColumn)(this.wrapped.Current); }
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
        /// Returns an enumerator that can iterate through the elements of this CsvFileColumnCollection.
        /// </summary>
        /// <returns>
        /// An object that implements System.Collections.IEnumerator.
        /// </returns>        
        public new virtual CsvColumnCollection.Enumerator GetEnumerator()
        {
            return new CsvColumnCollection.Enumerator(this);
        }
    }
}
