//
// Copyright (c) 2004-2024 Jaroslaw Kowalski <jaak@jkowalski.net>, Kim Christensen, Julian Verdurmen
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

namespace NLog.Targets
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;

    using System.Data;
    using System.Data.Common;
    using System.Reflection;
    using System.Text;
#if !NETSTANDARD1_3 && !NETSTANDARD1_5
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
    /// <para>
    /// Note .NET Core application cannot load connectionstrings from app.config / web.config. Instead use ${configsetting}
    /// </para>
    /// <a href="https://github.com/nlog/nlog/wiki/Database-target">See NLog Wiki</a>
    /// </remarks>
    /// <seealso href="https://github.com/nlog/nlog/wiki/Database-target">Documentation on NLog Wiki</seealso>
    /// <example>
    /// <para>
    /// The configuration is dependent on the database type, because
    /// there are different methods of specifying connection string, SQL
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
            DBProvider = "sqlserver";
            DBHost = ".";
#if !NETSTANDARD
            ConnectionStringsSettings = ConfigurationManager.ConnectionStrings;
#endif
            CommandType = CommandType.Text;
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
        /// <docgen category='Connection Options' order='50' />
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
        /// <docgen category='Installation Options' order='100' />
        public Layout InstallConnectionString { get; set; }

        /// <summary>
        /// Gets the installation DDL commands.
        /// </summary>
        /// <docgen category='Installation Options' order='100' />
        [ArrayParameter(typeof(DatabaseCommandInfo), "install-command")]
        public IList<DatabaseCommandInfo> InstallDdlCommands { get; } = new List<DatabaseCommandInfo>();

        /// <summary>
        /// Gets the uninstallation DDL commands.
        /// </summary>
        /// <docgen category='Installation Options' order='100' />
        [ArrayParameter(typeof(DatabaseCommandInfo), "uninstall-command")]
        public IList<DatabaseCommandInfo> UninstallDdlCommands { get; } = new List<DatabaseCommandInfo>();

        /// <summary>
        /// Gets or sets a value indicating whether to keep the
        /// database connection open between the log events.
        /// </summary>
        /// <docgen category='Connection Options' order='10' />
        [DefaultValue(false)]
        public bool KeepConnection { get; set; }

        /// <summary>
        /// Gets or sets the database host name. If the ConnectionString is not provided
        /// this value will be used to construct the "Server=" part of the
        /// connection string.
        /// </summary>
        /// <docgen category='Connection Options' order='50' />
        public Layout DBHost { get; set; }

        /// <summary>
        /// Gets or sets the database user name. If the ConnectionString is not provided
        /// this value will be used to construct the "User ID=" part of the
        /// connection string.
        /// </summary>
        /// <docgen category='Connection Options' order='50' />
        public Layout DBUserName { get; set; }

        /// <summary>
        /// Gets or sets the database password. If the ConnectionString is not provided
        /// this value will be used to construct the "Password=" part of the
        /// connection string.
        /// </summary>
        /// <docgen category='Connection Options' order='50' />
        public Layout DBPassword
        {
            get => _dbPassword;
            set
            {
                _dbPassword = value;
                if (value is SimpleLayout simpleLayout && simpleLayout.IsFixedText)
                    _dbPasswordFixed = EscapeValueForConnectionString(simpleLayout.OriginalText);
                else
                    _dbPasswordFixed = null;
            }
        }
        private Layout _dbPassword;
        private string _dbPasswordFixed;

        /// <summary>
        /// Gets or sets the database name. If the ConnectionString is not provided
        /// this value will be used to construct the "Database=" part of the
        /// connection string.
        /// </summary>
        /// <docgen category='Connection Options' order='50' />
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
        /// Gets the collection of parameters. Each item contains a mapping
        /// between NLog layout and a database named or positional parameter.
        /// </summary>
        /// <docgen category='SQL Statement' order='14' />
        [ArrayParameter(typeof(DatabaseParameterInfo), "parameter")]
        public IList<DatabaseParameterInfo> Parameters { get; } = new List<DatabaseParameterInfo>();

        /// <summary>
        /// Gets the collection of properties. Each item contains a mapping
        /// between NLog layout and a property on the DbConnection instance
        /// </summary>
        /// <docgen category='Connection Options' order='50' />
        [ArrayParameter(typeof(DatabaseObjectPropertyInfo), "connectionproperty")]
        public IList<DatabaseObjectPropertyInfo> ConnectionProperties { get; } = new List<DatabaseObjectPropertyInfo>();

        /// <summary>
        /// Gets the collection of properties. Each item contains a mapping
        /// between NLog layout and a property on the DbCommand instance
        /// </summary>
        /// <docgen category='Connection Options' order='50' />
        [ArrayParameter(typeof(DatabaseObjectPropertyInfo), "commandproperty")]
        public IList<DatabaseObjectPropertyInfo> CommandProperties { get; } = new List<DatabaseObjectPropertyInfo>();

        /// <summary>
        /// Configures isolated transaction batch writing. If supported by the database, then it will improve insert performance.
        /// </summary>
        /// <docgen category='Performance Tuning Options' order='10' />
        public System.Data.IsolationLevel? IsolationLevel { get; set; }

