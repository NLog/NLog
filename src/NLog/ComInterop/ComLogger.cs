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
    using System;
    using System.Runtime.InteropServices;

    using NLog;

    /// <summary>
    /// NLog COM Interop logger implementation.
    /// </summary>
    [ComVisible(true)]
    [ProgId("NLog.Logger")]
    [Guid("181f39a8-41a8-4e35-91b6-5f8d96f5e61c")]
    [ClassInterface(ClassInterfaceType.None)]
    public class ComLogger : IComLogger
    {
        private static readonly Logger DefaultLogger = LogManager.CreateNullLogger();

        private Logger logger = DefaultLogger;
        private string loggerName = string.Empty;

        /// <summary>
        /// Gets a value indicating whether the Trace level is enabled.
        /// </summary>
        /// <value></value>
        public bool IsTraceEnabled
        {
            get { return this.logger.IsTraceEnabled; }
        }

        /// <summary>
        /// Gets a value indicating whether the Debug level is enabled.
        /// </summary>
        /// <value></value>
        public bool IsDebugEnabled
        {
            get { return this.logger.IsDebugEnabled; }
        }

        /// <summary>
        /// Gets a value indicating whether the Info level is enabled.
        /// </summary>
        /// <value></value>
        public bool IsInfoEnabled
        {
            get { return this.logger.IsInfoEnabled; }
        }

        /// <summary>
        /// Gets a value indicating whether the Warn level is enabled.
        /// </summary>
        /// <value></value>
        public bool IsWarnEnabled
        {
            get { return this.logger.IsWarnEnabled; }
        }

        /// <summary>
        /// Gets a value indicating whether the Error level is enabled.
        /// </summary>
        /// <value></value>
        public bool IsErrorEnabled
        {
            get { return this.logger.IsErrorEnabled; }
        }

        /// <summary>
        /// Gets a value indicating whether the Fatal level is enabled.
        /// </summary>
        /// <value></value>
        public bool IsFatalEnabled
        {
            get { return this.logger.IsFatalEnabled; }
        }

        /// <summary>
        /// Gets or sets the logger name.
        /// </summary>
        /// <value></value>
        public string LoggerName
        {
            get
            {
                return this.loggerName;
            }

            set
            {
                this.loggerName = value;
                this.logger = LogManager.GetLogger(value);
            }
        }

        /// <summary>
        /// Writes the diagnostic message at the specified level.
        /// </summary>
        /// <param name="level">The log level.</param>
        /// <param name="message">A <see langword="string"/> to be written.</param>
        public void Log(string level, string message)
        {
            this.logger.Log(LogLevel.FromString(level), message);
        }

        /// <summary>
        /// Writes the diagnostic message at the Trace level.
        /// </summary>
        /// <param name="message">A <see langword="string"/> to be written.</param>
        public void Trace(string message)
        {
            this.logger.Trace(message);
        }

        /// <summary>
        /// Writes the diagnostic message at the Debug level.
        /// </summary>
        /// <param name="message">A <see langword="string"/> to be written.</param>
        public void Debug(string message)
        {
            this.logger.Debug(message);
        }

        /// <summary>
        /// Writes the diagnostic message at the Info level.
        /// </summary>
        /// <param name="message">A <see langword="string"/> to be written.</param>
        public void Info(string message)
        {
            this.logger.Info(message);
        }

        /// <summary>
        /// Writes the diagnostic message at the Warn level.
        /// </summary>
        /// <param name="message">A <see langword="string"/> to be written.</param>
        public void Warn(string message)
        {
            this.logger.Warn(message);
        }

        /// <summary>
        /// Writes the diagnostic message at the Error level.
        /// </summary>
        /// <param name="message">A <see langword="string"/> to be written.</param>
        public void Error(string message)
        {
            this.logger.Error(message);
        }

        /// <summary>
        /// Writes the diagnostic message at the Fatal level.
        /// </summary>
        /// <param name="message">A <see langword="string"/> to be written.</param>
        public void Fatal(string message)
        {
            this.logger.Fatal(message);
        }

        /// <summary>
        /// Checks if the specified log level is enabled.
        /// </summary>
        /// <param name="level">The log level.</param>
        /// <returns>
        /// A value indicating whether the specified log level is enabled.
        /// </returns>
        public bool IsEnabled(string level)
        {
            return this.logger.IsEnabled(LogLevel.FromString(level));
        }
    }
}

#endif