// 
// Copyright (c) 2004-2018 Jaroslaw Kowalski <jaak@jkowalski.net>, Kim Christensen, Julian Verdurmen
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

#if !SILVERLIGHT && !__IOS__ && !NETSTANDARD1_3

namespace NLog.Internal
{
    using System;
    using System.Diagnostics;
    using System.IO;

    /// <summary>
    /// Portable implementation of <see cref="ThreadIDHelper"/>.
    /// </summary>
    internal class PortableThreadIDHelper : ThreadIDHelper
    {
        private const string UnknownProcessName = "<unknown>";

        private readonly int _currentProcessId;

        private string _currentProcessName;
        private string _currentProcessBaseName;

        /// <summary>
        /// Initializes a new instance of the <see cref="PortableThreadIDHelper" /> class.
        /// </summary>
        public PortableThreadIDHelper()
        {
            _currentProcessId = Process.GetCurrentProcess().Id;
        }

        /// <summary>
        /// Gets current process ID.
        /// </summary>
        /// <value></value>
        public override int CurrentProcessID => _currentProcessId;

        /// <summary>
        /// Gets current process name.
        /// </summary>
        /// <value></value>
        public override string CurrentProcessName
        {
            get
            {
                GetProcessName();
                return _currentProcessName;
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
                GetProcessName();
                return _currentProcessBaseName;
            }
        }

        /// <summary>
        /// Gets the name of the process.
        /// </summary>
        private void GetProcessName()
        {
            if (_currentProcessName == null)
            {
                try
                {
                    _currentProcessName = Process.GetCurrentProcess().MainModule.FileName;
                }
                catch (Exception exception)
                {
                    if (exception.MustBeRethrown())
                    {
                        throw;
                    }

                    _currentProcessName = UnknownProcessName;
                }

                _currentProcessBaseName = Path.GetFileNameWithoutExtension(_currentProcessName);
            }
        }
    }
}

#endif