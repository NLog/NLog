using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using NLogViewer.Receivers;
using NLogViewer.Configuration;

namespace NLogViewer.UI
{
    public partial class ReceiverPropertiesDialog : Form
    {
        private ILogEventReceiver _receiver;

        public ReceiverPropertiesDialog()
        {
            InitializeComponent();
        }

        public ReceiverPropertiesDialog(ILogEventReceiver receiver)
        {
            InitializeComponent();
            _receiver = receiver;
        }

        private void ReceiverPropertiesDialog_Load(object sender, EventArgs e)
        {
            Control c1;

            if (_receiver is IWizardConfigurable)
            {
                c1 = ((IWizardConfigurable)_receiver).GetWizardPage().Control;
            }
            else
            {
                c1 = new LogReceiverPropertyPage();
            }

            IWizardPropertyPage<ILogEventReceiver> prop = c1 as IWizardPropertyPage<ILogEventReceiver>;
            if (prop != null)
                prop.TargetObject = _receiver;

            tabPage1.Controls.Add(c1);
            c1.Dock = DockStyle.Fill;
        }
    }
}