using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.IO;

namespace NLogViewer.UI
{
    public partial class IntroDialog : UserControl
    {
        public IntroDialog()
        {
            InitializeComponent();
        }

        public void ReloadRecentFiles()
        {
            listViewRecentFiles.Items.Clear();
            foreach (string s in AppPreferences.GetRecentFileList())
            {
                try
                {
                    ListViewItem lvi = new ListViewItem();
                    lvi.Tag = s;
                    lvi.Text = Path.GetFileNameWithoutExtension(s);
                    lvi.SubItems.Add(s);
                    lvi.SubItems.Add(Convert.ToString(File.GetLastWriteTime(s)));
                    listViewRecentFiles.Items.Add(lvi);
                }
                catch
                {
                    // ignore 
                }
            }
            listViewRecentFiles.AutoResizeColumns(ColumnHeaderAutoResizeStyle.HeaderSize);
            buttonOpen.Enabled = false;
        }

        private void IntroDialog_Load(object sender, EventArgs e)
        {
            ReloadRecentFiles();
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {

        }

        private void listView1_SelectedIndexChanged(object sender, EventArgs e)
        {
            buttonOpen.Enabled = listViewRecentFiles.SelectedItems.Count > 0;
        }
    }
}