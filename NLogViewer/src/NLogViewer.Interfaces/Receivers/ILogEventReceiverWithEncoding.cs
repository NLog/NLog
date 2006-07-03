using System;
using System.Collections.Specialized;
using System.Collections.Generic;
using System.IO;

using NLogViewer.Events;
using System.Text;

namespace NLogViewer.Parsers
{
    public interface ILogEventParserWithEncoding
    {
        Encoding Encoding { get; set; }
    }
}
