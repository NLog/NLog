// 
// Copyright (c) 2004,2005 Jaroslaw Kowalski <jkowalski@users.sourceforge.net>
// 
// 
// All rights reserved.
// 
// Redistribution and use in source and binary forms, with or without 
// modification, are permitted provided that the following conditions 
// are met:
// 
// * Redistributions of source code must retain the above copyright notice, 
//   this list of conditions and the following disclaimer. 
// 
// * Redistributions in binary form must reproduce the above copyright notice,
//   this list of conditions and the following disclaimer in the documentation
//   and/or other materials provided with the distribution. 
// 
// * Neither the name of the Jaroslaw Kowalski nor the names of its 
//   contributors may be used to endorse or promote products derived from this
//   software without specific prior written permission. 
// 
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS"
// AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE 
// IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE 
// ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT OWNER OR CONTRIBUTORS BE 
// LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR 
// CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF
// SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS 
// INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN 
// CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) 
// ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF 
// THE POSSIBILITY OF SUCH DAMAGE.
// 

using System;
using System.Net;
using System.Net.Sockets;
using System.Xml;
using System.IO;
using System.Xml.Serialization;
using System.Windows.Forms;
using System.Collections.Specialized;

using NLogViewer.Configuration;
using NLogViewer.Events;
using NLogViewer.Parsers;
using System.ComponentModel;
using System.Drawing.Design;
using System.Windows.Forms.Design;
using NLogViewer.Receivers.UI;
using System.Data;

namespace NLogViewer.Receivers
{
    [LogEventReceiver("DATABASE", "Database Receiver", "Receives log events by executing an SQL query on a database")]
    public class DatabaseReceiver : LogEventReceiverSkeleton, IWizardConfigurable
    {
        private string _connectionString;
        private string _connectionType;
        private string _query;
        private IsolationLevel _isolationLevel = IsolationLevel.ReadCommitted;

        public string ConnectionType
        {
            get { return _connectionType; }
            set { _connectionType = value; }
        }

        public string ConnectionString
        {
            get { return _connectionString; }
            set { _connectionString = value; }
        }

        public IsolationLevel IsolationLevel
        {
            get { return _isolationLevel; }
            set { _isolationLevel = value; }
        }

        public string Query
        {
            get { return _query; }
            set { _query = value; }
        }

        public DatabaseReceiver()
        {
        }

        public override void InputThread()
        {
            try
            {
                Type connectionType = Type.GetType(ConnectionType, true);
                using (IDbConnection conn = (IDbConnection)Activator.CreateInstance(connectionType))
                {
                    conn.ConnectionString = ConnectionString;
                    conn.Open();

                    using (IDbTransaction tran = conn.BeginTransaction(_isolationLevel))
                    {
                        using (IDbCommand cmd = conn.CreateCommand())
                        {
                            cmd.Transaction = tran;
                            cmd.CommandText = Query;
                            using (IDataReader reader = cmd.ExecuteReader(CommandBehavior.SequentialAccess))
                            {
                                int[] ordinal2logeventordinal = null;

                                while (reader.Read())
                                {
                                    LogEvent le = CreateLogEvent();

                                    if (ordinal2logeventordinal == null)
                                    {
                                        ordinal2logeventordinal = new int[reader.FieldCount];

                                        for (int i = 0; i < reader.FieldCount; ++i)
                                        {
                                            ordinal2logeventordinal[i] = le.Columns.GetOrAllocateOrdinal(reader.GetName(i));
                                        }
                                    }

                                    for (int i = 0; i < reader.FieldCount; ++i)
                                    {
                                        if (!reader.IsDBNull(i))
                                            le[ordinal2logeventordinal[i]] = reader.GetValue(i);
                                    }
                                    EventReceived(le);
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }

        public IWizardPage GetWizardPage()
        {
            return new DatabaseReceiverPropertyPage(this);
        }
    }
}
