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

#if !SILVERLIGHT && !__IOS__ && !__ANDROID__ && !UWP10 || NETSTANDARD1_3

namespace NLog.Targets
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Text;
    using NLog.Internal;
    using NLog.Config;
    using System.IO;

    /// <summary>
    /// Writes log messages to the console with customizable coloring.
    /// </summary>
    /// <seealso href="https://github.com/nlog/nlog/wiki/ColoredConsole-target">Documentation on NLog Wiki</seealso>
    [Target("ColoredConsole")]
    public sealed class ColoredConsoleTarget : TargetWithLayoutHeaderAndFooter
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ColoredConsoleTarget" /> class.
        /// </summary>
        /// <remarks>
        /// The default value of the layout is: <code>${longdate}|${level:uppercase=true}|${logger}|${message}</code>
        /// </remarks>
        public ColoredConsoleTarget()
        {
            this.WordHighlightingRules = new List<ConsoleWordHighlightingRule>();
            this.RowHighlightingRules = new List<ConsoleRowHighlightingRule>();
            this.UseDefaultRowHighlightingRules = true;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ColoredConsoleTarget" /> class.
        /// </summary>
        /// <remarks>
        /// The default value of the layout is: <code>${longdate}|${level:uppercase=true}|${logger}|${message}</code>
        /// </remarks>
        /// <param name="name">Name of the target.</param>
        public ColoredConsoleTarget(string name) : this()
        {
            this.Name = name;
        }

        /// <summary>
        /// Gets or sets a value indicating whether the error stream (stderr) should be used instead of the output stream (stdout).
        /// </summary>
        /// <docgen category='Output Options' order='10' />
        [DefaultValue(false)]
        public bool ErrorStream { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to use default row highlighting rules.
        /// </summary>
        /// <remarks>
        /// The default rules for Windows are:
        /// <table>
        /// <tr><th>Condition</th><th>Foreground Color</th><th>Background Color</th></tr>
        /// <tr><td>level == LogLevel.Fatal</td><td>Red</td><td>NoChange</td></tr>
        /// <tr><td>level == LogLevel.Error</td><td>Yellow</td><td>NoChange</td></tr>
        /// <tr><td>level == LogLevel.Warn</td><td>Magenta</td><td>NoChange</td></tr>
        /// <tr><td>level == LogLevel.Info</td><td>White</td><td>NoChange</td></tr>
        /// <tr><td>level == LogLevel.Debug</td><td>Gray</td><td>NoChange</td></tr>
        /// <tr><td>level == LogLevel.Trace</td><td>DarkGray</td><td>NoChange</td></tr>
        /// </table>
        /// The default rules for Unix based systems are:
        /// <table>
        /// <tr><th>Condition</th><th>Foreground Color</th><th>Background Color</th></tr>
        /// <tr><td>level == LogLevel.Fatal</td><td>DarkRed</td><td>NoChange</td></tr>
        /// <tr><td>level == LogLevel.Error</td><td>DarkYellow</td><td>NoChange</td></tr>
        /// <tr><td>level == LogLevel.Warn</td><td>DarkMagenta</td><td>NoChange</td></tr>
        /// <tr><td>level == LogLevel.Info</td><td>DarkGreen</td><td>NoChange</td></tr>
        /// <tr><td>level == LogLevel.Debug</td><td>Gray</td><td>NoChange</td></tr>
        /// <tr><td>level == LogLevel.Trace</td><td>Gray</td><td>NoChange</td></tr>
        /// </table>
        /// </remarks>
        /// <docgen category='Highlighting Rules' order='9' />
        [DefaultValue(true)]
        public bool UseDefaultRowHighlightingRules { get; set; }

#if !NETSTANDARD1_3
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
        /// Gets the row highlighting rules.
        /// </summary>
        /// <docgen category='Highlighting Rules' order='10' />
        [ArrayParameter(typeof(ConsoleRowHighlightingRule), "highlight-row")]
        public IList<ConsoleRowHighlightingRule> RowHighlightingRules { get; private set; }

        /// <summary>
        /// Gets the word highlighting rules.
        /// </summary>
        /// <docgen category='Highlighting Rules' order='11' />
        [ArrayParameter(typeof(ConsoleWordHighlightingRule), "highlight-word")]
        public IList<ConsoleWordHighlightingRule> WordHighlightingRules { get; private set; }

        /// <summary>
        /// Initializes the target.
        /// </summary>
        protected override void InitializeTarget()
        {
            base.InitializeTarget();
            if (Header != null)
            {
                LogEventInfo lei = LogEventInfo.CreateNullEvent();
                this.Output(lei, Header.Render(lei));
            }
        }

        /// <summary>
        /// Closes the target and releases any unmanaged resources.
        /// </summary>
        protected override void CloseTarget()
        {
            if (Footer != null)
            {
                LogEventInfo lei = LogEventInfo.CreateNullEvent();
                this.Output(lei, Footer.Render(lei));
            }

            base.CloseTarget();
        }

            /// <summary>
        /// Writes the specified log event to the console highlighting entries
        /// and words based on a set of defined rules.
        /// </summary>
        /// <param name="logEvent">Log event.</param>
        protected override void Write(LogEventInfo logEvent)
        {
            this.Output(logEvent, this.Layout.Render(logEvent));
        }

        private void Output(LogEventInfo logEvent, string message)
        {
            var colorizer = CreateConsoleColorizer(message);       

            var matchingRule = GetMatchingRowHighlightingRule(logEvent, colorizer.DefaultConsoleRowHighlightingRules);
            colorizer.RowHighlightingRule = matchingRule;
            colorizer.WordHighlightingRules = this.WordHighlightingRules;

            var consoleStream = this.ErrorStream ? Console.Error : Console.Out;
            colorizer.ColorizeMessage(consoleStream);
        }

        private IConsoleColorizer CreateConsoleColorizer(string message)
        {
            if (!PlatformDetector.IsUnix)
                return new ConsoleColorizer(message);
            else
                return new AnsiConsoleColorizer(message);
        }

        private ConsoleRowHighlightingRule GetMatchingRowHighlightingRule(LogEventInfo logEvent, IList<ConsoleRowHighlightingRule> defaultConsoleRowHighlightingRules)
        {
            foreach (ConsoleRowHighlightingRule rule in this.RowHighlightingRules)
            {
                if (rule.CheckCondition(logEvent))
                    return rule;
            }

            if (this.UseDefaultRowHighlightingRules)
            {
                foreach (ConsoleRowHighlightingRule rule in defaultConsoleRowHighlightingRules)
                {
                    if (rule.CheckCondition(logEvent))
                        return rule;
                }
            }

            return ConsoleRowHighlightingRule.Default;
        }
    }
}

#endif
