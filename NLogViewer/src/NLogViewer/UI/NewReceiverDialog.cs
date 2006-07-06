using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using NLogViewer.Receivers;
using NLogViewer.Parsers;
using NLogViewer.Configuration;

namespace NLogViewer.UI
{
    public partial class NewReceiverDialog : NLogViewer.UI.WizardForm
    {
        const int PAGE_SELECT_RECEIVER = 0;
        const int PAGE_RECEIVER_PROPERTIES = 1;
        const int PAGE_SELECT_PARSER = 2;
        const int PAGE_PARSER_PROPERTIES = 3;
        const int PAGE_SELECT_ENCODING = 4;
        const int PAGE_SUMMARY = 5;

        private ILogEventReceiver _receiver = null;
        private ILogEventParser _parser = null;

        public NewReceiverDialog()
        {
            InitializeComponent();
        }

        private void NewReceiverDialog_Load(object sender, EventArgs e)
        {
            SelectLogReceiverPropertyPage page0 = new SelectLogReceiverPropertyPage();
            page0.ReceiverChanged += new EventHandler(page0_ReceiverChanged);
            Pages.Add(page0);
            Pages.Add(new LogReceiverPropertyPage());
            SelectLogParserPropertyPage page2 = new SelectLogParserPropertyPage();
            page2.ParserChanged += new EventHandler(page2_ParserChanged);
            Pages.Add(page2);
            Pages.Add(new LogParserPropertyPage());
            Pages.Add(new SelectEncodingPropertyPage());
            Pages.Add(new SummaryPropertyPage());
            InitializeWizard();
        }

        void page2_ParserChanged(object sender, EventArgs e)
        {
            if (((SelectLogParserPropertyPage)sender).SelectedLogParser != null)
            {
                _parser = LogEventParserFactory.CreateLogParser(
                    ((SelectLogParserPropertyPage)sender).SelectedLogParser.Name);

                if (_parser is IWizardConfigurable)
                    ReplacePage(PAGE_PARSER_PROPERTIES, ((IWizardConfigurable)_parser).GetWizardPage());
                else
                    ReplacePage(PAGE_PARSER_PROPERTIES, new LogParserPropertyPage());

                IWizardPropertyPage<ILogEventParser> pp = Pages[PAGE_PARSER_PROPERTIES] as IWizardPropertyPage<ILogEventParser>;
                if (pp != null)
                    pp.TargetObject = _parser;
            }
            else
            {
                _parser = null;
            }
            UnActivatePage(PAGE_PARSER_PROPERTIES);
        }

        void page0_ReceiverChanged(object sender, EventArgs e)
        {
            if (((SelectLogReceiverPropertyPage)sender).SelectedLogReceiver != null)
            {
                _receiver = LogReceiverFactory.CreateLogReceiver(
                    ((SelectLogReceiverPropertyPage)sender).SelectedLogReceiver.Name);

                if (_receiver is IWizardConfigurable)
                    ReplacePage(PAGE_RECEIVER_PROPERTIES, ((IWizardConfigurable)_receiver).GetWizardPage());
                else
                    ReplacePage(PAGE_RECEIVER_PROPERTIES, new LogReceiverPropertyPage());

                IWizardPropertyPage<ILogEventReceiver> pp = Pages[PAGE_RECEIVER_PROPERTIES] as IWizardPropertyPage<ILogEventReceiver>;
                if (pp != null)
                    pp.TargetObject = _receiver;
            }
            else
            {
                _receiver = null;
            }
            UnActivatePage(PAGE_RECEIVER_PROPERTIES);
        }

        protected override void ActivatePage(int pageNumber)
        {
            switch (pageNumber)
            {
                case PAGE_SELECT_RECEIVER:
                    break;

                case PAGE_RECEIVER_PROPERTIES:
                    break;

                case PAGE_SELECT_PARSER:
                    break;

                case PAGE_PARSER_PROPERTIES:
                    ILogEventParser parser = LogEventParserFactory.CreateLogParser(
                        FindPage<SelectLogParserPropertyPage>().SelectedLogParser.Name);

                    if (parser is IWizardConfigurable)
                    {
                        ReplacePage(3, ((IWizardConfigurable)parser).GetWizardPage());
                    }
                    else
                    {
                        ReplacePage(3, new LogParserPropertyPage());
                    }

                    IWizardPropertyPage<ILogEventParser> ppp = Pages[3] as IWizardPropertyPage<ILogEventParser>;
                    if (ppp != null)
                        ppp.TargetObject = parser;
                    break;

                case PAGE_SELECT_ENCODING:
                    break;

                case PAGE_SUMMARY:
                    SummaryPropertyPage summaryPage = FindPage<SummaryPropertyPage>();
                    DisplaySummary(summaryPage);
                    break;
                //base.ActivatePage(pageNumber);
            }
        }

        protected override bool ShouldSkipPage(int page)
        {
            if (!(_receiver is ILogEventReceiverWithParser))
            {
                if (page == PAGE_SELECT_PARSER)
                    return true;
                if (page == PAGE_PARSER_PROPERTIES)
                    return true;
            }
            if (!(_parser is ILogEventParserWithEncoding))
            {
                if (page == PAGE_SELECT_ENCODING)
                    return true;
            }
            return false;
        }

        public ILogEventReceiver Receiver
        {
            get { return _receiver; }
        }

        public ILogEventParser Parser
        {
            get { return _parser; }
        }

        private void DisplaySummary(SummaryPropertyPage summaryPage)
        {
        }
    }
}