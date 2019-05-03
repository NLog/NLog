// 
// Copyright (c) 2004-2019 Jaroslaw Kowalski <jaak@jkowalski.net>, Kim Christensen, Julian Verdurmen
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
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Linq;
    using System.Text;
    using NLog.Config;
    using NLog.Internal;
    using NLog.Common;

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
        internal bool IsInitialized;
        private bool _scannedForObjects;

        /// <summary>
        /// Gets a value indicating whether this layout is thread-agnostic (can be rendered on any thread).
        /// </summary>
        /// <remarks>
        /// Layout is thread-agnostic if it has been marked with [ThreadAgnostic] attribute and all its children are
        /// like that as well.
        /// 
        /// Thread-agnostic layouts only use contents of <see cref="LogEventInfo"/> for its output.
        /// </remarks>
        internal bool ThreadAgnostic { get; set; }

        internal bool ThreadSafe { get; set; }

        internal bool MutableUnsafe { get; set; }

        /// <summary>
        /// Gets the level of stack trace information required for rendering.
        /// </summary>
        internal StackTraceUsage StackTraceUsage { get; private set; }

        private const int MaxInitialRenderBufferLength = 16384;
        private int _maxRenderedLength;

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
            if (!ThreadAgnostic || MutableUnsafe)
            {
                Render(logEvent);
            }
        }

        /// <summary>
        /// Renders the event info in layout.
        /// </summary>
        /// <param name="logEvent">The event info.</param>
        /// <returns>String representing log event.</returns>
        public string Render(LogEventInfo logEvent)
        {
            if (!IsInitialized)
            {
                Initialize(LoggingConfiguration);
            }

            if (!ThreadAgnostic || MutableUnsafe)
            {
                object cachedValue;
                if (logEvent.TryGetCachedLayoutValue(this, out cachedValue))
                {
                    return cachedValue?.ToString() ?? string.Empty;
                }
            }

            string layoutValue = GetFormattedMessage(logEvent) ?? string.Empty;
            if (!ThreadAgnostic || MutableUnsafe)
            {
                // Would be nice to only do this in Precalculate(), but we need to ensure internal cache
                // is updated for for custom Layouts that overrides Precalculate (without calling base.Precalculate)
                logEvent.AddCachedLayoutValue(this, layoutValue);
            }
            return layoutValue;
        }

        internal virtual void PrecalculateBuilder(LogEventInfo logEvent, StringBuilder target)
        {
            Precalculate(logEvent); // Allow custom Layouts to work with OptimizeBufferReuse
        }

        /// <summary>
        /// Optimized version of <see cref="Render(LogEventInfo)"/> for internal Layouts. Works best
        /// when override of <see cref="RenderFormattedMessage(LogEventInfo, StringBuilder)"/> is available.
        /// </summary>
        /// <param name="logEvent">The event info.</param>
        /// <param name="target">Appends the string representing log event to target</param>
        /// <param name="cacheLayoutResult">Should rendering result be cached on LogEventInfo</param>
        internal void RenderAppendBuilder(LogEventInfo logEvent, StringBuilder target, bool cacheLayoutResult = false)
        {
            if (!IsInitialized)
            {
                Initialize(LoggingConfiguration);
            }

            if (!ThreadAgnostic || MutableUnsafe)
            {
                object cachedValue;
                if (logEvent.TryGetCachedLayoutValue(this, out cachedValue))
                {
                    target.Append(cachedValue?.ToString() ?? string.Empty);
                    return;
                }
            }

            cacheLayoutResult = cacheLayoutResult && !ThreadAgnostic;
            using (var localTarget = new AppendBuilderCreator(target, cacheLayoutResult))
            {
                RenderFormattedMessage(logEvent, localTarget.Builder);
                if (cacheLayoutResult)
                {
                    // when needed as it generates garbage
                    logEvent.AddCachedLayoutValue(this, localTarget.Builder.ToString());
                }
            }
        }

        /// <summary>
        /// Valid default implementation of <see cref="GetFormattedMessage" />, when having implemented the optimized <see cref="RenderFormattedMessage"/>
        /// </summary>
        /// <param name="logEvent">The logging event.</param>
        /// <param name="reusableBuilder">StringBuilder to help minimize allocations [optional].</param>
        /// <returns>The rendered layout.</returns>
        internal string RenderAllocateBuilder(LogEventInfo logEvent, StringBuilder reusableBuilder = null)
        {
            int initialLength = _maxRenderedLength;
            if (initialLength > MaxInitialRenderBufferLength)
            {
                initialLength = MaxInitialRenderBufferLength;
            }

            var sb = reusableBuilder ?? new StringBuilder(initialLength);
            RenderFormattedMessage(logEvent, sb);
            if (sb.Length > _maxRenderedLength)
            {
                _maxRenderedLength = sb.Length;
            }

            return sb.ToString();
        }

        /// <summary>
        /// Renders the layout for the specified logging event by invoking layout renderers.
        /// </summary>
        /// <param name="logEvent">The logging event.</param>
        /// <param name="target"><see cref="StringBuilder"/> for the result</param>
        protected virtual void RenderFormattedMessage(LogEventInfo logEvent, StringBuilder target)
        {
            target.Append(GetFormattedMessage(logEvent) ?? string.Empty);
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
            if (!IsInitialized)
            {
                LoggingConfiguration = configuration;
                IsInitialized = true;
                _scannedForObjects = false;

                InitializeLayout();

                if (!_scannedForObjects)
                {
                    InternalLogger.Debug("Initialized Layout done but not scanned for objects");
                    PerformObjectScanning();
                }
            }
        }

        internal void PerformObjectScanning()
        {
            var objectGraphScannerList = ObjectGraphScanner.FindReachableObjects<object>(true, this);

            // determine whether the layout is thread-agnostic
            // layout is thread agnostic if it is thread-agnostic and 
            // all its nested objects are thread-agnostic.
            ThreadAgnostic = objectGraphScannerList.All(item => item.GetType().IsDefined(typeof(ThreadAgnosticAttribute), true));
            ThreadSafe = objectGraphScannerList.All(item => item.GetType().IsDefined(typeof(ThreadSafeAttribute), true));
            MutableUnsafe = objectGraphScannerList.Any(item => item.GetType().IsDefined(typeof(MutableUnsafeAttribute), true));

            // determine the max StackTraceUsage, to decide if Logger needs to capture callsite
            StackTraceUsage = StackTraceUsage.None;    // Incase this Layout should implement IUsesStackTrace
            StackTraceUsage = objectGraphScannerList.OfType<IUsesStackTrace>().DefaultIfEmpty().Max(item => item?.StackTraceUsage ?? StackTraceUsage.None);

            _scannedForObjects = true;
        }

        /// <summary>
        /// Closes this instance.
        /// </summary>
        internal void Close()
        {
            if (IsInitialized)
            {
                LoggingConfiguration = null;
                IsInitialized = false;
                CloseLayout();
            }
        }

        /// <summary>
        /// Initializes the layout.
        /// </summary>
        protected virtual void InitializeLayout()
        {
            PerformObjectScanning();
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

        /// <summary>
        /// Register a custom Layout.
        /// </summary>
        /// <remarks>Short-cut for registing to default <see cref="ConfigurationItemFactory"/></remarks>
        /// <typeparam name="T"> Type of the Layout.</typeparam>
        /// <param name="name"> Name of the Layout.</param>
        public static void Register<T>(string name)
            where T : Layout
        {
            var layoutRendererType = typeof(T);
            Register(name, layoutRendererType);
        }

        /// <summary>
        /// Register a custom Layout.
        /// </summary>
        /// <remarks>Short-cut for registing to default <see cref="ConfigurationItemFactory"/></remarks>
        /// <param name="layoutType"> Type of the Layout.</param>
        /// <param name="name"> Name of the Layout.</param>
        public static void Register(string name, Type layoutType)
        {
            ConfigurationItemFactory.Default.Layouts
                .RegisterDefinition(name, layoutType);
        }

        /// <summary>
        /// Optimized version of <see cref="Precalculate(LogEventInfo)"/> for internal Layouts, when
        /// override of <see cref="RenderFormattedMessage(LogEventInfo, StringBuilder)"/> is available.
        /// </summary>
        internal void PrecalculateBuilderInternal(LogEventInfo logEvent, StringBuilder target)
        {
            if (!ThreadAgnostic || MutableUnsafe)
            {
                RenderAppendBuilder(logEvent, target, true);
            }
        }

        internal string ToStringWithNestedItems<T>(IList<T> nestedItems, Func<T, string> nextItemToString)
        {
            if (nestedItems?.Count > 0)
            {
                var nestedNames = nestedItems.Select(nextItemToString).ToArray();
                return string.Concat(GetType().Name, "=", string.Join("|", nestedNames));
            }
            return base.ToString();
        }

        /// <summary>
        /// Try get value
        /// </summary>
        /// <param name="logEvent"></param>
        /// <param name="rawValue">rawValue if return result is true</param>
        /// <returns>false if we could not determine the rawValue</returns>
        internal virtual bool TryGetRawValue(LogEventInfo logEvent, out object rawValue)
        {
            rawValue = null;
            return false;
        }
    }
}
