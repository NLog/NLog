// 
// Copyright (c) 2004 Jaroslaw Kowalski <jkowalski@users.sourceforge.net>
// 
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
using System.Text;
using System.IO;

using NLog.Internal;
using NLog.Config;

namespace NLog.LayoutAppenders
{
    [LayoutAppender("exception")]
    public class ExceptionLayoutAppender: LayoutAppender
    {
        private string _format;
        private string _separator = " ";

        delegate void ExceptionDataAppender(StringBuilder sb, Exception ex);

        private ExceptionDataAppender[] _exceptionDataAppenders = null;

        public ExceptionLayoutAppender()
        {
            Format = "message";
        }

        public string Format
        {
            get { return _format; }
            set { _format = value; CompileFormat(value); }
        }

        public string Separator
        {
            get { return _separator; }
            set { _separator = value; }
        }

        private void AppendMessage(StringBuilder sb, Exception ex) {
            sb.Append(ex.Message);
        }

#if !NETCF
        private void AppendMethod(StringBuilder sb, Exception ex) {
            sb.Append(ex.TargetSite.ToString());
        }

        private void AppendStackTrace(StringBuilder sb, Exception ex) 
        {
            sb.Append(ex.StackTrace);
        }
#endif

        private void AppendToString(StringBuilder sb, Exception ex) 
        {
            sb.Append(ex.ToString());
        }

        private void AppendType(StringBuilder sb, Exception ex) 
        {
            sb.Append(ex.GetType().FullName);
        }

        private void AppendShortType(StringBuilder sb, Exception ex) 
        {
            sb.Append(ex.GetType().Name);
        }

        private void CompileFormat(string format)
        {
            string[] parts = format.Replace(" ","").Split(',');
            ArrayList dataAppenders = new ArrayList();
            
            foreach (string s in parts)
            {
                switch (s.ToLower())
                {
                    case "message":
                        dataAppenders.Add(new ExceptionDataAppender(AppendMessage));
                        break;
                        
                    case "type":
                        dataAppenders.Add(new ExceptionDataAppender(AppendType));
                        break;
                    
                    case "shorttype":
                        dataAppenders.Add(new ExceptionDataAppender(AppendShortType));
                        break;

                    case "tostring":
                        dataAppenders.Add(new ExceptionDataAppender(AppendToString));
                        break;

#if !NETCF
                        case "stacktrace":
                        dataAppenders.Add(new ExceptionDataAppender(AppendStackTrace));
                        break;

                    case "method":
                        dataAppenders.Add(new ExceptionDataAppender(AppendMethod));
                        break;
#endif
                    default:
                        InternalLogger.Warn("Unknown exception data appender: {0}", s);
                        break;
                    
                }
            }
            _exceptionDataAppenders = (ExceptionDataAppender[])dataAppenders.ToArray(typeof(ExceptionDataAppender));
        }

        protected internal override int GetEstimatedBufferSize(LogEventInfo ev)
        {
            return 32;
        }

        protected internal override void Append(StringBuilder builder, LogEventInfo ev)
        {
            if (ev.Exception != null)
            {
                StringBuilder sb2 = new StringBuilder(128);

                for (int i = 0; i < _exceptionDataAppenders.Length; ++i)
                {
                    if (i != 0)
                        sb2.Append(Separator);
                    _exceptionDataAppenders[i](sb2, ev.Exception);
                }
                builder.Append(ApplyPadding(sb2.ToString()));
            }
        }
    }
}
