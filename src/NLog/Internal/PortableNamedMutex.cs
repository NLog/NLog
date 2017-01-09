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

namespace NLog.Internal
{
    using System;
    using System.IO;
    using System.Threading;
    using System.Collections.Generic;
    using NLog.Common;

#if SupportsMutex
    using System.Security.AccessControl;
    using System.Security.Principal;
#endif

    /// <summary>
    /// Curious if Unix Semaphores are working better
    /// </summary>
    internal class PortableNamedMutex : IDisposable
    {
#if SupportsMutex
        readonly Mutex _mutex;
#endif
#if !SILVERLIGHT
        readonly FileStream _fileMutex;
#endif
        readonly LockCounter _lockObject;

        private class LockCounter
        {
            public LockCounter()
            {
                UseCount = 1;
            }
            public int UseCount;
        };

        readonly static object _lockProcess = new object();
        readonly static Dictionary<string, LockCounter> _lockObjects = new Dictionary<string, LockCounter>(StringComparer.Ordinal);

        public PortableNamedMutex(string name, string mutexPath)
        {
#if SupportsMutex
            try
            {
                if (PlatformDetector.SupportsSharableMutex)
                {
                    _mutex = CreateSharableMutex(name);
                    return;
                }
            }
            catch (System.Security.SecurityException ex)
            {
                InternalLogger.Warn(ex, "Failed to create global mutex: {0}", name);
            }
#endif

#if !SILVERLIGHT
            if (!string.IsNullOrEmpty(mutexPath))
            {
                string fileName = name;
                foreach (char invalidChar in Path.GetInvalidFileNameChars())
                    fileName = fileName.Replace(invalidChar, '_');

                string filePath = Path.Combine(mutexPath, fileName);
                if (filePath.Length > 255)
                {
                    fileName = Path.Combine(mutexPath, fileName.Substring(filePath.Length - 255));
                }

                try
                {
                    if (!Directory.Exists(mutexPath))
                        Directory.CreateDirectory(mutexPath);

                    Random random = null;
                    for (int i = 1; i <= 10; ++i)
                    {
                        try
                        {
                            _fileMutex = new FileStream(filePath, FileMode.Append, FileAccess.Write, FileShare.ReadWrite | FileShare.Delete, 1);
                            if (_fileMutex.Length == 0)
                            {
                                var bytes = System.Text.Encoding.Default.GetBytes("NLog File Archive Mutex");
                                _fileMutex.Write(bytes, 0, bytes.Length);
                            }
                            break;
                        }
                        catch (System.IO.IOException)
                        {
                            if (i == 10)
                                throw;
                            if (random == null)
                                random = new Random();
                            Thread.Sleep(random.Next(10, 20));
                        }
                    }
                    return;
                }
                catch (System.IO.IOException ex)
                {
                    InternalLogger.Warn(ex, "Failed to create global file mutex: {0}", filePath);
                }
            }
#endif

            lock (_lockProcess)
            {
                if (!_lockObjects.TryGetValue(name, out _lockObject))
                {
                    _lockObject = new LockCounter();
                    _lockObjects[name] = _lockObject;
                    if (_lockObjects.Count > 5000)
                    {
                        List<string> cleanupKeys = new List<string>();
                        foreach (var lockObject in _lockObjects)
                            if (lockObject.Value.UseCount <= 0)
                                cleanupKeys.Add(lockObject.Key);
                        foreach (var key in cleanupKeys)
                            _lockObjects.Remove(key);
                    }
                }
                else
                {
                    Interlocked.Increment(ref _lockObject.UseCount);
                }
            }
        }

#if SupportsMutex
        /// <summary>
        /// Creates a mutex that is sharable by more than one process.
        /// </summary>
        /// <param name="name">The prefix to use for the name of the mutex.</param>
        /// <returns>A <see cref="Mutex"/> object which is sharable by multiple processes.</returns>
        private Mutex CreateSharableMutex(string name)
        {
            // Creates a mutex sharable by more than one process
            var mutexSecurity = new MutexSecurity();
            var everyoneSid = new SecurityIdentifier(WellKnownSidType.WorldSid, null);
            mutexSecurity.AddAccessRule(new MutexAccessRule(everyoneSid, MutexRights.FullControl, AccessControlType.Allow));

            // The constructor will either create new mutex or open
            // an existing one, in a thread-safe manner
            bool createdNew;
            return new Mutex(false, name, out createdNew, mutexSecurity);
        }
#endif

        public void WaitOne()
        {
#if SupportsMutex
            if (_mutex != null)
            {
                try
                {
                    _mutex.WaitOne();
                }
                catch (AbandonedMutexException)
                {
                    // ignore the exception, another process was killed without properly releasing the mutex
                    // the mutex has been acquired, so proceed to writing
                    // See: http://msdn.microsoft.com/en-us/library/system.threading.abandonedmutexexception.aspx
                }
            }
            else
#endif
#if !SILVERLIGHT
            if (_fileMutex != null)
            {
                Random random = null;
                for (int i = 1; i <= 1000; ++i)
                {
                    try
                    {
                        _fileMutex.Lock(0, 1);
                        break;
                    }
                    catch (System.IO.IOException)
                    {
                        if (i == 1000)
                            throw;

                        if (random == null)
                            random = new Random();
                        Thread.Sleep(random.Next(10, 20));
                    }
                }
            }
            else
#endif
                Monitor.Enter(_lockObject);
        }

        public void ReleaseMutex()
        {
#if SupportsMutex
            if (_mutex != null)
                _mutex.ReleaseMutex();
            else
#endif
#if !SILVERLIGHT
            if (_fileMutex != null)
                _fileMutex.Unlock(0, 1);
            else
#endif
                Monitor.Exit(_lockObject);
        }

        public void Close()
        {
            Dispose();
        }

        public void Dispose()
        {
            if (_disposed)
                return;

            _disposed = true;

#if SupportsMutex
            if (_mutex != null)
                _mutex.Close();
            else
#endif
#if !SILVERLIGHT
            if (_fileMutex != null)
                _fileMutex.Close(); // TODO File Cleanup
            else
#endif
                Interlocked.Decrement(ref _lockObject.UseCount);
        }
        private bool _disposed;
    }
}
