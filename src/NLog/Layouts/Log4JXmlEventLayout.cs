// 
// Copyright (c) 2004-2016 Jaroslaw Kowalski <jaak@jkowalski.net>, Kim Christensen, Julian Verdurmen
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
    using NLog.LayoutRenderers;

    /// <summary>
    /// A specialized layout that renders Log4j-compatible XML events.
    /// </summary>
    /// <remarks>
    /// This layout is not meant to be used explicitly. Instead you can use ${log4jxmlevent} layout renderer.
    /// </remarks>
    [Layout("Log4JXmlEventLayout")]
    public class Log4JXmlEventLayout : Layout
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Log4JXmlEventLayout" /> class.
        /// </summary>
        public Log4JXmlEventLayout()
        {
            this.Renderer = new Log4JXmlEventLayoutRenderer();
        }

        /// <summary>
        /// Gets the <see cref="Log4JXmlEventLayoutRenderer"/> instance that renders log events.
        /// </summary>
        public Log4JXmlEventLayoutRenderer Renderer { get; private set; }

        /// <summary>
        /// Gets or sets a value indicating whether to include contents of the <see cref="MappedDiagnosticsContext"/> dictionary.
        /// </summary>
        /// <docgen category='Payload Options' order='10' />
        public bool IncludeMdc { get { return Renderer.IncludeMdc; } set { Renderer.IncludeMdc = value; } }

#if NET4_0 || NET4_5
        /// <summary>
        /// Gets or sets a value indicating whether to include contents of the <see cref="MappedDiagnosticsLogicalContext"/> dictionary.
        /// </summary>
        /// <docgen category='Payload Options' order='10' />
        public bool IncludeMdlc { get { return Renderer.IncludeMdlc; } set { Renderer.IncludeMdlc = value; } }
#endif

        /// <summary>
        /// Gets or sets the option to include all properties from the log events
        /// </summary>
        /// <docgen category='Payload Options' order='10' />
        public bool IncludeAllProperties { get { return Renderer.IncludeAllProperties; } set { Renderer.IncludeAllProperties = value; } }

        /// <summary>
        /// Renders the layout for the specified logging event by invoking layout renderers.
        /// </summary>
        /// <param name="logEvent">The logging event.</param>
        /// <returns>The rendered layout.</returns>
        protected override string GetFormattedMessage(LogEventInfo logEvent)
        {
            return RenderAllocateBuilder(logEvent);
        }

        /// <summary>
        /// Renders the layout for the specified logging event by invoking layout renderers.
        /// </summary>
        /// <param name="logEvent">The logging event.</param>
        /// <param name="target">Initially empty <see cref="System.Text.StringBuilder"/> for the result</param>
        protected override void RenderFormattedMessage(LogEventInfo logEvent, System.Text.StringBuilder target)
        {
            this.Renderer.RenderAppendBuilder(logEvent, target);
        }
    }
}