#if !NETSTANDARD
        internal DbProviderFactory ProviderFactory { get; set; }

        // this is so we can mock the connection string without creating sub-processes
        internal ConnectionStringSettingsCollection ConnectionStringsSettings { get; set; }
#endif

        internal Type ConnectionType { get; private set; }

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

        internal IDbConnection OpenConnection(string connectionString, LogEventInfo logEventInfo)
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

            if (connection is null)
            {
                throw new NLogRuntimeException("Creation of connection failed");
            }

            connection.ConnectionString = connectionString;
            if (ConnectionProperties?.Count > 0)
            {
                ApplyDatabaseObjectProperties(connection, ConnectionProperties, logEventInfo ?? LogEventInfo.CreateNullEvent());
            }

            connection.Open();
            return connection;
        }

        private void ApplyDatabaseObjectProperties(object databaseObject, IList<DatabaseObjectPropertyInfo> objectProperties, LogEventInfo logEventInfo)
        {
            for (int i = 0; i < objectProperties.Count; ++i)
            {
                var propertyInfo = objectProperties[i];
                try
                {
                    var propertyValue = propertyInfo.RenderValue(logEventInfo);
                    if (!propertyInfo.SetPropertyValue(databaseObject, propertyValue))
                    {
                        InternalLogger.Warn("{0}: Failed to lookup property {1} on {2}", this, propertyInfo.Name, databaseObject.GetType());
                    }
                }
                catch (Exception ex)
                {
                    InternalLogger.Error(ex, "{0}: Failed to assign value for property {1} on {2}", this, propertyInfo.Name, databaseObject.GetType());
                    if (LogManager.ThrowExceptions)
                        throw;
                }
            }
        }

        /// <inheritdoc/>
        protected override void InitializeTarget()
        {
            base.InitializeTarget();

            bool foundProvider = false;
            string providerName = string.Empty;

#if !NETSTANDARD
            if (!string.IsNullOrEmpty(ConnectionStringName))
            {
                // read connection string and provider factory from the configuration file
                var cs = ConnectionStringsSettings[ConnectionStringName];
                if (cs is null)
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
                providerName = InitConnectionString(providerName);
            }

#if !NETSTANDARD
            if (string.IsNullOrEmpty(providerName))
            {
                providerName = GetProviderNameFromDbProviderFactories(providerName);
            }

            if (!string.IsNullOrEmpty(providerName))
            {
                foundProvider = InitProviderFactory(providerName);
            }
#endif

            if (!foundProvider)
            {
                try
                {
                    SetConnectionType();
                    if (ConnectionType is null)
                    {
                        InternalLogger.Warn("{0}: No ConnectionType created from DBProvider={1}", this, DBProvider);
                    }
                }
                catch (Exception ex)
                {
                    InternalLogger.Error(ex, "{0}: Failed to create ConnectionType from DBProvider={1}", this, DBProvider);
                    throw;
                }
            }
        }

        private string InitConnectionString(string providerName)
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
                    InternalLogger.Warn(ex, "{0}: DbConnectionStringBuilder failed to parse '{1}' ConnectionString", this, ConnectionStringName);
                else
#endif
                    InternalLogger.Warn(ex, "{0}: DbConnectionStringBuilder failed to parse ConnectionString", this);
            }

            return providerName;
        }

