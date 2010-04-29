// 
// Copyright (c) 2004-2006 Jaroslaw Kowalski <jaak@jkowalski.net>
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
// * Neither the name of Jaroslaw Kowalski nor the names of its 
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
using System.Text;
using System.Diagnostics;
using System.Reflection;
using System.Data;
using System.Collections;

using NLog.Internal;
using NLog.Config;

namespace NLog.Targets
{
    /// <summary>
    /// Writes logging messages to the database using an ADO.NET provider.
    /// </summary>
    /// <example>
    /// <para>
    /// The configuration is dependent on the database type, because
    /// there are differnet methods of specifying connection string, SQL
    /// command and command parameters.
    /// </para>
    /// <para>MS SQL Server using System.Data.SqlClient:</para>
    /// <code lang="XML" src="examples/targets/Configuration File/Database/MSSQL/NLog.config" height="450" />
    /// 
    /// <para>Oracle using System.Data.OracleClient:</para>
    /// <code lang="XML" src="examples/targets/Configuration File/Database/Oracle.Native/NLog.config" height="350" />
    /// 
    /// <para>Oracle using System.Data.OleDbClient:</para>
    /// <code lang="XML" src="examples/targets/Configuration File/Database/Oracle.OleDb/NLog.config" height="350" />
    /// 
    /// <para>To set up the log target programmatically use code like this (an equivalent of MSSQL configuration):</para>
    /// <code lang="C#" src="examples/targets/Configuration API/Database/MSSQL/Example.cs" height="630" />
    /// </example>
    [Target("Database", IgnoresLayout=true)]
    public sealed class DatabaseTarget: Target
    {
        private Assembly _system_data_assembly = typeof(IDbConnection).Assembly;
        private Type _connectionType = null;
        private bool _keepConnection = true;
        private bool _useTransaction = false;
        private Layout _connectionString = null;
        private Layout _dbHostLayout = new Layout(".");
        private Layout _dbUserNameLayout = null;
        private Layout _dbPasswordLayout = null;
        private Layout _dbDatabaseLayout = null;
        private Layout _compiledCommandTextLayout = null;
        private DatabaseParameterInfoCollection _parameters = new DatabaseParameterInfoCollection();
        private IDbConnection _activeConnection = null;
        private string _connectionStringCache = null;

        /// <summary>
        /// Creates a new instance of the <see cref="DatabaseTarget"/> object and sets
        /// the default values of some properties;
        /// </summary>
        public DatabaseTarget()
        {
            DBProvider = "sqlserver";
        }

