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
    using JetBrains.Annotations;

    /// <summary>
    /// Helper class for dealing with exceptions.
    /// </summary>
    internal static class ExceptionHelper
    {
        /// <summary>
        /// Determines whether the exception is so serious, that it should always
        /// be thrown and rather not be logged.
        /// </summary>
        /// <param name="exception">The exception to check.</param>
        /// <returns><c>true</c>, if the exception is considered serious.</returns>
        public static bool IsServereException(this Exception exception)
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

            return false;
        }

        /// <summary>
        /// Determines whether the exception must be rethrown
        /// and logs an non severe exception message to the internal logger.
        /// 
        /// NLog Configuration errors are logged as warning, all others as error.
        /// </summary>
        /// <param name="exception">The exception.</param>
        /// <returns>
        /// True if the exception must be rethrown, false otherwise.
        /// </returns>
        public static bool MustBeRethrown(this Exception exception)
        {
            return MustBeRethrown(exception, null, null);
        }

        /// <summary>
        /// Determines whether the exception must be rethrown
        /// and optionally logs a message into internal logger
        /// for an non severe exception.
        /// 
        /// NLog Configuration errors are logged as warning, all others as error.
        /// </summary>
        /// <param name="exception">The exception.</param>
        /// <param name="logMessage">
        /// An optional log message, if <c>logMessage</c> is set to a certain level.
        /// </param>
        /// <returns>
        /// <c>True</c> if the exception must be rethrown, <c>false</c> otherwise.
        /// </returns>
        public static bool MustBeRethrown(this Exception exception, string logMessage)
        {
            return MustBeRethrown(exception, logMessage, null);
        }

        /// <summary>
        /// Determines whether the exception must be rethrown
        /// and optionally logs a message into internal logger
        /// for an non severe exception.
        /// 
        /// NLog Configuration errors are logged as warning, all others as error.
        /// </summary>
        /// <param name="exception">The exception.</param>
        /// <param name="logMessage">An optional log message, if <c>logMessage</c> is set to a certain level.</param>
        /// <param name="args">Arguments for the format string in <paramref name="logMessage"/>.</param>
        /// <returns>
        /// <c>True</c> if the exception must be rethrown, <c>false</c> otherwise.
        /// </returns>
        [StringFormatMethod("logMessage")]
        public static bool MustBeRethrown(this Exception exception, string logMessage, params object[] args)
        {

            if (exception.IsServereException())
            {
                //no futher logging, because it can make servere exceptions only worse.
                return true;
            }

            var isConfigError = exception is NLogConfigurationException
                              || exception.GetType().IsSubclassOf(typeof(NLogConfigurationException));

            //we throw always configuration exceptions (historical)
            var shallRethrow = LogManager.ThrowExceptions || isConfigError;



            var level = isConfigError ? LogLevel.Warn : LogLevel.Error;

            var exceptionText = exception.ToString();
            if (string.IsNullOrEmpty(logMessage))
            {
                InternalLogger.Log(level, exceptionText);
            }
            else
            {
                InternalLogger.Log(level, string.Format("{0}: {1}", logMessage, exceptionText), args);
            }

            return shallRethrow;
        }
    }
}