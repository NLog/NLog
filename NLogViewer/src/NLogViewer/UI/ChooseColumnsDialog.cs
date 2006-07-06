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

        public ChooseColumnsDialog()
        {
            InitializeComponent();
        }

        public Session Session
        {
            get { return _session; }
            set { _session = value; }
        }

        private List<CheckBox> _checkboxes = new List<CheckBox>();
        private List<TextBox> _widths = new List<TextBox>();
        private List<ComboBox> _combos = new List<ComboBox>();

        private void ChooseColumnsDialog_Load(object sender, EventArgs e)
        {
            foreach (LogColumn lc in Session.Columns)
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

                ComboBox combo = new ComboBox();
                combo.DropDownStyle = ComboBoxStyle.DropDownList;
                combo.Items.Add(LogColumnGrouping.None);
                combo.Items.Add(LogColumnGrouping.Flat);
                combo.Items.Add(LogColumnGrouping.Hierarchy);
                combo.Items.Add(LogColumnGrouping.FileSystem);
                combo.SelectedItem = lc.Grouping;
                combo.Dock = DockStyle.Fill;
                _combos.Add(combo);
                tableLayoutPanel1.Controls.Add(combo);
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

            Session.Columns = lcc;

            DialogResult = DialogResult.OK;
            Close();
        }
    }
}