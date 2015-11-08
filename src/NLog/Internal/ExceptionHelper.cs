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

namespace NLog.Internal
{
    using Common;
    using System;
    using System.Threading;

    /// <summary>
    /// Helper class for dealing with exceptions.
    /// </summary>
    internal static class ExceptionHelper
    {
        /// <summary>
        /// Determines whether the exception must be rethrown
        /// and logs an non severe exception message to the internal logger.
        /// </summary>
        /// <param name="exception">The exception.</param>
        /// <returns>
        /// True if the exception must be rethrown, false otherwise.
        /// </returns>
        public static bool MustBeRethrown(this Exception exception)
        {
            return MustBeRethrown(exception, null, null, null);
        }

        /// <summary>
        /// Determines whether the exception must be rethrown
        /// and optionally logs a message into internal logger
        /// for an non severe exception.
        /// </summary>
        /// <param name="exception">The exception.</param>
        /// <param name="logException">Determines, if a message should be written to the internal logger.</param>
        /// <returns>
        /// True if the exception must be rethrown, false otherwise.
        /// </returns>
        public static bool MustBeRethrown(this Exception exception, bool logException)
        {
            return MustBeRethrown(exception, logException ? null : LogLevel.Off, null, null);
        }

        /// <summary>
        /// Determines whether the exception must be rethrown
        /// and optionally logs a message into internal logger
        /// for an non severe exception.
        /// </summary>
        /// <param name="exception">The exception.</param>
        /// <param name="logMessage">
        /// An optional log message, if <c>logMessage</c> is set to a certain level.
        /// </param>
        /// <returns>
        /// True if the exception must be rethrown, false otherwise.
        /// </returns>
        public static bool MustBeRethrown(this Exception exception, string logMessage)
        {
            return MustBeRethrown(exception, null, logMessage, null);
        }

        /// <summary>
        /// Determines whether the exception must be rethrown
        /// and optionally logs a message into internal logger
        /// for an non severe exception.
        /// </summary>
        /// <param name="exception">The exception.</param>
        /// <param name="logMessage">An optional log message, if <c>logMessage</c> is set to a certain level.</param>
        /// <param name="args">Arguments for the format string in <c>logMessage</c>.</param>
        /// <returns>
        /// True if the exception must be rethrown, false otherwise.
        /// </returns>
        public static bool MustBeRethrown(this Exception exception, string logMessage, params object[] args)
        {
            return MustBeRethrown(exception, null, logMessage, args);
        }

        /// <summary>
        /// Determines whether the exception must be rethrown
        /// and optionally logs a message into internal logger
        /// for an non severe exception.
        /// </summary>
        /// <param name="exception">The exception.</param>
        /// <param name="level">Specifies the level to log at (or <see cref="NLog.LogLevel.Off"/> to disable)
        /// If <c>NULL</c>, it's automatically chosen between error (if exception rethrown)
        /// and warning (if not rethrown).
        /// </param>
        /// <param name="logMessage">An optional log message, if <c>logMessage</c> is set to a certain level.</param>
        /// <param name="args">Arguments for the format string in <c>logMessage</c>.</param>
        /// <returns>
        /// True if the exception must be rethrown, false otherwise.
        /// </returns>
        public static bool MustBeRethrown(this Exception exception, LogLevel level, string logMessage, params object[] args)
        {
            if (MustRethrowSevere(exception))
            {
                return true;
            }

            if (exception.GetType().IsSubclassOf(typeof(NLogConfigurationException)))
            {
                return true;
            }

            var shallRethrow = LogManager.ThrowExceptions;

            if(level == null)
            {
                level = shallRethrow ? LogLevel.Error : LogLevel.Warn;

            }

            if (level != LogLevel.Off)
            {
                // only log after the 'serious' exceptions above

                if (string.IsNullOrEmpty(logMessage))
                {
                    InternalLogger.Log(level, exception.Message);
                }
                else
                {
                    InternalLogger.Log(level, logMessage, args);
                }
            }
            return shallRethrow;
        }

        /// <summary>
        /// Determines whether the exception is severe and must be rethrown.
        /// No Logging.
        /// </summary>
        public static bool MustRethrowSevere(this Exception exception)
        {
            if (exception is StackOverflowException)
            {
                return true;
            }

            if (exception is ThreadAbortException)
            {
                return true;
            }

            if (exception is OutOfMemoryException)
            {
                return true;
            }

            if (exception is NLogConfigurationException)
            {
                return true;
            }

            return false;
        }
    }
}