#region usings
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
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
        readonly ConcurrentDictionary<AsyncLogEventInfo, int> _entriesCounts
            = new ConcurrentDictionary<AsyncLogEventInfo, int>(
                new AsyncLogEventInfoEqualityComparer());



        class AsyncLogEventInfoEqualityComparer : IEqualityComparer<AsyncLogEventInfo>
        {
            public bool Equals(AsyncLogEventInfo x, AsyncLogEventInfo y)
            {
                LogEventInfo a = x.LogEvent;
                LogEventInfo b = y.LogEvent;
                return a.LoggerName == b.LoggerName &&
                       a.FormattedMessage == b.FormattedMessage &&
                       a.Level == b.Level &&
                       a.Exception?.ToString() == b.Exception?.ToString();
            }


            public int GetHashCode(AsyncLogEventInfo x)
            {
                LogEventInfo a = x.LogEvent;
                int noe = a.LoggerName.GetHashCode() ^ a.FormattedMessage.GetHashCode() ^
                          a.Level.GetHashCode();
                return a.Exception == null ? noe : noe ^ a.Exception.GetHashCode();
            }
        }



        Timer _flushTimer;
        readonly object _lockObject = new object();


        #region ctors
        /// <summary>
        /// 
        /// </summary>
        public DistinctlyThrottlingWrapperTarget()
            : this(null, null, 5000) { }


        /// <summary>
        /// 
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
        }
        #endregion


        /// <summary>
        /// Gets or sets the timeout (in milliseconds) after which the contents of buffer will be flushed 
        /// </summary>
        [RequiredParameter]
        public int FlushTimeout { get; set; }


        /// <summary>
        /// Append count of accumulated waiting messages to the <see cref="LogEventInfo.Message"/> when this wrapper is flushed
        /// </summary>
        /// <docgen category='Rendering Options' order='10' />
        [DefaultValue(null)]
        public string AccumulatedCountMessageAppendFormat { get; set; }


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


        /// <summary>
        /// The first record is to log immediately, then accumulate for a time and flush by timer. Equivalence is taken into account.
        /// </summary>
        protected override void Write(AsyncLogEventInfo logEvent)
        {
            PrecalculateVolatileLayouts(logEvent.LogEvent);

            int count = _entriesCounts.AddOrUpdate(logEvent, 0, (k, v) => v + 1);

            if (count == 0)
            {
                WrappedTarget.WriteAsyncLogEvents(logEvent);
                TurnOnTimerIfOffline();
            }
        }


        volatile bool _isTimerOnNow;


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

            //Console.WriteLine("FLUSH " + DateTime.Now);
            //Console.WriteLine(
            //    $"---{string.Join(",", _entriesCounts.Select(kvp => $"{kvp.Key.LogEvent.FormattedMessage}({kvp.Value})"))}");

            lock (_lockObject)
            {
                ICollection<AsyncLogEventInfo> keys = _entriesCounts.Keys;
                foreach (AsyncLogEventInfo e in keys)
                {
                    int count;
                    if (_entriesCounts.TryRemove(e, out count) && count > 0)
                    {
                        if (count > 1 &&
                            !string.IsNullOrWhiteSpace(AccumulatedCountMessageAppendFormat))
                            e.LogEvent.Message += string.Format(AccumulatedCountMessageAppendFormat,
                                count);
                        WrappedTarget.WriteAsyncLogEvents(e);
                    }
                }
            }
        }
    }
}