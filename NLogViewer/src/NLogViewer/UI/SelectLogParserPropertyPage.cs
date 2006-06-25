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
                lvi.SubItems.Add(new ListViewItem.ListViewSubItem(lvi, lepi.Description));
                listView1.Items.Add(lvi);
            }
        }

    }
}
