using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;
using NLogViewer.UI;
using System.IO;
using NLogViewer.Parsers;

namespace NLogViewer.Parsers.UI
{
    public partial class RegexLogEventParserPropertyPage : WizardPage
    {
        private RegexLogEventParser _parser;

        public RegexLogEventParserPropertyPage()
        {
            InitializeComponent();
        }

        public RegexLogEventParserPropertyPage(RegexLogEventParser parser)
        {
            _parser = parser;
            InitializeComponent();
        }

        public override bool ValidatePage()
        {
            if (textBox1.Text == "")
            {
                MessageBox.Show("You must provide a regular expression.");
                return false;
            }
            return true;
        }

        private void contextMenuStrip1_Opening(object sender, CancelEventArgs e)
        {

        }

        private void RegexLogEventParserPropertyPage_Load(object sender, EventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e)
        {
            contextMenuStrip1.Show(button1, 0, button1.Height);
        }

        private void toolStripMenuItemAddCaptureGroup_Click(object sender, EventArgs e)
        {
            using (AddRegexCaptureGroup dlg = new AddRegexCaptureGroup())
            {
                if (dlg.ShowDialog(this) == DialogResult.OK)
                {
                    textBox1.AppendText("(<" + dlg.CaptureGroupName + ">:" + dlg.RegularExpression + ")");
                }
            }
        }
    }
}
