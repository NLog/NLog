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

using System.IO;
using NLog.Common;

namespace NLog.Targets
{
    using System;
    using System.Text;
    using System.ComponentModel;

    /// <summary>
    /// Writes log messages to the console.
    /// </summary>
    /// <seealso href="https://github.com/nlog/nlog/wiki/Console-target">Documentation on NLog Wiki</seealso>
    /// <example>
    /// <p>
    /// To set up the target in the <a href="config.html">configuration file</a>, 
    /// use the following syntax:
    /// </p>
    /// <code lang="XML" source="examples/targets/Configuration File/Console/NLog.config" />
    /// <p>
    /// This assumes just one target and a single rule. More configuration
    /// options are described <a href="config.html">here</a>.
    /// </p>
    /// <p>
    /// To set up the log target programmatically use code like this:
    /// </p>
    /// <code lang="C#" source="examples/targets/Configuration API/Console/Simple/Example.cs" />
    /// </example>
    [Target("Console")]
    public sealed class ConsoleTarget : TargetWithLayoutHeaderAndFooter
    {
        /// <summary>
        /// Should logging being paused/stopped because of the race condition bug in Console.Writeline?
        /// </summary>
        /// <remarks>
        ///   Console.Out.Writeline / Console.Error.Writeline could throw 'IndexOutOfRangeException', which is a bug. 
        /// See http://stackoverflow.com/questions/33915790/console-out-and-console-error-race-condition-error-in-a-windows-service-written
        /// and https://connect.microsoft.com/VisualStudio/feedback/details/2057284/console-out-probable-i-o-race-condition-issue-in-multi-threaded-windows-service
        ///             
        /// Full error: 
        ///   Error during session close: System.IndexOutOfRangeException: Probable I/ O race condition detected while copying memory.
        ///   The I/ O package is not thread safe by default.In multithreaded applications, 
        ///   a stream must be accessed in a thread-safe way, such as a thread - safe wrapper returned by TextReader's or 
        ///   TextWriter's Synchronized methods.This also applies to classes like StreamWriter and StreamReader.
        /// 
        /// </remarks>
        private bool _pauseLogging;

        /// <summary>
        /// Gets or sets a value indicating whether to send the log messages to the standard error instead of the standard output.
        /// </summary>
        /// <docgen category='Console Options' order='10' />
        [DefaultValue(false)]
        public bool Error { get; set; }

#if !SILVERLIGHT && !__IOS__ && !__ANDROID__
        /// <summary>
        /// The encoding for writing messages to the <see cref="Console"/>.
        ///  </summary>
        /// <remarks>Has side effect</remarks>
        public Encoding Encoding
        {
            get
            {
                return ConsoleTargetHelper.GetConsoleOutputEncoding(_encoding, IsInitialized, _pauseLogging);
            }
            set
            {
                if (ConsoleTargetHelper.SetConsoleOutputEncoding(value, IsInitialized, _pauseLogging))
                    _encoding = value;
            }
        }
        private Encoding _encoding;
#endif

        /// <summary>
        /// Gets or sets a value indicating whether to auto-check if the console is available
        ///  - Disables console writing if Environment.UserInteractive = False (Windows Service)
        ///  - Disables console writing if Console Standard Input is not available (Non-Console-App)
        /// </summary>
        [DefaultValue(false)]
        public bool DetectConsoleAvailable { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ConsoleTarget" /> class.
        /// </summary>
        /// <remarks>
        /// The default value of the layout is: <code>${longdate}|${level:uppercase=true}|${logger}|${message}</code>
        /// </remarks>
        public ConsoleTarget() : base()
        {
            _pauseLogging = false;
            this.DetectConsoleAvailable = false;
            this.OptimizeBufferReuse = true;
        }

        /// <summary>
        /// 
        /// Initializes a new instance of the <see cref="ConsoleTarget" /> class.
        /// </summary>
        /// <remarks>
        /// The default value of the layout is: <code>${longdate}|${level:uppercase=true}|${logger}|${message}</code>
        /// </remarks>
        /// <param name="name">Name of the target.</param>
        public ConsoleTarget(string name) : this()
        {
            this.Name = name;
        }

        /// <summary>
        /// Initializes the target.
        /// </summary>
        protected override void InitializeTarget()
        {
            _pauseLogging = false;
            if (DetectConsoleAvailable)
            {
                string reason;
                _pauseLogging = !ConsoleTargetHelper.IsConsoleAvailable(out reason);
                if (_pauseLogging)
                {
                    InternalLogger.Info("Console has been detected as turned off. Disable DetectConsoleAvailable to skip detection. Reason: {0}", reason);
                }
            }
#if !SILVERLIGHT && !__IOS__ && !__ANDROID__
            if (_encoding != null && !_pauseLogging)
                Console.OutputEncoding = _encoding;
#endif
            base.InitializeTarget();
            if (this.Header != null)
            {
                this.WriteToOutput(base.RenderLogEvent(this.Header, LogEventInfo.CreateNullEvent()));
            }
        }

        /// <summary>
        /// Closes the target and releases any unmanaged resources.
        /// </summary>
        protected override void CloseTarget()
        {
            if (this.Footer != null)
            {
                this.WriteToOutput(base.RenderLogEvent(this.Footer, LogEventInfo.CreateNullEvent()));
            }

            base.CloseTarget();
        }

        /// <summary>
        /// Writes the specified logging event to the Console.Out or
        /// Console.Error depending on the value of the Error flag.
        /// </summary>
        /// <param name="logEvent">The logging event.</param>
        /// <remarks>
        /// Note that the Error option is not supported on .NET Compact Framework.
        /// </remarks>
        protected override void Write(LogEventInfo logEvent)
        {
            if (_pauseLogging)
            {
                //check early for performance
                return;
            }

            this.WriteToOutput(base.RenderLogEvent(this.Layout, logEvent));
        }

        /// <summary>
        /// Write to output
        /// </summary>
        /// <param name="textLine">text to be written.</param>
        private void WriteToOutput(string textLine)
        {
            if (_pauseLogging)
            {
                return;
            }

            var output = GetOutput();

            try
            {
                output.WriteLine(textLine);
            }
            catch (IndexOutOfRangeException ex)
            {
                //this is a bug and therefor stopping logging. For docs, see PauseLogging property
                _pauseLogging = true;
                InternalLogger.Warn(ex, "An IndexOutOfRangeException has been thrown and this is probably due to a race condition." +
                                        "Logging to the console will be paused. Enable by reloading the config or re-initialize the targets");
            }
        }

        private TextWriter GetOutput()
        {
            return this.Error ? Console.Error : Console.Out;
        }
    }
}
