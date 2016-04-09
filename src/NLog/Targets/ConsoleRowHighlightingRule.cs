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

#if !SILVERLIGHT

namespace NLog.Targets
{
    using System.ComponentModel;
    using NLog.Conditions;
    using NLog.Config;

    /// <summary>
    /// The row-highlighting condition.
    /// </summary>
    [NLogConfigurationItem]
    public class ConsoleRowHighlightingRule
    {
        /// <summary>
        /// Initializes static members of the ConsoleRowHighlightingRule class.
        /// </summary>
        static ConsoleRowHighlightingRule()
        {
            Default = new ConsoleRowHighlightingRule(null, ConsoleOutputColor.NoChange, ConsoleOutputColor.NoChange);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ConsoleRowHighlightingRule" /> class.
        /// </summary>
        public ConsoleRowHighlightingRule()
            : this(null, ConsoleOutputColor.NoChange, ConsoleOutputColor.NoChange)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ConsoleRowHighlightingRule" /> class.
        /// </summary>
        /// <param name="condition">The condition.</param>
        /// <param name="foregroundColor">Color of the foreground.</param>
        /// <param name="backgroundColor">Color of the background.</param>
        public ConsoleRowHighlightingRule(ConditionExpression condition, ConsoleOutputColor foregroundColor, ConsoleOutputColor backgroundColor)
        {
            this.Condition = condition;
            this.ForegroundColor = foregroundColor;
            this.BackgroundColor = backgroundColor;
        }

        /// <summary>
        /// Gets the default highlighting rule. Doesn't change the color.
        /// </summary>
        public static ConsoleRowHighlightingRule Default { get; private set; }

        /// <summary>
        /// Gets or sets the condition that must be met in order to set the specified foreground and background color.
        /// </summary>
        /// <docgen category='Rule Matching Options' order='10' />
        [RequiredParameter]
        public ConditionExpression Condition { get; set; }

        /// <summary>
        /// Gets or sets the foreground color.
        /// </summary>
        /// <docgen category='Formatting Options' order='10' />
        [DefaultValue("NoChange")]
        public ConsoleOutputColor ForegroundColor { get; set; }

        /// <summary>
        /// Gets or sets the background color.
        /// </summary>
        /// <docgen category='Formatting Options' order='10' />
        [DefaultValue("NoChange")]
        public ConsoleOutputColor BackgroundColor { get; set; }

        /// <summary>
        /// Checks whether the specified log event matches the condition (if any).
        /// </summary>
        /// <param name="logEvent">
        /// Log event.
        /// </param>
        /// <returns>
        /// A value of <see langword="true"/> if the condition is not defined or 
        /// if it matches, <see langword="false"/> otherwise.
        /// </returns>
        public bool CheckCondition(LogEventInfo logEvent)
        {
            if (this.Condition == null)
            {
                return true;
            }

            return true.Equals(this.Condition.Evaluate(logEvent));
        }
    }
}

#endif