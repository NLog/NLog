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
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.Text;
    using NLog.Common;
    using NLog.Config;
    using NLog.Internal;
    using NLog.LayoutRenderers;

    /// <summary>
    /// Represents a string with embedded placeholders that can render contextual information.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This layout is not meant to be used explicitly. Instead you can just use a string containing layout
    /// renderers everywhere the layout is required.
    /// </para>
    /// <a href="https://github.com/NLog/NLog/wiki/SimpleLayout">See NLog Wiki</a>
    /// </remarks>
    /// <seealso href="https://github.com/NLog/NLog/wiki/SimpleLayout">Documentation on NLog Wiki</seealso>
    [Layout("SimpleLayout")]
    [ThreadAgnostic]
    [AppDomainFixedOutput]
    public sealed class SimpleLayout : Layout, IUsesStackTrace, IStringValueRenderer
    {
        private readonly IRawValue _rawValueRenderer;
        private IStringValueRenderer _stringValueRenderer;

        /// <summary>
        /// Initializes a new instance of the <see cref="SimpleLayout" /> class.
        /// </summary>
        public SimpleLayout()
            : this(string.Empty)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SimpleLayout" /> class.
        /// </summary>
        /// <param name="txt">The layout string to parse.</param>
        public SimpleLayout([Localizable(false)] string txt)
            : this(txt, ConfigurationItemFactory.Default)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SimpleLayout"/> class.
        /// </summary>
        /// <param name="txt">The layout string to parse.</param>
        /// <param name="configurationItemFactory">The NLog factories to use when creating references to layout renderers.</param>
        public SimpleLayout([Localizable(false)] string txt, ConfigurationItemFactory configurationItemFactory)
            : this(txt, configurationItemFactory, null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SimpleLayout"/> class.
        /// </summary>
        /// <param name="txt">The layout string to parse.</param>
        /// <param name="configurationItemFactory">The NLog factories to use when creating references to layout renderers.</param>
        /// <param name="throwConfigExceptions">Whether <see cref="NLogConfigurationException"/> should be thrown on parse errors.</param>
        internal SimpleLayout([Localizable(false)] string txt, ConfigurationItemFactory configurationItemFactory, bool? throwConfigExceptions)
            : this(LayoutParser.CompileLayout(txt, configurationItemFactory, throwConfigExceptions, out var parsedTxt), parsedTxt)
        {
            OriginalText = txt ?? string.Empty;
        }

        internal SimpleLayout(LayoutRenderer[] layoutRenderers, [Localizable(false)] string txt)
        {
            Text = txt ?? string.Empty;
            OriginalText = txt ?? string.Empty;

            _layoutRenderers = layoutRenderers ?? ArrayHelper.Empty<LayoutRenderer>();
            _renderers = null;

            FixedText = null;
            _rawValueRenderer = null;
            _stringValueRenderer = null;

            if (_layoutRenderers.Length == 0)
            {
                FixedText = string.Empty;
            }
            else if (_layoutRenderers.Length == 1)
            {
                if (_layoutRenderers[0] is LiteralLayoutRenderer renderer)
                {
                    FixedText = renderer.Text;
                }
                else if (_layoutRenderers[0] is IStringValueRenderer stringValueRenderer)
                {
                    _stringValueRenderer = stringValueRenderer;
                }

                if (_layoutRenderers[0] is IRawValue rawValueRenderer)
                {
                    _rawValueRenderer = rawValueRenderer;
                }
            }
        }

        /// <summary>
        /// Original text before parsing as Layout renderes.
        /// </summary>
        public string OriginalText { get; }

        /// <summary>
        /// Gets or sets the layout text that could be parsed.
        /// </summary>
        /// <docgen category='Layout Options' order='10' />
        public string Text { get; }

        /// <summary>
        /// Is the message fixed? (no Layout renderers used)
        /// </summary>
        public bool IsFixedText => FixedText != null;

        /// <summary>
        /// Get the fixed text. Only set when <see cref="IsFixedText"/> is <c>true</c>
        /// </summary>
        public string FixedText { get; }

        /// <summary>
        /// Is the message a simple formatted string? (Can skip StringBuilder)
        /// </summary>
        internal bool IsSimpleStringText => _stringValueRenderer != null;

        /// <summary>
        /// Gets a collection of <see cref="LayoutRenderer"/> objects that make up this layout.
        /// </summary>
        [NLogConfigurationIgnoreProperty]
        public ReadOnlyCollection<LayoutRenderer> Renderers => _renderers ?? (_renderers = new ReadOnlyCollection<LayoutRenderer>(_layoutRenderers));
        private ReadOnlyCollection<LayoutRenderer> _renderers;
        private readonly LayoutRenderer[] _layoutRenderers;

        /// <summary>
        /// Gets a collection of <see cref="LayoutRenderer"/> objects that make up this layout.
        /// </summary>
        public IEnumerable<LayoutRenderer> LayoutRenderers => _layoutRenderers;

        /// <summary>
        /// Gets the level of stack trace information required for rendering.
        /// </summary>
        public new StackTraceUsage StackTraceUsage => base.StackTraceUsage;

        /// <summary>
        /// Implicitly converts the specified string as LayoutRenderer-expression into a <see cref="SimpleLayout"/>.
        /// </summary>
        /// <param name="text">Text to be converted.</param>
        /// <returns>A <see cref="SimpleLayout"/> object.</returns>
        public static implicit operator SimpleLayout([Localizable(false)] string text)
        {
            if (text is null) return null;

            return new SimpleLayout(text);
        }

        /// <summary>
        /// Escapes the passed text so that it can
        /// be used literally in all places where
        /// layout is normally expected without being
        /// treated as layout.
        /// </summary>
        /// <param name="text">The text to be escaped.</param>
        /// <returns>The escaped text.</returns>
        /// <remarks>
        /// Escaping is done by replacing all occurrences of
        /// '${' with '${literal:text=${}'
        /// </remarks>
        [Obsolete("Instead use Layout.FromLiteral()")]
        public static string Escape([Localizable(false)] string text)
        {
            return text.Replace("${", @"${literal:text=\$\{}");
        }

        /// <summary>
        /// Evaluates the specified text by expanding all layout renderers.
        /// </summary>
        /// <param name="text">The text to be evaluated.</param>
        /// <param name="logEvent">Log event to be used for evaluation.</param>
        /// <returns>The input text with all occurrences of ${} replaced with
        /// values provided by the appropriate layout renderers.</returns>
        public static string Evaluate([Localizable(false)] string text, LogEventInfo logEvent)
        {
            return Evaluate(text, null, logEvent);
        }

        /// <summary>
        /// Evaluates the specified text by expanding all layout renderers
        /// in new <see cref="LogEventInfo" /> context.
        /// </summary>
        /// <param name="text">The text to be evaluated.</param>
        /// <returns>The input text with all occurrences of ${} replaced with
        /// values provided by the appropriate layout renderers.</returns>
        public static string Evaluate([Localizable(false)] string text)
        {
            return Evaluate(text, null);
        }

        internal static string Evaluate(string text, LoggingConfiguration loggingConfiguration, LogEventInfo logEventInfo = null, bool? throwConfigExceptions = null)
        {
            try
            {
                if (string.IsNullOrEmpty(text))
                    return string.Empty;

                if (text.IndexOf('$') < 0 || text.IndexOf('{') < 0 || text.IndexOf('}') < 0)
                    return text;

                throwConfigExceptions = throwConfigExceptions ?? loggingConfiguration?.LogFactory.ThrowConfigExceptions;
                var layout = Layout.FromString(text, throwConfigExceptions ?? LogManager.ThrowConfigExceptions ?? LogManager.ThrowExceptions);
                layout.Initialize(loggingConfiguration);
                return layout.Render(logEventInfo ?? LogEventInfo.CreateNullEvent());
            }
            catch (NLogConfigurationException ex)
            {
                if (throwConfigExceptions ?? LogManager.ThrowConfigExceptions ?? LogManager.ThrowExceptions)
                    throw;

                InternalLogger.Warn(ex, "Failed to Evaluate SimpleLayout: {0}", text);
                return text;
            }
            catch (Exception ex)
            {
                InternalLogger.Warn(ex, "Failed to Evaluate SimpleLayout: {0}", text);
                return text;
            }
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            if (string.IsNullOrEmpty(Text) && !IsFixedText && _layoutRenderers.Length > 0)
            {
                return ToStringWithNestedItems(_layoutRenderers, r => r.ToString());
            }

            return Text;
        }

        /// <inheritdoc/>
        protected override void InitializeLayout()
        {
            foreach (var renderer in _layoutRenderers)
            {
                try
                {
                    renderer.Initialize(LoggingConfiguration);
                }
                catch (Exception exception)
                {
                    //also check IsErrorEnabled, otherwise 'MustBeRethrown' writes it to Error
                    if (InternalLogger.IsWarnEnabled || InternalLogger.IsErrorEnabled)
                    {
                        InternalLogger.Warn(exception, "Exception in '{0}.Initialize()'", renderer.GetType());
                    }

                    if (exception.MustBeRethrown())
                    {
                        throw;
                    }
                }
            }

            base.InitializeLayout();
        }

        /// <inheritdoc/>
        public override void Precalculate(LogEventInfo logEvent)
        {
            if (PrecalculateMustRenderLayoutValue(logEvent))
            {
                using (var localTarget = new AppendBuilderCreator(true))
                {
                    RenderFormattedMessage(logEvent, localTarget.Builder);
                    logEvent.AddCachedLayoutValue(this, localTarget.Builder.ToString());
                }
            }
        }

        internal override void PrecalculateBuilder(LogEventInfo logEvent, StringBuilder target)
        {
            if (PrecalculateMustRenderLayoutValue(logEvent))
            {
                RenderFormattedMessage(logEvent, target);
                logEvent.AddCachedLayoutValue(this, target.ToString());
            }
        }

        private bool PrecalculateMustRenderLayoutValue(LogEventInfo logEvent)
        {
            if (!IsInitialized)
            {
                Initialize(LoggingConfiguration);
            }

            if (ThreadAgnostic && !ThreadAgnosticImmutable)
                return false;

            if (_rawValueRenderer != null && TryGetRawValue(logEvent, out var rawValue) && IsRawValueImmutable(rawValue))
                return false;   // If raw value is immutable, then we can skip precalculate-caching

            if (logEvent.TryGetCachedLayoutValue(this, out var _))
                return false;

            if (IsSimpleStringText)
            {
                var cachedLayout = GetFormattedMessage(logEvent);
                logEvent.AddCachedLayoutValue(this, cachedLayout);
                return false;
            }

            return true;
        }

        private static bool IsRawValueImmutable(object value)
        {
            return value != null && (Convert.GetTypeCode(value) != TypeCode.Object || value.GetType().IsValueType);
        }

        /// <inheritdoc/>
        internal override bool TryGetRawValue(LogEventInfo logEvent, out object rawValue)
        {
            if (_rawValueRenderer is null)
            {
                rawValue = null;
                return false;
            }
            return TryGetSafeRawValue(logEvent, out rawValue);
        }

        private bool TryGetSafeRawValue(LogEventInfo logEvent, out object rawValue)
        {
            try
            {
                if (!IsInitialized)
                {
                    Initialize(LoggingConfiguration);
                }

                if ((!ThreadAgnostic || ThreadAgnosticImmutable) && logEvent.TryGetCachedLayoutValue(this, out _))
                {
                    rawValue = null;
                    return false;    // Raw-Value has been precalculated, so not available
                }

                return _rawValueRenderer.TryGetRawValue(logEvent, out rawValue);
            }
            catch (Exception exception)
            {
                //also check IsErrorEnabled, otherwise 'MustBeRethrown' writes it to Error
                if (InternalLogger.IsWarnEnabled || InternalLogger.IsErrorEnabled)
                {
                    InternalLogger.Warn(exception, "Exception in TryGetRawValue using '{0}.TryGetRawValue()'", _rawValueRenderer?.GetType());
                }

                if (exception.MustBeRethrown())
                {
                    throw;
                }
            }

            rawValue = null;
            return false;
        }

        /// <inheritdoc/>
        protected override string GetFormattedMessage(LogEventInfo logEvent)
        {
            if (IsFixedText)
                return FixedText;

            string stringValue = string.Empty;
            if (_stringValueRenderer is null || !TryGetSafeStringValue(logEvent, out stringValue))
                return RenderAllocateBuilder(logEvent);
            else
                return stringValue;
        }

        private bool TryGetSafeStringValue(LogEventInfo logEvent, out string stringValue)
        {
            try
            {
                if (!IsInitialized)
                {
                    Initialize(LoggingConfiguration);
                }

                stringValue = _stringValueRenderer.GetFormattedString(logEvent);
                if (stringValue is null)
                {
                    _stringValueRenderer = null;    // Optimization is not possible
                    return false;
                }
                return true;
            }
            catch (Exception exception)
            {
                //also check IsErrorEnabled, otherwise 'MustBeRethrown' writes it to Error
                if (InternalLogger.IsWarnEnabled || InternalLogger.IsErrorEnabled)
                {
                    InternalLogger.Warn(exception, "Exception in '{0}.GetFormattedString()'", _stringValueRenderer?.GetType());
                }

                if (exception.MustBeRethrown())
                {
                    throw;
                }
            }

            stringValue = null;
            return false;
        }

        /// <inheritdoc/>
        protected override void RenderFormattedMessage(LogEventInfo logEvent, StringBuilder target)
        {
            if (FixedText is null)
            {
                foreach (var renderer in _layoutRenderers)
                {
                    renderer.RenderAppendBuilder(logEvent, target);
                }
            }
            else
            {
                target.Append(FixedText);
            }
        }

        string IStringValueRenderer.GetFormattedString(LogEventInfo logEvent)
        {
            if (IsFixedText)
                return FixedText;
            if (IsSimpleStringText)
                return Render(logEvent);
            return null;
        }
    }
}
