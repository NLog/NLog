using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

using NLogViewer.Parsers;
using NLogViewer.Events;
using System.Xml;
using NLogViewer.Configuration;
using System.Collections.Specialized;

namespace NLogViewer.Parsers
{
    [LogEventParser("FIXED", "Fixed Width", "NOT IMPLEMENTED - Fixed Width Columns")]
    public class FixedWidthLogEventParser : ILogEventParser, ILogEventParserWithEncoding
    {
        public ILogEventParserInstance Begin(Stream stream)
        {
            throw new NotImplementedException();
        }

        private Encoding _encoding = Encoding.UTF8;

        public Encoding Encoding
        {
            get { return _encoding; }
            set { _encoding = value; }
        }
    }
}
