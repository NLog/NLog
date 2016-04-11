// 
// Copyright (c) 2004-2016 Jaroslaw Kowalski <jaak@jkowalski.net>, Kim Christensen, Julian Verdurmen
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

#if !SILVERLIGHT && !__IOS__ && !__ANDROID__

namespace NLog.Targets
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Configuration;
    using System.Data;
    using System.Data.Common;
    using System.Globalization;
    using System.Reflection;
    using System.Text;
    using System.Transactions;
    using NLog.Common;
    using NLog.Config;
    using NLog.Internal;
    using NLog.Layouts;
    using ConfigurationManager = System.Configuration.ConfigurationManager;

    /// <summary>
    /// Writes log messages to the database using an ADO.NET provider.
    /// </summary>
    /// <seealso href="https://github.com/nlog/nlog/wiki/Database-target">Documentation on NLog Wiki</seealso>
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
    /// <para>Oracle using System.Data.OleDBClient:</para>
    /// <code lang="XML" source="examples/targets/Configuration File/Database/Oracle.OleDB/NLog.config" height="350" />
    /// <para>To set up the log target programmatically use code like this (an equivalent of MSSQL configuration):</para>
    /// <code lang="C#" source="examples/targets/Configuration API/Database/MSSQL/Example.cs" height="630" />
    /// </example>
    [Target("Database")]
    public sealed class DatabaseTarget : Target, IInstallable
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
            this.InstallDdlCommands = new List<DatabaseCommandInfo>();
            this.UninstallDdlCommands = new List<DatabaseCommandInfo>();
            this.DBProvider = "sqlserver";
            this.DBHost = ".";
            this.ConnectionStringsSettings = ConfigurationManager.ConnectionStrings;
            this.CommandType = CommandType.Text;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DatabaseTarget" /> class.
        /// </summary>
        /// <param name="name">Name of the target.</param>
        public DatabaseTarget(string name) : this()
        {
            this.Name = name;
        }

        /// <summary>
        /// Gets or sets the name of the database provider.
        /// </summary>
        /// <remarks>
        /// <para>
        /// The parameter name should be a provider invariant name as registered in machine.config or app.config. Common values are:
        /// </para>
        /// <ul>
        /// <li><c>System.Data.SqlClient</c> - <see href="http://msdn.microsoft.com/en-us/library/system.data.sqlclient.aspx">SQL Sever Client</see></li>
        /// <li><c>System.Data.SqlServerCe.3.5</c> - <see href="http://www.microsoft.com/sqlserver/2005/en/us/compact.aspx">SQL Sever Compact 3.5</see></li>
        /// <li><c>System.Data.OracleClient</c> - <see href="http://msdn.microsoft.com/en-us/library/system.data.oracleclient.aspx">Oracle Client from Microsoft</see> (deprecated in .NET Framework 4)</li>
        /// <li><c>Oracle.DataAccess.Client</c> - <see href="http://www.oracle.com/technology/tech/windows/odpnet/index.html">ODP.NET provider from Oracle</see></li>
        /// <li><c>System.Data.SQLite</c> - <see href="http://sqlite.phxsoftware.com/">System.Data.SQLite driver for SQLite</see></li>
        /// <li><c>Npgsql</c> - <see href="http://npgsql.projects.postgresql.org/">Npgsql driver for PostgreSQL</see></li>
        /// <li><c>MySql.Data.MySqlClient</c> - <see href="http://www.mysql.com/downloads/connector/net/">MySQL Connector/Net</see></li>
        /// </ul>
        /// <para>(Note that provider invariant names are not supported on .NET Compact Framework).</para>
        /// <para>
        /// Alternatively the parameter value can be be a fully qualified name of the provider 
        /// connection type (class implementing <see cref="IDbConnection" />) or one of the following tokens:
        /// </para>
        /// <ul>
        /// <li><c>sqlserver</c>, <c>mssql</c>, <c>microsoft</c> or <c>msde</c> - SQL Server Data Provider</li>
        /// <li><c>oledb</c> - OLEDB Data Provider</li>
        /// <li><c>odbc</c> - ODBC Data Provider</li>
        /// </ul>
        /// </remarks>
        /// <docgen category='Connection Options' order='10' />
        [RequiredParameter]
        [DefaultValue("sqlserver")]
        public string DBProvider { get; set; }

        /// <summary>
        /// Gets or sets the name of the connection string (as specified in <see href="http://msdn.microsoft.com/en-us/library/bf7sd233.aspx">&lt;connectionStrings&gt; configuration section</see>.
        /// </summary>
        /// <docgen category='Connection Options' order='10' />
        public string ConnectionStringName { get; set; }

        /// <summary>
        /// Gets or sets the connection string. When provided, it overrides the values
        /// specified in DBHost, DBUserName, DBPassword, DBDatabase.
        /// </summary>
        /// <docgen category='Connection Options' order='10' />
        public Layout ConnectionString { get; set; }

        /// <summary>
        /// Gets or sets the connection string using for installation and uninstallation. If not provided, regular ConnectionString is being used.
        /// </summary>
        /// <docgen category='Installation Options' order='10' />
        public Layout InstallConnectionString { get; set; }

        /// <summary>
        /// Gets the installation DDL commands.
        /// </summary>
        /// <docgen category='Installation Options' order='10' />
        [ArrayParameter(typeof(DatabaseCommandInfo), "install-command")]
        public IList<DatabaseCommandInfo> InstallDdlCommands { get; private set; }

        /// <summary>
        /// Gets the uninstallation DDL commands.
        /// </summary>
        /// <docgen category='Installation Options' order='10' />
        [ArrayParameter(typeof(DatabaseCommandInfo), "uninstall-command")]
        public IList<DatabaseCommandInfo> UninstallDdlCommands { get; private set; }

        /// <summary>
        /// Gets or sets a value indicating whether to keep the 
        /// database connection open between the log events.
        /// </summary>
        /// <docgen category='Connection Options' order='10' />
        [DefaultValue(false)]
        public bool KeepConnection { get; set; }

        /// <summary>
        /// Obsolete - value will be ignored! The logging code always runs outside of transaction. 
        /// 
        /// Gets or sets a value indicating whether to use database transactions. 
        /// Some data providers require this.
        /// </summary>
        /// <docgen category='Connection Options' order='10' />
        /// <remarks>
        /// This option was removed in NLog 4.0 because the logging code always runs outside of transaction. 
        /// This ensures that the log gets written to the database if you rollback the main transaction because of an error and want to log the error.
        /// </remarks>
        [Obsolete("Obsolete - value will be ignored - logging code always runs outside of transaction. Will be removed in NLog 6.")]
        public bool? UseTransactions { get; set; }

        /// <summary>
        /// Gets or sets the database host name. If the ConnectionString is not provided
        /// this value will be used to construct the "Server=" part of the
        /// connection string.
        /// </summary>
        /// <docgen category='Connection Options' order='10' />
        public Layout DBHost { get; set; }

        /// <summary>
        /// Gets or sets the database user name. If the ConnectionString is not provided
        /// this value will be used to construct the "User ID=" part of the
        /// connection string.
        /// </summary>
        /// <docgen category='Connection Options' order='10' />
        public Layout DBUserName { get; set; }

        /// <summary>
        /// Gets or sets the database password. If the ConnectionString is not provided
        /// this value will be used to construct the "Password=" part of the
        /// connection string.
        /// </summary>
        /// <docgen category='Connection Options' order='10' />
        public Layout DBPassword { get; set; }

        /// <summary>
        /// Gets or sets the database name. If the ConnectionString is not provided
        /// this value will be used to construct the "Database=" part of the
        /// connection string.
        /// </summary>
        /// <docgen category='Connection Options' order='10' />
        public Layout DBDatabase { get; set; }

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
        /// Gets or sets the type of the SQL command to be run on each log level.
        /// </summary>
        /// <remarks>
        /// This specifies how the command text is interpreted, as "Text" (default) or as "StoredProcedure".
        /// When using the value StoredProcedure, the commandText-property would 
        /// normally be the name of the stored procedure. TableDirect method is not supported in this context.
        /// </remarks>
        /// <docgen category='SQL Statement' order='11' />
        [DefaultValue(CommandType.Text)]
        public CommandType CommandType { get; set; }

        /// <summary>
        /// Gets the collection of parameters. Each parameter contains a mapping
        /// between NLog layout and a database named or positional parameter.
        /// </summary>
        /// <docgen category='SQL Statement' order='12' />
        [ArrayParameter(typeof(DatabaseParameterInfo), "parameter")]
        public IList<DatabaseParameterInfo> Parameters { get; private set; }

        internal DbProviderFactory ProviderFactory { get; set; }

        // this is so we can mock the connection string without creating sub-processes
        internal ConnectionStringSettingsCollection ConnectionStringsSettings { get; set; }

        internal Type ConnectionType { get; set; }

        /// <summary>
        /// Performs installation which requires administrative permissions.
        /// </summary>
        /// <param name="installationContext">The installation context.</param>
        public void Install(InstallationContext installationContext)
        {
            this.RunInstallCommands(installationContext, this.InstallDdlCommands);
        }

        /// <summary>
        /// Performs uninstallation which requires administrative permissions.
        /// </summary>
        /// <param name="installationContext">The installation context.</param>
        public void Uninstall(InstallationContext installationContext)
        {
            this.RunInstallCommands(installationContext, this.UninstallDdlCommands);
        }

        /// <summary>
        /// Determines whether the item is installed.
        /// </summary>
        /// <param name="installationContext">The installation context.</param>
        /// <returns>
        /// Value indicating whether the item is installed or null if it is not possible to determine.
        /// </returns>
        public bool? IsInstalled(InstallationContext installationContext)
        {
            return null;
        }

        internal IDbConnection OpenConnection(string connectionString)
        {
            IDbConnection connection;

            if (this.ProviderFactory != null)
            {
                connection = this.ProviderFactory.CreateConnection();
            }
            else
            {
                connection = (IDbConnection)Activator.CreateInstance(this.ConnectionType);
            }

            connection.ConnectionString = connectionString;
            connection.Open();
            return connection;
        }

        /// <summary>
        /// Initializes the target. Can be used by inheriting classes
        /// to initialize logging.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId = "connectionStrings", Justification = "Name of the config file section.")]
        protected override void InitializeTarget()
        {
            base.InitializeTarget();

#pragma warning disable 618
            if (UseTransactions.HasValue)
#pragma warning restore 618
            {
                InternalLogger.Warn("UseTransactions is obsolete and will not be used - will be removed in NLog 6");
            }

            bool foundProvider = false;

            if (!string.IsNullOrEmpty(this.ConnectionStringName))
            {
                // read connection string and provider factory from the configuration file
                var cs = this.ConnectionStringsSettings[this.ConnectionStringName];
                if (cs == null)
                {
                    throw new NLogConfigurationException("Connection string '" + this.ConnectionStringName + "' is not declared in <connectionStrings /> section.");
                }

                this.ConnectionString = SimpleLayout.Escape(cs.ConnectionString);
                if (!string.IsNullOrEmpty(cs.ProviderName))
                {
                    this.ProviderFactory = DbProviderFactories.GetFactory(cs.ProviderName);
                    foundProvider = true;
                }
            
            }

            if (!foundProvider)
            {
                foreach (DataRow row in DbProviderFactories.GetFactoryClasses().Rows)
                {
                    var invariantname = (string)row["InvariantName"];
                    if (invariantname == this.DBProvider)
                    {
                        this.ProviderFactory = DbProviderFactories.GetFactory(this.DBProvider);
                        foundProvider = true;
                        break;
                    }
                }
            }

            if (!foundProvider)
            {
                switch (this.DBProvider.ToUpper(CultureInfo.InvariantCulture))
                {
                    case "SQLSERVER":
                    case "MSSQL":
                    case "MICROSOFT":
                    case "MSDE":
                        this.ConnectionType = systemDataAssembly.GetType("System.Data.SqlClient.SqlConnection", true);
                        break;

                    case "OLEDB":
                        this.ConnectionType = systemDataAssembly.GetType("System.Data.OleDb.OleDbConnection", true);
                        break;

                    case "ODBC":
                        this.ConnectionType = systemDataAssembly.GetType("System.Data.Odbc.OdbcConnection", true);
                        break;

                    default:
                        this.ConnectionType = Type.GetType(this.DBProvider, true);
                        break;
                }
            }
        }

        /// <summary>
        /// Closes the target and releases any unmanaged resources.
        /// </summary>
        protected override void CloseTarget()
        {
            base.CloseTarget();
            InternalLogger.Trace("DatabaseTarget: close connection because of CloseTarget");
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
                this.WriteEventToDatabase(logEvent);
            }
            catch (Exception exception)
            {
                InternalLogger.Error(exception, "Error when writing to database.");

                if (exception.MustBeRethrownImmediately())
                {
                    throw;
                }

                InternalLogger.Trace("DatabaseTarget: close connection because of error");
                this.CloseConnection();
                throw;
            }
            finally
            {
                if (!this.KeepConnection)
                {
                    InternalLogger.Trace("DatabaseTarget: close connection (KeepConnection = false).");
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
            var buckets = SortHelpers.BucketSort(logEvents, c => this.BuildConnectionString(c.LogEvent));

            try
            {
                foreach (var kvp in buckets)
                {
                    foreach (AsyncLogEventInfo ev in kvp.Value)
                    {
                        try
                        {
                            this.WriteEventToDatabase(ev.LogEvent);
                            ev.Continuation(null);
                        }
                        catch (Exception exception)
                        {
                            // in case of exception, close the connection and report it
                            InternalLogger.Error(exception, "Error when writing to database.");

                            if (exception.MustBeRethrownImmediately())
                            {
                                throw;
                            }
                            InternalLogger.Trace("DatabaseTarget: close connection because of exception");
                            this.CloseConnection();
                            ev.Continuation(exception);

                            if (exception.MustBeRethrown())
                            {
                                throw;
                            }
                        }
                    }
                }
            }
            finally
            {
                if (!this.KeepConnection)
                {
                    InternalLogger.Trace("DatabaseTarget: close connection because of KeepConnection=false");
                    this.CloseConnection();
                }
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security", "CA2100:Review SQL queries for security vulnerabilities", Justification = "It's up to the user to ensure proper quoting.")]
        private void WriteEventToDatabase(LogEventInfo logEvent)
        {
            //Always suppress transaction so that the caller does not rollback loggin if they are rolling back their transaction.
            using (TransactionScope transactionScope = new TransactionScope(TransactionScopeOption.Suppress))
            {
                this.EnsureConnectionOpen(this.BuildConnectionString(logEvent));

                IDbCommand command = this.activeConnection.CreateCommand();
                command.CommandText = this.CommandText.Render(logEvent);
                command.CommandType = this.CommandType;

                InternalLogger.Trace("Executing {0}: {1}", command.CommandType, command.CommandText);

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

                    InternalLogger.Trace("  Parameter: '{0}' = '{1}' ({2})", p.ParameterName, p.Value, p.DbType);
                }

                int result = command.ExecuteNonQuery();
                InternalLogger.Trace("Finished execution, result = {0}", result);

                //not really needed as there is no transaction at all.
                transactionScope.Complete();
            }
        }

        private string BuildConnectionString(LogEventInfo logEvent)
        {
            if (this.ConnectionString != null)
            {
                return this.ConnectionString.Render(logEvent);
            }

            var sb = new StringBuilder();

            sb.Append("Server=");
            sb.Append(this.DBHost.Render(logEvent));
            sb.Append(";");
            if (this.DBUserName == null)
            {
                sb.Append("Trusted_Connection=SSPI;");
            }
            else
            {
                sb.Append("User id=");
                sb.Append(this.DBUserName.Render(logEvent));
                sb.Append(";Password=");
                sb.Append(this.DBPassword.Render(logEvent));
                sb.Append(";");
            }

            if (this.DBDatabase != null)
            {
                sb.Append("Database=");
                sb.Append(this.DBDatabase.Render(logEvent));
            }

            return sb.ToString();
        }

        private void EnsureConnectionOpen(string connectionString)
        {
            if (this.activeConnection != null)
            {
                if (this.activeConnectionString != connectionString)
                {
                    InternalLogger.Trace("DatabaseTarget: close connection because of opening new.");
                    this.CloseConnection();
                }
            }

            if (this.activeConnection != null)
            {
                return;
            }

            InternalLogger.Trace("DatabaseTarget: open connection.");
            this.activeConnection = this.OpenConnection(connectionString);
            this.activeConnectionString = connectionString;
        }

        private void CloseConnection()
        {
            if (this.activeConnection != null)
            {
                this.activeConnection.Close();
                this.activeConnection.Dispose();
                this.activeConnection = null;
                this.activeConnectionString = null;
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security", "CA2100:Review SQL queries for security vulnerabilities", Justification = "It's up to the user to ensure proper quoting.")]
        private void RunInstallCommands(InstallationContext installationContext, IEnumerable<DatabaseCommandInfo> commands)
        {
            // create log event that will be used to render all layouts
            LogEventInfo logEvent = installationContext.CreateLogEvent();

            try
            {
                foreach (var commandInfo in commands)
                {
                    string cs;

                    if (commandInfo.ConnectionString != null)
                    {
                        // if there is connection string specified on the command info, use it
                        cs = commandInfo.ConnectionString.Render(logEvent);
                    }
                    else if (this.InstallConnectionString != null)
                    {
                        // next, try InstallConnectionString
                        cs = this.InstallConnectionString.Render(logEvent);
                    }
                    else
                    {
                        // if it's not defined, fall back to regular connection string
                        cs = this.BuildConnectionString(logEvent);
                    }

                    this.EnsureConnectionOpen(cs);

                    var command = this.activeConnection.CreateCommand();
                    command.CommandType = commandInfo.CommandType;
                    command.CommandText = commandInfo.Text.Render(logEvent);

                    try
                    {
                        installationContext.Trace("Executing {0} '{1}'", command.CommandType, command.CommandText);
                        command.ExecuteNonQuery();
                    }
                    catch (Exception exception)
                    {
                        if (exception.MustBeRethrownImmediately())
                        {
                            throw;
                        }

                        if (commandInfo.IgnoreFailures || installationContext.IgnoreFailures)
                        {
                            installationContext.Warning(exception.Message);
                        }
                        else
                        {
                            installationContext.Error(exception.Message);
                            throw;
                        }

                      
                    }
                }
            }
            finally
            {
                InternalLogger.Trace("DatabaseTarget: close connection after install.");

                this.CloseConnection();
            }
        }
    }
}

#endif
