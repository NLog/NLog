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

#if !SILVERLIGHT && !__ANDROID__ && !__IOS__
// Unfortunately, Xamarin Android and Xamarin iOS don't support mutexes (see https://github.com/mono/mono/blob/3a9e18e5405b5772be88bfc45739d6a350560111/mcs/class/corlib/System.Threading/Mutex.cs#L167) 
#define SupportsMutex
#endif

namespace NLog.Internal.FileAppenders
{
    using System;
    using System.IO;
    using System.Security;
    using System.Threading;
    using System.Text;

#if SupportsMutex
    using System.Security.AccessControl;
    using System.Security.Principal;
    using System.Security.Cryptography;
    using NLog.Common;
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
        protected BaseMutexFileAppender(string fileName, ICreateFileParameters createParameters) 
            : base(fileName, createParameters)
        {
#if SupportsMutex
            try
            {
                ArchiveMutex = CreateArchiveMutex();
            }
            catch (SecurityException ex)
            {
                InternalLogger.Warn(ex, "Failed to create archive mutex");
            }
#endif
        }

#if SupportsMutex
        /// <summary>
        /// Gets the mutually-exclusive lock for archiving files.
        /// </summary>
        /// <value>The mutex for archiving.</value>
        public Mutex ArchiveMutex { get; private set; }

        /// <summary>
        /// Creates a mutually-exclusive lock for archiving files.
        /// </summary>
        /// <returns>A <see cref="Mutex"/> object which can be used for controlling the archiving of files.</returns>
        protected virtual Mutex CreateArchiveMutex()
        {
            return new Mutex();
        }

        /// <summary>
        /// Creates a mutex for archiving that is sharable by more than one process.
        /// </summary>
        /// <returns>A <see cref="Mutex"/> object which can be used for controlling the archiving of files.</returns>
        protected Mutex CreateSharableArchiveMutex()
        {
            return CreateSharableMutex("FileArchiveLock");
        }

        /// <summary>
        /// Creates a mutex that is sharable by more than one process.
        /// </summary>
        /// <param name="mutexNamePrefix">The prefix to use for the name of the mutex.</param>
        /// <returns>A <see cref="Mutex"/> object which is sharable by multiple processes.</returns>
        protected Mutex CreateSharableMutex(string mutexNamePrefix)
        {
            if (!PlatformDetector.SupportsSharableMutex)
                return new Mutex();

            // Creates a mutex sharable by more than one process
            var mutexSecurity = new MutexSecurity();
            var everyoneSid = new SecurityIdentifier(WellKnownSidType.WorldSid, null);
            mutexSecurity.AddAccessRule(new MutexAccessRule(everyoneSid, MutexRights.FullControl, AccessControlType.Allow));

            // The constructor will either create new mutex or open
            // an existing one, in a thread-safe manner
            bool createdNew;
            return new Mutex(false, GetMutexName(mutexNamePrefix), out createdNew, mutexSecurity);
        }

        private string GetMutexName(string mutexNamePrefix)
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
