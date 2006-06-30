using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;
using NLogViewer.Configuration;

namespace NLogViewer.UI
{
    public partial class WizardPage : UserControl, IWizardPage
    {
        private string _title;
        private string _label1;
        private string _label2;

        public WizardPage()
        {
            InitializeComponent();
        }

        public string Title
        {
            get { return _title; }
            set { _title = value; }
        }

        public string Label1
        {
            get { return _label1; }
            set { _label1 = value; }
        }

        public string Label2
        {
            get { return _label2; }
            set { _label2 = value; }
        }

        public virtual void ActivatePage()
        {
        }

        public virtual bool ValidatePage()
        {
            return true;
        }

        Control IWizardPage.Control
        {
            get
            {
                return this;
            }
        }
    }
}
