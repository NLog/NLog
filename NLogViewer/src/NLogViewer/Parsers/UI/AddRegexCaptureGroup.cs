using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace NLogViewer.Parsers.UI
{
    public partial class AddRegexCaptureGroup : Form
    {
        public AddRegexCaptureGroup()
        {
            InitializeComponent();
        }

        public string CaptureGroupName
        {
            get { return textBoxGroupName.Text; }
            set { textBoxGroupName.Text = value; }
        }

        public string RegularExpression
        {
            get { return textBoxExpression.Text; }
            set { textBoxExpression.Text = value; }
        }

        private void buttonOK_Click(object sender, EventArgs e)
        {
            if (textBoxGroupName.Text == "")
            {
                MessageBox.Show(this, "You must provide Capture Group Name");
                return;
            }
            if (textBoxExpression.Text == "")
            {
                MessageBox.Show(this, "You must provide Regular Expression");
                return;
            }
            DialogResult = DialogResult.OK;
            Close();
        }
    }
}