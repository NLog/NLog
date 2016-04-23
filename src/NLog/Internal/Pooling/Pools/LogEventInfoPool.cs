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

namespace NLog.Internal.Pooling.Pools
{
    internal class LogEventInfoPool : PoolBaseOfT<LogEventInfo>
    {
        /// <summary>
        /// Initializes an instance of the <see cref="LogEventInfoPool"/>
        /// </summary>
        /// <param name="poolSize">Size of the pool.</param>
        /// <param name="preFill">Whether or not to prefill the pool</param>
        public LogEventInfoPool(int poolSize, bool preFill = false) : 
            base(poolSize, preFill)
        {
        }

        /// <summary>
        /// Factory method that creates an instance of the pooled type.
        /// </summary>
        /// <returns>An instance of the pooled type</returns>
        protected override LogEventInfo Factory()
        {
            return new LogEventInfo(this);
        }

        /// <summary>
        /// Specialized method where all needed parameters for the LogEventInfo class can be sent.
        /// </summary>
        /// <param name="level">The LogLevel for the log event</param>
        /// <param name="loggerName">The logger name</param>
        /// <param name="formatProvider">The format provider.</param>
        /// <param name="message">The log message.</param>
        /// <param name="parameters">Any parameters for the log message if its a format string.</param>
        /// <param name="exception">The exception if any.</param>
        /// <returns>A poolsed LogEventInfo with the given information.</returns>
        public LogEventInfo Get(LogLevel level, string loggerName, IFormatProvider formatProvider, [Localizable(false)] string message, object[] parameters, Exception exception)
        {
            var info = this.Get();

            // init as if it was freshly created
            info.Init();
            info.SetPool(this);
            info.Level = level;
            info.LoggerName = loggerName;
            info.FormatProvider = formatProvider;
            info.Message = message;
            info.Parameters = parameters;
            info.Exception = exception;

            info.CalcFormattedMessageIfNeeded();
            return info;
        }

        /// <summary>
        /// Helper method for getting a NullEvent that is used by different parts of the code to render.
        /// </summary>
        /// <returns>A "Null" LogEventInfo.</returns>
        public LogEventInfo GetNullEvent()
        {
            return this.Get(LogLevel.Off, string.Empty, null, string.Empty, null, null);
        }

        /// <summary>
        /// Has to be implemented in implementation.
        /// PoolBase will call this to figure out how big a pool should be dependent on the given configuration.
        /// </summary>
        /// <param name="configuration">The configuration.</param>
        /// <returns>The number of pooled objects this pool should support.</returns>
        protected override int GetPoolSize(PoolConfiguration configuration)
        {
            // use default of 2 times the estimated log events per second, since people might have more than one target that should log an event
            // And we clone a log event per target to prevent threading issues.
            return configuration.EstimatedLogEventsPerSecond * 2;
        }

        /// <summary>
        /// Clears the item if necessary before putting it back into the pool.
        /// </summary>
        /// <param name="item">The item to clear.</param>
        protected override void Clear(LogEventInfo item)
        {
            // Clear all data from the log event
            item.Clear();
        }
    }
}