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

using System.Globalization;

namespace NLog.LayoutRenderers
{
    using System;
    using System.Text;
    using NLog.Common;
    using NLog.Config;
    using NLog.Internal;

    /// <summary>
    /// Render environmental information related to logging events.
    /// </summary>
    [NLogConfigurationItem]
    public abstract class LayoutRenderer : ISupportsInitialize, IRenderable, IDisposable
    {
        private const int MaxInitialRenderBufferLength = 16384;
        private int maxRenderedLength;
        private bool isInitialized;

        /// <summary>
        /// Gets the logging configuration this target is part of.
        /// </summary>
        protected LoggingConfiguration LoggingConfiguration { get; private set; }

        /// <summary>
        /// Returns a <see cref="System.String"/> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String"/> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            var lra = (LayoutRendererAttribute)Attribute.GetCustomAttribute(this.GetType(), typeof(LayoutRendererAttribute));
            if (lra != null)
            {
                return "Layout Renderer: ${" + lra.Name + "}";
            }

            return this.GetType().Name;
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Renders the the value of layout renderer in the context of the specified log event.
        /// </summary>
        /// <param name="logEvent">The log event.</param>
        /// <returns>String representation of a layout renderer.</returns>
        public string Render(LogEventInfo logEvent)
        {
            int initialLength = this.maxRenderedLength;
            if (initialLength > MaxInitialRenderBufferLength)
            {
                initialLength = MaxInitialRenderBufferLength;
            }

            var builder = new StringBuilder(initialLength);

            this.Render(builder, logEvent);
            if (builder.Length > this.maxRenderedLength)
            {
                this.maxRenderedLength = builder.Length;
            }

            return builder.ToString();
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
            if (this.LoggingConfiguration == null)
                this.LoggingConfiguration = configuration;

            if (!this.isInitialized)
            {
                this.isInitialized = true;
                this.InitializeLayoutRenderer();
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
                this.CloseLayoutRenderer();
            }
        }

        internal void Render(StringBuilder builder, LogEventInfo logEvent)
        {
            if (!this.isInitialized)
            {
                this.isInitialized = true;
                this.InitializeLayoutRenderer();
            }

            try
            {
                this.Append(builder, logEvent);
            }
            catch (Exception exception)
            {
                InternalLogger.Warn(exception, "Exception in layout renderer.");

                if (exception.MustBeRethrown())
                {
                    throw;
                }
              
            }
        }

        /// <summary>
        /// Renders the specified environmental information and appends it to the specified <see cref="StringBuilder" />.
        /// </summary>
        /// <param name="builder">The <see cref="StringBuilder"/> to append the rendered data to.</param>
        /// <param name="logEvent">Logging event.</param>
        protected abstract void Append(StringBuilder builder, LogEventInfo logEvent);

        /// <summary>
        /// Initializes the layout renderer.
        /// </summary>
        protected virtual void InitializeLayoutRenderer()
        {
        }

        /// <summary>
        /// Closes the layout renderer.
        /// </summary>      
        protected virtual void CloseLayoutRenderer()
        {
        }

        /// <summary>
        /// Releases unmanaged and - optionally - managed resources.
        /// </summary>
        /// <param name="disposing">True to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                this.Close();
            }
        }

        /// <summary>
        /// Get the <see cref="IFormatProvider"/> for rendering the messages to a <see cref="string"/>
        /// </summary>
        /// <param name="logEvent">LogEvent with culture</param>
        /// <param name="layoutCulture">Culture in on Layout level</param>
        /// <returns></returns>
        protected IFormatProvider GetFormatProvider(LogEventInfo logEvent, IFormatProvider layoutCulture = null)
        {
            var culture = logEvent.FormatProvider;

            if (culture == null)
            {
                culture = layoutCulture;
            }

            if (culture == null && this.LoggingConfiguration != null)
            {
                culture = this.LoggingConfiguration.DefaultCultureInfo;
            }
            return culture;
        }

        /// <summary>
        /// Get the <see cref="CultureInfo"/> for rendering the messages to a <see cref="string"/>, needed for date and number formats
        /// </summary>
        /// <param name="logEvent">LogEvent with culture</param>
        /// <param name="layoutCulture">Culture in on Layout level</param>
        /// <returns></returns>
        /// <remarks>
        /// <see cref="GetFormatProvider"/> is preferred
        /// </remarks>
        protected CultureInfo GetCulture(LogEventInfo logEvent, CultureInfo layoutCulture = null)
        {
            var culture = logEvent.FormatProvider as CultureInfo;

            if (culture == null)
            {
                culture = layoutCulture;
            }

            if (culture == null && this.LoggingConfiguration != null)
            {
                culture =  this.LoggingConfiguration.DefaultCultureInfo;
            }
            return culture;
        }
    }
}