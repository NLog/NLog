using System;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace SilverlightConsoleRunner
{
    public partial class RunnerForm : Form
    {
        public RunnerForm()
        {
            InitializeComponent();
        }

        public string Url { get; set; }

        private void RunnerForm_Load(object sender, EventArgs e)
        {
            webBrowser1.Navigate(this.Url);
        }
    }
}
