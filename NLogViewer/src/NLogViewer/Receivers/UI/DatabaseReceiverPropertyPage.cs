using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Data.Common;
using System.Text;
using System.Windows.Forms;
using NLogViewer.Receivers;
using NLogViewer.Configuration;
using NLogViewer.UI;
using System.Data.SqlClient;
using System.Data.OleDb;
using System.Data.Odbc;

namespace NLogViewer.Receivers.UI
{
    public partial class DatabaseReceiverPropertyPage : WizardPage
    {
        private DatabaseReceiver _receiver;

        public DatabaseReceiverPropertyPage()
        {
            InitializeComponent();
        }

        public DatabaseReceiverPropertyPage(DatabaseReceiver receiver)
        {
            _receiver = receiver;
            InitializeComponent();
        }

        private void DatabaseReceiverPropertyPage_Load(object sender, EventArgs e)
        {
            comboBox1.Items.Add(new ConnectionType(typeof(SqlConnection), "SQL Server", new SqlConnectionStringBuilder()));
            comboBox1.Items.Add(new ConnectionType(typeof(OleDbConnection), "OLE DB", new OleDbConnectionStringBuilder()));
            comboBox1.Items.Add(new ConnectionType(typeof(OdbcConnection), "ODBC", new OdbcConnectionStringBuilder()));
            comboBox1.Items.Add(new ConnectionType(null, "Other", null));
            comboBox1.SelectedIndex = 0;

            comboBox2.Items.Add(IsolationLevel.ReadCommitted);
            comboBox2.Items.Add(IsolationLevel.ReadUncommitted);
            comboBox2.Items.Add(IsolationLevel.Snapshot);
            comboBox2.Items.Add(IsolationLevel.RepeatableRead);
            comboBox2.Items.Add(IsolationLevel.Serializable);
            comboBox2.Items.Add(IsolationLevel.Chaos);
            comboBox2.Items.Add(IsolationLevel.Unspecified);

            comboBox2.SelectedIndex = 0;
        }

        class ConnectionType
        {
            private string _type;
            private string _description;
            private DbConnectionStringBuilder _connectionStringBuilder;

            public ConnectionType(Type type, string description, DbConnectionStringBuilder connectionStringBuilder)
            {
                if (type != null)
                    _type = type.AssemblyQualifiedName;
                else
                    _type = null;
                _description = description;
                _connectionStringBuilder = connectionStringBuilder;
            }

            public string Type
            {
                get { return _type; }
            }

            public DbConnectionStringBuilder ConnectionStringBuilder
            {
                get
                {
                    return _connectionStringBuilder;
                }
            }

            public override string ToString()
            {
                return _description;
            }
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            ConnectionType ct = comboBox1.SelectedItem as ConnectionType;
            if (ct != null && ct.Type != null)
            {
                textBoxConnectionType.Text = ct.Type;
                textBoxConnectionType.ReadOnly = true;

                buttonBuildConnectionString.Enabled = (ct.ConnectionStringBuilder != null);
                    
            }
            else
            {
                textBoxConnectionType.Text = "";
                textBoxConnectionType.ReadOnly = false;
                buttonBuildConnectionString.Enabled = false;
            }
        }

        private void buttonBuildConnectionString_Click(object sender, EventArgs e)
        {
            ConnectionType ct = comboBox1.SelectedItem as ConnectionType;
            if (ct != null && ct.ConnectionStringBuilder != null)
            {
                using (ConnectionStringBuilderDialog dlg = new ConnectionStringBuilderDialog())
                {
                    dlg.Builder = ct.ConnectionStringBuilder;
                    dlg.Builder.ConnectionString = textBoxConnectionString.Text;
                    if (dlg.ShowDialog(this) == DialogResult.OK)
                    {
                        textBoxConnectionString.Text = dlg.Builder.ConnectionString;
                    }
                }
            }
        }

        private void buttonTest_Click(object sender, EventArgs e)
        {
            try
            {
                Type connectionType = Type.GetType(textBoxConnectionType.Text, true);
                using (IDbConnection conn = (IDbConnection)Activator.CreateInstance(connectionType))
                {
                    conn.ConnectionString = textBoxConnectionString.Text;
                    conn.Open();

                    using (IDbTransaction tran = conn.BeginTransaction())
                    {
                        using (IDbCommand cmd = conn.CreateCommand())
                        {
                            cmd.Transaction = tran;
                            cmd.CommandText = textBoxQueryString.Text;
                            using (IDataReader reader = cmd.ExecuteReader())
                            {
                                int fieldCount = reader.FieldCount;

                                MessageBox.Show(this, "Connection successful. Query returned " + fieldCount + " columns.");
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, ex.ToString());
            }
        }

        public override bool ValidatePage()
        {
            if (textBoxConnectionString.Text == "")
            {
                MessageBox.Show(this, "You must provide connection string.");
                return false;
            }
            if (textBoxConnectionType.Text == "")
            {
                MessageBox.Show(this, "You must provide connection type.");
                return false;
            }
            if (textBoxQueryString.Text == "")
            {
                MessageBox.Show(this, "You must provide query string.");
                return false;
            }
            _receiver.ConnectionType = textBoxConnectionType.Text;
            _receiver.ConnectionString = textBoxConnectionString.Text;
            _receiver.IsolationLevel = (IsolationLevel)comboBox2.SelectedItem;
            _receiver.Query = textBoxQueryString.Text;
            return true;
        }
    }
}
