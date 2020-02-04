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
    using System.Text;
    using NLog.Config;

    /// <summary>
    /// Render a Nested Diagnostic Context item.
    /// See <see cref="NestedDiagnosticsContext"/>
    /// </summary>
    [LayoutRenderer("ndc")]
    [ThreadSafe]
    public class NdcLayoutRenderer : LayoutRenderer
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="NdcLayoutRenderer" /> class.
        /// </summary>
        public NdcLayoutRenderer()
        {
            Separator = " ";
            BottomFrames = -1;
            TopFrames = -1;
        }

        /// <summary>
        /// Gets or sets the number of top stack frames to be rendered.
        /// </summary>
        /// <docgen category='Rendering Options' order='10' />
        public int TopFrames { get; set; }

        /// <summary>
        /// Gets or sets the number of bottom stack frames to be rendered.
        /// </summary>
        /// <docgen category='Rendering Options' order='10' />
        public int BottomFrames { get; set; }

        /// <summary>
        /// Gets or sets the separator to be used for concatenating nested diagnostics context output.
        /// </summary>
        /// <docgen category='Rendering Options' order='10' />
        public string Separator { get; set; }

        /// <summary>
        /// Renders the specified Nested Diagnostics Context item and appends it to the specified <see cref="StringBuilder" />.
        /// </summary>
        /// <param name="builder">The <see cref="StringBuilder"/> to append the rendered data to.</param>
        /// <param name="logEvent">Logging event.</param>
        protected override void Append(StringBuilder builder, LogEventInfo logEvent)
        {
            if (TopFrames == 1)
            {
                // Allows fast rendering of topframes=1
                var topFrame = NestedDiagnosticsContext.PeekObject();
                if (topFrame != null)
                    AppendAsString(topFrame, GetFormatProvider(logEvent), builder);
                return;
            }

            var messages = NestedDiagnosticsContext.GetAllObjects();
            if (messages.Length == 0)
                return;

            int startPos = 0;
            int endPos = messages.Length;

            if (TopFrames != -1)
            {
                endPos = Math.Min(TopFrames, messages.Length);
            }
            else if (BottomFrames != -1)
            {
                startPos = messages.Length - Math.Min(BottomFrames, messages.Length);
            }

            var formatProvider = GetFormatProvider(logEvent);
            string currentSeparator = string.Empty;
            for (int i = endPos - 1; i >= startPos; --i)
            {
                builder.Append(currentSeparator);
                AppendAsString(messages[i], formatProvider, builder);
                currentSeparator = Separator;
            }
        }

        private static void AppendAsString(object message, IFormatProvider formatProvider, StringBuilder builder)
        {
            string stringValue = Convert.ToString(message, formatProvider);
            builder.Append(stringValue);
        }
    }
}
