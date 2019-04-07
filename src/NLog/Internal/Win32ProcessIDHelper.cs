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

#if !SILVERLIGHT && !__IOS__ && !__ANDROID__ && !NETSTANDARD

namespace NLog.Internal
{
    using System;
    using System.Security;
    using System.Text;

    /// <summary>
    /// Win32-optimized implementation of <see cref="ProcessIDHelper"/>.
    /// </summary>
    [SecuritySafeCritical]
    internal class Win32ProcessIDHelper : ProcessIDHelper
    {
        private readonly int _currentProcessId;
        private readonly string _currentProcessFilePath = string.Empty;

        /// <summary>
        /// Initializes a new instance of the <see cref="Win32ProcessIDHelper" /> class.
        /// </summary>
        public Win32ProcessIDHelper()
        {
            _currentProcessId = NativeMethods.GetCurrentProcessId();

            var sb = new StringBuilder(512);
            if (0 == NativeMethods.GetModuleFileName(IntPtr.Zero, sb, sb.Capacity))
            {
                throw new InvalidOperationException("Cannot determine program name.");
            }

            _currentProcessFilePath = sb.ToString();
        }

        /// <summary>
        /// Gets current process ID.
        /// </summary>
        public override int CurrentProcessID => _currentProcessId;

        /// <summary>
        /// Gets current process absolute file path.
        /// </summary>
        public override string CurrentProcessFilePath => _currentProcessFilePath;
    }
}

#endif