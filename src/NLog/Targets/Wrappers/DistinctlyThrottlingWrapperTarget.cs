// 
// Copyright (c) 2004-2018 Konstantin Chernyaev <konstantin.chernyaev@list.ru>
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
// * Neither the name of Konstantin Chernyaev nor the names of its 
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

#define NET3_5


#region usings
using System;
#if !NET3_5
using System.Collections.Concurrent;
#endif
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading;
using NLog.Common;
using NLog.Config;
using NLog.Internal;
#endregion



namespace NLog.Targets.Wrappers
{
    /// <summary>
    /// The first record is to log immediately, then accumulate for a time and flush by timer. Equivalence is taken into account.
    /// </summary>
    [Target("DistinctlyThrottlingWrapper")]
    public class DistinctlyThrottlingWrapperTarget : WrapperTargetBase
    {
#if !NET3_5
        ConcurrentDictionary<AsyncLogEventInfo, Tuple<int, StringBuilder>> _entriesCounts;
#else
        Dictionary<AsyncLogEventInfo, Tuple<int, StringBuilder>> _entriesCounts;
#endif



        class AsyncLogEventInfoEqualityComparer : IEqualityComparer<AsyncLogEventInfo>
        {
            public AsyncLogEventInfoEqualityComparer(bool useFormattedMessage)
            {
                _useFormattedMessage = useFormattedMessage;
            }


            readonly bool _useFormattedMessage = false;


            public bool Equals(AsyncLogEventInfo x, AsyncLogEventInfo y)
            {
                LogEventInfo a = x.LogEvent;
                LogEventInfo b = y.LogEvent;

                return a.LoggerName == b.LoggerName &&
                       a.Level == b.Level &&
                       (_useFormattedMessage
                           ? a.FormattedMessage == b.FormattedMessage
                           : a.Message == b.Message) &&
                       // exception.ToString is very expensive so do it last
                       a.Exception?.ToString() == b.Exception?.ToString();
            }


            public int GetHashCode(AsyncLogEventInfo x)
            {
                LogEventInfo a = x.LogEvent;
                int withoutExc = a.LoggerName.GetHashCode() ^
                                 (_useFormattedMessage ? a.FormattedMessage : a.Message)
                                 .GetHashCode() ^
                                 a.Level.GetHashCode();
                return a.Exception == null
                    ? withoutExc
                    : withoutExc ^ (a.Exception.Message?.GetHashCode() ?? 0) ^
                      a.Exception.GetType().GetHashCode() ^ a.Exception.TargetSite.GetHashCode();
                // do not use a.Exception.StackTrace - i think it is performance impact
            }
        }



        Timer _flushTimer;
        readonly object _lockObject = new object();
        volatile bool _isTimerOnNow;


        #region ctors
        const int FlushTimeoutDefault = 5000;


        /// <summary>
        /// Initializes a new instance of the <see cref="DistinctlyThrottlingWrapperTarget" /> class with default values for properties.
        /// </summary>
        public DistinctlyThrottlingWrapperTarget()
            : this(null, null, FlushTimeoutDefault) { }


        /// <summary>
        /// Initializes a new instance of the <see cref="DistinctlyThrottlingWrapperTarget" /> class with default values for properties.
        /// </summary>
        /// <param name="name">Name of the target.</param>
        /// <param name="wrappedTarget">The wrapped target.</param>
        public DistinctlyThrottlingWrapperTarget(string name, Target wrappedTarget)
            : this(name, wrappedTarget, FlushTimeoutDefault) { }


        /// <summary>
        /// Initializes a new instance of the <see cref="DistinctlyThrottlingWrapperTarget" /> class with default values for properties.
        /// </summary>
        /// <param name="wrappedTarget">The wrapped target.</param>
        public DistinctlyThrottlingWrapperTarget(Target wrappedTarget)
            : this(null, wrappedTarget, FlushTimeoutDefault) { }


        /// <summary>
        /// Initializes a new instance of the <see cref="DistinctlyThrottlingWrapperTarget" /> class
        /// </summary>
        /// <param name="name"></param>
        /// <param name="wrappedTarget"></param>
        /// <param name="flushTimeout"></param>
        public DistinctlyThrottlingWrapperTarget(string name, Target wrappedTarget,
            int flushTimeout)
        {
            Name = name;
            WrappedTarget = wrappedTarget;
            FlushTimeout = flushTimeout;
            GroupByTemplate = true;
        }
        #endregion


        /// <summary>
        /// Gets or sets the timeout (in milliseconds) after which the contents of buffer will be flushed 
        /// </summary>
        [RequiredParameter]
        [DefaultValue(5000)]
        public int FlushTimeout { get; set; } = FlushTimeoutDefault;


        /// <summary>
        /// just backing field for GroupByTemplate
        /// </summary>
        bool _GroupByTemplate;


        /// <summary>
        /// 
        /// </summary>
        [DefaultValue(true)]
        public bool GroupByTemplate
        {
            get { return _GroupByTemplate; }
            set
            {
                _GroupByTemplate = value;
#if !NET3_5
                _entriesCounts =
                    new ConcurrentDictionary<AsyncLogEventInfo, Tuple<int, StringBuilder>>(
                        new AsyncLogEventInfoEqualityComparer(!_GroupByTemplate));
#else
                _entriesCounts =
                    new Dictionary<AsyncLogEventInfo, Tuple<int, StringBuilder>>(
                        new AsyncLogEventInfoEqualityComparer(!_GroupByTemplate));
#endif
            }
        }


        /// <summary>
        /// 
        /// </summary>
        [DefaultValue("\\n")]
        public string GroupByTemplateSeparator { get; set; } = Environment.NewLine;


