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

namespace NLog.Layouts
{
    using System.ComponentModel;
    using NLog.Config;
    using NLog.Internal;

    /// <summary>
    /// Abstract interface that layouts must implement.
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1724:TypeNamesShouldNotMatchNamespaces", Justification = "Few people will see this conflict.")]
    [NLogConfigurationItem]
    public abstract class Layout : ISupportsInitialize, IRenderable
    {
        /// <summary>
        /// Is this layout initialized? See <see cref="Initialize(NLog.Config.LoggingConfiguration)"/>
        /// </summary>
        private bool isInitialized;

        /// <summary>
        /// Does the layout contains threadAgnostic layout renders? If contains non-threadAgnostic-layoutrendender, then this layout is also not threadAgnostic. 
        /// See <see cref="IsThreadAgnostic"/> and <see cref="Initialize"/>.
        /// </summary>
        private bool threadAgnostic;

        /// <summary>
        /// Gets a value indicating whether this layout is thread-agnostic (can be rendered on any thread).
        /// </summary>
        /// <remarks>
        /// Layout is thread-agnostic if it has been marked with [ThreadAgnostic] attribute and all its children are
        /// like that as well.
        /// Thread-agnostic layouts only use contents of <see cref="LogEventInfo"/> for its output.
        /// </remarks>
		internal bool IsThreadAgnostic
		{
            get { return this.threadAgnostic; }
        }

        /// <summary>
        /// Gets the logging configuration this target is part of.
        /// </summary>
        protected LoggingConfiguration LoggingConfiguration { get; private set; }

        /// <summary>
        /// Converts a given text to a <see cref="Layout" />.
        /// </summary>
        /// <param name="text">Text to be converted.</param>
        /// <returns><see cref="SimpleLayout"/> object represented by the text.</returns>
        public static implicit operator Layout([Localizable(false)] string text)
        {
            return FromString(text);
        }

        /// <summary>
        /// Implicitly converts the specified string to a <see cref="SimpleLayout"/>.
        /// </summary>
        /// <param name="layoutText">The layout string.</param>
        /// <returns>Instance of <see cref="SimpleLayout"/>.</returns>
        public static Layout FromString(string layoutText)
        {
            return FromString(layoutText, ConfigurationItemFactory.Default);
        }

        /// <summary>
        /// Implicitly converts the specified string to a <see cref="SimpleLayout"/>.
        /// </summary>
        /// <param name="layoutText">The layout string.</param>
        /// <param name="configurationItemFactory">The NLog factories to use when resolving layout renderers.</param>
        /// <returns>Instance of <see cref="SimpleLayout"/>.</returns>
        public static Layout FromString(string layoutText, ConfigurationItemFactory configurationItemFactory)
        {
            return new SimpleLayout(layoutText, configurationItemFactory);
        }

        /// <summary>
        /// Precalculates the layout for the specified log event and stores the result
        /// in per-log event cache.
        /// 
        /// Only if the layout doesn't have [ThreadAgnostic] and doens't contain layouts with [ThreadAgnostic]. 
        /// </summary>
        /// <param name="logEvent">The log event.</param>
        /// <remarks>
        /// Calling this method enables you to store the log event in a buffer
        /// and/or potentially evaluate it in another thread even though the 
        /// layout may contain thread-dependent renderer.
        /// </remarks>
        public virtual void Precalculate(LogEventInfo logEvent)
        {
            if (!this.threadAgnostic)
            {
                this.Render(logEvent);
            }
        }

        /// <summary>
        /// Renders the event info in layout.
        /// </summary>
        /// <param name="logEvent">The event info.</param>
        /// <returns>String representing log event.</returns>
        public string Render(LogEventInfo logEvent)
        {
            if (!this.isInitialized)
            {
                this.isInitialized = true;
                this.InitializeLayout();
            }

            return this.GetFormattedMessage(logEvent);
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
        /// Initializes this instance.
        /// </summary>
        /// <param name="configuration">The configuration.</param>
		internal void Initialize(LoggingConfiguration configuration)
		{
            if (!this.isInitialized)
            {
                this.LoggingConfiguration = configuration;
                this.isInitialized = true;

                // determine whether the layout is thread-agnostic
                // layout is thread agnostic if it is thread-agnostic and 
                // all its nested objects are thread-agnostic.
                this.threadAgnostic = true;
                foreach (object item in ObjectGraphScanner.FindReachableObjects<object>(this))
                {
                    if (!item.GetType().IsDefined(typeof(ThreadAgnosticAttribute), true))
                    {
                        this.threadAgnostic = false;
                        break;
                    }
                }

                this.InitializeLayout();
            }
        }

        /// <summary>
        /// Closes this instance.
        /// </summary>
		internal void Close()
		{
            if (this.isInitialized)
            {
                this.LoggingConfiguration = null;
                this.isInitialized = false;
                this.CloseLayout();
            }
        }

        /// <summary>
        /// Initializes the layout.
        /// </summary>
        protected virtual void InitializeLayout()
        {
        }

        /// <summary>
        /// Closes the layout.
        /// </summary>
        protected virtual void CloseLayout()
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
