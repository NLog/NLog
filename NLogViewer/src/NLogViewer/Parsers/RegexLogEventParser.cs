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
    [LogEventParser("REGEX", "Regular Expression", "NOT IMPLEMENTED - Regular Expression")]
    public class RegexLogEventParser : ILogEventParser
    {
        public ILogEventParserInstance Begin(Stream stream)
        {
            throw new NotImplementedException();
        }

        public void Configure(NameValueCollection parameters)
        {
        }
    }
}