        /// <summary>
        /// Append count of waiting accumulated messages to the <see cref="LogEventInfo.Message"/> when this wrapper is flushed. Pattern {0} means the place for count for string.Format.
        /// For example, " (Hits: {0})"
        /// </summary>
        [DefaultValue(" - {0} times:")]
        public string CountAppendFormat { get; set; } = " - {0} times";


        /// <inheritdoc />
        protected override void InitializeTarget()
        {
            base.InitializeTarget();
            InternalLogger.Trace("BufferingWrapper(Name={0}): Create Timer", Name);
            _flushTimer = new Timer(FlushCallback, null, Timeout.Infinite, Timeout.Infinite);
        }


        /// <summary>
        /// Flushes pending events in the buffer (if any), followed by flushing the WrappedTarget.
        /// </summary>
        /// <param name="asyncContinuation">The asynchronous continuation.</param>
        protected override void FlushAsync(AsyncContinuation asyncContinuation)
        {
            Flush();
            base.FlushAsync(asyncContinuation);
        }


        /// <summary>
        /// Closes the target by flushing pending events in the buffer (if any).
        /// </summary>
        protected override void CloseTarget()
        {
            Timer currentTimer = _flushTimer;
            if (currentTimer != null)
            {
                _flushTimer = null;
                if (currentTimer.WaitForDispose(TimeSpan.FromSeconds(1)))
                    lock (_lockObject)
                        Flush();
            }

            base.CloseTarget();
        }


        /// <inheritdoc />
        /// <summary>
        /// The first record is to log immediately, then accumulate for a time and flush by timer. Equivalence is taken into account.
        /// </summary>
        protected override void Write(AsyncLogEventInfo e)
        {
            PrecalculateVolatileLayouts(e.LogEvent);

#if !NET3_5
            Tuple<int, StringBuilder> count = _entriesCounts.AddOrUpdate(e,
                /*do not store first - it is logged out immediately*/
                new Tuple<int, StringBuilder>(0,
                    GroupByTemplate && e.LogEvent.Message.Contains("{")
                        ? new StringBuilder()
                        : null),
                (k, v) =>
                {
                    // but store all the others
                    if (GroupByTemplate && e.LogEvent.Message.Contains("{"))
                    {
                        v.Item2.Append(Escape(e.LogEvent.FormattedMessage));
                        v.Item2.Append(this.GroupByTemplateSeparator);
                    }
                    return new Tuple<int, StringBuilder>(v.Item1 + 1, v.Item2);
                });
#else
            Tuple<int, StringBuilder> count;
            lock (_lockObject)
            {
                if (_entriesCounts.TryGetValue(e, out count))
                {
                    if (GroupByTemplate && e.LogEvent.Message.Contains("{"))
                    {
                        count.Item2.Append(Escape(e.LogEvent.FormattedMessage));
                        count.Item2.Append(this.GroupByTemplateSeparator);
                    }
                    count = new Tuple<int, StringBuilder>(count.Item1 + 1, count.Item2);
                }
                else
                    count = new Tuple<int, StringBuilder>(0,
                        GroupByTemplate && e.LogEvent.Message.Contains("{")
                            ? new StringBuilder()
                            : null);
                _entriesCounts[e] = count;
                //_entriesCounts.Add(e, count);
            }
#endif

            if (count.Item1 == 0)
            {
                WrappedTarget.WriteAsyncLogEvents(e);
                TurnOnTimerIfOffline();
            }
        }


        void TurnOnTimerIfOffline()
        {
            if (!_isTimerOnNow)
            {
                _flushTimer.Change(FlushTimeout, Timeout.Infinite);
                _isTimerOnNow = true;
            }
        }


        void FlushCallback(object _)
        {
            _isTimerOnNow = false;
            try
            {
                lock (_lockObject)
                {
                    if (_flushTimer == null)
                        return;

                    Flush();
                }
            }
            catch (Exception exception)
            {
                InternalLogger.Error(exception,
                    "BufferingWrapper(Name={0}): Error in flush procedure.", Name);

                if (exception.MustBeRethrownImmediately())
                    throw; // Throwing exceptions here will crash the entire application (.NET 2.0 behavior)
            }
        }


        void Flush()
        {
            if (WrappedTarget == null)
            {
                InternalLogger.Error("BufferingWrapper(Name={0}): WrappedTarget is NULL", Name);
                return;
            }

            lock (_lockObject)
            {
#if !NET3_5
                ICollection<AsyncLogEventInfo> keys = _entriesCounts.Keys;
#else
                ICollection<AsyncLogEventInfo> keys = _entriesCounts.Keys.ToList();
#endif
                foreach (AsyncLogEventInfo e in keys)
                {
                    Tuple<int, StringBuilder> count;
#if !NET3_5
                    if (_entriesCounts.TryRemove(e, out count) && count.Item1 > 0)
#else
                    count = _entriesCounts[e];
                    if (_entriesCounts.Remove(e))
#endif
                    {
                        if (count.Item1 > 1 && !string.IsNullOrWhiteSpace(CountAppendFormat))
                            if (GroupByTemplate && e.LogEvent.Message.Contains("{"))
                                // cut off the last?? it is separator - i think do not
                                e.LogEvent.Message = Escape(e.LogEvent.Message) +
                                                     string.Format(CountAppendFormat, count.Item1) +
                                                     (this.GroupByTemplateSeparator ==
                                                      Environment.NewLine
                                                         ? Environment.NewLine
                                                         : "") +
                                                     count.Item2;
                            else
                                e.LogEvent.Message += string.Format(CountAppendFormat, count.Item1);
                        WrappedTarget.WriteAsyncLogEvents(e);
                    }
                }
            }
        }


        static string Escape(string s) => s.Replace("{", "{{").Replace("}", "}}");
    }
}