// 
// Copyright (c) 2004-2006 Jaroslaw Kowalski <jaak@jkowalski.net>
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

using System;
using System.Text;
using System.Reflection;
using System.Collections;

using NLog.Config;
using NLog.Internal;
#if !NETCF
using NLog.Internal.Win32;
#endif
using System.IO;

namespace NLog.Internal
{
    // optimized routines to get the size and last write time
    // of the specified file
    internal abstract class FileInfoHelper
    {
        public static readonly FileInfoHelper Helper;

        static FileInfoHelper()
        {
#if NETCF
            Helper = new GenericFileInfoHelper();
#else
            if (PlatformDetector.IsCurrentOSCompatibleWith(RuntimeOS.Windows) ||
                PlatformDetector.IsCurrentOSCompatibleWith(RuntimeOS.WindowsNT))
            {
                Helper = new Win32FileInfoHelper();
            }
            else
            {
                Helper = new GenericFileInfoHelper();
            }
#endif
        }

        public abstract bool GetFileInfo(string fileName, IntPtr fileHandle, out DateTime lastWriteTime, out long fileLength);
    }

#if !NETCF
    internal class Win32FileInfoHelper : FileInfoHelper
    {
        public override bool GetFileInfo(string fileName, IntPtr fileHandle, out DateTime lastWriteTime, out long fileLength)
        {
            Win32FileHelper.BY_HANDLE_FILE_INFORMATION fi;

            if (Win32FileHelper.GetFileInformationByHandle(fileHandle, out fi))
            {
                lastWriteTime = DateTime.FromFileTime(fi.ftLastWriteTime);
                fileLength = fi.nFileSizeLow + (((long)fi.nFileSizeHigh) << 32);
                return true;
            }
            else
            {
                lastWriteTime = DateTime.MinValue;
                fileLength = -1;
                return false;
            }
        }
    }
#endif
    
    internal class GenericFileInfoHelper : FileInfoHelper
    {
        public override bool GetFileInfo(string fileName, IntPtr fileHandle, out DateTime lastWriteTime, out long fileLength)
        {
            FileInfo fi = new FileInfo(fileName);
            if (fi.Exists)
            {
                fileLength = fi.Length;
                lastWriteTime = fi.LastWriteTime;
                return true;
            }
            else
            {
                fileLength = -1;
                lastWriteTime = DateTime.MinValue;
                return false;
            }
        }
    }
}
