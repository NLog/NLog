using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace NLogViewer.UI
{
    public partial class IntroDialog : Form
    {
        public IntroDialog()
        {
            InitializeComponent();
        }

        private void IntroDialog_Load(object sender, EventArgs e)
        {
            checkBox1.Checked = AppPreferences.ShowWelcomeScreenOnStartup;
        }

        private void button3_Click(object sender, EventArgs e)
        {

        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {

        }

        private void listView1_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Yes;
            Close();
        }

        private void buttonCancel_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.No;
            Close();
        }

        private void IntroDialog_FormClosing(object sender, FormClosingEventArgs e)
        {
            AppPreferences.ShowWelcomeScreenOnStartup = checkBox1.Checked;

        }
    }
}