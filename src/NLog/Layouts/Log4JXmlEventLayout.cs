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

namespace NLog.Layouts
{
    using System;
    using System.Collections.Generic;
    using System.Text;
    using NLog.Config;
    using NLog.LayoutRenderers;
    using NLog.Targets;

    /// <summary>
    /// A specialized layout that renders Log4j-compatible XML events.
    /// </summary>
    /// <remarks>
    /// This layout is not meant to be used explicitly. Instead you can use ${log4jxmlevent} layout renderer.
    /// </remarks>
    [Layout("Log4JXmlEventLayout")]
    [ThreadAgnostic]
    [ThreadSafe]
    [AppDomainFixedOutput]
    public class Log4JXmlEventLayout : Layout, IIncludeContext
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Log4JXmlEventLayout" /> class.
        /// </summary>
        public Log4JXmlEventLayout()
        {
            Renderer = new Log4JXmlEventLayoutRenderer();
            Parameters = new List<NLogViewerParameterInfo>();
            Renderer.Parameters = Parameters;
        }

        /// <summary>
        /// Gets the <see cref="Log4JXmlEventLayoutRenderer"/> instance that renders log events.
        /// </summary>
        public Log4JXmlEventLayoutRenderer Renderer { get; }

        /// <summary>
        /// Gets the collection of parameters. Each parameter contains a mapping
        /// between NLog layout and a named parameter.
        /// </summary>
        /// <docgen category='Payload Options' order='10' />
        [ArrayParameter(typeof(NLogViewerParameterInfo), "parameter")]
        public IList<NLogViewerParameterInfo> Parameters { get => Renderer.Parameters; set => Renderer.Parameters = value;  }

        /// <summary>
        /// Gets or sets the option to include all properties from the log events
        /// </summary>
        /// <docgen category='Payload Options' order='10' />
        public bool IncludeEventProperties
        {
            get => Renderer.IncludeEventProperties;
            set => Renderer.IncludeEventProperties = value;
        }

        /// <summary>
        /// Gets or sets whether to include the contents of the <see cref="ScopeContext"/> properties-dictionary.
        /// </summary>
        /// <docgen category='Payload Options' order='10' />
        public bool IncludeScopeProperties
        {
            get => Renderer.IncludeScopeProperties;
            set => Renderer.IncludeScopeProperties = value;
        }

        /// <summary>
        /// Gets or sets whether to include the contents of the <see cref="ScopeContext"/> operation-call-stack.
        /// </summary>
        /// <docgen category='Payload Options' order='10' />
        public bool IncludeScopeNestedStates
        {
            get => Renderer.IncludeScopeNestedStates;
            set => Renderer.IncludeScopeNestedStates = value;
        }

        /// <summary>
        /// Gets or sets the option to include all properties from the log events
        /// </summary>
        /// <docgen category='Payload Options' order='10' />
        [Obsolete("Replaced by IncludeEventProperties. Marked obsolete on NLog 5.0")]
        public bool IncludeAllProperties { get => IncludeEventProperties; set => IncludeEventProperties = value; }

        /// <summary>
        /// Gets or sets a value indicating whether to include contents of the <see cref="MappedDiagnosticsContext"/> dictionary.
        /// </summary>
        /// <docgen category='Payload Options' order='10' />
        [Obsolete("Replaced by IncludeScopeProperties. Marked obsolete on NLog 5.0")]
        public bool IncludeMdc { get => Renderer.IncludeMdc; set => Renderer.IncludeMdc = value; }

        /// <summary>
        /// Gets or sets a value indicating whether to include contents of the <see cref="NestedDiagnosticsContext"/> stack.
        /// </summary>
        /// <docgen category='Payload Options' order='10' />
        [Obsolete("Replaced by IncludeScopeNestedStates. Marked obsolete on NLog 5.0")]
        public bool IncludeNdc { get => Renderer.IncludeNdc; set => Renderer.IncludeNdc = value; }

        /// <summary>
        /// Gets or sets a value indicating whether to include contents of the <see cref="MappedDiagnosticsLogicalContext"/> dictionary.
        /// </summary>
        /// <docgen category='Payload Options' order='10' />
        [Obsolete("Replaced by IncludeScopeProperties. Marked obsolete on NLog 5.0")]
        public bool IncludeMdlc { get => Renderer.IncludeMdlc; set => Renderer.IncludeMdlc = value; }

        /// <summary>
        /// Gets or sets a value indicating whether to include contents of the <see cref="NestedDiagnosticsLogicalContext"/> stack.
        /// </summary>
        /// <docgen category='Payload Options' order='10' />
        [Obsolete("Replaced by IncludeScopeNestedStates. Marked obsolete on NLog 5.0")]
        public bool IncludeNdlc { get => Renderer.IncludeNdlc; set => Renderer.IncludeNdlc = value; }

        /// <summary>
        /// Gets or sets the log4j:event logger-xml-attribute (Default ${logger})
        /// </summary>
        /// <docgen category='Payload Options' order='10' />
        public Layout LoggerName
        {
            get => Renderer.LoggerName;
            set => Renderer.LoggerName = value;
        }

        /// <summary>
        /// Gets or sets the AppInfo field. By default it's the friendly name of the current AppDomain.
        /// </summary>
        /// <docgen category='Payload Options' order='10' />
        public Layout AppInfo
        {
            get => Renderer.AppInfo;
            set => Renderer.AppInfo = value;
        }

        /// <summary>
        ///  Gets or sets whether the log4j:throwable xml-element should be written as CDATA
        /// </summary>
        /// <docgen category='Payload Options' order='10' />
        public bool WriteThrowableCData
        {
            get => Renderer.WriteThrowableCData;
            set => Renderer.WriteThrowableCData = value;
        }

        /// <summary>
        /// Gets or sets a value indicating whether to include call site (class and method name) in the information sent over the network.
        /// </summary>
        /// <docgen category='Payload Options' order='10' />
        public bool IncludeCallSite
        {
            get => Renderer.IncludeCallSite;
            set => Renderer.IncludeCallSite = value;
        }

        /// <summary>
        /// Gets or sets a value indicating whether to include source info (file name and line number) in the information sent over the network.
        /// </summary>
        /// <docgen category='Payload Options' order='10' />
        public bool IncludeSourceInfo
        {
            get => Renderer.IncludeSourceInfo;
            set => Renderer.IncludeSourceInfo = value;
        }

        internal override void PrecalculateBuilder(LogEventInfo logEvent, StringBuilder target)
        {
            PrecalculateBuilderInternal(logEvent, target);
        }

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
        /// <param name="target"><see cref="StringBuilder"/> for the result</param>
        protected override void RenderFormattedMessage(LogEventInfo logEvent, StringBuilder target)
        {
            Renderer.RenderAppendBuilder(logEvent, target);
        }
    }
}