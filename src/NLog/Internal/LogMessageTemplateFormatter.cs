// 
// Copyright (c) 2004-2017 Jaroslaw Kowalski <jaak@jkowalski.net>, Kim Christensen, Julian Verdurmen
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

namespace NLog.Internal
{
    using System;
    using System.Globalization;
    using MessageTemplates;

    internal sealed class LogMessageTemplateFormatter : ILogMessageFormatter
    {
        public static readonly LogMessageTemplateFormatter DefaultAuto = new LogMessageTemplateFormatter(false);
        public static readonly LogMessageTemplateFormatter Default = new LogMessageTemplateFormatter(true);
        private static readonly StringBuilderPool _builderPool = new StringBuilderPool(Environment.ProcessorCount * 2);

        /// <summary>
        /// When true: Do not fallback to StringBuilder.Format for positional templates
        /// </summary>
        private readonly bool _forceTemplateRenderer;

        /// <summary>
        /// New formatter
        /// </summary>
        /// <param name="forceTemplateRenderer">When true: Do not fallback to StringBuilder.Format for positional templates</param>
        private LogMessageTemplateFormatter(bool forceTemplateRenderer)
        {
            _forceTemplateRenderer = forceTemplateRenderer;
            MessageFormatter = FormatMessage;
        }

        /// <summary>
        /// The MessageFormatter delegate
        /// </summary>
        public LogMessageFormatter MessageFormatter { get; }

        public bool HasProperties(LogEventInfo logEvent)
        {
            //if message is empty, there no parameters
            //null check cheapest, so in-front
            return logEvent.Parameters != null && !string.IsNullOrEmpty(logEvent.Message) && logEvent.Parameters.Length > 0;
        }

        public string FormatMessage(LogEventInfo logEvent)
        {
            if (!HasProperties(logEvent))
            {
                return logEvent.Message;
            }
            // Prevent multiple layouts on different targets to render the same properties
            lock (logEvent)
            {
                using (var builder = _builderPool.Acquire())
                {
                    logEvent.Message.Render(logEvent.FormatProvider ?? CultureInfo.CurrentCulture, logEvent.Parameters, _forceTemplateRenderer, builder.Item, out var messageTemplateParameterList);
                    if (logEvent.PropertiesDictionary == null)
                    {
                        if (messageTemplateParameterList != null && messageTemplateParameterList.Count > 0)
                        {
                            logEvent.PropertiesDictionary = new PropertiesDictionary(messageTemplateParameterList);
                        }
                    }
                    else
                    {
                        logEvent.PropertiesDictionary.MessageProperties = messageTemplateParameterList;
                    }
                    return builder.Item.ToString();
                }
            }
        }
    }
}
