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

#if !NETCF

using System;
using System.Runtime.InteropServices;

using NLog.Targets;

namespace NLog.Win32.Targets
{
    /// <summary>
    /// A collection of elements of type ConsoleWordHighlightingRule
    /// </summary>
    public class ConsoleWordHighlightingRuleCollection: System.Collections.CollectionBase
    {
        /// <summary>
        /// Initializes a new empty instance of the ConsoleWordHighlightingRuleCollection class.
        /// </summary>
        public ConsoleWordHighlightingRuleCollection()
        {
            // empty
        }

        /// <summary>
        /// Initializes a new instance of the ConsoleWordHighlightingRuleCollection class, containing elements
        /// copied from an array.
        /// </summary>
        /// <param name="items">
        /// The array whose elements are to be added to the new ConsoleWordHighlightingRuleCollection.
        /// </param>
        public ConsoleWordHighlightingRuleCollection(ConsoleWordHighlightingRule[] items)
        {
            this.AddRange(items);
        }

        /// <summary>
        /// Initializes a new instance of the ConsoleWordHighlightingRuleCollection class, containing elements
        /// copied from another instance of ConsoleWordHighlightingRuleCollection
        /// </summary>
        /// <param name="items">
        /// The ConsoleWordHighlightingRuleCollection whose elements are to be added to the new ConsoleWordHighlightingRuleCollection.
        /// </param>
        public ConsoleWordHighlightingRuleCollection(ConsoleWordHighlightingRuleCollection items)
        {
            this.AddRange(items);
        }

        /// <summary>
        /// Adds the elements of an array to the end of this ConsoleWordHighlightingRuleCollection.
        /// </summary>
        /// <param name="items">
        /// The array whose elements are to be added to the end of this ConsoleWordHighlightingRuleCollection.
        /// </param>
        public virtual void AddRange(ConsoleWordHighlightingRule[] items)
        {
            foreach (ConsoleWordHighlightingRule item in items)
            {
                this.List.Add(item);
            }
        }

        /// <summary>
        /// Adds the elements of another ConsoleWordHighlightingRuleCollection to the end of this ConsoleWordHighlightingRuleCollection.
        /// </summary>
        /// <param name="items">
        /// The ConsoleWordHighlightingRuleCollection whose elements are to be added to the end of this ConsoleWordHighlightingRuleCollection.
        /// </param>
        public virtual void AddRange(ConsoleWordHighlightingRuleCollection items)
        {
            foreach (ConsoleWordHighlightingRule item in items)
            {
                this.List.Add(item);
            }
        }

        /// <summary>
        /// Adds an instance of type ConsoleWordHighlightingRule to the end of this ConsoleWordHighlightingRuleCollection.
        /// </summary>
        /// <param name="value">
        /// The ConsoleWordHighlightingRule to be added to the end of this ConsoleWordHighlightingRuleCollection.
        /// </param>
        public virtual void Add(ConsoleWordHighlightingRule value)
        {
            this.List.Add(value);
        }

        /// <summary>
        /// Determines whether a specfic ConsoleWordHighlightingRule value is in this ConsoleWordHighlightingRuleCollection.
        /// </summary>
        /// <param name="value">
        /// The ConsoleWordHighlightingRule value to locate in this ConsoleWordHighlightingRuleCollection.
        /// </param>
        /// <returns>
        /// true if value is found in this ConsoleWordHighlightingRuleCollection;
        /// false otherwise.
        /// </returns>
        public virtual bool Contains(ConsoleWordHighlightingRule value)
        {
            return this.List.Contains(value);
        }

        /// <summary>
        /// Return the zero-based index of the first occurrence of a specific value
        /// in this ConsoleWordHighlightingRuleCollection
        /// </summary>
        /// <param name="value">
        /// The ConsoleWordHighlightingRule value to locate in the ConsoleWordHighlightingRuleCollection.
        /// </param>
        /// <returns>
        /// The zero-based index of the first occurrence of the _ELEMENT value if found;
        /// -1 otherwise.
        /// </returns>
        public virtual int IndexOf(ConsoleWordHighlightingRule value)
        {
            return this.List.IndexOf(value);
        }

        /// <summary>
        /// Inserts an element into the ConsoleWordHighlightingRuleCollection at the specified index
        /// </summary>
        /// <param name="index">
        /// The index at which the ConsoleWordHighlightingRule is to be inserted.
        /// </param>
        /// <param name="value">
        /// The ConsoleWordHighlightingRule to insert.
        /// </param>
        public virtual void Insert(int index, ConsoleWordHighlightingRule value)
        {
            this.List.Insert(index, value);
        }

        /// <summary>
        /// Gets or sets the ConsoleWordHighlightingRule at the given index in this ConsoleWordHighlightingRuleCollection.
        /// </summary>
        public virtual ConsoleWordHighlightingRule this[int index]
        {
            get { return (ConsoleWordHighlightingRule) this.List[index]; }
            set { this.List[index] = value; }
        }

        /// <summary>
        /// Removes the first occurrence of a specific ConsoleWordHighlightingRule from this ConsoleWordHighlightingRuleCollection.
        /// </summary>
        /// <param name="value">
        /// The ConsoleWordHighlightingRule value to remove from this ConsoleWordHighlightingRuleCollection.
        /// </param>
        public virtual void Remove(ConsoleWordHighlightingRule value)
        {
            this.List.Remove(value);
        }

        /// <summary>
        /// Type-specific enumeration class, used by ConsoleWordHighlightingRuleCollection.GetEnumerator.
        /// </summary>
        public class Enumerator: System.Collections.IEnumerator
        {
            private System.Collections.IEnumerator wrapped;

            /// <summary>
            /// 
            /// </summary>
            /// <param name="collection"></param>
            public Enumerator(ConsoleWordHighlightingRuleCollection collection)
            {
                this.wrapped = ((System.Collections.CollectionBase)collection).GetEnumerator();
            }

            /// <summary>
            /// 
            /// </summary>
            public ConsoleWordHighlightingRule Current
            {
                get { return (ConsoleWordHighlightingRule) (this.wrapped.Current); }
            }

            object System.Collections.IEnumerator.Current
            {
                get { return (ConsoleWordHighlightingRule) (this.wrapped.Current); }
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
        /// Returns an enumerator that can iterate through the elements of this ConsoleWordHighlightingRuleCollection.
        /// </summary>
        /// <returns>
        /// An object that implements System.Collections.IEnumerator.
        /// </returns>        
        public new virtual ConsoleWordHighlightingRuleCollection.Enumerator GetEnumerator()
        {
            return new ConsoleWordHighlightingRuleCollection.Enumerator(this);
        }
    }
}

#endif