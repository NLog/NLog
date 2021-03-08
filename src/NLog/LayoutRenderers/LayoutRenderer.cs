// 
// Copyright (c) 2004-2020 Jaroslaw Kowalski <jaak@jkowalski.net>, Kim Christensen, Julian Verdurmen
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

namespace NLog.LayoutRenderers
{
    using System;
    using System.Globalization;
    using System.Text;
    using NLog.Common;
    using NLog.Config;
    using NLog.Internal;

    /// <summary>
    /// Render environmental information related to logging events.
    /// </summary>
    [NLogConfigurationItem]
    public abstract class LayoutRenderer : ISupportsInitialize, IRenderable
    {
        private const int MaxInitialRenderBufferLength = 16384;
        private int _maxRenderedLength;
        private bool _isInitialized;
        private IValueFormatter _valueFormatter;

        /// <summary>
        /// Gets the logging configuration this target is part of.
        /// </summary>
        protected LoggingConfiguration LoggingConfiguration { get; private set; }

        /// <summary>
        /// Value formatter
        /// </summary>
        protected IValueFormatter ValueFormatter => _valueFormatter ?? (_valueFormatter = ResolveService<IValueFormatter>());

        /// <summary>
        /// Returns a <see cref="System.String"/> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String"/> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            var lra = GetType().GetFirstCustomAttribute<LayoutRendererAttribute>();
            if (lra != null)
            {
                return $"Layout Renderer: ${{{lra.Name}}}";
            }

            return GetType().Name;
        }

        /// <summary>
        /// Renders the the value of layout renderer in the context of the specified log event.
        /// </summary>
        /// <param name="logEvent">The log event.</param>
        /// <returns>String representation of a layout renderer.</returns>
        public string Render(LogEventInfo logEvent)
        {
            int initialLength = _maxRenderedLength;
            if (initialLength > MaxInitialRenderBufferLength)
            {
                initialLength = MaxInitialRenderBufferLength;
            }

            var builder = new StringBuilder(initialLength);
            RenderAppendBuilder(logEvent, builder);
            if (builder.Length > _maxRenderedLength)
            {
                _maxRenderedLength = builder.Length;
            }

            return builder.ToString();
        }

        /// <summary>
        /// Initializes this instance.
        /// </summary>
        /// <param name="configuration">The configuration.</param>
        void ISupportsInitialize.Initialize(LoggingConfiguration configuration)
        {
            Initialize(configuration);
        }

        /// <summary>
        /// Closes this instance.
        /// </summary>
        void ISupportsInitialize.Close()
        {
            Close();
        }

        /// <summary>
        /// Initializes this instance.
        /// </summary>
        /// <param name="configuration">The configuration.</param>
        internal void Initialize(LoggingConfiguration configuration)
        {
            if (LoggingConfiguration == null)
                LoggingConfiguration = configuration;

            if (!_isInitialized)
            {
                _isInitialized = true;
                Initialize();
            }
        }

        private void Initialize()
        {
            try
            {
                PropertyHelper.CheckRequiredParameters(this);
                InitializeLayoutRenderer();
            }
            catch (Exception ex)
            {
                InternalLogger.Error(ex, "Exception in layout renderer initialization.");
                if (ex.MustBeRethrown())
                {
                    throw;
                }
            }
        }

        /// <summary>
        /// Closes this instance.
        /// </summary>
        internal void Close()
        {
            if (_isInitialized)
            {
                LoggingConfiguration = null;
                _valueFormatter = null;
                _isInitialized = false;
                CloseLayoutRenderer();
            }
        }

        /// <summary>
        /// Renders the value of layout renderer in the context of the specified log event.
        /// </summary>
        /// <param name="logEvent">The log event.</param>
        /// <param name="builder">The layout render output is appended to builder</param>
        internal void RenderAppendBuilder(LogEventInfo logEvent, StringBuilder builder)
        {
            if (!_isInitialized)
            {
                _isInitialized = true;
                Initialize();
            }

            try
            {
                Append(builder, logEvent);
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
        /// Renders the value of layout renderer in the context of the specified log event into <see cref="StringBuilder" />.
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
        /// Get the <see cref="IFormatProvider"/> for rendering the messages to a <see cref="string"/>
        /// </summary>
        /// <param name="logEvent">LogEvent with culture</param>
        /// <param name="layoutCulture">Culture in on Layout level</param>
        /// <returns></returns>
        protected IFormatProvider GetFormatProvider(LogEventInfo logEvent, IFormatProvider layoutCulture = null)
        {
            return logEvent.FormatProvider ?? layoutCulture ?? LoggingConfiguration?.DefaultCultureInfo;
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
            return logEvent.FormatProvider as CultureInfo ?? layoutCulture ?? LoggingConfiguration?.DefaultCultureInfo;
        }

        /// <summary>
        /// Register a custom layout renderer.
        /// </summary>
        /// <remarks>Short-cut for registering to default <see cref="ConfigurationItemFactory"/></remarks>
        /// <typeparam name="T"> Type of the layout renderer.</typeparam>
        /// <param name="name"> Name of the layout renderer - without ${}.</param>
        public static void Register<T>(string name)
            where T: LayoutRenderer
        {
            var layoutRendererType = typeof(T);
            Register(name, layoutRendererType);
        }

        /// <summary>
        /// Register a custom layout renderer.
        /// </summary>
        /// <remarks>Short-cut for registering to default <see cref="ConfigurationItemFactory"/></remarks>
        /// <param name="layoutRendererType"> Type of the layout renderer.</param>
        /// <param name="name"> Name of the layout renderer - without ${}.</param>
        public static void Register(string name, Type layoutRendererType)
        {
            ConfigurationItemFactory.Default.LayoutRenderers
                .RegisterDefinition(name, layoutRendererType);
        }

        /// <summary>
        /// Register a custom layout renderer with a callback function <paramref name="func"/>. The callback receives the logEvent.
        /// </summary>
        /// <param name="name">Name of the layout renderer - without ${}.</param>
        /// <param name="func">Callback that returns the value for the layout renderer.</param>
        public static void Register(string name, Func<LogEventInfo, object> func)
        {
            Register(name, (info, configuration) => func(info));
        }

        /// <summary>
        /// Register a custom layout renderer with a callback function <paramref name="func"/>. The callback receives the logEvent and the current configuration.
        /// </summary>
        /// <param name="name">Name of the layout renderer - without ${}.</param>
        /// <param name="func">Callback that returns the value for the layout renderer.</param>
        public static void Register(string name, Func<LogEventInfo, LoggingConfiguration, object> func)
        {
            var layoutRenderer = new FuncLayoutRenderer(name, func);
            
            ConfigurationItemFactory.Default.GetLayoutRenderers().RegisterFuncLayout(name, layoutRenderer);
        }

        /// <summary>
        /// Resolves the interface service-type from the service-repository
        /// </summary>
        protected T ResolveService<T>() where T : class
        {
            return LoggingConfiguration.GetServiceProvider().ResolveService<T>(_isInitialized);
        }
    }
}