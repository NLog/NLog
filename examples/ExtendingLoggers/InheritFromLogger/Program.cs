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

namespace InheritFromLogger
{
    /// <summary>
    /// Provides methods to write messages with event IDs - useful for the Event Log target
    /// Inherits from the Logger class.
    /// </summary>
    public class LoggerWithEventID : Logger
    {
        public LoggerWithEventID()
        {
        }

        // additional method that takes eventID as an argument
        public void DebugWithEventID(int eventID, string message, params object[] args)
        {
            if (IsDebugEnabled)
            {
                // create log event 
                LogEventInfo lei = new LogEventInfo(LogLevel.Debug, Name, null, message, args);

                // set the per-log context data
                // this data can be retrieved using ${event-context:EventID}
                lei.Context["EventID"] = eventID;

                // log the message
                base.Log(typeof(LoggerWithEventID), lei);
            }
        }

        // other methods omitted for brevity
    }

    class Program
    {
        // get the current class logger as an instance of LoggerWithEventID class

        private static LoggerWithEventID LoggerWithEventID = (LoggerWithEventID)LogManager.GetCurrentClassLogger(typeof(LoggerWithEventID));

        static void Main(string[] args)
        {
            // this writes 5 messages to the Event Log, each with a different EventID

            LoggerWithEventID.DebugWithEventID(123, "message 1", 1, 2, 3);
            LoggerWithEventID.DebugWithEventID(124, "message 2", 1, 2, 3);
            LoggerWithEventID.DebugWithEventID(125, "message 3", 1, 2, 3);
            LoggerWithEventID.DebugWithEventID(126, "message 4", 1, 2, 3);
            LoggerWithEventID.DebugWithEventID(127, "message 5", 1, 2, 3);
        }
    }
}
