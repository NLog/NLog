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
    using System.Runtime.InteropServices;
    using NLog.Common;
    using NLog.Config;

    /// <summary>
    /// NLog COM Interop LogManager implementation.
    /// </summary>
    [ComVisible(true)]
    [ProgId("NLog.LogManager")]
    [Guid("9a7e8d84-72e4-478a-9a05-23c7ef0cfca8")]
    [ClassInterface(ClassInterfaceType.None)]
    public class ComLogManager : IComLogManager
    {
        /// <summary>
        /// Gets or sets a value indicating whether to log internal messages to the console.
        /// </summary>
        /// <value>
        /// A value of <c>true</c> if internal messages should be logged to the console; otherwise, <c>false</c>.
        /// </value>
        public bool InternalLogToConsole
        {
            get { return InternalLogger.LogToConsole; }
            set { InternalLogger.LogToConsole = value; }
        }

        /// <summary>
        /// Gets or sets the name of the internal log level.
        /// </summary>
        /// <value></value>
        public string InternalLogLevel
        {
            get { return InternalLogger.LogLevel.ToString(); }
            set { InternalLogger.LogLevel = NLog.LogLevel.FromString(value); }
        }

        /// <summary>
        /// Gets or sets the name of the internal log file.
        /// </summary>
        /// <value></value>
        public string InternalLogFile
        {
            get { return InternalLogger.LogFile; }
            set { InternalLogger.LogFile = value; }
        }

        /// <summary>
        /// Creates the specified logger object and assigns a LoggerName to it.
        /// </summary>
        /// <param name="loggerName">The name of the logger.</param>
        /// <returns>The new logger instance.</returns>
        public IComLogger GetLogger(string loggerName)
        {
            IComLogger logger = new ComLogger
            {
                LoggerName = loggerName
            };

            return logger;
        }

        /// <summary>
        /// Loads NLog configuration from the specified file.
        /// </summary>
        /// <param name="fileName">The name of the file to load NLog configuration from.</param>
        public void LoadConfigFromFile(string fileName)
        {
            LogManager.Configuration = new XmlLoggingConfiguration(fileName);
        }
    }
}

#endif