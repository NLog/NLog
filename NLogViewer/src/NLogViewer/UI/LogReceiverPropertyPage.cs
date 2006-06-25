using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;
using NLogViewer.Receivers;

namespace NLogViewer.UI
{
    public partial class LogReceiverPropertyPage : WizardPage
    {
        public LogReceiverPropertyPage()
        {
            InitializeComponent();
        }

        public ILogEventReceiver Receiver
        {
            get { return propertyGrid1.SelectedObject as ILogEventReceiver; }
            set { propertyGrid1.SelectedObject = value; }
        }
    }
}
