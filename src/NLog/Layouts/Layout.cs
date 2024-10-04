//
// Copyright (c) 2004-2024 Jaroslaw Kowalski <jaak@jkowalski.net>, Kim Christensen, Julian Verdurmen
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
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using System.Text;
    using JetBrains.Annotations;
    using NLog.Common;
    using NLog.Config;
    using NLog.Internal;
    using NLog.Targets;

    /// <summary>
    /// Abstract interface that layouts must implement.
    /// </summary>
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

        internal bool ThreadAgnosticImmutable { get; set; }

        /// <summary>
        /// Gets the level of stack trace information required for rendering.
        /// </summary>
        internal StackTraceUsage StackTraceUsage { get; set; }

        /// <summary>
        /// Gets the logging configuration this target is part of.
        /// </summary>
        [CanBeNull]
        protected internal LoggingConfiguration LoggingConfiguration { get; private set; }

        /// <summary>
        /// Converts a given text to a <see cref="Layout" />.
        /// </summary>
        /// <param name="text">Text to be converted.</param>
        /// <returns><see cref="SimpleLayout"/> object represented by the text.</returns>
        public static implicit operator Layout([Localizable(false)] string text)
        {
            return FromString(text, ConfigurationItemFactory.Default);
        }

        /// <summary>
        /// Implicitly converts the specified string to a <see cref="SimpleLayout"/>.
        /// </summary>
        /// <param name="layoutText">The layout string.</param>
        /// <returns>Instance of <see cref="SimpleLayout"/>.</returns>'
        public static Layout FromString([Localizable(false)] string layoutText)
        {
            return FromString(layoutText, ConfigurationItemFactory.Default);
        }

        /// <summary>
        /// Implicitly converts the specified string to a <see cref="SimpleLayout"/>.
        /// </summary>
        /// <param name="layoutText">The layout string.</param>
        /// <param name="configurationItemFactory">The NLog factories to use when resolving layout renderers.</param>
        /// <returns>Instance of <see cref="SimpleLayout"/>.</returns>
        public static Layout FromString([Localizable(false)] string layoutText, ConfigurationItemFactory configurationItemFactory)
        {
            return new SimpleLayout(layoutText, configurationItemFactory);
        }

        /// <summary>
        /// Implicitly converts the specified string to a <see cref="SimpleLayout"/>.
        /// </summary>
        /// <param name="layoutText">The layout string.</param>
        /// <param name="throwConfigExceptions">Whether <see cref="NLogConfigurationException"/> should be thrown on parse errors (false = replace unrecognized tokens with a space).</param>
        /// <returns>Instance of <see cref="SimpleLayout"/>.</returns>
        public static Layout FromString([Localizable(false)] string layoutText, bool throwConfigExceptions)
        {
            try
            {
                return new SimpleLayout(layoutText, ConfigurationItemFactory.Default, throwConfigExceptions);
            }
            catch (NLogConfigurationException)
            {
                throw;
            }
            catch (Exception ex)
            {
                if (!throwConfigExceptions || ex.MustBeRethrownImmediately())
                    throw;

                throw new NLogConfigurationException($"Invalid Layout: {layoutText}", ex);
            }
        }

        /// <summary>
        /// Create a <see cref="SimpleLayout"/> from a lambda method.
        /// </summary>
        /// <param name="layoutMethod">Method that renders the layout.</param>
        /// <param name="options">Whether method is ThreadAgnostic and doesn't depend on context of the logging application thread.</param>
        /// <returns>Instance of <see cref="SimpleLayout"/>.</returns>
        public static Layout FromMethod(Func<LogEventInfo, object> layoutMethod, LayoutRenderOptions options = LayoutRenderOptions.None)
        {
            Guard.ThrowIfNull(layoutMethod);

            var name = $"{layoutMethod.Method?.DeclaringType?.ToString()}.{layoutMethod.Method?.Name}";
            var layoutRenderer = CreateFuncLayoutRenderer((l, c) => layoutMethod(l), options, name);
            return new SimpleLayout(new[] { layoutRenderer }, layoutRenderer.LayoutRendererName, ConfigurationItemFactory.Default);
        }

        internal static LayoutRenderers.FuncLayoutRenderer CreateFuncLayoutRenderer(Func<LogEventInfo, LoggingConfiguration, object> layoutMethod, LayoutRenderOptions options, string name)
        {
            if ((options & LayoutRenderOptions.ThreadAgnostic) == LayoutRenderOptions.ThreadAgnostic)
                return new LayoutRenderers.FuncThreadAgnosticLayoutRenderer(name, layoutMethod);
            else
                return new LayoutRenderers.FuncLayoutRenderer(name, layoutMethod);
        }

        /// <summary>
        /// Precalculates the layout for the specified log event and stores the result
        /// in per-log event cache.
        ///
        /// Only if the layout doesn't have [ThreadAgnostic] and doesn't contain layouts with [ThreadAgnostic].
        /// </summary>
        /// <param name="logEvent">The log event.</param>
        /// <remarks>
        /// Calling this method enables you to store the log event in a buffer
        /// and/or potentially evaluate it in another thread even though the
        /// layout may contain thread-dependent renderer.
        /// </remarks>
        public virtual void Precalculate(LogEventInfo logEvent)
        {
            if (!ThreadAgnostic || ThreadAgnosticImmutable)
            {
                using (var localTarget = new AppendBuilderCreator(true))
                {
                    RenderAppendBuilder(logEvent, localTarget.Builder, true);
                }
            }
        }

        /// <summary>
        /// Renders formatted output using the log event as context.
        /// </summary>
        /// <remarks>Inside a <see cref="Target"/>, <see cref="Target.RenderLogEvent"/> is preferred for performance reasons.</remarks>
        /// <param name="logEvent">The logging event.</param>
        /// <returns>The formatted output as string.</returns>
        public string Render(LogEventInfo logEvent)
        {
            return Render(logEvent, true);
        }

        internal string Render(LogEventInfo logEvent, bool cacheLayoutResult)
        {
            if (!IsInitialized)
            {
                Initialize(LoggingConfiguration);
            }

            bool lookupCacheLayout = !ThreadAgnostic || ThreadAgnosticImmutable;
            if (lookupCacheLayout)
            {
                object cachedValue;
                if (logEvent.TryGetCachedLayoutValue(this, out cachedValue))
                {
                    return cachedValue?.ToString() ?? string.Empty;
                }
            }

            string layoutValue = GetFormattedMessage(logEvent) ?? string.Empty;
            if (lookupCacheLayout && cacheLayoutResult)
            {
                // Would be nice to only do this in Precalculate(), but we need to ensure internal cache
                // is updated for custom Layouts that overrides Precalculate (without calling base.Precalculate)
                logEvent.AddCachedLayoutValue(this, layoutValue);
            }
            return layoutValue;
        }

        /// <summary>
        /// Optimized version of <see cref="Render(LogEventInfo)"/> that works best when
        /// override of <see cref="RenderFormattedMessage(LogEventInfo, StringBuilder)"/> is available.
        /// </summary>
        /// <param name="logEvent">The logging event.</param>
        /// <param name="target">Appends the formatted output to target</param>
        public void Render(LogEventInfo logEvent, StringBuilder target)
        {
            RenderAppendBuilder(logEvent, target, false);
        }

        internal virtual void PrecalculateBuilder(LogEventInfo logEvent, StringBuilder target)
        {
            Precalculate(logEvent); // Allow custom Layouts to also work
        }

        /// <summary>
        /// Optimized version of <see cref="Render(LogEventInfo)"/> that works best when
        /// override of <see cref="RenderFormattedMessage(LogEventInfo, StringBuilder)"/> is available.
        /// </summary>
        /// <param name="logEvent">The logging event.</param>
        /// <param name="target">Appends the string representing log event to target</param>
        /// <param name="cacheLayoutResult">Should rendering result be cached on LogEventInfo</param>
        private void RenderAppendBuilder(LogEventInfo logEvent, StringBuilder target, bool cacheLayoutResult)
        {
            if (!IsInitialized)
            {
                Initialize(LoggingConfiguration);
            }

            if (!ThreadAgnostic || ThreadAgnosticImmutable)
            {
                object cachedValue;
                if (logEvent.TryGetCachedLayoutValue(this, out cachedValue))
                {
                    target.Append(cachedValue?.ToString());
                    return;
                }
            }
            else
            {
                cacheLayoutResult = false;
            }

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
        /// <returns>The rendered layout.</returns>
        internal string RenderAllocateBuilder(LogEventInfo logEvent)
        {
            using (var localTarget = new AppendBuilderCreator(true))
            {
                RenderFormattedMessage(logEvent, localTarget.Builder);
                return localTarget.Builder.ToString();
            }
        }

        internal string RenderAllocateBuilder(LogEventInfo logEvent, StringBuilder target)
        {
            RenderFormattedMessage(logEvent, target);
            return target.ToString();
        }

        /// <summary>
        /// Renders formatted output using the log event as context.
        /// </summary>
        /// <param name="logEvent">The logging event.</param>
        /// <param name="target">Appends the formatted output to target</param>
        protected virtual void RenderFormattedMessage(LogEventInfo logEvent, StringBuilder target)
        {
            target.Append(GetFormattedMessage(logEvent));
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
                try
                {
                    LoggingConfiguration = configuration;

                    _scannedForObjects = false;

                    PropertyHelper.CheckRequiredParameters(ConfigurationItemFactory.Default, this);

                    InitializeLayout();

                    if (!_scannedForObjects)
                    {
                        InternalLogger.Debug("{0} Initialized Layout done but not scanned for objects", GetType());
                        PerformObjectScanning();
                    }
                }
                finally
                {
                    IsInitialized = true;
                }
            }
        }

        internal void PerformObjectScanning()
        {
            var objectGraphScannerList = ObjectGraphScanner.FindReachableObjects<IRenderable>(ConfigurationItemFactory.Default, true, this);
            var objectGraphTypes = new HashSet<Type>(objectGraphScannerList.Select(o => o.GetType()));
            objectGraphTypes.Remove(typeof(SimpleLayout));
            objectGraphTypes.Remove(typeof(NLog.LayoutRenderers.LiteralLayoutRenderer));

            // determine whether the layout is thread-agnostic
            // layout is thread agnostic if it is thread-agnostic and
            // all its nested objects are thread-agnostic.
            ThreadAgnostic = objectGraphTypes.All(t => t.IsDefined(typeof(ThreadAgnosticAttribute), true));
            ThreadAgnosticImmutable = ThreadAgnostic && objectGraphTypes.Any(t => t.IsDefined(typeof(ThreadAgnosticImmutableAttribute), true));

            if (objectGraphScannerList.Count > 1 && objectGraphTypes.Count > 0)
            {
                foreach (var nestedLayout in objectGraphScannerList.OfType<Layout>())
                {
                    if (!ReferenceEquals(nestedLayout, this))
                    {
                        nestedLayout.Initialize(LoggingConfiguration);
                        ThreadAgnostic = nestedLayout.ThreadAgnostic && ThreadAgnostic;
                        ThreadAgnosticImmutable = ThreadAgnostic && (nestedLayout.ThreadAgnosticImmutable || ThreadAgnosticImmutable);
                    }
                }
            }

            // determine the max StackTraceUsage, to decide if Logger needs to capture callsite
            StackTraceUsage = StackTraceUsage.None;    // In case this Layout should implement IUsesStackTrace
            StackTraceUsage = objectGraphScannerList.OfType<IUsesStackTrace>().DefaultIfEmpty().Aggregate(StackTraceUsage.None, (usage, item) => usage | item?.StackTraceUsage ?? StackTraceUsage.None);

            _scannedForObjects = true;
        }

        internal Layout[] ResolveLayoutPrecalculation(IEnumerable<Layout> allLayouts)
        {
            if (!_scannedForObjects || (ThreadAgnostic && !ThreadAgnosticImmutable))
                return null;

            int layoutCount = 0;
            int precalculateLayoutCount = 0;
            int precalculateSimpleLayoutCount = 0;

            foreach (var layout in allLayouts)
            {
                ++layoutCount;
                if (layout?.ThreadAgnostic == false || layout?.ThreadAgnosticImmutable == true)
                {
                    precalculateLayoutCount++;
                    if (layout is SimpleLayout)
                    {
                        precalculateSimpleLayoutCount++;
                    }
                }
            }

            if (layoutCount <= 1 || precalculateLayoutCount > 4 || (precalculateLayoutCount - precalculateSimpleLayoutCount) > 2 || (layoutCount - precalculateSimpleLayoutCount) <= 1 || precalculateLayoutCount == 0)
            {
                return null;
            }
            else
            {
                return allLayouts.Where(layout => layout?.ThreadAgnostic == false || layout?.ThreadAgnosticImmutable == true).ToArray();
            }
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
        /// Renders formatted output using the log event as context.
        /// </summary>
        /// <param name="logEvent">The logging event.</param>
        /// <returns>The formatted output.</returns>
        protected abstract string GetFormattedMessage(LogEventInfo logEvent);

        /// <summary>
        /// Obsolete and replaced by <see cref="LogManager.Setup()"/> with NLog v5.2.
        ///
        /// Register a custom Layout.
        /// </summary>
        /// <remarks>Short-cut for registering to default <see cref="ConfigurationItemFactory"/></remarks>
        /// <typeparam name="T"> Type of the Layout.</typeparam>
        /// <param name="name"> Name of the Layout.</param>
        [Obsolete("Instead use LogManager.Setup().SetupExtensions(). Marked obsolete with NLog v5.2")]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public static void Register<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor | DynamicallyAccessedMemberTypes.PublicProperties)] T>(string name)
            where T : Layout
        {
            var layoutRendererType = typeof(T);
            Register(name, layoutRendererType);
        }

        /// <summary>
        /// Obsolete and replaced by <see cref="LogManager.Setup()"/> with NLog v5.2.
        ///
        /// Register a custom Layout.
        /// </summary>
        /// <remarks>Short-cut for registering to default <see cref="ConfigurationItemFactory"/></remarks>
        /// <param name="layoutType"> Type of the Layout.</param>
        /// <param name="name"> Name of the Layout.</param>
        [Obsolete("Instead use LogManager.Setup().SetupExtensions(). Marked obsolete with NLog v5.2")]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public static void Register(string name, [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor | DynamicallyAccessedMemberTypes.PublicProperties)] Type layoutType)
        {
            ConfigurationItemFactory.Default.GetLayoutFactory().RegisterDefinition(name, layoutType);
        }

        /// <summary>
        /// Optimized version of <see cref="Precalculate(LogEventInfo)"/> for internal Layouts, when
        /// override of <see cref="RenderFormattedMessage(LogEventInfo, StringBuilder)"/> is available.
        /// </summary>
        internal void PrecalculateBuilderInternal(LogEventInfo logEvent, StringBuilder target, Layout[] precalculateLayout)
        {
            if (!ThreadAgnostic || ThreadAgnosticImmutable)
            {
                if (precalculateLayout is null)
                {
                    RenderAppendBuilder(logEvent, target, true);
                }
                else
                {
                    foreach (var layout in precalculateLayout)
                    {
                        layout.PrecalculateBuilder(logEvent, target);
                        target.Length = 0;
                    }
                }
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

        /// <summary>
        /// Resolve from DI <see cref="LogFactory.ServiceRepository"/>
        /// </summary>
        /// <remarks>Avoid calling this while handling a LogEvent, since random deadlocks can occur</remarks>
        protected T ResolveService<T>() where T : class
        {
            return LoggingConfiguration.GetServiceProvider().ResolveService<T>(IsInitialized);
        }
    }
}
