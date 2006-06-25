using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using NLogViewer.Receivers;

namespace NLogViewer.UI
{
    public partial class NewReceiverDialog : NLogViewer.UI.WizardForm
    {
        public NewReceiverDialog()
        {
            InitializeComponent();
        }

        private void NewReceiverDialog_Load(object sender, EventArgs e)
        {
            Pages.Add(new SelectLogReceiverPropertyPage());
            Pages.Add(new LogReceiverPropertyPage());
            InitializeWizard();
        }

        protected override void ActivatePage(int pageNumber)
        {
            if (pageNumber == 1)
            {
                ILogEventReceiver receiver = LogReceiverFactory.CreateLogReceiver(
                    FindPage<SelectLogReceiverPropertyPage>().SelectedLogReceiver.Name, null);

                FindPage<LogReceiverPropertyPage>().Receiver = receiver;
            }
            //base.ActivatePage(pageNumber);
        }
    }
}

