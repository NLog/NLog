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

#if !SILVERLIGHT && !__IOS__ && !__ANDROID__ && !NETSTANDARD1_0

using System;
using System.Collections.Generic;
using System.ComponentModel;
#if !NETSTANDARD
using System.Configuration;
#endif
using System.Data;
using System.Data.Common;
using System.Reflection;
using System.Text;
using System.Transactions;
using NLog.Common;
using NLog.Config;
using NLog.Internal;
using NLog.Layouts;

namespace NLog.Targets
{
    /// <summary>
    /// Brother of <see cref="DatabaseTarget"/> with support for bulk-insert using <see cref="DbDataAdapter"/> and <see cref="DataTable"/>
    /// </summary>
    [Target("DataTable")]
    public sealed class DataTableTarget : Target
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DataTableTarget" /> class.
        /// </summary>
        public DataTableTarget()
        {
            Parameters = new List<DatabaseParameterInfo>();
            OptimizeBufferReuse = true;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DataTableTarget" /> class.
        /// </summary>
        /// <param name="name">Name of the target.</param>
        public DataTableTarget(string name) : this()
        {
            Name = name;
        }

        /// <summary>
        /// Gets the collection of parameters. Each parameter contains a mapping
        /// between NLog layout and a database named or positional parameter.
        /// </summary>
        /// <docgen category='SQL Statement' order='14' />
        [ArrayParameter(typeof(DatabaseParameterInfo), "parameter")]
        public IList<DatabaseParameterInfo> Parameters { get; } = new List<DatabaseParameterInfo>();

        /// <summary>
        /// TypeName of the specific factory. Ex: MySql.Data.MySqlClient.MySqlClientFactory, MySql.Data
        /// </summary>
        /// <remarks>
        /// Remember to add the relevant nuget-package to your project. Ex. System.Data.SqlClient for NetCore
        /// </remarks>
        [RequiredParameter]
        [DefaultValue("sqlserver")]
        public string DbProviderFactory { get; set; } = "sqlserver";

#if !NETSTANDARD
        /// <summary>
        /// Gets or sets the name of the connection string (as specified in <see href="http://msdn.microsoft.com/en-us/library/bf7sd233.aspx">&lt;connectionStrings&gt; configuration section</see>.
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
        public CommandType CommandType { get; set; } = CommandType.Text;

        /// <summary>
        /// Creates a prepared (or compiled) version of the CommandText as a temporary stored procedure (can improves performance on batching)
        /// </summary>
        /// <remarks>
        /// Before you call Prepare, specify the DbType of each parameter in the statement to be prepared.
        /// For each parameter that has a variable length data type, you must set the Size property to the maximum size needed
        /// </remarks>
        public bool PrepareDbCommand { get; set; }

        /// <summary>
        /// Gets or sets a value that enables or disables batch processing support (0 = No Limit)
        /// </summary>
        [DefaultValue(-1)]
        public int UpdateBatchSize { get; set; } = -1;

        internal DbProviderFactory ProviderFactory { get; set; }

#if !NETSTANDARD
        // this is so we can mock the connection string without creating sub-processes
        internal ConnectionStringSettingsCollection ConnectionStringsSettings { get; set; }
#endif

        private IPropertyTypeConverter PropertyTypeConverter
        {
            get => _propertyTypeConverter ?? (_propertyTypeConverter = ConfigurationItemFactory.Default.PropertyTypeConverter);
            set => _propertyTypeConverter = value;
        }
        private IPropertyTypeConverter _propertyTypeConverter;

        private struct ConnectionPartitionKey : IEquatable<ConnectionPartitionKey>
        {
            public readonly string ConnectionString;
            public readonly string CommandText;

            public ConnectionPartitionKey(string connectionString, string commandText)
            {
                ConnectionString = connectionString ?? string.Empty;
                CommandText = commandText ?? string.Empty;
            }

            public override int GetHashCode()
            {
                return ConnectionString.GetHashCode() ^ CommandText.GetHashCode();
            }

            public bool Equals(ConnectionPartitionKey other)
            {
                return ConnectionString == other.ConnectionString && CommandText == other.CommandText;
            }

            public override bool Equals(object obj)
            {
                return obj is ConnectionPartitionKey key && Equals(key);
            }
        }

        SortHelpers.KeySelector<AsyncLogEventInfo, ConnectionPartitionKey> _connectionPartitionKeyDelegate;

        private DbConnection _activeConnection;
        private string _activeConnectionString;
        private readonly Dictionary<ConnectionPartitionKey, DbCommand> _preparedDbCommands = new Dictionary<ConnectionPartitionKey, DbCommand>();
        private DataTable _activeDataTable;

        internal DbConnection OpenConnection(string connectionString)
        {
            DbConnection connection = ProviderFactory?.CreateConnection();
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
                        InternalLogger.Warn(ex, "DataTableTarget(Name={0}): DbConnectionStringBuilder failed to parse '{1}' ConnectionString", Name, ConnectionStringName);
                    else
#endif
                        InternalLogger.Warn(ex, "DataTableTarget(Name={0}): DbConnectionStringBuilder failed to parse ConnectionString", Name);
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
                    InternalLogger.Error(ex, "DataTableTarget(Name={0}): DbProviderFactories failed to get factory from ProviderName={1}", Name, providerName);
                    throw;
                }
            }
#endif

