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
using System.Xml;
using System.IO;
using System.Threading;
using System.Collections;
using System.Collections.Specialized;

using NLog;
using NLog.Config;

using NLog.Internal;
using NLog.Internal.FileAppenders;
using System.Text;
using System.Globalization;
#if !NETCF
using System.Runtime.InteropServices;
using NLog.Internal.Win32;
using System.ComponentModel;
#endif

namespace NLog.Targets
{
    /// <summary>
    /// File target that can produce *.csv (Comma Separated Values) files.
    /// This is an extension to the <a href="target.File.html">File</a> Target.
    /// </summary>
    /// <example>
    /// <p>
    /// To set up the target in the <a href="config.html">configuration file</a>, 
    /// use the following syntax:
    /// </p>
    /// <code lang="XML" src="examples/targets/Configuration File/CSVFile/NLog.config" />
    /// <p>
    /// This assumes just one target and a single rule. More configuration
    /// options are described <a href="config.html">here</a>.
    /// </p>
    /// <p>
    /// To set up the log target programmatically use code like this:
    /// </p>
    /// <code lang="C#" src="examples/targets/Configuration API/CSVFile/Simple/Example.cs" />
    /// <p>More examples can be found in <a href="target.File.html">File</a> Target reference.</p>
    /// </example>
    [Target("CSVFile", IgnoresLayout = true)]
    public class CsvFileTarget: FileTarget
    {
        /// <summary>
        /// Specifies allowed column delimiters.
        /// </summary>
        public enum ColumnDelimiterMode
        {
            /// <summary>
            /// Automatically detect from regional settings.
            /// </summary>
            Auto,

            /// <summary>
            /// Comma (ASCII 44)
            /// </summary>
            Comma,

            /// <summary>
            /// Semicolon (ASCII 59)
            /// </summary>
            Semicolon,

            /// <summary>
            /// Tab character (ASCII 9)
            /// </summary>
            Tab,

            /// <summary>
            /// Pipe character (ASCII 124)
            /// </summary>
            Pipe,

            /// <summary>
            /// Space character (ASCII 32)
            /// </summary>
            Space,

            /// <summary>
            /// Custom string, specified by the CustomDelimiter
            /// </summary>
            Custom,
        }


        private CsvFileColumnCollection _columns = new CsvFileColumnCollection();
        private ColumnDelimiterMode _columnDelimiter = ColumnDelimiterMode.Auto;
        private CsvQuotingMode _quotingMode = CsvQuotingMode.Auto;
        private char[] _quotableCharacters;
        private string _quoteChar = "\"";
        private string _doubleQuoteChar;
        private string _customColumnDelimiter;
        private string _actualColumnDelimiter;

        /// <summary>
        /// Array of parameters to be passed.
        /// </summary>
        [ArrayParameter(typeof(CsvFileColumn), "column")]
        public CsvFileColumnCollection Columns
        {
            get { return _columns; }
        }

        /// <summary>
        /// Column delimiter.
        /// </summary>
        [System.ComponentModel.DefaultValue("Auto")]
        public ColumnDelimiterMode Delimiter
        {
            get { return _columnDelimiter; }
            set { _columnDelimiter = value; }
        }

        /// <summary>
        /// Quoting mode.
        /// </summary>
        public CsvQuotingMode Quoting
        {
            get { return _quotingMode; }
            set { _quotingMode = value; }
        }

        /// <summary>
        /// Quote Character
        /// </summary>
        [System.ComponentModel.DefaultValue("\"")]
        public string QuoteChar
        {
            get { return _quoteChar; }
            set { _quoteChar = value; }
        }

        /// <summary>
        /// Custom column delimiter value (valid when ColumnDelimiter is set to 'Custom')
        /// </summary>
        public string CustomColumnDelimiter
        {
            get { return _customColumnDelimiter; }
            set { _customColumnDelimiter = value; }
        }

        /// <summary>
        /// Formats the log event for write.
        /// </summary>
        /// <param name="logEvent">The log event to be formatted.</param>
        /// <returns>A string representation of the log event.</returns>
        protected override string GetFormattedMessage(LogEventInfo logEvent)
        {
            StringBuilder sb = new StringBuilder();

            bool first = true;

            foreach (CsvFileColumn col in Columns)
            {
                if (!first)
                {
                    sb.Append(_actualColumnDelimiter);
                }

                first = false;

                bool useQuoting;
                string text = col.CompiledLayout.GetFormattedMessage(logEvent);

                switch (Quoting)
                {
                    case CsvQuotingMode.Nothing:
                        useQuoting = false;
                        break;

                    case CsvQuotingMode.All:
                        useQuoting = true;
                        break;

                    default:
                    case CsvQuotingMode.Auto:
                        if (text.IndexOfAny(_quotableCharacters) >= 0)
                            useQuoting = true;
                        else
                            useQuoting = false;
                        break;
                }

                if (useQuoting)
                    sb.Append(QuoteChar);

                if (useQuoting)
                    sb.Append(text.Replace(QuoteChar, _doubleQuoteChar));
                else
                    sb.Append(text);

                if (useQuoting)
                    sb.Append(QuoteChar);
            }

            return sb.ToString();
        }

        /// <summary>
        /// Initializes the target.
        /// </summary>
        public override void Initialize()
        {
            base.Initialize();
            switch (Delimiter)
            {
                case ColumnDelimiterMode.Auto:
                    _actualColumnDelimiter = CultureInfo.CurrentCulture.TextInfo.ListSeparator;
                    break;

                case ColumnDelimiterMode.Comma:
                    _actualColumnDelimiter = ",";
                    break;

                case ColumnDelimiterMode.Semicolon:
                    _actualColumnDelimiter = ";";
                    break;

                case ColumnDelimiterMode.Pipe:
                    _actualColumnDelimiter = "|";
                    break;

                case ColumnDelimiterMode.Tab:
                    _actualColumnDelimiter = "\t";
                    break;

                case ColumnDelimiterMode.Space:
                    _actualColumnDelimiter = " ";
                    break;

                case ColumnDelimiterMode.Custom:
                    _actualColumnDelimiter = _customColumnDelimiter;
                    break;
            }
            _quotableCharacters = (QuoteChar + NewLineChars + _actualColumnDelimiter).ToCharArray();
            _doubleQuoteChar = _quoteChar + _quoteChar;
        }
    }
}
