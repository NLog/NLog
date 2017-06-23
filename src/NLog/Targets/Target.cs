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
        private readonly object lockObject = new object();
        private List<Layout> allLayouts;
        
        /// <summary> Are all layouts in this target thread-agnostic, if so we don't precalculate the layouts </summary>
        private bool allLayoutsAreThreadAgnostic;
        private bool scannedForLayouts;
        private Exception initializeException;

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
        protected object SyncRoot
        {
            get { return this.lockObject; }
        }

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
                if (this.isInitialized)
                    return true;    // Initialization has completed

                // Lets wait for initialization to complete, and then check again
                lock (this.SyncRoot)
                {
                    return this.isInitialized;
                }
            }
        }
        private volatile bool isInitialized;

        /// <summary>
        /// Can be used if <see cref="OptimizeBufferReuse"/> has been enabled.
        /// </summary>
        internal readonly ReusableBuilderCreator ReusableLayoutBuilder = new ReusableBuilderCreator();

        /// <summary>
        /// Initializes this instance.
        /// </summary>
        /// <param name="configuration">The configuration.</param>
        void ISupportsInitialize.Initialize(LoggingConfiguration configuration)
        {
            lock (this.SyncRoot)
            { 
                bool wasInitialized = this.isInitialized;
                this.Initialize(configuration);
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
            this.Close();
        }

        /// <summary>
        /// Closes the target.
        /// </summary>
        public void Dispose()
        {
            this.Dispose(true);
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
                throw new ArgumentNullException("asyncContinuation");
            }

            asyncContinuation = AsyncHelpers.PreventMultipleCalls(asyncContinuation);

            lock (this.SyncRoot)
            {
                if (!this.IsInitialized)
                {
                    // In case target was Closed
                    asyncContinuation(null);
                    return;
                }

                try
                {
                    this.FlushAsync(asyncContinuation);
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
            if (this.allLayoutsAreThreadAgnostic)
                return;

            // Not all Layouts support concurrent threads, so we have to protect them
            lock (this.SyncRoot)
            {
                if (!this.isInitialized)
                    return;

                if (this.allLayouts != null)
                {
                    if (this.OptimizeBufferReuse)
                    {
                        using (var targetBuilder = this.ReusableLayoutBuilder.Allocate())
                        {
                            foreach (Layout layout in this.allLayouts)
                            {
                                targetBuilder.Result.ClearBuilder();
                                layout.PrecalculateBuilder(logEvent, targetBuilder.Result);
                            }
                        }
                    }
                    else
                    {
                        foreach (Layout layout in this.allLayouts)
                        {
                            layout.Precalculate(logEvent);
                        }
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
            var targetAttribute = (TargetAttribute)Attribute.GetCustomAttribute(this.GetType(), typeof(TargetAttribute));
            if (targetAttribute != null)
            {
                return targetAttribute.Name + " Target[" + (this.Name ?? "(unnamed)") + "]";
            }

            return this.GetType().Name;
        }

        /// <summary>
        /// Writes the log to the target.
        /// </summary>
        /// <param name="logEvent">Log event to write.</param>
        public void WriteAsyncLogEvent(AsyncLogEventInfo logEvent)
        {
            if (!this.IsInitialized)
            {
                lock (this.SyncRoot)
                {
                    logEvent.Continuation(null);
                }
                return;
            }

            if (this.initializeException != null)
            {
                lock (this.SyncRoot)
                {
                    logEvent.Continuation(this.CreateInitException());
                }
                return;
            }

            var wrappedContinuation = AsyncHelpers.PreventMultipleCalls(logEvent.Continuation);
            var wrappedLogEvent = logEvent.LogEvent.WithContinuation(wrappedContinuation);
            this.WriteAsyncThreadSafe(wrappedLogEvent);
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
        internal void WriteAsyncLogEvents(IList<AsyncLogEventInfo> logEvents)
        {
            if (logEvents == null || logEvents.Count == 0)
            {
                return;
            }

            if (!this.IsInitialized)
            {
                lock (this.SyncRoot)
                {
                    for (int i = 0; i < logEvents.Count; ++i)
                    {
                        logEvents[i].Continuation(null);
                    }
                }
                return;
            }

            if (this.initializeException != null)
            {
                lock (this.SyncRoot)
                {
                    for (int i = 0; i < logEvents.Count; ++i)
                    {
                        logEvents[i].Continuation(this.CreateInitException());
                    }
                }
                return;
            }

            IList<AsyncLogEventInfo> wrappedEvents;
            if (this.OptimizeBufferReuse)
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

            this.WriteAsyncThreadSafe(wrappedEvents);
        }

        /// <summary>
        /// Initializes this instance.
        /// </summary>
        /// <param name="configuration">The configuration.</param>
        internal void Initialize(LoggingConfiguration configuration)
        {
            lock (this.SyncRoot)
            {
                this.LoggingConfiguration = configuration;

                if (!this.IsInitialized)
                {
                    PropertyHelper.CheckRequiredParameters(this);
                    try
                    {
                        this.InitializeTarget();
                        this.initializeException = null;
                        if (!scannedForLayouts)
                        {
                            InternalLogger.Debug("InitializeTarget is done but not scanned For Layouts");
                            //this is critical, as we need the layouts. So if base.InitializeTarget() isn't called, we fix the layouts here.
                            FindAllLayouts();
                        }
                    }
                    catch (Exception exception)
                    {
                        InternalLogger.Error(exception, "Error initializing target '{0}'.", this);

                        this.initializeException = exception;

                        if (exception.MustBeRethrown())
                        {
                            throw;
                        }
                    }
                    finally
                    {
                        this.isInitialized = true;
                    }
                }
            }
        }

        /// <summary>
        /// Closes this instance.
        /// </summary>
        internal void Close()
        {
            lock (this.SyncRoot)
            {
                this.LoggingConfiguration = null;

                if (this.IsInitialized)
                {
                    this.isInitialized = false;

                    try
                    {
                        if (this.initializeException == null)
                        {
                            // if Init succeeded, call Close()
                            InternalLogger.Debug("Closing target '{0}'.", this);
                            this.CloseTarget();
                            InternalLogger.Debug("Closed target '{0}'.", this);
                        }
                    }
                    catch (Exception exception)
                    {
                        InternalLogger.Error(exception, "Error closing target '{0}'.", this);

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
            if (disposing)
            {
                if (this.isInitialized)
                {
                    this.isInitialized = false;
                    if (this.initializeException == null)
                    {
                        this.CloseTarget();
                    }
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
            this.allLayouts = ObjectGraphScanner.FindReachableObjects<Layout>(this);
            InternalLogger.Trace("{0} has {1} layouts", this, this.allLayouts.Count);
            this.allLayoutsAreThreadAgnostic = allLayouts.All(layout => layout.ThreadAgnostic);
            this.StackTraceUsage = allLayouts.DefaultIfEmpty().Max(layout => layout == null ? StackTraceUsage.None : layout.StackTraceUsage);
            this.scannedForLayouts = true;
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
                this.MergeEventProperties(logEvent.LogEvent);

                this.Write(logEvent.LogEvent);
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
        /// </summary>
        /// <param name="logEvent">Log event to be written out.</param>
        protected virtual void WriteAsyncThreadSafe(AsyncLogEventInfo logEvent)
        {
            lock (this.SyncRoot)
            {
                if (!this.IsInitialized)
                {
                    // In case target was Closed
                    logEvent.Continuation(null);
                    return;
                }

                try
                {
                    this.Write(logEvent);
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
        }

        /// <summary>
        /// NOTE! Obsolete, instead override Write(IList{AsyncLogEventInfo} logEvents)
        /// 
        /// Writes an array of logging events to the log target. By default it iterates on all
        /// events and passes them to "Write" method. Inheriting classes can use this method to
        /// optimize batch writes.
        /// </summary>
        /// <param name="logEvents">Logging events to be written out.</param>
        [Obsolete("Instead override Write(IList<AsyncLogEventInfo> logEvents. Marked obsolete on NLog 4.5")]
        protected virtual void Write(AsyncLogEventInfo[] logEvents)
        {
            Write((IList<AsyncLogEventInfo>)logEvents);
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
                this.Write(logEvents[i]);
            }
        }

        /// <summary>
        /// NOTE! Obsolete, instead override WriteAsyncThreadSafe(IList{AsyncLogEventInfo} logEvents)
        /// 
        /// Writes an array of logging events to the log target, in a thread safe manner.
        /// </summary>
        /// <param name="logEvents">Logging events to be written out.</param>
        [Obsolete("Instead override WriteAsyncThreadSafe(IList<AsyncLogEventInfo> logEvents. Marked obsolete on NLog 4.5")]
        protected virtual void WriteAsyncThreadSafe(AsyncLogEventInfo[] logEvents)
        {
            WriteAsyncThreadSafe((IList<AsyncLogEventInfo>)logEvents);
        }

        /// <summary>
        /// Writes an array of logging events to the log target, in a thread safe manner.
        /// </summary>
        /// <param name="logEvents">Logging events to be written out.</param>
        protected virtual void WriteAsyncThreadSafe(IList<AsyncLogEventInfo> logEvents)
        {
            lock (this.SyncRoot)
            {
                if (!this.IsInitialized)
                {
                    // In case target was Closed
                    for (int i = 0; i < logEvents.Count; ++i)
                    {
                        logEvents[i].Continuation(null);
                    }
                    return;
                }

                try
                {
                    AsyncLogEventInfo[] logEventsArray = this.OptimizeBufferReuse ? null : logEvents as AsyncLogEventInfo[];
                    if (!this.OptimizeBufferReuse && logEventsArray != null)
                    {
                        // Backwards compatibility
#pragma warning disable 612, 618
                        this.Write(logEventsArray);
#pragma warning restore 612, 618
                    }
                    else
                    {
                        this.Write(logEvents);
                    }
                }
                catch (Exception exception)
                {
                    if (exception.MustBeRethrown())
                    {
                        throw;
                    }

                    // in case of synchronous failure, assume that nothing is running asynchronously
                    for (int i = 0; i < logEvents.Count; ++i)
                    {
                        logEvents[i].Continuation(exception);
                    }
                }
            }
        }

        private Exception CreateInitException()
        {
            return new NLogRuntimeException("Target " + this + " failed to initialize.", this.initializeException);
        }

        /// <summary>
        /// Merges (copies) the event context properties from any event info object stored in
        /// parameters of the given event info object.
        /// </summary>
        /// <param name="logEvent">The event info object to perform the merge to.</param>
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
                var logEventParameter = logEvent.Parameters[i] as LogEventInfo;
                if (logEventParameter != null && logEventParameter.HasProperties)
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
            if (OptimizeBufferReuse)
            {
                SimpleLayout simpleLayout = layout as SimpleLayout;
                if (simpleLayout != null && simpleLayout.IsFixedText)
                    return simpleLayout.Render(logEvent);

                using (var localTarget = ReusableLayoutBuilder.Allocate())
                {
                    return layout.RenderAllocateBuilder(logEvent, localTarget.Result, false);
                }
            }
            else
            {
                return layout.Render(logEvent);
            }
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
