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
    using System;
    using System.Threading;
    using NLog.Common;

    /// <summary>
    /// Helper class for dealing with exceptions.
    /// </summary>
    internal static class ExceptionHelper
    {
        /// <summary>
        /// Determines whether the exception must be rethrown.
        /// </summary>
        /// <param name="exception">The exception.</param>
        /// <returns>True if the exception must be rethrown, false otherwise.</returns>
        public static bool MustBeRethrown(this Exception exception)
        {

            // although seldom thrown, catch it (http://stackoverflow.com/a/1599238/1966710)
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

            if (exception.GetType().IsSubclassOf(typeof(NLogConfigurationException)))
            {
                return true;
            }

            return false;
        }



        /// <summary>
        /// Rethrows the exception if necessary and eventually logs the specified message at error level.
        /// </summary>
        /// <param name="ex">The exception, that occurred.</param>
        public static void HandleException(this Exception ex)
        {
            HandleException(ex, LogLevel.Off, null);
        }

        /// <summary>
        /// Rethrows the exception if necessary and eventually logs the specified message at error level.
        /// </summary>
        /// <param name="ex">The exception, that occurred.</param>
        /// <param name="message">Log message, which may include positional parameters.</param>
        /// <param name="args">Arguments to the log message.</param>
        public static void HandleException(this Exception ex, string message, params object[] args)
        {
            HandleException(ex, LogLevel.Error, message, args);
        }

        /// <summary>
        /// Rethrows the exception, if necessary and eventually logs the specified message.
        /// </summary>
        /// <param name="ex">The exception, that occurred.</param>
        /// <param name="level">Log level for the optional log message.</param>
        /// <param name="message">Log message, which may include positional parameters.</param>
        /// <param name="args">Arguments to the log message.</param>
        public static void HandleException(this Exception ex, LogLevel level, string message, params object[] args)
        {
            if(MustBeRethrown(ex))
            {
                RethrowException(ex);
                return;
            }

            if (!string.IsNullOrEmpty(message))
            {
                InternalLogger.Log(level, message, args);
            }

            if (LogManager.ThrowExceptions)
            {
                RethrowException(ex);
            }

        }

        private static void RethrowException(Exception ex)
        {
            throw new Exception("An error occured, see inner exception for details.", ex);
        }
    }
}
