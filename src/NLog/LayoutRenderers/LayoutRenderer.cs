// 
// Copyright (c) 2004-2021 Jaroslaw Kowalski <jaak@jkowalski.net>, Kim Christensen, Julian Verdurmen
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
    using System.ComponentModel;
    using System.Diagnostics.CodeAnalysis;
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

        /// <inheritdoc/>
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
        /// Renders the value of layout renderer in the context of the specified log event.
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

        /// <inheritdoc/>
        void ISupportsInitialize.Initialize(LoggingConfiguration configuration)
        {
            Initialize(configuration);
        }

        /// <inheritdoc/>
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
            if (LoggingConfiguration is null)
                LoggingConfiguration = configuration;

            if (!_isInitialized)
            {
                Initialize();
            }
        }

        private void Initialize()
        {
            try
            {
                PropertyHelper.CheckRequiredParameters(ConfigurationItemFactory.Default, this);
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
            finally
            {
                _isInitialized = true;  // Only one attempt, must Close to retry
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
        /// Get the <see cref="CultureInfo"/> for rendering the messages to a <see cref="string"/>
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
        /// Obsolete and replaced by <see cref="LogManager.Setup()"/> with NLog v5.2.
        /// 
        /// Register a custom layout renderer.
        /// </summary>
        /// <remarks>Short-cut for registering to default <see cref="ConfigurationItemFactory"/></remarks>
        /// <typeparam name="T">Type of the layout renderer.</typeparam>
        /// <param name="name">The layout-renderer type-alias for use in NLog configuration - without '${ }'</param>
        [Obsolete("Instead use LogManager.Setup().SetupExtensions(). Marked obsolete with NLog v5.2")]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public static void Register<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor | DynamicallyAccessedMemberTypes.PublicProperties)] T>(string name)
            where T : LayoutRenderer
        {
            var layoutRendererType = typeof(T);
            Register(name, layoutRendererType);
        }

        /// <summary>
        /// Obsolete and replaced by <see cref="LogManager.Setup()"/> with NLog v5.2.
        /// 
        /// Register a custom layout renderer.
        /// </summary>
        /// <remarks>Short-cut for registering to default <see cref="ConfigurationItemFactory"/></remarks>
        /// <param name="layoutRendererType"> Type of the layout renderer.</param>
        /// <param name="name">The layout-renderer type-alias for use in NLog configuration - without '${ }'</param>
        [Obsolete("Instead use LogManager.Setup().SetupExtensions(). Marked obsolete with NLog v5.2")]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public static void Register(string name, [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor | DynamicallyAccessedMemberTypes.PublicProperties)] Type layoutRendererType)
        {
            Guard.ThrowIfNull(layoutRendererType);
            Guard.ThrowIfNullOrEmpty(name);
            ConfigurationItemFactory.Default.GetLayoutRendererFactory().RegisterDefinition(name, layoutRendererType);
        }

        /// <summary>
        /// Obsolete and replaced by <see cref="LogManager.Setup()"/> with NLog v5.2.
        /// 
        /// Register a custom layout renderer with a callback function <paramref name="func"/>. The callback receives the logEvent.
        /// </summary>
        /// <param name="name">The layout-renderer type-alias for use in NLog configuration - without '${ }'</param>
        /// <param name="func">Callback that returns the value for the layout renderer.</param>
        [Obsolete("Instead use LogManager.Setup().SetupExtensions(). Marked obsolete with NLog v5.2")]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public static void Register(string name, Func<LogEventInfo, object> func)
        {
            Guard.ThrowIfNull(func);
            Register(name, (info, configuration) => func(info));
        }

        /// <summary>
        /// Obsolete and replaced by <see cref="LogManager.Setup()"/> with NLog v5.2.
        /// 
        /// Register a custom layout renderer with a callback function <paramref name="func"/>. The callback receives the logEvent and the current configuration.
        /// </summary>
        /// <param name="name">The layout-renderer type-alias for use in NLog configuration - without '${ }'</param>
        /// <param name="func">Callback that returns the value for the layout renderer.</param>
        [Obsolete("Instead use LogManager.Setup().SetupExtensions(). Marked obsolete with NLog v5.2")]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public static void Register(string name, Func<LogEventInfo, LoggingConfiguration, object> func)
        {
            Guard.ThrowIfNull(func);
            var layoutRenderer = new FuncLayoutRenderer(name, func);
            Register(layoutRenderer);
        }

        /// <summary>
        /// Obsolete and replaced by <see cref="LogManager.Setup()"/> with NLog v5.2.
        /// 
        /// Register a custom layout renderer with a callback function <paramref name="layoutRenderer"/>. The callback receives the logEvent and the current configuration.
        /// </summary>
        /// <param name="layoutRenderer">Renderer with callback func</param>
        [Obsolete("Instead use LogManager.Setup().SetupExtensions(). Marked obsolete with NLog v5.2")]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public static void Register(FuncLayoutRenderer layoutRenderer)
        {
            Guard.ThrowIfNull(layoutRenderer);
            ConfigurationItemFactory.Default.GetLayoutRendererFactory().RegisterFuncLayout(layoutRenderer.LayoutRendererName, layoutRenderer);
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