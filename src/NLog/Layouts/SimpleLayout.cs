// 
// Copyright (c) 2004-2006 Jaroslaw Kowalski <jaak@jkowalski.net>
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

using System;
using System.Collections.Generic;
using System.Text;

using NLog.Internal;
using NLog.LayoutRenderers;

namespace NLog.Layouts
{
    /// <summary>
    /// Represents a string with embedded placeholders that can render contextual information.
    /// </summary>
    [Layout("SimpleLayout")]
    public sealed class SimpleLayout : Layout
    {
        #region Constants and Fields

        private string fixedText;

        private bool isVolatile = false;

        private string layoutText;
        private LayoutRenderer[] renderers;
        private StackTraceUsage stackTraceUsage = StackTraceUsage.None;

        #endregion

        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the SimpleLayout class.
        /// </summary>
        public SimpleLayout()
        {
            this.Text = String.Empty;
        }

        /// <summary>
        /// Initializes a new instance of the SimpleLayout class.
        /// </summary>
        /// <param name="txt">The layout string to parse.</param>
        public SimpleLayout(string txt)
        {
            this.Text = txt;
        }

        internal SimpleLayout(LayoutRenderer[] renderers, string text)
        {
            this.SetRenderers(renderers, text);
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets the layout text.
        /// </summary>
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
                    new LayoutParser.Tokenizer(value),
                    false,
                    out txt);

                this.SetRenderers(renderers, txt);
            }
        }

        /// <summary>
        /// Gets a collection of <see cref="LayoutRenderer"/> objects that make up this layout.
        /// </summary>
        internal IEnumerable<LayoutRenderer> Renderers
        {
            get { return this.renderers; }
        }

        #endregion

        #region Operators

        /// <summary>
        /// Converts a text to a simple layout.
        /// </summary>
        /// <param name="text">Text to be converted.</param>
        /// <returns>A <see cref="SimpleLayout"/> object.</returns>
        public static implicit operator SimpleLayout(string text)
        {
            return new SimpleLayout(text);
        }

        #endregion

        #region Public Methods

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
            SimpleLayout l = new SimpleLayout(text);
            return l.GetFormattedMessage(logEvent);
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
        /// Renders the layout for the specified logging event by invoking layout renderers
        /// that make up the event.
        /// </summary>
        /// <param name="logEvent">The logging event.</param>
        /// <returns>The rendered layout.</returns>
        public override string GetFormattedMessage(LogEventInfo logEvent)
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

            int size = 0;

            for (int i = 0; i < this.renderers.Length; ++i)
            {
                LayoutRenderer app = this.renderers[i];
                try
                {
                    int ebs = app.GetEstimatedBufferSize(logEvent);
                    size += ebs;
                }
                catch (Exception ex)
                {
                    if (InternalLogger.IsWarnEnabled)
                    {
                        InternalLogger.Warn("Exception in {0}.GetEstimatedBufferSize(): {1}.", app.GetType().FullName, ex);
                    }
                }
            }

            StringBuilder builder = new StringBuilder(size);

            for (int i = 0; i < this.renderers.Length; ++i)
            {
                LayoutRenderer app = this.renderers[i];
                try
                {
                    app.Append(builder, logEvent);
                }
                catch (Exception ex)
                {
                    if (InternalLogger.IsWarnEnabled)
                    {
                        InternalLogger.Warn("Exception in {0}.Append(): {1}.", app.GetType().FullName, ex);
                    }
                }
            }

            string value = builder.ToString();
            logEvent.AddCachedLayoutValue(this, value);
            return value;
        }

        /// <summary>
        /// Returns the value indicating whether a stack trace and/or the source file
        /// information should be gathered during layout processing.
        /// </summary>
        /// <returns>0 - don't include stack trace<br/>1 - include stack trace without source file information<br/>2 - include full stack trace.</returns>
        public override StackTraceUsage GetStackTraceUsage()
        {
            return this.stackTraceUsage;
        }

        /// <summary>
        /// Initializes the layout.
        /// </summary>
        public override void Initialize()
        {
            base.Initialize();

            foreach (LayoutRenderer lr in this.Renderers)
            {
                lr.Initialize();
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the value of layout is fixed for current AppDomain.
        /// </summary>
        /// <returns>
        /// A value of <c>true</c> if value of layout is fixed for current AppDomain, otherwise <c>false</c>.
        /// </returns>
        public override bool IsAppDomainFixed()
        {
            return this.fixedText != null;
        }

        /// <summary>
        /// Returns the value indicating whether this layout includes any volatile 
        /// layout renderers.
        /// </summary>
        /// <returns>A value of <see langword="true" /> when the layout includes at least 
        /// one volatile renderer, <see langword="false"/> otherwise.</returns>
        /// <remarks>
        /// Volatile layout renderers are dependent on information not contained 
        /// in <see cref="LogEventInfo"/> (such as thread-specific data, MDC data, NDC data).
        /// </remarks>
        public override bool IsVolatile()
        {
            return this.isVolatile;
        }

        /// <summary>
        /// Returns a <see cref="T:System.String"></see> that represents the current object.
        /// </summary>
        /// <returns>
        /// A <see cref="T:System.String"></see> that represents the current object.
        /// </returns>
        public override string ToString()
        {
            return this.Text;
        }

        #endregion

        #region Methods

        internal void SetRenderers(LayoutRenderer[] renderers, string text)
        {
            this.renderers = renderers;
            if (this.renderers.Length == 1 && this.renderers[0] is LiteralLayoutRenderer)
            {
                this.fixedText = ((LiteralLayoutRenderer)this.renderers[0]).Text;
            }
            else
            {
                this.fixedText = null;
            }

            this.layoutText = text;

            this.isVolatile = false;
            this.stackTraceUsage = StackTraceUsage.None;

            foreach (LayoutRenderer lr in renderers)
            {
                StackTraceUsage stu = lr.GetStackTraceUsage();
                if (stu > this.stackTraceUsage)
                {
                    this.stackTraceUsage = stu;
                }

                if (lr.IsVolatile())
                {
                    this.isVolatile = true;
                }
            }
        }

        #endregion
    }
}
