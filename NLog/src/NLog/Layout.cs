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

namespace NLog
{
    /// <summary>
    /// Represents a string with embedded placeholders that can render contextual information.
    /// </summary>
    public class Layout
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

        private string _layoutText;
        private LayoutRenderer[] _renderers;
        private int _needsStackTrace = 0;
        private bool _isVolatile = false;
        private StringBuilder builder = new StringBuilder();

        /// <summary>
        /// The layout text
        /// </summary>
        public string Text
        {
            get
            {
                return _layoutText;
            }
            set
            {
                _layoutText = value;
                _renderers = CompileLayout(_layoutText, out _needsStackTrace, out _isVolatile);
            }
        }

        /// <summary>
        /// Renders the layout for the specified logging event by invoking layout renderers
        /// that make up the event.
        /// </summary>
        /// <param name="logEvent">The logging event.</param>
        /// <returns>The rendered layout.</returns>
        public string GetFormattedMessage(LogEventInfo logEvent)
        {
            if (_renderers.Length == 1 && _renderers[0] is LiteralLayoutRenderer)
            {
                return ((LiteralLayoutRenderer)(_renderers[0])).Text;
            }

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

        private static LayoutRenderer[] CompileLayout(string s, out int needsStackTrace, out bool isVolatile)
        {
            ArrayList result = new ArrayList();
            needsStackTrace = 0;
            isVolatile = false;

            int startingPos = 0;
            int pos = s.IndexOf("${", startingPos);

            while (pos >= 0)
            {
                if (pos != startingPos)
                {
                    result.Add(new LiteralLayoutRenderer(s.Substring(startingPos, pos - startingPos)));
                }
                int pos2 = s.IndexOf("}", pos + 2);
                if (pos2 >= 0)
                {
                    startingPos = pos2 + 1;
                    string item = s.Substring(pos + 2, pos2 - pos - 2);
                    int paramPos = item.IndexOf(':');
                    string LayoutRenderer = item;
                    string LayoutRendererParams = null;
                    if (paramPos >= 0)
                    {
                        LayoutRendererParams = LayoutRenderer.Substring(paramPos + 1);
                        LayoutRenderer = LayoutRenderer.Substring(0, paramPos);
                    }

                    LayoutRenderer newLayoutRenderer = LayoutRendererFactory.CreateLayoutRenderer(LayoutRenderer, LayoutRendererParams);
                    int nst = newLayoutRenderer.NeedsStackTrace();
                    if (nst > needsStackTrace)
                        needsStackTrace = nst;
                    if (newLayoutRenderer.IsVolatile())
                        isVolatile = true;

                    result.Add(newLayoutRenderer);
                    pos = s.IndexOf("${", startingPos);
                }
                else
                {
                    break;
                }
            }
            if (startingPos != s.Length)
            {
                result.Add(new LiteralLayoutRenderer(s.Substring(startingPos, s.Length - startingPos)));
            }

            return (LayoutRenderer[])result.ToArray(typeof(LayoutRenderer));
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
    }
}
