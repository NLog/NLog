// 
// Copyright (c) 2004-2009 Jaroslaw Kowalski <jaak@jkowalski.net>
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

using System;
using System.Collections.Generic;
using NLog.Common;
using NLog.Config;
using NLog.Layouts;

namespace NLog.Targets
{
    /// <summary>
    /// Represents logging target.
    /// </summary>
    public abstract class Target : IDisposable
    {
        private readonly List<Layout> allLayouts = new List<Layout>();
        private StackTraceUsage stackTraceUsage;

        /// <summary>
        /// Gets a value indicating whether the target has been initialized by calling <see cref="Initialize" />.
        /// </summary>
        public bool IsInitialized { get; private set; }

        /// <summary>
        /// Gets or sets the name of the target.
        /// </summary>
        [RequiredParameter]
        public string Name { get; set; }

        /// <summary>
        /// Returns the text representation of the object. Used for diagnostics.
        /// </summary>
        /// <returns>A string that describes the target.</returns>
        public override string ToString()
        {
            return (this.Name ?? "unnamed") + ":" + this.GetType().Name;
        }

        /// <summary>
        /// Flush any pending log messages (in case of asynchronous targets).
        /// </summary>
        public void Flush()
        {
            this.Flush(TimeSpan.MaxValue);
        }

        /// <summary>
        /// Flush any pending log messages (in case of asynchronous targets).
        /// </summary>
        /// <param name="timeout">Maximum time to allow for the flush. Any messages after that time will be discarded.</param>
        public virtual void Flush(TimeSpan timeout)
        {
            // do nothing
        }

        /// <summary>
        /// Flush any pending log messages (in case of asynchronous targets).
        /// </summary>
        /// <param name="timeoutMilliseconds">Maximum time to allow for the flush. Any messages after that time will be discarded.</param>
        public void Flush(int timeoutMilliseconds)
        {
            this.Flush(TimeSpan.FromMilliseconds(timeoutMilliseconds));
        }

        /// <summary>
        /// Calls the <see cref="Layout.Precalculate"/> on each volatile layout
        /// used by this target.
        /// </summary>
        /// <param name="logEvent">The log event.</param>
        /// <remarks>
        /// A layout is volatile if it contains at least one <see cref="Layout"/> for 
        /// which <see cref="Layout.IsVolatile"/> returns true.
        /// </remarks>
        public void PrecalculateVolatileLayouts(LogEventInfo logEvent)
        {
            foreach (Layout l in this.allLayouts)
            {
                if (l.IsVolatile())
                {
                    l.Precalculate(logEvent);
                }
            }
        }

        /// <summary>
        /// Adds all layouts used by this target to the specified collection.
        /// </summary>
        /// <param name="layouts">The collection to add layouts to.</param>
        public virtual void PopulateLayouts(ICollection<Layout> layouts)
        {
        }

        /// <summary>
        /// Initializes the target. Can be used by inheriting classes
        /// to initialize logging.
        /// </summary>
        public virtual void Initialize()
        {
            this.PopulateLayouts(this.allLayouts);

            foreach (Layout l in this.allLayouts)
            {
                l.Initialize();

                StackTraceUsage stu = l.GetStackTraceUsage();
                if (stu > this.stackTraceUsage)
                {
                    this.stackTraceUsage = stu;
                }
            }

            this.IsInitialized = true;
        }

        /// <summary>
        /// Writes logging event to the log target. Must be overridden in inheriting
        /// classes.
        /// </summary>
        /// <param name="logEvent">Logging event to be written out.</param>
        protected internal abstract void Write(LogEventInfo logEvent);

        /// <summary>
        /// Writes an array of logging events to the log target. By default it iterates on all
        /// events and passes them to "Append" method. Inheriting classes can use this method to
        /// optimize batch writes.
        /// </summary>
        /// <param name="logEvents">Logging events to be written out.</param>
        protected internal virtual void Write(LogEventInfo[] logEvents)
        {
            for (int i = 0; i < logEvents.Length; ++i)
            {
                this.Write(logEvents[i]);
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether stack trace information should be gathered during log event processing.
        /// </summary>
        /// <returns>
        /// A <see cref="StackTraceUsage"/> value which determines stack trace information to be gathered.
        /// </returns>
        protected internal virtual StackTraceUsage GetStackTraceUsage()
        {
            return this.stackTraceUsage;
        }

        /// <summary>
        /// Closes the target and releases any unmanaged resources.
        /// </summary>
        protected internal virtual void Close()
        {
            if (!this.IsInitialized)
            {
                InternalLogger.Warn("Called Close() without Initialize() on {0}({1})", this, this.GetHashCode());
            }
            else
            {
                InternalLogger.Trace("Closing {0}({1})...", this, this.GetHashCode());
            }

            foreach (Layout l in this.allLayouts)
            {
                l.Close();
            }

            this.IsInitialized = false;
        }

        #region IDisposable Members

        void IDisposable.Dispose()
        {
            if (this.IsInitialized)
            {
                this.Close();
            }

            GC.SuppressFinalize(true);
        }

        #endregion
    }
}
