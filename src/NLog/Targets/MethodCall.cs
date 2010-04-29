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
    /// Calls the specified static method on each logging message and passes contextual parameters to it.
    /// </summary>
    /// <example>
    /// <p>
    /// To set up the target in the <a href="config.html">configuration file</a>, 
    /// use the following syntax:
    /// </p>
    /// <code lang="XML" src="examples/targets/Configuration File/MethodCall/NLog.config" />
    /// <p>
    /// This assumes just one target and a single rule. More configuration
    /// options are described <a href="config.html">here</a>.
    /// </p>
    /// <p>
    /// To set up the log target programmatically use code like this:
    /// </p>
    /// <code lang="C#" src="examples/targets/Configuration API/MethodCall/Simple/Example.cs" />
    /// </example>
    [Target("MethodCall")]
    public sealed class MethodCallTarget: MethodCallTargetBase
    {
        private string _className = null;
        private MethodInfo _methodInfo = null;
        private string _methodName = null;

        /// <summary>
        /// The class name.
        /// </summary>
        public string ClassName
        {
            get { return _className; }
            set
            {
                _className = value;
                UpdateMethodInfo();
            }
        }

        /// <summary>
        /// The method name. The method must be public and static.
        /// </summary>
        public string MethodName
        {
            get { return _methodName; }
            set
            {
                _methodName = value;
                UpdateMethodInfo();
            }
        }

        private MethodInfo Method
        {
            get { return _methodInfo; }
            set { _methodInfo = value; }
        }

        private void UpdateMethodInfo()
        {
            if (ClassName != null && MethodName != null)
            {
                Type targetType = Type.GetType(ClassName);
                Method = targetType.GetMethod(MethodName);
            }
            else
            {
                Method = null;
            }
        }

        /// <summary>
        /// Calls the specified Method.
        /// </summary>
        /// <param name="parameters">Method parameters.</param>
        protected override void DoInvoke(object[]parameters)
        {
            if (Method != null)
            {
                Method.Invoke(null, parameters);
            }
        }
    }
}
