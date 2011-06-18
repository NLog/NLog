// 
// Copyright (c) 2004-2011 Jaroslaw Kowalski <jaak@jkowalski.net>
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

namespace NLog.Targets
{
    using System;
    using System.Reflection;

    /// <summary>
    /// Calls the specified static method on each log message and passes contextual parameters to it.
    /// </summary>
    /// <seealso href="http://nlog-project.org/wiki/MethodCall_target">Documentation on NLog Wiki</seealso>
    /// <example>
    /// <p>
    /// To set up the target in the <a href="config.html">configuration file</a>, 
    /// use the following syntax:
    /// </p>
    /// <code lang="XML" source="examples/targets/Configuration File/MethodCall/NLog.config" />
    /// <p>
    /// This assumes just one target and a single rule. More configuration
    /// options are described <a href="config.html">here</a>.
    /// </p>
    /// <p>
    /// To set up the log target programmatically use code like this:
    /// </p>
    /// <code lang="C#" source="examples/targets/Configuration API/MethodCall/Simple/Example.cs" />
    /// </example>
    [Target("MethodCall")]
    public sealed class MethodCallTarget : MethodCallTargetBase
    {
        /// <summary>
        /// Gets or sets the class name.
        /// </summary>
        /// <docgen category='Invocation Options' order='10' />
        public string ClassName { get; set; }

        /// <summary>
        /// Gets or sets the method name. The method must be public and static.
        /// </summary>
        /// <docgen category='Invocation Options' order='10' />
        public string MethodName { get; set; }

        private MethodInfo Method { get; set; }

        /// <summary>
        /// Initializes the target.
        /// </summary>
        protected override void InitializeTarget()
        {
            base.InitializeTarget();

            if (this.ClassName != null && this.MethodName != null)
            {
                Type targetType = Type.GetType(this.ClassName);
                this.Method = targetType.GetMethod(this.MethodName);
            }
            else
            {
                this.Method = null;
            }
        }

        /// <summary>
        /// Calls the specified Method.
        /// </summary>
        /// <param name="parameters">Method parameters.</param>
        protected override void DoInvoke(object[] parameters)
        {
            if (this.Method != null)
            {
                this.Method.Invoke(null, parameters);
            }
        }
    }
}