#if !NETSTANDARD
        private bool InitProviderFactory(string providerName)
        {
            bool foundProvider;
            try
            {
                ProviderFactory = DbProviderFactories.GetFactory(providerName);
                foundProvider = true;
            }
            catch (Exception ex)
            {
                InternalLogger.Error(ex, "{0}: DbProviderFactories failed to get factory from ProviderName={1}", this, providerName);
                throw;
            }

            return foundProvider;
        }

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
#if NETSTANDARD
                    {
                        try
                        {
                            var assembly = Assembly.Load(new AssemblyName("Microsoft.Data.SqlClient"));
                            ConnectionType = assembly.GetType("Microsoft.Data.SqlClient.SqlConnection", true, true);
                        }
                        catch (Exception ex)
                        {
                            InternalLogger.Warn(ex, "{0}: Failed to load assembly 'Microsoft.Data.SqlClient'. Falling back to 'System.Data.SqlClient'.", this);
                            var assembly = Assembly.Load(new AssemblyName("System.Data.SqlClient"));
                            ConnectionType = assembly.GetType("System.Data.SqlClient.SqlConnection", true, true);
                        }
                        break;
                    }
                case "SYSTEM.DATA.SQLCLIENT":
                    {
                        var assembly = Assembly.Load(new AssemblyName("System.Data.SqlClient"));
                        ConnectionType = assembly.GetType("System.Data.SqlClient.SqlConnection", true, true);
                        break;
                    }
#else
                case "SYSTEM.DATA.SQLCLIENT":
                    {
                        var assembly = typeof(IDbConnection).GetAssembly();
                        ConnectionType = assembly.GetType("System.Data.SqlClient.SqlConnection", true, true);
                        break;
                    }
