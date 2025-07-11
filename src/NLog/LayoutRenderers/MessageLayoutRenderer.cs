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

namespace NLog.LayoutRenderers
{
    using System;
    using System.Text;
    using NLog.Config;
    using NLog.Internal;

    /// <summary>
    /// The formatted log message.
    /// </summary>
    /// <remarks>
    /// <a href="https://github.com/NLog/NLog/wiki/Message-Layout-Renderer">See NLog Wiki</a>
    /// </remarks>
    /// <seealso href="https://github.com/NLog/NLog/wiki/Message-Layout-Renderer">Documentation on NLog Wiki</seealso>
    [LayoutRenderer("message")]
    [ThreadAgnostic]
    public class MessageLayoutRenderer : LayoutRenderer, IStringValueRenderer
    {
        /// <summary>
        /// Gets or sets a value indicating whether to log exception along with message.
        /// </summary>
        /// <remarks>Default: <see langword="false"/></remarks>
        /// <docgen category='Layout Options' order='10' />
        public bool WithException { get; set; }

        /// <summary>
        /// Gets or sets the string that separates message from the exception.
        /// </summary>
        /// <remarks>Default: <c>|</c></remarks>
        /// <docgen category='Layout Options' order='10' />
        public string ExceptionSeparator { get; set; } = "|";

        /// <summary>
        /// Gets or sets whether it should render the raw message without formatting parameters
        /// </summary>
        /// <remarks>Default: <see langword="false"/></remarks>
        /// <docgen category='Layout Options' order='10' />
        public bool Raw { get; set; }

        /// <inheritdoc/>
        protected override void Append(StringBuilder builder, LogEventInfo logEvent)
        {
            bool exceptionOnly = logEvent.Exception != null && WithException && logEvent.Parameters?.Length == 1 && ReferenceEquals(logEvent.Parameters[0], logEvent.Exception) && logEvent.Message == "{0}";

            if (Raw)
            {
                builder.Append(logEvent.Message);
            }
            else if (!exceptionOnly)
            {
                if (logEvent.MessageFormatter?.Target is ILogMessageFormatter messageFormatter)
                {
                    logEvent.AppendFormattedMessage(messageFormatter, builder);
                }
                else
                {
                    builder.Append(logEvent.FormattedMessage);
                }
            }

            if (WithException && logEvent.Exception != null)
            {
                Exception primaryException = GetPrimaryException(logEvent.Exception);
                if (!exceptionOnly)
                    builder.Append(ExceptionSeparator);
                builder.Append(primaryException.ToString());
            }
        }

        private static Exception GetPrimaryException(Exception exception)
        {
#if !NET35
            if (exception is AggregateException aggregateException)
            {
                if (aggregateException.InnerExceptions?.Count == 1)
                {
                    var innerException = aggregateException.InnerExceptions[0];
                    if (!(innerException is AggregateException))
                        return innerException;  // Skip calling Flatten()
                }

                aggregateException = aggregateException.Flatten();
                if (aggregateException.InnerExceptions?.Count == 1)
                {
                    return aggregateException.InnerExceptions[0];
                }

                return aggregateException;  // Matches ${exception} default behavior
            }
#endif
            return exception;
        }

        string? IStringValueRenderer.GetFormattedString(LogEventInfo logEvent)
        {
            if (WithException)
                return null;
            else
                return (Raw ? logEvent.Message : logEvent.FormattedMessage) ?? string.Empty;
        }
    }
}
