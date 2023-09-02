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
    public class SimpleLayout : Layout, IUsesStackTrace
    {
        private string _fixedText;
        private string _layoutText;
        private IRawValue _rawValueRenderer;
        private IStringValueRenderer _stringValueRenderer;
        private readonly ConfigurationItemFactory _configurationItemFactory;

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
            :this(txt, configurationItemFactory, null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SimpleLayout"/> class.
        /// </summary>
        /// <param name="txt">The layout string to parse.</param>
        /// <param name="configurationItemFactory">The NLog factories to use when creating references to layout renderers.</param>
        /// <param name="throwConfigExceptions">Whether <see cref="NLogConfigurationException"/> should be thrown on parse errors.</param>
        internal SimpleLayout([Localizable(false)] string txt, ConfigurationItemFactory configurationItemFactory, bool? throwConfigExceptions)
        {
            _configurationItemFactory = configurationItemFactory;
            SetLayoutText(txt, throwConfigExceptions);
        }

        internal SimpleLayout(LayoutRenderer[] renderers, [Localizable(false)] string text, ConfigurationItemFactory configurationItemFactory)
        {
            _configurationItemFactory = configurationItemFactory;
            OriginalText = text;
            SetLayoutRenderers(renderers, text);
        }

        /// <summary>
        /// Original text before compile to Layout renderes
        /// </summary>
        public string OriginalText { get; private set; }

        /// <summary>
        /// Gets or sets the layout text.
        /// </summary>
        /// <docgen category='Layout Options' order='10' />
        public string Text
        {
            get => _layoutText;
            set => SetLayoutText(value);
        }

        private void SetLayoutText(string value, bool? throwConfigExceptions = null)
        {
            OriginalText = value;
            var renderers = LayoutParser.CompileLayout(value, _configurationItemFactory, throwConfigExceptions, out var txt);
            SetLayoutRenderers(renderers, txt);
        }

        /// <summary>
        /// Is the message fixed? (no Layout renderers used)
        /// </summary>
        public bool IsFixedText => _fixedText != null;

        /// <summary>
        /// Get the fixed text. Only set when <see cref="IsFixedText"/> is <c>true</c>
        /// </summary>
        public string FixedText => _fixedText;

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
        private LayoutRenderer[] _layoutRenderers;

        /// <summary>
        /// Gets a collection of <see cref="LayoutRenderer"/> objects that make up this layout.
        /// </summary>
        public IEnumerable<LayoutRenderer> LayoutRenderers => _layoutRenderers;

        /// <summary>
        /// Gets the level of stack trace information required for rendering.
        /// </summary>
        public new StackTraceUsage StackTraceUsage => base.StackTraceUsage;

        /// <summary>
        /// Converts a text to a simple layout.
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
            var layout = new SimpleLayout(text);
            return layout.Render(logEvent);
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
            return Evaluate(text, LogEventInfo.CreateNullEvent());
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            if (string.IsNullOrEmpty(Text) && !IsFixedText && _layoutRenderers.Length > 0)
            {
                return ToStringWithNestedItems(_layoutRenderers, r => r.ToString());
            }

            return Text ?? _fixedText ?? string.Empty;
        }

        internal void SetLayoutRenderers(LayoutRenderer[] layoutRenderers, string text)
        {
            _layoutRenderers = layoutRenderers ?? ArrayHelper.Empty<LayoutRenderer>();
            _renderers = null;

            _fixedText = null;
            _rawValueRenderer = null;
            _stringValueRenderer = null;

            if (_layoutRenderers.Length == 0)
            {
                _fixedText = string.Empty;
            }
            else if (_layoutRenderers.Length == 1)
            {
                if (_layoutRenderers[0] is LiteralLayoutRenderer renderer)
                {
                    _fixedText = renderer.Text;
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

            _layoutText = text;

            if (LoggingConfiguration != null)
            {
                PerformObjectScanning();
            }
        }

        /// <inheritdoc/>
        protected override void InitializeLayout()
        {
            for (int i = 0; i < _layoutRenderers.Length; i++)
            {
                LayoutRenderer renderer = _layoutRenderers[i];
                try
                {
                    renderer.Initialize(LoggingConfiguration);
                }
                catch (Exception exception)
                {
                    //also check IsErrorEnabled, otherwise 'MustBeRethrown' writes it to Error

                    //check for performance
                    if (InternalLogger.IsWarnEnabled || InternalLogger.IsErrorEnabled)
                    {
                        InternalLogger.Warn(exception, "Exception in '{0}.InitializeLayout()'", renderer.GetType().FullName);
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
            if (!IsLogEventMutableSafe(logEvent))
            {
                Render(logEvent);
            }
        }

        internal override void PrecalculateBuilder(LogEventInfo logEvent, StringBuilder target)
        {
            if (!IsLogEventMutableSafe(logEvent))
            {
                PrecalculateBuilderInternal(logEvent, target, null);
            }
        }

        private bool IsLogEventMutableSafe(LogEventInfo logEvent)
        {
            if (_rawValueRenderer != null)
            {
                try
                {
                    if (!IsInitialized)
                    {
                        Initialize(LoggingConfiguration);
                    }

                    if (ThreadAgnostic)
                    {
                        if (MutableUnsafe)
                        {
                            // If raw value doesn't have the ability to mutate, then we can skip precalculate
                            var success = _rawValueRenderer.TryGetRawValue(logEvent, out var value);
                            if (success && IsObjectValueMutableSafe(value))
                                return true;
                        }
                        else
                        {
                            return true;
                        }
                    }

                    return false;
                }
                catch (Exception exception)
                {
                    //also check IsErrorEnabled, otherwise 'MustBeRethrown' writes it to Error

                    //check for performance
                    if (InternalLogger.IsWarnEnabled || InternalLogger.IsErrorEnabled)
                    {
                        InternalLogger.Warn(exception, "Exception in precalculate using '{0}.TryGetRawValue()'", _rawValueRenderer?.GetType());
                    }

                    if (exception.MustBeRethrown())
                    {
                        throw;
                    }
                }
            }

            return ThreadAgnostic && !MutableUnsafe;
        }

        private static bool IsObjectValueMutableSafe(object value)
        {
            return value != null && (Convert.GetTypeCode(value) != TypeCode.Object || value.GetType().IsValueType());
        }

        /// <inheritdoc/>
        internal override bool TryGetRawValue(LogEventInfo logEvent, out object rawValue)
        {
            if (_rawValueRenderer != null)
            {
                try
                {
                    if (!IsInitialized)
                    {
                        Initialize(LoggingConfiguration);
                    }

                    if ((!ThreadAgnostic || MutableUnsafe) && logEvent.TryGetCachedLayoutValue(this, out _))
                    {
                        rawValue = null;
                        return false;    // Raw-Value has been precalculated, so not available
                    }

                    var success = _rawValueRenderer.TryGetRawValue(logEvent, out rawValue);
                    return success;
                }
                catch (Exception exception)
                {
                    //also check IsErrorEnabled, otherwise 'MustBeRethrown' writes it to Error

                    //check for performance
                    if (InternalLogger.IsWarnEnabled || InternalLogger.IsErrorEnabled)
                    {
                        InternalLogger.Warn(exception, "Exception in TryGetRawValue using '{0}.TryGetRawValue()'", _rawValueRenderer?.GetType());
                    }

                    if (exception.MustBeRethrown())
                    {
                        throw;
                    }
                }
            }

            rawValue = null;
            return false;
        }

        /// <inheritdoc/>
        protected override string GetFormattedMessage(LogEventInfo logEvent)
        {
            if (IsFixedText)
            {
                return _fixedText;
            }

            if (_stringValueRenderer != null)
            {
                try
                {
                    string stringValue = _stringValueRenderer.GetFormattedString(logEvent);
                    if (stringValue != null)
                        return stringValue;

                    _stringValueRenderer = null;    // Optimization is not possible
                }
                catch (Exception exception)
                {
                    //also check IsErrorEnabled, otherwise 'MustBeRethrown' writes it to Error
                    //check for performance
                    if (InternalLogger.IsWarnEnabled || InternalLogger.IsErrorEnabled)
                    {
                        InternalLogger.Warn(exception, "Exception in '{0}.GetFormattedString()'", _stringValueRenderer.GetType().FullName);
                    }

                    if (exception.MustBeRethrown())
                    {
                        throw;
                    }
                }
            }

            return RenderAllocateBuilder(logEvent);
        }

        private void RenderAllRenderers(LogEventInfo logEvent, StringBuilder target)
        {
            //Memory profiling pointed out that using a foreach-loop was allocating
            //an Enumerator. Switching to a for-loop avoids the memory allocation.
            for (int i = 0; i < _layoutRenderers.Length; i++)
            {
                LayoutRenderer renderer = _layoutRenderers[i];
                try
                {
                    renderer.RenderAppendBuilder(logEvent, target);
                }
                catch (Exception exception)
                {
                    //also check IsErrorEnabled, otherwise 'MustBeRethrown' writes it to Error

                    //check for performance
                    if (InternalLogger.IsWarnEnabled || InternalLogger.IsErrorEnabled)
                    {
                        InternalLogger.Warn(exception, "Exception in '{0}.Append()'", renderer.GetType().FullName);
                    }

                    if (exception.MustBeRethrown())
                    {
                        throw;
                    }
                }
            }
        }

        /// <inheritdoc/>
        protected override void RenderFormattedMessage(LogEventInfo logEvent, StringBuilder target)
        {
            if (IsFixedText)
            {
                target.Append(_fixedText);
                return;
            }

            RenderAllRenderers(logEvent, target);
        }
    }
}
