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

#if !SILVERLIGHT && !NETSTANDARD1_3

namespace NLog.Internal
{
    using System.IO;

    /// <summary>
    /// Returns details about current process and thread in a portable manner.
    /// </summary>
    internal abstract class ProcessIDHelper
    {
        private const string UnknownProcessName = "<unknown>";

        private static ProcessIDHelper _threadIDHelper;
        private string _currentProcessBaseName;

        /// <summary>
        /// Gets the singleton instance of PortableThreadIDHelper or
        /// Win32ThreadIDHelper depending on runtime environment.
        /// </summary>
        /// <value>The instance.</value>
        public static ProcessIDHelper Instance => _threadIDHelper ?? (_threadIDHelper = Create());

        /// <summary>
        /// Gets current process ID.
        /// </summary>
        public abstract int CurrentProcessID { get; }

        /// <summary>
        /// Gets current process absolute file path.
        /// </summary>
        public abstract string CurrentProcessFilePath { get; }

        /// <summary>
        /// Gets current process name (excluding filename extension, if any).
        /// </summary>
        public string CurrentProcessBaseName => _currentProcessBaseName ?? (_currentProcessBaseName = string.IsNullOrEmpty(CurrentProcessFilePath) ? UnknownProcessName : Path.GetFileNameWithoutExtension(CurrentProcessFilePath));

        /// <summary>
        /// Initializes the ThreadIDHelper class.
        /// </summary>
        private static ProcessIDHelper Create()
        {
#if !SILVERLIGHT && !__IOS__ && !__ANDROID__ && !NETSTANDARD
            if (PlatformDetector.IsWin32)
            {
                return new Win32ProcessIDHelper();
            }
            else
#endif
            {
                return new PortableProcessIDHelper();
            }
        }
    }
}

#endif
