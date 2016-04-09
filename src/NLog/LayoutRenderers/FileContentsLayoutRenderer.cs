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

namespace NLog.LayoutRenderers
{
    using System;
    using System.IO;
    using System.Text;
    using NLog.Common;
    using NLog.Config;
    using NLog.Internal;
    using NLog.Layouts;

    /// <summary>
    /// Renders contents of the specified file.
    /// </summary>
    [LayoutRenderer("file-contents")]
    public class FileContentsLayoutRenderer : LayoutRenderer
    {
        private string lastFileName;
        private string currentFileContents;

        /// <summary>
        /// Initializes a new instance of the <see cref="FileContentsLayoutRenderer" /> class.
        /// </summary>
        public FileContentsLayoutRenderer()
        {
#if SILVERLIGHT
            this.Encoding = Encoding.UTF8;
#else
            this.Encoding = Encoding.Default;
#endif
            this.lastFileName = string.Empty;
        }

        /// <summary>
        /// Gets or sets the name of the file.
        /// </summary>
        /// <docgen category='File Options' order='10' />
        [DefaultParameter]
        public Layout FileName { get; set; }

        /// <summary>
        /// Gets or sets the encoding used in the file.
        /// </summary>
        /// <value>The encoding.</value>
        /// <docgen category='File Options' order='10' />
        public Encoding Encoding { get; set; }

        /// <summary>
        /// Renders the contents of the specified file and appends it to the specified <see cref="StringBuilder" />.
        /// </summary>
        /// <param name="builder">The <see cref="StringBuilder"/> to append the rendered data to.</param>
        /// <param name="logEvent">Logging event.</param>
        protected override void Append(StringBuilder builder, LogEventInfo logEvent)
        {
            lock (this)
            {
                string fileName = this.FileName.Render(logEvent);

                if (fileName != this.lastFileName)
                {
                    this.currentFileContents = this.ReadFileContents(fileName);
                    this.lastFileName = fileName;
                }
            }

            builder.Append(this.currentFileContents);
        }

        private string ReadFileContents(string fileName)
        {
            try
            {
                using (var reader = new StreamReader(fileName, this.Encoding))
                {
                    return reader.ReadToEnd();
                }
            }
            catch (Exception exception)
            {
                InternalLogger.Error(exception, "Cannot read file contents of '{0}'.", fileName);

                if (exception.MustBeRethrown())
                {
                    throw;
                }
               
                return string.Empty;
            }
        }
    }
}
