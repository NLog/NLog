// 
// Copyright (c) 2004-2019 Jaroslaw Kowalski <jaak@jkowalski.net>, Kim Christensen, Julian Verdurmen
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
    using System.Collections.Generic;
    using System.Linq;

    using NLog.Common;
    using NLog.Config;
    using NLog.Internal;
    using NLog.Layouts;

    /// <summary>
    /// Represents logging target.
    /// </summary>
    [NLogConfigurationItem]
    public abstract class Target : ISupportsInitialize, IDisposable
    {
        private List<Layout> _allLayouts;
        
        /// <summary> Are all layouts in this target thread-agnostic, if so we don't precalculate the layouts </summary>
        private bool _allLayoutsAreThreadAgnostic;
        private bool _allLayoutsAreThreadSafe;
        private bool _oneLayoutIsMutableUnsafe;
        private bool _scannedForLayouts;
        private Exception _initializeException;

        /// <summary>
        /// The Max StackTraceUsage of all the <see cref="Layout"/> in this Target
        /// </summary>
        internal StackTraceUsage StackTraceUsage { get; private set; }

        /// <summary>
        /// Gets or sets the name of the target.
        /// </summary>
        /// <docgen category='General Options' order='10' />
        public string Name { get; set; }

        /// <summary>
        /// Target supports reuse of internal buffers, and doesn't have to constantly allocate new buffers
        /// Required for legacy NLog-targets, that expects buffers to remain stable after Write-method exit
        /// </summary>
        /// <docgen category='Performance Tuning Options' order='10' />
        public bool OptimizeBufferReuse { get; set; }

        /// <summary>
        /// Gets the object which can be used to synchronize asynchronous operations that must rely on the .
        /// </summary>
        protected object SyncRoot { get; } = new object();

        /// <summary>
        /// Gets the logging configuration this target is part of.
        /// </summary>
        protected LoggingConfiguration LoggingConfiguration { get; private set; }

        /// <summary>
        /// Gets a value indicating whether the target has been initialized.
        /// </summary>
        protected bool IsInitialized
        {
            get
            {
                if (_isInitialized)
                    return true;    // Initialization has completed

                // Lets wait for initialization to complete, and then check again
                lock (SyncRoot)
                {
                    return _isInitialized;
                }
            }
        }
        private volatile bool _isInitialized;

        /// <summary>
        /// Can be used if <see cref="OptimizeBufferReuse"/> has been enabled.
        /// </summary>
        internal readonly ReusableBuilderCreator ReusableLayoutBuilder = new ReusableBuilderCreator();
        private StringBuilderPool _precalculateStringBuilderPool;

        /// <summary>
        /// Initializes this instance.
        /// </summary>
        /// <param name="configuration">The configuration.</param>
        void ISupportsInitialize.Initialize(LoggingConfiguration configuration)
        {
            lock (SyncRoot)
            { 
                bool wasInitialized = _isInitialized;
                Initialize(configuration);
                if (wasInitialized && configuration != null)
                {
                    FindAllLayouts();
                }
            }
        }

        /// <summary>
        /// Closes this instance.
        /// </summary>
        void ISupportsInitialize.Close()
        {
            Close();
        }

        /// <summary>
        /// Closes the target.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Flush any pending log messages (in case of asynchronous targets).
        /// </summary>
        /// <param name="asyncContinuation">The asynchronous continuation.</param>
        public void Flush(AsyncContinuation asyncContinuation)
        {
            if (asyncContinuation == null)
            {
                throw new ArgumentNullException(nameof(asyncContinuation));
            }

            asyncContinuation = AsyncHelpers.PreventMultipleCalls(asyncContinuation);

            lock (SyncRoot)
            {
                if (!IsInitialized)
                {
                    // In case target was Closed
                    asyncContinuation(null);
                    return;
                }

                try
                {
                    FlushAsync(asyncContinuation);
                }
                catch (Exception exception)
                {
                    if (exception.MustBeRethrown())
                    {
                        throw;
                    }

                    asyncContinuation(exception);
                }
            }
        }

        /// <summary>
        /// Calls the <see cref="Layout.Precalculate"/> on each volatile layout
        /// used by this target.
        /// This method won't prerender if all layouts in this target are thread-agnostic.
        /// </summary>
        /// <param name="logEvent">
        /// The log event.
        /// </param>
        public void PrecalculateVolatileLayouts(LogEventInfo logEvent)
        {
            if (_allLayoutsAreThreadAgnostic && (!_oneLayoutIsMutableUnsafe || logEvent.IsLogEventMutableSafe()))
            {
                return;
            }

            // Not all Layouts support concurrent threads, so we have to protect them
            if (OptimizeBufferReuse && _allLayoutsAreThreadSafe)
            {
                PrecalculateVolatileLayoutsConcurrent(logEvent);
            }
            else
            {
                PrecalculateVolatileLayoutsWithLock(logEvent);
            }
        }

        private void PrecalculateVolatileLayoutsConcurrent(LogEventInfo logEvent)
        {
            if (!IsInitialized)
                return;

            if (_allLayouts == null)
                return;

            if (_precalculateStringBuilderPool == null)
            {
                System.Threading.Interlocked.CompareExchange(ref _precalculateStringBuilderPool, new StringBuilderPool(Environment.ProcessorCount * 2), null);
            }

            using (var targetBuilder = _precalculateStringBuilderPool.Acquire())
            {
                foreach (Layout layout in _allLayouts)
                {
                    targetBuilder.Item.ClearBuilder();
                    layout.PrecalculateBuilder(logEvent, targetBuilder.Item);
                }
            }
        }

        private void PrecalculateVolatileLayoutsWithLock(LogEventInfo logEvent)
        {
            lock (SyncRoot)
            {
                if (!_isInitialized)
                    return;

                if (_allLayouts == null)
                    return;

                if (OptimizeBufferReuse)
                {
                    using (var targetBuilder = ReusableLayoutBuilder.Allocate())
                    {
                        foreach (Layout layout in _allLayouts)
                        {
                            targetBuilder.Result.ClearBuilder();
                            layout.PrecalculateBuilder(logEvent, targetBuilder.Result);
                        }
                    }
                }
                else
                {
                    foreach (Layout layout in _allLayouts)
                    {
                        layout.Precalculate(logEvent);
                    }
                }
            }
        }

        /// <summary>
        /// Returns a <see cref="System.String"/> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String"/> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            var targetAttribute = GetType().GetFirstCustomAttribute<TargetAttribute>();
            if (targetAttribute != null)
            {
                return $"{targetAttribute.Name} Target[{(Name ?? "(unnamed)")}]";
            }

            return GetType().Name;
        }

        /// <summary>
        /// Writes the log to the target.
        /// </summary>
        /// <param name="logEvent">Log event to write.</param>
        public void WriteAsyncLogEvent(AsyncLogEventInfo logEvent)
        {
            if (!IsInitialized)
            {
                lock (SyncRoot)
                {
                    logEvent.Continuation(null);
                }
                return;
            }

            if (_initializeException != null)
            {
                lock (SyncRoot)
                {
                    logEvent.Continuation(CreateInitException());
                }
                return;
            }

            var wrappedContinuation = AsyncHelpers.PreventMultipleCalls(logEvent.Continuation);
            var wrappedLogEvent = logEvent.LogEvent.WithContinuation(wrappedContinuation);
            try
            {
                WriteAsyncThreadSafe(wrappedLogEvent);
            }
            catch (Exception ex)
            {
                if (ex.MustBeRethrown())
                    throw;

                wrappedLogEvent.Continuation(ex);
            }
        }

        /// <summary>
        /// Writes the array of log events.
        /// </summary>
        /// <param name="logEvents">The log events.</param>
        public void WriteAsyncLogEvents(params AsyncLogEventInfo[] logEvents)
        {
            if (logEvents == null || logEvents.Length == 0)
            {
                return;
            }

            WriteAsyncLogEvents((IList<AsyncLogEventInfo>)logEvents);
        }

        /// <summary>
        /// Writes the array of log events.
        /// </summary>
        /// <param name="logEvents">The log events.</param>
        public void WriteAsyncLogEvents(IList<AsyncLogEventInfo> logEvents)
        {
            if (logEvents == null || logEvents.Count == 0)
            {
                return;
            }

            if (!IsInitialized)
            {
                lock (SyncRoot)
                {
                    for (int i = 0; i < logEvents.Count; ++i)
                    {
                        logEvents[i].Continuation(null);
                    }
                }
                return;
            }

            if (_initializeException != null)
            {
                lock (SyncRoot)
                {
                    for (int i = 0; i < logEvents.Count; ++i)
                    {
                        logEvents[i].Continuation(CreateInitException());
                    }
                }
                return;
            }

            IList<AsyncLogEventInfo> wrappedEvents;
            if (OptimizeBufferReuse)
            {
                for (int i = 0; i < logEvents.Count; ++i)
                {
                    logEvents[i] = logEvents[i].LogEvent.WithContinuation(AsyncHelpers.PreventMultipleCalls(logEvents[i].Continuation));
                }
                wrappedEvents = logEvents;
            }
            else
            {
                var cloneLogEvents = new AsyncLogEventInfo[logEvents.Count];
                for (int i = 0; i < logEvents.Count; ++i)
                {
                    AsyncLogEventInfo ev = logEvents[i];
                    cloneLogEvents[i] = ev.LogEvent.WithContinuation(AsyncHelpers.PreventMultipleCalls(ev.Continuation));
                }
                wrappedEvents = cloneLogEvents;
            }

            try
            {
                WriteAsyncThreadSafe(wrappedEvents);
            }
            catch (Exception exception)
            {
                if (exception.MustBeRethrown())
                {
                    throw;
                }

                // in case of synchronous failure, assume that nothing is running asynchronously
                for (int i = 0; i < wrappedEvents.Count; ++i)
                {
                    wrappedEvents[i].Continuation(exception);
                }
            }
        }

        /// <summary>
        /// Initializes this instance.
        /// </summary>
        /// <param name="configuration">The configuration.</param>
        internal void Initialize(LoggingConfiguration configuration)
        {
            lock (SyncRoot)
            {
                LoggingConfiguration = configuration;

                if (!IsInitialized)
                {
                    PropertyHelper.CheckRequiredParameters(this);
                    try
                    {
                        InitializeTarget();
                        _initializeException = null;
                        if (!_scannedForLayouts)
                        {
                            InternalLogger.Debug("{0}: InitializeTarget is done but not scanned For Layouts", this);
                            //this is critical, as we need the layouts. So if base.InitializeTarget() isn't called, we fix the layouts here.
                            FindAllLayouts();
                        }
                    }
                    catch (Exception exception)
                    {
                        InternalLogger.Error(exception, "{0}: Error initializing target", this);

                        _initializeException = exception;

                        if (exception.MustBeRethrown())
                        {
                            throw;
                        }
                    }
                    finally
                    {
                        _isInitialized = true;
                    }
                }
            }
        }

        /// <summary>
        /// Closes this instance.
        /// </summary>
        internal void Close()
        {
            lock (SyncRoot)
            {
                LoggingConfiguration = null;

                if (IsInitialized)
                {
                    _isInitialized = false;

                    try
                    {
                        if (_initializeException == null)
                        {
                            // if Init succeeded, call Close()
                            InternalLogger.Debug("Closing target '{0}'.", this);
                            CloseTarget();
                            InternalLogger.Debug("Closed target '{0}'.", this);
                        }
                    }
                    catch (Exception exception)
                    {
                        InternalLogger.Error(exception, "{0}: Error closing target", this);

                        if (exception.MustBeRethrown())
                        {
                            throw;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Releases unmanaged and - optionally - managed resources.
        /// </summary>
        /// <param name="disposing">True to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (disposing && _isInitialized)
            {
                _isInitialized = false;
                if (_initializeException == null)
                {
                    CloseTarget();
                }
            }
        }

        /// <summary>
        /// Initializes the target. Can be used by inheriting classes
        /// to initialize logging.
        /// </summary>
        protected virtual void InitializeTarget()
        {
            //rescan as amount layouts can be changed.
            FindAllLayouts();
        }

        private void FindAllLayouts()
        {
            _allLayouts = ObjectGraphScanner.FindReachableObjects<Layout>(false, this);
            InternalLogger.Trace("{0} has {1} layouts", this, _allLayouts.Count);
            _allLayoutsAreThreadAgnostic = _allLayouts.All(layout => layout.ThreadAgnostic);
            _oneLayoutIsMutableUnsafe = _allLayoutsAreThreadAgnostic && _allLayouts.Any(layout => layout.MutableUnsafe);
            if (!_allLayoutsAreThreadAgnostic || _oneLayoutIsMutableUnsafe)
            {
                _allLayoutsAreThreadSafe = _allLayouts.All(layout => layout.ThreadSafe);
            }
            StackTraceUsage = _allLayouts.DefaultIfEmpty().Max(layout => layout?.StackTraceUsage ?? StackTraceUsage.None);
            if (this is IUsesStackTrace usesStackTrace && usesStackTrace.StackTraceUsage > StackTraceUsage)
                StackTraceUsage = usesStackTrace.StackTraceUsage;
            _scannedForLayouts = true;
        }

        /// <summary>
        /// Closes the target and releases any unmanaged resources.
        /// </summary>
        protected virtual void CloseTarget()
        {
        }

        /// <summary>
        /// Flush any pending log messages asynchronously (in case of asynchronous targets).
        /// </summary>
        /// <param name="asyncContinuation">The asynchronous continuation.</param>
        protected virtual void FlushAsync(AsyncContinuation asyncContinuation)
        {
            asyncContinuation(null);
        }

        /// <summary>
        /// Writes logging event to the log target. Must be overridden in inheriting
        /// classes.
        /// </summary>
        /// <param name="logEvent">Logging event to be written out.</param>
        protected virtual void Write(LogEventInfo logEvent)
        {
            // Override to perform the actual write-operation
        }

        /// <summary>
        /// Writes async log event to the log target.
        /// </summary>
        /// <param name="logEvent">Async Log event to be written out.</param>
        protected virtual void Write(AsyncLogEventInfo logEvent)
        {
            try
            {
                Write(logEvent.LogEvent);
                logEvent.Continuation(null);
            }
            catch (Exception exception)
            {
                if (exception.MustBeRethrown())
                {
                    throw;
                }

                logEvent.Continuation(exception);
            }
        }

        /// <summary>
        /// Writes a log event to the log target, in a thread safe manner.
        /// Any override of this method has to provide their own synchronization mechanism.
        /// 
        /// !WARNING! Custom targets should only override this method if able to provide their
        /// own synchronization mechanism. <see cref="Layout" />-objects are not guaranteed to be
        /// threadsafe, so using them without a SyncRoot-object can be dangerous.
        /// </summary>
        /// <param name="logEvent">Log event to be written out.</param>
        protected virtual void WriteAsyncThreadSafe(AsyncLogEventInfo logEvent)
        {
            lock (SyncRoot)
            {
                if (!IsInitialized)
                {
                    // In case target was Closed
                    logEvent.Continuation(null);
                    return;
                }

                Write(logEvent);
            }
        }

        /// <summary>
        /// Writes an array of logging events to the log target. By default it iterates on all
        /// events and passes them to "Write" method. Inheriting classes can use this method to
        /// optimize batch writes.
        /// </summary>
        /// <param name="logEvents">Logging events to be written out.</param>
        protected virtual void Write(IList<AsyncLogEventInfo> logEvents)
        {
            for (int i = 0; i < logEvents.Count; ++i)
            {
                Write(logEvents[i]);
            }
        }

        /// <summary>
        /// Writes an array of logging events to the log target, in a thread safe manner.
        /// Any override of this method has to provide their own synchronization mechanism.
        /// 
        /// !WARNING! Custom targets should only override this method if able to provide their
        /// own synchronization mechanism. <see cref="Layout" />-objects are not guaranteed to be
        /// threadsafe, so using them without a SyncRoot-object can be dangerous.
        /// </summary>
        /// <param name="logEvents">Logging events to be written out.</param>
        protected virtual void WriteAsyncThreadSafe(IList<AsyncLogEventInfo> logEvents)
        {
            lock (SyncRoot)
            {
                if (!IsInitialized)
                {
                    // In case target was Closed
                    for (int i = 0; i < logEvents.Count; ++i)
                    {
                        logEvents[i].Continuation(null);
                    }
                    return;
                }

                Write(logEvents);
            }
        }

        private Exception CreateInitException()
        {
            return new NLogRuntimeException($"Target {this} failed to initialize.", _initializeException);
        }

        /// <summary>
        /// Merges (copies) the event context properties from any event info object stored in
        /// parameters of the given event info object.
        /// </summary>
        /// <param name="logEvent">The event info object to perform the merge to.</param>
        [Obsolete("Logger.Trace(logEvent) now automatically captures the logEvent Properties. Marked obsolete on NLog 4.6")]
        protected void MergeEventProperties(LogEventInfo logEvent)
        {
            if (logEvent.Parameters == null || logEvent.Parameters.Length == 0)
            {
                return;
            }

            //Memory profiling pointed out that using a foreach-loop was allocating
            //an Enumerator. Switching to a for-loop avoids the memory allocation.
            for (int i = 0; i < logEvent.Parameters.Length; ++i)
            {
                if (logEvent.Parameters[i] is LogEventInfo logEventParameter && logEventParameter.HasProperties)
                {
                    foreach (var key in logEventParameter.Properties.Keys)
                    {
                        logEvent.Properties.Add(key, logEventParameter.Properties[key]);
                    }
                    logEventParameter.Properties.Clear();
                }
            }
        }

        /// <summary>
        /// Renders the event info in layout.
        /// </summary>
        /// <param name="layout">The layout.</param>
        /// <param name="logEvent">The event info.</param>
        /// <returns>String representing log event.</returns>
        protected string RenderLogEvent(Layout layout, LogEventInfo logEvent)
        {
            if (layout == null || logEvent == null)
                return null;    // Signal that input was wrong

            if (OptimizeBufferReuse)
            {
                SimpleLayout simpleLayout = layout as SimpleLayout;
                if (simpleLayout != null && simpleLayout.IsFixedText)
                {
                    return simpleLayout.Render(logEvent);
                }

                if (TryGetCachedValue(layout, logEvent, out var value))
                {
                    return value;
                }

                if (simpleLayout != null && simpleLayout.IsSimpleStringText)
                {
                    return simpleLayout.Render(logEvent);
                }

                using (var localTarget = ReusableLayoutBuilder.Allocate())
                {
                    return layout.RenderAllocateBuilder(logEvent, localTarget.Result);
                }
            }
            else
            {
                return layout.Render(logEvent);
            }
        }

        private static bool TryGetCachedValue(Layout layout, LogEventInfo logEvent, out string value)
        {
            if ((!layout.ThreadAgnostic || layout.MutableUnsafe) && logEvent.TryGetCachedLayoutValue(layout, out var value2))
            {
                {
                    value = value2?.ToString() ?? string.Empty;
                    return true;
                }
            }

            value = null;
            return false;
        }

        /// <summary>
        /// Register a custom Target.
        /// </summary>
        /// <remarks>Short-cut for registing to default <see cref="ConfigurationItemFactory"/></remarks>
        /// <typeparam name="T"> Type of the Target.</typeparam>
        /// <param name="name"> Name of the Target.</param>
        public static void Register<T>(string name)
            where T : Target
        {
            var layoutRendererType = typeof(T);
            Register(name, layoutRendererType);
        }

        /// <summary>
        /// Register a custom Target.
        /// </summary>
        /// <remarks>Short-cut for registing to default <see cref="ConfigurationItemFactory"/></remarks>
        /// <param name="targetType"> Type of the Target.</param>
        /// <param name="name"> Name of the Target.</param>
        public static void Register(string name, Type targetType)
        {
            ConfigurationItemFactory.Default.Targets
                .RegisterDefinition(name, targetType);
        }
    }
}
