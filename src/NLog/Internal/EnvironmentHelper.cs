// 
// Copyright (c) 2004-2020 Jaroslaw Kowalski <jaak@jkowalski.net>, Kim Christensen, Julian Verdurmen
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
    using System.Security;

    /// <summary>
    /// Safe way to get environment variables.
    /// </summary>
	internal static class EnvironmentHelper
	{
		internal static string NewLine
		{
            get
            {
#if !SILVERLIGHT || WINDOWS_PHONE
                string newline = Environment.NewLine;
#else
                string newline = "\r\n";
#endif
                return newline;
            }
        }

        internal static string GetMachineName()
        {
            try
            {
#if SILVERLIGHT
                return "SilverLight";
#elif NETSTANDARD1_3
                var machineName = EnvironmentHelper.GetSafeEnvironmentVariable("COMPUTERNAME") ?? string.Empty;
                if (string.IsNullOrEmpty(machineName))
                    machineName = EnvironmentHelper.GetSafeEnvironmentVariable("HOSTNAME") ?? string.Empty;
                return machineName;
#else
                return Environment.MachineName;
#endif
            }
            catch (System.Security.SecurityException)
            {
                return string.Empty;
            }
        }

        internal static string GetSafeEnvironmentVariable(string name)
        {
#if !SILVERLIGHT
            try
            {
                string s = Environment.GetEnvironmentVariable(name);

                if (string.IsNullOrEmpty(s))
                {
                    return null;
                }

                return s;
            }
            catch (SecurityException)
            {
                return null;
            }
#else
            return null;
#endif
        }
    }
}
