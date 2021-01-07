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

using System;
using System.IO;
using System.Text;
using NLog.Common;

namespace NLog.Targets
{
    internal static class ConsoleTargetHelper
    {
        private static readonly object _lockObject = new object();

        public static bool IsConsoleAvailable(out string reason)
        {
            reason = string.Empty;
#if !MONO && !NETSTANDARD1_3 && !NETSTANDARD1_5
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
                reason = $"Unexpected exception: {ex.GetType().Name}:{ex.Message}";
                InternalLogger.Warn(ex, "Failed to detect whether console is available.");
                return false;
            }
#endif
            return true;
        }

        public static Encoding GetConsoleOutputEncoding(Encoding currentEncoding, bool isInitialized, bool pauseLogging)
        {
#if !NETSTANDARD1_3
            if (currentEncoding != null)
                return currentEncoding;
            else if ((isInitialized && !pauseLogging) || IsConsoleAvailable(out _))
                return Console.OutputEncoding;
#if !NETSTANDARD1_5
            return Encoding.Default;
#else
            return currentEncoding;
#endif
#else
            return currentEncoding;
#endif
        }

        public static bool SetConsoleOutputEncoding(Encoding newEncoding, bool isInitialized, bool pauseLogging)
        {
#if !NETSTANDARD1_3
            if (!isInitialized)
            {
                return true;    // Waiting for console target to be initialized
            }
            else if (!pauseLogging)
            {
                try
                {
                    Console.OutputEncoding = newEncoding;   // Can throw exception if console is not available
                    return true;
                }
                catch (Exception ex)
                {
                    InternalLogger.Warn(ex, "Failed changing Console.OutputEncoding to {0}", newEncoding);
                }
            }
#endif
            return false;       // No console available
        }

        public static void WriteLineThreadSafe(TextWriter console, string message, bool flush = false)
        {
            lock (_lockObject)
            {
                console.WriteLine(message);
                if (flush)
                    console.Flush();
            }
        }

        public static void WriteBufferThreadSafe(TextWriter console, char[] buffer, int length, bool flush = false)
        {
            lock (_lockObject)
            {
                console.Write(buffer, 0, length);
                if (flush)
                    console.Flush();
            }
        }
    }
}
