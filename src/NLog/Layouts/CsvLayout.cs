using NLog.LayoutRenderers;
using System.Text;
using NLog.Config;
using System.Globalization;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using NLog.Internal;
namespace NLog.Layouts
{
    /// <summary>
    /// A specialized layout that renders CSV-formatted events.
    /// </summary>
    [Layout("CSVLayout")]
    public class CsvLayout : Layout
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


        private ICollection<CsvColumn> _columns = new List<CsvColumn>();
        private ColumnDelimiterMode _columnDelimiter = ColumnDelimiterMode.Auto;
        private CsvQuotingMode _quotingMode = CsvQuotingMode.Auto;
        private char[] _quotableCharacters;
        private string _quoteChar = "\"";
        private string _doubleQuoteChar;
        private string _customColumnDelimiter;
        private string _actualColumnDelimiter;
        private Layout _thisHeader;
        private bool _withHeader = true;

        /// <summary>
        /// Initializes a new instance of the <see cref="CsvLayout"/> class.
        /// </summary>
        public CsvLayout()
        {
            _thisHeader = new CsvHeaderLayout(this);
        }

        /// <summary>
        /// Array of parameters to be passed.
        /// </summary>
        [ArrayParameter(typeof(CsvColumn), "column")]
        public ICollection<CsvColumn> Columns
        {
            get { return _columns; }
        }

        /// <summary>
        /// Whether CVS should include header.
        /// </summary>
        /// <value><c>true</c> if CVS should include header; otherwise, <c>false</c>.</value>
        public bool WithHeader
        {
            get { return _withHeader; }
            set { _withHeader = value; }
        }

        /// <summary>
        /// Column delimiter.
        /// </summary>
        [DefaultValue("Auto")]
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
        [DefaultValue("\"")]
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
        public override string GetFormattedMessage(LogEventInfo logEvent)
        {
            string cachedValue;

            if (logEvent.TryGetCachedLayoutValue(this, out cachedValue))
                return cachedValue;

            StringBuilder sb = new StringBuilder();

            bool first = true;

            foreach (CsvColumn col in Columns)
            {
                if (!first)
                {
                    sb.Append(_actualColumnDelimiter);
                }

                first = false;

                bool useQuoting;
                string text = col.Layout.GetFormattedMessage(logEvent);

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

            logEvent.AddCachedLayoutValue(this, sb.ToString());
            return sb.ToString();
        }

        /// <summary>
        /// Initializes the layout.
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
            _quotableCharacters = (QuoteChar + "\r\n" + _actualColumnDelimiter).ToCharArray();
            _doubleQuoteChar = _quoteChar + _quoteChar;

            foreach (CsvColumn c in Columns)
            {
                c.Layout.Initialize();
            }
        }

        /// <summary>
        /// Returns the value indicating whether a stack trace and/or the source file
        /// information should be gathered during layout processing.
        /// </summary>
        /// <returns>0 - don't include stack trace<br/>1 - include stack trace without source file information<br/>2 - include full stack trace</returns>
        public override StackTraceUsage GetStackTraceUsage()
        {
            StackTraceUsage stu = 0;

            foreach (CsvColumn cc in _columns)
            {
                stu = StackTraceUsageUtils.Max(stu, cc.Layout.GetStackTraceUsage());
            }
            return stu;
        }

        /// <summary>
        /// Returns the value indicating whether this layout includes any volatile 
        /// layout renderers.
        /// </summary>
        /// <returns><see langword="true" /> when the layout includes at least 
        /// one volatile renderer, <see langword="false"/> otherwise.</returns>
        /// <remarks>
        /// Volatile layout renderers are dependent on information not contained 
        /// in <see cref="LogEventInfo"/> (such as thread-specific data, MDC data, NDC data).
        /// </remarks>
        public override bool IsVolatile()
        {
            foreach (CsvColumn cc in _columns)
            {
                if (cc.Layout.IsVolatile())
                    return true;
            }
            return false;
        }

        /// <summary>
        /// Closes the layout.
        /// </summary>
        public override void Close()
        {
            foreach (CsvColumn c in Columns)
                c.Layout.Close();
        }

        /// <summary>
        /// Gets the header.
        /// </summary>
        /// <param name="logEvent">The log event to be formatted.</param>
        /// <returns>A string representation of the log event.</returns>
        public string GetHeader(LogEventInfo logEvent)
        {
            StringBuilder sb = new StringBuilder();

            bool first = true;

            foreach (CsvColumn col in Columns)
            {
                if (!first)
                {
                    sb.Append(_actualColumnDelimiter);
                }

                first = false;

                bool useQuoting;
                string text = col.Name;

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
        /// Main layout (can be repeated multiple times)
        /// </summary>
        /// <value></value>
        public Layout Layout
        {
            get { return this; }
            set { throw new Exception("Cannot modify the layout of CsvLayout"); }
        }

        /// <summary>
        /// Header
        /// </summary>
        /// <value></value>
        public Layout Header
        {
            get
            {
                if (WithHeader)
                    return _thisHeader;
                else
                    return null;
            }
            set { }
        }

        /// <summary>
        /// Footer
        /// </summary>
        /// <remarks>CSV has no footer.</remarks>
        public Layout Footer
        {
            get { return null; }
            set { }
        }

        class CsvHeaderLayout : Layout
        {
            private CsvLayout _parent;

            /// <summary>
            /// Initializes a new instance of the <see cref="CsvHeaderLayout"/> class.
            /// </summary>
            /// <param name="parent">The parent.</param>
            public CsvHeaderLayout(CsvLayout parent)
            {
                _parent = parent;
            }

            /// <summary>
            /// Renders the layout for the specified logging event by invoking layout renderers.
            /// </summary>
            /// <param name="logEvent">The logging event.</param>
            /// <returns>The rendered layout.</returns>
            public override string GetFormattedMessage(LogEventInfo logEvent)
            {
                return _parent.GetHeader(logEvent);
            }

            /// <summary>
            /// Returns the value indicating whether a stack trace and/or the source file
            /// information should be gathered during layout processing.
            /// </summary>
            /// <returns>
            /// 0 - don't include stack trace<br/>1 - include stack trace without source file information<br/>2 - include full stack trace
            /// </returns>
            public override StackTraceUsage GetStackTraceUsage()
            {
                return 0;
            }

            /// <summary>
            /// Returns the value indicating whether this layout includes any volatile
            /// layout renderers.
            /// </summary>
            /// <returns>
            /// 	<see langword="true"/> when the layout includes at least
            /// one volatile renderer, <see langword="false"/> otherwise.
            /// </returns>
            /// <remarks>
            /// Volatile layout renderers are dependent on information not contained
            /// in <see cref="LogEventInfo"/> (such as thread-specific data, MDC data, NDC data).
            /// </remarks>
            public override bool IsVolatile()
            {
                return false;
            }
        }
    }
}
