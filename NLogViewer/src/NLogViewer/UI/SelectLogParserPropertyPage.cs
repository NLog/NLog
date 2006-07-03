using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;
using NLogViewer.Parsers;

namespace NLogViewer.UI
{
    public partial class SelectLogParserPropertyPage : WizardPage
    {
        private LogEventParserInfo _selectedLogParser;

        public event EventHandler ParserChanged;

        public SelectLogParserPropertyPage()
        {
            InitializeComponent();
        }

        private void SelectLogParserPropertyPage_Load(object sender, EventArgs e)
        {
            foreach (LogEventParserInfo lepi in LogEventParserFactory.Parsers)
            {
                ListViewItem lvi = new ListViewItem();
                lvi.Text = lepi.Summary;
                lvi.Tag = lepi;
                lvi.SubItems.Add(new ListViewItem.ListViewSubItem(lvi, lepi.Description));
                listView1.Items.Add(lvi);
            }
        }

        public LogEventParserInfo SelectedLogParser
        {
            get { return _selectedLogParser; }
            set { _selectedLogParser = value; }
        }

        private void listView1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (listView1.SelectedItems.Count == 1)
            {
                _selectedLogParser = listView1.SelectedItems[0].Tag as LogEventParserInfo;
            }
            else
            {
                _selectedLogParser = null;
            }
            if (ParserChanged != null)
                ParserChanged(this, new EventArgs());
        }

        public override bool ValidatePage()
        {
            if (_selectedLogParser != null)
                return true;

            MessageBox.Show(this, "Please select one of the parsers.");
            return false;
        }
    }
}
