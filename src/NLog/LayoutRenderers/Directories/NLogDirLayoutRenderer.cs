// 
// Copyright (c) 2004-2021 Jaroslaw Kowalski <jaak@jkowalski.net>, Kim Christensen, Julian Verdurmen
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

    /// <summary>
    /// The directory where NLog.dll is located.
    /// </summary>
    /// <remarks>
    /// <a href="https://github.com/NLog/NLog/wiki/NLogDir-Layout-Renderer">See NLog Wiki</a>
    /// </remarks>
    /// <seealso href="https://github.com/NLog/NLog/wiki/NLogDir-Layout-Renderer">Documentation on NLog Wiki</seealso>
    [LayoutRenderer("nlogdir")]
    [AppDomainFixedOutput]
    [ThreadAgnostic]
    public class NLogDirLayoutRenderer : LayoutRenderer
    {
        /// <summary>
        /// Gets or sets the name of the file to be Path.Combine()'d with the directory name.
        /// </summary>
        /// <docgen category='Advanced Options' order='50' />
        public string File { get; set; }

        /// <summary>
        /// Gets or sets the name of the directory to be Path.Combine()'d with the directory name.
        /// </summary>
        /// <docgen category='Advanced Options' order='50' />
        public string Dir { get; set; }

        private static string NLogDir => _nlogDir ?? (_nlogDir = ResolveNLogDir());
        private static string _nlogDir;
        private string _nlogCombinedPath;

        /// <inheritdoc/>
        protected override void InitializeLayoutRenderer()
        {
            _nlogCombinedPath = null;
            base.InitializeLayoutRenderer();
        }

        /// <inheritdoc/>
        protected override void CloseLayoutRenderer()
        {
            _nlogCombinedPath = null;
            base.CloseLayoutRenderer();
        }

        /// <inheritdoc/>
        protected override void Append(StringBuilder builder, LogEventInfo logEvent)
        {
            var path = _nlogCombinedPath ?? (_nlogCombinedPath = PathHelpers.CombinePaths(NLogDir, Dir, File));
            builder.Append(path);
        }

        private static string ResolveNLogDir()
        {
            var nlogAssembly = typeof(LogFactory).GetAssembly();
            if (!string.IsNullOrEmpty(nlogAssembly.Location))
            {
                return Path.GetDirectoryName(nlogAssembly.Location);
            }
            else
            {
#pragma warning disable CS0618 // Type or member is obsolete
                return AssemblyHelpers.GetAssemblyFileLocation(nlogAssembly) ?? string.Empty;
#pragma warning restore CS0618 // Type or member is obsolete
            }
        }
    }
}

#endif