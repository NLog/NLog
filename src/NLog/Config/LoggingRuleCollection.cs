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

using NLog;
using NLog.Targets;
using NLog.Filters;

namespace NLog.Config
{
    // CLOVER:OFF
    /// <summary>
    /// A collection of elements of type LoggingRule
    /// </summary>
    public class LoggingRuleCollection: System.Collections.CollectionBase
    {
        /// <summary>
        /// Initializes a new empty instance of the LoggingRuleCollection class.
        /// </summary>
        public LoggingRuleCollection()
        {
            // empty
        }

        /// <summary>
        /// Initializes a new instance of the LoggingRuleCollection class, containing elements
        /// copied from an array.
        /// </summary>
        /// <param name="items">
        /// The array whose elements are to be added to the new LoggingRuleCollection.
        /// </param>
        public LoggingRuleCollection(LoggingRule[]items)
        {
            this.AddRange(items);
        }

        /// <summary>
        /// Initializes a new instance of the LoggingRuleCollection class, containing elements
        /// copied from another instance of LoggingRuleCollection
        /// </summary>
        /// <param name="items">
        /// The LoggingRuleCollection whose elements are to be added to the new LoggingRuleCollection.
        /// </param>
        public LoggingRuleCollection(LoggingRuleCollection items)
        {
            this.AddRange(items);
        }

        /// <summary>
        /// Adds the elements of an array to the end of this LoggingRuleCollection.
        /// </summary>
        /// <param name="items">
        /// The array whose elements are to be added to the end of this LoggingRuleCollection.
        /// </param>
        public virtual void AddRange(LoggingRule[]items)
        {
            foreach (LoggingRule item in items)
            {
                this.List.Add(item);
            }
        }

        /// <summary>
        /// Adds the elements of another LoggingRuleCollection to the end of this LoggingRuleCollection.
        /// </summary>
        /// <param name="items">
        /// The LoggingRuleCollection whose elements are to be added to the end of this LoggingRuleCollection.
        /// </param>
        public virtual void AddRange(LoggingRuleCollection items)
        {
            foreach (LoggingRule item in items)
            {
                this.List.Add(item);
            }
        }

        /// <summary>
        /// Adds an instance of type LoggingRule to the end of this LoggingRuleCollection.
        /// </summary>
        /// <param name="value">
        /// The LoggingRule to be added to the end of this LoggingRuleCollection.
        /// </param>
        public virtual void Add(LoggingRule value)
        {
            this.List.Add(value);
        }

        /// <summary>
        /// Determines whether a specfic LoggingRule value is in this LoggingRuleCollection.
        /// </summary>
        /// <param name="value">
        /// The LoggingRule value to locate in this LoggingRuleCollection.
        /// </param>
        /// <returns>
        /// true if value is found in this LoggingRuleCollection;
        /// false otherwise.
        /// </returns>
        public virtual bool Contains(LoggingRule value)
        {
            return this.List.Contains(value);
        }

        /// <summary>
        /// Return the zero-based index of the first occurrence of a specific value
        /// in this LoggingRuleCollection
        /// </summary>
        /// <param name="value">
        /// The LoggingRule value to locate in the LoggingRuleCollection.
        /// </param>
        /// <returns>
        /// The zero-based index of the first occurrence of the _ELEMENT value if found;
        /// -1 otherwise.
        /// </returns>
        public virtual int IndexOf(LoggingRule value)
        {
            return this.List.IndexOf(value);
        }

        /// <summary>
        /// Inserts an element into the LoggingRuleCollection at the specified index
        /// </summary>
        /// <param name="index">
        /// The index at which the LoggingRule is to be inserted.
        /// </param>
        /// <param name="value">
        /// The LoggingRule to insert.
        /// </param>
        public virtual void Insert(int index, LoggingRule value)
        {
            this.List.Insert(index, value);
        }

        /// <summary>
        /// Gets or sets the LoggingRule at the given index in this LoggingRuleCollection.
        /// </summary>
        public virtual LoggingRule this[int index]
        {
            get { return (LoggingRule)this.List[index]; }
            set { this.List[index] = value; }
        }

        /// <summary>
        /// Removes the first occurrence of a specific LoggingRule from this LoggingRuleCollection.
        /// </summary>
        /// <param name="value">
        /// The LoggingRule value to remove from this LoggingRuleCollection.
        /// </param>
        public virtual void Remove(LoggingRule value)
        {
            this.List.Remove(value);
        }

        /// <summary>
        /// Type-specific enumeration class, used by LoggingRuleCollection.GetEnumerator.
        /// </summary>
        public class Enumerator: System.Collections.IEnumerator
        {
            private System.Collections.IEnumerator wrapped;

            /// <summary>
            /// 
            /// </summary>
            /// <param name="collection"></param>
            public Enumerator(LoggingRuleCollection collection)
            {
                this.wrapped = ((System.Collections.CollectionBase)collection).GetEnumerator();
            }

            /// <summary>
            /// Returns the current object.
            /// </summary>
            public LoggingRule Current
            {
                get { return (LoggingRule)(this.wrapped.Current); }
            }

            /// <summary>
            /// Returns the current object.
            /// </summary>
            object System.Collections.IEnumerator.Current
            {
                get { return (LoggingRule)(this.wrapped.Current); }
            }

            /// <summary>
            /// Advances to the next object.
            /// </summary>
            /// <returns>A <see cref="MoveNext"/> result</returns>
            public bool MoveNext()
            {
                return this.wrapped.MoveNext();
            }

            /// <summary>
            /// Resets the enumerator.
            /// </summary>
            public void Reset()
            {
                this.wrapped.Reset();
            }
        }

        /// <summary>
        /// Returns an enumerator that can iterate through the elements of this LoggingRuleCollection.
        /// </summary>
        /// <returns>
        /// An object that implements System.Collections.IEnumerator.
        /// </returns>        
        public new virtual LoggingRuleCollection.Enumerator GetEnumerator()
        {
            return new LoggingRuleCollection.Enumerator(this);
        }
    }
}
