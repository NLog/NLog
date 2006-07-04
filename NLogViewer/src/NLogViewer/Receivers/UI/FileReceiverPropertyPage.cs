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
            textBox1.Text = _receiver.FileName;
            foreach (string s in AppPreferences.RecentFiles.GetList())
            {
                listView1.Items.Add(s);
            }
            listView1.AutoResizeColumns(ColumnHeaderAutoResizeStyle.HeaderSize);
            checkBoxMonitorFileForChanges.Checked = _receiver.MonitorChanges;
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
            _receiver.FileName = textBox1.Text;
            _receiver.MonitorChanges = checkBoxMonitorFileForChanges.Checked;
            AppPreferences.RecentFiles.AddToList(textBox1.Text);
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

        private void listView1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (listView1.SelectedItems.Count > 0)
            {
                textBox1.Text = listView1.SelectedItems[0].Text;
            }
        }
    }
}
