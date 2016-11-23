// 
// Copyright (c) 2004-2016 Jaroslaw Kowalski <jaak@jkowalski.net>, Kim Christensen, Julian Verdurmen
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
using System.IO;
using NLog.Common;

namespace NLog.Targets
{
    internal static class ConsoleTargetHelper
    {
        public static bool IsConsoleAvailable(out string reason)
        {
            reason = string.Empty;
#if !SILVERLIGHT && !__IOS__ && !__ANDROID__ && !MONO
            try
            {
                if (!Environment.UserInteractive)
                {
                    if (Internal.PlatformDetector.IsMono && Console.In is StreamReader)
                        return true;    // Extra bonus check for Mono, that doesn't support Environment.UserInteractive

                    reason = "Environment.UserInteractive = False";
                    return false;
                }
                else if (Console.OpenStandardInput(1) == Stream.Null)
                {
                    reason = "Console.OpenStandardInput = Null";
                    return false;
                }
            }
            catch (Exception ex)
            {
                reason = string.Format("Unexpected exception: {0}:{1}", ex.GetType().Name, ex.Message);
                InternalLogger.Warn(ex, "Failed to detect whether console is available.");
                return false;
            }
#endif
            return true;
        }
    }
}
