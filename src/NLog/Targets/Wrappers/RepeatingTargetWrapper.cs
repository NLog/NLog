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
    using System.ComponentModel;
    using System.Threading;
    using NLog.Common;
    using NLog.Internal;

    /// <summary>
    /// Repeats each log event the specified number of times.
    /// </summary>
    /// <seealso href="https://github.com/nlog/nlog/wiki/RepeatingWrapper-target">Documentation on NLog Wiki</seealso>
    /// <example>
    /// <p>This example causes each log message to be repeated 3 times.</p>
    /// <p>
    /// To set up the target in the <a href="config.html">configuration file</a>, 
    /// use the following syntax:
    /// </p>
    /// <code lang="XML" source="examples/targets/Configuration File/RepeatingWrapper/NLog.config" />
    /// <p>
    /// The above examples assume just one target and a single rule. See below for
    /// a programmatic configuration that's equivalent to the above config file:
    /// </p>
    /// <code lang="C#" source="examples/targets/Configuration API/RepeatingWrapper/Simple/Example.cs" />
    /// </example>
    [Target("RepeatingWrapper", IsWrapper = true)]
    public class RepeatingTargetWrapper : WrapperTargetBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="RepeatingTargetWrapper" /> class.
        /// </summary>
        public RepeatingTargetWrapper()
            : this(null, 3)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RepeatingTargetWrapper" /> class.
        /// </summary>
        /// <param name="name">Name of the target.</param>
        /// <param name="wrappedTarget">The wrapped target.</param>
        /// <param name="repeatCount">The repeat count.</param>
        public RepeatingTargetWrapper(string name, Target wrappedTarget, int repeatCount) 
            : this(wrappedTarget, repeatCount)
        {
            this.Name = name;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RepeatingTargetWrapper" /> class.
        /// </summary>
        /// <param name="wrappedTarget">The wrapped target.</param>
        /// <param name="repeatCount">The repeat count.</param>
        public RepeatingTargetWrapper(Target wrappedTarget, int repeatCount)
        {
            WrappedTarget = wrappedTarget;
            this.RepeatCount = repeatCount;
        }

        /// <summary>
        /// Gets or sets the number of times to repeat each log message.
        /// </summary>
        /// <docgen category='Repeating Options' order='10' />
        [DefaultValue(3)]
        public int RepeatCount { get; set; }

        /// <summary>
        /// Forwards the log message to the <see cref="WrapperTargetBase.WrappedTarget"/> by calling the <see cref="Target.Write(LogEventInfo)"/> method <see cref="RepeatCount"/> times.
        /// </summary>
        /// <param name="logEvent">The log event.</param>
        protected override void Write(AsyncLogEventInfo logEvent)
        {
            AsyncHelpers.Repeat(this.RepeatCount, logEvent.Continuation, cont => this.WrappedTarget.WriteAsyncLogEvent(logEvent.LogEvent.WithContinuation(cont)));
        }
    }
}
