// 
// Copyright (c) 2004-2016 Jaroslaw Kowalski <jaak@jkowalski.net>, Kim Christensen, Julian Verdurmen
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
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Globalization;
    using System.Text;
    using NLog.Common;
    using NLog.Config;
    using NLog.Internal;

    /// <summary>
    /// Exception information provided through 
    /// a call to one of the Logger.*Exception() methods.
    /// </summary>
    [LayoutRenderer("exception")]
    [ThreadAgnostic]
    public class ExceptionLayoutRenderer : LayoutRenderer
    {
        private string format;
        private string innerFormat = string.Empty;
        private readonly Dictionary<ExceptionRenderingFormat, Action<StringBuilder, Exception>> _renderingfunctions;

        private static readonly Dictionary<String, ExceptionRenderingFormat> _formatsMapping = new Dictionary<string, ExceptionRenderingFormat>(StringComparer.OrdinalIgnoreCase)
                                                                                                    {
                                                                                                        {"MESSAGE",ExceptionRenderingFormat.Message}, 
                                                                                                        {"TYPE", ExceptionRenderingFormat.Type}, 
                                                                                                        {"SHORTTYPE",ExceptionRenderingFormat.ShortType}, 
                                                                                                        {"TOSTRING",ExceptionRenderingFormat.ToString}, 
                                                                                                        {"METHOD",ExceptionRenderingFormat.Method}, 
                                                                                                        {"STACKTRACE", ExceptionRenderingFormat.StackTrace}, 
                                                                                                        {"DATA",ExceptionRenderingFormat.Data}, 
                                                                                                    };

        /// <summary>
        /// Initializes a new instance of the <see cref="ExceptionLayoutRenderer" /> class.
        /// </summary>
        public ExceptionLayoutRenderer()
        {
            this.Format = "message";
            this.Separator = " ";
            this.InnerExceptionSeparator = EnvironmentHelper.NewLine;
            this.MaxInnerExceptionLevel = 0;

            _renderingfunctions = new Dictionary<ExceptionRenderingFormat, Action<StringBuilder, Exception>>() 
                                                                                                    {
                                                                                                        {ExceptionRenderingFormat.Message, AppendMessage}, 
                                                                                                        {ExceptionRenderingFormat.Type, AppendType},
                                                                                                        {ExceptionRenderingFormat.ShortType, AppendShortType},
                                                                                                        {ExceptionRenderingFormat.ToString, AppendToString},
                                                                                                        {ExceptionRenderingFormat.Method, AppendMethod},
                                                                                                        {ExceptionRenderingFormat.StackTrace, AppendStackTrace},
                                                                                                        {ExceptionRenderingFormat.Data, AppendData}
                                                                                                    };
        }

        /// <summary>
        /// Gets or sets the format of the output. Must be a comma-separated list of exception
        /// properties: Message, Type, ShortType, ToString, Method, StackTrace.
        /// This parameter value is case-insensitive.
        /// </summary>
        /// <see cref="Formats"/>
        /// <see cref="ExceptionRenderingFormat"/>
        /// <docgen category='Rendering Options' order='10' />
        [DefaultParameter]
        public string Format
        {
            get
            {
                return this.format;
            }

            set
            {
                this.format = value;
                Formats = CompileFormat(value);
            }
        }

        /// <summary>
        /// Gets or sets the format of the output of inner exceptions. Must be a comma-separated list of exception
        /// properties: Message, Type, ShortType, ToString, Method, StackTrace.
        /// This parameter value is case-insensitive.
        /// </summary>
        /// <docgen category='Rendering Options' order='10' />
        public string InnerFormat
        {
            get
            {
                return this.innerFormat;
            }

            set
            {
                this.innerFormat = value;
                InnerFormats = CompileFormat(value);
            }
        }

        /// <summary>
        /// Gets or sets the separator used to concatenate parts specified in the Format.
        /// </summary>
        /// <docgen category='Rendering Options' order='10' />
        [DefaultValue(" ")]
        public string Separator { get; set; }

        /// <summary>
        /// Gets or sets the maximum number of inner exceptions to include in the output.
        /// By default inner exceptions are not enabled for compatibility with NLog 1.0.
        /// </summary>
        /// <docgen category='Rendering Options' order='10' />
        [DefaultValue(0)]
        public int MaxInnerExceptionLevel { get; set; }

        /// <summary>
        /// Gets or sets the separator between inner exceptions.
        /// </summary>
        /// <docgen category='Rendering Options' order='10' />
        public string InnerExceptionSeparator { get; set; }

        /// <summary>
        ///  Gets the formats of the output of inner exceptions to be rendered in target.
        /// </summary>
        /// <docgen category='Rendering Options' order='10' />
        /// <see cref="ExceptionRenderingFormat"/>
        public List<ExceptionRenderingFormat> Formats
        {
            get;
            private set;
        }

        /// <summary>
        ///  Gets the formats of the output to be rendered in target.
        /// </summary>
        /// <docgen category='Rendering Options' order='10' />
        /// <see cref="ExceptionRenderingFormat"/>
        public List<ExceptionRenderingFormat> InnerFormats
        {
            get;
            private set;
        }

        /// <summary>
        /// Renders the specified exception information and appends it to the specified <see cref="StringBuilder" />.
        /// </summary>
        /// <param name="builder">The <see cref="StringBuilder"/> to append the rendered data to.</param>
        /// <param name="logEvent">Logging event.</param>
        protected override void Append(StringBuilder builder, LogEventInfo logEvent)
        {
            if (logEvent.Exception != null)
            {
                var sb2 = new StringBuilder(128);
                string separator = string.Empty;

                foreach (ExceptionRenderingFormat renderingFormat in this.Formats)
                {
                    sb2.Append(separator);

                    var currentRenderFunction = _renderingfunctions[renderingFormat];

                    currentRenderFunction(sb2, logEvent.Exception);

                    separator = this.Separator;
                }
                Exception currentException = logEvent.Exception.InnerException;
                int currentLevel = 0;
                while (currentException != null && currentLevel < this.MaxInnerExceptionLevel)
                {
                    // separate inner exceptions
                    sb2.Append(this.InnerExceptionSeparator);

                    separator = string.Empty;
                    foreach (ExceptionRenderingFormat renderingFormat in this.InnerFormats ?? this.Formats)
                    {
                        sb2.Append(separator);

                        var currentRenderFunction = _renderingfunctions[renderingFormat];

                        currentRenderFunction(sb2, currentException);

                        separator = this.Separator;
                    }

                    currentException = currentException.InnerException;
                    currentLevel++;
                }

                builder.Append(sb2.ToString());
            }
        }

        /// <summary>
        /// Appends the Message of an Exception to the specified <see cref="StringBuilder" />.
        /// </summary>
        /// <param name="sb">The <see cref="StringBuilder"/> to append the rendered data to.</param>
        /// <param name="ex">The exception containing the Message to append.</param>        
        protected virtual void AppendMessage(StringBuilder sb, Exception ex)
        {
            try
            {
                sb.Append(ex.Message);
            }
            catch (Exception exception)
            {
                var message = string.Format("Exception in {0}.AppendMessage(): {1}.", typeof(ExceptionLayoutRenderer).FullName, exception.GetType().FullName);
                sb.Append("NLog message:" + message);

                if (InternalLogger.IsWarnEnabled)
                {
                    InternalLogger.Warn(message);
                }
            }
        }

        /// <summary>
        /// Appends the method name from Exception's stack trace to the specified <see cref="StringBuilder" />.
        /// </summary>
        /// <param name="sb">The <see cref="StringBuilder"/> to append the rendered data to.</param>
        /// <param name="ex">The Exception whose method name should be appended.</param>        
        protected virtual void AppendMethod(StringBuilder sb, Exception ex)
        {
#if SILVERLIGHT
            sb.Append(ParseMethodNameFromStackTrace(ex.StackTrace));
#else
            if (ex.TargetSite != null)
            {
                sb.Append(ex.TargetSite.ToString());
            }
#endif
        }

        /// <summary>
        /// Appends the stack trace from an Exception to the specified <see cref="StringBuilder" />.
        /// </summary>
        /// <param name="sb">The <see cref="StringBuilder"/> to append the rendered data to.</param>
        /// <param name="ex">The Exception whose stack trace should be appended.</param>        
        protected virtual void AppendStackTrace(StringBuilder sb, Exception ex)
        {
            sb.Append(ex.StackTrace);
        }

        /// <summary>
        /// Appends the result of calling ToString() on an Exception to the specified <see cref="StringBuilder" />.
        /// </summary>
        /// <param name="sb">The <see cref="StringBuilder"/> to append the rendered data to.</param>
        /// <param name="ex">The Exception whose call to ToString() should be appended.</param>       
        protected virtual void AppendToString(StringBuilder sb, Exception ex)
        {
            sb.Append(ex.ToString());
        }

        /// <summary>
        /// Appends the type of an Exception to the specified <see cref="StringBuilder" />.
        /// </summary>
        /// <param name="sb">The <see cref="StringBuilder"/> to append the rendered data to.</param>
        /// <param name="ex">The Exception whose type should be appended.</param>        
        protected virtual void AppendType(StringBuilder sb, Exception ex)
        {
            sb.Append(ex.GetType().FullName);
        }

        /// <summary>
        /// Appends the short type of an Exception to the specified <see cref="StringBuilder" />.
        /// </summary>
        /// <param name="sb">The <see cref="StringBuilder"/> to append the rendered data to.</param>
        /// <param name="ex">The Exception whose short type should be appended.</param>
        protected virtual void AppendShortType(StringBuilder sb, Exception ex)
        {
            sb.Append(ex.GetType().Name);
        }

        /// <summary>
        /// Appends the contents of an Exception's Data property to the specified <see cref="StringBuilder" />.
        /// </summary>
        /// <param name="sb">The <see cref="StringBuilder"/> to append the rendered data to.</param>
        /// <param name="ex">The Exception whose Data property elements should be appended.</param>
        protected virtual void AppendData(StringBuilder sb, Exception ex)
        {
            string separator = string.Empty;
            foreach (var key in ex.Data.Keys)
            {
                sb.Append(separator);
                sb.AppendFormat("{0}: {1}", key, ex.Data[key]);
                separator = ";";
            }
        }

        /// <summary>
        /// Split the string and then compile into list of Rendering formats.
        /// </summary>
        /// <param name="formatSpecifier"></param>
        /// <returns></returns>
        private static List<ExceptionRenderingFormat> CompileFormat(string formatSpecifier)
        {
            List<ExceptionRenderingFormat> formats = new List<ExceptionRenderingFormat>();
            string[] parts = formatSpecifier.Replace(" ", string.Empty).Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries);

            foreach (string s in parts)
            {
                ExceptionRenderingFormat renderingFormat;
                if (_formatsMapping.TryGetValue(s, out renderingFormat))
                {
                    formats.Add(renderingFormat);
                }
                else
                {
                    InternalLogger.Warn("Unknown exception data target: {0}", s);
                }
            }
            return formats;
        }
