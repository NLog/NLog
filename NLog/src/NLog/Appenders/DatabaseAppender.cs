// 
// Copyright (c) 2004 Jaroslaw Kowalski <jaak@polbox.com>
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
using System.Diagnostics;
using System.Reflection;
using System.Data;
using System.Collections;

using NLog.Internal;
using NLog.Config;

namespace NLog.Appenders
{
    [Appender("Database")]
    public sealed class DatabaseAppender : Appender
    {
        private Assembly _system_data_assembly = typeof(IDbConnection).Assembly;
        private Type _connectionType = null;
        private string _connectionString = null;
        private bool _keepConnection = true;
        private bool _useTransaction = false;
        private string _dbHost = ".";
        private string _dbUserName = null;
        private string _dbPassword = null;
        private string _dbDatabase = null;
        private string _commandText = null;
        private ArrayList _parameters = new ArrayList();
        private IDbConnection _activeConnection = null;

        public DatabaseAppender()
        {
            Provider = "sqlserver";
        }

        public string Provider
        {
            get { return _connectionType.FullName; }
            set { 
                switch (value)
                {
                    case "sqlserver":
                    case "mssql":
                    case "microsoft":
                    case "msde":
                        _connectionType = _system_data_assembly.GetType("System.Data.SqlClient.SqlConnection");
                    break;

                    case "oledb":
                        _connectionType = _system_data_assembly.GetType("System.Data.OleDb.OleDbConnection");
                    break;

                    case "odbc":
                        _connectionType = _system_data_assembly.GetType("System.Data.Odbc.OdbcConnection");
                    break;

                    default:
                        _connectionType = Type.GetType(value); 
                        break;
                }
            }
                
        }

        [RequiredParameter]
        public string ConnectionString
        {
            get { return _connectionString; }
            set { _connectionString = value; }
        }

        public bool KeepConnection
        {
            get { return _keepConnection; }
            set { _keepConnection = value; }
        }

        public bool UseTransactions
        {
            get { return _useTransaction; }
            set { _useTransaction = value; }
        }

        public string DBHost
        {
            get { return _dbHost; }
            set { _dbHost = value; }
        }

        public string DBUserName
        {
            get { return _dbUserName; }
            set { _dbUserName = value; }
        }

        public string DBPassword
        {
            get { return _dbPassword; }
            set { _dbPassword = value; }
        }

        public string DBDatabase
        {
            get { return _dbDatabase; }
            set { _dbDatabase = value; }
        }

        public string CommandText
        {
            get { return _commandText; }
            set { _commandText = value; }
        }

        [ArrayParameter(typeof(ParameterInfo),"parameter")]
        public ArrayList Parameters
        {
            get { return _parameters; }
        }

        protected internal override void Append(LogEventInfo ev) {
            if (_keepConnection) {
                lock (this) {
                    if (_activeConnection == null)
                        _activeConnection = OpenConnection();
                    DoAppend(ev);
                }
            } else {
                try {
                    _activeConnection = OpenConnection();
                    DoAppend(ev);
                }
                finally {
                    if (_activeConnection != null) {
                        _activeConnection.Close();
                        _activeConnection = null;
                    }
                }
            }
        }

        private void DoAppend(LogEventInfo ev) {
        }

        private IDbConnection OpenConnection() {
            ConstructorInfo constructor = _connectionType.GetConstructor(new Type[] { typeof(string) });
            IDbConnection retVal = (IDbConnection)constructor.Invoke(new object[] { _connectionString });

            if (retVal != null)
                retVal.Open();

            return retVal;
        }

        public class ParameterInfo
        {
            public ParameterInfo()
            {
            }

            private Layout _compiledlayout;
            private string _name;

            [RequiredParameter]
            public string Name
            {
                get { return _name; }
                set { _name = value; }
            }

            [RequiredParameter]
            public string Layout
            {
                get { return _compiledlayout.Text; }
                set { _compiledlayout = new Layout(value); }
            }

            public Layout CompiledLayout
            {
                get { return _compiledlayout; }
                set { _compiledlayout = value; }
            }
        }
    }
}
