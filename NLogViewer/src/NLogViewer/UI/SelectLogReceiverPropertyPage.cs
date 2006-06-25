using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;
using NLogViewer.Receivers;

namespace NLogViewer.UI
{
    public partial class SelectLogReceiverPropertyPage : WizardPage
    {
        private LogEventReceiverInfo _selectedLogReceiver;

        public SelectLogReceiverPropertyPage()
        {
            _selectedLogReceiver = null;
            InitializeComponent();
        }

        private void SelectLogReceiverPropertyPage_Load(object sender, EventArgs e)
        {
            foreach (LogEventReceiverInfo leri in LogReceiverFactory.Receivers)
            {
                ListViewItem lvi = new ListViewItem();
                lvi.Text = leri.Summary;
                lvi.SubItems.Add(new ListViewItem.ListViewSubItem(lvi, leri.Description));
                lvi.Tag = leri;
                listView1.Items.Add(lvi);
            }
        }

        public LogEventReceiverInfo SelectedLogReceiver
        {
            get { return _selectedLogReceiver; }
            set { _selectedLogReceiver = value; }
        }

        private void listView1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (listView1.SelectedItems.Count == 1)
            {
                _selectedLogReceiver = listView1.SelectedItems[0].Tag as LogEventReceiverInfo;
            }
            else
            {
                _selectedLogReceiver = null;
            }
        }

        public override bool ValidatePage()
        {
            if (_selectedLogReceiver != null)
                return true;

            MessageBox.Show(this, "Please select one of the receivers.");
            return false;
        }
    }
}
