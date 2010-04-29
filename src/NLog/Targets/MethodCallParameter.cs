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
    /// <summary>
    /// A parameter to MethodCall.
    /// </summary>
    public class MethodCallParameter
    {
        private Type _type;
        private Layout _compiledlayout;
        private string _name;

        /// <summary>
        /// Constructs a new instance of <see cref="MethodCallParameter"/> and sets
        /// the type to String.
        /// </summary>
        public MethodCallParameter()
        {
            _type = typeof(string);
        }

        /// <summary>
        /// Constructs a new instance of <see cref="MethodCallParameter"/>, sets
        /// the type to String and initializes the Layout property.
        /// </summary>
        public MethodCallParameter(string layout)
        {
            _type = typeof(string);
            Layout = layout;
        }

        /// <summary>
        /// Constructs a new instance of <see cref="MethodCallParameter"/>, sets
        /// the type to String and initializes the Name and Layout properties.
        /// </summary>
        public MethodCallParameter(string name, string layout)
        {
            _type = typeof(string);
            Name = name;
            Layout = layout;
        }

        /// <summary>
        /// Constructs a new instance of <see cref="MethodCallParameter"/>, sets
        /// the type to String and initializes the Name, Layout and Type properties.
        /// </summary>
        public MethodCallParameter(string name, Type type, string layout)
        {
            _type = type;
            Name = name;
            Layout = layout;
        }

        /// <summary>
        /// The name of the parameter.
        /// </summary>
        public string Name
        {
            get { return _name; }
            set { _name = value; }
        }

        /// <summary>
        /// The type of the parameter.
        /// </summary>
        public string Type
        {
            get { return _type.FullName; }
            set { _type = System.Type.GetType(value); }
        }

        /// <summary>
        /// The layout that should be use to calcuate the value for the parameter.
        /// </summary>
        [RequiredParameter]
        public string Layout
        {
            get { return _compiledlayout.Text; }
            set { _compiledlayout = new Layout(value); }
        }

        /// <summary>
        /// The compiled layout that should be use to calcuate the value for the parameter.
        /// </summary>
        public Layout CompiledLayout
        {
            get { return _compiledlayout; }
            set { _compiledlayout = value; }
        }

        internal object GetValue(LogEventInfo logEvent)
        {
            return Convert.ChangeType(CompiledLayout.GetFormattedMessage(logEvent), _type, CultureInfo.InvariantCulture);
        }
    }
}
