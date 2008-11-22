// 
// Copyright (c) 2004-2006 Jaroslaw Kowalski <jaak@jkowalski.net>
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
using System.Text;
using System.Collections;

using NLog.Config;
using System.Collections.Generic;
using NLog.Layouts;
using NLog.Internal;

namespace NLog.Targets
{
    /// <summary>
    /// Represents logging target.
    /// </summary>
    public abstract class Target
    {
        private bool _isInitialized = false;

        /// <summary>
        /// Determines whether the target has been initialized by calling <see cref="Initialize" />.
        /// </summary>
        public bool IsInitialized
        {
            get { return _isInitialized; }
        }

        /// <summary>
        /// Creates a new instance of the logging target and initializes
        /// default layout.
        /// </summary>
        /// <remarks>
        /// The default value of the layout is: <code>${longdate}|${level:uppercase=true}|${logger}|${message}</code>
        /// </remarks>
        protected Target()
        {
        }

        private List<Layout> _allLayouts = new List<Layout>();
        private StackTraceUsage _stackTraceUsage;
        private string _name;

        /// <summary>
        /// The name of the target.
        /// </summary>
        [RequiredParameter]
        public string Name
        {
            get { return _name; }
            set { _name = value; }
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
                Write(logEvents[i]);
            }
        }

        /// <summary>
        /// Determines whether stack trace information should be gathered
        /// during log event processing. By default it calls <see cref="Layout.GetStackTraceUsage" /> on
        /// the result of <see cref="GetLayouts()"/>.
        /// </summary>
        /// <returns>0 - don't include stack trace<br/>1 - include stack trace without source file information<br/>2 - include full stack trace</returns>
        protected internal virtual StackTraceUsage GetStackTraceUsage()
        {
            return _stackTraceUsage;
        }

        /// <summary>
        /// Returns the text representation of the object. Used for diagnostics.
        /// </summary>
        /// <returns>A string that describes the target.</returns>
        public override string ToString()
        {
            return ((this.Name != null) ? this.Name : "unnamed") + ":" + this.GetType().Name;
        }

        /// <summary>
        /// Flush any pending log messages (in case of asynchronous targets).
        /// </summary>
        public void Flush()
        {
            Flush(TimeSpan.MaxValue);
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
            Flush(TimeSpan.FromMilliseconds(timeoutMilliseconds));
        }

        /// <summary>
        /// Closes the target and releases any unmanaged resources.
        /// </summary>
        protected internal virtual void Close()
        {
            if (!_isInitialized)
                InternalLogger.Warn("Called Close() without Initialize() on " + this.ToString() + "(" + this.GetHashCode() + ")");
            else
                InternalLogger.Trace("Closing " + this.ToString() + "(" + this.GetHashCode() + ")...");
            foreach (Layout l in _allLayouts)
            {
                l.Close();
            }
            _isInitialized = false;
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
            foreach (Layout l in _allLayouts)
            {
                if (l.IsVolatile())
                    l.Precalculate(logEvent);
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
            PopulateLayouts(_allLayouts);

            foreach (Layout l in _allLayouts)
            {
                l.Initialize();

                StackTraceUsage stu = l.GetStackTraceUsage();
                if (stu > _stackTraceUsage)
                    _stackTraceUsage = stu;
            }
            _isInitialized = true;
        }
    }
}
