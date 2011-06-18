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

#if !NET_CF && !SILVERLIGHT

namespace NLog.ComInterop
{
    using System.Diagnostics.CodeAnalysis;
    using System.Runtime.InteropServices;

    /// <summary>
    /// NLog COM Interop LogManager interface.
    /// </summary>
    [Guid("7ee3af3b-ba37-45b6-8f5d-cc23bb46c698")]
    [InterfaceType(ComInterfaceType.InterfaceIsDual)]
    [ComVisible(true)]
    public interface IComLogManager
    {
        /// <summary>
        /// Loads NLog configuration from the specified file.
        /// </summary>
        /// <param name="fileName">The name of the file to load NLog configuration from.</param>
        [SuppressMessage("Microsoft.StyleCop.CSharp.OrderingRules", "SA1201:ElementsMustAppearInTheCorrectOrder",
            Justification = "Cannot change this, this is for backwards compatibility.")]
        void LoadConfigFromFile(string fileName);

        /// <summary>
        /// Gets or sets a value indicating whether internal messages should be written to the console.
        /// </summary>
        bool InternalLogToConsole { get; set; }

        /// <summary>
        /// Gets or sets the name of the internal log file.
        /// </summary>
        string InternalLogFile { get; set; }

        /// <summary>
        /// Gets or sets the name of the internal log level.
        /// </summary>
        string InternalLogLevel { get; set; }

        /// <summary>
        /// Creates the specified logger object and assigns a LoggerName to it.
        /// </summary>
        /// <param name="loggerName">Logger name.</param>
        /// <returns>The new logger instance.</returns>
        IComLogger GetLogger(string loggerName);
    }
}

#endif