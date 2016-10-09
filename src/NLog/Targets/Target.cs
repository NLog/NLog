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
        private bool scannedForLayouts;
        private Exception initializeException;

        internal Internal.PoolFactory.ILogEventObjectFactory _objectFactory = Internal.PoolFactory.LogEventObjectFactory.Instance;

        /// <summary>
        /// Gets or sets the name of the target.
        /// </summary>
        /// <docgen category='General Options' order='10' />
        public string Name { get; set; }

        /// <summary>
        /// Get or sets the object-factory-pool configuration <see cref="PoolSetup"/> for the Target
        /// </summary>
        public PoolSetup PoolSetup { get; set; }

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
        protected bool IsInitialized { get; private set; }

        /// <summary>
        /// Get all used layouts in this target.
        /// </summary>
        /// <returns></returns>
        internal List<Layout> GetAllLayouts()
        {

            if (!scannedForLayouts)
            {
                FindAllLayouts();
            }
            return allLayouts;
        }

        /// <summary>
        /// Initializes this instance.
        /// </summary>
        /// <param name="configuration">The configuration.</param>
        void ISupportsInitialize.Initialize(LoggingConfiguration configuration)
        {
            this.Initialize(configuration);
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

            lock (this.SyncRoot)
            {
                if (!this.IsInitialized)
                {
                    asyncContinuation(null);
                    return;
                }

                asyncContinuation = AsyncHelpers.PreventMultipleCalls(asyncContinuation);

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
        /// </summary>
        /// <param name="logEvent">
        /// The log event.
        /// </param>
        public void PrecalculateVolatileLayouts(LogEventInfo logEvent)
        {
            lock (this.SyncRoot)
            {
                if (this.IsInitialized)
                {
                    if (this.allLayouts != null)
                    {
                        for (int x = 0; x < this.allLayouts.Count; x++)
                        {
                            Layout l = this.allLayouts[x];
                            l.Precalculate(logEvent);
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
            lock (this.SyncRoot)
            {
                if (!this.IsInitialized)
                {
                    logEvent.Continuation(null);
                    return;
                }

                if (this.initializeException != null)
                {
                    logEvent.Continuation(this.CreateInitException());
                    return;
                }

                var wrappedContinuation = AsyncHelpers.PreventMultipleCalls(logEvent.Continuation);

                try
                {
                    this.Write(logEvent.LogEvent.WithContinuation(wrappedContinuation));
                }
                catch (Exception exception)
                {
                    if (exception.MustBeRethrown())
                    {
                        throw;
                    }

                    wrappedContinuation(exception);
                }
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

            WriteAsyncLogEvents(new ArraySegment<AsyncLogEventInfo>(logEvents));
        }

        /// <summary>
        /// Writes the array of log events.
        /// </summary>
        /// <param name="logEvents">The log events.</param>
        public void WriteAsyncLogEvents(ArraySegment<AsyncLogEventInfo> logEvents)
        {
            if (logEvents.Count == 0)
            {
                return;
            }

            lock (this.SyncRoot)
            {
                if (!this.IsInitialized)
                {
                    for (int x = logEvents.Offset; x < logEvents.Offset + logEvents.Count; ++x)
                    {
                        logEvents.Array[x].Continuation(null);
                    }

                    return;
                }

                if (this.initializeException != null)
                {
                    for (int x = logEvents.Offset; x < logEvents.Offset + logEvents.Count; ++x)
                    {
                        logEvents.Array[x].Continuation(this.CreateInitException());
                    }

                    return;
                }

                using (var wrappedArray = _objectFactory.CreateAsyncLogEventArray(logEvents.Count))
                {
                    for (int x = 0; x < logEvents.Count; ++x)
                    {
                        var ev = logEvents.Array[x + logEvents.Offset];
                        wrappedArray.Buffer[x] = ev.LogEvent.WithContinuation(AsyncHelpers.PreventMultipleCalls(ev.Continuation));
                    }

                    try
                    {
                        if (ReferenceEquals(_objectFactory, Internal.PoolFactory.LogEventObjectFactory.Instance))
                        {
                            // Backwards compatibility
#pragma warning disable 612, 618
                            this.Write(wrappedArray.Buffer);
#pragma warning restore 612, 618
                        }
                        else
                        {
                            this.Write(new ArraySegment<AsyncLogEventInfo>(wrappedArray.Buffer, 0, logEvents.Count));
                        }
                    }
                    catch (Exception exception)
                    {
                        if (exception.MustBeRethrown())
                        {
                            throw;
                        }

                        // in case of synchronous failure, assume that nothing is running asynchronously
                        for (int x = 0; x < logEvents.Count; ++x)
                        {
                            wrappedArray.Buffer[x].Continuation(exception);
                        }
                    }
                }
            }
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
                    this.IsInitialized = true;
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
                    this.IsInitialized = false;

                    try
                    {
                        if (this.initializeException == null)
                        {
                            // if Init succeeded, call Close()
                            this.CloseTarget();
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

        internal void WriteAsyncLogEvents(ArraySegment<AsyncLogEventInfo> logEvents, AsyncContinuation continuation)
        {
            if (logEvents.Count == 0)
            {
                continuation(null);
            }
            else
            {
                using (var wrappedArray = _objectFactory.CreateAsyncLogEventArray(logEvents.Count))
                {
                    CompleteWhenAllContinuation.Counter remainingCounter = new CompleteWhenAllContinuation.Counter();
                    remainingCounter.Increment();

                    for (int i = 0; i < logEvents.Count; ++i)
                    {
                        CompleteWhenAllContinuation completeWhenAllDone = _objectFactory.CreateCompleteWhenAllContinuation(remainingCounter);

                        AsyncLogEventInfo ev = logEvents.Array[i + logEvents.Offset];
                        wrappedArray.Buffer[i] = ev.LogEvent.WithContinuation(completeWhenAllDone.CreateContinuation(ev.Continuation, continuation));
                    }

                    this.WriteAsyncLogEvents(new ArraySegment<AsyncLogEventInfo>(wrappedArray.Buffer, 0, logEvents.Count));

                    if (remainingCounter.Decrement()==0)
                    {
                        continuation(null);
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
                this.CloseTarget();
            }
        }

        /// <summary>
        /// Initializes the target. Can be used by inheriting classes
        /// to initialize logging.
        /// </summary>
        protected virtual void InitializeTarget()
        {
            if (LoggingConfiguration != null)
                LoggingConfiguration.ConfigurePool(ref _objectFactory, Name, PoolSetup, false, 0);
            //rescan as amount layouts can be changed.
            FindAllLayouts();
        }

        private void FindAllLayouts()
        {
            this.allLayouts = new List<Layout>(ObjectGraphScanner.FindReachableObjects<Layout>(this));
            InternalLogger.Trace("{0} has {1} layouts", this, this.allLayouts.Count);
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
        /// Writes logging event to the log target.
        /// classes.
        /// </summary>
        /// <param name="logEvent">
        /// Logging event to be written out.
        /// </param>
        protected virtual void Write(LogEventInfo logEvent)
        {
            // do nothing
        }

        /// <summary>
        /// Writes log event to the log target. Must be overridden in inheriting
        /// classes.
        /// </summary>
        /// <param name="logEvent">Log event to be written out.</param>
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
        /// Writes an array of logging events to the log target. By default it iterates on all
        /// events and passes them to "Write" method. Inheriting classes can use this method to
        /// optimize batch writes.
        /// </summary>
        /// <param name="logEvents">Logging events to be written out.</param>
        [Obsolete("Instead use Write(ArraySegment<AsyncLogEventInfo> logEvents)")]
        protected virtual void Write(AsyncLogEventInfo[] logEvents)
        {
            Write(new ArraySegment<AsyncLogEventInfo>(logEvents));
        }

        /// <summary>
        /// Writes an array of logging events to the log target. By default it iterates on all
        /// events and passes them to "Write" method. Inheriting classes can use this method to
        /// optimize batch writes.
        /// </summary>
        /// <param name="logEvents">Logging events to be written out.</param>
        protected virtual void Write(ArraySegment<AsyncLogEventInfo> logEvents)
        {
            for (int i = logEvents.Offset; i < (logEvents.Offset + logEvents.Count); ++i)
            {
                this.Write(logEvents.Array[i]);
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
            if (logEvent.Parameters == null)
            {
                return;
            }

            //Memory profiling pointed out that using a foreach-loop was allocating
            //an Enumerator. Switching to a for-loop avoids the memory allocation.
            for (int i = 0; i< logEvent.Parameters.Length; ++i)
            { 
                var logEventParameter = logEvent.Parameters[i] as LogEventInfo;
                if (logEventParameter != null)
                {
                    foreach (var key in logEventParameter.Properties.Keys)
                    {
                        logEvent.Properties.Add(key, logEventParameter.Properties[key]);
                    }
                    logEventParameter.Properties.Clear();
                }
            }
        }
    }
}