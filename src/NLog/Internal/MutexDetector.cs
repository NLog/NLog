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

namespace NLog.Internal
{
    using System;
    using NLog.Common;
    using NLog.Internal.FileAppenders;

    /// <summary>
    /// Detects the platform the NLog is running on.
    /// </summary>
    internal static class MutexDetector
    {
        /// <summary>
        /// Gets a value indicating whether current runtime supports use of mutex
        /// </summary>
        public static bool SupportsSharableMutex
        {
            get
            {
#if NETSTANDARD1_5
                return RunTimeSupportsSharableMutex;
#elif !NETSTANDARD1_3
                if (PlatformDetector.IsMono && Environment.Version.Major < 4)
                    return false;   // MONO ver. 4 is needed for named Mutex to work
                else
                    return RunTimeSupportsSharableMutex;
#else
                return false;
#endif
            }
        }

        /// <summary>
        ///  Will creating a mutex succeed runtime?
        /// "Cached" detection
        /// </summary>
        private static bool? _runTimeSupportsSharableMutex;

        /// <summary>
        /// Will creating a mutex succeed runtime?
        /// </summary>
        private static bool RunTimeSupportsSharableMutex
        {
            get
            {
                if (_runTimeSupportsSharableMutex.HasValue)
                {
                    return _runTimeSupportsSharableMutex.Value;
                }


                try
                {
#if SupportsMutex
                    var mutex = BaseMutexFileAppender.ForceCreateSharableMutex("NLogMutexTester");
                    mutex.Close(); //"dispose"

                    _runTimeSupportsSharableMutex = true;
#else
                    _runTimeSupportsSharableMutex = false;
#endif
                }
                catch (Exception ex)
                {
                    InternalLogger.Debug(ex, "Failed to create sharable mutex processes");
                    _runTimeSupportsSharableMutex = false;
                }

                return _runTimeSupportsSharableMutex.Value;
            }

        }
    }
}
