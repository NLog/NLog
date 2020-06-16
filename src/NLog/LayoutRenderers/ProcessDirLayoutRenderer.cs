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

#if !NETSTANDARD1_3

namespace NLog.LayoutRenderers
{
    using System;
    using System.IO;
    using System.Text;
    using NLog.Config;
    using NLog.Internal;
    using NLog.Internal.Fakeables;

    /// <summary>
    /// The executable directory from the <see cref="System.Diagnostics.Process.MainModule"/> FileName,
    /// using the current process <see cref="System.Diagnostics.Process.GetCurrentProcess()"/>
    /// </summary>
    [LayoutRenderer("processdir")]
    [AppDomainFixedOutput]
    [ThreadAgnostic]
    [ThreadSafe]
    public class ProcessDirLayoutRenderer : LayoutRenderer
    {
        private readonly string _processDir;

        /// <summary>
        /// Gets or sets the name of the file to be Path.Combine()'d with with the process directory.
        /// </summary>
        /// <docgen category='Advanced Options' order='10' />
        public string File { get; set; }

        /// <summary>
        /// Gets or sets the name of the directory to be Path.Combine()'d with with the process directory.
        /// </summary>
        /// <docgen category='Advanced Options' order='10' />
        public string Dir { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ProcessDirLayoutRenderer" /> class.
        /// </summary>
        public ProcessDirLayoutRenderer()
            : this(LogFactory.DefaultAppEnvironment)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ProcessDirLayoutRenderer" /> class.
        /// </summary>
        internal ProcessDirLayoutRenderer(IAppEnvironment appEnvironment)
        {
            _processDir = Path.GetDirectoryName(appEnvironment.CurrentProcessFilePath);
        }

        /// <inheritdoc/>
        protected override void Append(StringBuilder builder, LogEventInfo logEvent)
        {
            var path = PathHelpers.CombinePaths(_processDir, Dir, File);
            builder.Append(path);
        }
    }
}

#endif