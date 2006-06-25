using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;

namespace NLogViewer.UI
{
    public partial class WizardPage : UserControl
    {
        private string _title;

        public WizardPage()
        {
            InitializeComponent();
        }

        public string Title
        {
            get { return _title; }
            set { _title = value; }
        }

        public virtual void ActivatePage()
        {
        }

        public virtual bool ValidatePage()
        {
            return true;
        }
    }
}
