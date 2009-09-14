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

using System.Collections.Generic;
using NLog.Common;
using NLog.Config;

namespace NLog.Layouts
{
    /// <summary>
    /// Abstract interface that layouts must implement.
    /// </summary>
    public abstract class Layout
    {
        protected Layout()
        {
            IsInitialized = false;
        }

        /// <summary>
        /// Gets a value indicating whether this instance is initialized.
        /// </summary>
        /// <value>
        /// A value of <c>true</c> if this instance is initialized; otherwise, <c>false</c>.
        /// </value>
        public bool IsInitialized { get; private set; }

        /// <summary>
        /// Converts a given text to a <see cref="Layout" />.
        /// </summary>
        /// <param name="text">Text to be converted.</param>
        /// <returns><see cref="SimpleLayout"/> object represented by the text.</returns>
        public static implicit operator Layout(string text)
        {
            return new SimpleLayout(text);
        }

        public static Layout FromString(string text)
        {
            return new SimpleLayout(text);
        }

        /// <summary>
        /// Renders the layout for the specified logging event by invoking layout renderers.
        /// </summary>
        /// <param name="logEvent">The logging event.</param>
        /// <returns>The rendered layout.</returns>
        public abstract string GetFormattedMessage(LogEventInfo logEvent);

        /// <summary>
        /// Gets or sets a value indicating whether stack trace information should be gathered during log event processing. 
        /// </summary>
        /// <returns>A <see cref="StackTraceUsage" /> value that determines stack trace handling.</returns>
        public abstract StackTraceUsage GetStackTraceUsage();

        /// <summary>
        /// Returns the value indicating whether this layout includes any volatile 
        /// layout renderers.
        /// </summary>
        /// <returns>A value of <see langword="true" /> when the layout includes at least 
        /// one volatile renderer, <see langword="false"/> otherwise.</returns>
        /// <remarks>
        /// Volatile layout renderers are dependent on information not contained 
        /// in <see cref="LogEventInfo"/> (such as thread-specific data, MDC data, NDC data).
        /// </remarks>
        public abstract bool IsVolatile();

        /// <summary>
        /// Precalculates the layout for the specified log event and stores the result
        /// in per-log event cache.
        /// </summary>
        /// <param name="logEvent">The log event.</param>
        /// <remarks>
        /// Calling this method enables you to store the log event in a buffer
        /// and/or potentially evaluate it in another thread even though the 
        /// layout may contain thread-dependent renderer.
        /// </remarks>
        public virtual void Precalculate(LogEventInfo logEvent)
        {
            this.GetFormattedMessage(logEvent);
        }

        /// <summary>
        /// Initializes the layout.
        /// </summary>
        public virtual void Initialize()
        {
            this.IsInitialized = true;
        }

        /// <summary>
        /// Closes the layout.
        /// </summary>
        public virtual void Close()
        {
            if (!this.IsInitialized)
            {
                InternalLogger.Warn("Called Close() without Initialize() on " + this.ToString() + "(" + this.GetHashCode() + ")");
            }
            else
            {
                InternalLogger.Trace("Closing " + this.ToString() + "(" + this.GetHashCode() + ")...");
            }

            this.IsInitialized = false;
        }

        /// <summary>
        /// Add this layout and all sub-layouts to the specified collection..
        /// </summary>
        /// <param name="layouts">The collection of layouts.</param>
        public virtual void PopulateLayouts(ICollection<Layout> layouts)
        {
            layouts.Add(this);
        }

        /// <summary>
        /// Gets or sets a value indicating whether the value of layout is fixed for current AppDomain.
        /// </summary>
        /// <returns>
        /// A value of <c>true</c> if value of layout is fixed for current AppDomain, otherwise <c>false</c>.
        /// </returns>
        public virtual bool IsAppDomainFixed()
        {
            return false;
        }
    }
}
