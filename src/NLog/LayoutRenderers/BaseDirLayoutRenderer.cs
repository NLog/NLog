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

namespace NLog.LayoutRenderers
{
    using System;
    using System.IO;
    using System.Text;
    using NLog.Config;
    using NLog.Internal;
    using NLog.Internal.Fakeables;

    /// <summary>
    /// The current application domain's base directory.
    /// </summary>
    /// <remarks>
    /// <a href="https://github.com/NLog/NLog/wiki/Basedir-Layout-Renderer">See NLog Wiki</a>
    /// </remarks>
    /// <seealso href="https://github.com/NLog/NLog/wiki/Basedir-Layout-Renderer">Documentation on NLog Wiki</seealso>
    [LayoutRenderer("basedir")]
    [AppDomainFixedOutput]
    [ThreadAgnostic]
    public class BaseDirLayoutRenderer : LayoutRenderer
    {
        private readonly string _appDomainDirectory;
        private string _baseDir;

        private readonly IAppEnvironment _appEnvironment;

        /// <summary>
        /// Use base dir of current process. Alternative one can just use ${processdir}
        /// </summary>
        /// <docgen category='Layout Options' order='10' />
        public bool ProcessDir { get; set; }

        /// <summary>
        /// Fallback to the base dir of current process, when AppDomain.BaseDirectory is Temp-Path (.NET Core 3 - Single File Publish)
        /// </summary>
        /// <docgen category='Layout Options' order='10' />
        public bool FixTempDir { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="BaseDirLayoutRenderer" /> class.
        /// </summary>
        public BaseDirLayoutRenderer() : this(LogFactory.DefaultAppEnvironment)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BaseDirLayoutRenderer" /> class.
        /// </summary>
        internal BaseDirLayoutRenderer(IAppEnvironment appEnvironment)
        {
            _baseDir = _appDomainDirectory = appEnvironment.AppDomainBaseDirectory;
            _appEnvironment = appEnvironment;
        }

        /// <summary>
        /// Gets or sets the name of the file to be Path.Combine()'d with the base directory.
        /// </summary>
        /// <docgen category='Advanced Options' order='50' />
        public string File { get; set; }

        /// <summary>
        /// Gets or sets the name of the directory to be Path.Combine()'d with the base directory.
        /// </summary>
        /// <docgen category='Advanced Options' order='50' />
        public string Dir { get; set; }

        /// <inheritdoc/>
        protected override void InitializeLayoutRenderer()
        {
            base.InitializeLayoutRenderer();

            _baseDir = _appDomainDirectory;

            if (ProcessDir)
            {
                var processDir = GetProcessDir();
                if (!string.IsNullOrEmpty(processDir))
                    _baseDir = processDir;
            }
            else if (FixTempDir)
            {
                var fixTempDir = GetFixedTempBaseDir(_appDomainDirectory);
                if (!string.IsNullOrEmpty(fixTempDir))
                    _baseDir = fixTempDir;
            }
            _baseDir = AppEnvironmentWrapper.FixFilePathWithLongUNC(_baseDir);
            _baseDir = PathHelpers.CombinePaths(_baseDir, Dir, File);
        }

        /// <inheritdoc/>
        protected override void Append(StringBuilder builder, LogEventInfo logEvent)
        {
            builder.Append(_baseDir);
        }

        private string GetFixedTempBaseDir(string baseDir)
        {
            try
            {
                var tempDir = _appEnvironment.UserTempFilePath;
                if (PathHelpers.IsTempDir(baseDir, tempDir))
                {
                    var processDir = GetProcessDir();
                    if (!string.IsNullOrEmpty(processDir) && !PathHelpers.IsTempDir(processDir, tempDir))
                    {
                        return processDir;
                    }
                }
                return baseDir;
            }
            catch (Exception ex)
            {
                Common.InternalLogger.Warn(ex, "BaseDir LayoutRenderer unexpected exception");
                return baseDir;
            }
        }

        private string GetProcessDir()
        {
            return Path.GetDirectoryName(_appEnvironment.CurrentProcessFilePath);
        }
    }
}

