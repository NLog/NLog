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
using System.Collections;
using System.Text;
using System.IO;

using NLog.Internal;
using NLog.Config;
using System.ComponentModel;

namespace NLog.LayoutRenderers
{
    /// <summary>
    /// Exception information provided through 
    /// a call to one of the Logger.*Exception() methods.
    /// </summary>
    [LayoutRenderer("exception",UsingLogEventInfo=true)]
    public class ExceptionLayoutRenderer: LayoutRenderer
    {
        private string _format;
        private string _separator = " ";

        delegate void ExceptionDataTarget(StringBuilder sb, Exception ex);

        private ExceptionDataTarget[] _exceptionDataTargets = null;

        /// <summary>
        /// Initializes a new instance of <see cref="ExceptionLayoutRenderer"/>.
        /// </summary>
        public ExceptionLayoutRenderer()
        {
            Format = "message";
        }


        /// <summary>
        /// The format of the output. Must be a comma-separated list of exception
        /// properties: Message, Type, ShortType, ToString, Method, StackTrace.
        /// This parameter value is case-insensitive.
        /// </summary>
        [DefaultParameter]
        public string Format
        {
            get { return _format; }
            set { _format = value; CompileFormat(value); }
        }

        /// <summary>
        /// The separator used to concatenate parts specified in the Format.
        /// </summary>
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
            ArrayList dataTargets = new ArrayList();
            
            foreach (string s in parts)
            {
                switch (s.ToLower())
                {
                    case "message":
                        dataTargets.Add(new ExceptionDataTarget(AppendMessage));
                        break;
                        
                    case "type":
                        dataTargets.Add(new ExceptionDataTarget(AppendType));
                        break;
                    
                    case "shorttype":
                        dataTargets.Add(new ExceptionDataTarget(AppendShortType));
                        break;

                    case "tostring":
                        dataTargets.Add(new ExceptionDataTarget(AppendToString));
                        break;

#if !NETCF
                   case "stacktrace":
                        dataTargets.Add(new ExceptionDataTarget(AppendStackTrace));
                        break;

                    case "method":
                        dataTargets.Add(new ExceptionDataTarget(AppendMethod));
                        break;
#endif
                    default:
                        InternalLogger.Warn("Unknown exception data target: {0}", s);
                        break;
                    
                }
            }
            _exceptionDataTargets = (ExceptionDataTarget[])dataTargets.ToArray(typeof(ExceptionDataTarget));
        }

        /// <summary>
        /// Returns the estimated number of characters that are needed to
        /// hold the rendered value for the specified logging event.
        /// </summary>
        /// <param name="logEvent">Logging event information.</param>
        /// <returns>The number of characters.</returns>
        /// <remarks>
        /// If the exact number is not known or
        /// expensive to calculate this function should return a rough estimate
        /// that's big enough in most cases, but not too big, in order to conserve memory.
        /// </remarks>
        protected internal override int GetEstimatedBufferSize(LogEventInfo logEvent)
        {
            return 32;
        }

        /// <summary>
        /// Renders the specified exception information and appends it to the specified <see cref="StringBuilder" />.
        /// </summary>
        /// <param name="builder">The <see cref="StringBuilder"/> to append the rendered data to.</param>
        /// <param name="logEvent">Logging event.</param>
        protected internal override void Append(StringBuilder builder, LogEventInfo logEvent)
        {
            if (logEvent.Exception != null)
            {
                StringBuilder sb2 = new StringBuilder(128);

                for (int i = 0; i < _exceptionDataTargets.Length; ++i)
                {
                    if (i != 0)
                        sb2.Append(Separator);
                    _exceptionDataTargets[i](sb2, logEvent.Exception);
                }
                builder.Append(ApplyPadding(sb2.ToString()));
            }
        }
    }
}
