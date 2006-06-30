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
        private ILogEventReceiver _receiver = null;
        private ILogEventParser _parser = null;

        public NewReceiverDialog()
        {
            InitializeComponent();
        }

        private void NewReceiverDialog_Load(object sender, EventArgs e)
        {
            Pages.Add(new SelectLogReceiverPropertyPage());
            Pages.Add(new LogReceiverPropertyPage());
            Pages.Add(new SelectLogParserPropertyPage());
            Pages.Add(new LogParserPropertyPage());
            Pages.Add(new SummaryPropertyPage());
            InitializeWizard();
        }

        protected override void ActivatePage(int pageNumber)
        {
            switch (pageNumber)
            {
                case 1:
                    // log receiver property page

                    ILogEventReceiver receiver = LogReceiverFactory.CreateLogReceiver(
                        FindPage<SelectLogReceiverPropertyPage>().SelectedLogReceiver.Name, null);

                    if (receiver is IWizardConfigurable)
                    {
                        ReplacePage(1, ((IWizardConfigurable)receiver).GetWizardPage());
                    }
                    else
                    {
                        ReplacePage(1, new LogReceiverPropertyPage());
                    }

                    IWizardPropertyPage<ILogEventReceiver> pp = Pages[1] as IWizardPropertyPage<ILogEventReceiver>;
                    if (pp != null)
                        pp.TargetObject = receiver;

                    _receiver = receiver;
                    break;

                case 2:
                    // select log parser
                    break;

                case 3:
                    // log parser property page

                    ILogEventParser parser = LogEventParserFactory.CreateLogParser(
                        FindPage<SelectLogParserPropertyPage>().SelectedLogParser.Name, null);

                    if (parser is IWizardConfigurable)
                    {
                        Pages[3] = ((IWizardConfigurable)parser).GetWizardPage();
                    }
                    else
                    {
                        Pages[3] = new LogParserPropertyPage();
                    }

                    FindPage<LogParserPropertyPage>().TargetObject = parser;
                    break;

                case 4:
                    SummaryPropertyPage summaryPage = FindPage<SummaryPropertyPage>();
                    DisplaySummary(summaryPage);
                    break;
                //base.ActivatePage(pageNumber);
            }
        }

        public override int ForwardOffset(int currentPage)
        {
            if (currentPage == 1)
            {
                if (!(_receiver is ILogEventReceiverWithParser))
                    return 4;
            }
            return base.ForwardOffset(currentPage);
        }

        public override int PreviousOffset(int currentPage)
        {
            if (currentPage == 4)
            {
                if (!(_receiver is ILogEventReceiverWithParser))
                    return 1;
            }

            return base.PreviousOffset(currentPage);
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