// 
// Copyright (c) 2004 Jaroslaw Kowalski <jaak@polbox.com>
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
// * Neither the name of the Jaroslaw Kowalski nor the names of its 
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
using System.Collections;
using System.Diagnostics;

namespace NLog
{
	sealed class LoggerImpl : Logger
    {
        private const int STACK_TRACE_SKIP_METHODS = 3;
        
        private ArrayList[] _appendersByLevel;
        private string _loggerName;

        public LoggerImpl(string name, ArrayList[] appendersByLevel) {
            _loggerName = name;
            _appendersByLevel = appendersByLevel;
        }

        protected override void Write(LogLevel level, IFormatProvider formatProvider, string message, object[] args) {
            WriteToAppenders(level, _appendersByLevel[(int)level], formatProvider, message, args);
        }

        internal string Name
        {
            get { return _loggerName; }
        }

        internal void Reconfig(ArrayList[] appendersByLevel)
        {
            _appendersByLevel = appendersByLevel;
        }

        private void WriteToAppenders(LogLevel level, ArrayList appenders, IFormatProvider formatProvider, string message, object[] args) {
            if (LogManager.ReloadConfigOnNextLog)
                LogManager.ReloadConfig();

            if (appenders.Count == 0)
                return;

            string formattedMessage;
            
            if (args == null)
                formattedMessage = message;
            else
                formattedMessage = String.Format(formatProvider, message, args);

            LogEventInfo logMessage = new LogEventInfo(DateTime.Now, level, _loggerName, formattedMessage);
#if !NETCF            
            bool needTrace = false;
            bool needTraceSources = false;

            for (int i = 0; i < appenders.Count; ++i) {
                Appender app = (Appender)appenders[i];
                
                int nst = app.NeedsStackTrace();
                
                if (nst > 1) {
                    needTraceSources = true;
                }
                if (nst > 0) {
                    needTrace = true;
                    break;
                }
            }

            StackTrace stackTrace = null;
            if (needTrace) {
                int firstUserFrame = 0;
                stackTrace = new StackTrace(STACK_TRACE_SKIP_METHODS, needTraceSources);

                for (int i = 0; i < stackTrace.FrameCount; ++i)
                {
                    System.Reflection.MethodBase mb = stackTrace.GetFrame(i).GetMethod();

                    if (!mb.DeclaringType.FullName.StartsWith("NLog."))
                    {
                        firstUserFrame = i;
                        break;
                    }
                    else 
                    {
                        // Console.WriteLine("skipping stack frame: " + mb);
                    }
                }
                logMessage.SetStackTrace(stackTrace, firstUserFrame);
            }
#endif            
            for (int i = 0; i < appenders.Count; ++i) {
                Appender app = (Appender)appenders[i];
                
                try {
                    app.Append(logMessage);
                }
                catch (Exception ex) {
                    InternalLogger.Error("Appender exception: {0}", ex);
                }
            }
        }

        public override bool IsEnabled(LogLevel level) {
            return _appendersByLevel[(int)level].Count > 0;
        }

        public override bool IsDebugEnabled
        {
            get { return _appendersByLevel[(int)LogLevel.Debug].Count > 0; }
        }
        
        public override bool IsInfoEnabled
        {
            get { return _appendersByLevel[(int)LogLevel.Info].Count > 0; }
        }
        
        public override bool IsWarnEnabled
        {
            get { return _appendersByLevel[(int)LogLevel.Warn].Count > 0; }
        }
        
        public override bool IsErrorEnabled
        {
            get { return _appendersByLevel[(int)LogLevel.Error].Count > 0; }
        }
        
        public override bool IsFatalEnabled
        {
            get { return _appendersByLevel[(int)LogLevel.Fatal].Count > 0; }
        }
    }
}
