// 
// Copyright (c) 2004-2020 Jaroslaw Kowalski <jaak@jkowalski.net>, Kim Christensen, Julian Verdurmen
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
    using NLog.Common;
    using NLog.Conditions;
    using NLog.Internal;

    /// <summary>
    /// Causes a flush on a wrapped target if LogEvent satisfies the <see cref="Condition"/>.
    /// If condition isn't set, flushes on each write.
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
        /// Gets or sets the condition expression. Log events who meet this condition will cause
        /// a flush on the wrapped target.
        /// </summary>
        /// <docgen category='General Options' order='10' />
        public ConditionExpression Condition { get; set; }

        /// <summary>
        /// Delay the flush until the LogEvent has been confirmed as written
        /// </summary>
        /// <remarks>If not explicitly set, then disabled by default for <see cref="BufferingTargetWrapper"/> and AsyncTaskTarget
        /// </remarks>
        /// <docgen category='General Options' order='10' />
        public bool AsyncFlush
        {
            get => _asyncFlush ?? true;
            set => _asyncFlush = value;
        }
        private bool? _asyncFlush;

        /// <summary>
        /// Only flush when LogEvent matches condition. Ignore explicit-flush, config-reload-flush and shutdown-flush
        /// </summary>
        /// <docgen category='General Options' order='10' />
        public bool FlushOnConditionOnly { get; set; }

        private readonly AsyncOperationCounter _pendingManualFlushList = new AsyncOperationCounter();

        /// <summary>
        /// Initializes a new instance of the <see cref="AutoFlushTargetWrapper" /> class.
        /// </summary>
        public AutoFlushTargetWrapper()
            : this(null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AutoFlushTargetWrapper" /> class.
        /// </summary>
        /// <param name="wrappedTarget">The wrapped target.</param>
        /// <param name="name">Name of the target</param>
        public AutoFlushTargetWrapper(string name, Target wrappedTarget)
            : this(wrappedTarget)
        {
            Name = name;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AutoFlushTargetWrapper" /> class.
        /// </summary>
        /// <param name="wrappedTarget">The wrapped target.</param>
        public AutoFlushTargetWrapper(Target wrappedTarget)
        {
            WrappedTarget = wrappedTarget;
        }

        /// <summary>
        /// Initializes the target.
        /// </summary>
        protected override void InitializeTarget()
        {
            base.InitializeTarget();
            if (!_asyncFlush.HasValue && !TargetSupportsAsyncFlush(WrappedTarget))
            {
                AsyncFlush = false; // Disable AsyncFlush, so the intended trigger works
            }
        }

        private static bool TargetSupportsAsyncFlush(Target wrappedTarget)
        {
            if (wrappedTarget is BufferingTargetWrapper)
                return false;

#if !NET35
            if (wrappedTarget is AsyncTaskTarget)
                return false;
#endif

            return true;
        }

        /// <summary>
        /// Forwards the call to the <see cref="WrapperTargetBase.WrappedTarget"/>.Write()
        /// and calls <see cref="Target.Flush(AsyncContinuation)"/> on it if LogEvent satisfies
        /// the flush condition or condition is null.
        /// </summary>
        /// <param name="logEvent">Logging event to be written out.</param>
        protected override void Write(AsyncLogEventInfo logEvent)
        {
            if (Condition == null || Condition.Evaluate(logEvent.LogEvent).Equals(true))
            {
                if (AsyncFlush)
                {
                    AsyncContinuation currentContinuation = logEvent.Continuation;
                    AsyncContinuation wrappedContinuation = (ex) =>
                    {
                        if (ex == null)
                            FlushOnCondition();
                        _pendingManualFlushList.CompleteOperation(ex);
                        currentContinuation(ex);
                    };
                    _pendingManualFlushList.BeginOperation();
                    WrappedTarget.WriteAsyncLogEvent(logEvent.LogEvent.WithContinuation(wrappedContinuation));
                }
                else
                {
                    WrappedTarget.WriteAsyncLogEvent(logEvent);
                    FlushOnCondition();
                }
            }
            else
            {
                WrappedTarget.WriteAsyncLogEvent(logEvent);
            }
        }

        /// <summary>
        /// Schedules a flush operation, that triggers when all pending flush operations are completed (in case of asynchronous targets).
        /// </summary>
        /// <param name="asyncContinuation">The asynchronous continuation.</param>
        protected override void FlushAsync(AsyncContinuation asyncContinuation)
        {
            if (FlushOnConditionOnly)
                asyncContinuation(null);
            else
                FlushWrappedTarget(asyncContinuation);
        }

        private void FlushOnCondition()
        {
            if (FlushOnConditionOnly)
                FlushWrappedTarget((e) => { });
            else
                FlushAsync((e) => { });
        }

        private void FlushWrappedTarget(AsyncContinuation asyncContinuation)
        {
            var wrappedContinuation = _pendingManualFlushList.RegisterCompletionNotification(asyncContinuation);
            WrappedTarget.Flush(wrappedContinuation);
        }

        /// <summary>
        /// Closes the target.
        /// </summary>
        protected override void CloseTarget()
        {
            _pendingManualFlushList.Clear(); // Maybe consider to wait a short while if pending requests?
            base.CloseTarget();
        }
    }
}