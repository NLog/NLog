//
// Copyright (c) 2004-2024 Jaroslaw Kowalski <jaak@jkowalski.net>, Kim Christensen, Julian Verdurmen
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
        private const string LoggedKey = "NLog.ExceptionLoggedToInternalLogger";

        /// <summary>
        /// Mark this exception as logged to the <see cref="InternalLogger"/>.
        /// </summary>
        /// <param name="exception"></param>
        /// <returns></returns>
        public static void MarkAsLoggedToInternalLogger(this Exception exception)
        {
            if (exception != null)
            {
                exception.Data[LoggedKey] = true;
            }
        }

        /// <summary>
        /// Is this exception logged to the <see cref="InternalLogger"/>?
        /// </summary>
        /// <param name="exception"></param>
        /// <returns><see langword="true"/> if the <paramref name="exception"/> has been logged to the <see cref="InternalLogger"/>.</returns>
        public static bool IsLoggedToInternalLogger(this Exception exception)
        {
            if (exception?.Data?.Count > 0)
            {
                return exception.Data[LoggedKey] as bool? ?? false;
            }
            return false;
        }

        /// <summary>
        /// Determines whether the exception must be rethrown and logs the error to the <see cref="InternalLogger"/> if <see cref="IsLoggedToInternalLogger"/> is <see langword="false"/>.
        ///
        /// Advised to log first the error to the <see cref="InternalLogger"/> before calling this method.
        /// </summary>
        /// <param name="exception">The exception to check.</param>
        /// <param name="loggerContext">Target Object context of the exception.</param>
        /// <param name="callerMemberName">Target Method context of the exception.</param>
        /// <returns><see langword="true"/> if the <paramref name="exception"/> must be rethrown, <see langword="false"/> otherwise.</returns>
        public static bool MustBeRethrown(this Exception exception, IInternalLoggerContext? loggerContext = null, string? callerMemberName = null)
        {
            if (exception.MustBeRethrownImmediately())
            {
                //no further logging, because it can make severe exceptions only worse.
                return true;
            }

            var isConfigError = exception is NLogConfigurationException;
            var logFactory = loggerContext?.LogFactory;
            var throwExceptionsAll = logFactory?.ThrowExceptions == true || LogManager.ThrowExceptions;
            var shallRethrow = isConfigError ? (logFactory?.ThrowConfigExceptions ?? LogManager.ThrowConfigExceptions ?? throwExceptionsAll) : throwExceptionsAll;

            //we throw always configuration exceptions (historical)
            if (!exception.IsLoggedToInternalLogger())
            {
                var level = shallRethrow ? LogLevel.Error : LogLevel.Warn;
                if (loggerContext != null)
                {
                    if (string.IsNullOrEmpty(callerMemberName))
                        InternalLogger.Log(exception, level, "{0}: Error has been raised.", loggerContext);
                    else
                        InternalLogger.Log(exception, level, "{0}: Exception in {1}", loggerContext, callerMemberName);
                }
                else
                {
                    InternalLogger.Log(exception, level, "Error has been raised.");
                }
            }

            return shallRethrow;
        }

        /// <summary>
        /// Determines whether the exception must be rethrown immediately, without logging the error to the <see cref="InternalLogger"/>.
        ///
        /// Only used this method in special cases.
        /// </summary>
        /// <param name="exception">The exception to check.</param>
        /// <returns><see langword="true"/> if the <paramref name="exception"/> must be rethrown, <see langword="false"/> otherwise.</returns>
        public static bool MustBeRethrownImmediately(this Exception exception)
        {
            if (exception is StackOverflowException)
            {
                return true; // StackOverflowException cannot be caught since .NetFramework 2.0
            }

            if (exception is ThreadAbortException)
            {
                return true; // ThreadAbortException will automatically be rethrown at end of catch-block
            }

            if (exception is OutOfMemoryException)
            {
                return true;
            }

#if DEBUG
            if (exception is InvalidCastException)
            {
                return true;
            }
            if (exception is NullReferenceException)
            {
                return true;
            }
            if (exception is ArgumentNullException)
            {
                return true;
            }
            if (exception is ArgumentOutOfRangeException)
            {
                return true;
            }
            if (exception is DivideByZeroException)
            {
                return true;
            }
            if (exception is OverflowException)
            {
                return true;
            }
            if (exception is System.Net.WebException)
            {
                return false;   // Not a real InvalidOperationException
            }
            if (exception is InvalidOperationException)
            {
                return true;    // Ex. Collection was modified
            }
            if (exception is IndexOutOfRangeException)
            {
                return true;
            }
            if (exception is System.Reflection.TargetInvocationException)
            {
                return true;    // Compiler/reflection exception
            }
#endif
            return false;
        }
    }
}
