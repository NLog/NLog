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

#if !NETCF && !MONO

using System;
using System.Runtime.InteropServices;

using NLog.Targets;

namespace NLog
{
    /// <summary>
    /// A collection of elements of type RichTextBoxRowColoringRule
    /// </summary>
    public class RichTextBoxRowColoringRuleCollection : System.Collections.CollectionBase
    {
        /// <summary>
        /// Initializes a new empty instance of the RichTextBoxRowColoringRuleCollection class.
        /// </summary>
        public RichTextBoxRowColoringRuleCollection()
        {
            // empty
        }

        /// <summary>
        /// Initializes a new instance of the RichTextBoxRowColoringRuleCollection class, containing elements
        /// copied from an array.
        /// </summary>
        /// <param name="items">
        /// The array whose elements are to be added to the new RichTextBoxRowColoringRuleCollection.
        /// </param>
        public RichTextBoxRowColoringRuleCollection(RichTextBoxRowColoringRule[] items)
        {
            this.AddRange(items);
        }

        /// <summary>
        /// Initializes a new instance of the RichTextBoxRowColoringRuleCollection class, containing elements
        /// copied from another instance of RichTextBoxRowColoringRuleCollection
        /// </summary>
        /// <param name="items">
        /// The RichTextBoxRowColoringRuleCollection whose elements are to be added to the new RichTextBoxRowColoringRuleCollection.
        /// </param>
        public RichTextBoxRowColoringRuleCollection(RichTextBoxRowColoringRuleCollection items)
        {
            this.AddRange(items);
        }

        /// <summary>
        /// Adds the elements of an array to the end of this RichTextBoxRowColoringRuleCollection.
        /// </summary>
        /// <param name="items">
        /// The array whose elements are to be added to the end of this RichTextBoxRowColoringRuleCollection.
        /// </param>
        public virtual void AddRange(RichTextBoxRowColoringRule[] items)
        {
            foreach (RichTextBoxRowColoringRule item in items)
            {
                this.List.Add(item);
            }
        }

        /// <summary>
        /// Adds the elements of another RichTextBoxRowColoringRuleCollection to the end of this RichTextBoxRowColoringRuleCollection.
        /// </summary>
        /// <param name="items">
        /// The RichTextBoxRowColoringRuleCollection whose elements are to be added to the end of this RichTextBoxRowColoringRuleCollection.
        /// </param>
        public virtual void AddRange(RichTextBoxRowColoringRuleCollection items)
        {
            foreach (RichTextBoxRowColoringRule item in items)
            {
                this.List.Add(item);
            }
        }

        /// <summary>
        /// Adds an instance of type RichTextBoxWordColoringRule to the end of this RichTextBoxRowColoringRuleCollection.
        /// </summary>
        /// <param name="value">
        /// The RichTextBoxRowColoringRule to be added to the end of this RichTextBoxRowColoringRuleCollection.
        /// </param>
        public virtual void Add(RichTextBoxRowColoringRule value)
        {
            this.List.Add(value);
        }

        /// <summary>
        /// Determines whether a specfic RichTextBoxRowColoringRule value is in this RichTextBoxRowColoringRuleCollection.
        /// </summary>
        /// <param name="value">
        /// The RichTextBoxRowColoringRule value to locate in this RichTextBoxRowColoringRuleCollection.
        /// </param>
        /// <returns>
        /// true if value is found in this RichTextBoxRowColoringRuleCollection;
        /// false otherwise.
        /// </returns>
        public virtual bool Contains(RichTextBoxRowColoringRule value)
        {
            return this.List.Contains(value);
        }

        /// <summary>
        /// Return the zero-based index of the first occurrence of a specific value
        /// in this RichTextBoxRowColoringRuleCollection
        /// </summary>
        /// <param name="value">
        /// The RichTextBoxRowColoringRule value to locate in the RichTextBoxRowColoringRuleCollection.
        /// </param>
        /// <returns>
        /// The zero-based index of the first occurrence of the _ELEMENT value if found;
        /// -1 otherwise.
        /// </returns>
        public virtual int IndexOf(RichTextBoxRowColoringRule value)
        {
            return this.List.IndexOf(value);
        }

        /// <summary>
        /// Inserts an element into the RichTextBoxRowColoringRuleCollection at the specified index
        /// </summary>
        /// <param name="index">
        /// The index at which the RichTextBoxRowColoringRule is to be inserted.
        /// </param>
        /// <param name="value">
        /// The RichTextBoxRowColoringRule to insert.
        /// </param>
        public virtual void Insert(int index, RichTextBoxRowColoringRule value)
        {
            this.List.Insert(index, value);
        }

        /// <summary>
        /// Gets or sets the RichTextBoxRowColoringRule at the given index in this RichTextBoxRowColoringRuleCollection.
        /// </summary>
        public virtual RichTextBoxRowColoringRule this[int index]
        {
            get
            {
                return (RichTextBoxRowColoringRule)this.List[index];
            }
            set
            {
                this.List[index] = value;
            }
        }

        /// <summary>
        /// Removes the first occurrence of a specific RichTextBoxRowColoringRule from this RichTextBoxRowColoringRuleCollection.
        /// </summary>
        /// <param name="value">
        /// The RichTextBoxRowColoringRule value to remove from this RichTextBoxRowColoringRuleCollection.
        /// </param>
        public virtual void Remove(RichTextBoxRowColoringRule value)
        {
            this.List.Remove(value);
        }

        /// <summary>
        /// Type-specific enumeration class, used by RichTextBoxRowColoringRuleCollection.GetEnumerator.
        /// </summary>
        public class Enumerator : System.Collections.IEnumerator
        {
            private System.Collections.IEnumerator wrapped;

            /// <summary>
            /// 
            /// </summary>
            /// <param name="collection"></param>
            public Enumerator(RichTextBoxRowColoringRuleCollection collection)
            {
                this.wrapped = ((System.Collections.CollectionBase)collection).GetEnumerator();
            }

            /// <summary>
            /// 
            /// </summary>
            public RichTextBoxRowColoringRule Current
            {
                get
                {
                    return (RichTextBoxRowColoringRule)(this.wrapped.Current);
                }
            }

            object System.Collections.IEnumerator.Current
            {
                get
                {
                    return (RichTextBoxRowColoringRule)(this.wrapped.Current);
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
        /// Returns an enumerator that can iterate through the elements of this RichTextBoxRowColoringRuleCollection.
        /// </summary>
        /// <returns>
        /// An object that implements System.Collections.IEnumerator.
        /// </returns>        
        public new virtual RichTextBoxRowColoringRuleCollection.Enumerator GetEnumerator()
        {
            return new RichTextBoxRowColoringRuleCollection.Enumerator(this);
        }
    }
}
#endif
