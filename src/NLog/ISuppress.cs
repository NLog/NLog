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

namespace NLog
{
    using System;
#if ASYNC_SUPPORTED
    using System.Threading.Tasks;
#endif

    /// <summary>
    /// Provides an interface to execute System.Actions without surfacing any exceptions raised for that action.
    /// </summary>
    public interface ISuppress
    {
        /// <summary>
        /// Runs action. If the action throws, the exception is logged at <c>Error</c> level. Exception is not propagated outside of this method.
        /// </summary>
        /// <param name="action">Action to execute.</param>
        void Swallow(Action action);

        /// <summary>
        /// Runs the provided function and returns its result. If exception is thrown, it is logged at <c>Error</c> level.
        /// Exception is not propagated outside of this method. Fallback value is returned instead.
        /// </summary>
        /// <typeparam name="T">Return type of the provided function.</typeparam>
        /// <param name="func">Function to run.</param>
        /// <returns>Result returned by the provided function or fallback value in case of exception.</returns>
        T Swallow<T>(Func<T> func);

        /// <summary>
        /// Runs the provided function and returns its result. If exception is thrown, it is logged at <c>Error</c> level.
        /// Exception is not propagated outside of this method. Fallback value is returned instead.
        /// </summary>
        /// <typeparam name="T">Return type of the provided function.</typeparam>
        /// <param name="func">Function to run.</param>
        /// <param name="fallback">Fallback value to return in case of exception. Defaults to default value of type T.</param>
        /// <returns>Result returned by the provided function or fallback value in case of exception.</returns>
        T Swallow<T>(Func<T> func, T fallback);

#if ASYNC_SUPPORTED
        /// <summary>
        /// Runs async action. If the action throws, the exception is logged at <c>Error</c> level. Exception is not propagated outside of this method.
        /// </summary>
        /// <param name="asyncAction">Async action to execute.</param>
        Task SwallowAsync(Func<Task> asyncAction);

        /// <summary>
        /// Runs the provided async function and returns its result. If exception is thrown, it is logged at <c>Error</c> level.
        /// Exception is not propagated outside of this method. Fallback value is returned instead.
        /// </summary>
        /// <typeparam name="T">Return type of the provided function.</typeparam>
        /// <param name="asyncFunc">Async function to run.</param>
        /// <returns>Result returned by the provided function or fallback value in case of exception.</returns>
        Task<T> SwallowAsync<T>(Func<Task<T>> asyncFunc);

        /// <summary>
        /// Runs the provided async function and returns its result. If exception is thrown, it is logged at <c>Error</c> level.
        /// Exception is not propagated outside of this method. Fallback value is returned instead.
        /// </summary>
        /// <typeparam name="T">Return type of the provided function.</typeparam>
        /// <param name="asyncFunc">Async function to run.</param>
        /// <param name="fallback">Fallback value to return in case of exception. Defaults to default value of type T.</param>
        /// <returns>Result returned by the provided function or fallback value in case of exception.</returns>
        Task<T> SwallowAsync<T>(Func<Task<T>> asyncFunc, T fallback);
#endif
    }
}