#endif
                case "MICROSOFT.DATA.SQLCLIENT":
                    {
                        var assembly = Assembly.Load(new AssemblyName("Microsoft.Data.SqlClient"));
                        ConnectionType = assembly.GetType("Microsoft.Data.SqlClient.SqlConnection", true, true);
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

        /// <inheritdoc/>
        protected override void CloseTarget()
        {
            base.CloseTarget();
            InternalLogger.Trace("{0}: Close connection because of CloseTarget", this);
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
                WriteLogEventSuppressTransactionScope(logEvent, BuildConnectionString(logEvent));
            }
            finally
            {
                if (!KeepConnection)
                {
                    InternalLogger.Trace("{0}: Close connection (KeepConnection = false).", this);
                    CloseConnection();
                }
            }
        }

        /// <summary>
        /// Writes an array of logging events to the log target. By default it iterates on all
        /// events and passes them to "Write" method. Inheriting classes can use this method to
        /// optimize batch writes.
        /// </summary>
        /// <param name="logEvents">Logging events to be written out.</param>
        protected override void Write(IList<AsyncLogEventInfo> logEvents)
        {
            var buckets = GroupInBuckets(logEvents);
            if (buckets.Count == 1)
            {
                var firstBucket = System.Linq.Enumerable.First(buckets);
                WriteLogEventsToDatabase(firstBucket.Value, firstBucket.Key);
            }
            else
            {
                foreach (var bucket in buckets)
                    WriteLogEventsToDatabase(bucket.Value, bucket.Key);
            }
        }

        private ICollection<KeyValuePair<string, IList<AsyncLogEventInfo>>> GroupInBuckets(IList<AsyncLogEventInfo> logEvents)
        {
            if (logEvents.Count == 0)
            {
#if !NET35 && !NET45
                return Array.Empty<KeyValuePair<string, IList<AsyncLogEventInfo>>>();
#else
                return new KeyValuePair<string, IList<AsyncLogEventInfo>>[0];
#endif
            }

            string firstConnectionString = BuildConnectionString(logEvents[0].LogEvent);
            IDictionary<string, IList<AsyncLogEventInfo>> dictionary = null;
            for (int i = 1; i < logEvents.Count; ++i)
            {
                var connectionString = BuildConnectionString(logEvents[i].LogEvent);
                if (dictionary is null)
                {
                    if (connectionString == firstConnectionString)
                        continue;

                    var firstBucket = new List<AsyncLogEventInfo>(i);
                    for (int j = 0; j < i; ++j)
                        firstBucket.Add(logEvents[j]);

                    dictionary = new Dictionary<string, IList<AsyncLogEventInfo>>(StringComparer.Ordinal) { { firstConnectionString, firstBucket } };
                }

                if (!dictionary.TryGetValue(connectionString, out var bucket))
                    bucket = dictionary[connectionString] = new List<AsyncLogEventInfo>();

                bucket.Add(logEvents[i]);
            }

            return (ICollection<KeyValuePair<string, IList<AsyncLogEventInfo>>>)dictionary ?? new KeyValuePair<string, IList<AsyncLogEventInfo>>[] { new KeyValuePair<string, IList<AsyncLogEventInfo>>(firstConnectionString, logEvents) };
        }

        private void WriteLogEventsToDatabase(IList<AsyncLogEventInfo> logEvents, string connectionString)
        {
            try
            {
                if (IsolationLevel.HasValue && logEvents.Count > 1)
                {
                    try
                    {
                        WriteLogEventsWithTransactionScope(logEvents, connectionString);
                        for (int i = 0; i < logEvents.Count; i++)
                        {
                            logEvents[i].Continuation(null);
                        }
                    }
                    catch (Exception ex)
                    {
                        if (LogManager.ThrowExceptions)
                            throw;

                        for (int i = 0; i < logEvents.Count; i++)
                        {
                            logEvents[i].Continuation(ex);
                        }
                    }
                }
                else
                {
                    WriteLogEventsSuppressTransactionScope(logEvents, connectionString);
                }
            }
            finally
            {
                if (!KeepConnection)
                {
                    InternalLogger.Trace("{0}: Close connection because of KeepConnection=false", this);
                    CloseConnection();
                }
            }
        }

        private void WriteLogEventsSuppressTransactionScope(IList<AsyncLogEventInfo> logEvents, string connectionString)
        {
            for (int i = 0; i < logEvents.Count; i++)
            {
                try
                {
                    WriteLogEventSuppressTransactionScope(logEvents[i].LogEvent, connectionString);
                    logEvents[i].Continuation(null);
                }
                catch (Exception exception)
                {
                    if (LogManager.ThrowExceptions)
                        throw;

                    logEvents[i].Continuation(exception);
                }
            }
        }

        private void WriteLogEventsWithTransactionScope(IList<AsyncLogEventInfo> logEvents, string connectionString)
        {
            try
            {
                //Always suppress transaction so that the caller does not rollback logging if they are rolling back their transaction.
                using (TransactionScope transactionScope = new TransactionScope(TransactionScopeOption.Suppress))
                {
                    EnsureConnectionOpen(connectionString, logEvents.Count > 0 ? logEvents[0].LogEvent : null);

                    var dbTransaction = _activeConnection.BeginTransaction(IsolationLevel.Value);
                    try
                    {
                        for (int i = 0; i < logEvents.Count; ++i)
                        {
                            ExecuteDbCommandWithParameters(logEvents[i].LogEvent, _activeConnection, dbTransaction);
                        }

                        dbTransaction?.Commit();

                        dbTransaction?.Dispose();   // Can throw error on dispose, so no using

                        transactionScope.Complete();    //not really needed as there is no transaction at all.
                    }
                    catch
                    {
                        try
                        {
                            if (dbTransaction?.Connection != null)
                            {
                                dbTransaction.Rollback();
                            }
                            dbTransaction?.Dispose();
                        }
                        catch (Exception exception)
                        {
                            InternalLogger.Error(exception, "{0}: Error during rollback of batch writing {1} logevents to database.", this, logEvents.Count);
                            if (LogManager.ThrowExceptions)
                                throw;
                        }

                        throw;
                    }
                }
            }
            catch (Exception exception)
            {
                InternalLogger.Error(exception, "{0}: Error when batch writing {1} logevents to database.", this, logEvents.Count);
                if (LogManager.ThrowExceptions && !(exception is DbException))
                    throw;

                InternalLogger.Trace("{0}: Close connection because of error", this);
                CloseConnection();
                throw;
            }
        }

        private void WriteLogEventSuppressTransactionScope(LogEventInfo logEvent, string connectionString)
        {
            try
            {
                //Always suppress transaction so that the caller does not rollback logging if they are rolling back their transaction.
                using (TransactionScope transactionScope = new TransactionScope(TransactionScopeOption.Suppress))
                {
                    EnsureConnectionOpen(connectionString, logEvent);

                    ExecuteDbCommandWithParameters(logEvent, _activeConnection, null);

                    transactionScope.Complete();    //not really needed as there is no transaction at all.
                }
            }
            catch (Exception exception)
            {
                InternalLogger.Error(exception, "{0}: Error when writing to database.", this);
                if (LogManager.ThrowExceptions && !(exception is DbException))
                    throw;

                InternalLogger.Trace("{0}: Close connection because of error", this);
                CloseConnection();
                throw;
            }
        }

        /// <summary>
        /// Write logEvent to database
        /// </summary>
        private void ExecuteDbCommandWithParameters(LogEventInfo logEvent, IDbConnection dbConnection, IDbTransaction dbTransaction)
        {
            using (IDbCommand command = CreateDbCommand(logEvent, dbConnection))
            {
                if (dbTransaction != null)
                    command.Transaction = dbTransaction;

                int result = command.ExecuteNonQuery();
                InternalLogger.Trace("{0}: Finished execution, result = {1}", this, result);
            }
        }

        internal IDbCommand CreateDbCommand(LogEventInfo logEvent, IDbConnection dbConnection)
        {
            var commandText = RenderLogEvent(CommandText, logEvent);
            InternalLogger.Trace("{0}: Executing {1}: {2}", this, CommandType, commandText);
            return CreateDbCommandWithParameters(logEvent, dbConnection, CommandType, commandText, Parameters);
        }

        private IDbCommand CreateDbCommandWithParameters(LogEventInfo logEvent, IDbConnection dbConnection, CommandType commandType, string dbCommandText, IList<DatabaseParameterInfo> databaseParameterInfos)
        {
            var dbCommand = dbConnection.CreateCommand();
            dbCommand.CommandType = commandType;
            if (CommandProperties?.Count > 0)
            {
                ApplyDatabaseObjectProperties(dbCommand, CommandProperties, logEvent);
            }
            dbCommand.CommandText = dbCommandText;

            for (int i = 0; i < databaseParameterInfos.Count; ++i)
            {
                var parameterInfo = databaseParameterInfos[i];
                var dbParameter = CreateDatabaseParameter(dbCommand, parameterInfo);
                var dbParameterValue = GetDatabaseParameterValue(logEvent, parameterInfo);
                dbParameter.Value = dbParameterValue;
                dbCommand.Parameters.Add(dbParameter);
                InternalLogger.Trace("  DatabaseTarget: Parameter: '{0}' = '{1}' ({2})", dbParameter.ParameterName, dbParameter.Value, dbParameter.DbType);
            }

            return dbCommand;
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
            var dbUserName = RenderLogEvent(DBUserName, logEvent);
            if (string.IsNullOrEmpty(dbUserName))
            {
                sb.Append("Trusted_Connection=SSPI;");
            }
            else
            {
                sb.Append("User id=");
                sb.Append(dbUserName);
                sb.Append(";Password=");
                var password = _dbPasswordFixed ?? EscapeValueForConnectionString(_dbPassword.Render(logEvent));
                sb.Append(password);
                sb.Append(";");
            }
            var dbDatabase = RenderLogEvent(DBDatabase, logEvent);
            if (!string.IsNullOrEmpty(dbDatabase))
            {
                sb.Append("Database=");
                sb.Append(dbDatabase);
            }

            return sb.ToString();
        }

        /// <summary>
        /// Escape quotes and semicolons.
        /// See https://docs.microsoft.com/en-us/previous-versions/windows/desktop/ms722656(v=vs.85)#setting-values-that-use-reserved-characters
        /// </summary>
        private static string EscapeValueForConnectionString(string value)
        {
            if (string.IsNullOrEmpty(value))
                return value;

            const char singleQuote = '\'';
            if (value.IndexOf(singleQuote) == 0 && (value.LastIndexOf(singleQuote) == value.Length - 1))
            {
                // already escaped
                return value;
            }

            const char doubleQuote = '\"';
            if (value.IndexOf(doubleQuote) == 0 && (value.LastIndexOf(doubleQuote) == value.Length - 1))
            {
                // already escaped
                return value;
            }

            var containsSingle = value.IndexOf(singleQuote) >= 0;
            var containsDouble = value.IndexOf(doubleQuote) >= 0;
            if (containsSingle || containsDouble || value.IndexOf(';') >= 0)
            {
                if (!containsSingle)
                {
                    return singleQuote + value + singleQuote;
                }
                if (!containsDouble)
                {
                    return doubleQuote + value + doubleQuote;
                }

                // both single and double
                var escapedValue = value.Replace("\"", "\"\"");
                return doubleQuote + escapedValue + doubleQuote;
            }

            return value;
        }

        private void EnsureConnectionOpen(string connectionString, LogEventInfo logEventInfo)
        {
            if (_activeConnection != null && _activeConnectionString != connectionString)
            {
                InternalLogger.Trace("{0}: Close connection because of opening new.", this);
                CloseConnection();
            }

            if (_activeConnection != null)
            {
                return;
            }

            InternalLogger.Trace("{0}: Open connection.", this);
            _activeConnection = OpenConnection(connectionString, logEventInfo);
            _activeConnectionString = connectionString;
        }

        private void CloseConnection()
        {
            try
            {
                var activeConnection = _activeConnection;
                if (activeConnection != null)
                {
                    activeConnection.Close();
                    activeConnection.Dispose();
                }
            }
            catch (Exception ex)
            {
                InternalLogger.Warn(ex, "{0}: Error while closing connection.", this);
            }
            finally
            {
                _activeConnectionString = null;
                _activeConnection = null;
            }
        }

        private void RunInstallCommands(InstallationContext installationContext, IEnumerable<DatabaseCommandInfo> commands)
        {
            // create log event that will be used to render all layouts
            LogEventInfo logEvent = installationContext.CreateLogEvent();

            try
            {
                foreach (var commandInfo in commands)
                {
                    var connectionString = GetConnectionStringFromCommand(commandInfo, logEvent);

                    // Set ConnectionType if it has not been initialized already
                    if (ConnectionType is null)
                    {
                        SetConnectionType();
                    }

                    EnsureConnectionOpen(connectionString, logEvent);

                    string commandText = RenderLogEvent(commandInfo.Text, logEvent);

                    installationContext.Trace("DatabaseTarget(Name={0}) - Executing {1} '{2}'", Name, commandInfo.CommandType, commandText);

                    using (IDbCommand command = CreateDbCommandWithParameters(logEvent, _activeConnection, commandInfo.CommandType, commandText, commandInfo.Parameters))
                    {
                        try
                        {
                            command.ExecuteNonQuery();
                        }
                        catch (Exception exception)
                        {
                            if (LogManager.ThrowExceptions)
                                throw;

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
                InternalLogger.Trace("{0}: Close connection after install.", this);
                CloseConnection();
            }
        }

        private string GetConnectionStringFromCommand(DatabaseCommandInfo commandInfo, LogEventInfo logEvent)
        {
            string connectionString;
            if (commandInfo.ConnectionString != null)
            {
                // if there is connection string specified on the command info, use it
                connectionString = RenderLogEvent(commandInfo.ConnectionString, logEvent);
            }
            else if (InstallConnectionString != null)
            {
                // next, try InstallConnectionString
                connectionString = RenderLogEvent(InstallConnectionString, logEvent);
            }
            else
            {
                // if it's not defined, fall back to regular connection string
                connectionString = BuildConnectionString(logEvent);
            }

            return connectionString;
        }

        /// <summary>
        /// Create database parameter
        /// </summary>
        /// <param name="command">Current command.</param>
        /// <param name="parameterInfo">Parameter configuration info.</param>
        protected virtual IDbDataParameter CreateDatabaseParameter(IDbCommand command, DatabaseParameterInfo parameterInfo)
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
                InternalLogger.Error(ex, "  DatabaseTarget: Parameter: '{0}' - Failed to assign DbType={1}", parameterInfo.Name, parameterInfo.DbType);

                if (LogManager.ThrowExceptions)
                    throw;
            }

            return dbParameter;
        }

        /// <summary>
        /// Extract parameter value from the logevent
        /// </summary>
        /// <param name="logEvent">Current logevent.</param>
        /// <param name="parameterInfo">Parameter configuration info.</param>
        protected internal virtual object GetDatabaseParameterValue(LogEventInfo logEvent, DatabaseParameterInfo parameterInfo)
        {
            var value = parameterInfo.RenderValue(logEvent);
            if (parameterInfo.AllowDbNull && string.Empty.Equals(value))
                return DBNull.Value;
            else
                return value;
        }

#if NETSTANDARD1_3 || NETSTANDARD1_5
        /// <summary>
        /// Fake transaction
        ///
        /// Transactions aren't in .NET Core: https://github.com/dotnet/corefx/issues/2949
        /// </summary>
        private sealed class TransactionScope : IDisposable
        {
            private readonly TransactionScopeOption _suppress;

            public TransactionScope(TransactionScopeOption suppress)
            {
                _suppress = suppress;
            }

            public void Complete()
            {
                // Fake scope for compatibility, so nothing to do
            }

            public void Dispose()
            {
                // Fake scope for compatibility, so nothing to do
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
