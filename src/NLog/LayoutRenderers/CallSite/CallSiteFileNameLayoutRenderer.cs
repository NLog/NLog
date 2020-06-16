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

using System.ComponentModel;
using System.IO;
using System.Text;
using NLog.Config;
using NLog.Internal;

namespace NLog.LayoutRenderers
{
    /// <summary>
    /// The call site source file name. Full callsite <see cref="CallSiteLayoutRenderer"/>
    /// </summary>
    [LayoutRenderer("callsite-filename")]
    [ThreadAgnostic]
    [ThreadSafe]
    public class CallSiteFileNameLayoutRenderer : LayoutRenderer, IUsesStackTrace, IStringValueRenderer
    {
        /// <summary>
        /// Gets or sets a value indicating whether to include source file path.
        /// </summary>
        /// <docgen category='Rendering Options' order='10' />
        [DefaultValue(true)]
        public bool IncludeSourcePath { get; set; } = true;

        /// <summary>
        /// Gets or sets the number of frames to skip.
        /// </summary>
        /// <docgen category='Rendering Options' order='10' />
        [DefaultValue(0)]
        public int SkipFrames { get; set; }

        /// <summary>
        /// Logger should capture StackTrace, if it was not provided manually
        /// </summary>
        [DefaultValue(true)]
        public bool CaptureStackTrace { get; set; } = true;

        /// <summary>
        /// Gets the level of stack trace information required by the implementing class.
        /// </summary>
        StackTraceUsage IUsesStackTrace.StackTraceUsage => StackTraceUsageUtils.GetStackTraceUsage(true, SkipFrames, CaptureStackTrace);

        /// <inheritdoc/>
        protected override void Append(StringBuilder builder, LogEventInfo logEvent)
        {
            builder.Append(GetStringValue(logEvent));
        }

        /// <inheritdoc/>
        string IStringValueRenderer.GetFormattedString(LogEventInfo logEvent) => GetStringValue(logEvent);

        private string GetStringValue(LogEventInfo logEvent)
        {
            if (logEvent.CallSiteInformation != null)
            {
                string fileName = logEvent.CallSiteInformation.GetCallerFilePath(SkipFrames);
                if (!string.IsNullOrEmpty(fileName))
                {
                    return IncludeSourcePath ? fileName : Path.GetFileName(fileName);
                }
            }
            return string.Empty;
        }
    }
}
