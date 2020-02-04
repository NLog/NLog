// 
// Copyright (c) 2004-2020 Jaroslaw Kowalski <jaak@jkowalski.net>, Kim Christensen, Julian Verdurmen
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

namespace NLog.Config
{
    using System;

    /// <summary>
    /// Default filtering with static level config
    /// </summary>
    internal class LoggingRuleLevelFilter : ILoggingRuleLevelFilter
    {
        public static readonly ILoggingRuleLevelFilter Off = new LoggingRuleLevelFilter();
        public bool[] LogLevels { get; }

        public LoggingRuleLevelFilter(bool[] logLevels = null)
        {
            LogLevels = new bool[LogLevel.MaxLevel.Ordinal + 1];
            if (logLevels != null)
            {
                for (int i = 0; i < Math.Min(logLevels.Length, LogLevels.Length); ++i)
                    LogLevels[i] = logLevels[i];
            }
        }

        public LoggingRuleLevelFilter GetSimpleFilterForUpdate()
        {
            if (!ReferenceEquals(LogLevels, Off.LogLevels))
                return this;
            return new LoggingRuleLevelFilter();
        }

        public LoggingRuleLevelFilter SetLoggingLevels(LogLevel minLevel, LogLevel maxLevel, bool enable)
        {
            for (int i = minLevel.Ordinal; i <= Math.Min(maxLevel.Ordinal, LogLevels.Length - 1); ++i)
                LogLevels[i] = enable;
            return this;
        }
    }
}
