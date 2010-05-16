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

    using NLog.Common;
    using NLog.Config;
    using NLog.Internal;
    using NLog.Layouts;

    /// <summary>
    /// Represents logging target.
    /// </summary>
    public abstract class Target : ISupportsInitialize, INLogConfigurationItem, IDisposable
    {
        private List<Layout> allLayouts;

        /// <summary>
        /// Gets or sets the name of the target.
        /// </summary>
        /// <docgen category='General Options' order='10' />
        public string Name { get; set; }

        /// <summary>
        /// Flush any pending log messages (in case of asynchronous targets).
        /// </summary>
        /// <param name="asyncContinuation">The asynchronous continuation.</param>
        public void Flush(AsyncContinuation asyncContinuation)
        {
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

        /// <summary>
        /// Flush any pending log messages asynchronously (in case of asynchronous targets).
        /// </summary>
        /// <param name="asyncContinuation">The asynchronous continuation.</param>
        protected virtual void FlushAsync(AsyncContinuation asyncContinuation)
        {
            asyncContinuation(null);
        }

        /// <summary>
        /// Writes the log to the target.
        /// </summary>
        /// <param name="logEvent">Log event to write.</param>
        /// <param name="asyncContinuation">The asynchronous continuation.</param>
        public void WriteLogEvent(LogEventInfo logEvent, AsyncContinuation asyncContinuation)
        {
            asyncContinuation = AsyncHelpers.OneTimeOnly(asyncContinuation);

            try
            {
                this.Write(logEvent, asyncContinuation);
            }
            catch (Exception ex)
            {
                asyncContinuation(ex);
            }
        }

        /// <summary>
        /// Writes the array of log events.
        /// </summary>
        /// <param name="logEvents">The log events.</param>
        /// <param name="asyncContinuations">The asynchronous continuations.</param>
        public void WriteLogEvents(LogEventInfo[] logEvents, AsyncContinuation[] asyncContinuations)
        {
            var continuations = new AsyncContinuation[asyncContinuations.Length];
            for (int i = 0; i < continuations.Length; ++i)
            {
                continuations[i] = AsyncHelpers.OneTimeOnly(asyncContinuations[i]);
            }

            try
            {
                this.Write(logEvents, continuations);
            }
            catch (Exception ex)
            {
                // in case of synchronous failure, assume that nothing is running asynchronously
                foreach (var cont in continuations)
                {
                    cont(ex);
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
            foreach (Layout l in this.allLayouts)
            {
                l.Precalculate(logEvent);
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
        /// Closes the target.
        /// </summary>
        public void Dispose()
        {
            this.Close();
            GC.SuppressFinalize(true);
        }

        /// <summary>
        /// Initializes this instance.
        /// </summary>
        void ISupportsInitialize.Initialize()
        {
            this.Initialize();
        }

        /// <summary>
        /// Closes this instance.
        /// </summary>
        void ISupportsInitialize.Close()
        {
            this.Close();
        }

        /// <summary>
        /// Closes the target and releases any unmanaged resources.
        /// </summary>
        protected virtual void Close()
        {
        }

        /// <summary>
        /// Initializes the target. Can be used by inheriting classes
        /// to initialize logging.
        /// </summary>
        protected virtual void Initialize()
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
        /// <param name="asyncContinuation">The asynchronous continuation.</param>
        protected virtual void Write(LogEventInfo logEvent, AsyncContinuation asyncContinuation)
        {
            try
            {
                this.Write(logEvent);
                asyncContinuation(null);
            }
            catch (Exception ex)
            {
                asyncContinuation(ex);
            }
        }

        /// <summary>
        /// Writes an array of logging events to the log target. By default it iterates on all
        /// events and passes them to "Write" method. Inheriting classes can use this method to
        /// optimize batch writes.
        /// </summary>
        /// <param name="logEvents">Logging events to be written out.</param>
        /// <param name="asyncContinuations">The asynchronous continuations.</param>
        protected virtual void Write(LogEventInfo[] logEvents, AsyncContinuation[] asyncContinuations)
        {
            for (int i = 0; i < logEvents.Length; ++i)
            {
                this.Write(logEvents[i], asyncContinuations[i]);
            }
        }

        private void GetAllLayouts()
        {
            this.allLayouts = new List<Layout>(ObjectGraphScanner.FindReachableObjects<Layout>(this));
            InternalLogger.Trace("{0} has {1} layouts", this, this.allLayouts.Count);
        }
    }
}