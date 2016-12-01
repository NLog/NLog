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
    using Common;
    using Conditions;
    using Config;

    /// <summary>
    /// Causes a flush on a wrapped target if LogEvent statisfies the <see cref="Condition"/>
    /// </summary>
    [Target("FlushTriggerWrapper", IsWrapper = true)]
    public class FlushTriggerWrapper : WrapperTargetBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="FlushTriggerWrapper" /> class.
        /// </summary>
        public FlushTriggerWrapper()
            : this(null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="FlushTriggerWrapper" /> class.
        /// </summary>
        /// <param name="wrappedTarget">The wrapped target.</param>
        /// <param name="name">Name of the target</param>
        public FlushTriggerWrapper(string name, Target wrappedTarget)
            : this(wrappedTarget)
        {
            this.Name = name;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="FlushTriggerWrapper" /> class.
        /// </summary>
        /// <param name="wrappedTarget">The wrapped target.</param>
        public FlushTriggerWrapper(Target wrappedTarget)
        {
            this.WrappedTarget = wrappedTarget;
        }

        /// <summary>
        /// Gets or sets the condition expression. Log events who meet this condition will cause a flush
        /// on the wrapped target.
        /// </summary>
        /// <docgen category='Filtering Options' order='10' />
        [RequiredParameter]
        public ConditionExpression Condition { get; set; }

        /// <summary>
        /// Forwards the call to the <see cref="WrapperTargetBase.WrappedTarget"/>.Write()
        /// and calls <see cref="Target.Flush(AsyncContinuation)"/> on it if LogEvent satisfies the flush condition.
        /// </summary>
        /// <param name="logEvent">Logging event to be written out.</param>
        protected override void Write(AsyncLogEventInfo logEvent)
        {
            object conditionResult = this.Condition.Evaluate(logEvent.LogEvent);
            if (conditionResult.Equals(true))
            {
                var logEventWithFlush =
                    logEvent.LogEvent.WithContinuation(
                        AsyncHelpers.PrecededBy(logEvent.Continuation,this.WrappedTarget.Flush));

                this.WrappedTarget.WriteAsyncLogEvent(logEventWithFlush);
            }
            else
            {
                this.WrappedTarget.WriteAsyncLogEvent(logEvent);
            }
        }
    }
}