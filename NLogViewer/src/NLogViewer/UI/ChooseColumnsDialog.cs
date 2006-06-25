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
        private SessionConfiguration _configuration;

        public ChooseColumnsDialog()
        {
            InitializeComponent();
        }

        public SessionConfiguration Configuration
        {
            get { return _configuration; }
            set { _configuration = value; }
        }

        private List<CheckBox> _checkboxes = new List<CheckBox>();
        private List<TextBox> _widths = new List<TextBox>();

        private void ChooseColumnsDialog_Load(object sender, EventArgs e)
        {
            foreach (LogColumn lc in Configuration.Columns)
            {
                CheckBox cb = new CheckBox();
                cb.Text = lc.Name;
                cb.Checked = lc.Visible;
                cb.AutoSize = true;
                cb.Tag = lc;

                _checkboxes.Add(cb);
                tableLayoutPanel1.Controls.Add(cb);

                TextBox tb = new TextBox();
                tb.Text = lc.Width.ToString();
                tb.Dock = DockStyle.Fill;
                _widths.Add(tb);
                tableLayoutPanel1.Controls.Add(tb);
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            int checkedCount = 0;

            foreach (CheckBox cb in _checkboxes)
            {
                if (cb.Checked)
                    checkedCount++;
            }
            foreach (TextBox tb in _widths)
            {
                try
                {
                    int w = Convert.ToInt32(tb.Text);
                    if (w < 10)
                        throw new Exception("");
                }
                catch
                {
                    Color oldColor = tb.BackColor;

                    tb.BackColor = Color.Red;
                    MessageBox.Show(this, "Invalid width");
                    tb.BackColor = oldColor;
                    return;
                }
            }

            if (checkedCount == 0)
            {
                MessageBox.Show(this, "You must select at least one column");
                return;
            }

            LogColumnCollection lcc = new LogColumnCollection();

            for (int i =0; i < _checkboxes.Count; ++i)
            {
                if (_checkboxes[i].Checked)
                {
                    LogColumn lc = _checkboxes[i].Tag as LogColumn;
                    lc.Width = Convert.ToInt32(_widths[i].Text);
                    lc.Visible = true;
                    lcc.Add(lc);
                }
            }

            for (int i = 0; i < _checkboxes.Count; ++i)
            {
                if (!_checkboxes[i].Checked)
                {
                    LogColumn lc = _checkboxes[i].Tag as LogColumn;
                    lc.Width = Convert.ToInt32(_widths[i].Text);
                    lc.Visible = false;
                    lcc.Add(lc);
                }
            }

            Configuration.Columns = lcc;

            DialogResult = DialogResult.OK;
            Close();
        }
    }
}