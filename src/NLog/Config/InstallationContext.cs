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

namespace NLog.Config
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Specialized;
    using System.ComponentModel;
    using System.Globalization;
    using System.IO;

    /// <summary>
    /// Provides context for install/uninstall operations.
    /// </summary>
    public sealed class InstallationContext : IDisposable
    {
#if !SILVERLIGHT
        /// <summary>
        /// Mapping between log levels and console output colors.
        /// </summary>
        private static readonly Dictionary<LogLevel, ConsoleColor> logLevel2ConsoleColor = new Dictionary<LogLevel, ConsoleColor>()
        {
            { LogLevel.Trace, ConsoleColor.DarkGray },
            { LogLevel.Debug, ConsoleColor.Gray },
            { LogLevel.Info, ConsoleColor.White },
            { LogLevel.Warn, ConsoleColor.Yellow },
            { LogLevel.Error, ConsoleColor.Red },
            { LogLevel.Fatal, ConsoleColor.DarkRed },
        };
#endif

        /// <summary>
        /// Initializes a new instance of the <see cref="InstallationContext"/> class.
        /// </summary>
        public InstallationContext()
            : this(TextWriter.Null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="InstallationContext"/> class.
        /// </summary>
        /// <param name="logOutput">The log output.</param>
        public InstallationContext(TextWriter logOutput)
        {
            this.LogOutput = logOutput;
            this.Parameters = new Dictionary<string, string>();
            this.LogLevel = LogLevel.Info;
        }

        /// <summary>
        /// Gets or sets the installation log level.
        /// </summary>
        public LogLevel LogLevel { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to ignore failures during installation.
        /// </summary>
        public bool IgnoreFailures { get; set; }

        /// <summary>
        /// Gets the installation parameters.
        /// </summary>
        public IDictionary<string, string> Parameters { get; private set; }

        /// <summary>
        /// Gets or sets the log output.
        /// </summary>
        public TextWriter LogOutput { get; set; }

        /// <summary>
        /// Logs the specified trace message.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="arguments">The arguments.</param>
        public void Trace([Localizable(false)] string message, params object[] arguments)
        {
            this.Log(LogLevel.Trace, message, arguments);
        }

        /// <summary>
        /// Logs the specified debug message.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="arguments">The arguments.</param>
        public void Debug([Localizable(false)] string message, params object[] arguments)
        {
            this.Log(LogLevel.Debug, message, arguments);
        }

        /// <summary>
        /// Logs the specified informational message.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="arguments">The arguments.</param>
        public void Info([Localizable(false)] string message, params object[] arguments)
        {
            this.Log(LogLevel.Info, message, arguments);
        }

        /// <summary>
        /// Logs the specified warning message.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="arguments">The arguments.</param>
        public void Warning([Localizable(false)] string message, params object[] arguments)
        {
            this.Log(LogLevel.Warn, message, arguments);
        }

        /// <summary>
        /// Logs the specified error message.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="arguments">The arguments.</param>
        public void Error([Localizable(false)] string message, params object[] arguments)
        {
            this.Log(LogLevel.Error, message, arguments);
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            if (this.LogOutput != null)
            {
                this.LogOutput.Close();
                this.LogOutput = null;
            }
        }

        /// <summary>
        /// Creates the log event which can be used to render layouts during installation/uninstallations.
        /// </summary>
        /// <returns>Log event info object.</returns>
        public LogEventInfo CreateLogEvent()
        {
            var eventInfo = LogEventInfo.CreateNullEvent();

            // set properties on the event
            foreach (var kvp in this.Parameters)
            {
                eventInfo.Properties.Add(kvp.Key, kvp.Value);
            }

            return eventInfo;
        }

        private void Log(LogLevel logLevel, [Localizable(false)] string message, object[] arguments)
        {
            if (logLevel >= this.LogLevel)
            {
                if (arguments != null && arguments.Length > 0)
                {
                    message = string.Format(CultureInfo.InvariantCulture, message, arguments);
                }

#if !SILVERLIGHT && !__IOS__ && !__ANDROID__
                var oldColor = Console.ForegroundColor;
                Console.ForegroundColor = logLevel2ConsoleColor[logLevel];

                try
                {
                    this.LogOutput.WriteLine(message);
                }
                finally
                {
                    Console.ForegroundColor = oldColor;
                }
#else
                this.LogOutput.WriteLine(message);
#endif
            }
        }
    }
}
