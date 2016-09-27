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
using System.Text;
using NLog.Common;

namespace NLog.Internal.PoolFactory
{
    internal class LogEventObjectFactory : ILogEventObjectFactory
    {
        static ILogEventObjectFactory _instance = new LogEventObjectFactory();
        public static ILogEventObjectFactory Instance { get { return _instance; } }

        public PoolSetup PoolSetup { get { return PoolSetup.None; } }

        private LogEventObjectFactory()
        {
        }

        public LogEventInfo CreateLogEvent(LogLevel level, string loggerName, IFormatProvider formatProvider, [Localizable(false)] string message, object[] parameters, Exception exception)
        {
            return LogEventInfo.Create(level, loggerName, exception, formatProvider, message, parameters);
        }

        void ILogEventObjectFactory.ReleaseLogEvent(LogEventInfo item)
        {
            // No pooling
        }

        public StringBuilderTarget CreateStringBuilder(int capacity = 0)
        {
            if (capacity < 256)
                capacity = capacity > 0 ? 256 : 0;
            return new StringBuilderTarget(new StringBuilder(capacity));
        }

        void ILogEventObjectFactory.ReleaseStringBuilder(StringBuilderTarget item)
        {
            // No pooling
        }

        public MemoryStreamTarget CreateMemoryStream(int capacity = 0)
        {
            if (capacity < 1024)
                capacity = capacity > 0 ? 1024 : 0;
            return new MemoryStreamTarget(new System.IO.MemoryStream(capacity));
        }

        public void ReleaseMemoryStream(MemoryStreamTarget item)
        {
            if (item != null && item.Result != null)
                item.Result.Dispose();
        }

        public SingleCallContinuation CreateSingleCallContinuation(AsyncContinuation asyncContinuation)
        {
            return new SingleCallContinuation(asyncContinuation);
        }

        void ILogEventObjectFactory.ReleaseSingleCallContinuation(SingleCallContinuation item)
        {
            // No pooling
        }

        public ExceptionHandlerContinuation CreateExceptionHandlerContinuation(int originalThreadId, bool throwExceptions)
        {
            return new ExceptionHandlerContinuation(originalThreadId, throwExceptions);
        }

        void ILogEventObjectFactory.ReleaseExceptionHandlerContinuation(ExceptionHandlerContinuation item)
        {
            // No pooling
        }

        public CompleteWhenAllContinuation CreateCompleteWhenAllContinuation()
        {
            return new CompleteWhenAllContinuation();
        }

        void ILogEventObjectFactory.ReleaseCompleteWhenAllContinuation(CompleteWhenAllContinuation item)
        {
            // No pooling
        }

        public AsyncLogEventArray CreateAsyncLogEventArray(int capacity = 0)
        {
            return new AsyncLogEventArray(capacity);
        }

        void ILogEventObjectFactory.ReleaseAsyncLogEventArray(AsyncLogEventArray item)
        {
            // No pooling
        }

        void ILogEventObjectFactory.GetPoolsStats(StringBuilder builder)
        {
            // No stats
        }
    }
}
