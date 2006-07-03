using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;
using NLogViewer.Receivers;
using NLogViewer.Parsers;
using NLogViewer.Configuration;

namespace NLogViewer.UI
{
    public partial class LogParserPropertyPage : WizardPage, IWizardPropertyPage<ILogEventParser>
    {
        public LogParserPropertyPage()
        {
            InitializeComponent();
        }

        public ILogEventParser TargetObject
        {
            get { return propertyGrid1.SelectedObject as ILogEventParser; }
            set { propertyGrid1.SelectedObject = value; }
        }

        private void propertyGrid1_Click(object sender, EventArgs e)
        {

        }
    }
}
