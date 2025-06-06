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

namespace NLog.Internal
{
    using System;
    using System.IO;

    internal static class FileInfoExt
    {
        public static DateTime GetLastWriteTimeUtc(this FileInfo fileInfo)
        {
            return fileInfo.LastWriteTimeUtc;
        }
        public static DateTime GetCreationTimeUtc(this FileInfo fileInfo)
        {
            return fileInfo.CreationTimeUtc;
        }

        public static DateTime LookupValidFileCreationTimeUtc(this FileInfo fileInfo)
        {
            return LookupValidFileCreationTimeUtc(fileInfo, (f) => f.GetCreationTimeUtc(), (f) => f.GetLastWriteTimeUtc()).Value;
        }

        public static DateTime LookupValidFileCreationTimeUtc(this FileInfo fileInfo, DateTime? fallbackTime)
        {
            if (fallbackTime > DateTime.MinValue)
                return LookupValidFileCreationTimeUtc(fileInfo, (f) => f.GetCreationTimeUtc(), (f) => fallbackTime.Value, (f) => f.GetLastWriteTimeUtc()).Value;
            else
                return LookupValidFileCreationTimeUtc(fileInfo);
        }

        internal static DateTime? LookupValidFileCreationTimeUtc<T>(T fileInfo, Func<T, DateTime?> primary, Func<T, DateTime?> fallback, Func<T, DateTime?> finalFallback = null)
        {
            DateTime? fileCreationTime = primary(fileInfo);

            if (fileCreationTime.HasValue && fileCreationTime.Value.Year < 1980 && !PlatformDetector.IsWin32)
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
