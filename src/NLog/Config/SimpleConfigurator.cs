// 
// Copyright (c) 2004-2006 Jaroslaw Kowalski <jaak@jkowalski.net>
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
using System.Collections;
using System.Collections.Specialized;

using NLog;
using NLog.Targets;
using NLog.Filters;

namespace NLog.Config
{
    /// <summary>
    /// Provides simple programmatic configuration API used for trivial logging cases.
    /// </summary>
    public class SimpleConfigurator
    {
#if !NETCF
        /// <summary>
        /// Configures NLog for console logging so that all messages above and including
        /// the <see cref="LogLevel.Info"/> level are output to the console.
        /// </summary>
        public static void ConfigureForConsoleLogging()
        {
            ConfigureForConsoleLogging(LogLevel.Info);
        }

        /// <summary>
        /// Configures NLog for console logging so that all messages above and including
        /// the specified level are output to the console.
        /// </summary>
        /// <param name="minLevel">The minimal logging level.</param>
        public static void ConfigureForConsoleLogging(LogLevel minLevel)
        {
            ConsoleTarget consoleTarget = new ConsoleTarget();

            LoggingConfiguration config = new LoggingConfiguration();
            LoggingRule rule = new LoggingRule("*", minLevel, consoleTarget);
            config.LoggingRules.Add(rule);
            LogManager.Configuration = config;
        }
#endif

        /// <summary>
        /// Configures NLog for to log to the specified target so that all messages 
        /// above and including the <see cref="LogLevel.Info"/> level are output.
        /// </summary>
        /// <param name="target">The target to log all messages to.</param>
        public static void ConfigureForTargetLogging(Target target)
        {
            ConfigureForTargetLogging(target, LogLevel.Info);
        }

        /// <summary>
        /// Configures NLog for to log to the specified target so that all messages 
        /// above and including the specified level are output.
        /// </summary>
        /// <param name="target">The target to log all messages to.</param>
        /// <param name="minLevel">The minimal logging level.</param>
        public static void ConfigureForTargetLogging(Target target, LogLevel minLevel)
        {
            LoggingConfiguration config = new LoggingConfiguration();
            LoggingRule rule = new LoggingRule("*", minLevel, target);
            config.LoggingRules.Add(rule);
            LogManager.Configuration = config;
        }

        /// <summary>
        /// Configures NLog for file logging so that all messages above and including
        /// the <see cref="LogLevel.Info"/> level are written to the specified file.
        /// </summary>
        /// <param name="fileName">Log file name.</param>
        public static void ConfigureForFileLogging(string fileName)
        {
            ConfigureForFileLogging(fileName, LogLevel.Info);
        }

        /// <summary>
        /// Configures NLog for file logging so that all messages above and including
        /// the specified level are written to the specified file.
        /// </summary>
        /// <param name="fileName">Log file name.</param>
        /// <param name="minLevel">The minimal logging level.</param>
        public static void ConfigureForFileLogging(string fileName, LogLevel minLevel)
        {
            FileTarget target = new FileTarget();
            target.FileName = fileName;
            ConfigureForTargetLogging(target, minLevel);
        }
    }
}
