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
            get { return Console.OutputEncoding; }
            set { Console.OutputEncoding = value; }
        }
#endif

        /// <summary>
        /// Initializes a new instance of the <see cref="ConsoleTarget" /> class.
        /// </summary>
        /// <remarks>
        /// The default value of the layout is: <code>${longdate}|${level:uppercase=true}|${logger}|${message}</code>
        /// </remarks>
        public ConsoleTarget() : base()
        {
        }

        /// <summary>
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
            base.InitializeTarget();
            if (Header != null)
            {
                this.Output(Header.Render(LogEventInfo.CreateNullEvent()));
            }
        }

        /// <summary>
        /// Closes the target and releases any unmanaged resources.
        /// </summary>
        protected override void CloseTarget()
        {
            if (Footer != null)
            {
                this.Output(Footer.Render(LogEventInfo.CreateNullEvent()));
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
            this.Output(this.Layout.Render(logEvent));
        }

        /// <summary>
        /// Write to output
        /// </summary>
        /// <param name="textLine">text to be written.</param>
        private void Output(string textLine)
        {
            if (this.Error)
            {
                Console.Error.WriteLine(textLine);
            }
            else
            {
                Console.Out.WriteLine(textLine);
            }
        }
    }
}
