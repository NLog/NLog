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

namespace NLog
{
	public abstract class Logger
    {
        protected abstract void Write(LogLevel level, IFormatProvider formatProvider, string message, object[] args);

        public abstract bool IsEnabled(LogLevel level);
        public abstract bool IsDebugEnabled { get; }
        public abstract bool IsInfoEnabled { get; }
        public abstract bool IsWarnEnabled { get; }
        public abstract bool IsErrorEnabled { get; }
        public abstract bool IsFatalEnabled { get; }

        // make sure that each of the following methods does nothing but calling Write()
        // StackTrace functionality depends on it

		public void Log(LogLevel level, IFormatProvider formatProvider, string message, params object[] args) 
		{
			Write(level, formatProvider, message, args);
		}
        
		public void Log(LogLevel level, string message, params object[] args) 
		{
            Write(level, null, message, args);
        }
        
        public void Log(LogLevel level, string message) {
            Write(level, null, message, null);
        }
        
		public void Debug(IFormatProvider formatProvider, string message, params object[] args) 
		{
			Write(LogLevel.Debug, formatProvider, message, args);
		}
        
		public void Debug(string message, params object[] args) 
		{
            Write(LogLevel.Debug, null, message, args);
        }
        
        public void Debug(string message) {
            Write(LogLevel.Debug, null, message, null);
        }
        
		public void Info(IFormatProvider formatProvider, string message, params object[] args) 
		{
			Write(LogLevel.Info, formatProvider, message, args);
		}
        
		public void Info(string message, params object[] args) 
		{
            Write(LogLevel.Info, null, message, args);
        }
        
        public void Info(string message) {
            Write(LogLevel.Info, null, message, null);
        }
        
		public void Warn(IFormatProvider formatProvider, string message, params object[] args) 
		{
			Write(LogLevel.Warn, formatProvider, message, args);
		}
        
		public void Warn(string message, params object[] args) 
		{
            Write(LogLevel.Warn, null, message, args);
        }
        
        public void Warn(string message) {
            Write(LogLevel.Warn, null, message, null);
        }
        
		public void Error(IFormatProvider formatProvider, string message, params object[] args) 
		{
			Write(LogLevel.Error, formatProvider, message, args);
		}
        
		public void Error(string message, params object[] args) 
		{
            Write(LogLevel.Error, null, message, args);
        }
        
        public void Error(string message) {
            Write(LogLevel.Error, null, message, null);
        }
        
		public void Fatal(IFormatProvider formatProvider, string message, params object[] args) 
		{
			Write(LogLevel.Fatal, formatProvider, message, args);
		}
        
		public void Fatal(string message, params object[] args) 
		{
            Write(LogLevel.Fatal, null, message, args);
        }
        
        public void Fatal(string message) {
            Write(LogLevel.Fatal, null, message, null);
        }

    }
}
