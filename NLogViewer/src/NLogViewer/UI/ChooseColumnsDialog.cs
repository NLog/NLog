using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using NLogViewer.Configuration;

namespace NLogViewer.UI
{
    public partial class ChooseColumnsDialog : Form
    {
        private Session _session;
        private LogColumnCollection _lcc;

        public ChooseColumnsDialog()
        {
            InitializeComponent();
            this.groupingDataGridViewCheckBoxColumn.Items.AddRange(new object[] {
                LogColumnGrouping.Flat,
                LogColumnGrouping.FileSystem,
                LogColumnGrouping.Hierarchy,
                LogColumnGrouping.None});
        }

        public Session Session
        {
            get { return _session; }
            set { _session = value; }
        }

        private void ChooseColumnsDialog_Load(object sender, EventArgs e)
        {
            LogColumnCollection lcc = new LogColumnCollection();
            foreach (LogColumn lc in Session.Columns)
            {
                lcc.Add(lc.Clone());
            }

            _lcc = lcc;

            dataGridView1.DataSource = lcc;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Session.Columns = _lcc;
            Session.Dirty = true;
            DialogResult = DialogResult.OK;
            Close();
        }

        private void dataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {

        }

        private void buttonShowAll_Click(object sender, EventArgs e)
        {
            foreach (LogColumn lc in _lcc)
            {
                lc.Visible = true;
            }
            dataGridView1.Refresh();
        }

        private void buttonHideAll_Click(object sender, EventArgs e)
        {
            foreach (LogColumn lc in _lcc)
            {
                lc.Visible = false;
            }
            dataGridView1.Refresh();
        }
    }
}