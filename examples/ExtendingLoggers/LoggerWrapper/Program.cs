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
using System.Text;
using NLog;

namespace LoggerWrapper
{
    /// <summary>
    /// Provides methods to write messages with event IDs - useful for the Event Log target.
    /// Wraps a Logger instance.
    /// </summary>
    class MyLogger
    {
        private Logger _logger;

        public MyLogger(string name)
        {
            _logger = LogManager.GetLogger(name);
        }

        public void WriteMessage(string eventID, string message)
        {
            ///
            /// create log event from the passed message
            /// 
            LogEventInfo logEvent = new LogEventInfo(LogLevel.Info, _logger.Name, message);

            //
            // set event-specific context parameter
            // this context parameter can be retrieved using ${event-context:EventID}
            //
            logEvent.Context["EventID"] = eventID;

            // 
            // Call the Log() method. It is important to pass typeof(MyLogger) as the
            // first parameter. If you don't, ${callsite} and other callstack-related 
            // layout renderers will not work properly.
            //

            _logger.Log(typeof(MyLogger), logEvent);
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            MyLogger l = new MyLogger("uuu");

            l.WriteMessage("1234", "message");
        }
    }
}
