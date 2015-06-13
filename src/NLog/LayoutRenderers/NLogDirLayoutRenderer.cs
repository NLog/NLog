// 
// Copyright (c) 2004-2011 Jaroslaw Kowalski <jaak@jkowalski.net>
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

#if !SILVERLIGHT

namespace NLog.LayoutRenderers
{
    using System;
    using System.IO;
    using System.Text;

    using NLog.Config;

    /// <summary>
    /// The directory where NLog.dll is located.
    /// </summary>
    [LayoutRenderer("nlogdir")]
    [AppDomainFixedOutput]
    [ThreadAgnostic]
    public class NLogDirLayoutRenderer : LayoutRenderer
    {
        /// <summary>
        /// Initializes static members of the NLogDirLayoutRenderer class.
        /// </summary>
        static NLogDirLayoutRenderer()
        {
            var assembly = typeof(LogManager).Assembly;
            var location = !String.IsNullOrEmpty(assembly.Location)
                ? assembly.Location
                : new Uri(assembly.CodeBase).LocalPath;
            NLogDir = Path.GetDirectoryName(location);
        }

        /// <summary>
        /// Gets or sets the name of the file to be Path.Combine()'d with the directory name.
        /// </summary>
        /// <docgen category='Advanced Options' order='10' />
        public string File { get; set; }

        /// <summary>
        /// Gets or sets the name of the directory to be Path.Combine()'d with the directory name.
        /// </summary>
        /// <docgen category='Advanced Options' order='10' />
        public string Dir { get; set; }

        private static string NLogDir { get; set; }

        /// <summary>
        /// Renders the directory where NLog is located and appends it to the specified <see cref="StringBuilder" />.
        /// </summary>
        /// <param name="builder">The <see cref="StringBuilder"/> to append the rendered data to.</param>
        /// <param name="logEvent">Logging event.</param>
        protected override void Append(StringBuilder builder, LogEventInfo logEvent)
        {
            string baseDir = NLogDir;

            if (this.File != null)
            {
                builder.Append(Path.Combine(baseDir, this.File));
            }
            else if (this.Dir != null)
            {
                builder.Append(Path.Combine(baseDir, this.Dir));
            }
            else
            {
                builder.Append(baseDir);
            }
        }
    }
}

#endif