            if (!foundProvider)
            {
                try
                {
                    SetProviderFactory();
                    if (ProviderFactory == null)
                    {
                        InternalLogger.Warn("DataTableTarget(Name={0}): No ProviderFactory created from DbProviderFactory={1}", Name, DbProviderFactory);
                    }
                }
                catch (Exception ex)
                {
                    InternalLogger.Error(ex, "DataTableTarget(Name={0}): Failed to create ProviderFactory from DbProviderFactory={1}", Name, DbProviderFactory);
                    throw;
                }
            }
        }

#if !NETSTANDARD
        private string GetProviderNameFromDbProviderFactories(string providerName)
        {
            string dbProvider = DbProviderFactory?.Trim() ?? string.Empty;
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

        private void SetProviderFactory()
        {
            Type dbFactoryType;
            switch (DbProviderFactory.ToUpperInvariant())
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
                        dbFactoryType = assembly.GetType("System.Data.SqlClient.SqlClientFactory", true, true);
                        break;
                    }
#if !NETSTANDARD
                case "OLEDB":
                    {
                        var assembly = typeof(IDbConnection).GetAssembly();
                        dbFactoryType = assembly.GetType("System.Data.OleDb.OleDbFactory", true, true);
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
                        dbFactoryType = assembly.GetType("System.Data.Odbc.OdbcFactory", true, true);
                        break;
                    }
                default:
                    dbFactoryType = Type.GetType(DbProviderFactory, true, true);
                    break;
            }

            var dbFactoryInstance = dbFactoryType?.GetField("Instance", BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public);
            if (dbFactoryInstance != null)
            {
                ProviderFactory = (DbProviderFactory)dbFactoryInstance.GetValue(null);
            }
        }

        /// <summary>
        /// Closes the target and releases any unmanaged resources.
        /// </summary>
        protected override void CloseTarget()
        {
            PropertyTypeConverter = null;
            base.CloseTarget();
            InternalLogger.Trace("DataTableTarget(Name={0}): Close connection because of CloseTarget", Name);
            CloseConnection();
        }

        private DbDataAdapter CreateDatabaseAdapter(string connectionString, CommandType commandType, string dbCommandText, IList<DatabaseParameterInfo> databaseParameterInfos)
        {
            EnsureConnectionOpen(connectionString);

            if (!_preparedDbCommands.TryGetValue(new ConnectionPartitionKey(connectionString, dbCommandText), out var insertCommand))
            {
                insertCommand = _activeConnection.CreateCommand();
                insertCommand.CommandType = commandType;
                insertCommand.CommandText = dbCommandText;
                insertCommand.UpdatedRowSource = UpdateRowSource.None;

                var dataTable = new DataTable();
                dataTable.BeginInit();

                for (int i = 0; i < databaseParameterInfos.Count; ++i)
                {
                    var parameterInfo = databaseParameterInfos[i];
                    var dbParameter = CreateDatabaseParameter(insertCommand, parameterInfo);
                    var sourceColumnName = string.Concat(((char)('A' + i)).ToString(), "_", (i + 1).ToString());
                    dbParameter.SourceColumn = sourceColumnName;
                    insertCommand.Parameters.Add(dbParameter);
                    dataTable.Columns.Add(sourceColumnName, parameterInfo.ParameterType);
                    InternalLogger.Trace("  DataTableTarget: Parameter: '{0}' = '{1}' ({2})", dbParameter.ParameterName, dbParameter.DbType, sourceColumnName);
                }

                dataTable.EndInit();

                _activeDataTable = dataTable;

                if (PrepareDbCommand)
                {
                    insertCommand.Prepare();
                }

                _preparedDbCommands[new ConnectionPartitionKey(connectionString, dbCommandText)] = insertCommand;
            }

            var dbDataAdapter = ProviderFactory.CreateDataAdapter();
            dbDataAdapter.InsertCommand = insertCommand;
            if (UpdateBatchSize > 0)
            {
                dbDataAdapter.UpdateBatchSize = UpdateBatchSize;
            }
            return dbDataAdapter;
        }

        /// <summary>
        /// Create database parameter
        /// </summary>
        /// <param name="command">Current command.</param>
        /// <param name="parameterInfo">Parameter configuration info.</param>
        private IDbDataParameter CreateDatabaseParameter(IDbCommand command, DatabaseParameterInfo parameterInfo)
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
                    InternalLogger.Warn("  DataTableTarget: Parameter: '{0}' - Failed to assign DbType={1}", parameterInfo.Name, parameterInfo.DbType);
                }
            }
            catch (Exception ex)
            {
                if (ex.MustBeRethrownImmediately())
                    throw;

                InternalLogger.Error(ex, "  DataTableTarget: Parameter: '{0}' - Failed to assign DbType={1}", parameterInfo.Name, parameterInfo.DbType);

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
        private object CreateDatabaseParameterValue(LogEventInfo logEvent, DatabaseParameterInfo parameterInfo)
        {
            Type dbParameterType = parameterInfo.ParameterType;
            if (string.IsNullOrEmpty(parameterInfo.Format) && dbParameterType == typeof(string) && !(parameterInfo.UseRawValue ?? false))
            {
                return RenderLogEvent(parameterInfo.Layout, logEvent) ?? string.Empty;
            }

            IFormatProvider dbParameterCulture = GetDbParameterCulture(logEvent, parameterInfo);

            if ((parameterInfo.UseRawValue ?? true) && TryGetConvertedRawValue(logEvent, parameterInfo, dbParameterType, dbParameterCulture, out var value))
            {
                return value ?? CreateDefaultValue(dbParameterType);
            }

            try
            {
                InternalLogger.Trace("  DataTableTarget: Attempt to convert layout value for '{0}' into {1}", parameterInfo.Name, dbParameterType?.Name);
                string parameterValue = RenderLogEvent(parameterInfo.Layout, logEvent);
                if (string.IsNullOrEmpty(parameterValue))
                {
                    return CreateDefaultValue(dbParameterType);
                }
                return PropertyTypeConverter.Convert(parameterValue, dbParameterType, parameterInfo.Format, dbParameterCulture) ?? DBNull.Value;
            }
            catch (Exception ex)
            {
                if (ex.MustBeRethrownImmediately())
                    throw;

                InternalLogger.Warn(ex, "  DataTableTarget: Failed to convert layout value for '{0}' into {1}", parameterInfo.Name, dbParameterType?.Name);

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
                    InternalLogger.Trace("  DataTableTarget: Attempt to convert raw value for '{0}' into {1}",
                        parameterInfo.Name, dbParameterType?.Name);
                    if (ReferenceEquals(rawValue, DBNull.Value))
                    {
                        value = rawValue;
                        return true;
                    }

                    value = PropertyTypeConverter.Convert(rawValue, dbParameterType, parameterInfo.Format,
                            dbParameterCulture);
                    return true;
                }
                catch (Exception ex)
                {
                    if (ex.MustBeRethrownImmediately())
                        throw;

                    InternalLogger.Warn(ex, "  DataTableTarget: Failed to convert raw value for '{0}' into {1}",
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
            if (dbParameterType == typeof(string))
                return string.Empty;
            else if (dbParameterType.IsValueType())
                return Activator.CreateInstance(dbParameterType);
            else
                return DBNull.Value;
        }

        private IFormatProvider GetDbParameterCulture(LogEventInfo logEvent, DatabaseParameterInfo parameterInfo)
        {
            return parameterInfo.Culture ?? logEvent.FormatProvider ?? LoggingConfiguration?.DefaultCultureInfo;
        }

        private void WriteEventsToDatabase(IList<AsyncLogEventInfo> logEvents, string connectionString, CommandType commandType, string commandText, IList<DatabaseParameterInfo> parameters)
        {
            InternalLogger.Trace("DataTableTarget(Name={0}): {1} rows executing: {2}", Name, logEvents.Count, commandText);

            using (var databaseAdapter = CreateDatabaseAdapter(connectionString, commandType, commandText, parameters))
            {
                var insertTable = _activeDataTable.Clone();
                insertTable.BeginLoadData();
                object[] rowValues = new object[parameters.Count];
                for (int i = 0; i < logEvents.Count; ++i)
                {
                    for (int columnIdx = 0; columnIdx < rowValues.Length; ++columnIdx)
                    {
                        var dbParameter = parameters[columnIdx];
                        rowValues[columnIdx] = CreateDatabaseParameterValue(logEvents[i].LogEvent, dbParameter);
                        InternalLogger.Trace("  DataTableTarget: Row:{0} Parameter: '{1}' = '{2}'", i, dbParameter.Name, rowValues[columnIdx]);
                    }
                    insertTable.LoadDataRow(rowValues, false);
                }
                insertTable.EndLoadData();

                int result = databaseAdapter.Update(insertTable);
                InternalLogger.Trace("DataTableTarget(Name={0}): Finished execution, result = {1}", Name, result);
            }
        }

        /// <inheritdoc />
        protected override void Write(AsyncLogEventInfo logEvent)
        {
            Write((IList<AsyncLogEventInfo>)new[] { logEvent });
        }

        /// <inheritdoc />
        protected override void Write(IList<AsyncLogEventInfo> logEvents)
        {
            if (_connectionPartitionKeyDelegate == null)
                _connectionPartitionKeyDelegate = (l) => new ConnectionPartitionKey(BuildConnectionString(l.LogEvent), RenderLogEvent(CommandText, l.LogEvent));

            var buckets = logEvents.BucketSort(_connectionPartitionKeyDelegate);

            try
            {
                foreach (var kvp in buckets)
                {
                    try
                    {
                        //Always suppress transaction so that the caller does not rollback logging if they are rolling back their transaction.
                        using (TransactionScope transactionScope = new TransactionScope(TransactionScopeOption.Suppress))
                        {
                            WriteEventsToDatabase(kvp.Value, kvp.Key.ConnectionString, CommandType, kvp.Key.CommandText, Parameters);
                            for (int i = 0; i < kvp.Value.Count; i++)
                            {
                                kvp.Value[i].Continuation(null);
                            }
                            transactionScope.Complete();    //not really needed as there is no transaction at all.
                        }
                    }
                    catch (Exception exception)
                    {
                        // in case of exception, close the connection and report it
                        InternalLogger.Error(exception, "DataTableTarget(Name={0}): Error when writing to database.", Name);

                        if (exception.MustBeRethrownImmediately())
                        {
                            throw;
                        }

                        for (int i = 0; i < kvp.Value.Count; i++)
                        {
                            kvp.Value[i].Continuation(exception);
                        }

                        InternalLogger.Trace("DataTableTarget(Name={0}): Close connection because of exception", Name);
                        CloseConnection();

                        if (exception.MustBeRethrown())
                        {
                            throw;
                        }
                    }
                }
            }
            finally
            {
                if (!KeepConnection)
                {
                    InternalLogger.Trace("DataTableTarget(Name={0}): Close connection because of KeepConnection=false", Name);
                    CloseConnection();
                }
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
        private string BuildConnectionString(LogEventInfo logEvent)
        {
            if (ConnectionString != null)
            {
                return RenderLogEvent(ConnectionString, logEvent);
            }

            using (var targetBuilder = OptimizeBufferReuse ? ReusableLayoutBuilder.Allocate() : ReusableLayoutBuilder.None)
            {
                var sb = targetBuilder.Result ?? new StringBuilder();
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
        }

        private void EnsureConnectionOpen(string connectionString)
        {
            if (_activeConnection != null && _activeConnectionString != connectionString)
            {
                InternalLogger.Trace("DataTableTarget(Name={0}): Close connection because of opening new.", Name);
                CloseConnection();
            }

            if (_activeConnection != null)
            {
                return;
            }

            InternalLogger.Trace("DataTableTarget(Name={0}): Open connection.", Name);
            _activeConnection = OpenConnection(connectionString);
            _activeConnectionString = connectionString;
        }

        private void CloseConnection()
        {
            _preparedDbCommands.Clear();
            _activeConnectionString = null;

            if (_activeConnection != null)
            {
                _activeDataTable = null;
                _activeConnection.Close();
                _activeConnection.Dispose();
                _activeConnection = null;
            }
        }
    }
}

#endif