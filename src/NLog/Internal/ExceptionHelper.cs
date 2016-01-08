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
    using Common;
    using System.Threading;

    /// <summary>
    /// Helper class for dealing with exceptions.
    /// </summary>
    internal static class ExceptionHelper
    {
        /// <summary>
        /// Determines whether the exception must be rethrown.
        /// </summary>
        /// <param name="exception">The exception to check.</param>
        /// <returns><c>true</c>if the <paramref name="exception"/> must be rethrown, <c>false</c> otherwise.</returns>
        public static bool MustBeRethrown(this Exception exception)
        {
            if (exception.MustBeRethrownImmediately())
            {
                //no futher logging, because it can make servere exceptions only worse.
                return true;
            }

            var isConfigError = exception is NLogConfigurationException
                              || exception.GetType().IsSubclassOf(typeof(NLogConfigurationException));

            //we throw always configuration exceptions (historical)
            var shallRethrow = LogManager.ThrowExceptions || isConfigError;

            var level = isConfigError ? LogLevel.Warn : LogLevel.Error;

            InternalLogger.Log(exception, level, "Error has been raised.");

            return shallRethrow;
        }

        /// <summary>
        /// Determines whether the exception must be rethrown. Never logs to the <see cref="InternalLogger"/>.
        /// </summary>
        /// <param name="exception">The exception to check.</param>
        /// <returns><c>true</c>if the <paramref name="exception"/> must be rethrown, <c>false</c> otherwise.</returns>
        public static bool MustBeRethrownImmediately(this Exception exception)
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

            if (exception.GetType().IsSubclassOf(typeof(NLogConfigurationException)))
            {
                return true;
            }

            return false;
        }
    }
}
