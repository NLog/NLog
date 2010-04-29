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
    /// A collection of elements of type ConsoleRowHighlightingRule
    /// </summary>
    public class ConsoleRowHighlightingRuleCollection: System.Collections.CollectionBase
    {
        /// <summary>
        /// Initializes a new empty instance of the ConsoleRowHighlightingRuleCollection class.
        /// </summary>
        public ConsoleRowHighlightingRuleCollection()
        {
            // empty
        }

        /// <summary>
        /// Initializes a new instance of the ConsoleRowHighlightingRuleCollection class, containing elements
        /// copied from an array.
        /// </summary>
        /// <param name="items">
        /// The array whose elements are to be added to the new ConsoleRowHighlightingRuleCollection.
        /// </param>
        public ConsoleRowHighlightingRuleCollection(ConsoleRowHighlightingRule[] items)
        {
            this.AddRange(items);
        }

        /// <summary>
        /// Initializes a new instance of the ConsoleRowHighlightingRuleCollection class, containing elements
        /// copied from another instance of ConsoleRowHighlightingRuleCollection
        /// </summary>
        /// <param name="items">
        /// The ConsoleRowHighlightingRuleCollection whose elements are to be added to the new ConsoleRowHighlightingRuleCollection.
        /// </param>
        public ConsoleRowHighlightingRuleCollection(ConsoleRowHighlightingRuleCollection items)
        {
            this.AddRange(items);
        }

        /// <summary>
        /// Adds the elements of an array to the end of this ConsoleRowHighlightingRuleCollection.
        /// </summary>
        /// <param name="items">
        /// The array whose elements are to be added to the end of this ConsoleRowHighlightingRuleCollection.
        /// </param>
        public virtual void AddRange(ConsoleRowHighlightingRule[] items)
        {
            foreach (ConsoleRowHighlightingRule item in items)
            {
                this.List.Add(item);
            }
        }

        /// <summary>
        /// Adds the elements of another ConsoleRowHighlightingRuleCollection to the end of this ConsoleRowHighlightingRuleCollection.
        /// </summary>
        /// <param name="items">
        /// The ConsoleRowHighlightingRuleCollection whose elements are to be added to the end of this ConsoleRowHighlightingRuleCollection.
        /// </param>
        public virtual void AddRange(ConsoleRowHighlightingRuleCollection items)
        {
            foreach (ConsoleRowHighlightingRule item in items)
            {
                this.List.Add(item);
            }
        }

        /// <summary>
        /// Adds an instance of type ConsoleRowHighlightingRule to the end of this ConsoleRowHighlightingRuleCollection.
        /// </summary>
        /// <param name="value">
        /// The ConsoleRowHighlightingRule to be added to the end of this ConsoleRowHighlightingRuleCollection.
        /// </param>
        public virtual void Add(ConsoleRowHighlightingRule value)
        {
            this.List.Add(value);
        }

        /// <summary>
        /// Determines whether a specfic ConsoleRowHighlightingRule value is in this ConsoleRowHighlightingRuleCollection.
        /// </summary>
        /// <param name="value">
        /// The ConsoleRowHighlightingRule value to locate in this ConsoleRowHighlightingRuleCollection.
        /// </param>
        /// <returns>
        /// true if value is found in this ConsoleRowHighlightingRuleCollection;
        /// false otherwise.
        /// </returns>
        public virtual bool Contains(ConsoleRowHighlightingRule value)
        {
            return this.List.Contains(value);
        }

        /// <summary>
        /// Return the zero-based index of the first occurrence of a specific value
        /// in this ConsoleRowHighlightingRuleCollection
        /// </summary>
        /// <param name="value">
        /// The ConsoleRowHighlightingRule value to locate in the ConsoleRowHighlightingRuleCollection.
        /// </param>
        /// <returns>
        /// The zero-based index of the first occurrence of the _ELEMENT value if found;
        /// -1 otherwise.
        /// </returns>
        public virtual int IndexOf(ConsoleRowHighlightingRule value)
        {
            return this.List.IndexOf(value);
        }

        /// <summary>
        /// Inserts an element into the ConsoleRowHighlightingRuleCollection at the specified index
        /// </summary>
        /// <param name="index">
        /// The index at which the ConsoleRowHighlightingRule is to be inserted.
        /// </param>
        /// <param name="value">
        /// The ConsoleRowHighlightingRule to insert.
        /// </param>
        public virtual void Insert(int index, ConsoleRowHighlightingRule value)
        {
            this.List.Insert(index, value);
        }

        /// <summary>
        /// Gets or sets the ConsoleRowHighlightingRule at the given index in this ConsoleRowHighlightingRuleCollection.
        /// </summary>
        public virtual ConsoleRowHighlightingRule this[int index]
        {
            get { return (ConsoleRowHighlightingRule) this.List[index]; }
            set { this.List[index] = value; }
        }

        /// <summary>
        /// Removes the first occurrence of a specific ConsoleRowHighlightingRule from this ConsoleRowHighlightingRuleCollection.
        /// </summary>
        /// <param name="value">
        /// The ConsoleRowHighlightingRule value to remove from this ConsoleRowHighlightingRuleCollection.
        /// </param>
        public virtual void Remove(ConsoleRowHighlightingRule value)
        {
            this.List.Remove(value);
        }

        /// <summary>
        /// Type-specific enumeration class, used by ConsoleRowHighlightingRuleCollection.GetEnumerator.
        /// </summary>
        public class Enumerator: System.Collections.IEnumerator
        {
            private System.Collections.IEnumerator wrapped;

            /// <summary>
            /// 
            /// </summary>
            /// <param name="collection"></param>
            public Enumerator(ConsoleRowHighlightingRuleCollection collection)
            {
                this.wrapped = ((System.Collections.CollectionBase)collection).GetEnumerator();
            }

            /// <summary>
            /// 
            /// </summary>
            public ConsoleRowHighlightingRule Current
            {
                get { return (ConsoleRowHighlightingRule) (this.wrapped.Current); }
            }

            object System.Collections.IEnumerator.Current
            {
                get { return (ConsoleRowHighlightingRule) (this.wrapped.Current); }
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
        /// Returns an enumerator that can iterate through the elements of this ConsoleRowHighlightingRuleCollection.
        /// </summary>
        /// <returns>
        /// An object that implements System.Collections.IEnumerator.
        /// </returns>        
        public new virtual ConsoleRowHighlightingRuleCollection.Enumerator GetEnumerator()
        {
            return new ConsoleRowHighlightingRuleCollection.Enumerator(this);
        }
    }
}

#endif