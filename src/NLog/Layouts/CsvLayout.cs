using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Text;
using NLog.Config;
using NLog.Internal;

namespace NLog.Layouts
{
    /// <summary>
    /// A specialized layout that renders CSV-formatted events.
    /// </summary>
    [Layout("CsvLayout")]
    public class CsvLayout : Layout
    {
        private ICollection<CsvColumn> columns = new List<CsvColumn>();
        private char[] quotableCharacters;
        private string doubleQuoteChar;
        private string actualColumnDelimiter;
        private Layout thisHeader;

        /// <summary>
        /// Initializes a new instance of the <see cref="CsvLayout"/> class.
        /// </summary>
        public CsvLayout()
        {
            this.WithHeader = true;
            this.Delimiter = ColumnDelimiterMode.Auto;
            this.Quoting = CsvQuotingMode.Auto;
            this.QuoteChar = "\"";
            this.thisHeader = new CsvHeaderLayout(this);
        }

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
            /// Comma (ASCII 44).
            /// </summary>
            Comma,

            /// <summary>
            /// Semicolon (ASCII 59).
            /// </summary>
            Semicolon,

            /// <summary>
            /// Tab character (ASCII 9).
            /// </summary>
            Tab,

            /// <summary>
            /// Pipe character (ASCII 124).
            /// </summary>
            Pipe,

            /// <summary>
            /// Space character (ASCII 32).
            /// </summary>
            Space,

            /// <summary>
            /// Custom string, specified by the CustomDelimiter.
            /// </summary>
            Custom,
        }

        /// <summary>
        /// Gets the array of parameters to be passed.
        /// </summary>
        [ArrayParameter(typeof(CsvColumn), "column")]
        public ICollection<CsvColumn> Columns
        {
            get { return this.columns; }
        }

        /// <summary>
        /// Gets or sets a value indicating whether CVS should include header.
        /// </summary>
        /// <value>A value of <c>true</c> if CVS should include header; otherwise, <c>false</c>.</value>
        public bool WithHeader { get; set; }

        /// <summary>
        /// Gets or sets the column delimiter.
        /// </summary>
        [DefaultValue("Auto")]
        public ColumnDelimiterMode Delimiter { get; set; }

        /// <summary>
        /// Gets or sets the quoting mode.
        /// </summary>
        [DefaultValue("Auto")]
        public CsvQuotingMode Quoting { get; set; }

        /// <summary>
        /// Gets or sets the quote Character.
        /// </summary>
        [DefaultValue("\"")]
        public string QuoteChar { get; set; }

        /// <summary>
        /// Gets or sets the custom column delimiter value (valid when ColumnDelimiter is set to 'Custom').
        /// </summary>
        public string CustomColumnDelimiter { get; set; }

        /// <summary>
        /// Gets or sets the main layout (can be repeated multiple times).
        /// </summary>
        /// <value>
        /// </value>
        public Layout Layout
        {
            get { return this; }
            set { throw new Exception("Cannot modify the layout of CsvLayout"); }
        }

        /// <summary>
        /// Gets or sets the CSV Header.
        /// </summary>
        public Layout Header
        {
            get
            {
                if (this.WithHeader)
                {
                    return this.thisHeader;
                }
                else
                {
                    return null;
                }
            }

            set
            {
            }
        }

