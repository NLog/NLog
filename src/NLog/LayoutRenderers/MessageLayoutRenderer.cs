// 
// Copyright (c) 2004-2018 Jaroslaw Kowalski <jaak@jkowalski.net>, Kim Christensen, Julian Verdurmen
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

namespace NLog.LayoutRenderers
{
    using System;
    using System.Text;
    using NLog.Config;
    using NLog.Internal;

    /// <summary>
    /// The formatted log message.
    /// </summary>
    [LayoutRenderer("message")]
    [ThreadAgnostic]
    [ThreadSafe]
    public class MessageLayoutRenderer : LayoutRenderer
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MessageLayoutRenderer" /> class.
        /// </summary>
        public MessageLayoutRenderer()
        {
            ExceptionSeparator = EnvironmentHelper.NewLine;
        }

        /// <summary>
        /// Gets or sets a value indicating whether to log exception along with message.
        /// </summary>
        /// <docgen category='Layout Options' order='10' />
        public bool WithException { get; set; }

        /// <summary>
        /// Gets or sets the string that separates message from the exception.
        /// </summary>
        /// <docgen category='Layout Options' order='10' />
        public string ExceptionSeparator { get; set; }

        /// <summary>
        /// Gets or sets whether it should render the raw message without formatting parameters
        /// </summary>
        /// <docgen category='Layout Options' order='10' />
        public bool Raw { get; set; }

        /// <summary>
        /// Renders the log message including any positional parameters and appends it to the specified <see cref="StringBuilder" />.
        /// </summary>
        /// <param name="builder">The <see cref="StringBuilder"/> to append the rendered data to.</param>
        /// <param name="logEvent">Logging event.</param>
        protected override void Append(StringBuilder builder, LogEventInfo logEvent)
        {
            if (Raw)
            {
                builder.Append(logEvent.Message);
            }
            else
            {
                if (ReferenceEquals(logEvent.MessageFormatter, LogMessageTemplateFormatter.DefaultAutoSingleTarget.MessageFormatter))
                {
                    // Skip string-allocation of LogEventInfo.FormattedMessage, but just write directly to StringBuilder
                    logEvent.AppendFormattedMessage(LogMessageTemplateFormatter.DefaultAutoSingleTarget, builder);
                }
                else
                {
                    builder.Append(logEvent.FormattedMessage);
                }
            }
            if (WithException && logEvent.Exception != null)
            {
                builder.Append(ExceptionSeparator);
                builder.Append(logEvent.Exception.ToString());
            }
        }
    }
}
