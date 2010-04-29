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
//  this list of conditions and the following disclaimer. 
// 
// * Redistributions in binary form must reproduce the above copyright notice,
//  this list of conditions and the following disclaimer in the documentation
//  and/or other materials provided with the distribution. 
// 
// * Neither the name of Jaroslaw Kowalski nor the names of its 
//  contributors may be used to endorse or promote products derived from this
//  software without specific prior written permission. 
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

#if !NETCF

using System;
using System.Runtime.InteropServices;

using NLog.Targets;

namespace NLog.Win32.Targets
{
    /// <summary>
    /// Colored console output color.
    /// </summary>
    /// <remarks>
    /// Note that this enumeration is defined to be binary compatible with 
    /// .NET 2.0 System.ConsoleColor + some additions
    /// </remarks>
    public enum ConsoleOutputColor
    {
        /// <summary>
        /// Black (#000000)
        /// </summary>
        Black = 0,

        /// <summary>
        /// Dark blue (#000080)
        /// </summary>
        DarkBlue = 1,

        /// <summary>
        /// Dark green (#008000)
        /// </summary>
        DarkGreen = 2,

        /// <summary>
        /// Dark Cyan (#008080)
        /// </summary>
        DarkCyan = 3,

        /// <summary>
        /// Dark Red (#800000)
        /// </summary>
        DarkRed = 4,

        /// <summary>
        /// Dark Magenta (#800080)
        /// </summary>
        DarkMagenta = 5,

        /// <summary>
        /// Dark Yellow (#808000)
        /// </summary>
        DarkYellow = 6,

        /// <summary>
        /// Gray (#C0C0C0)
        /// </summary>
        Gray = 7,

        /// <summary>
        /// Dark Gray (#808080)
        /// </summary>
        DarkGray = 8,
        
        /// <summary>
        /// Blue (#0000FF)
        /// </summary>
        Blue = 9,

        /// <summary>
        /// Green (#00FF00)
        /// </summary>
        Green = 10,

        /// <summary>
        /// Cyan (#00FFFF)
        /// </summary>
        Cyan = 11,

        /// <summary>
        /// Red (#FF0000)
        /// </summary>
        Red = 12,

        /// <summary>
        /// Magenta (#FF00FF)
        /// </summary>
        Magenta = 13,

        /// <summary>
        /// Yellow (#FFFF00)
        /// </summary>
        Yellow = 14,

        /// <summary>
        /// White (#FFFFFF)
        /// </summary>
        White = 15,

        /// <summary>
        /// Don't change the color.
        /// </summary>
        NoChange = 16,
    }
}

#endif