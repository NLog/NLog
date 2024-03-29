// 
// Copyright (c) 2004-2021 Jaroslaw Kowalski <jaak@jkowalski.net>, Kim Christensen, Julian Verdurmen
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

namespace NLog.Layouts
{
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Text;
    using NLog.Config;
    using NLog.Internal;

    /// <summary>
    /// A specialized layout that renders CSV-formatted events.
    /// </summary>
    /// <remarks>
    /// <para>
    /// If <see cref="LayoutWithHeaderAndFooter.Header"/> is set, then the header generation with column names will be disabled.
    /// </para>
    /// <a href="https://github.com/NLog/NLog/wiki/CsvLayout">See NLog Wiki</a>
    /// </remarks>
    /// <seealso href="https://github.com/NLog/NLog/wiki/CsvLayout">Documentation on NLog Wiki</seealso>
    [Layout("CsvLayout")]
    [ThreadAgnostic]
    [AppDomainFixedOutput]
    public class CsvLayout : LayoutWithHeaderAndFooter
    {
        private string _actualColumnDelimiter;
        private string _doubleQuoteChar;
        private char[] _quotableCharacters;
        private Layout[] _precalculateLayouts;

        /// <summary>
        /// Initializes a new instance of the <see cref="CsvLayout"/> class.
        /// </summary>
        public CsvLayout()
        {
            Layout = this;
            Header = new CsvHeaderLayout(this);
            Footer = null;
        }

        /// <summary>
        /// Gets the array of parameters to be passed.
        /// </summary>
        /// <docgen category='Layout Options' order='10' />
        [ArrayParameter(typeof(CsvColumn), "column")]
        public IList<CsvColumn> Columns { get; } = new List<CsvColumn>();

        /// <summary>
        /// Gets or sets a value indicating whether CVS should include header.
        /// </summary>
        /// <value>A value of <c>true</c> if CVS should include header; otherwise, <c>false</c>.</value>
        /// <docgen category='Layout Options' order='10' />
        public bool WithHeader { get; set; } = true;

        /// <summary>
        /// Gets or sets the column delimiter.
        /// </summary>
        /// <docgen category='Layout Options' order='10' />
        public CsvColumnDelimiterMode Delimiter { get; set; } = CsvColumnDelimiterMode.Auto;

        /// <summary>
        /// Gets or sets the quoting mode.
        /// </summary>
        /// <docgen category='Layout Options' order='10' />
        public CsvQuotingMode Quoting { get; set; } = CsvQuotingMode.Auto;

        /// <summary>
        /// Gets or sets the quote Character.
        /// </summary>
        /// <docgen category='Layout Options' order='10' />
        public string QuoteChar { get; set; } = "\"";

        /// <summary>
        /// Gets or sets the custom column delimiter value (valid when <see cref="Delimiter"/> is set to <see cref="CsvColumnDelimiterMode.Custom"/>).
        /// </summary>
        /// <docgen category='Layout Options' order='10' />
        public string CustomColumnDelimiter { get; set; }

        /// <inheritdoc/>
        protected override void InitializeLayout()
        {
            if (!WithHeader)
            {
                Header = null;
            }

            base.InitializeLayout();

            switch (Delimiter)
            {
                case CsvColumnDelimiterMode.Auto:
                    _actualColumnDelimiter = CultureInfo.CurrentCulture.TextInfo.ListSeparator;
                    break;

                case CsvColumnDelimiterMode.Comma:
                    _actualColumnDelimiter = ",";
                    break;

                case CsvColumnDelimiterMode.Semicolon:
                    _actualColumnDelimiter = ";";
                    break;

                case CsvColumnDelimiterMode.Pipe:
                    _actualColumnDelimiter = "|";
                    break;

                case CsvColumnDelimiterMode.Tab:
                    _actualColumnDelimiter = "\t";
                    break;

                case CsvColumnDelimiterMode.Space:
                    _actualColumnDelimiter = " ";
                    break;

                case CsvColumnDelimiterMode.Custom:
                    _actualColumnDelimiter = CustomColumnDelimiter;
                    break;
            }

            _quotableCharacters = (QuoteChar + "\r\n" + _actualColumnDelimiter).ToCharArray();
            _doubleQuoteChar = QuoteChar + QuoteChar;
            _precalculateLayouts = ResolveLayoutPrecalculation(Columns.Select(cln => cln.Layout));
        }

        /// <inheritdoc/>
        protected override void CloseLayout()
        {
            _precalculateLayouts = null;
            base.CloseLayout();
        }

        internal override void PrecalculateBuilder(LogEventInfo logEvent, StringBuilder target)
        {
            PrecalculateBuilderInternal(logEvent, target, _precalculateLayouts);
        }

