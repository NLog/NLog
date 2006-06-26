using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace NLogViewer.UI
{
    public partial class OptionsDialog : Form
    {
        public OptionsDialog()
        {
            InitializeComponent();
        }

        private void buttonOK_Click(object sender, EventArgs e)
        {
            AppPreferences.ShowWelcomeScreenOnStartup = checkBoxShowWelcomeScreenOnStartup.Checked;
            DialogResult = DialogResult.OK;
            Close();
        }

        private void OptionsDialog_Load(object sender, EventArgs e)
        {
            checkBoxShowWelcomeScreenOnStartup.Checked = AppPreferences.ShowWelcomeScreenOnStartup;
        }
    }
}