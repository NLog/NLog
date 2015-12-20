// 
// Copyright (c) 2004-2011 Jaroslaw Kowalski <jaak@jkowalski.net>
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

#if !SILVERLIGHT && !__IOS__

namespace NLog.Internal
{
    using NLog.Config;

    /// <summary>
    /// Returns details about current process and thread in a portable manner.
    /// </summary>
    internal abstract class ThreadIDHelper
    {
        /// <summary>
        /// Initializes static members of the ThreadIDHelper class.
        /// </summary>
        static ThreadIDHelper()
        {
            if (PlatformDetector.IsWin32)
            {
                Instance = new Win32ThreadIDHelper();
            }
            else
            {
                Instance = new PortableThreadIDHelper();
            }
        }

        /// <summary>
        /// Gets the singleton instance of PortableThreadIDHelper or
        /// Win32ThreadIDHelper depending on runtime environment.
        /// </summary>
        /// <value>The instance.</value>
        public static ThreadIDHelper Instance { get; private set; }

        /// <summary>
        /// Gets current process ID.
        /// </summary>
        public abstract int CurrentProcessID { get; }

        /// <summary>
        /// Gets current process name.
        /// </summary>
        public abstract string CurrentProcessName { get; }

        /// <summary>
        /// Gets current process name (excluding filename extension, if any).
        /// </summary>
        public abstract string CurrentProcessBaseName { get; }
    }
}

#endif