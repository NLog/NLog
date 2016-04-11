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
        /// Runs the provided action. If the action throws, the exception is logged at <c>Error</c> level. The exception is not propagated outside of this method.
        /// </summary>
        /// <param name="action">Action to execute.</param>
        void Swallow(Action action);

        /// <summary>
        /// Runs the provided function and returns its result. If an exception is thrown, it is logged at <c>Error</c> level.
        /// The exception is not propagated outside of this method; a default value is returned instead.
        /// </summary>
        /// <typeparam name="T">Return type of the provided function.</typeparam>
        /// <param name="func">Function to run.</param>
        /// <returns>Result returned by the provided function or the default value of type <typeparamref name="T"/> in case of exception.</returns>
        T Swallow<T>(Func<T> func);

        /// <summary>
        /// Runs the provided function and returns its result. If an exception is thrown, it is logged at <c>Error</c> level.
        /// The exception is not propagated outside of this method; a fallback value is returned instead.
        /// </summary>
        /// <typeparam name="T">Return type of the provided function.</typeparam>
        /// <param name="func">Function to run.</param>
        /// <param name="fallback">Fallback value to return in case of exception.</param>
        /// <returns>Result returned by the provided function or fallback value in case of exception.</returns>
        T Swallow<T>(Func<T> func, T fallback);

#if ASYNC_SUPPORTED
        /// <summary>
        /// Logs an exception is logged at <c>Error</c> level if the provided task does not run to completion.
        /// </summary>
        /// <param name="task">The task for which to log an error if it does not run to completion.</param>
        /// <remarks>This method is useful in fire-and-forget situations, where application logic does not depend on completion of task. This method is avoids C# warning CS4014 in such situations.</remarks>
        void Swallow(Task task);

        /// <summary>
        /// Returns a task that completes when a specified task to completes. If the task does not run to completion, an exception is logged at <c>Error</c> level. The returned task always runs to completion.
        /// </summary>
        /// <param name="task">The task for which to log an error if it does not run to completion.</param>
        /// <returns>A task that completes in the <see cref="TaskStatus.RanToCompletion"/> state when <paramref name="task"/> completes.</returns>
        Task SwallowAsync(Task task);

        /// <summary>
        /// Runs async action. If the action throws, the exception is logged at <c>Error</c> level. The exception is not propagated outside of this method.
        /// </summary>
        /// <param name="asyncAction">Async action to execute.</param>
        /// <returns>A task that completes in the <see cref="TaskStatus.RanToCompletion"/> state when <paramref name="asyncAction"/> completes.</returns>

        Task SwallowAsync(Func<Task> asyncAction);

        /// <summary>
        /// Runs the provided async function and returns its result. If the task does not run to completion, an exception is logged at <c>Error</c> level.
        /// The exception is not propagated outside of this method; a default value is returned instead.
        /// </summary>
        /// <typeparam name="TResult">Return type of the provided function.</typeparam>
        /// <param name="asyncFunc">Async function to run.</param>
        /// <returns>A task that represents the completion of the supplied task. If the supplied task ends in the <see cref="TaskStatus.RanToCompletion"/> state, the result of the new task will be the result of the supplied task; otherwise, the result of the new task will be the default value of type <typeparamref name="TResult"/>.</returns>
        Task<TResult> SwallowAsync<TResult>(Func<Task<TResult>> asyncFunc);

        /// <summary>
        /// Runs the provided async function and returns its result. If the task does not run to completion, an exception is logged at <c>Error</c> level.
        /// The exception is not propagated outside of this method; a fallback value is returned instead.
        /// </summary>
        /// <typeparam name="TResult">Return type of the provided function.</typeparam>
        /// <param name="asyncFunc">Async function to run.</param>
        /// <param name="fallback">Fallback value to return if the task does not end in the <see cref="TaskStatus.RanToCompletion"/> state.</param>
        /// <returns>A task that represents the completion of the supplied task. If the supplied task ends in the <see cref="TaskStatus.RanToCompletion"/> state, the result of the new task will be the result of the supplied task; otherwise, the result of the new task will be the fallback value.</returns>
        Task<TResult> SwallowAsync<TResult>(Func<Task<TResult>> asyncFunc, TResult fallback);
#endif
    }
}
