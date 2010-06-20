// 
// Copyright (c) 2004-2010 Jaroslaw Kowalski <jaak@jkowalski.net>
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
    public abstract class Target : ISupportsInitialize, INLogConfigurationItem, IDisposable
    {
        private object lockObject = new object();
        private List<Layout> allLayouts;

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
        /// Gets a value indicating whether the target has been initialized.
        /// </summary>
        protected bool IsInitialized { get; private set; }

        /// <summary>
        /// Initializes this instance.
        /// </summary>
        public void Initialize()
        {
            lock (this.SyncRoot)
            {
                if (!this.IsInitialized)
                {
                    PropertyHelper.CheckRequiredParameters(this);
                    this.InitializeTarget();
                    this.IsInitialized = true;
                }
            }
        }

        /// <summary>
        /// Closes this instance.
        /// </summary>
        public void Close()
        {
            lock (this.SyncRoot)
            {
                if (this.IsInitialized)
                {
                    this.CloseTarget();
                    this.IsInitialized = false;
                }
            }
        }

        /// <summary>
        /// Closes the target.
        /// </summary>
        void IDisposable.Dispose()
        {
            this.CloseTarget();
            GC.SuppressFinalize(true);
        }

        /// <summary>
        /// Flush any pending log messages (in case of asynchronous targets).
        /// </summary>
        /// <param name="asyncContinuation">The asynchronous continuation.</param>
        public void Flush(AsyncContinuation asyncContinuation)
        {
            lock (this.SyncRoot)
            {
                if (!this.IsInitialized)
                {
                    asyncContinuation(null);
                    return;
                }

                asyncContinuation = AsyncHelpers.OneTimeOnly(asyncContinuation);

                try
                {
                    this.FlushAsync(asyncContinuation);
                }
                catch (Exception ex)
                {
                    asyncContinuation(ex);
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

                var wrappedContinuation = AsyncHelpers.OneTimeOnly(logEvent.Continuation);

                try
                {
                    this.Write(logEvent.LogEvent.WithContinuation(wrappedContinuation));
                }
                catch (Exception ex)
                {
                    wrappedContinuation(ex);
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

                var wrappedEvents = new AsyncLogEventInfo[logEvents.Length];
                for (int i = 0; i < logEvents.Length; ++i)
                {
                    wrappedEvents[i] = logEvents[i].LogEvent.WithContinuation(AsyncHelpers.OneTimeOnly(logEvents[i].Continuation));
                }

                try
                {
                    this.Write(wrappedEvents);
                }
                catch (Exception ex)
                {
                    // in case of synchronous failure, assume that nothing is running asynchronously
                    foreach (var ev in wrappedEvents)
                    {
                        ev.Continuation(ex);
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
        /// Initializes the target. Can be used by inheriting classes
        /// to initialize logging.
        /// </summary>
        protected virtual void InitializeTarget()
        {
            this.GetAllLayouts();
        }

        /// <summary>
        /// Writes logging event to the log target. Must be overridden in inheriting
        /// classes.
        /// </summary>
        /// <param name="logEvent">
        /// Logging event to be written out.
        /// </param>
        protected abstract void Write(LogEventInfo logEvent);

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
            catch (Exception ex)
            {
                logEvent.Continuation(ex);
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

        private void GetAllLayouts()
        {
            this.allLayouts = new List<Layout>(ObjectGraphScanner.FindReachableObjects<Layout>(this));
            InternalLogger.Trace("{0} has {1} layouts", this, this.allLayouts.Count);
        }
    }
}