        /// <summary>
        /// The name of the database provider. It can be:
        /// <c>sqlserver, mssql, microsoft, msde</c> (all for MSSQL database), <c>oledb, odbc</c> or other name in which case
        /// it's treated as a fully qualified type name of the data provider *Connection class.
        /// </summary>
        [RequiredParameter]
        [System.ComponentModel.DefaultValue("sqlserver")]
        public string DBProvider
        {
            get { return _connectionType.FullName; }
            set
            {
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

        /// <summary>
        /// The connection string. When provided, it overrides the values
        /// specified in DBHost, DBUserName, DBPassword, DBDatabase.
        /// </summary>
        [AcceptsLayout]
        public string ConnectionString
        {
            get { return Convert.ToString(_connectionString); }
            set { _connectionString = new Layout(value); }
        }

        /// <summary>
        /// Keep the database connection open between the log events.
        /// </summary>
        [System.ComponentModel.DefaultValue(true)]
        public bool KeepConnection
        {
            get { return _keepConnection; }
            set { _keepConnection = value; }
        }

        /// <summary>
        /// Use database transactions. Some data providers require this.
        /// </summary>
        [System.ComponentModel.DefaultValue(false)]
        public bool UseTransactions
        {
            get
            {
                return _useTransaction;
            
            }
            set { _useTransaction = value; }
        }

        /// <summary>
        /// The database host name. If the ConnectionString is not provided
        /// this value will be used to construct the "Server=" part of the
        /// connection string.
        /// </summary>
        [AcceptsLayout]
        public string DBHost
        {
            get { return Convert.ToString(_dbHostLayout); }
            set { _dbHostLayout = new Layout(value); }
        }

        /// <summary>
        /// The database host name. If the ConnectionString is not provided
        /// this value will be used to construct the "Server=" part of the
        /// connection string.
        /// </summary>
        public Layout DBHostLayout
        {
            get { return _dbHostLayout; }
            set { _dbHostLayout = value; }
        }

        /// <summary>
        /// The database user name. If the ConnectionString is not provided
        /// this value will be used to construct the "User ID=" part of the
        /// connection string.
        /// </summary>
        [AcceptsLayout]
        public string DBUserName
        {
            get { return Convert.ToString(_dbUserNameLayout); }
            set { _dbUserNameLayout = new Layout(value); }
        }

        /// <summary>
        /// The database user name. If the ConnectionString is not provided
        /// this value will be used to construct the "User ID=" part of the
        /// connection string.
        /// </summary>
        public Layout DBUserNameLayout
        {
            get { return _dbUserNameLayout; }
            set { _dbUserNameLayout = value; }
        }

        /// <summary>
        /// The database password. If the ConnectionString is not provided
        /// this value will be used to construct the "Password=" part of the
        /// connection string.
        /// </summary>
        [AcceptsLayout]
        public string DBPassword
        {
            get { return Convert.ToString(_dbPasswordLayout); }
            set { _dbPasswordLayout = new Layout(value); }
        }

        /// <summary>
        /// The database password. If the ConnectionString is not provided
        /// this value will be used to construct the "Password=" part of the
        /// connection string.
        /// </summary>
        public Layout DBPasswordLayout
        {
            get { return _dbPasswordLayout; }
            set { _dbPasswordLayout = value; }
        }

        /// <summary>
        /// The database name. If the ConnectionString is not provided
        /// this value will be used to construct the "Database=" part of the
        /// connection string.
        /// </summary>
        [AcceptsLayout]
        public string DBDatabase
        {
            get { return Convert.ToString(_dbDatabaseLayout); }
            set { _dbDatabaseLayout = new Layout(value); }
        }

        /// <summary>
        /// The database name. If the ConnectionString is not provided
        /// this value will be used to construct the "Database=" part of the
        /// connection string.
        /// </summary>
        public Layout DBDatabaseLayout
        {
            get { return _dbDatabaseLayout; }
            set { _dbDatabaseLayout = value; }
        }

        /// <summary>
        /// The text of the SQL command to be run on each log level.
        /// </summary>
        /// <remarks>
        /// Typically this is a SQL INSERT statement or a stored procedure call. 
        /// It should use the database-specific parameters (marked as <c>@parameter</c>
        /// for SQL server or <c>:parameter</c> for Oracle, other data providers
        /// have their own notation) and not the layout renderers, 
        /// because the latter is prone to SQL injection attacks.
        /// The layout renderers should be specified as &lt;parameters />&gt; instead.
        /// </remarks>
        [AcceptsLayout]
        [RequiredParameter]
        public string CommandText
        {
            get { return Convert.ToString(_compiledCommandTextLayout); }
            set { _compiledCommandTextLayout = new Layout(value); }
        }

        /// <summary>
        /// The text of the SQL command to be run on each log level.
        /// </summary>
        /// <remarks>
        /// Typically this is a SQL INSERT statement or a stored procedure call. 
        /// It should use the database-specific parameters (marked as <c>@parameter</c>
        /// for SQL server or <c>:parameter</c> for Oracle, other data providers
        /// have their own notation) and not the layout renderers, 
        /// because the latter is prone to SQL injection attacks.
        /// The layout renderers should be specified as &lt;parameters />&lt; instead.
        /// </remarks>
        public Layout CommandTextLayout
        {
            get { return _compiledCommandTextLayout; }
            set { _compiledCommandTextLayout = value; }
        }

        /// <summary>
        /// The collection of paramters. Each parameter contains a mapping
        /// between NLog layout and a database named or positional parameter.
        /// </summary>
        [ArrayParameter(typeof(DatabaseParameterInfo), "parameter")]
        public DatabaseParameterInfoCollection Parameters
        {
            get { return _parameters; }
        }

        /// <summary>
        /// Writes the specified logging event to the database. It creates
        /// a new database command, prepares parameters for it by calculating
        /// layouts and executes the command.
        /// </summary>
        /// <param name="logEvent">The logging event.</param>
        protected internal override void Write(LogEventInfo logEvent)
        {
            if (_keepConnection)
            {
                lock(this)
                {
                    if (_activeConnection == null)
                        _activeConnection = OpenConnection(logEvent);
                    DoAppend(logEvent);
                }
            }
            else
            {
                try
                {
                    _activeConnection = OpenConnection(logEvent);
                    DoAppend(logEvent);
                }
                finally
                {
                    if (_activeConnection != null)
                    {
                        _activeConnection.Close();
                        _activeConnection = null;
                    }
                }
            }
        }

        /// <summary>
        /// Adds all layouts used by this target to the specified collection.
        /// </summary>
        /// <param name="layouts">The collection to add layouts to.</param>
        public override void PopulateLayouts(LayoutCollection layouts)
        {
            base.PopulateLayouts (layouts);

            if (DBHostLayout != null) DBHostLayout.PopulateLayouts(layouts);
            if (DBUserNameLayout != null) DBUserNameLayout.PopulateLayouts(layouts);
            if (DBDatabaseLayout != null) DBDatabaseLayout.PopulateLayouts(layouts);
            if (DBPasswordLayout != null) DBPasswordLayout.PopulateLayouts(layouts);
            if (CommandTextLayout != null) CommandTextLayout.PopulateLayouts(layouts);

            for (int i = 0; i < Parameters.Count; ++i)
            {
                if (Parameters[i].CompiledLayout != null)
                    Parameters[i].CompiledLayout.PopulateLayouts(layouts);
            }
        }

        private void DoAppend(LogEventInfo logEvent)
        {
            IDbCommand command = _activeConnection.CreateCommand();
            command.CommandText = CommandTextLayout.GetFormattedMessage(logEvent);
            foreach (DatabaseParameterInfo par in Parameters)
            {
                IDbDataParameter p = command.CreateParameter();
                p.Direction = ParameterDirection.Input;
                if (par.Name != null)
                    p.ParameterName = par.Name;
                if (par.Size != 0)
                    p.Size = par.Size;
                if (par.Precision != 0)
                    p.Precision = par.Precision;
                if (par.Scale != 0)
                    p.Scale = par.Scale;
                p.Value = par.CompiledLayout.GetFormattedMessage(logEvent);
                command.Parameters.Add(p);
            }
            command.ExecuteNonQuery();
        }

        private IDbConnection OpenConnection(LogEventInfo logEvent)
        {
            ConstructorInfo constructor = _connectionType.GetConstructor(new Type[]
            {
                typeof(string)
            }

            );
            IDbConnection retVal = (IDbConnection)constructor.Invoke(new object[]
            {
                BuildConnectionString(logEvent)
            }

            );

            if (retVal != null)
                retVal.Open();

            return retVal;
        }

        private string BuildConnectionString(LogEventInfo logEvent)
        {
            if (_connectionStringCache != null)
                return _connectionStringCache;

            if (_connectionString != null)
                return _connectionString.GetFormattedMessage(logEvent);

            StringBuilder sb = new StringBuilder();

            sb.Append("Server=");
            sb.Append(DBHostLayout.GetFormattedMessage(logEvent));
            sb.Append(";");
            if (DBUserNameLayout == null)
            {
                sb.Append("Trusted_Connection=SSPI;");
            }
            else
            {
                sb.Append("User id=");
                sb.Append(DBUserNameLayout.GetFormattedMessage(logEvent));
                sb.Append(";Password=");
                sb.Append(DBPasswordLayout.GetFormattedMessage(logEvent));
                sb.Append(";");
            }

            if (DBDatabaseLayout != null)
            {
                sb.Append("Database=");
                sb.Append(DBDatabaseLayout.GetFormattedMessage(logEvent));
            }

            _connectionStringCache = sb.ToString();

            InternalLogger.Debug("Connection string: {0}", _connectionStringCache);
            return _connectionStringCache;
        }
    }
}
