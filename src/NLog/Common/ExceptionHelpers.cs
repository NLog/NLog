// 
// Copyright (c) 2004-2010 Jaroslaw Kowalski <jaak@jkowalski.net>
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

namespace NLog.Common
{
    using System;

    /// <summary>
    /// Helper functions for handling exceptions.
    /// </summary>
    public static class ExceptionHelpers
    {
        /// <summary>
        /// Function which returns the specified data type.
        /// </summary>
        /// <typeparam name="T">Function return type.</typeparam>
        /// <returns>Value of T.</returns>
        /// <remarks>This is needed temporarily because Func type is not available in .NET 2.0</remarks>
        public delegate T Func<T>();

        /// <summary>
        /// Tries to evaluate function, returns the default on exception.
        /// </summary>
        /// <typeparam name="T">Function return type.</typeparam>
        /// <param name="function">The function.</param>
        /// <param name="errorMessage">The error message to be logged when the function throws.</param>
        /// <param name="defaultValue">The default value.</param>
        /// <returns>Value of the function or the default value in case the function throws an exception.</returns>
        public static T ReturnDefaultOnException<T>(Func<T> function, string errorMessage, T defaultValue)
        {
            try
            {
                return function();
            }
            catch (Exception ex)
            {
                InternalLogger.Error(errorMessage, ex);
                return defaultValue;
            }
        }
    }
}
