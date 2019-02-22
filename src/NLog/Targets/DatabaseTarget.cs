// 
// Copyright (c) 2004-2019 Jaroslaw Kowalski <jaak@jkowalski.net>, Kim Christensen, Julian Verdurmen
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

    using System.Data;
    using System.Data.Common;
#if NETSTANDARD
    using System.Reflection;
#endif
    using System.Text;
#if !NETSTANDARD1_0
    using System.Transactions;
#endif

    using NLog.Common;
    using NLog.Config;
    using NLog.Internal;
    using NLog.Layouts;

#if !NETSTANDARD
    using System.Configuration;
    using ConfigurationManager = System.Configuration.ConfigurationManager;
#endif

    /// <summary>
    /// Writes log messages to the database using an ADO.NET provider.
    /// </summary>
    /// <remarks>
    /// - NETSTANDARD cannot load connectionstrings from .config
    /// </remarks>
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
    public class DatabaseTarget : Target, IInstallable
    {
        private IDbConnection _activeConnection;
        private string _activeConnectionString;

        /// <summary>
        /// Initializes a new instance of the <see cref="DatabaseTarget" /> class.
        /// </summary>
        public DatabaseTarget()
        {
            Parameters = new List<DatabaseParameterInfo>();
            InstallDdlCommands = new List<DatabaseCommandInfo>();
            UninstallDdlCommands = new List<DatabaseCommandInfo>();
            DBProvider = "sqlserver";
            DBHost = ".";
#if !NETSTANDARD
            ConnectionStringsSettings = ConfigurationManager.ConnectionStrings;
#endif
            CommandType = CommandType.Text;
            OptimizeBufferReuse = GetType() == typeof(DatabaseTarget);  // Class not sealed, reduce breaking changes
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DatabaseTarget" /> class.
        /// </summary>
        /// <param name="name">Name of the target.</param>
        public DatabaseTarget(string name) : this()
        {
            Name = name;
        }

        /// <summary>
        /// Gets or sets the name of the database provider.
        /// </summary>
        /// <remarks>
        /// <para>
        /// The parameter name should be a provider invariant name as registered in machine.config or app.config. Common values are:
        /// </para>
        /// <ul>
        /// <li><c>System.Data.SqlClient</c> - <see href="https://msdn.microsoft.com/en-us/library/system.data.sqlclient.aspx">SQL Sever Client</see></li>
        /// <li><c>System.Data.SqlServerCe.3.5</c> - <see href="https://www.microsoft.com/sqlserver/2005/en/us/compact.aspx">SQL Sever Compact 3.5</see></li>
        /// <li><c>System.Data.OracleClient</c> - <see href="https://msdn.microsoft.com/en-us/library/system.data.oracleclient.aspx">Oracle Client from Microsoft</see> (deprecated in .NET Framework 4)</li>
        /// <li><c>Oracle.DataAccess.Client</c> - <see href="https://www.oracle.com/technology/tech/windows/odpnet/index.html">ODP.NET provider from Oracle</see></li>
        /// <li><c>System.Data.SQLite</c> - <see href="http://sqlite.phxsoftware.com/">System.Data.SQLite driver for SQLite</see></li>
        /// <li><c>Npgsql</c> - <see href="https://www.npgsql.org/">Npgsql driver for PostgreSQL</see></li>
        /// <li><c>MySql.Data.MySqlClient</c> - <see href="https://www.mysql.com/downloads/connector/net/">MySQL Connector/Net</see></li>
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

#if !NETSTANDARD
        /// <summary>
        /// Gets or sets the name of the connection string (as specified in <see href="https://msdn.microsoft.com/en-us/library/bf7sd233.aspx">&lt;connectionStrings&gt; configuration section</see>.
        /// </summary>
        /// <docgen category='Connection Options' order='10' />
        public string ConnectionStringName { get; set; }
#endif

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
        [Obsolete("Value will be ignored as logging code always executes outside of a transaction. Marked obsolete on NLog 4.0 and it will be removed in NLog 6.")]
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
        /// <docgen category='SQL Statement' order='14' />
        [ArrayParameter(typeof(DatabaseParameterInfo), "parameter")]
        public IList<DatabaseParameterInfo> Parameters { get; private set; }

#if !NETSTANDARD
        internal DbProviderFactory ProviderFactory { get; set; }

        // this is so we can mock the connection string without creating sub-processes
        internal ConnectionStringSettingsCollection ConnectionStringsSettings { get; set; }
#endif

        internal Type ConnectionType { get; set; }

        private IPropertyTypeConverter PropertyTypeConverter
        {
            get => _propertyTypeConverter ?? (_propertyTypeConverter = ConfigurationItemFactory.Default.PropertyTypeConverter);
            set => _propertyTypeConverter = value;
        }
        private IPropertyTypeConverter _propertyTypeConverter;

        /// <summary>
        /// Performs installation which requires administrative permissions.
        /// </summary>
        /// <param name="installationContext">The installation context.</param>
        public void Install(InstallationContext installationContext)
        {
            RunInstallCommands(installationContext, InstallDdlCommands);
        }

        /// <summary>
        /// Performs uninstallation which requires administrative permissions.
        /// </summary>
        /// <param name="installationContext">The installation context.</param>
        public void Uninstall(InstallationContext installationContext)
        {
            RunInstallCommands(installationContext, UninstallDdlCommands);
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

#if !NETSTANDARD
            if (ProviderFactory != null)
            {
                connection = ProviderFactory.CreateConnection();
            }
            else
#endif
            {
                connection = (IDbConnection)Activator.CreateInstance(ConnectionType);
            }

            if (connection == null)
            {
                throw new NLogRuntimeException("Creation of connection failed");
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
                InternalLogger.Warn("DatabaseTarget(Name={0}): UseTransactions property is obsolete and will not be used - will be removed in NLog 6", Name);
            }

            bool foundProvider = false;
            string providerName = string.Empty;

#if !NETSTANDARD
            if (!string.IsNullOrEmpty(ConnectionStringName))
            {
                // read connection string and provider factory from the configuration file
                var cs = ConnectionStringsSettings[ConnectionStringName];
                if (cs == null)
                {
                    throw new NLogConfigurationException($"Connection string '{ConnectionStringName}' is not declared in <connectionStrings /> section.");
                }

                if (!string.IsNullOrEmpty(cs.ConnectionString?.Trim()))
                {
                    ConnectionString = SimpleLayout.Escape(cs.ConnectionString.Trim());
                }
                providerName = cs.ProviderName?.Trim() ?? string.Empty;
            }
#endif

            if (ConnectionString != null)
            {
                try
                {
                    var connectionString = BuildConnectionString(LogEventInfo.CreateNullEvent());
                    var dbConnectionStringBuilder = new DbConnectionStringBuilder { ConnectionString = connectionString };
                    if (dbConnectionStringBuilder.TryGetValue("provider connection string", out var connectionStringValue))
                    {
                        // Special Entity Framework Connection String
                        if (dbConnectionStringBuilder.TryGetValue("provider", out var providerValue))
                        {
                            // Provider was overriden by ConnectionString
                            providerName = providerValue.ToString()?.Trim() ?? string.Empty;
                        }

                        // ConnectionString was overriden by ConnectionString :)
                        ConnectionString = SimpleLayout.Escape(connectionStringValue.ToString());
                    }
                }
                catch (Exception ex)
                {
#if !NETSTANDARD
                    if (!string.IsNullOrEmpty(ConnectionStringName))
                        InternalLogger.Warn(ex, "DatabaseTarget(Name={0}): DbConnectionStringBuilder failed to parse '{1}' ConnectionString", Name, ConnectionStringName);
                    else
#endif
                        InternalLogger.Warn(ex, "DatabaseTarget(Name={0}): DbConnectionStringBuilder failed to parse ConnectionString", Name);
                }
            }

#if !NETSTANDARD
            if (string.IsNullOrEmpty(providerName))
            {
                providerName = GetProviderNameFromDbProviderFactories(providerName);
            }

            if (!string.IsNullOrEmpty(providerName))
            {
                try
                {
                    ProviderFactory = DbProviderFactories.GetFactory(providerName);
                    foundProvider = true;
                }
                catch (Exception ex)
                {
                    InternalLogger.Error(ex, "DatabaseTarget(Name={0}): DbProviderFactories failed to get factory from ProviderName={1}", Name, providerName);
                    throw;
                }
            }
#endif

            if (!foundProvider)
            {
                try
                {
                    SetConnectionType();
                    if (ConnectionType == null)
                    {
                        InternalLogger.Warn("DatabaseTarget(Name={0}): No ConnectionType created from DBProvider={1}", Name, DBProvider);
                    }
                }
                catch (Exception ex)
                {
                    InternalLogger.Error(ex, "DatabaseTarget(Name={0}): Failed to create ConnectionType from DBProvider={1}", Name, DBProvider);
                    throw;
                }
            }
        }

#if !NETSTANDARD
        private string GetProviderNameFromDbProviderFactories(string providerName)
        {
            string dbProvider = DBProvider?.Trim() ?? string.Empty;
            if (!string.IsNullOrEmpty(dbProvider))
            {
                foreach (DataRow row in DbProviderFactories.GetFactoryClasses().Rows)
                {
                    var invariantname = (string)row["InvariantName"];
                    if (string.Equals(invariantname, dbProvider, StringComparison.OrdinalIgnoreCase))
                    {
                        providerName = invariantname;
                        break;
                    }
                }
            }

            return providerName;
        }
#endif
        /// <summary>
        /// Set the <see cref="ConnectionType"/> to use it for opening connections to the database.
        /// </summary>
        private void SetConnectionType()
        {
            switch (DBProvider.ToUpperInvariant())
            {
                case "SQLSERVER":
                case "MSSQL":
                case "MICROSOFT":
                case "MSDE":
                case "SYSTEM.DATA.SQLCLIENT":
                    {
#if NETSTANDARD
                        var assembly = Assembly.Load(new AssemblyName("System.Data.SqlClient"));
#else
                        var assembly = typeof(IDbConnection).GetAssembly();
#endif
                        ConnectionType = assembly.GetType("System.Data.SqlClient.SqlConnection", true, true);
                        break;
                    }
#if !NETSTANDARD
                case "OLEDB":
                    {
                        var assembly = typeof(IDbConnection).GetAssembly();
                        ConnectionType = assembly.GetType("System.Data.OleDb.OleDbConnection", true, true);
                        break;
                    }
#endif
                case "ODBC":
                case "SYSTEM.DATA.ODBC":
                    {
#if NETSTANDARD
                        var assembly = Assembly.Load(new AssemblyName("System.Data.Odbc"));
#else
                        var assembly = typeof(IDbConnection).GetAssembly();
#endif
                        ConnectionType = assembly.GetType("System.Data.Odbc.OdbcConnection", true, true);
                        break;
                    }
                default:
                    ConnectionType = Type.GetType(DBProvider, true, true);
                    break;
            }
        }

        /// <summary>
        /// Closes the target and releases any unmanaged resources.
        /// </summary>
        protected override void CloseTarget()
        {
            PropertyTypeConverter = null;
            base.CloseTarget();
            InternalLogger.Trace("DatabaseTarget(Name={0}): Close connection because of CloseTarget", Name);
            CloseConnection();
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
                WriteEventToDatabase(logEvent);
            }
            catch (Exception exception)
            {
                InternalLogger.Error(exception, "DatabaseTarget(Name={0}): Error when writing to database.", Name);

                if (exception.MustBeRethrownImmediately())
                {
                    throw;
                }

                InternalLogger.Trace("DatabaseTarget(Name={0}): Close connection because of error", Name);
                CloseConnection();
                throw;
            }
            finally
            {
                if (!KeepConnection)
                {
                    InternalLogger.Trace("DatabaseTarget(Name={0}): Close connection (KeepConnection = false).", Name);
                    CloseConnection();
                }
            }
        }

        /// <summary>
        /// NOTE! Obsolete, instead override Write(IList{AsyncLogEventInfo} logEvents)
        /// 
        /// Writes an array of logging events to the log target. By default it iterates on all
        /// events and passes them to "Write" method. Inheriting classes can use this method to
        /// optimize batch writes.
        /// </summary>
        /// <param name="logEvents">Logging events to be written out.</param>
        [Obsolete("Instead override Write(IList<AsyncLogEventInfo> logEvents. Marked obsolete on NLog 4.5")]
        protected override void Write(AsyncLogEventInfo[] logEvents)
        {
            Write((IList<AsyncLogEventInfo>)logEvents);
        }

        /// <summary>
        /// Writes an array of logging events to the log target. By default it iterates on all
        /// events and passes them to "Write" method. Inheriting classes can use this method to
        /// optimize batch writes.
        /// </summary>
        /// <param name="logEvents">Logging events to be written out.</param>
        protected override void Write(IList<AsyncLogEventInfo> logEvents)
        {
            var buckets = logEvents.BucketSort(c => BuildConnectionString(c.LogEvent));

            try
            {
                foreach (var kvp in buckets)
                {
                    for (int i = 0; i < kvp.Value.Count; i++)
                    {
                        AsyncLogEventInfo ev = kvp.Value[i];

                        try
                        {
                            WriteEventToDatabase(ev.LogEvent);
                            ev.Continuation(null);
                        }
                        catch (Exception exception)
                        {
                            // in case of exception, close the connection and report it
                            InternalLogger.Error(exception, "DatabaseTarget(Name={0}): Error when writing to database.", Name);

                            if (exception.MustBeRethrownImmediately())
                            {
                                throw;
                            }
                            InternalLogger.Trace("DatabaseTarget(Name={0}): Close connection because of exception", Name);
                            CloseConnection();
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
                if (!KeepConnection)
                {
                    InternalLogger.Trace("DatabaseTarget(Name={0}): Close connection because of KeepConnection=false", Name);
                    CloseConnection();
                }
            }
        }
        /// <summary>
        /// Write logEvent to database
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security", "CA2100:Review SQL queries for security vulnerabilities", Justification = "It's up to the user to ensure proper quoting.")]
        private void WriteEventToDatabase(LogEventInfo logEvent)
        {
            //Always suppress transaction so that the caller does not rollback logging if they are rolling back their transaction.
            using (TransactionScope transactionScope = new TransactionScope(TransactionScopeOption.Suppress))
            {
                EnsureConnectionOpen(BuildConnectionString(logEvent));

                using (IDbCommand command = _activeConnection.CreateCommand())
                {
                    command.CommandText = RenderLogEvent(CommandText, logEvent);
                    if (command.CommandType != CommandType)
                    {
                        command.CommandType = CommandType;  // Some DbProviders will throw when trying to modify CommandType
                    }

                    InternalLogger.Trace("DatabaseTarget(Name={0}): Executing {1}: {2}", Name, command.CommandType, command.CommandText);

                    AddParametersToCommand(command, Parameters, logEvent);

                    int result = command.ExecuteNonQuery();
                    InternalLogger.Trace("DatabaseTarget(Name={0}): Finished execution, result = {1}", Name, result);
                }

                //not really needed as there is no transaction at all.
                transactionScope.Complete();
            }
        }

        /// <summary>
        /// Build the connectionstring from the properties. 
        /// </summary>
        /// <remarks>
        ///  Using <see cref="ConnectionString"/> at first, and falls back to the properties <see cref="DBHost"/>, 
        ///  <see cref="DBUserName"/>, <see cref="DBPassword"/> and <see cref="DBDatabase"/>
        /// </remarks>
        /// <param name="logEvent">Event to render the layout inside the properties.</param>
        /// <returns></returns>
        protected string BuildConnectionString(LogEventInfo logEvent)
        {
            if (ConnectionString != null)
            {
                return RenderLogEvent(ConnectionString, logEvent);
            }

            var sb = new StringBuilder();

            sb.Append("Server=");
            sb.Append(RenderLogEvent(DBHost, logEvent));
            sb.Append(";");
            if (DBUserName == null)
            {
                sb.Append("Trusted_Connection=SSPI;");
            }
            else
            {
                sb.Append("User id=");
                sb.Append(RenderLogEvent(DBUserName, logEvent));
                sb.Append(";Password=");
                sb.Append(RenderLogEvent(DBPassword, logEvent));
                sb.Append(";");
            }

            if (DBDatabase != null)
            {
                sb.Append("Database=");
                sb.Append(RenderLogEvent(DBDatabase, logEvent));
            }

            return sb.ToString();
        }

        private void EnsureConnectionOpen(string connectionString)
        {
            if (_activeConnection != null && _activeConnectionString != connectionString)
            {
                InternalLogger.Trace("DatabaseTarget(Name={0}): Close connection because of opening new.", Name);
                CloseConnection();
            }

            if (_activeConnection != null)
            {
                return;
            }

            InternalLogger.Trace("DatabaseTarget(Name={0}): Open connection.", Name);
            _activeConnection = OpenConnection(connectionString);
            _activeConnectionString = connectionString;
        }

        private void CloseConnection()
        {
            if (_activeConnection != null)
            {
                _activeConnection.Close();
                _activeConnection.Dispose();
                _activeConnection = null;
                _activeConnectionString = null;
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
                        cs = RenderLogEvent(commandInfo.ConnectionString, logEvent);
                    }
                    else if (InstallConnectionString != null)
                    {
                        // next, try InstallConnectionString
                        cs = RenderLogEvent(InstallConnectionString, logEvent);
                    }
                    else
                    {
                        // if it's not defined, fall back to regular connection string
                        cs = BuildConnectionString(logEvent);
                    }

                    // Set ConnectionType if it has not been initialized already
                    if (ConnectionType == null)
                    {
                        SetConnectionType();
                    }

                    EnsureConnectionOpen(cs);

                    using (IDbCommand command = _activeConnection.CreateCommand())
                    {
                        command.CommandType = commandInfo.CommandType;
                        command.CommandText = RenderLogEvent(commandInfo.Text, logEvent);

                        AddParametersToCommand(command, commandInfo.Parameters, logEvent);

                        try
                        {
                            installationContext.Trace("DatabaseTarget(Name={0}) - Executing {1} '{2}'", Name, command.CommandType, command.CommandText);
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
            }
            finally
            {
                InternalLogger.Trace("DatabaseTarget(Name={0}): Close connection after install.", Name);

                CloseConnection();
            }
        }

        /// <summary>
        /// Adds the given list of DatabaseParameterInfo to the given IDbCommand after transforming them into IDbDataParameters.
        /// </summary>
        /// <param name="command">The IDbCommand to add parameters to</param>
        /// <param name="databaseParameterInfos">The list of DatabaseParameterInfo to transform into IDbDataParameters and to add to the IDbCommand</param>
        /// <param name="logEvent">The log event to base the parameter's layout rendering on.</param>
        private void AddParametersToCommand(IDbCommand command, IList<DatabaseParameterInfo> databaseParameterInfos, LogEventInfo logEvent)
        {
            for (int i = 0; i < databaseParameterInfos.Count; ++i)
            {
                DatabaseParameterInfo parameterInfo = databaseParameterInfos[i];

                var dbParameter = CreateDatabaseParameter(command, parameterInfo, logEvent);

                var dbParameterValue = GetParameterValue(logEvent, parameterInfo);

                dbParameter.Value = dbParameterValue;

                InternalLogger.Trace("  DatabaseTarget: Parameter: '{0}' = '{1}' ({2})", dbParameter.ParameterName, dbParameter.Value, dbParameter.DbType);
                command.Parameters.Add(dbParameter);
            }
        }

        /// <summary>
        /// Create database parameter
        /// </summary>
        /// <param name="command">Current command.</param>
        /// <param name="parameterInfo">Parameter configuration info.</param>
        /// <param name="logEvent">Current logevent.</param>
        protected virtual IDbDataParameter CreateDatabaseParameter(IDbCommand command, DatabaseParameterInfo parameterInfo, LogEventInfo logEvent)
        {
            IDbDataParameter dbParameter = command.CreateParameter();
            dbParameter.Direction = ParameterDirection.Input;
            if (parameterInfo.Name != null)
            {
                dbParameter.ParameterName = parameterInfo.Name;
            }

            if (parameterInfo.Size != 0)
            {
                dbParameter.Size = parameterInfo.Size;
            }

            if (parameterInfo.Precision != 0)
            {
                dbParameter.Precision = parameterInfo.Precision;
            }

            if (parameterInfo.Scale != 0)
            {
                dbParameter.Scale = parameterInfo.Scale;
            }

            try
            {
                if (!parameterInfo.SetDbType(dbParameter))
                {
                    InternalLogger.Warn("  DatabaseTarget: Parameter: '{0}' - Failed to assign DbType={1}", parameterInfo.Name, parameterInfo.DbType);
                }
            }
            catch (Exception ex)
            {
                if (ex.MustBeRethrownImmediately())
                    throw;

                InternalLogger.Error(ex, "  DatabaseTarget: Parameter: '{0}' - Failed to assign DbType={1}", parameterInfo.Name, parameterInfo.DbType);

                if (ex.MustBeRethrown())
                    throw;
            }

            return dbParameter;
        }

        /// <summary>
        /// Extract parameter value from the logevent
        /// </summary>
        /// <param name="logEvent">Current logevent.</param>
        /// <param name="parameterInfo">Parameter configuration info.</param>
        protected internal virtual object GetParameterValue(LogEventInfo logEvent, DatabaseParameterInfo parameterInfo)
        {
            Type dbParameterType = parameterInfo.ParameterType;
            if (string.IsNullOrEmpty(parameterInfo.Format) && dbParameterType == typeof(string) && !(parameterInfo.UseRawValue ?? false))
            {
                return RenderLogEvent(parameterInfo.Layout, logEvent) ?? string.Empty;
            }

            IFormatProvider dbParameterCulture = GetDbParameterCulture(logEvent, parameterInfo);

            if ((parameterInfo.UseRawValue ?? true) && TryGetConvertedRawValue(logEvent, parameterInfo, dbParameterType, dbParameterCulture, out var value))
            {
                return value;
            }

            try
            {
                InternalLogger.Trace("  DatabaseTarget: Attempt to convert layout value for '{0}' into {1}", parameterInfo.Name, dbParameterType?.Name);
                string layoutValue = RenderLogEvent(parameterInfo.Layout, logEvent);
                return PropertyTypeConverter.Convert(layoutValue, dbParameterType, parameterInfo.Format, dbParameterCulture) ?? DBNull.Value;
            }
            catch (Exception ex)
            {
                if (ex.MustBeRethrownImmediately())
                    throw;

                InternalLogger.Warn(ex, "  DatabaseTarget: Failed to convert layout value for '{0}' into {1}", parameterInfo.Name, dbParameterType?.Name);

                if (ex.MustBeRethrown())
                    throw;

                return CreateDefaultValue(dbParameterType);
            }
        }

        private bool TryGetConvertedRawValue(LogEventInfo logEvent, DatabaseParameterInfo parameterInfo, Type dbParameterType,
            IFormatProvider dbParameterCulture, out object value)
        {
            if (parameterInfo.Layout.TryGetRawValue(logEvent, out var rawValue))
            {
                try
                {
                    InternalLogger.Trace("  DatabaseTarget: Attempt to convert raw value for '{0}' into {1}",
                        parameterInfo.Name, dbParameterType?.Name);
                    if (ReferenceEquals(rawValue, DBNull.Value))
                    {
                        value = rawValue;
                        return true;
                    }

                    {
                        value = PropertyTypeConverter.Convert(rawValue, dbParameterType, parameterInfo.Format,
                                dbParameterCulture) ?? DBNull.Value;
                        return true;
                    }
                }
                catch (Exception ex)
                {
                    if (ex.MustBeRethrownImmediately())
                        throw;

                    InternalLogger.Warn(ex, "  DatabaseTarget: Failed to convert raw value for '{0}' into {1}",
                        parameterInfo.Name, dbParameterType?.Name);

                    if (ex.MustBeRethrown())
                        throw;
                }
            }

            value = null;
            return false;
        }

        /// <summary>
        /// Create Default Value of Type
        /// </summary>
        /// <param name="dbParameterType"></param>
        /// <returns></returns>
        private static object CreateDefaultValue(Type dbParameterType)
        {
            return (dbParameterType?.IsAbstract() == false ? Activator.CreateInstance(dbParameterType) : null) ?? DBNull.Value;
        }

        private IFormatProvider GetDbParameterCulture(LogEventInfo logEvent, DatabaseParameterInfo parameterInfo)
        {
            return parameterInfo.Culture ?? logEvent.FormatProvider ?? LoggingConfiguration?.DefaultCultureInfo;
        }

#if NETSTANDARD1_0
        /// <summary>
        /// Fake transaction
        /// 
        /// Transactions aren't in .NET Core: https://github.com/dotnet/corefx/issues/2949
        /// </summary>
        private class TransactionScope : IDisposable
        {
            private readonly TransactionScopeOption suppress;

            public TransactionScope(TransactionScopeOption suppress)
            {
                this.suppress = suppress;
            }

            public void Complete() { }

            /// <summary>
            ///     Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
            /// </summary>
            public void Dispose()
            {

            }
        }

        /// <summary>
        /// Fake option
        /// </summary>
        private enum TransactionScopeOption
        {
            Required,
            RequiresNew,
            Suppress,
        }
#endif
    }
}

#endif
