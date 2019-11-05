// 
// Copyright (c) 2004-2019 Jaroslaw Kowalski <jaak@jkowalski.net>, Kim Christensen, Julian Verdurmen
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
using System.Linq;
using System.Text;
using NLog.Common;
using NLog.MessageTemplates;

namespace NLog.Internal
{
    class LogMessageTemplateFormatterAdapter //todo fix better name
    {
        private readonly TemplateRenderer _templateRenderer;
        public static readonly ILogMessageFormatter DefaultAuto = new LogMessageTemplateFormatter(false, false);
        public static readonly ILogMessageFormatter Default = new LogMessageTemplateFormatter(true, false);
        public static readonly ILogMessageFormatter DefaultAutoSingleTarget = new LogMessageTemplateFormatter(false, true);
        
        internal static readonly ILogMessageFormatter StringFormatMessageFormatter = new StringFormatMessageFormatter();

        internal static ILogMessageFormatter DefaultMessageFormatter { get; private set; } = DefaultAuto;


        private ILogMessageFormatter _messageFormatter = DefaultMessageFormatter;

        /// <summary>
        /// Set the <see cref="DefaultMessageFormatter"/>
        /// </summary>
        /// <param name="mode">true = Always, false = Never, null = Auto Detect</param>
        internal static void SetDefaultMessageFormatter(bool? mode)
        {
            if (mode == true)
            {
                InternalLogger.Info("Message Template Format always enabled");
                DefaultMessageFormatter = Default;
            }
            else if (mode == false)
            {
                InternalLogger.Info("Message Template String Format always enabled");
                DefaultMessageFormatter = StringFormatMessageFormatter;
            }
            else
            {
                //null = auto
                InternalLogger.Info("Message Template Auto Format enabled");
                DefaultMessageFormatter = DefaultAuto;
            }
        }


        /// <inheritdoc />
        public LogMessageTemplateFormatterAdapter(TemplateRenderer templateRenderer)
        {
            _templateRenderer = templateRenderer;
        }

        public void AppendFormattedMessage(LogEventInfo logEventInfo, StringBuilder builder)
        {
            var useAutoSingle = UseAutoSingle(logEventInfo);

            if (useAutoSingle)
            {
                DefaultAutoSingleTarget.AppendFormattedMessage(logEventInfo, builder, _templateRenderer);
            }
            else
            {
                Default.AppendFormattedMessage(logEventInfo, builder, _templateRenderer);
            }
        }

        private static bool UseAutoSingle(LogEventInfo logEventInfo)
        {
            bool useAutoSingle;
            if (logEventInfo.HasFormattedMessage)
                useAutoSingle = false;
            else
            {
                var parameters = logEventInfo.Parameters;
                if (parameters == null || parameters.Length == 0)
                    useAutoSingle = false;
                else
                {
                    if (logEventInfo.Message?.Length < 256 && ReferenceEquals(DefaultMessageFormatter, DefaultAuto))
                        useAutoSingle = true;
                    else
                        useAutoSingle = false;
                }
            }

            return useAutoSingle;
        }
    }
}
