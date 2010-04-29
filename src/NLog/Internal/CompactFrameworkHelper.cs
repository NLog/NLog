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

#if NETCF

using System;
using System.IO;
using System.Text;
using System.Runtime.InteropServices;

namespace NLog.Internal
{
    internal sealed class CompactFrameworkHelper
    {
        private static string _exeName;
        private static string _exeBaseDir;

        public static string GetExeFileName()
        {
            if (_exeName == null)
            {
                LoadExeInfo();
            }
            return _exeName;
        }

        public static string GetExeBaseDir()
        {
            if (_exeName == null)
            {
                LoadExeInfo();
            }
            return _exeBaseDir;
        }

        private static void LoadExeInfo()
        {
            lock (typeof(CompactFrameworkHelper))
            {
                if (_exeName == null)
                {
                    StringBuilder sb = new StringBuilder(512);

                    // passing 0 as the first parameter gets us the name of the EXE

                    GetModuleFileName(IntPtr.Zero, sb, sb.Capacity);
                    _exeName = sb.ToString();
                    _exeBaseDir = Path.GetDirectoryName(_exeName);
                }
            }
        }

        [DllImport("coredll.dll", CharSet=CharSet.Unicode)]
        private static extern int GetModuleFileName(IntPtr hModule, StringBuilder szBuffer, int nCapacity);
    }
}

#endif