        /// <summary>
        /// Gets or sets the CSV Footer.
        /// </summary>
        /// <remarks>
        /// CSV has no footer.
        /// </remarks>
        public Layout Footer
        {
            get { return null; }
            set { }
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
            {
                return cachedValue;
            }

            StringBuilder sb = new StringBuilder();
            bool first = true;

            foreach (CsvColumn col in this.Columns)
            {
                if (!first)
                {
                    sb.Append(this.actualColumnDelimiter);
                }

                first = false;

                bool useQuoting;
                string text = col.Layout.GetFormattedMessage(logEvent);

                switch (this.Quoting)
                {
                    case CsvQuotingMode.Nothing:
                        useQuoting = false;
                        break;

                    case CsvQuotingMode.All:
                        useQuoting = true;
                        break;

                    default:
                    case CsvQuotingMode.Auto:
                        if (text.IndexOfAny(this.quotableCharacters) >= 0)
                        {
                            useQuoting = true;
                        }
                        else
                        {
                            useQuoting = false;
                        }

                        break;
                }

                if (useQuoting)
                {
                    sb.Append(this.QuoteChar);
                }

                if (useQuoting)
                {
                    sb.Append(text.Replace(this.QuoteChar, this.doubleQuoteChar));
                }
                else
                {
                    sb.Append(text);
                }

                if (useQuoting)
                {
                    sb.Append(this.QuoteChar);
                }
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
            switch (this.Delimiter)
            {
                case ColumnDelimiterMode.Auto:
                    this.actualColumnDelimiter = CultureInfo.CurrentCulture.TextInfo.ListSeparator;
                    break;

                case ColumnDelimiterMode.Comma:
                    this.actualColumnDelimiter = ",";
                    break;

                case ColumnDelimiterMode.Semicolon:
                    this.actualColumnDelimiter = ";";
                    break;

                case ColumnDelimiterMode.Pipe:
                    this.actualColumnDelimiter = "|";
                    break;

                case ColumnDelimiterMode.Tab:
                    this.actualColumnDelimiter = "\t";
                    break;

                case ColumnDelimiterMode.Space:
                    this.actualColumnDelimiter = " ";
                    break;

                case ColumnDelimiterMode.Custom:
                    this.actualColumnDelimiter = this.CustomColumnDelimiter;
                    break;
            }

            this.quotableCharacters = (this.QuoteChar + "\r\n" + this.actualColumnDelimiter).ToCharArray();
            this.doubleQuoteChar = this.QuoteChar + this.QuoteChar;

            foreach (CsvColumn c in this.Columns)
            {
                c.Layout.Initialize();
            }
        }

        /// <summary>
        /// Returns the value indicating whether a stack trace and/or the source file
        /// information should be gathered during layout processing.
        /// </summary>
        /// <returns>0 - don't include stack trace<br/>1 - include stack trace without source file information<br/>2 - include full stack trace.</returns>
        public override StackTraceUsage GetStackTraceUsage()
        {
            StackTraceUsage stu = StackTraceUsage.None;

            foreach (CsvColumn cc in this.columns)
            {
                stu = StackTraceUsageUtils.Max(stu, cc.Layout.GetStackTraceUsage());
            }

            return stu;
        }

        /// <summary>
        /// Returns the value indicating whether this layout includes any volatile 
        /// layout renderers.
        /// </summary>
        /// <returns>A value of <see langword="true" /> when the layout includes at least 
        /// one volatile renderer, <see langword="false"/> otherwise.</returns>
        /// <remarks>
        /// Volatile layout renderers are dependent on information not contained 
        /// in <see cref="LogEventInfo"/> (such as thread-specific data, MDC data, NDC data).
        /// </remarks>
        public override bool IsVolatile()
        {
            foreach (CsvColumn cc in this.columns)
            {
                if (cc.Layout.IsVolatile())
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Closes the layout.
        /// </summary>
        public override void Close()
        {
            foreach (CsvColumn c in this.Columns)
            {
                c.Layout.Close();
            }
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

            foreach (CsvColumn col in this.Columns)
            {
                if (!first)
                {
                    sb.Append(this.actualColumnDelimiter);
                }

                first = false;

                bool useQuoting;
                string text = col.Name;

                switch (this.Quoting)
                {
                    case CsvQuotingMode.Nothing:
                        useQuoting = false;
                        break;

                    case CsvQuotingMode.All:
                        useQuoting = true;
                        break;

                    default:
                    case CsvQuotingMode.Auto:
                        if (text.IndexOfAny(this.quotableCharacters) >= 0)
                        {
                            useQuoting = true;
                        }
                        else
                        {
                            useQuoting = false;
                        }

                        break;
                }

                if (useQuoting)
                {
                    sb.Append(this.QuoteChar);
                }

                if (useQuoting)
                {
                    sb.Append(text.Replace(this.QuoteChar, this.doubleQuoteChar));
                }
                else
                {
                    sb.Append(text);
                }

                if (useQuoting)
                {
                    sb.Append(this.QuoteChar);
                }
            }

            return sb.ToString();
        }

        /// <summary>
        /// Header for CSV layout.
        /// </summary>
        private class CsvHeaderLayout : Layout
        {
            private CsvLayout parent;

            /// <summary>
            /// Initializes a new instance of the <see cref="CsvHeaderLayout"/> class.
            /// </summary>
            /// <param name="parent">The parent.</param>
            public CsvHeaderLayout(CsvLayout parent)
            {
                this.parent = parent;
            }

            /// <summary>
            /// Renders the layout for the specified logging event by invoking layout renderers.
            /// </summary>
            /// <param name="logEvent">The logging event.</param>
            /// <returns>The rendered layout.</returns>
            public override string GetFormattedMessage(LogEventInfo logEvent)
            {
                return this.parent.GetHeader(logEvent);
            }

            /// <summary>
            /// Returns the value indicating whether a stack trace and/or the source file
            /// information should be gathered during layout processing.
            /// </summary>
            /// <returns>
            /// 0 - don't include stack trace<br/>1 - include stack trace without source file information<br/>2 - include full stack trace
            /// .</returns>
            public override StackTraceUsage GetStackTraceUsage()
            {
                return 0;
            }

            /// <summary>
            /// Returns the value indicating whether this layout includes any volatile
            /// layout renderers.
            /// </summary>
            /// <returns>
            /// A value of <see langword="true"/> when the layout includes at least
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
