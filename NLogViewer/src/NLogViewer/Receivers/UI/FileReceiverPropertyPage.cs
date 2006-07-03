using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;
using NLogViewer.UI;
using System.IO;

namespace NLogViewer.Receivers.UI
{
    public partial class FileReceiverPropertyPage : WizardPage
    {
        private FileReceiver _receiver;

        public FileReceiverPropertyPage()
        {
            InitializeComponent();
        }

        public FileReceiverPropertyPage(FileReceiver receiver)
        {
            _receiver = receiver;
            InitializeComponent();
        }

        private void FileReceiverPropertyPage_Load(object sender, EventArgs e)
        {

        }

        public override bool ValidatePage()
        {
            if (textBox1.Text == "")
            {
                MessageBox.Show("You must provide a file name.");
                return false;
            }
            if (!File.Exists(textBox1.Text))
            {
                MessageBox.Show("File '" + textBox1.Text + "' does not exist.");
                return false;
            }
            return true;
        }

        private void buttonBrowse_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog ofd = new OpenFileDialog())
            {
                ofd.Filter = "Log files (*.log;*.txt;*.csv)|*.log;*.txt;*.csv|All files (*.*)|*.*";
                if (DialogResult.OK == ofd.ShowDialog())
                {
                    textBox1.Text = ofd.FileName;
                }
            }
        }
    }
}
