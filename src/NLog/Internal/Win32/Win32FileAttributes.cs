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

#if !NETCF

using System;
using System.Reflection;
using System.Globalization;
using NLog.Internal;
using System.IO;
using System.Runtime.InteropServices;

namespace NLog.Internal.Win32
{
    /// <summary>
    /// Win32 file attributes
    /// </summary>
    /// <remarks>
    /// For more information see <a href="http://msdn.microsoft.com/library/default.asp?url=/library/en-us/fileio/fs/createfile.asp">http://msdn.microsoft.com/library/default.asp?url=/library/en-us/fileio/fs/createfile.asp</a>.
    /// </remarks>
    [Flags]
    public enum Win32FileAttributes : int
    {
        /// <summary>
        /// Read-only
        /// </summary>
        Readonly = 0x00000001,

        /// <summary>
        /// Hidden
        /// </summary>
        Hidden = 0x00000002,

        /// <summary>
        /// System
        /// </summary>
        System = 0x00000004,

        /// <summary>
        /// File should be archived.
        /// </summary>
        Archive = 0x00000020,

        /// <summary>
        /// Device
        /// </summary>
        Device = 0x00000040,

        /// <summary>
        /// Normal
        /// </summary>
        Normal = 0x00000080,

        /// <summary>
        /// File is temporary (should be kept in cache and not 
        /// written to disk if possible)
        /// </summary>
        Temporary = 0x00000100,

        /// <summary>
        /// Sparse file.
        /// </summary>
        SparseFile = 0x00000200,

        /// <summary>
        /// Reparse point.
        /// </summary>
        ReparsePoint = 0x00000400,

        /// <summary>
        /// Compress file contents.
        /// </summary>
        Compressed = 0x00000800,

        /// <summary>
        /// File should not be indexed by the content indexing service. 
        /// </summary>
        NotContentIndexed = 0x00002000,

        /// <summary>
        /// Encrypt file.
        /// </summary>
        Encrypted = 0x00004000,

        /// <summary>
        /// The system writes through any intermediate cache and goes directly to disk. 
        /// </summary>
        WriteThrough = unchecked((int)0x80000000),

        /// <summary>
        /// The system opens a file with no system caching
        /// </summary>
        NoBuffering = 0x20000000,

        /// <summary>
        /// Delete file after it is closed.
        /// </summary>
        DeleteOnClose = 0x04000000,

        /// <summary>
        /// A file is accessed according to POSIX rules.
        /// </summary>
        PosixSemantics = 0x01000000,
    };
}

#endif
