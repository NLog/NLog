using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Data.Common;
using System.Data.SqlClient;

namespace NLogViewer.Receivers.UI
{
    public partial class ConnectionStringBuilderDialog : Form
    {
        public ConnectionStringBuilderDialog()
        {
            InitializeComponent();
        }

        private void ConnectionStringBuilderDialog_Load(object sender, EventArgs e)
        {
            
        }

        public DbConnectionStringBuilder Builder
        {
            get { return propertyGrid1.SelectedObject as DbConnectionStringBuilder; }
            set { propertyGrid1.SelectedObject = value; }
        }
    }
}