        /// <inheritdoc/>
        protected override string GetFormattedMessage(LogEventInfo logEvent)
        {
            return RenderAllocateBuilder(logEvent);
        }

        /// <inheritdoc/>
        protected override void RenderFormattedMessage(LogEventInfo logEvent, StringBuilder target)
        {
            //Memory profiling pointed out that using a foreach-loop was allocating
            //an Enumerator. Switching to a for-loop avoids the memory allocation.
            for (int i = 0; i < Columns.Count; i++)
            {
                Layout columnLayout = Columns[i].Layout;
                RenderColumnLayout(logEvent, columnLayout, Columns[i]._quoting ?? Quoting, target, i);
            }
        }

        private void RenderColumnLayout(LogEventInfo logEvent, Layout columnLayout, CsvQuotingMode quoting, StringBuilder target, int i)
        {
            if (i != 0)
            {
                target.Append(_actualColumnDelimiter);
            }

            if (quoting == CsvQuotingMode.All)
            {
                target.Append(QuoteChar);
            }

            int orgLength = target.Length;
            columnLayout.Render(logEvent, target);
            if (orgLength != target.Length && ColumnValueRequiresQuotes(quoting, target, orgLength))
            {
                string columnValue = target.ToString(orgLength, target.Length - orgLength);
                target.Length = orgLength;
                if (quoting != CsvQuotingMode.All)
                {
                    target.Append(QuoteChar);
                }
                target.Append(columnValue.Replace(QuoteChar, _doubleQuoteChar));
                target.Append(QuoteChar);
            }
            else
            {
                if (quoting == CsvQuotingMode.All)
                {
                    target.Append(QuoteChar);
                }
            }
        }

        /// <summary>
        /// Get the headers with the column names.
        /// </summary>
        /// <returns></returns>
        private void RenderHeader(StringBuilder sb)
        {
            LogEventInfo logEvent = LogEventInfo.CreateNullEvent();

            //Memory profiling pointed out that using a foreach-loop was allocating
            //an Enumerator. Switching to a for-loop avoids the memory allocation.
            for (int i = 0; i < Columns.Count; i++)
            {
                CsvColumn col = Columns[i];
                var columnLayout = new SimpleLayout(new LayoutRenderers.LayoutRenderer[] { new LayoutRenderers.LiteralLayoutRenderer(col.Name) }, col.Name, ConfigurationItemFactory.Default);
                columnLayout.Initialize(LoggingConfiguration);
                RenderColumnLayout(logEvent, columnLayout, col._quoting ?? Quoting, sb, i);
            }
        }

        private bool ColumnValueRequiresQuotes(CsvQuotingMode quoting, StringBuilder sb, int startPosition)
        {
            switch (quoting)
            {
                case CsvQuotingMode.Nothing:
                    return false;

                case CsvQuotingMode.All:
                    if (QuoteChar.Length == 1)
                        return sb.IndexOf(QuoteChar[0], startPosition) >= 0;
                    else
                        return sb.IndexOfAny(_quotableCharacters, startPosition) >= 0;

                case CsvQuotingMode.Auto:
                default:
                    return sb.IndexOfAny(_quotableCharacters, startPosition) >= 0;
            }
        }

        /// <summary>
        /// Header with column names for CSV layout.
        /// </summary>
        [ThreadAgnostic]
        [AppDomainFixedOutput]
        internal sealed class CsvHeaderLayout : Layout
        {
            private readonly CsvLayout _parent;
            private string _headerOutput;

            /// <summary>
            /// Initializes a new instance of the <see cref="CsvHeaderLayout"/> class.
            /// </summary>
            /// <param name="parent">The parent.</param>
            public CsvHeaderLayout(CsvLayout parent)
            {
                _parent = parent;
            }

            /// <inheritdoc/>
            protected override void InitializeLayout()
            {
                _headerOutput = null;
                base.InitializeLayout();
            }

            private string GetHeaderOutput()
            {
                return _headerOutput ?? (_headerOutput = BuilderHeaderOutput());
            }

            private string BuilderHeaderOutput()
            {
                StringBuilder sb = new StringBuilder();
                _parent.RenderHeader(sb);
                return sb.ToString();
            }

            internal override void PrecalculateBuilder(LogEventInfo logEvent, StringBuilder target)
            {
                // Precalculation and caching is not needed
            }

            /// <inheritdoc/>
            protected override string GetFormattedMessage(LogEventInfo logEvent)
            {
                return GetHeaderOutput();
            }

            /// <inheritdoc/>
            protected override void RenderFormattedMessage(LogEventInfo logEvent, StringBuilder target)
            {
                target.Append(GetHeaderOutput());
            }
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            return ToStringWithNestedItems(Columns, c => c.Name);
        }
    }
}
