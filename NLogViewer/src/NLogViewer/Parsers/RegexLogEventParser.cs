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
        private string _expression = @"^(?<IPAddress>\d+.\d+.\d+.\d+)";
        private Regex _compiledRegex;

        private Encoding _encoding = Encoding.UTF8;

        Encoding ILogEventParserWithEncoding.Encoding
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
            private StreamReader _reader;
            private RegexLogEventParser _parser;

            public Instance(Stream stream, RegexLogEventParser parser)
            {
                _parser = parser;
                _reader = new StreamReader(stream, parser._encoding);
            }

            public bool ReadNext(LogEvent le)
            {
                string line = _reader.ReadLine();
                if (line == null)
                    return false;

                Match match = _parser._compiledRegex.Match(line);
                if (!match.Success)
                    return false;

                string[] names = _parser._compiledRegex.GetGroupNames();
                for (int i = 1; i < names.Length; ++i)
                {
                    string v = match.Groups[i].Value;
                    le[names[i]] = v;
                }

                return true;
            }

            public void Dispose()
            {
            }
        }

        public ILogEventParserInstance Begin(Stream stream)
        {
            return new Instance(stream, this);
        }

        public IWizardPage GetWizardPage()
        {
            return new RegexLogEventParserPropertyPage(this);
        }
    }
}
