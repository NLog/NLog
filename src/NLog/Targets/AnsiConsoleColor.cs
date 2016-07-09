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

#if !SILVERLIGHT && !__IOS__ && !__ANDROID__ && !UWP10 || NETSTANDARD1_3

namespace NLog.Targets
{
    using System;
    
    internal static class AnsiConsoleColor
    {
        internal static string GetForegroundColorEscapeCode(ConsoleColor color)
        {
            switch (color)
            {
                case ConsoleColor.Black:
                    return "\x1B[30m";
                case ConsoleColor.Blue:
                    return "\x1B[1m\x1B[34m";
                case ConsoleColor.Cyan:
                    return "\x1B[1m\x1B[36m";
                case ConsoleColor.DarkBlue:
                    return "\x1B[34m";
                case ConsoleColor.DarkCyan:
                    return "\x1B[36m";
                case ConsoleColor.DarkGray:
                    return "\x1B[1m\x1B[30m";
                case ConsoleColor.DarkGreen:
                    return "\x1B[32m";    
                case ConsoleColor.DarkMagenta:
                    return "\x1B[35m";
                case ConsoleColor.DarkRed:
                    return "\x1B[31m";
                case ConsoleColor.DarkYellow:
                    return "\x1B[33m";
                case ConsoleColor.Gray:
                    return "\x1B[37m";
                case ConsoleColor.Green:
                    return "\x1B[1m\x1B[32m";
                case ConsoleColor.Magenta:
                    return "\x1B[1m\x1B[35m";
                case ConsoleColor.Red:
                    return "\x1B[1m\x1B[31m";
                case ConsoleColor.White:
                    return "\x1B[1m\x1B[37m";
                case ConsoleColor.Yellow:
                    return "\x1B[1m\x1B[33m";
                default:
                    return TerminalDefaultForegroundColorEscapeCode; // default foreground color
            }
        }
        
        internal static string TerminalDefaultForegroundColorEscapeCode
        {
            get { return "\x1B[39m\x1B[22m"; }
        }

        internal static string GetBackgroundColorEscapeCode(ConsoleColor color)
        {
            switch (color)
            {
                case ConsoleColor.Black:
                    return "\x1B[40m";
                case ConsoleColor.Blue:
                    return "\x1B[104m";
                case ConsoleColor.Cyan:
                    return "\x1B[106m";
                case ConsoleColor.DarkBlue:
                    return "\x1B[44m";
                case ConsoleColor.DarkCyan:
                    return "\x1B[46m";
                case ConsoleColor.DarkGray:
                    return "\x1B[100m";
                case ConsoleColor.DarkGreen:
                    return "\x1B[42m";    
                case ConsoleColor.DarkMagenta:
                    return "\x1B[45m";
                case ConsoleColor.DarkRed:
                    return "\x1B[41m";
                case ConsoleColor.DarkYellow:
                    return "\x1B[43m";
                case ConsoleColor.Gray:
                    return "\x1B[47m";
                case ConsoleColor.Green:
                    return "\x1B[102m";    
                case ConsoleColor.Magenta:
                    return "\x1B[105m";
                case ConsoleColor.Red:
                    return "\x1B[101m";
                case ConsoleColor.White:
                    return "\x1B[107m";
                case ConsoleColor.Yellow:
                    return "\x1B[103m";
                default:
                    return TerminalDefaultBackgroundColorEscapeCode;
            }
        }
        
        internal static string TerminalDefaultBackgroundColorEscapeCode
        {
            get { return "\x1B[49m"; }
        }

        /// <summary>
        /// Resets both foreground and background color.
        /// </summary>
        internal static string TerminalDefaultColorEscapeCode
        {
            get { return "\x1B[0m"; }
        }
    }
}

#endif