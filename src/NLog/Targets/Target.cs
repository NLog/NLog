// 
// Copyright (c) 2004-2011 Jaroslaw Kowalski <jaak@jkowalski.net>
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
    using System.Threading;

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
                    foreach (Layout l in this.allLayouts)
                    {
                        l.Precalculate(logEvent);
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
            lock (this.SyncRoot)
            {
                if (!this.IsInitialized)
                {
                    foreach (var ev in logEvents)
                    {
                        ev.Continuation(null);
                    }

                    return;
                }

                if (this.initializeException != null)
                {
                    foreach (var ev in logEvents)
                    {
                        ev.Continuation(this.CreateInitException());
                    }

                    return;
                }

                var wrappedEvents = new AsyncLogEventInfo[logEvents.Length];
                for (int i = 0; i < logEvents.Length; ++i)
                {
                    wrappedEvents[i] = logEvents[i].LogEvent.WithContinuation(AsyncHelpers.PreventMultipleCalls(logEvents[i].Continuation));
                }

                try
                {
                    this.Write(wrappedEvents);
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
                    }
                    catch (Exception exception)
                    {
                        if (exception.MustBeRethrown())
                        {
                            throw;
                        }

                        this.initializeException = exception;
                        InternalLogger.Error("Error initializing target {0} {1}.", this, exception);
                        throw;
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
                        if (exception.MustBeRethrown())
                        {
                            throw;
                        }

                        InternalLogger.Error("Error closing target {0} {1}.", this, exception);
                        throw;
                    }
                }
            }
        }

        internal void WriteAsyncLogEvents(AsyncLogEventInfo[] logEventInfos, AsyncContinuation continuation)
        {
            if (logEventInfos.Length == 0)
            {
                continuation(null);
            }
            else
            {
                var wrappedLogEventInfos = new AsyncLogEventInfo[logEventInfos.Length];
                int remaining = logEventInfos.Length;
                for (int i = 0; i < logEventInfos.Length; ++i)
                {
                    AsyncContinuation originalContinuation = logEventInfos[i].Continuation;
                    AsyncContinuation wrappedContinuation = ex =>
                                                                {
                                                                    originalContinuation(ex);
                                                                    if (0 == Interlocked.Decrement(ref remaining))
                                                                    {
                                                                        continuation(null);
                                                                    }
                                                                };

                    wrappedLogEventInfos[i] = logEventInfos[i].LogEvent.WithContinuation(wrappedContinuation);
                }

                this.WriteAsyncLogEvents(wrappedLogEventInfos);
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
            this.GetAllLayouts();
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
            for (int i = 0; i < logEvents.Length; ++i)
            {
                this.Write(logEvents[i]);
            }
        }

        private Exception CreateInitException()
        {
            return new NLogRuntimeException("Target " + this + " failed to initialize.", this.initializeException);
        }

        private void GetAllLayouts()
        {
            this.allLayouts = new List<Layout>(ObjectGraphScanner.FindReachableObjects<Layout>(this));
            InternalLogger.Trace("{0} has {1} layouts", this, this.allLayouts.Count);
        }
    }
}