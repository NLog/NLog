using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;

namespace NLogViewer.UI
{
    public partial class SelectEncodingPropertyPage : WizardPage
    {
        public SelectEncodingPropertyPage()
        {
            InitializeComponent();
        }

        private void SelectEncodingPropertyPage_Load(object sender, EventArgs e)
        {
            AddEncoding(Encoding.ASCII, 0);
            AddEncoding(Encoding.GetEncoding(1250), 0);
            AddEncoding(Encoding.GetEncoding(1251), 0);
            AddEncoding(Encoding.GetEncoding(1252), 0);
            AddEncoding(Encoding.GetEncoding(1253), 0);
            AddEncoding(Encoding.GetEncoding(1254), 0);
            AddEncoding(Encoding.GetEncoding(1255), 0);
            AddEncoding(Encoding.GetEncoding(1256), 0);
            AddEncoding(Encoding.GetEncoding(1257), 0);
            AddEncoding(Encoding.GetEncoding(1258), 0);
            AddEncoding(Encoding.UTF8, 0);

            foreach (EncodingInfo encodingInfo in Encoding.GetEncodings())
            {
                AddEncoding(encodingInfo.GetEncoding(), 1);
            }
        }

        private ListViewItem AddEncoding(Encoding encoding, int group)
        {
            ListViewItem lvi = new ListViewItem();
            lvi.Text = encoding.WebName;
            lvi.SubItems.Add(new ListViewItem.ListViewSubItem(lvi, Convert.ToString(encoding.CodePage)));
            lvi.SubItems.Add(new ListViewItem.ListViewSubItem(lvi, encoding.EncodingName));
            lvi.Group = listView1.Groups[group];
            listView1.Items.Add(lvi);
            if (group == 0 && encoding.CodePage == Encoding.Default.CodePage)
                lvi.Selected = true;
            return lvi;
        }
    }
}
