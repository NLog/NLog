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

namespace NLog.Targets
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using NLog.Config;
    using NLog.Layouts;

    /// <summary>
    /// Sends log messages to the remote instance of Chainsaw / NLogViewer application using Log4J Xml
    /// </summary>
    /// <remarks>
    /// <a href="https://github.com/nlog/nlog/wiki/Chainsaw-target">See NLog Wiki</a>
    /// </remarks>
    /// <seealso href="https://github.com/nlog/nlog/wiki/Chainsaw-target">Documentation on NLog Wiki</seealso>
    /// <example>
    /// <p>
    /// To set up the target in the <a href="https://github.com/NLog/NLog/wiki/Configuration-file">configuration file</a>,
    /// use the following syntax:
    /// </p>
    /// <code lang="XML" source="examples/targets/Configuration File/Log4JXml/NLog.config" />
    /// <p>
    /// To set up the log target programmatically use code like this:
    /// </p>
    /// <code lang="C#" source="examples/targets/Configuration API/Log4JXml/Simple/Example.cs" />
    /// </example>
    [Target("Log4JXml")]
    [Target("Chainsaw")]
    [Target("NLogViewer")]
    public class Log4JXmlTarget : NetworkTarget
    {
        private readonly Log4JXmlEventLayout _log4JLayout = new Log4JXmlEventLayout();

        /// <summary>
        /// Initializes a new instance of the <see cref="Log4JXmlTarget" /> class.
        /// </summary>
        /// <remarks>
        /// The default value of the layout is: <code>${longdate}|${level:uppercase=true}|${logger}|${message:withexception=true}</code>
        /// </remarks>
        public Log4JXmlTarget()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Log4JXmlTarget" /> class.
        /// </summary>
        /// <remarks>
        /// The default value of the layout is: <code>${longdate}|${level:uppercase=true}|${logger}|${message:withexception=true}</code>
        /// </remarks>
        /// <param name="name">Name of the target.</param>
        public Log4JXmlTarget(string name) : this()
        {
            Name = name;
        }

        /// <summary>
        /// Gets or sets a value indicating whether to include NLog-specific extensions to log4j schema.
        /// </summary>
        /// <docgen category='Layout Options' order='10' />
        [Obsolete("Non standard extension to the Log4j-XML format. Marked obsolete with NLog v5.4")]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public bool IncludeNLogData
        {
            get => _log4JLayout.IncludeNLogData;
            set => _log4JLayout.IncludeNLogData = value;
        }

        /// <summary>
        /// Gets or sets the log4j:event logger-xml-attribute. Default: ${logger}
        /// </summary>
        /// <docgen category='Layout Options' order='10' />
        public Layout LoggerName
        {
            get => _log4JLayout.LoggerName;
            set => _log4JLayout.LoggerName = value;
        }

        /// <summary>
        /// Gets or sets the log4j:event message-xml-element. Default: ${message}
        /// </summary>
        /// <docgen category='Layout Options' order='10' />
        public Layout FormattedMessage
        {
            get => _log4JLayout.FormattedMessage;
            set => _log4JLayout.FormattedMessage = value;
        }

        /// <summary>
        /// Gets or sets the log4j:event log4japp-xml-element. By default it's the friendly name of the current AppDomain.
        /// </summary>
        /// <docgen category='Layout Options' order='10' />
        public Layout AppInfo
        {
            get => _log4JLayout.AppInfo;
            set => _log4JLayout.AppInfo = value;
        }

        /// <summary>
        /// Gets or sets a value indicating whether to include call site (class and method name) in the information sent over the network.
        /// </summary>
        /// <docgen category='Layout Options' order='10' />
        public bool IncludeCallSite
        {
            get => _log4JLayout.IncludeCallSite;
            set => _log4JLayout.IncludeCallSite = value;
        }

        /// <summary>
        /// Gets or sets a value indicating whether to include source info (file name and line number) in the information sent over the network.
        /// </summary>
        /// <docgen category='Layout Options' order='10' />
        public bool IncludeSourceInfo
        {
            get => _log4JLayout.IncludeSourceInfo;
            set => _log4JLayout.IncludeSourceInfo = value;
        }

        /// <summary>
        /// Obsolete and replaced by <see cref="IncludeScopeProperties"/> with NLog v5.
        /// Gets or sets a value indicating whether to include <see cref="MappedDiagnosticsContext"/> dictionary contents.
        /// </summary>
        /// <docgen category='Layout Options' order='10' />
        [Obsolete("Replaced by IncludeScopeProperties. Marked obsolete on NLog 5.0")]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public bool IncludeMdc
        {
            get => _log4JLayout.IncludeMdc;
            set => _log4JLayout.IncludeMdc = value;
        }

        /// <summary>
        /// Gets or sets whether to include log4j:NDC in output from <see cref="ScopeContext"/> nested context.
        /// </summary>
        /// <docgen category='Layout Options' order='10' />
        public bool IncludeNdc
        {
            get => _log4JLayout.IncludeNdc;
            set => _log4JLayout.IncludeNdc = value;
        }

        /// <summary>
        /// Gets or sets the option to include all properties from the log events
        /// </summary>
        /// <docgen category='Layout Options' order='10' />
        public bool IncludeEventProperties { get => _log4JLayout.IncludeEventProperties; set => _log4JLayout.IncludeEventProperties = value; }

        /// <summary>
        /// Gets or sets whether to include the contents of the <see cref="ScopeContext"/> properties-dictionary.
        /// </summary>
        /// <docgen category='Layout Options' order='10' />
        public bool IncludeScopeProperties { get => _log4JLayout.IncludeScopeProperties; set => _log4JLayout.IncludeScopeProperties = value; }

        /// <summary>
        /// Gets or sets whether to include log4j:NDC in output from <see cref="ScopeContext"/> nested context.
        /// </summary>
        /// <docgen category='Layout Options' order='10' />
        public bool IncludeScopeNested { get => _log4JLayout.IncludeScopeNested; set => _log4JLayout.IncludeScopeNested = value; }

        /// <summary>
        /// Gets or sets the separator for <see cref="ScopeContext"/> operation-states-stack.
        /// </summary>
        /// <docgen category='Layout Options' order='10' />
        public string ScopeNestedSeparator { get => _log4JLayout.ScopeNestedSeparator; set => _log4JLayout.ScopeNestedSeparator = value; }

        /// <summary>
        /// Obsolete and replaced by <see cref="IncludeEventProperties"/> with NLog v5.
        /// Gets or sets the option to include all properties from the log events
        /// </summary>
        /// <docgen category='Layout Options' order='10' />
        [Obsolete("Replaced by IncludeEventProperties. Marked obsolete on NLog 5.0")]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public bool IncludeAllProperties { get => IncludeEventProperties; set => IncludeEventProperties = value; }

        /// <summary>
        /// Obsolete and replaced by <see cref="IncludeScopeProperties"/> with NLog v5.
        /// Gets or sets a value indicating whether to include <see cref="MappedDiagnosticsLogicalContext"/> dictionary contents.
        /// </summary>
        /// <docgen category='Layout Options' order='10' />
        [Obsolete("Replaced by IncludeScopeProperties. Marked obsolete on NLog 5.0")]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public bool IncludeMdlc { get => _log4JLayout.IncludeMdlc; set => _log4JLayout.IncludeMdlc = value; }

        /// <summary>
        /// Obsolete and replaced by <see cref="IncludeNdc"/> with NLog v5.
        /// Gets or sets a value indicating whether to include contents of the <see cref="NestedDiagnosticsLogicalContext"/> stack.
        /// </summary>
        /// <docgen category='Layout Options' order='10' />
        [Obsolete("Replaced by IncludeNdc. Marked obsolete on NLog 5.0")]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public bool IncludeNdlc { get => _log4JLayout.IncludeNdlc; set => _log4JLayout.IncludeNdlc = value; }

        /// <summary>
        /// Obsolete and replaced by <see cref="NdcItemSeparator"/> with NLog v5.
        /// Gets or sets the stack separator for log4j:NDC in output from <see cref="ScopeContext"/> nested context.
        /// </summary>
        /// <docgen category='Layout Options' order='10' />
        [Obsolete("Replaced by NdcItemSeparator. Marked obsolete on NLog 5.0")]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public string NdlcItemSeparator { get => _log4JLayout.NdlcItemSeparator; set => _log4JLayout.NdlcItemSeparator = value; }

        /// <summary>
        /// Gets or sets the stack separator for log4j:NDC in output from <see cref="ScopeContext"/> nested context.
        /// </summary>
        /// <docgen category='Layout Options' order='10' />
        public string NdcItemSeparator
        {
            get => _log4JLayout.NdcItemSeparator;
            set => _log4JLayout.NdcItemSeparator = value;
        }

        /// <summary>
        /// Gets the collection of parameters. Each parameter contains a mapping
        /// between NLog layout and a named parameter.
        /// </summary>
        /// <docgen category='Layout Options' order='10' />
        [ArrayParameter(typeof(Log4JXmlEventParameter), "parameter")]
        public IList<Log4JXmlEventParameter> Parameters => _log4JLayout.Parameters;

        /// <summary>
        /// Gets or sets the instance of <see cref="Log4JXmlEventLayout"/> that is used to format log messages.
        /// </summary>
        /// <docgen category='Layout Options' order='1' />
        public override Layout Layout
        {
            get => _log4JLayout;
            set { /* Fixed Log4JXmlEventLayout */ } // NOSONAR
        }
    }
}
