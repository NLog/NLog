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
using System.ComponentModel;
using NLog.Common;

namespace NLog.Internal.PoolFactory
{
    /// <summary>
    /// Object-Factory-Inteface for creating temporary objects needed
    /// when logging <see cref="LogEventInfo"/>-objects
    /// </summary>
    internal interface ILogEventObjectFactory
    {
        /// <summary>
        /// Current pool configuration used by this object factory
        /// </summary>
        PoolSetup PoolSetup { get; }

        /// <summary>
        /// Outputs the <see cref="Logger"/>s and <see cref="Targets.Target"/>s and their pool usage
        /// </summary>
        /// <param name="builder"></param>
        void GetPoolsStats(System.Text.StringBuilder builder);

        /// <summary>
        /// Factory method for <see cref="LogEventInfo"/> 
        /// </summary>
        LogEventInfo CreateLogEvent(LogLevel level, string loggerName, IFormatProvider formatProvider, [Localizable(false)] string message, object[] parameters, Exception exception);

        /// <summary>
        /// Put back <see cref="LogEventInfo"/> into object pool after usage for reuse
        /// </summary>
        void ReleaseLogEvent(LogEventInfo item);

        /// <summary>
        /// Factory method for <see cref="ReusableStringBuilder"/> 
        /// </summary>
        ReusableStringBuilder CreateStringBuilder(int capacity = 0);

        /// <summary>
        /// Put back <see cref="ReusableStringBuilder"/> into object pool after usage for reuse
        /// </summary>
        void ReleaseStringBuilder(ReusableStringBuilder item);

        /// <summary>
        /// Factory method for <see cref="ReusableMemoryStream"/> 
        /// </summary>
        ReusableMemoryStream CreateMemoryStream(int capacity = 0);

        /// <summary>
        /// Put back <see cref="ReusableMemoryStream"/> into object pool after usage for reuse
        /// </summary>
        void ReleaseMemoryStream(ReusableMemoryStream item);

        /// <summary>
        /// Factory method for <see cref="ReusableAsyncLogEventInfoArray"/> 
        /// </summary>
        ReusableAsyncLogEventInfoArray CreateAsyncLogEventArray(int capacity = 0);

        /// <summary>
        /// Put back <see cref="ReusableAsyncLogEventInfoArray"/> into object pool after usage for reuse
        /// </summary>
        void ReleaseAsyncLogEventArray(ReusableAsyncLogEventInfoArray item);

        /// <summary>
        /// Factory method for <see cref="ExceptionHandlerContinuation"/> 
        /// </summary>
        ExceptionHandlerContinuation CreateExceptionHandlerContinuation(int originalThreadId, bool throwExceptions);

        /// <summary>
        /// Put back <see cref="ExceptionHandlerContinuation"/> into object pool after usage for reuse
        /// </summary>
        void ReleaseExceptionHandlerContinuation(ExceptionHandlerContinuation item);

        /// <summary>
        /// Factory method for <see cref="CompleteWhenAllContinuation"/> 
        /// </summary>
        CompleteWhenAllContinuation CreateCompleteWhenAllContinuation(CompleteWhenAllContinuation.Counter externalCounter = null);

        /// <summary>
        /// Put back <see cref="CompleteWhenAllContinuation"/> into object pool after usage for reuse
        /// </summary>
        void ReleaseCompleteWhenAllContinuation(CompleteWhenAllContinuation item);
    }
}
