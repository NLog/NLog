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

#if !SILVERLIGHT && !NET_CF

namespace NLog.Internal
{
    using System;
    using System.Diagnostics;
    using System.IO;
    using System.Threading;

    /// <summary>
    /// Portable implementation of <see cref="ThreadIDHelper"/>.
    /// </summary>
    internal class PortableThreadIDHelper : ThreadIDHelper
    {
        private const string UnknownProcessName = "<unknown>";

        private readonly int currentProcessID;

        private string currentProcessName;
        private string currentProcessBaseName;

        /// <summary>
        /// Initializes a new instance of the <see cref="PortableThreadIDHelper" /> class.
        /// </summary>
        public PortableThreadIDHelper()
        {
            this.currentProcessID = Process.GetCurrentProcess().Id;
        }

        /// <summary>
        /// Gets current thread ID.
        /// </summary>
        /// <value></value>
        public override int CurrentThreadID
        {
            get { return Thread.CurrentThread.ManagedThreadId; }
        }

        /// <summary>
        /// Gets current process ID.
        /// </summary>
        /// <value></value>
        public override int CurrentProcessID
        {
            get { return this.currentProcessID; }
        }

        /// <summary>
        /// Gets current process name.
        /// </summary>
        /// <value></value>
        public override string CurrentProcessName
        {
            get
            {
                this.GetProcessName();
                return this.currentProcessName;
            }
        }

        /// <summary>
        /// Gets current process name (excluding filename extension, if any).
        /// </summary>
        /// <value></value>
        public override string CurrentProcessBaseName
        {
            get
            {
                this.GetProcessName();
                return this.currentProcessBaseName;
            }
        }

        /// <summary>
        /// Gets the name of the process.
        /// </summary>
        private void GetProcessName()
        {
            if (this.currentProcessName == null)
            {
                try
                {
                    this.currentProcessName = Process.GetCurrentProcess().MainModule.FileName;
                }
                catch (Exception exception)
                {
                    if (exception.MustBeRethrown())
                    {
                        throw;
                    }

                    this.currentProcessName = UnknownProcessName;
                }

                this.currentProcessBaseName = Path.GetFileNameWithoutExtension(this.currentProcessName);
            }
        }
    }
}

#endif