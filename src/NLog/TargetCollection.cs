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
    /// A collection of elements of type Target
    /// </summary>
    public class TargetCollection: System.Collections.CollectionBase
    {
        /// <summary>
        /// Initializes a new empty instance of the TargetCollection class.
        /// </summary>
        public TargetCollection()
        {
            // empty
        }

        /// <summary>
        /// Initializes a new instance of the TargetCollection class, containing elements
        /// copied from an array.
        /// </summary>
        /// <param name="items">
        /// The array whose elements are to be added to the new TargetCollection.
        /// </param>
        public TargetCollection(Target[]items)
        {
            this.AddRange(items);
        }

        /// <summary>
        /// Initializes a new instance of the TargetCollection class, containing elements
        /// copied from another instance of TargetCollection
        /// </summary>
        /// <param name="items">
        /// The TargetCollection whose elements are to be added to the new TargetCollection.
        /// </param>
        public TargetCollection(TargetCollection items)
        {
            this.AddRange(items);
        }

        /// <summary>
        /// Adds the elements of an array to the end of this TargetCollection.
        /// </summary>
        /// <param name="items">
        /// The array whose elements are to be added to the end of this TargetCollection.
        /// </param>
        public virtual void AddRange(Target[]items)
        {
            foreach (Target item in items)
            {
                this.List.Add(item);
            }
        }

        /// <summary>
        /// Adds the elements of another TargetCollection to the end of this TargetCollection.
        /// </summary>
        /// <param name="items">
        /// The TargetCollection whose elements are to be added to the end of this TargetCollection.
        /// </param>
        public virtual void AddRange(TargetCollection items)
        {
            foreach (Target item in items)
            {
                this.List.Add(item);
            }
        }

        /// <summary>
        /// Adds an instance of type Target to the end of this TargetCollection.
        /// </summary>
        /// <param name="value">
        /// The Target to be added to the end of this TargetCollection.
        /// </param>
        public virtual void Add(Target value)
        {
            this.List.Add(value);
        }

        /// <summary>
        /// Determines whether a specfic Target value is in this TargetCollection.
        /// </summary>
        /// <param name="value">
        /// The Target value to locate in this TargetCollection.
        /// </param>
        /// <returns>
        /// true if value is found in this TargetCollection;
        /// false otherwise.
        /// </returns>
        public virtual bool Contains(Target value)
        {
            return this.List.Contains(value);
        }

        /// <summary>
        /// Return the zero-based index of the first occurrence of a specific value
        /// in this TargetCollection
        /// </summary>
        /// <param name="value">
        /// The Target value to locate in the TargetCollection.
        /// </param>
        /// <returns>
        /// The zero-based index of the first occurrence of the _ELEMENT value if found;
        /// -1 otherwise.
        /// </returns>
        public virtual int IndexOf(Target value)
        {
            return this.List.IndexOf(value);
        }

        /// <summary>
        /// Inserts an element into the TargetCollection at the specified index
        /// </summary>
        /// <param name="index">
        /// The index at which the Target is to be inserted.
        /// </param>
        /// <param name="value">
        /// The Target to insert.
        /// </param>
        public virtual void Insert(int index, Target value)
        {
            this.List.Insert(index, value);
        }

        /// <summary>
        /// Gets or sets the Target at the given index in this TargetCollection.
        /// </summary>
        public virtual Target this[int index]
        {
            get { return (Target)this.List[index]; }
            set { this.List[index] = value; }
        }

        /// <summary>
        /// Removes the first occurrence of a specific Target from this TargetCollection.
        /// </summary>
        /// <param name="value">
        /// The Target value to remove from this TargetCollection.
        /// </param>
        public virtual void Remove(Target value)
        {
            this.List.Remove(value);
        }

        /// <summary>
        /// Type-specific enumeration class, used by TargetCollection.GetEnumerator.
        /// </summary>
        public class Enumerator: System.Collections.IEnumerator
        {
            private System.Collections.IEnumerator wrapped;

            /// <summary>
            /// 
            /// </summary>
            /// <param name="collection"></param>
            public Enumerator(TargetCollection collection)
            {
                this.wrapped = ((System.Collections.CollectionBase)collection).GetEnumerator();
            }

            /// <summary>
            /// 
            /// </summary>
            public Target Current
            {
                get { return (Target)(this.wrapped.Current); }
            }

            object System.Collections.IEnumerator.Current
            {
                get { return (Target)(this.wrapped.Current); }
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
        /// Returns an enumerator that can iterate through the elements of this TargetCollection.
        /// </summary>
        /// <returns>
        /// An object that implements System.Collections.IEnumerator.
        /// </returns>        
        public new virtual TargetCollection.Enumerator GetEnumerator()
        {
            return new TargetCollection.Enumerator(this);
        }
    }
}
