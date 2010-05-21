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

namespace NLog.Layouts
{
    using NLog.Config;
    using NLog.Internal;

    /// <summary>
    /// Abstract interface that layouts must implement.
    /// </summary>
    public abstract class Layout : ISupportsInitialize, INLogConfigurationItem, IRenderable
    {
        private bool isInitialized;

        /// <summary>
        /// Converts a given text to a <see cref="Layout" />.
        /// </summary>
        /// <param name="text">Text to be converted.</param>
        /// <returns><see cref="SimpleLayout"/> object represented by the text.</returns>
        public static implicit operator Layout(string text)
        {
            return FromString(text);
        }

        /// <summary>
        /// Implicitly converts the specified string to a <see cref="SimpleLayout"/>.
        /// </summary>
        /// <param name="layoutString">The layout string.</param>
        /// <returns>Instance of <see cref="SimpleLayout"/>.</returns>
        public static Layout FromString(string layoutString)
        {
            return FromString(layoutString, NLogFactories.Default);
        }

        /// <summary>
        /// Implicitly converts the specified string to a <see cref="SimpleLayout"/>.
        /// </summary>
        /// <param name="layoutString">The layout string.</param>
        /// <param name="nlogFactories">The NLog factories to use when resolving layout renderers.</param>
        /// <returns>Instance of <see cref="SimpleLayout"/>.</returns>
        public static Layout FromString(string layoutString, NLogFactories nlogFactories)
        {
            return new SimpleLayout(layoutString, nlogFactories);
        }

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
            this.Render(logEvent);
        }

        /// <summary>
        /// Renders the event info in layout.
        /// </summary>
        /// <param name="eventInfo">The event info.</param>
        /// <returns>String representing log event.</returns>
        public string Render(LogEventInfo eventInfo)
        {
            if (!this.isInitialized)
            {
                this.isInitialized = true;
                this.Initialize();
            }

            return this.GetFormattedMessage(eventInfo);
        }

        /// <summary>
        /// Initializes this instance.
        /// </summary>
        void ISupportsInitialize.Initialize()
        {
            if (!this.isInitialized)
            {
                this.isInitialized = true;
                this.Initialize();
            }
        }

        /// <summary>
        /// Closes this instance.
        /// </summary>
        void ISupportsInitialize.Close()
        {
            if (this.isInitialized)
            {
                this.isInitialized = false;
                this.Close();
            }
        }

        /// <summary>
        /// Initializes the layout.
        /// </summary>
        protected virtual void Initialize()
        {
        }

        /// <summary>
        /// Closes the layout.
        /// </summary>
        protected virtual void Close()
        {
        }

        /// <summary>
        /// Renders the layout for the specified logging event by invoking layout renderers.
        /// </summary>
        /// <param name="logEvent">The logging event.</param>
        /// <returns>The rendered layout.</returns>
        protected abstract string GetFormattedMessage(LogEventInfo logEvent);
    }
}
