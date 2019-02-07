// 
// Copyright (c) 2004-2019 Jaroslaw Kowalski <jaak@jkowalski.net>, Kim Christensen, Julian Verdurmen
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

namespace NLog.Internal
{
    using System;
    using System.IO;

    /// <summary>
    /// Optimized routines to get the basic file characteristics of the specified file.
    /// </summary>
    internal abstract class FileCharacteristicsHelper
    {
        /// <summary>
        /// Initializes static members of the FileCharacteristicsHelper class.
        /// </summary>
        public static FileCharacteristicsHelper CreateHelper(bool forcedManaged)
        {
#if !SILVERLIGHT && !__IOS__ && !__ANDROID__ && !NETSTANDARD
            if (!forcedManaged && PlatformDetector.IsDesktopWin32 && !PlatformDetector.IsMono)
            {
                return new Win32FileCharacteristicsHelper();
            }
            else
#endif
            {
                return new PortableFileCharacteristicsHelper();
            }
        }

        /// <summary>
        /// Gets the information about a file.
        /// </summary>
        /// <param name="fileName">Name of the file.</param>
        /// <param name="fileStream">The file stream.</param>
        /// <returns>The file characteristics, if the file information was retrieved successfully, otherwise null.</returns>
        public abstract FileCharacteristics GetFileCharacteristics(string fileName, FileStream fileStream);

        public static DateTime? ValidateFileCreationTime<T>(T fileInfo, Func<T, DateTime?> primary, Func<T, DateTime?> fallback, Func<T, DateTime?> finalFallback = null)
        {
            DateTime? fileCreationTime = primary(fileInfo);

            if (fileCreationTime.HasValue && fileCreationTime.Value.Year < 1980 && !PlatformDetector.IsDesktopWin32)
            {
                // Non-Windows-FileSystems doesn't always provide correct CreationTime/BirthTime
                fileCreationTime = fallback(fileInfo);
                if (finalFallback != null && (!fileCreationTime.HasValue || fileCreationTime.Value.Year < 1980))
                {
                    fileCreationTime = finalFallback(fileInfo);
                }
            }
            return fileCreationTime;
        }
    }
}
