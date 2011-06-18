// 
// Copyright (c) 2004-2011 Jaroslaw Kowalski <jaak@jkowalski.net>
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
    using System.Collections.ObjectModel;
    using System.Text;
    using NLog.Common;
    using NLog.Config;
    using NLog.Internal;
    using NLog.LayoutRenderers;

    /// <summary>
    /// Represents a string with embedded placeholders that can render contextual information.
    /// </summary>
    /// <remarks>
    /// This layout is not meant to be used explicitly. Instead you can just use a string containing layout 
    /// renderers everywhere the layout is required.
    /// </remarks>
    [Layout("SimpleLayout")]
    [ThreadAgnostic]
    [AppDomainFixedOutput]
    public class SimpleLayout : Layout
    {
        private const int MaxInitialRenderBufferLength = 16384;
        private int maxRenderedLength;

        private string fixedText;
        private string layoutText;
        private ConfigurationItemFactory configurationItemFactory;

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
        public SimpleLayout(string txt)
            : this(txt, ConfigurationItemFactory.Default)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SimpleLayout"/> class.
        /// </summary>
        /// <param name="txt">The layout string to parse.</param>
        /// <param name="configurationItemFactory">The NLog factories to use when creating references to layout renderers.</param>
        public SimpleLayout(string txt, ConfigurationItemFactory configurationItemFactory)
        {
            this.configurationItemFactory = configurationItemFactory;
            this.Text = txt;
        }

        internal SimpleLayout(LayoutRenderer[] renderers, string text, ConfigurationItemFactory configurationItemFactory)
        {
            this.configurationItemFactory = configurationItemFactory;
            this.SetRenderers(renderers, text);
        }

        /// <summary>
        /// Gets or sets the layout text.
        /// </summary>
        /// <docgen category='Layout Options' order='10' />
        public string Text
        {
            get
            {
                return this.layoutText;
            }

            set
            {
                LayoutRenderer[] renderers;
                string txt;

                renderers = LayoutParser.CompileLayout(
                    this.configurationItemFactory,
                    new SimpleStringReader(value),
                    false,
                    out txt);

                this.SetRenderers(renderers, txt);
            }
        }

        /// <summary>
        /// Gets a collection of <see cref="LayoutRenderer"/> objects that make up this layout.
        /// </summary>
        public ReadOnlyCollection<LayoutRenderer> Renderers { get; private set; }

        /// <summary>
        /// Converts a text to a simple layout.
        /// </summary>
        /// <param name="text">Text to be converted.</param>
        /// <returns>A <see cref="SimpleLayout"/> object.</returns>
        public static implicit operator SimpleLayout(string text)
        {
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
        /// Escaping is done by replacing all occurences of
        /// '${' with '${literal:text=${}'
        /// </remarks>
        public static string Escape(string text)
        {
            return text.Replace("${", "${literal:text=${}");
        }

        /// <summary>
        /// Evaluates the specified text by expadinging all layout renderers.
        /// </summary>
        /// <param name="text">The text to be evaluated.</param>
        /// <param name="logEvent">Log event to be used for evaluation.</param>
        /// <returns>The input text with all occurences of ${} replaced with
        /// values provided by the appropriate layout renderers.</returns>
        public static string Evaluate(string text, LogEventInfo logEvent)
        {
            var l = new SimpleLayout(text);
            return l.Render(logEvent);
        }

        /// <summary>
        /// Evaluates the specified text by expadinging all layout renderers
        /// in new <see cref="LogEventInfo" /> context.
        /// </summary>
        /// <param name="text">The text to be evaluated.</param>
        /// <returns>The input text with all occurences of ${} replaced with
        /// values provided by the appropriate layout renderers.</returns>
        public static string Evaluate(string text)
        {
            return Evaluate(text, LogEventInfo.CreateNullEvent());
        }

        /// <summary>
        /// Returns a <see cref="T:System.String"></see> that represents the current object.
        /// </summary>
        /// <returns>
        /// A <see cref="T:System.String"></see> that represents the current object.
        /// </returns>
        public override string ToString()
        {
            return "'" + this.Text + "'";
        }

        internal void SetRenderers(LayoutRenderer[] renderers, string text)
        {
            this.Renderers = new ReadOnlyCollection<LayoutRenderer>(renderers);
            if (this.Renderers.Count == 1 && this.Renderers[0] is LiteralLayoutRenderer)
            {
                this.fixedText = ((LiteralLayoutRenderer)this.Renderers[0]).Text;
            }
            else
            {
                this.fixedText = null;
            }

            this.layoutText = text;
        }

        /// <summary>
        /// Renders the layout for the specified logging event by invoking layout renderers
        /// that make up the event.
        /// </summary>
        /// <param name="logEvent">The logging event.</param>
        /// <returns>The rendered layout.</returns>
        protected override string GetFormattedMessage(LogEventInfo logEvent)
        {
            if (this.fixedText != null)
            {
                return this.fixedText;
            }

            string cachedValue;

            if (logEvent.TryGetCachedLayoutValue(this, out cachedValue))
            {
                return cachedValue;
            }

            int initialSize = this.maxRenderedLength;
            if (initialSize > MaxInitialRenderBufferLength)
            {
                initialSize = MaxInitialRenderBufferLength;
            }

            var builder = new StringBuilder(initialSize);

            foreach (LayoutRenderer renderer in this.Renderers)
            {
                try
                {
                    renderer.Render(builder, logEvent);
                }
                catch (Exception exception)
                {
                    if (exception.MustBeRethrown())
                    {
                        throw;
                    }

                    if (InternalLogger.IsWarnEnabled)
                    {
                        InternalLogger.Warn("Exception in {0}.Append(): {1}.", renderer.GetType().FullName, exception);
                    }
                }
            }

            if (builder.Length > this.maxRenderedLength)
            {
                this.maxRenderedLength = builder.Length;
            }

            string value = builder.ToString();
            logEvent.AddCachedLayoutValue(this, value);
            return value;
        }
    }
}
