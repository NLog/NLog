// 
// Copyright (c) 2004 Jaroslaw Kowalski <jaak@polbox.com>
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
// * Neither the name of the Jaroslaw Kowalski nor the names of its 
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
using System.Xml;
using System.IO;
using System.Reflection;
using System.Globalization;
using System.Text;

using NLog.Config;
using NLog.Appenders;

namespace NLog.Internal
{
	/// <summary>
	/// A dictionary with keys of type string and values of type Appender
	/// </summary>
	internal class AppenderDictionary: System.Collections.DictionaryBase
	{
		/// <summary>
		/// Initializes a new empty instance of the AppenderDictionary class
		/// </summary>
		public AppenderDictionary()
		{
			// empty
		}

		/// <summary>
		/// Gets or sets the Appender associated with the given string
		/// </summary>
		/// <param name="key">
		/// The string whose value to get or set.
		/// </param>
		public virtual Appender this[string key]
		{
			get
			{
				return (Appender) this.Dictionary[key];
			}
			set
			{
				this.Dictionary[key] = value;
			}
		}

		/// <summary>
		/// Adds an element with the specified key and value to this AppenderDictionary.
		/// </summary>
		/// <param name="key">
		/// The string key of the element to add.
		/// </param>
		/// <param name="value">
		/// The Appender value of the element to add.
		/// </param>
		public virtual void Add(string key, Appender value)
		{
			this.Dictionary.Add(key, value);
		}

		/// <summary>
		/// Determines whether this AppenderDictionary contains a specific key.
		/// </summary>
		/// <param name="key">
		/// The string key to locate in this AppenderDictionary.
		/// </param>
		/// <returns>
		/// true if this AppenderDictionary contains an element with the specified key;
		/// otherwise, false.
		/// </returns>
		public virtual bool Contains(string key)
		{
			return this.Dictionary.Contains(key);
		}

		/// <summary>
		/// Determines whether this AppenderDictionary contains a specific key.
		/// </summary>
		/// <param name="key">
		/// The string key to locate in this AppenderDictionary.
		/// </param>
		/// <returns>
		/// true if this AppenderDictionary contains an element with the specified key;
		/// otherwise, false.
		/// </returns>
		public virtual bool ContainsKey(string key)
		{
			return this.Dictionary.Contains(key);
		}

		/// <summary>
		/// Determines whether this AppenderDictionary contains a specific value.
		/// </summary>
		/// <param name="value">
		/// The Appender value to locate in this AppenderDictionary.
		/// </param>
		/// <returns>
		/// true if this AppenderDictionary contains an element with the specified value;
		/// otherwise, false.
		/// </returns>
		public virtual bool ContainsValue(Appender value)
		{
			foreach (Appender item in this.Dictionary.Values)
			{
				if (item == value)
					return true;
			}
			return false;
		}

		/// <summary>
		/// Removes the element with the specified key from this AppenderDictionary.
		/// </summary>
		/// <param name="key">
		/// The string key of the element to remove.
		/// </param>
		public virtual void Remove(string key)
		{
			this.Dictionary.Remove(key);
		}

		/// <summary>
		/// Gets a collection containing the keys in this AppenderDictionary.
		/// </summary>
		public virtual System.Collections.ICollection Keys
		{
			get
			{
				return this.Dictionary.Keys;
			}
		}

		/// <summary>
		/// Gets a collection containing the values in this AppenderDictionary.
		/// </summary>
		public virtual System.Collections.ICollection Values
		{
			get
			{
				return this.Dictionary.Values;
			}
		}
	}
}
