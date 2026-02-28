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

#if NETSTANDARD

namespace NLog.Internal
{
    using System;
    using System.IO;
    using System.Runtime.InteropServices;

    internal static class NativeMethods
    {
        [Flags]
        public enum Win32SecurityOptions : uint
        {
            /// <summary>
            /// Security Quality of Service (SQOS) information requested.
            /// </summary>
            SECURITY_SQOS_PRESENT = 0x00100000,
            /// <summary>
            /// Impersonates a client at the Anonymous impersonation level. 
            /// </summary>
            SECURITY_ANONYMOUS = 0 << 16,
            /// <summary>
            /// Impersonates a client at the Identification impersonation level. 
            /// </summary>
            SECURITY_IDENTIFICATION = 1 << 16,
            /// <summary>
            /// Impersonate a client at the impersonation level. This is the default behavior if no other flags are specified along with the SECURITY_SQOS_PRESENT flag. 
            /// </summary>
            SECURITY_IMPERSONATION = 2 << 16,
            /// <summary>
            /// Impersonates a client at the Delegation impersonation level. 
            /// </summary>
            SECURITY_DELEGATION = 3 << 16,
        }

        [Flags]
        public enum Win32FileAccess : uint
        {
            /// <summary>
            /// For a file, the right to append data to a file.
            /// to overwrite existing data.
            /// </summary>
            FILE_APPEND_DATA = 0x00000004,
            /// <summary>
            /// The right to use the object for synchronization. Enables a thread to wait until the object
            /// is in the signaled state. This is required if opening a synchronous handle.
            /// </summary>
            SYNCHRONIZE = 0x00100000,
            FILE_GENERIC_READ = 0x80000000,
            FILE_GENERIC_WRITE = 0x40000000,
            FILE_GENERIC_EXECUTE = 0x20000000,
            /// <summary>
            /// The right to delete the object.
            /// </summary>
            DELETE = 0x00010000,
        }

        [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        public static extern Microsoft.Win32.SafeHandles.SafeFileHandle CreateFile(
            string lpFileName,
            Win32FileAccess dwDesiredAccess,
            FileShare dwShareMode,
            IntPtr lpSecurityAttributes,
            FileMode dwCreationDisposition,
            uint dwFlagsAndAttributes,
            IntPtr hTemplateFile);
    }
}

#endif
