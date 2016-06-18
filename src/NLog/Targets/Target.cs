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

using NLog.Internal.Pooling;
using NLog.Internal.Pooling.Pools;

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
        private object lockObject = new object();
        private List<Layout> allLayouts;
        private bool scannedForLayouts;
        private Exception initializeException;

        /// <summary>
        /// Gets or sets the name of the target.
        /// </summary>
        /// <docgen category='General Options' order='10' />
        public string Name { get; set; }

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

                asyncContinuation = AsyncHelpers.PreventMultipleCalls(this.LoggingConfiguration, asyncContinuation);

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

                AsyncContinuation wrappedContinuation = AsyncHelpers.PreventMultipleCalls(this.LoggingConfiguration, logEvent.Continuation); ;

                // Create async continuation to put log item back into the pool
                if (this.LoggingConfiguration.PoolingEnabled())
                {
                    var pool = this.LoggingConfiguration.PoolFactory.Get<CombinedAsyncContinuationPool, CombinedAsyncContinuation>();

                    wrappedContinuation = pool.Get(wrappedContinuation, logEvent.LogEvent.PutBackDelegate).Delegate;
                }

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

            this.WriteAsyncLogEvents(new ArraySegment<AsyncLogEventInfo>(logEvents));
        }


        internal void WriteAsyncLogEvents(ArraySegment<AsyncLogEventInfo> logEvents)
        {
            if (logEvents.Count == 0)
            {
                return;
            }

            lock (this.SyncRoot)
            {
                if (!this.IsInitialized)
                {
                    for(int x=0;x<logEvents.Count;x++)
                    {
                        var ev = logEvents.Array[x];

                        ev.Continuation(null);
                    }

                    return;
                }

                if (this.initializeException != null)
                {
                    for (int x = 0; x < logEvents.Count; x++)
                    {
                        var ev = logEvents.Array[x];
                        ev.Continuation(this.CreateInitException());
                    }

                    return;
                }
                AsyncLogEventInfo[] wrappedEvents;
                if (this.LoggingConfiguration.PoolingEnabled())
                {
                    wrappedEvents = this.LoggingConfiguration.PoolFactory.Get<AsyncLogEventInfoArrayPool, AsyncLogEventInfo[]>().Get(logEvents.Count);
                }
                else
                {
                    wrappedEvents = new AsyncLogEventInfo[logEvents.Count];
                }
                for (int i = 0; i < logEvents.Count; ++i)
                {
                    wrappedEvents[i] = logEvents.Array[i].LogEvent.WithContinuation(AsyncHelpers.PreventMultipleCalls(this.LoggingConfiguration, logEvents.Array[i].Continuation));
                }

                try
                {
                    this.Write(new ArraySegment<AsyncLogEventInfo>(wrappedEvents,0,logEvents.Count));
                }
                catch (Exception exception)
                {
                    if (exception.MustBeRethrown())
                    {
                        throw;
                    }

                    // in case of synchronous failure, assume that nothing is running asynchronously
                    foreach (var ev in wrappedEvents)
                    {
                        ev.Continuation(exception);
                    }
                }
                finally
                {
                    this.LoggingConfiguration.PutBack(wrappedEvents);
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

        internal void WriteAsyncLogEvents(AsyncLogEventInfo[] logEventInfos, AsyncContinuation continuation)
        {
            this.WriteAsyncLogEvents(new ArraySegment<AsyncLogEventInfo>(logEventInfos), continuation);
        }

        internal void WriteAsyncLogEvents(ArraySegment<AsyncLogEventInfo> logEventInfos, AsyncContinuation continuation)
        {
            if (logEventInfos.Count == 0)
            {
                continuation(null);
            }
            else
            {
                AsyncLogEventInfo[] wrappedLogEventInfos = null;
                try
                {
                    Counter counter;

                    GenericPool<ContinueWhenAll> continuePool = null;
                    int remaining = logEventInfos.Count;

                    if (this.LoggingConfiguration.PoolingEnabled())
                    {
                        counter = this.LoggingConfiguration.PoolFactory.Get<Counter>();
                        continuePool = this.LoggingConfiguration.PoolFactory.Get<GenericPool<ContinueWhenAll>, ContinueWhenAll>();
                        wrappedLogEventInfos = this.LoggingConfiguration.PoolFactory.Get<AsyncLogEventInfoArrayPool, AsyncLogEventInfo[]>().Get(logEventInfos.Count);
                    }
                    else
                    {
                        wrappedLogEventInfos = new AsyncLogEventInfo[logEventInfos.Count];
                        counter = new Counter();
                    }

                    counter.Reset(remaining);

                    for (int i = 0; i < logEventInfos.Count; ++i)
                    {
                        AsyncContinuation originalContinuation = logEventInfos.Array[i].Continuation;

                        ContinueWhenAll cont;
                        if (continuePool != null)
                        {
                            cont = continuePool.Get();
                            cont.Reset(counter, originalContinuation, continuation);
                            wrappedLogEventInfos[i] = logEventInfos.Array[i].LogEvent.WithContinuation(cont.Delegate);
                        }
                        else
                        {
                            cont = new ContinueWhenAll();

                        }
                        cont.Reset(counter, originalContinuation, continuation);
                        wrappedLogEventInfos[i] = logEventInfos.Array[i].LogEvent.WithContinuation(cont.Delegate);

                    }

                    this.WriteAsyncLogEvents(new ArraySegment<AsyncLogEventInfo>(wrappedLogEventInfos,0, logEventInfos.Count));
                }
                finally
                {
                    this.LoggingConfiguration.PutBack(wrappedLogEventInfos);
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
            //rescan as amount layouts can be changed.
            FindAllLayouts();
        }

        private void FindAllLayouts()
        {
            this.allLayouts = new List<Layout>(ObjectGraphScanner.FindReachableObjects<Layout>(this));
            InternalLogger.Trace("{0} has {1} layouts", this, this.allLayouts.Count.AsString());
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
        protected virtual void Write(AsyncLogEventInfo[] logEvents)
        {
            this.Write(new ArraySegment<AsyncLogEventInfo>(logEvents));
        }
        

        /// <summary>
        /// Writes an array of logging events to the log target. By default it iterates on all
        /// events and passes them to "Write" method. Inheriting classes can use this method to
        /// optimize batch writes.
        /// </summary>
        /// <param name="logEvents">Logging events to be written out.</param>
        protected virtual void Write(ArraySegment<AsyncLogEventInfo> logEvents)
        {
            for (int i = 0; i < logEvents.Count; ++i)
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

            foreach (var item in logEvent.Parameters)
            {
                var logEventParameter = item as LogEventInfo;
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