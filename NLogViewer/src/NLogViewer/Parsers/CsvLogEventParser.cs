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
    [LogEventParser("CSV", "Comma Separated", "NOT IMPLEMENTED - Comma Separated Values")]
    public class CsvLogEventParser : ILogEventParser, ILogEventParserWithEncoding
    {
        public ILogEventParserInstance Begin(Stream stream)
        {
            throw new NotImplementedException();
        }

        private Encoding _encoding = Encoding.UTF8;

        Encoding ILogEventParserWithEncoding.Encoding
        {
            get { return _encoding; }
            set { _encoding = value; }
        }
    }
}
