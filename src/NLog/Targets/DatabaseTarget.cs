// 
// Copyright (c) 2004-2010 Jaroslaw Kowalski <jaak@jkowalski.net>
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

#if !SILVERLIGHT

namespace NLog.Targets
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Data;
    using System.Globalization;
    using System.Reflection;
    using System.Text;
    using NLog.Common;
    using NLog.Config;
    using NLog.Internal;
    using NLog.Layouts;

    /// <summary>
    /// Writes log messages to the database using an ADO.NET provider.
    /// </summary>
    /// <seealso href="http://nlog-project.org/wiki/Database_target">Documentation on NLog Wiki</seealso>
    /// <example>
    /// <para>
    /// The configuration is dependent on the database type, because
    /// there are differnet methods of specifying connection string, SQL
    /// command and command parameters.
    /// </para>
    /// <para>MS SQL Server using System.Data.SqlClient:</para>
    /// <code lang="XML" source="examples/targets/Configuration File/Database/MSSQL/NLog.config" height="450" />
    /// <para>Oracle using System.Data.OracleClient:</para>
    /// <code lang="XML" source="examples/targets/Configuration File/Database/Oracle.Native/NLog.config" height="350" />
    /// <para>Oracle using System.Data.OleDbClient:</para>
    /// <code lang="XML" source="examples/targets/Configuration File/Database/Oracle.OleDb/NLog.config" height="350" />
    /// <para>To set up the log target programmatically use code like this (an equivalent of MSSQL configuration):</para>
    /// <code lang="C#" source="examples/targets/Configuration API/Database/MSSQL/Example.cs" height="630" />
    /// </example>
    [Target("Database")]
    public sealed class DatabaseTarget : Target
    {
        private static Assembly systemDataAssembly = typeof(IDbConnection).Assembly;

        private IDbConnection activeConnection = null;
        private string activeConnectionString;

        /// <summary>
        /// Initializes a new instance of the <see cref="DatabaseTarget" /> class.
        /// </summary>
        public DatabaseTarget()
        {
            this.Parameters = new List<DatabaseParameterInfo>();
            this.DbProvider = "sqlserver";
            this.DbHost = ".";
        }

        /// <summary>
        /// Gets or sets the name of the database provider. It can be:
        /// <c>sqlserver, mssql, microsoft, msde</c> (all for MSSQL database), <c>oledb, odbc</c> or other name in which case
        /// it's treated as a fully qualified type name of the data provider *Connection class.
        /// </summary>
        /// <docgen category='Connection Options' order='10' />
        [RequiredParameter]
        [DefaultValue("sqlserver")]
        public string DbProvider { get; set; }

        /// <summary>
        /// Gets or sets the connection string. When provided, it overrides the values
        /// specified in DbHost, DbUserName, DbPassword, DbDatabase.
        /// </summary>
        /// <docgen category='Connection Options' order='10' />
        public Layout ConnectionString { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to keep the 
        /// database connection open between the log events.
        /// </summary>
        /// <docgen category='Connection Options' order='10' />
        [DefaultValue(true)]
        public bool KeepConnection { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to use database transactions. 
        /// Some data providers require this.
        /// </summary>
        /// <docgen category='Connection Options' order='10' />
        [DefaultValue(false)]
        public bool UseTransactions { get; set; }

        /// <summary>
        /// Gets or sets the database host name. If the ConnectionString is not provided
        /// this value will be used to construct the "Server=" part of the
        /// connection string.
        /// </summary>
        /// <docgen category='Connection Options' order='10' />
        public Layout DbHost { get; set; }

        /// <summary>
        /// Gets or sets the database user name. If the ConnectionString is not provided
        /// this value will be used to construct the "User ID=" part of the
        /// connection string.
        /// </summary>
        /// <docgen category='Connection Options' order='10' />
        public Layout DbUserName { get; set; }

        /// <summary>
        /// Gets or sets the database password. If the ConnectionString is not provided
        /// this value will be used to construct the "Password=" part of the
        /// connection string.
        /// </summary>
        /// <docgen category='Connection Options' order='10' />
        public Layout DbPassword { get; set; }

        /// <summary>
        /// Gets or sets the database name. If the ConnectionString is not provided
        /// this value will be used to construct the "Database=" part of the
        /// connection string.
        /// </summary>
        /// <docgen category='Connection Options' order='10' />
        public Layout DbDatabase { get; set; }

        /// <summary>
        /// Gets or sets the text of the SQL command to be run on each log level.
        /// </summary>
        /// <remarks>
        /// Typically this is a SQL INSERT statement or a stored procedure call. 
        /// It should use the database-specific parameters (marked as <c>@parameter</c>
        /// for SQL server or <c>:parameter</c> for Oracle, other data providers
        /// have their own notation) and not the layout renderers, 
        /// because the latter is prone to SQL injection attacks.
        /// The layout renderers should be specified as &lt;parameter /&gt; elements instead.
        /// </remarks>
        /// <docgen category='SQL Statement' order='10' />
        [RequiredParameter]
        public Layout CommandText { get; set; }

        /// <summary>
        /// Gets the collection of parameters. Each parameter contains a mapping
        /// between NLog layout and a database named or positional parameter.
        /// </summary>
        /// <docgen category='SQL Statement' order='11' />
        [ArrayParameter(typeof(DatabaseParameterInfo), "parameter")]
        public IList<DatabaseParameterInfo> Parameters { get; private set; }

        internal Type ConnectionType { get; set; }

        internal ConstructorInfo ConnectionConstructorInfo { get; set; }

        /// <summary>
        /// Initializes the target. Can be used by inheriting classes
        /// to initialize logging.
        /// </summary>
        protected override void InitializeTarget()
        {
            base.InitializeTarget();
            switch (this.DbProvider.ToUpper(CultureInfo.InvariantCulture))
            {
                case "sqlserver":
                case "mssql":
                case "microsoft":
                case "msde":
                    this.ConnectionType = systemDataAssembly.GetType("System.Data.SqlClient.SqlConnection", true);
                    break;

                case "oledb":
                    this.ConnectionType = systemDataAssembly.GetType("System.Data.OleDb.OleDbConnection", true);
                    break;

                case "odbc":
                    this.ConnectionType = systemDataAssembly.GetType("System.Data.Odbc.OdbcConnection", true);
                    break;

                default:
                    this.ConnectionType = Type.GetType(this.DbProvider, true);
                    break;
            }

            this.ConnectionConstructorInfo = this.ConnectionType.GetConstructor(new[] { typeof(string) });
            if (this.ConnectionType == null)
            {
                throw new NLogConfigurationException("Constructor with a string parameter not found on '" + this.ConnectionType.FullName + "' type.");
            }
        }

        /// <summary>
        /// Closes the target and releases any unmanaged resources.
        /// </summary>
        protected override void CloseTarget()
        {
            base.CloseTarget();

            this.CloseConnection();
        }

        /// <summary>
        /// Writes the specified logging event to the database. It creates
        /// a new database command, prepares parameters for it by calculating
        /// layouts and executes the command.
        /// </summary>
        /// <param name="logEvent">The logging event.</param>
        protected override void Write(LogEventInfo logEvent)
        {
            try
            {
                string connectionString = this.BuildConnectionString(logEvent);
                this.WriteEventToDatabase(logEvent, connectionString);
            }
            catch (Exception ex)
            {
                InternalLogger.Error("Error when writing to database {0}", ex);
                this.CloseConnection();
                throw;
            }
            finally
            {
                if (!this.KeepConnection)
                {
                    this.CloseConnection();
                }
            }
        }

        /// <summary>
        /// Writes an array of logging events to the log target. By default it iterates on all
        /// events and passes them to "Write" method. Inheriting classes can use this method to
        /// optimize batch writes.
        /// </summary>
        /// <param name="logEvents">Logging events to be written out.</param>
        protected override void Write(AsyncLogEventInfo[] logEvents)
        {
            string[] connectionStrings = new string[logEvents.Length];
            for (int i = 0; i < logEvents.Length; ++i)
            {
                connectionStrings[i] = this.BuildConnectionString(logEvents[i].LogEvent);
            }

            var buckets = SortHelpers.BucketSort(connectionStrings, logEvents);

            try
            {
                foreach (var kvp in buckets)
                {
                    string connectionString = kvp.Key;
                    foreach (AsyncLogEventInfo ev in kvp.Value)
                    {
                        try
                        {
                            this.WriteEventToDatabase(ev.LogEvent, connectionString);
                            ev.Continuation(null);
                        }
                        catch (Exception ex)
                        {
                            // in case of exception, close the connection and report it
                            InternalLogger.Error("Error when writing to database {0}", ex);
                            this.CloseConnection();
                            ev.Continuation(ex);
                        }
                    }
                }
            }
            finally
            {
                if (!this.KeepConnection)
                {
                    this.CloseConnection();
                }
            }
        }

        private void WriteEventToDatabase(LogEventInfo logEvent, string connectionString)
        {
            this.EnsureConnectionOpen(this.BuildConnectionString(logEvent));

            IDbCommand command = this.activeConnection.CreateCommand();
            command.CommandText = this.CommandText.Render(logEvent);

            foreach (DatabaseParameterInfo par in this.Parameters)
            {
                IDbDataParameter p = command.CreateParameter();
                p.Direction = ParameterDirection.Input;
                if (par.Name != null)
                {
                    p.ParameterName = par.Name;
                }

                if (par.Size != 0)
                {
                    p.Size = par.Size;
                }

                if (par.Precision != 0)
                {
                    p.Precision = par.Precision;
                }

                if (par.Scale != 0)
                {
                    p.Scale = par.Scale;
                }

                string stringValue = par.Layout.Render(logEvent);

                p.Value = stringValue;
                command.Parameters.Add(p);
            }

            command.ExecuteNonQuery();
        }

        private IDbConnection OpenConnection(string connectionString)
        {
            var retVal = (IDbConnection)this.ConnectionConstructorInfo.Invoke(new object[] { connectionString });

            if (retVal != null)
            {
                retVal.Open();
            }

            return retVal;
        }

        private string BuildConnectionString(LogEventInfo logEvent)
        {
            if (this.ConnectionString != null)
            {
                return this.ConnectionString.Render(logEvent);
            }

            StringBuilder sb = new StringBuilder();

            sb.Append("Server=");
            sb.Append(this.DbHost.Render(logEvent));
            sb.Append(";");
            if (this.DbUserName == null)
            {
                sb.Append("Trusted_Connection=SSPI;");
            }
            else
            {
                sb.Append("User id=");
                sb.Append(this.DbUserName.Render(logEvent));
                sb.Append(";Password=");
                sb.Append(this.DbPassword.Render(logEvent));
                sb.Append(";");
            }

            if (this.DbDatabase != null)
            {
                sb.Append("Database=");
                sb.Append(this.DbDatabase.Render(logEvent));
            }

            return sb.ToString();
        }

        private void EnsureConnectionOpen(string connectionString)
        {
            if (this.activeConnection != null)
            {
                if (this.activeConnectionString != connectionString)
                {
                    this.CloseConnection();
                }
            }

            if (this.activeConnection != null)
            {
                return;
            }

            this.activeConnection = this.OpenConnection(connectionString);
            this.activeConnectionString = connectionString;
        }

        private void CloseConnection()
        {
            if (this.activeConnection != null)
            {
                this.activeConnection.Close();
                this.activeConnection = null;
                this.activeConnectionString = null;
            }
        }
    }
}

#endif