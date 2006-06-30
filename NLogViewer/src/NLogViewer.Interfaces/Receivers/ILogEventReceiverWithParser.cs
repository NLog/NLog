using System;
using System.Collections.Specialized;
using System.Collections.Generic;
using System.IO;

using NLogViewer.Events;

namespace NLogViewer.Parsers
{
    public interface ILogEventReceiverWithParser
    {
        ILogEventParser Parser { get; set; }
    }
}
