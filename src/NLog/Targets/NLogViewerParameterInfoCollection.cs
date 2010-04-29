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
using System.Text;
using System.Diagnostics;
using System.Reflection;
using System.Data;
using System.Collections;

using NLog.Internal;
using NLog.Config;

namespace NLog.Targets
{
    // CLOVER:OFF
    /// <summary>
    /// A collection of elements of type NLogViewerParameterInfo
    /// </summary>
    public class NLogViewerParameterInfoCollection: System.Collections.CollectionBase
    {
        /// <summary>
        /// Initializes a new empty instance of the NLogViewerParameterInfoCollection class.
        /// </summary>
        public NLogViewerParameterInfoCollection()
        {
            // empty
        }

        /// <summary>
        /// Initializes a new instance of the NLogViewerParameterInfoCollection class, containing elements
        /// copied from an array.
        /// </summary>
        /// <param name="items">
        /// The array whose elements are to be added to the new NLogViewerParameterInfoCollection.
        /// </param>
        public NLogViewerParameterInfoCollection(NLogViewerParameterInfo[]items)
        {
            this.AddRange(items);
        }

        /// <summary>
        /// Initializes a new instance of the NLogViewerParameterInfoCollection class, containing elements
        /// copied from another instance of NLogViewerParameterInfoCollection
        /// </summary>
        /// <param name="items">
        /// The NLogViewerParameterInfoCollection whose elements are to be added to the new NLogViewerParameterInfoCollection.
        /// </param>
        public NLogViewerParameterInfoCollection(NLogViewerParameterInfoCollection items)
        {
            this.AddRange(items);
        }

        /// <summary>
        /// Adds the elements of an array to the end of this NLogViewerParameterInfoCollection.
        /// </summary>
        /// <param name="items">
        /// The array whose elements are to be added to the end of this NLogViewerParameterInfoCollection.
        /// </param>
        public virtual void AddRange(NLogViewerParameterInfo[]items)
        {
            foreach (NLogViewerParameterInfo item in items)
            {
                this.List.Add(item);
            }
        }

        /// <summary>
        /// Adds the elements of another NLogViewerParameterInfoCollection to the end of this NLogViewerParameterInfoCollection.
        /// </summary>
        /// <param name="items">
        /// The NLogViewerParameterInfoCollection whose elements are to be added to the end of this NLogViewerParameterInfoCollection.
        /// </param>
        public virtual void AddRange(NLogViewerParameterInfoCollection items)
        {
            foreach (NLogViewerParameterInfo item in items)
            {
                this.List.Add(item);
            }
        }

        /// <summary>
        /// Adds an instance of type NLogViewerParameterInfo to the end of this NLogViewerParameterInfoCollection.
        /// </summary>
        /// <param name="value">
        /// The NLogViewerParameterInfo to be added to the end of this NLogViewerParameterInfoCollection.
        /// </param>
        public virtual void Add(NLogViewerParameterInfo value)
        {
            this.List.Add(value);
        }

        /// <summary>
        /// Determines whether a specfic NLogViewerParameterInfo value is in this NLogViewerParameterInfoCollection.
        /// </summary>
        /// <param name="value">
        /// The NLogViewerParameterInfo value to locate in this NLogViewerParameterInfoCollection.
        /// </param>
        /// <returns>
        /// true if value is found in this NLogViewerParameterInfoCollection;
        /// false otherwise.
        /// </returns>
        public virtual bool Contains(NLogViewerParameterInfo value)
        {
            return this.List.Contains(value);
        }

        /// <summary>
        /// Return the zero-based index of the first occurrence of a specific value
        /// in this NLogViewerParameterInfoCollection
        /// </summary>
        /// <param name="value">
        /// The NLogViewerParameterInfo value to locate in the NLogViewerParameterInfoCollection.
        /// </param>
        /// <returns>
        /// The zero-based index of the first occurrence of the _ELEMENT value if found;
        /// -1 otherwise.
        /// </returns>
        public virtual int IndexOf(NLogViewerParameterInfo value)
        {
            return this.List.IndexOf(value);
        }

        /// <summary>
        /// Inserts an element into the NLogViewerParameterInfoCollection at the specified index
        /// </summary>
        /// <param name="index">
        /// The index at which the NLogViewerParameterInfo is to be inserted.
        /// </param>
        /// <param name="value">
        /// The NLogViewerParameterInfo to insert.
        /// </param>
        public virtual void Insert(int index, NLogViewerParameterInfo value)
        {
            this.List.Insert(index, value);
        }

        /// <summary>
        /// Gets or sets the NLogViewerParameterInfo at the given index in this NLogViewerParameterInfoCollection.
        /// </summary>
        public virtual NLogViewerParameterInfo this[int index]
        {
            get { return (NLogViewerParameterInfo)this.List[index]; }
            set { this.List[index] = value; }
        }

        /// <summary>
        /// Removes the first occurrence of a specific NLogViewerParameterInfo from this NLogViewerParameterInfoCollection.
        /// </summary>
        /// <param name="value">
        /// The NLogViewerParameterInfo value to remove from this NLogViewerParameterInfoCollection.
        /// </param>
        public virtual void Remove(NLogViewerParameterInfo value)
        {
            this.List.Remove(value);
        }

        /// <summary>
        /// Type-specific enumeration class, used by NLogViewerParameterInfoCollection.GetEnumerator.
        /// </summary>
        public class Enumerator: System.Collections.IEnumerator
        {
            private System.Collections.IEnumerator wrapped;

            /// <summary>
            /// 
            /// </summary>
            /// <param name="collection"></param>
            public Enumerator(NLogViewerParameterInfoCollection collection)
            {
                this.wrapped = ((System.Collections.CollectionBase)collection).GetEnumerator();
            }

            /// <summary>
            /// 
            /// </summary>
            public NLogViewerParameterInfo Current
            {
                get { return (NLogViewerParameterInfo)(this.wrapped.Current); }
            }

            /// <summary>
            /// 
            /// </summary>
            object System.Collections.IEnumerator.Current
            {
                get { return (NLogViewerParameterInfo)(this.wrapped.Current); }
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
        /// Returns an enumerator that can iterate through the elements of this NLogViewerParameterInfoCollection.
        /// </summary>
        /// <returns>
        /// An object that implements System.Collections.IEnumerator.
        /// </returns>        
        public new virtual NLogViewerParameterInfoCollection.Enumerator GetEnumerator()
        {
            return new NLogViewerParameterInfoCollection.Enumerator(this);
        }
    }
}
