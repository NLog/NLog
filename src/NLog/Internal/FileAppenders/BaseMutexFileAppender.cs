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
#define SupportsMutex
#endif

namespace NLog.Internal.FileAppenders
{
    using System;
    using System.IO;
    using System.Security;
    using System.Threading;
    using System.Text;
    using JetBrains.Annotations;

#if SupportsMutex
#if !NETSTANDARD
    using System.Security.AccessControl;
    using System.Security.Principal;
#endif
    using System.Security.Cryptography;
#endif
    using Common;

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
        protected BaseMutexFileAppender(string fileName, ICreateFileParameters createParameters)
            : base(fileName, createParameters)
        {
            if (createParameters.IsArchivingEnabled && createParameters.ConcurrentWrites)
            {
                if (MutexDetector.SupportsSharableMutex)
                {
#if SupportsMutex
                    ArchiveMutex = CreateArchiveMutex();
#endif
                }
                else
                {
                    InternalLogger.Debug("{0}: Mutex for file archive not supported", CreateFileParameters);
                }
            }
        }

#if SupportsMutex
        /// <summary>
        /// Gets the mutually-exclusive lock for archiving files.
        /// </summary>
        /// <value>The mutex for archiving.</value>
        [CanBeNull]
        public Mutex ArchiveMutex { get; private set; }

        private Mutex CreateArchiveMutex()
        {
            try
            {
                return CreateSharableMutex("FileArchiveLock");
            }
            catch (Exception ex)
            {
                if (ex is SecurityException || ex is UnauthorizedAccessException || ex is NotSupportedException || ex is NotImplementedException || ex is PlatformNotSupportedException)
                {
                    InternalLogger.Warn(ex, "{0}: Failed to create global archive mutex: {1}", CreateFileParameters, FileName);
                    return new Mutex();
                }

                InternalLogger.Error(ex, "{0}: Failed to create global archive mutex: {1}", CreateFileParameters, FileName);
                if (ex.MustBeRethrown())
                    throw;
                return new Mutex();
            }
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
                ArchiveMutex?.Close();    // Only closed on dispose, Mutex must survive, when closing FileAppender before archive
            }
        }

        /// <summary>
        /// Creates a mutex that is sharable by more than one process.
        /// </summary>
        /// <param name="mutexNamePrefix">The prefix to use for the name of the mutex.</param>
        /// <returns>A <see cref="Mutex"/> object which is sharable by multiple processes.</returns>
        protected Mutex CreateSharableMutex(string mutexNamePrefix)
        {
            if (!MutexDetector.SupportsSharableMutex)
                throw new NotSupportedException("Creating Mutex not supported");

            var name = GetMutexName(mutexNamePrefix);

            return ForceCreateSharableMutex(name);
        }

        internal static Mutex ForceCreateSharableMutex(string name)
        {
#if !NETSTANDARD
            // Creates a mutex sharable by more than one process
            var mutexSecurity = new MutexSecurity();
            var everyoneSid = new SecurityIdentifier(WellKnownSidType.WorldSid, null);
            mutexSecurity.AddAccessRule(new MutexAccessRule(everyoneSid, MutexRights.FullControl, AccessControlType.Allow));

            // The constructor will either create new mutex or open
            // an existing one, in a thread-safe manner
            return new Mutex(false, name, out _, mutexSecurity);
#else
            //Mutex with 4 args has keyword "unsafe"
            return new Mutex(false, name);
#endif
        }

        private string GetMutexName(string mutexNamePrefix)
        {
            const string mutexNameFormatString = @"Global\NLog-File{0}-{1}";
            const int maxMutexNameLength = 260;

            string canonicalName = Path.GetFullPath(FileName).ToLowerInvariant();

            // Mutex names must not contain a slash, it's the namespace separator,
            // but all other are OK
            canonicalName = canonicalName.Replace('\\', '_');
            canonicalName = canonicalName.Replace('/', '_');
            string mutexName = string.Format(mutexNameFormatString, mutexNamePrefix, canonicalName);

            // A mutex name must not exceed MAX_PATH (260) characters
            if (mutexName.Length <= maxMutexNameLength)
            {
                return mutexName;
            }

            // The unusual case of the path being too long; let's hash the canonical name,
            // so it can be safely shortened and still remain unique
            string hash;
            using (MD5 md5 = MD5.Create())
            {
                byte[] bytes = md5.ComputeHash(Encoding.UTF8.GetBytes(canonicalName));
                hash = Convert.ToBase64String(bytes);
            }

            // The hash makes the name unique, but also add the end of the path,
            // so the end of the name tells us which file it is (for debugging)
            mutexName = string.Format(mutexNameFormatString, mutexNamePrefix, hash);
            int cutOffIndex = canonicalName.Length - (maxMutexNameLength - mutexName.Length);
            return mutexName + canonicalName.Substring(cutOffIndex);
        }
#endif
    }
}
