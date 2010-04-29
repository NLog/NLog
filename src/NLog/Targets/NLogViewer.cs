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
using System.IO;
using System.Text;
using System.Xml;
using System.Reflection;
using System.Diagnostics;

using NLog.Internal;
using System.Net;
using System.Net.Sockets;

using NLog.Config;
using NLog.LayoutRenderers;
using NLog.Layouts;

namespace NLog.Targets
{
    /// <summary>
    /// Sends logging messages to the remote instance of NLog Viewer. 
    /// </summary>
    /// <example>
    /// <p>
    /// To set up the target in the <a href="config.html">configuration file</a>, 
    /// use the following syntax:
    /// </p>
    /// <code lang="XML" src="examples/targets/Configuration File/NLogViewer/NLog.config" />
    /// <p>
    /// This assumes just one target and a single rule. More configuration
    /// options are described <a href="config.html">here</a>.
    /// </p>
    /// <p>
    /// To set up the log target programmatically use code like this:
    /// </p>
    /// <code lang="C#" src="examples/targets/Configuration API/NLogViewer/Simple/Example.cs" />
    /// <p>
    /// NOTE: If your receiver application is ever likely to be off-line, don't use TCP protocol
    /// or you'll get TCP timeouts and your application will crawl. 
    /// Either switch to UDP transport or use <a href="target.AsyncWrapper.html">AsyncWrapper</a> target
    /// so that your application threads will not be blocked by the timing-out connection attempts.
    /// </p>
    /// </example>
    [Target("NLogViewer", IgnoresLayout=true)]
    public class NLogViewerTarget: NetworkTarget
    {
        private NLogViewerParameterInfoCollection _parameters = new NLogViewerParameterInfoCollection();

        private Log4JXmlEventLayoutRenderer Renderer
        {
            get { return Layout.Renderer; }
        }

        /// <summary>
        /// An instance of <see cref="Log4JXmlEventLayout"/> that is used to format log messages.
        /// </summary>
        protected new Log4JXmlEventLayout Layout
        {
            get { return base.CompiledLayout as Log4JXmlEventLayout; }
            set { CompiledLayout = value; }
        }

        /// <summary>
        /// Include NLog-specific extensions to log4j schema.
        /// </summary>
        public bool IncludeNLogData
        {
            get { return Renderer.IncludeNLogData; }
            set { Renderer.IncludeNLogData = value;  }
        }

        /// <summary>
        /// Creates a new instance of the <see cref="NLogViewerTarget"/> 
        /// and initializes default property values.
        /// </summary>
        public NLogViewerTarget()
        {
            CompiledLayout = new Log4JXmlEventLayout();
            Renderer.Parameters = _parameters;
            NewLine = false;
        }

        /// <summary>
        /// The AppInfo field. By default it's the friendly name of the current AppDomain.
        /// </summary>
        public string AppInfo
        {
            get { return Renderer.AppInfo; }
            set { Renderer.AppInfo = value; }
        }

#if !NETCF
        /// <summary>
        /// Include call site (class and method name) in the information sent over the network.
        /// </summary>
        public bool IncludeCallSite
        {
            get { return Renderer.IncludeCallSite; }
            set { Renderer.IncludeCallSite = value; }
        }

        /// <summary>
        /// Include source info (file name and line number) in the information sent over the network.
        /// </summary>
        public bool IncludeSourceInfo
        {
            get { return Renderer.IncludeSourceInfo; }
            set { Renderer.IncludeSourceInfo = value; }
        }

        /// <summary>
        /// Adds all layouts used by this target to the specified collection.
        /// </summary>
        /// <param name="layouts">The collection to add layouts to.</param>
        public override void PopulateLayouts(LayoutCollection layouts)
        {
            base.PopulateLayouts (layouts);
            for (int i = 0; i < Parameters.Count; ++i)
                Parameters[i].CompiledLayout.PopulateLayouts(layouts);
        }

        /// <summary>
        /// Returns the value indicating whether call site and/or source information should be gathered.
        /// </summary>
        /// <returns>2 - when IncludeSourceInfo is set, 1 when IncludeCallSite is set, 0 otherwise</returns>
        protected internal override int NeedsStackTrace()
        {
            if (IncludeSourceInfo)
                return 2;
            if (IncludeCallSite)
                return 1;

            return base.NeedsStackTrace();
        }
#endif

        /// <summary>
        /// Include MDC dictionary in the information sent over the network.
        /// </summary>
        public bool IncludeMDC
        {
            get { return Renderer.IncludeMDC; }
            set { Renderer.IncludeMDC = value; }
        }

        /// <summary>
        /// Include NDC stack.
        /// </summary>
        public bool IncludeNDC
        {
            get { return Renderer.IncludeNDC; }
            set { Renderer.IncludeNDC = value; }
        }
        /// <summary>
        /// The collection of parameters. Each parameter contains a mapping
        /// between NLog layout and a named parameter.
        /// </summary>
        [ArrayParameter(typeof(NLogViewerParameterInfo), "parameter")]
        public NLogViewerParameterInfoCollection Parameters
        {
            get { return _parameters; }
        }
    }
}
