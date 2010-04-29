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
    /// <summary>
    /// Represents a parameter to a Database target.
    /// </summary>
    public class DatabaseParameterInfo
    {
        /// <summary>
        /// Creates a new instance of <see cref="DatabaseParameterInfo"/>.
        /// </summary>
        public DatabaseParameterInfo(){}

        /// <summary>
        /// Creates a new instance of <see cref="DatabaseParameterInfo"/>
        /// and sets Name and Layout properties to the specified values.
        /// </summary>
        public DatabaseParameterInfo(string name, string layout)
        {
            Name = name;
            Layout = layout;
        }

        private Layout _compiledlayout;
        private string _name;
        private int _size = 0;
        private byte _precision = 0;
        private byte _scale = 0;

        /// <summary>
        /// Database parameter name.
        /// </summary>
        [RequiredParameter]
        public string Name
        {
            get { return _name; }
            set { _name = value; }
        }

        /// <summary>
        /// The layout that should be use to calcuate the value for the parameter.
        /// </summary>
        [RequiredParameter]
        [AcceptsLayout]
        public string Layout
        {
            get { return _compiledlayout.Text; }
            set { _compiledlayout = new Layout(value); }
        }

        /// <summary>
        /// The compiled representation of the Layout property.
        /// </summary>
        public Layout CompiledLayout
        {
            get { return _compiledlayout; }
            set { _compiledlayout = value; }
        }

        /// <summary>
        /// Database parameter size.
        /// </summary>
        [System.ComponentModel.DefaultValue(0)]
        public int Size
        {
            get { return _size; }
            set { _size = value; }
        }

        /// <summary>
        /// Database parameter precision.
        /// </summary>
        [System.ComponentModel.DefaultValue(0)]
        public byte Precision
        {
            get { return _precision; }
            set { _precision = value; }
        }

        /// <summary>
        /// Database parameter scale.
        /// </summary>
        [System.ComponentModel.DefaultValue(0)]
        public byte Scale
        {
            get { return _scale; }
            set { _scale = value; }
        }
    }
}
