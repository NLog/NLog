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

namespace NLog
{
    using System.IO;
    using NLog.Common;
    using NLog.Config;

    /// <summary>
    /// Extension methods to setup NLog <see cref="InternalLogger"/> options
    /// </summary>
    public static class SetupInternalLoggerBuilderExtensions
    {
        /// <summary>
        /// Configures <see cref="InternalLogger.LogLevel"/>
        /// </summary>
        public static ISetupInternalLoggerBuilder SetMinimumLogLevel(this ISetupInternalLoggerBuilder setupBuilder, LogLevel logLevel)
        {
            var orgValue = InternalLogger.LogLevel;
            InternalLogger.LogLevel = logLevel;
            if (InternalLogger.LogLevel != LogLevel.Off && orgValue == LogLevel.Off)
            {
                var orgLogFile = InternalLogger.LogFile;
                if (!string.IsNullOrEmpty(orgLogFile))
                {
                    InternalLogger.LogFile = orgLogFile;    // InternalLogger.LogFile property-setter skips directory creation when LogLevel.Off
                }
            }
            return setupBuilder;
        }

        /// <summary>
        /// Configures <see cref="InternalLogger.LogFile"/>
        /// </summary>
        public static ISetupInternalLoggerBuilder LogToFile(this ISetupInternalLoggerBuilder setupBuilder, string fileName)
        {
            InternalLogger.LogFile = fileName;
            return setupBuilder;
        }

        /// <summary>
        /// Configures <see cref="InternalLogger.LogToConsole"/>
        /// </summary>
        public static ISetupInternalLoggerBuilder LogToConsole(this ISetupInternalLoggerBuilder setupBuilder, bool enabled)
        {
            InternalLogger.LogToConsole = enabled;
            return setupBuilder;
        }

        /// <summary>
        /// Configures <see cref="InternalLogger.LogWriter"/>
        /// </summary>
        public static ISetupInternalLoggerBuilder LogToWriter(this ISetupInternalLoggerBuilder setupBuilder, TextWriter writer)
        {
            InternalLogger.LogWriter = writer;
            return setupBuilder;
        }

        /// <summary>
        /// Configures <see cref="InternalLogger.InternalEventOccurred"/>
        /// </summary>
        public static ISetupInternalLoggerBuilder AddEventSubscription(this ISetupInternalLoggerBuilder setupBuilder, InternalEventOccurredHandler eventSubscriber)
        {
            InternalLogger.InternalEventOccurred += eventSubscriber;
            return setupBuilder;
        }

        /// <summary>
        /// Configures <see cref="InternalLogger.InternalEventOccurred"/>
        /// </summary>
        public static ISetupInternalLoggerBuilder RemoveEventSubscription(this ISetupInternalLoggerBuilder setupBuilder, InternalEventOccurredHandler eventSubscriber)
        {
            InternalLogger.InternalEventOccurred -= eventSubscriber;
            return setupBuilder;
        }
    }
}
