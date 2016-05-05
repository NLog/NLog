// 
// Copyright (c) 2004-2016 Jaroslaw Kowalski <jaak@jkowalski.net>, Kim Christensen, Julian Verdurmen
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

namespace NLog.Targets.Wrappers
{
    using System;
    using NLog.Common;
    using NLog.Internal;

    /// <summary>
    /// Causes a flush after each write on a wrapped target.
    /// </summary>
    /// <seealso href="https://github.com/nlog/nlog/wiki/AutoFlushWrapper-target">Documentation on NLog Wiki</seealso>
    /// <example>
    /// <p>
    /// To set up the target in the <a href="config.html">configuration file</a>, 
    /// use the following syntax:
    /// </p>
    /// <code lang="XML" source="examples/targets/Configuration File/AutoFlushWrapper/NLog.config" />
    /// <p>
    /// The above examples assume just one target and a single rule. See below for
    /// a programmatic configuration that's equivalent to the above config file:
    /// </p>
    /// <code lang="C#" source="examples/targets/Configuration API/AutoFlushWrapper/Simple/Example.cs" />
    /// </example>
    [Target("AutoFlushWrapper", IsWrapper = true)]
    public class AutoFlushTargetWrapper : WrapperTargetBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AutoFlushTargetWrapper" /> class.
        /// </summary>
        /// <remarks>
        /// The default value of the layout is: <code>${longdate}|${level:uppercase=true}|${logger}|${message}</code>
        /// </remarks>
        public AutoFlushTargetWrapper()
            : this(null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AutoFlushTargetWrapper" /> class.
        /// </summary>
        /// <remarks>
        /// The default value of the layout is: <code>${longdate}|${level:uppercase=true}|${logger}|${message}</code>
        /// </remarks>
        /// <param name="wrappedTarget">The wrapped target.</param>
        /// <param name="name">Name of the target</param>
        public AutoFlushTargetWrapper(string name, Target wrappedTarget)
            : this(wrappedTarget)
        {
            this.Name = name;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AutoFlushTargetWrapper" /> class.
        /// </summary>
        /// <param name="wrappedTarget">The wrapped target.</param>
        public AutoFlushTargetWrapper(Target wrappedTarget)
        {
            this.WrappedTarget = wrappedTarget;
        }

        /// <summary>
        /// Forwards the call to the <see cref="WrapperTargetBase.WrappedTarget"/>.Write()
        /// and calls <see cref="Target.Flush(AsyncContinuation)"/> on it.
        /// </summary>
        /// <param name="logEvent">Logging event to be written out.</param>
        protected override void Write(AsyncLogEventInfo logEvent)
        {
            this.WrappedTarget.WriteAsyncLogEvent(logEvent.LogEvent.WithContinuation(AsyncHelpers.PrecededBy(logEvent.Continuation, this.WrappedTarget.Flush)));
        }
    }
}
