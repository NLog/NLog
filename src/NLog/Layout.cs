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
using System.Text;
using System.Collections;

using NLog.Internal;
using NLog.LayoutRenderers;

using System.Threading;
using NLog.Config;
using System.IO;
using System.Reflection;

namespace NLog
{
    /// <summary>
    /// Represents a string with embedded placeholders that can render contextual information.
    /// </summary>
    [Layout("SimpleLayout")]
    public sealed class Layout : ILayout
    {
        /// <summary>
        /// Creates a new instance of the <see cref="Layout"/> and sets it to empty string.
        /// </summary>
        public Layout()
        {
            Text = String.Empty;
        }

        /// <summary>
        /// Creates a new instance of the <see cref="Layout"/> and sets it to the specified string.
        /// </summary>
        /// <param name="txt">The layout string to parse.</param>
        public Layout(string txt)
        {
            Text = txt;
        }

        internal Layout(LayoutRenderer[] renderers, string text)
        {
            SetRenderers(renderers, text);
        }

        private string _layoutText;
        private LayoutRenderer[] _renderers;
        private int _needsStackTrace = 0;
        private bool _isVolatile = false;
        private string _fixedText;

        /// <summary>
        /// The layout text
        /// </summary>
        [AcceptsLayout]
        public string Text
        {
            get { return _layoutText; }
            set { 
                LayoutRenderer[] renderers;
                string txt;
                
                renderers = LayoutParser.CompileLayout(
                    new LayoutParser.Tokenizer(value), 
                    false,
                    out txt);

                SetRenderers(renderers, txt);
            }
        }

        /// <summary>
        /// Returns true if this layout produces a value that doesn't change for a particular
        /// AppDomain.
        /// </summary>
        public bool IsAppDomainFixed
        {
            get { return _fixedText != null; }
        }

        /// <summary>
        /// Renders the layout for the specified logging event by invoking layout renderers
        /// that make up the event.
        /// </summary>
        /// <param name="logEvent">The logging event.</param>
        /// <returns>The rendered layout.</returns>
        public string GetFormattedMessage(LogEventInfo logEvent)
        {
            if (_fixedText != null)
                return _fixedText;

            string cachedValue = logEvent.GetCachedLayoutValue(this);
            if (cachedValue != null)
                return cachedValue;

            int size = 0;

            for (int i = 0; i < _renderers.Length; ++i)
            {
                LayoutRenderer app = _renderers[i];
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

            for (int i = 0; i < _renderers.Length; ++i)
            {
                LayoutRenderer app = _renderers[i];
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
        /// <returns>0 - don't include stack trace<br/>1 - include stack trace without source file information<br/>2 - include full stack trace</returns>
        public int NeedsStackTrace()
        {
            return _needsStackTrace;
        }

        /// <summary>
        /// Returns the value indicating whether this layout includes any volatile 
        /// layout renderers.
        /// </summary>
        /// <returns><see langword="true" /> when the layout includes at least 
        /// one volatile renderer, <see langword="false"/> otherwise.</returns>
        /// <remarks>
        /// Volatile layout renderers are dependent on information not contained 
        /// in <see cref="LogEventInfo"/> (such as thread-specific data, MDC data, NDC data).
        /// </remarks>
        public bool IsVolatile()
        {
            return _isVolatile;
        }

        /// <summary>
        /// A collection of <see cref="LayoutRenderer"/> objects that make up this layout.
        /// </summary>
        public LayoutRenderer[] Renderers
        {
            get { return _renderers; }
        }

        internal void SetRenderers(LayoutRenderer[] renderers, string text)
        {
            _renderers = renderers;
            if (_renderers.Length == 1 && _renderers[0] is LiteralLayoutRenderer)
                _fixedText = ((LiteralLayoutRenderer)(_renderers[0])).Text;
            else
                _fixedText = null;

            _layoutText = text;

            _isVolatile = false;
            _needsStackTrace = 0;

            foreach (LayoutRenderer lr in renderers)
            {
                int nst = lr.NeedsStackTrace();
                if (nst > _needsStackTrace)
                    _needsStackTrace = nst;
                if (lr.IsVolatile())
                    _isVolatile = true;
            }
        }

        /// <summary>
        /// Precalculates the layout for the specified log event and stores the result
        /// in per-log event cache.
        /// </summary>
        /// <param name="logEvent">The log event.</param>
        /// <remarks>
        /// Calling this method enables you to store the log event in a buffer
        /// and/or potentially evaluate it in another thread even though the 
        /// layout may contain thread-dependent renderer.
        /// </remarks>
        public void Precalculate(LogEventInfo logEvent)
        {
            //Console.WriteLine("Precalculating {0}", this.Text);
            GetFormattedMessage(logEvent);
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
        /// <param name="logEvent">Log event to be used for evaluation</param>
        /// <returns>The input text with all occurences of ${} replaced with
        /// values provided by the appropriate layout renderers.</returns>
        public static string Evaluate(string text, LogEventInfo logEvent)
        {
            Layout l = new Layout(text);
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
        /// Initializes the layout.
        /// </summary>
        public void Initialize()
        {
            foreach (LayoutRenderer lr in Renderers)
            {
                lr.Initialize();
            }
        }

        /// <summary>
        /// Closes the layout;
        /// </summary>
        public void Close()
        {
        }

        /// <summary>
        /// Add this layout and all sub-layouts to the specified collection..
        /// </summary>
        /// <param name="layouts">The collection of layouts.</param>
        public void PopulateLayouts(LayoutCollection layouts)
        {
            layouts.Add(this);
        }

        /// <summary>
        /// Returns a <see cref="T:System.String"></see> that represents the current <see cref="T:System.Object"></see>.
        /// </summary>
        /// <returns>
        /// A <see cref="T:System.String"></see> that represents the current <see cref="T:System.Object"></see>.
        /// </returns>
        public override string ToString()
        {
            return Text;
        }
    }
}
