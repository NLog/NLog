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

namespace NLog.Targets
{
    // CLOVER:OFF
    /// <summary>
    /// A collection of elements of type MethodCallParameter
    /// </summary>
    public class MethodCallParameterCollection: System.Collections.CollectionBase
    {
        /// <summary>
        /// Initializes a new empty instance of the MethodCallParameterCollection class.
        /// </summary>
        public MethodCallParameterCollection()
        {
            // empty
        }

        /// <summary>
        /// Initializes a new instance of the MethodCallParameterCollection class, containing elements
        /// copied from an array.
        /// </summary>
        /// <param name="items">
        /// The array whose elements are to be added to the new MethodCallParameterCollection.
        /// </param>
        public MethodCallParameterCollection(MethodCallParameter[]items)
        {
            this.AddRange(items);
        }

        /// <summary>
        /// Initializes a new instance of the MethodCallParameterCollection class, containing elements
        /// copied from another instance of MethodCallParameterCollection
        /// </summary>
        /// <param name="items">
        /// The MethodCallParameterCollection whose elements are to be added to the new MethodCallParameterCollection.
        /// </param>
        public MethodCallParameterCollection(MethodCallParameterCollection items)
        {
            this.AddRange(items);
        }

        /// <summary>
        /// Adds the elements of an array to the end of this MethodCallParameterCollection.
        /// </summary>
        /// <param name="items">
        /// The array whose elements are to be added to the end of this MethodCallParameterCollection.
        /// </param>
        public virtual void AddRange(MethodCallParameter[]items)
        {
            foreach (MethodCallParameter item in items)
            {
                this.List.Add(item);
            }
        }

        /// <summary>
        /// Adds the elements of another MethodCallParameterCollection to the end of this MethodCallParameterCollection.
        /// </summary>
        /// <param name="items">
        /// The MethodCallParameterCollection whose elements are to be added to the end of this MethodCallParameterCollection.
        /// </param>
        public virtual void AddRange(MethodCallParameterCollection items)
        {
            foreach (MethodCallParameter item in items)
            {
                this.List.Add(item);
            }
        }

        /// <summary>
        /// Adds an instance of type MethodCallParameter to the end of this MethodCallParameterCollection.
        /// </summary>
        /// <param name="value">
        /// The MethodCallParameter to be added to the end of this MethodCallParameterCollection.
        /// </param>
        public virtual void Add(MethodCallParameter value)
        {
            this.List.Add(value);
        }

        /// <summary>
        /// Determines whether a specfic MethodCallParameter value is in this MethodCallParameterCollection.
        /// </summary>
        /// <param name="value">
        /// The MethodCallParameter value to locate in this MethodCallParameterCollection.
        /// </param>
        /// <returns>
        /// true if value is found in this MethodCallParameterCollection;
        /// false otherwise.
        /// </returns>
        public virtual bool Contains(MethodCallParameter value)
        {
            return this.List.Contains(value);
        }

        /// <summary>
        /// Return the zero-based index of the first occurrence of a specific value
        /// in this MethodCallParameterCollection
        /// </summary>
        /// <param name="value">
        /// The MethodCallParameter value to locate in the MethodCallParameterCollection.
        /// </param>
        /// <returns>
        /// The zero-based index of the first occurrence of the _ELEMENT value if found;
        /// -1 otherwise.
        /// </returns>
        public virtual int IndexOf(MethodCallParameter value)
        {
            return this.List.IndexOf(value);
        }

        /// <summary>
        /// Inserts an element into the MethodCallParameterCollection at the specified index
        /// </summary>
        /// <param name="index">
        /// The index at which the MethodCallParameter is to be inserted.
        /// </param>
        /// <param name="value">
        /// The MethodCallParameter to insert.
        /// </param>
        public virtual void Insert(int index, MethodCallParameter value)
        {
            this.List.Insert(index, value);
        }

        /// <summary>
        /// Gets or sets the MethodCallParameter at the given index in this MethodCallParameterCollection.
        /// </summary>
        public virtual MethodCallParameter this[int index]
        {
            get { return (MethodCallParameter)this.List[index]; }
            set { this.List[index] = value; }
        }

        /// <summary>
        /// Removes the first occurrence of a specific MethodCallParameter from this MethodCallParameterCollection.
        /// </summary>
        /// <param name="value">
        /// The MethodCallParameter value to remove from this MethodCallParameterCollection.
        /// </param>
        public virtual void Remove(MethodCallParameter value)
        {
            this.List.Remove(value);
        }

        /// <summary>
        /// Type-specific enumeration class, used by MethodCallParameterCollection.GetEnumerator.
        /// </summary>
        public class Enumerator: System.Collections.IEnumerator
        {
            private System.Collections.IEnumerator wrapped;

            /// <summary>
            /// 
            /// </summary>
            /// <param name="collection"></param>
            public Enumerator(MethodCallParameterCollection collection)
            {
                this.wrapped = ((System.Collections.CollectionBase)collection).GetEnumerator();
            }

            /// <summary>
            /// 
            /// </summary>
            public MethodCallParameter Current
            {
                get { return (MethodCallParameter)(this.wrapped.Current); }
            }

            /// <summary>
            /// 
            /// </summary>
            object System.Collections.IEnumerator.Current
            {
                get { return (MethodCallParameter)(this.wrapped.Current); }
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
        /// Returns an enumerator that can iterate through the elements of this MethodCallParameterCollection.
        /// </summary>
        /// <returns>
        /// An object that implements System.Collections.IEnumerator.
        /// </returns>        
        public new virtual MethodCallParameterCollection.Enumerator GetEnumerator()
        {
            return new MethodCallParameterCollection.Enumerator(this);
        }
    }
}
