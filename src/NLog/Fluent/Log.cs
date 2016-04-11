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
using System.Runtime.CompilerServices;

namespace NLog.Fluent
{
#if NET4_5
    /// <summary>
    /// A global logging class using caller info to find the logger.
    /// </summary>
    public static class Log
    {
        private static readonly ILogger _logger = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// Starts building a log event with the specified <see cref="LogLevel" />.
        /// </summary>
        /// <param name="logLevel">The log level.</param>
        /// <param name="callerFilePath">The full path of the source file that contains the caller. This is the file path at the time of compile.</param>
        /// <returns>An instance of the fluent <see cref="LogBuilder"/>.</returns>
        public static LogBuilder Level(LogLevel logLevel, [CallerFilePath]string callerFilePath = null)
        {
            return Create(logLevel, callerFilePath);
        }
        
        /// <summary>
        /// Starts building a log event at the <c>Trace</c> level.
        /// </summary>
        /// <param name="callerFilePath">The full path of the source file that contains the caller. This is the file path at the time of compile.</param>
        /// <returns>An instance of the fluent <see cref="LogBuilder"/>.</returns>
        public static LogBuilder Trace([CallerFilePath]string callerFilePath = null)
        {
            return Create(LogLevel.Trace, callerFilePath);
        }

        /// <summary>
        /// Starts building a log event at the <c>Debug</c> level.
        /// </summary>
        /// <param name="callerFilePath">The full path of the source file that contains the caller. This is the file path at the time of compile.</param>
        /// <returns>An instance of the fluent <see cref="LogBuilder"/>.</returns>
        public static LogBuilder Debug([CallerFilePath]string callerFilePath = null)
        {
            return Create(LogLevel.Debug, callerFilePath);
        }

        /// <summary>
        /// Starts building a log event at the <c>Info</c> level.
        /// </summary>
        /// <param name="callerFilePath">The full path of the source file that contains the caller. This is the file path at the time of compile.</param>
        /// <returns>An instance of the fluent <see cref="LogBuilder"/>.</returns>
        public static LogBuilder Info([CallerFilePath]string callerFilePath = null)
        {
            return Create(LogLevel.Info, callerFilePath);
        }

        /// <summary>
        /// Starts building a log event at the <c>Warn</c> level.
        /// </summary>
        /// <param name="callerFilePath">The full path of the source file that contains the caller. This is the file path at the time of compile.</param>
        /// <returns>An instance of the fluent <see cref="LogBuilder"/>.</returns>
        public static LogBuilder Warn([CallerFilePath]string callerFilePath = null)
        {
            return Create(LogLevel.Warn, callerFilePath);
        }

        /// <summary>
        /// Starts building a log event at the <c>Error</c> level.
        /// </summary>
        /// <param name="callerFilePath">The full path of the source file that contains the caller. This is the file path at the time of compile.</param>
        /// <returns>An instance of the fluent <see cref="LogBuilder"/>.</returns>
        public static LogBuilder Error([CallerFilePath]string callerFilePath = null)
        {
            return Create(LogLevel.Error, callerFilePath);
        }

        /// <summary>
        /// Starts building a log event at the <c>Fatal</c> level.
        /// </summary>
        /// <param name="callerFilePath">The full path of the source file that contains the caller. This is the file path at the time of compile.</param>
        /// <returns>An instance of the fluent <see cref="LogBuilder"/>.</returns>
        public static LogBuilder Fatal([CallerFilePath]string callerFilePath = null)
        {
            return Create(LogLevel.Fatal, callerFilePath);
        }

        private static LogBuilder Create(LogLevel logLevel, string callerFilePath)
        {
            string name = Path.GetFileNameWithoutExtension(callerFilePath ?? string.Empty);
            var logger = string.IsNullOrWhiteSpace(name) ? _logger : LogManager.GetLogger(name);

            var builder = new LogBuilder(logger, logLevel);
            if (callerFilePath != null)
                builder.Property("CallerFilePath", callerFilePath);

            return builder;
        }
    }
#endif
}