#if SILVERLIGHT
        /// <summary>
        /// Find name of method on stracktrace.
        /// </summary>
        /// <param name="stackTrace">Full stracktrace</param>
        /// <returns></returns>
        protected static string ParseMethodNameFromStackTrace(string stackTrace)
        {
            // get the first line of the stack trace
            string stackFrameLine;

            int p = stackTrace.IndexOfAny(new[] { '\r', '\n' });
            if (p >= 0)
            {
                stackFrameLine = stackTrace.Substring(0, p);
            }
            else
            {
                stackFrameLine = stackTrace;
            }

            // stack trace is composed of lines which look like this
            //
            // at NLog.UnitTests.LayoutRenderers.ExceptionTests.GenericClass`3.Method2[T1,T2,T3](T1 aaa, T2 b, T3 o, Int32 i, DateTime now, Nullable`1 gfff, List`1[] something)
            //
            // "at " prefix can be localized so we cannot hard-code it but it's followed by a space, class name (which does not have a space in it) and opening parenthesis
            int lastSpace = -1;
            int startPos = 0;
            int endPos = stackFrameLine.Length;

            for (int i = 0; i < stackFrameLine.Length; ++i)
            {
                switch (stackFrameLine[i])
                {
                    case ' ':
                        lastSpace = i;
                        break;

                    case '(':
                        startPos = lastSpace + 1;
                        break;

                    case ')':
                        endPos = i + 1;

                        // end the loop
                        i = stackFrameLine.Length;
                        break;
                }
            }

            return stackTrace.Substring(startPos, endPos - startPos);
        }
#endif
    }
}