using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

using NLogViewer.Parsers;
using NLogViewer.Events;
using System.Xml;
using NLogViewer.Configuration;
using System.Collections.Specialized;
using System.Text.RegularExpressions;
using NLogViewer.Parsers.UI;

namespace NLogViewer.Parsers
{
    [LogEventParser("REGEX", "Regular Expression", "Regular Expression")]
    public class RegexLogEventParser : ILogEventParser, IWizardConfigurable, ILogEventParserWithEncoding
    {
        private string _expression;
        private Regex _compiledRegex;

        private Encoding _encoding = Encoding.UTF8;

        public Encoding Encoding
        {
            get { return _encoding; }
            set { _encoding = value; }
        }

        public string Expression
        {
            get { return _expression; }
            set
            {
                _expression = value;
                _compiledRegex = new Regex(_expression);
            }
        }

        class Instance : ILogEventParserInstance
        {
            public LogEvent ReadNext()
            {
                throw new Exception("The method or operation is not implemented.");
            }

            #region IDisposable Members

            public void Dispose()
            {
                throw new Exception("The method or operation is not implemented.");
            }

            #endregion
        }

        public ILogEventParserInstance Begin(Stream stream)
        {
            throw new NotImplementedException();
        }

        public IWizardPage GetWizardPage()
        {
            return new RegexLogEventParserPropertyPage(this);
        }
    }
}
