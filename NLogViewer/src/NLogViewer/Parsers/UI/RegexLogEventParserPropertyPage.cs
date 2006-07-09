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
using System.Xml;

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
            try
            {
                _parser.Expression = textBox1.Text;
                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, "ERROR: " + ex);
                return false;
            }
        }

        private void contextMenuStrip1_Opening(object sender, CancelEventArgs e)
        {

        }

        private void RegexLogEventParserPropertyPage_Load(object sender, EventArgs e)
        {
            try
            {
                XmlDocument doc = new XmlDocument();

                doc.Load(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Regexes.xml"));
                foreach (XmlElement el in doc.SelectNodes("//regex"))
                {
                    ListViewItem lvi = new ListViewItem();
                    lvi.Text = el.SelectSingleNode("name").InnerText;
                    lvi.SubItems.Add(el.SelectSingleNode("expression").InnerText);
                    lvi.Tag = el.SelectSingleNode("expression").InnerText;
                    listView1.Items.Add(lvi);
                }
                listView1.AutoResizeColumns(ColumnHeaderAutoResizeStyle.HeaderSize);
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, ex.Message);
                // ignore
            }
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

        private void listView1_Enter(object sender, EventArgs e)
        {

        }

        private void listView1_DoubleClick(object sender, EventArgs e)
        {
            foreach (ListViewItem lvi in listView1.SelectedItems)
            {
                textBox1.Text = (string)lvi.Tag;
            }
        }
    }
}
