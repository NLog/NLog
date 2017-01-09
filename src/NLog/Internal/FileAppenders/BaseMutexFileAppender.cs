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

namespace NLog.Internal.FileAppenders
{
    using System;
    using System.IO;
    using System.Security;
    using System.Text;
#if !SILVERLIGHT
    using System.Security.Cryptography;
#endif

    /// <summary>
    /// Base class for optimized file appenders which require the usage of a mutex. 
    /// 
    /// It is possible to use this class as replacement of BaseFileAppender and the mutex functionality 
    /// is not enforced to the implementing subclasses.
    /// </summary>
    [SecuritySafeCritical]
    internal abstract class BaseMutexFileAppender : BaseFileAppender
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="BaseMutexFileAppender" /> class.
        /// </summary>
        /// <param name="fileName">Name of the file.</param>
        /// <param name="createParameters">The create parameters.</param>
        public BaseMutexFileAppender(string fileName, ICreateFileParameters createParameters)
            : base(fileName, createParameters)
        {
            ArchiveMutex = CreateArchiveMutex();
        }

        /// <summary>
        /// Gets the mutually-exclusive lock for archiving files.
        /// </summary>
        /// <value>The mutex for archiving.</value>
        public PortableNamedMutex ArchiveMutex { get; private set; }

        private PortableNamedMutex CreateArchiveMutex()
        {
            string filePath = CreateFileParameters.CurrentMutexFilePath;
            if (filePath == string.Empty)
                filePath = Path.Combine(Path.GetDirectoryName(FileName), "NLogLock");
            return new PortableNamedMutex(GetMutexName("FileArchiveLock"), filePath);
        }

        /// <summary>
        /// Releases unmanaged and - optionally - managed resources.
        /// </summary>
        /// <param name="disposing">True to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            if (disposing)
            {
                if (ArchiveMutex != null)
                    ArchiveMutex.Dispose();    // Only closed on dispose, Mutex must survieve, when closing FileAppender before archive
            }
        }

        protected string GetMutexName(string mutexNamePrefix)
        {
            const string mutexNameFormatString = @"Global\NLog-File{0}-{1}";
            const int maxMutexNameLength = 260;

            string canonicalName = Path.GetFullPath(FileName).ToLowerInvariant();

            // Mutex names must not contain a backslash, it's the namespace separator,
            // but all other are OK
            canonicalName = canonicalName.Replace('\\', '/');

            string mutexName = string.Format(mutexNameFormatString, mutexNamePrefix, canonicalName);

            // A mutex name must not exceed MAX_PATH (260) characters
            if (mutexName.Length <= maxMutexNameLength)
            {
                return mutexName;
            }

            string hash;
#if !SILVERLIGHT
            // The unusual case of the path being too long; let's hash the canonical name,
            // so it can be safely shortened and still remain unique
            using (MD5 md5 = MD5.Create())
            {
                byte[] bytes = md5.ComputeHash(Encoding.UTF8.GetBytes(canonicalName));
                hash = Convert.ToBase64String(bytes);
            }
#else
            hash = canonicalName.GetHashCode().ToString();
#endif

            // The hash makes the name unique, but also add the end of the path,
            // so the end of the name tells us which file it is (for debugging)
            mutexName = string.Format(mutexNameFormatString, mutexNamePrefix, hash);
            int cutOffIndex = canonicalName.Length - (maxMutexNameLength - mutexName.Length);
            return mutexName + canonicalName.Substring(cutOffIndex);
        }
    }
}
