// 
// Copyright (c) 2004-2020 Jaroslaw Kowalski <jaak@jkowalski.net>, Kim Christensen, Julian Verdurmen
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

namespace NLog.UnitTests.Targets
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
#if !NETSTANDARD
    using System.Configuration;
#endif
    using System.Data;
    using System.Data.Common;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using NLog.Common;
    using NLog.Config;
    using NLog.Targets;
    using Xunit;
    using Xunit.Extensions;

#if MONO
    using Mono.Data.Sqlite;
    using System.Data.SqlClient;
#elif NETSTANDARD
    using Microsoft.Data.SqlClient;
    using Microsoft.Data.Sqlite;
#else
    using System.Data.SqlClient;
    using System.Data.SQLite;
#endif

    public class DatabaseTargetTests : NLogTestBase
    {
#if !MONO && !NETSTANDARD
        static DatabaseTargetTests()
        {
            var data = (DataSet)ConfigurationManager.GetSection("system.data");
            var providerFactories = data.Tables["DBProviderFactories"];
            providerFactories.Rows.Add("MockDb Provider", "MockDb Provider", "MockDb",
                typeof(MockDbFactory).AssemblyQualifiedName);
            providerFactories.AcceptChanges();
        }
#endif

        [Fact]
        public void SimpleDatabaseTest()
        {
            MockDbConnection.ClearLog();
            DatabaseTarget dt = new DatabaseTarget()
            {
                CommandText = "INSERT INTO FooBar VALUES('${message}')",
                ConnectionString = "FooBar",
                DBProvider = typeof(MockDbConnection).AssemblyQualifiedName,
            };

            dt.Initialize(null);
            Assert.Same(typeof(MockDbConnection), dt.ConnectionType);

            List<Exception> exceptions = new List<Exception>();
            dt.WriteAsyncLogEvent(new LogEventInfo(LogLevel.Info, "MyLogger", "msg1").WithContinuation(exceptions.Add));
            dt.WriteAsyncLogEvent(new LogEventInfo(LogLevel.Info, "MyLogger", "msg2").WithContinuation(exceptions.Add));
            dt.WriteAsyncLogEvent(new LogEventInfo(LogLevel.Info, "MyLogger", "msg3").WithContinuation(exceptions.Add));
            foreach (var ex in exceptions)
            {
                Assert.Null(ex);
            }

            string expectedLog = @"Open('FooBar').
ExecuteNonQuery: INSERT INTO FooBar VALUES('msg1')
Close()
Dispose()
Open('FooBar').
ExecuteNonQuery: INSERT INTO FooBar VALUES('msg2')
Close()
Dispose()
Open('FooBar').
ExecuteNonQuery: INSERT INTO FooBar VALUES('msg3')
Close()
Dispose()
";

            AssertLog(expectedLog);
        }

        [Fact]
        public void SimpleBatchedDatabaseTest()
        {
            MockDbConnection.ClearLog();
            DatabaseTarget dt = new DatabaseTarget()
            {
                CommandText = "INSERT INTO FooBar VALUES('${message}')",
                ConnectionString = "FooBar",
                DBProvider = typeof(MockDbConnection).AssemblyQualifiedName,
            };

            dt.Initialize(null);
            Assert.Same(typeof(MockDbConnection), dt.ConnectionType);

            List<Exception> exceptions = new List<Exception>();
            var events = new[]
            {
                new LogEventInfo(LogLevel.Info, "MyLogger", "msg1").WithContinuation(exceptions.Add),
                new LogEventInfo(LogLevel.Info, "MyLogger", "msg2").WithContinuation(exceptions.Add),
                new LogEventInfo(LogLevel.Info, "MyLogger", "msg3").WithContinuation(exceptions.Add),
            };

            dt.WriteAsyncLogEvents(events);
            foreach (var ex in exceptions)
            {
                Assert.Null(ex);
            }

            string expectedLog = @"Open('FooBar').
ExecuteNonQuery: INSERT INTO FooBar VALUES('msg1')
ExecuteNonQuery: INSERT INTO FooBar VALUES('msg2')
ExecuteNonQuery: INSERT INTO FooBar VALUES('msg3')
Close()
Dispose()
";

            AssertLog(expectedLog);
        }

        [Fact]
        public void KeepConnectionOpenTest()
        {
            MockDbConnection.ClearLog();
            DatabaseTarget dt = new DatabaseTarget()
            {
                CommandText = "INSERT INTO FooBar VALUES('${message}')",
                ConnectionString = "FooBar",
                DBProvider = typeof(MockDbConnection).AssemblyQualifiedName,
                KeepConnection = true,
            };

            dt.Initialize(null);
            Assert.Same(typeof(MockDbConnection), dt.ConnectionType);

            List<Exception> exceptions = new List<Exception>();
            dt.WriteAsyncLogEvent(new LogEventInfo(LogLevel.Info, "MyLogger", "msg1").WithContinuation(exceptions.Add));
            dt.WriteAsyncLogEvent(new LogEventInfo(LogLevel.Info, "MyLogger", "msg2").WithContinuation(exceptions.Add));
            dt.WriteAsyncLogEvent(new LogEventInfo(LogLevel.Info, "MyLogger", "msg3").WithContinuation(exceptions.Add));
            foreach (var ex in exceptions)
            {
                Assert.Null(ex);
            }

            string expectedLog = @"Open('FooBar').
ExecuteNonQuery: INSERT INTO FooBar VALUES('msg1')
ExecuteNonQuery: INSERT INTO FooBar VALUES('msg2')
ExecuteNonQuery: INSERT INTO FooBar VALUES('msg3')
";

            AssertLog(expectedLog);

            MockDbConnection.ClearLog();
            dt.Close();
            expectedLog = @"Close()
Dispose()
";

            AssertLog(expectedLog);
        }

        [Fact]
        public void KeepConnectionOpenBatchedTest()
        {
            MockDbConnection.ClearLog();
            DatabaseTarget dt = new DatabaseTarget()
            {
                CommandText = "INSERT INTO FooBar VALUES('${message}')",
                ConnectionString = "FooBar",
                DBProvider = typeof(MockDbConnection).AssemblyQualifiedName,
                KeepConnection = true,
            };

            dt.Initialize(null);
            Assert.Same(typeof(MockDbConnection), dt.ConnectionType);
            var exceptions = new List<Exception>();

            var events = new[]
            {
                new LogEventInfo(LogLevel.Info, "MyLogger", "msg1").WithContinuation(exceptions.Add),
                new LogEventInfo(LogLevel.Info, "MyLogger", "msg2").WithContinuation(exceptions.Add),
                new LogEventInfo(LogLevel.Info, "MyLogger", "msg3").WithContinuation(exceptions.Add),
            };

            dt.WriteAsyncLogEvents(events);
            foreach (var ex in exceptions)
            {
                Assert.Null(ex);
            }

            string expectedLog = @"Open('FooBar').
ExecuteNonQuery: INSERT INTO FooBar VALUES('msg1')
ExecuteNonQuery: INSERT INTO FooBar VALUES('msg2')
ExecuteNonQuery: INSERT INTO FooBar VALUES('msg3')
";

            AssertLog(expectedLog);

            MockDbConnection.ClearLog();
            dt.Close();
            expectedLog = @"Close()
Dispose()
";

            AssertLog(expectedLog);
        }

        [Fact]
        public void KeepConnectionOpenBatchedIsolationLevelTest()
        {
            MockDbConnection.ClearLog();
            DatabaseTarget dt = new DatabaseTarget()
            {
                CommandText = "INSERT INTO FooBar VALUES('${message}')",
                ConnectionString = "FooBar",
                DBProvider = typeof(MockDbConnection).AssemblyQualifiedName,
                KeepConnection = true,
                IsolationLevel = IsolationLevel.ReadCommitted,
            };

            dt.Initialize(null);
            Assert.Same(typeof(MockDbConnection), dt.ConnectionType);
            var exceptions = new List<Exception>();

            var events = new[]
            {
                new LogEventInfo(LogLevel.Info, "MyLogger", "msg1").WithContinuation(exceptions.Add),
                new LogEventInfo(LogLevel.Info, "MyLogger", "msg2").WithContinuation(exceptions.Add),
                new LogEventInfo(LogLevel.Info, "MyLogger", "msg3").WithContinuation(exceptions.Add),
            };

            dt.WriteAsyncLogEvents(events);
            foreach (var ex in exceptions)
            {
                Assert.Null(ex);
            }

            string expectedLog = @"Open('FooBar').
DbTransaction.Begin(ReadCommitted)
ExecuteNonQuery (DbTransaction=Active): INSERT INTO FooBar VALUES('msg1')
ExecuteNonQuery (DbTransaction=Active): INSERT INTO FooBar VALUES('msg2')
ExecuteNonQuery (DbTransaction=Active): INSERT INTO FooBar VALUES('msg3')
DbTransaction.Commit()
DbTransaction.Dispose()
";

            AssertLog(expectedLog);

            MockDbConnection.ClearLog();
            dt.Close();
            expectedLog = @"Close()
Dispose()
";

            AssertLog(expectedLog);
        }

        [Fact]
        public void KeepConnectionOpenTest2()
        {
            MockDbConnection.ClearLog();
            DatabaseTarget dt = new DatabaseTarget()
            {
                CommandText = "INSERT INTO FooBar VALUES('${message}')",
                ConnectionString = "Database=${logger}",
                DBProvider = typeof(MockDbConnection).AssemblyQualifiedName,
                KeepConnection = true,
            };

            dt.Initialize(null);
            Assert.Same(typeof(MockDbConnection), dt.ConnectionType);

            List<Exception> exceptions = new List<Exception>();
            dt.WriteAsyncLogEvent(new LogEventInfo(LogLevel.Info, "MyLogger", "msg1").WithContinuation(exceptions.Add));
            dt.WriteAsyncLogEvent(new LogEventInfo(LogLevel.Info, "MyLogger", "msg2").WithContinuation(exceptions.Add));
            dt.WriteAsyncLogEvent(new LogEventInfo(LogLevel.Info, "MyLogger2", "msg3").WithContinuation(exceptions.Add));
            dt.WriteAsyncLogEvent(new LogEventInfo(LogLevel.Info, "MyLogger", "msg4").WithContinuation(exceptions.Add));
            foreach (var ex in exceptions)
            {
                Assert.Null(ex);
            }

            string expectedLog = @"Open('Database=MyLogger').
ExecuteNonQuery: INSERT INTO FooBar VALUES('msg1')
ExecuteNonQuery: INSERT INTO FooBar VALUES('msg2')
Close()
Dispose()
Open('Database=MyLogger2').
ExecuteNonQuery: INSERT INTO FooBar VALUES('msg3')
Close()
Dispose()
Open('Database=MyLogger').
ExecuteNonQuery: INSERT INTO FooBar VALUES('msg4')
";

            AssertLog(expectedLog);

            MockDbConnection.ClearLog();
            dt.Close();
            expectedLog = @"Close()
Dispose()
";

            AssertLog(expectedLog);
        }

        [Fact]
        public void KeepConnectionOpenBatchedTest2()
        {
            MockDbConnection.ClearLog();
            DatabaseTarget dt = new DatabaseTarget()
            {
                CommandText = "INSERT INTO FooBar VALUES('${message}')",
                ConnectionString = "Database=${logger}",
                DBProvider = typeof(MockDbConnection).AssemblyQualifiedName,
                KeepConnection = true,
            };

            dt.Initialize(null);

            Assert.Same(typeof(MockDbConnection), dt.ConnectionType);

            // when we pass multiple log events in an array, the target will bucket-sort them by
            // connection string and group all commands for the same connection string together
            // to minimize number of db open/close operations
            // in this case msg1, msg2 and msg4 will be written together to MyLogger database
            // and msg3 will be written to MyLogger2 database

            List<Exception> exceptions = new List<Exception>();
            var events = new[]
            {
                new LogEventInfo(LogLevel.Info, "MyLogger", "msg1").WithContinuation(exceptions.Add),
                new LogEventInfo(LogLevel.Info, "MyLogger", "msg2").WithContinuation(exceptions.Add),
                new LogEventInfo(LogLevel.Info, "MyLogger2", "msg3").WithContinuation(exceptions.Add),
                new LogEventInfo(LogLevel.Info, "MyLogger", "msg4").WithContinuation(exceptions.Add),
            };

            dt.WriteAsyncLogEvents(events);
            foreach (var ex in exceptions)
            {
                Assert.Null(ex);
            }

            string expectedLog = @"Open('Database=MyLogger').
ExecuteNonQuery: INSERT INTO FooBar VALUES('msg1')
ExecuteNonQuery: INSERT INTO FooBar VALUES('msg2')
ExecuteNonQuery: INSERT INTO FooBar VALUES('msg4')
Close()
Dispose()
Open('Database=MyLogger2').
ExecuteNonQuery: INSERT INTO FooBar VALUES('msg3')
";

            AssertLog(expectedLog);

            MockDbConnection.ClearLog();
            dt.Close();
            expectedLog = @"Close()
Dispose()
";

            AssertLog(expectedLog);
        }

        [Fact]
        public void InstallParameterTest()
        {
            MockDbConnection.ClearLog();

            DatabaseCommandInfo installDbCommand = new DatabaseCommandInfo
            {
                Text = $"INSERT INTO dbo.SomeTable(SomeColumn) SELECT @paramOne WHERE NOT EXISTS(SELECT 1 FROM dbo.SomeOtherTable WHERE SomeColumn = @paramOne);"
            };
            installDbCommand.Parameters.Add(new DatabaseParameterInfo("paramOne", "SomeValue"));

            DatabaseTarget dt = new DatabaseTarget()
            {
                DBProvider = typeof(MockDbConnection).AssemblyQualifiedName,
                KeepConnection = true,
                CommandText = "not_important"
            };
            dt.InstallDdlCommands.Add(installDbCommand);

            dt.Initialize(null);

            Assert.Same(typeof(MockDbConnection), dt.ConnectionType);

            dt.Install(new InstallationContext());

            string expectedLog = @"Open('Server=.;Trusted_Connection=SSPI;').
CreateParameter(0)
Parameter #0 Direction=Input
Parameter #0 Name=paramOne
Parameter #0 Value=""SomeValue""
Add Parameter Parameter #0
ExecuteNonQuery: INSERT INTO dbo.SomeTable(SomeColumn) SELECT @paramOne WHERE NOT EXISTS(SELECT 1 FROM dbo.SomeOtherTable WHERE SomeColumn = @paramOne);
Close()
Dispose()
";

            AssertLog(expectedLog);
        }

        [Fact]
        public void ParameterTest()
        {
            MockDbConnection.ClearLog();
            DatabaseTarget dt = new DatabaseTarget()
            {
                CommandText = "INSERT INTO FooBar VALUES(@msg, @lvl, @lg)",
                DBProvider = typeof(MockDbConnection).AssemblyQualifiedName,
                KeepConnection = true,
                Parameters =
                {
                    new DatabaseParameterInfo("msg", "${message}"),
                    new DatabaseParameterInfo("lvl", "${level}"),
                    new DatabaseParameterInfo("lg", "${logger}")

                }
            };

            dt.Initialize(null);

            Assert.Same(typeof(MockDbConnection), dt.ConnectionType);

            // when we pass multiple log events in an array, the target will bucket-sort them by
            // connection string and group all commands for the same connection string together
            // to minimize number of db open/close operations
            // in this case msg1, msg2 and msg4 will be written together to MyLogger database
            // and msg3 will be written to MyLogger2 database

            List<Exception> exceptions = new List<Exception>();
            var events = new[]
            {
                new LogEventInfo(LogLevel.Info, "MyLogger", "msg1").WithContinuation(exceptions.Add),
                new LogEventInfo(LogLevel.Debug, "MyLogger2", "msg3").WithContinuation(exceptions.Add),
            };

            dt.WriteAsyncLogEvents(events);
            foreach (var ex in exceptions)
            {
                Assert.Null(ex);
            }

            string expectedLog = @"Open('Server=.;Trusted_Connection=SSPI;').
CreateParameter(0)
Parameter #0 Direction=Input
Parameter #0 Name=msg
Parameter #0 Value=""msg1""
Add Parameter Parameter #0
CreateParameter(1)
Parameter #1 Direction=Input
Parameter #1 Name=lvl
Parameter #1 Value=""Info""
Add Parameter Parameter #1
CreateParameter(2)
Parameter #2 Direction=Input
Parameter #2 Name=lg
Parameter #2 Value=""MyLogger""
Add Parameter Parameter #2
ExecuteNonQuery: INSERT INTO FooBar VALUES(@msg, @lvl, @lg)
CreateParameter(0)
Parameter #0 Direction=Input
Parameter #0 Name=msg
Parameter #0 Value=""msg3""
Add Parameter Parameter #0
CreateParameter(1)
Parameter #1 Direction=Input
Parameter #1 Name=lvl
Parameter #1 Value=""Debug""
Add Parameter Parameter #1
CreateParameter(2)
Parameter #2 Direction=Input
Parameter #2 Name=lg
Parameter #2 Value=""MyLogger2""
Add Parameter Parameter #2
ExecuteNonQuery: INSERT INTO FooBar VALUES(@msg, @lvl, @lg)
";

            AssertLog(expectedLog);

            MockDbConnection.ClearLog();
            dt.Close();
            expectedLog = @"Close()
Dispose()
";

            AssertLog(expectedLog);
        }

        [Theory]
        [InlineData(null, true, @"""2""")]
        [InlineData(null, false, @"""2""")]
        [InlineData(DbType.Int32, true, "2")]
        [InlineData(DbType.Int32, false, "2")]
        [InlineData(DbType.Object, true, @"""2""")]
        [InlineData(DbType.Object, false, "Info")]
        public void LevelParameterTest(DbType? dbType, bool noRawValue, string expectedValue)
        {
            string lvlLayout = noRawValue ? "${level:format=Ordinal:norawvalue=true}" : "${level:format=Ordinal}";

            MockDbConnection.ClearLog();
            DatabaseTarget dt = new DatabaseTarget()
            {
                CommandText = "INSERT INTO FooBar VALUES(@lvl, @msg)",
                DBProvider = typeof(MockDbConnection).AssemblyQualifiedName,
                KeepConnection = true,
                Parameters =
                {
                    new DatabaseParameterInfo("lvl", lvlLayout) { DbType = dbType?.ToString() },
                    new DatabaseParameterInfo("msg", "${message}")
                }
            };

            dt.Initialize(null);

            Assert.Same(typeof(MockDbConnection), dt.ConnectionType);

            List<Exception> exceptions = new List<Exception>();
            var events = new[]
            {
                new LogEventInfo(LogLevel.Info, "MyLogger", "msg1").WithContinuation(exceptions.Add),
            };

            dt.WriteAsyncLogEvents(events);
            foreach (var ex in exceptions)
            {
                Assert.Null(ex);
            }

            string expectedLog = string.Format(@"Open('Server=.;Trusted_Connection=SSPI;').
CreateParameter(0)
Parameter #0 Direction=Input
Parameter #0 Name=lvl{0}
Parameter #0 Value={1}
Add Parameter Parameter #0
CreateParameter(1)
Parameter #1 Direction=Input
Parameter #1 Name=msg
Parameter #1 Value=""msg1""
Add Parameter Parameter #1
ExecuteNonQuery: INSERT INTO FooBar VALUES(@lvl, @msg)
", dbType.HasValue ? $"\r\nParameter #0 DbType={dbType.Value}" : "", expectedValue);

            AssertLog(expectedLog);

            MockDbConnection.ClearLog();
            dt.Close();
            expectedLog = @"Close()
Dispose()
";

            AssertLog(expectedLog);
        }

        [Theory]
        [InlineData("${counter}", DbType.Int16, (short)1)]
        [InlineData("${counter}", DbType.Int32, 1)]
        [InlineData("${counter}", DbType.Int64, (long)1)]
        [InlineData("${counter:norawvalue=true}", DbType.Int16, (short)1)] //fallback
        [InlineData("${counter}", DbType.VarNumeric, 1, false, true)]
        [InlineData("${counter}", DbType.AnsiString, "1")]
        [InlineData("${level}", DbType.AnsiString, "Debug")]
        [InlineData("${level}", DbType.Int32, 1)]
        [InlineData("${level}", DbType.UInt16, (ushort)1)]
        [InlineData("${event-properties:boolprop}", DbType.Boolean, true)]
        [InlineData("${event-properties:intprop}", DbType.Int32, 123)]
        [InlineData("${event-properties:intprop}", DbType.AnsiString, "123")]
        [InlineData("${event-properties:intprop}", DbType.AnsiStringFixedLength, "123")]
        [InlineData("${event-properties:intprop}", DbType.String, "123")]
        [InlineData("${event-properties:intprop}", DbType.StringFixedLength, "123")]
        [InlineData("${event-properties:almostAsIntProp}", DbType.Int16, (short)124)]
        [InlineData("${event-properties:almostAsIntProp:norawvalue=true}", DbType.Int16, (short)124)]
        [InlineData("${event-properties:almostAsIntProp}", DbType.Int32, 124)]
        [InlineData("${event-properties:almostAsIntProp}", DbType.Int64, (long)124)]
        [InlineData("${event-properties:almostAsIntProp}", DbType.AnsiString, " 124 ")]
        [InlineData("${event-properties:emptyprop}", DbType.AnsiString, "")]
        [InlineData("${event-properties:emptyprop}", DbType.AnsiString, "", true)]
        [InlineData("${event-properties:NullRawValue}", DbType.AnsiString, "")]
        [InlineData("${event-properties:NullRawValue}", DbType.Int32, 0)]
        [InlineData("${event-properties:NullRawValue}", DbType.AnsiString, null, true)]
        [InlineData("${event-properties:NullRawValue}", DbType.Int32, null, true)]
        [InlineData("${event-properties:NullRawValue}", DbType.Guid, null, true)]
        [InlineData("", DbType.AnsiString, null, true)]
        [InlineData("", DbType.Int32, null, true)]
        [InlineData("", DbType.Guid, null, true)]
        public void GetParameterValueTest(string layout, DbType dbtype, object expected, bool allowDbNull = false, bool convertToDecimal = false)
        {
            // Arrange
            var logEventInfo = new LogEventInfo(LogLevel.Debug, "logger1", "message 2");
            logEventInfo.Properties["intprop"] = 123;
            logEventInfo.Properties["boolprop"] = true;
            logEventInfo.Properties["emptyprop"] = "";
            logEventInfo.Properties["almostAsIntProp"] = " 124 ";
            logEventInfo.Properties["dateprop"] = new DateTime(2018, 12, 30, 13, 34, 56);

            var parameterName = "@param1";
            var databaseParameterInfo = new DatabaseParameterInfo
            {
                DbType = dbtype.ToString(),
                Layout = layout,
                Name = parameterName,
                AllowDbNull = allowDbNull,
            };
            databaseParameterInfo.SetDbType(new MockDbConnection().CreateCommand().CreateParameter());

            // Act
            var result = new DatabaseTarget().GetDatabaseParameterValue(logEventInfo, databaseParameterInfo);

            //Assert
            if (convertToDecimal)
            {
                //fix that we can't pass decimals into attributes (InlineData)
                expected = (decimal)(int)expected;
            }

            Assert.Equal(expected ?? DBNull.Value, result);
        }

        [Theory]
        [MemberData(nameof(ConvertFromStringTestCases))]
        public void GetParameterValueFromStringTest(string value, DbType dbType, object expected, string format = null, CultureInfo cultureInfo = null, bool? allowDbNull = null)
        {

            var culture = System.Threading.Thread.CurrentThread.CurrentCulture;

            try
            {
                System.Threading.Thread.CurrentThread.CurrentCulture = new CultureInfo("NL-nl");

                // Arrange
                var databaseParameterInfo = new DatabaseParameterInfo("@test", value)
                {
                    Format = format,
                    DbType = dbType.ToString(),
                    Culture = cultureInfo,
                    AllowDbNull = allowDbNull ?? false,
                };
                databaseParameterInfo.SetDbType(new MockDbConnection().CreateCommand().CreateParameter());

                // Act
                var result = new DatabaseTarget().GetDatabaseParameterValue(LogEventInfo.CreateNullEvent(), databaseParameterInfo);

                // Assert
                Assert.Equal(expected, result);
            }
            finally
            {
                // Restore
                System.Threading.Thread.CurrentThread.CurrentCulture = culture;
            }
        }

        public static IEnumerable<object[]> ConvertFromStringTestCases()
        {
            yield return new object[] { "true", DbType.Boolean, true };
            yield return new object[] { "True", DbType.Boolean, true };
            yield return new object[] { "1,2", DbType.VarNumeric, (decimal)1.2 };
            yield return new object[] { "1,2", DbType.Currency, (decimal)1.2 };
            yield return new object[] { "1,2", DbType.Decimal, (decimal)1.2 };
            yield return new object[] { "1,2", DbType.Double, (double)1.2 };
            yield return new object[] { "1,2", DbType.Single, (Single)1.2 };
            yield return new object[] { "2:30", DbType.Time, new TimeSpan(0, 2, 30, 0), };
            yield return new object[] { "2018-12-23 22:56", DbType.DateTime, new DateTime(2018, 12, 23, 22, 56, 0), };
            yield return new object[] { "2018-12-23 22:56", DbType.DateTime2, new DateTime(2018, 12, 23, 22, 56, 0), };
            yield return new object[] { "23-12-2018 22:56", DbType.DateTime, new DateTime(2018, 12, 23, 22, 56, 0), "dd-MM-yyyy HH:mm" };
            yield return new object[] { new DateTime(2018, 12, 23, 22, 56, 0).ToString(CultureInfo.InvariantCulture), DbType.DateTime, new DateTime(2018, 12, 23, 22, 56, 0), null, CultureInfo.InvariantCulture };
            yield return new object[] { "2018-12-23", DbType.Date, new DateTime(2018, 12, 23, 0, 0, 0), };
            yield return new object[] { "2018-12-23 +2:30", DbType.DateTimeOffset, new DateTimeOffset(2018, 12, 23, 0, 0, 0, new TimeSpan(2, 30, 0)) };
            yield return new object[] { "23-12-2018 22:56 +2:30", DbType.DateTimeOffset, new DateTimeOffset(2018, 12, 23, 22, 56, 0, new TimeSpan(2, 30, 0)), "dd-MM-yyyy HH:mm zzz" };
            yield return new object[] { "3888CCA3-D11D-45C9-89A5-E6B72185D287", DbType.Guid, Guid.Parse("3888CCA3-D11D-45C9-89A5-E6B72185D287") };
            yield return new object[] { "3888CCA3D11D45C989A5E6B72185D287", DbType.Guid, Guid.Parse("3888CCA3-D11D-45C9-89A5-E6B72185D287") };
            yield return new object[] { "3888CCA3D11D45C989A5E6B72185D287", DbType.Guid, Guid.Parse("3888CCA3-D11D-45C9-89A5-E6B72185D287"), "N" };
            yield return new object[] { "3", DbType.Byte, (byte)3 };
            yield return new object[] { "3", DbType.SByte, (sbyte)3 };
            yield return new object[] { "3", DbType.Int16, (short)3 };
            yield return new object[] { " 3 ", DbType.Int16, (short)3 };
            yield return new object[] { "3", DbType.Int32, 3 };
            yield return new object[] { "3", DbType.Int64, (long)3 };
            yield return new object[] { "3", DbType.UInt16, (ushort)3 };
            yield return new object[] { "3", DbType.UInt32, (uint)3 };
            yield return new object[] { "3", DbType.UInt64, (ulong)3 };
            yield return new object[] { "3", DbType.AnsiString, "3" };
            yield return new object[] { "${db-null}", DbType.DateTime, DBNull.Value };
            yield return new object[] { "${event-properties:userid}", DbType.Int32, 0 };
            yield return new object[] { "${date:universalTime=true:format=yyyy-MM:norawvalue=true}", DbType.DateTime, DateTime.SpecifyKind(DateTime.UtcNow.Date.AddDays(-DateTime.UtcNow.Day + 1), DateTimeKind.Unspecified) };
            yield return new object[] { "${shortdate:universalTime=true}", DbType.DateTime, DateTime.UtcNow.Date, null, null, true };
            yield return new object[] { "${shortdate:universalTime=true}", DbType.DateTime, DateTime.UtcNow.Date, null, null, false };
            yield return new object[] { "${shortdate:universalTime=true}", DbType.String, DateTime.UtcNow.Date.ToString("yyyy-MM-dd"), null, null, true };
            yield return new object[] { "${shortdate:universalTime=true}", DbType.String, DateTime.UtcNow.Date.ToString("yyyy-MM-dd"), null, null, false };
        }

        [Fact]
        public void ParameterFacetTest()
        {
            MockDbConnection.ClearLog();
            DatabaseTarget dt = new DatabaseTarget()
            {
                CommandText = "INSERT INTO FooBar VALUES(@msg, @lvl, @lg)",
                DBProvider = typeof(MockDbConnection).AssemblyQualifiedName,
                KeepConnection = true,
                Parameters =
                {
                    new DatabaseParameterInfo("msg", "${message}")
                    {
                        Precision = 3,
                        Scale = 7,
                        Size = 9,
                    },
                    new DatabaseParameterInfo("lvl", "${level}")
                    {
                        Scale = 7
                    },
                    new DatabaseParameterInfo("lg", "${logger}")
                    {
                        Precision = 0
                    },
                }
            };

            dt.Initialize(null);

            Assert.Same(typeof(MockDbConnection), dt.ConnectionType);

            // when we pass multiple log events in an array, the target will bucket-sort them by
            // connection string and group all commands for the same connection string together
            // to minimize number of db open/close operations
            // in this case msg1, msg2 and msg4 will be written together to MyLogger database
            // and msg3 will be written to MyLogger2 database

            var exceptions = new List<Exception>();
            var events = new[]
            {
                new LogEventInfo(LogLevel.Info, "MyLogger", "msg1").WithContinuation(exceptions.Add),
                new LogEventInfo(LogLevel.Debug, "MyLogger2", "msg3").WithContinuation(exceptions.Add),
            };

            dt.WriteAsyncLogEvents(events);
            dt.Close();
            foreach (var ex in exceptions)
            {
                Assert.Null(ex);
            }

            string expectedLog = @"Open('Server=.;Trusted_Connection=SSPI;').
CreateParameter(0)
Parameter #0 Direction=Input
Parameter #0 Name=msg
Parameter #0 Size=9
Parameter #0 Precision=3
Parameter #0 Scale=7
Parameter #0 Value=""msg1""
Add Parameter Parameter #0
CreateParameter(1)
Parameter #1 Direction=Input
Parameter #1 Name=lvl
Parameter #1 Scale=7
Parameter #1 Value=""Info""
Add Parameter Parameter #1
CreateParameter(2)
Parameter #2 Direction=Input
Parameter #2 Name=lg
Parameter #2 Value=""MyLogger""
Add Parameter Parameter #2
ExecuteNonQuery: INSERT INTO FooBar VALUES(@msg, @lvl, @lg)
CreateParameter(0)
Parameter #0 Direction=Input
Parameter #0 Name=msg
Parameter #0 Size=9
Parameter #0 Precision=3
Parameter #0 Scale=7
Parameter #0 Value=""msg3""
Add Parameter Parameter #0
CreateParameter(1)
Parameter #1 Direction=Input
Parameter #1 Name=lvl
Parameter #1 Scale=7
Parameter #1 Value=""Debug""
Add Parameter Parameter #1
CreateParameter(2)
Parameter #2 Direction=Input
Parameter #2 Name=lg
Parameter #2 Value=""MyLogger2""
Add Parameter Parameter #2
ExecuteNonQuery: INSERT INTO FooBar VALUES(@msg, @lvl, @lg)
Close()
Dispose()
";
            AssertLog(expectedLog);
        }

        [Fact]
        public void ParameterDbTypePropertyNameTest()
        {
            MockDbConnection.ClearLog();
            LoggingConfiguration c = XmlLoggingConfiguration.CreateFromXmlString(@"
            <nlog>
                <targets>
                    <target name='dt' type='Database'>
                        <DBProvider>MockDb</DBProvider>
                        <ConnectionString>FooBar</ConnectionString>
                        <CommandText>INSERT INTO FooBar VALUES(@message,@level,@date)</CommandText>
                        <parameter name='@message' layout='${message}'/>
                        <parameter name='@level' dbType=' MockDbType.int32  ' layout='${level:format=Ordinal}'/>
                        <parameter name='@date' dbType='MockDbType.DateTime' format='yyyy-MM-dd HH:mm:ss.fff' layout='${date:format=yyyy-MM-dd HH\:mm\:ss.fff}'/>
                    </target>
                </targets>
            </nlog>");

            DatabaseTarget dt = c.FindTargetByName("dt") as DatabaseTarget;
            Assert.NotNull(dt);
            dt.DBProvider = typeof(MockDbConnection).AssemblyQualifiedName;
            dt.Initialize(c);
            List<Exception> exceptions = new List<Exception>();
            var alogEvent = new LogEventInfo(LogLevel.Info, "MyLogger", "msg1").WithContinuation(exceptions.Add);
            dt.WriteAsyncLogEvent(alogEvent);
            dt.WriteAsyncLogEvent(alogEvent);
            foreach (var ex in exceptions)
            {
                Assert.Null(ex);
            }

            string expectedLog = @"Open('FooBar').
CreateParameter(0)
Parameter #0 Direction=Input
Parameter #0 Name=@message
Parameter #0 Value=""msg1""
Add Parameter Parameter #0
CreateParameter(1)
Parameter #1 Direction=Input
Parameter #1 Name=@level
Parameter #1 MockDbType=Int32
Parameter #1 Value=""{0}""
Add Parameter Parameter #1
CreateParameter(2)
Parameter #2 Direction=Input
Parameter #2 Name=@date
Parameter #2 MockDbType=DateTime
Parameter #2 Value={1}
Add Parameter Parameter #2
ExecuteNonQuery: INSERT INTO FooBar VALUES(@message,@level,@date)
Close()
Dispose()
";
            expectedLog = string.Format(expectedLog + expectedLog, LogLevel.Info.Ordinal, alogEvent.LogEvent.TimeStamp.ToString(CultureInfo.InvariantCulture));
            AssertLog(expectedLog);
        }

        [Fact]
        public void ConnectionStringBuilderTest1()
        {
            DatabaseTarget dt;

            dt = new DatabaseTarget();
            Assert.Equal("Server=.;Trusted_Connection=SSPI;", GetConnectionString(dt));

            dt = new DatabaseTarget();
            dt.DBHost = "${logger}";
            Assert.Equal("Server=Logger1;Trusted_Connection=SSPI;", GetConnectionString(dt));

            dt = new DatabaseTarget();
            dt.DBHost = "HOST1";
            dt.DBDatabase = "${logger}";
            Assert.Equal("Server=HOST1;Trusted_Connection=SSPI;Database=Logger1", GetConnectionString(dt));

            dt = new DatabaseTarget();
            dt.DBHost = "HOST1";
            dt.DBDatabase = "${logger}";
            dt.DBUserName = "user1";
            dt.DBPassword = "password1";
            Assert.Equal("Server=HOST1;User id=user1;Password=password1;Database=Logger1", GetConnectionString(dt));

            dt = new DatabaseTarget();
            dt.ConnectionString = "customConnectionString42";
            dt.DBHost = "HOST1";
            dt.DBDatabase = "${logger}";
            dt.DBUserName = "user1";
            dt.DBPassword = "password1";
            Assert.Equal("customConnectionString42", GetConnectionString(dt));
        }

        [Fact]
        public void DatabaseExceptionTest1()
        {
            MockDbConnection.ClearLog();
            var exceptions = new List<Exception>();

            using (new NoThrowNLogExceptions())
            {
                var db = new DatabaseTarget();
                db.CommandText = "not important";
                db.ConnectionString = "cannotconnect";
                db.DBProvider = typeof(MockDbConnection).AssemblyQualifiedName;
                db.Initialize(null);
                db.WriteAsyncLogEvent(LogEventInfo.CreateNullEvent().WithContinuation(exceptions.Add));
                db.Close();
            }

            Assert.Single(exceptions);
            Assert.NotNull(exceptions[0]);
            Assert.Equal("Cannot open fake database.", exceptions[0].Message);
            Assert.Equal("Open('cannotconnect').\r\n", MockDbConnection.Log);
        }

        [Fact]
        public void DatabaseExceptionTest2()
        {
            MockDbConnection.ClearLog();
            var exceptions = new List<Exception>();

            using (new NoThrowNLogExceptions())
            {
                var db = new DatabaseTarget();
                db.CommandText = "not important";
                db.ConnectionString = "cannotexecute";
                db.KeepConnection = true;
                db.DBProvider = typeof(MockDbConnection).AssemblyQualifiedName;
                db.Initialize(null);
                db.WriteAsyncLogEvent(LogEventInfo.CreateNullEvent().WithContinuation(exceptions.Add));
                db.WriteAsyncLogEvent(LogEventInfo.CreateNullEvent().WithContinuation(exceptions.Add));
                db.WriteAsyncLogEvent(LogEventInfo.CreateNullEvent().WithContinuation(exceptions.Add));
                db.Close();
            }

            Assert.Equal(3, exceptions.Count);
            Assert.NotNull(exceptions[0]);
            Assert.NotNull(exceptions[1]);
            Assert.NotNull(exceptions[2]);
            Assert.Equal("Failure during ExecuteNonQuery", exceptions[0].Message);
            Assert.Equal("Failure during ExecuteNonQuery", exceptions[1].Message);
            Assert.Equal("Failure during ExecuteNonQuery", exceptions[2].Message);

            string expectedLog = @"Open('cannotexecute').
ExecuteNonQuery: not important
Close()
Dispose()
Open('cannotexecute').
ExecuteNonQuery: not important
Close()
Dispose()
Open('cannotexecute').
ExecuteNonQuery: not important
Close()
Dispose()
";
            AssertLog(expectedLog);
        }

        [Fact]
        public void DatabaseBatchExceptionTest()
        {
            MockDbConnection.ClearLog();
            var exceptions = new List<Exception>();

            using (new NoThrowNLogExceptions())
            {
                var db = new DatabaseTarget();
                db.CommandText = "not important";
                db.ConnectionString = "cannotexecute";
                db.KeepConnection = true;
                db.DBProvider = typeof(MockDbConnection).AssemblyQualifiedName;
                db.Initialize(null);
                var events = new[]
                {
                    LogEventInfo.CreateNullEvent().WithContinuation(exceptions.Add),
                    LogEventInfo.CreateNullEvent().WithContinuation(exceptions.Add),
                    LogEventInfo.CreateNullEvent().WithContinuation(exceptions.Add),
                };

                db.WriteAsyncLogEvents(events);
                db.Close();
            }

            Assert.Equal(3, exceptions.Count);
            Assert.NotNull(exceptions[0]);
            Assert.NotNull(exceptions[1]);
            Assert.NotNull(exceptions[2]);
            Assert.Equal("Failure during ExecuteNonQuery", exceptions[0].Message);
            Assert.Equal("Failure during ExecuteNonQuery", exceptions[1].Message);
            Assert.Equal("Failure during ExecuteNonQuery", exceptions[2].Message);

            string expectedLog = @"Open('cannotexecute').
ExecuteNonQuery: not important
Close()
Dispose()
Open('cannotexecute').
ExecuteNonQuery: not important
Close()
Dispose()
Open('cannotexecute').
ExecuteNonQuery: not important
Close()
Dispose()
";
            AssertLog(expectedLog);
        }

        [Fact]
        public void DatabaseBatchIsolationLevelExceptionTest()
        {
            MockDbConnection.ClearLog();
            var exceptions = new List<Exception>();

            using (new NoThrowNLogExceptions())
            {
                var db = new DatabaseTarget();
                db.CommandText = "not important";
                db.ConnectionString = "cannotexecute";
                db.KeepConnection = true;
                db.IsolationLevel = IsolationLevel.Serializable;
                db.DBProvider = typeof(MockDbConnection).AssemblyQualifiedName;
                db.Initialize(null);
                var events = new[]
                {
                    LogEventInfo.CreateNullEvent().WithContinuation(exceptions.Add),
                    LogEventInfo.CreateNullEvent().WithContinuation(exceptions.Add),
                    LogEventInfo.CreateNullEvent().WithContinuation(exceptions.Add),
                };

                db.WriteAsyncLogEvents(events);
                db.Close();
            }

            Assert.Equal(3, exceptions.Count);
            Assert.NotNull(exceptions[0]);
            Assert.NotNull(exceptions[1]);
            Assert.NotNull(exceptions[2]);
            Assert.Equal("Failure during ExecuteNonQuery", exceptions[0].Message);
            Assert.Equal("Failure during ExecuteNonQuery", exceptions[1].Message);
            Assert.Equal("Failure during ExecuteNonQuery", exceptions[2].Message);

            string expectedLog = @"Open('cannotexecute').
DbTransaction.Begin(Serializable)
ExecuteNonQuery (DbTransaction=Active): not important
DbTransaction.Rollback()
DbTransaction.Dispose()
Close()
Dispose()
";
            AssertLog(expectedLog);
        }

        [Fact]
        public void DatabaseExceptionTest3()
        {
            MockDbConnection.ClearLog();
            var exceptions = new List<Exception>();

            using (new NoThrowNLogExceptions())
            {
                var db = new DatabaseTarget();
                db.CommandText = "not important";
                db.ConnectionString = "cannotexecute";
                db.KeepConnection = true;
                db.DBProvider = typeof(MockDbConnection).AssemblyQualifiedName;
                db.Initialize(null);
                db.WriteAsyncLogEvents(
                    LogEventInfo.CreateNullEvent().WithContinuation(exceptions.Add),
                    LogEventInfo.CreateNullEvent().WithContinuation(exceptions.Add),
                    LogEventInfo.CreateNullEvent().WithContinuation(exceptions.Add));
                db.Close();
            }

            Assert.Equal(3, exceptions.Count);
            Assert.NotNull(exceptions[0]);
            Assert.NotNull(exceptions[1]);
            Assert.NotNull(exceptions[2]);
            Assert.Equal("Failure during ExecuteNonQuery", exceptions[0].Message);
            Assert.Equal("Failure during ExecuteNonQuery", exceptions[1].Message);
            Assert.Equal("Failure during ExecuteNonQuery", exceptions[2].Message);

            string expectedLog = @"Open('cannotexecute').
ExecuteNonQuery: not important
Close()
Dispose()
Open('cannotexecute').
ExecuteNonQuery: not important
Close()
Dispose()
Open('cannotexecute').
ExecuteNonQuery: not important
Close()
Dispose()
";
            AssertLog(expectedLog);
        }

#if !MONO && !NETSTANDARD
        [Fact]
        public void ConnectionStringNameInitTest()
        {
            var dt = new DatabaseTarget
            {
                ConnectionStringName = "MyConnectionString",
                CommandText = "notimportant",
            };

            Assert.Same(ConfigurationManager.ConnectionStrings, dt.ConnectionStringsSettings);
            dt.ConnectionStringsSettings = new ConnectionStringSettingsCollection()
            {
                new ConnectionStringSettings("MyConnectionString", "cs1", "MockDb"),
            };

            dt.Initialize(null);
            Assert.Same(MockDbFactory.Instance, dt.ProviderFactory);
            Assert.Equal("cs1", dt.ConnectionString.Render(LogEventInfo.CreateNullEvent()));
        }

        [Fact]
        public void ConnectionStringNameNegativeTest_if_ThrowConfigExceptions()
        {
            LogManager.ThrowConfigExceptions = true;
            var dt = new DatabaseTarget
            {
                ConnectionStringName = "MyConnectionString",
                CommandText = "notimportant",
                ConnectionStringsSettings = new ConnectionStringSettingsCollection(),
            };

            try
            {
                dt.Initialize(null);
                Assert.True(false, "Exception expected.");
            }
            catch (NLogConfigurationException configurationException)
            {
                Assert.Equal(
                    "Connection string 'MyConnectionString' is not declared in <connectionStrings /> section.",
                    configurationException.Message);
            }
        }

        [Fact]
        public void ProviderFactoryInitTest()
        {
            var dt = new DatabaseTarget();
            dt.DBProvider = "MockDb";
            dt.CommandText = "Notimportant";
            dt.Initialize(null);
            Assert.Same(MockDbFactory.Instance, dt.ProviderFactory);
            dt.OpenConnection("myConnectionString", null);
            Assert.Equal(1, MockDbConnection2.OpenCount);
            Assert.Equal("myConnectionString", MockDbConnection2.LastOpenConnectionString);
        }
#endif

        [Fact]
        public void AccessTokenShouldBeSet()
        {
            // Arrange
            var accessToken = "123";
            MockDbConnection.ClearLog();
            var databaseTarget = new DatabaseTarget
            {
                DBProvider = typeof(MockDbConnection).AssemblyQualifiedName,
                CommandText = "command1",
            };
            databaseTarget.ConnectionProperties.Add(new DatabaseObjectPropertyInfo() { Name = "AccessToken", Layout = accessToken });
            databaseTarget.Initialize(new LoggingConfiguration());

            // Act
            var connection1 = databaseTarget.OpenConnection(".", null);
            var connection2 = databaseTarget.OpenConnection(".", null); // Twice because we use compiled method on 2nd attempt

            // Assert
            var sqlConnection1 = Assert.IsType<MockDbConnection>(connection1);
            Assert.Equal(accessToken, sqlConnection1.AccessToken);  // Verify dynamic setter method invoke assigns correctly
            var sqlConnection2 = Assert.IsType<MockDbConnection>(connection2);
            Assert.Equal(accessToken, sqlConnection2.AccessToken);  // Verify compiled method also assigns correctly
        }

        [Fact]
        public void AccessTokenWithInvalidTypeCannotBeSet()
        {
            // Arrange
            MockDbConnection.ClearLog();
            var databaseTarget = new DatabaseTarget
            {
                DBProvider = typeof(MockDbConnection).AssemblyQualifiedName,
                CommandText = "command1",
            };
            databaseTarget.ConnectionProperties.Add(new DatabaseObjectPropertyInfo() { Name = "AccessToken", Layout = "abc", PropertyType = typeof(int) });
            databaseTarget.Initialize(new LoggingConfiguration());

            // Act + Assert
            Assert.Throws<FormatException>(() => databaseTarget.OpenConnection(".", null));
        }

        [Fact]
        public void CommandTimeoutShouldBeSet()
        {
            // Arrange
            var commandTimeout = "123";
            MockDbConnection.ClearLog();
            var databaseTarget = new DatabaseTarget
            {
                DBProvider = typeof(MockDbConnection).AssemblyQualifiedName,
                CommandText = "command1",
            };
            databaseTarget.CommandProperties.Add(new DatabaseObjectPropertyInfo() { Name = "CommandTimeout", Layout = commandTimeout, PropertyType = typeof(int) });
            databaseTarget.Initialize(new LoggingConfiguration());

            // Act
            var connection = databaseTarget.OpenConnection(".", null);
            var command1 = databaseTarget.CreateDbCommand(LogEventInfo.CreateNullEvent(), connection);
            var command2 = databaseTarget.CreateDbCommand(LogEventInfo.CreateNullEvent(), connection);    // Twice because we use compiled method on 2nd attempt

            // Assert
            var sqlCommand1 = Assert.IsType<MockDbCommand>(command1);
            Assert.Equal(commandTimeout, sqlCommand1.CommandTimeout.ToString());  // Verify dynamic setter method invoke assigns correctly
            var sqlCommand2 = Assert.IsType<MockDbCommand>(command2);
            Assert.Equal(commandTimeout, sqlCommand2.CommandTimeout.ToString());  // Verify compiled method also assigns correctly
        }

        [Fact]
        public void SqlServerShorthandNotationTest()
        {
            foreach (string provName in new[] { "microsoft", "msde", "mssql", "sqlserver" })
            {
                var dt = new DatabaseTarget()
                {
                    Name = "myTarget",
                    DBProvider = provName,
                    ConnectionString = "notimportant",
                    CommandText = "notimportant",
                };

                dt.Initialize(null);

                Assert.Equal(typeof(SqlConnection), dt.ConnectionType);
            }
        }

#if !NETSTANDARD
        [Fact]
        public void OleDbShorthandNotationTest()
        {
            var dt = new DatabaseTarget()
            {
                Name = "myTarget",
                DBProvider = "oledb",
                ConnectionString = "notimportant",
                CommandText = "notimportant",
            };

            dt.Initialize(null);
            Assert.Equal(typeof(System.Data.OleDb.OleDbConnection), dt.ConnectionType);
        }

        [Fact]
        public void OdbcShorthandNotationTest()
        {
            var dt = new DatabaseTarget()
            {
                Name = "myTarget",
                DBProvider = "odbc",
                ConnectionString = "notimportant",
                CommandText = "notimportant",
            };

            dt.Initialize(null);
            Assert.Equal(typeof(System.Data.Odbc.OdbcConnection), dt.ConnectionType);
        }
#endif

        [Fact]
        public void SQLite_InstallAndLogMessageProgrammatically()
        {
            SQLiteTest sqlLite = new SQLiteTest("TestLogProgram.sqlite");

            // delete database if it for some reason already exists 
            sqlLite.TryDropDatabase();
            LogManager.ThrowExceptions = true;

            try
            {
                sqlLite.CreateDatabase();

                var connectionString = sqlLite.GetConnectionString();

                DatabaseTarget testTarget = new DatabaseTarget("TestSqliteTarget");
                testTarget.ConnectionString = connectionString;
                testTarget.DBProvider = GetSQLiteDbProvider();

                testTarget.InstallDdlCommands.Add(new DatabaseCommandInfo()
                {
                    CommandType = CommandType.Text,
                    Text = $@"
                    CREATE TABLE NLogTestTable (
                        Id int PRIMARY KEY,
                        Message varchar(100) NULL)"
                });

                using (var context = new InstallationContext())
                {
                    testTarget.Install(context);
                }

                // check so table is created
                var tableName = sqlLite.IssueScalarQuery("SELECT name FROM sqlite_master WHERE type = 'table' AND name = 'NLogTestTable'");
                Assert.Equal("NLogTestTable", tableName);

                testTarget.CommandText = "INSERT INTO NLogTestTable (Message) VALUES (@message)";
                testTarget.Parameters.Add(new DatabaseParameterInfo("@message", new NLog.Layouts.SimpleLayout("${message}")));

                // setup logging
                var config = new LoggingConfiguration();
                config.AddTarget("dbTarget", testTarget);

                var rule = new LoggingRule("*", LogLevel.Debug, testTarget);
                config.LoggingRules.Add(rule);

                // try to log
                LogManager.Configuration = config;

                var logger = LogManager.GetLogger("testLog");
                logger.Debug("Test debug message");
                logger.Error("Test error message");

                // will return long
                var logcount = sqlLite.IssueScalarQuery("SELECT count(1) FROM NLogTestTable");

                Assert.Equal((long)2, logcount);
            }
            finally
            {
                sqlLite.TryDropDatabase();
            }
        }

        private string GetSQLiteDbProvider()
        {
#if MONO
            return "Mono.Data.Sqlite.SqliteConnection, Mono.Data.Sqlite";
#elif NETSTANDARD
            return "Microsoft.Data.Sqlite.SqliteConnection, Microsoft.Data.Sqlite";
#else
            return "System.Data.SQLite.SQLiteConnection, System.Data.SQLite";
#endif
        }

        [Fact]
        public void SQLite_InstallAndLogMessage()
        {
            SQLiteTest sqlLite = new SQLiteTest("TestLogXml.sqlite");

            // delete database just in case
            sqlLite.TryDropDatabase();
            LogManager.ThrowExceptions = true;

            try
            {
                sqlLite.CreateDatabase();

                var connectionString = sqlLite.GetConnectionString();
                string dbProvider = GetSQLiteDbProvider();

                // Create log with xml config
                LogManager.Configuration = XmlLoggingConfiguration.CreateFromXmlString(@"
            <nlog xmlns='http://www.nlog-project.org/schemas/NLog.xsd'
                  xmlns:xsi='http://www.w3.org/2001/XMLSchema-instance' throwExceptions='true'>
                <targets>
                    <target name='database' xsi:type='Database' dbProvider=""" + dbProvider + @""" connectionstring=""" + connectionString + @"""
                        commandText='insert into NLogSqlLiteTest (Message) values (@message);'>
                        <parameter name='@message' layout='${message}' />
<install-command ignoreFailures=""false""
                 text=""CREATE TABLE NLogSqlLiteTest (
    Id int PRIMARY KEY,
    Message varchar(100) NULL
);""/>

                    </target>
                </targets>
                <rules>
                    <logger name='*' writeTo='database' />
                </rules>
            </nlog>");

                //install 
                InstallationContext context = new InstallationContext();
                LogManager.Configuration.Install(context);

                // check so table is created
                var tableName = sqlLite.IssueScalarQuery("SELECT name FROM sqlite_master WHERE type = 'table' AND name = 'NLogSqlLiteTest'");
                Assert.Equal("NLogSqlLiteTest", tableName);

                // start to log
                var logger = LogManager.GetLogger("SQLite");
                logger.Debug("Test");
                logger.Error("Test2");
                logger.Info("Final test row");

                // returns long
                var logcount = sqlLite.IssueScalarQuery("SELECT count(1) FROM NLogSqlLiteTest");
                Assert.Equal((long)3, logcount);
            }
            finally
            {
                sqlLite.TryDropDatabase();
            }
        }

        [Fact]
        public void SQLite_InstallTest()
        {
            SQLiteTest sqlLite = new SQLiteTest("TestInstallXml.sqlite");

            // delete database just in case
            sqlLite.TryDropDatabase();
            LogManager.ThrowExceptions = true;

            try
            {
                sqlLite.CreateDatabase();

                var connectionString = sqlLite.GetConnectionString();
                string dbProvider = GetSQLiteDbProvider();

                // Create log with xml config
                LogManager.Configuration = XmlLoggingConfiguration.CreateFromXmlString(@"
            <nlog xmlns='http://www.nlog-project.org/schemas/NLog.xsd'
                  xmlns:xsi='http://www.w3.org/2001/XMLSchema-instance' throwExceptions='true'>
                <targets>
                    <target name='database' xsi:type='Database' dbProvider=""" + dbProvider + @""" connectionstring=""" + connectionString + @"""
                        commandText='not_important'>
<install-command ignoreFailures=""false""
                 text=""CREATE TABLE NLogSqlLiteTestAppNames (
    Id int PRIMARY KEY,
    Name varchar(100) NULL
);
INSERT INTO NLogSqlLiteTestAppNames(Id, Name) VALUES (1, @appName);"">
<parameter name='@appName' layout='MyApp' />
</install-command>

                    </target>
                </targets>
                <rules>
                    <logger name='*' writeTo='database' />
                </rules>
            </nlog>");

                //install 
                InstallationContext context = new InstallationContext();
                LogManager.Configuration.Install(context);

                // check so table is created
                var tableName = sqlLite.IssueScalarQuery("SELECT name FROM sqlite_master WHERE type = 'table' AND name = 'NLogSqlLiteTestAppNames'");
                Assert.Equal("NLogSqlLiteTestAppNames", tableName);

                // returns long
                var logcount = sqlLite.IssueScalarQuery("SELECT count(*) FROM NLogSqlLiteTestAppNames");
                Assert.Equal((long)1, logcount);

                // check if entry was correct
                var entryValue = sqlLite.IssueScalarQuery("SELECT Name FROM NLogSqlLiteTestAppNames WHERE ID = 1");
                Assert.Equal("MyApp", entryValue);
            }
            finally
            {
                sqlLite.TryDropDatabase();
            }
        }

        [Fact]
        public void SQLite_InstallProgramaticallyTest()
        {
            SQLiteTest sqlLite = new SQLiteTest("TestInstallProgram.sqlite");

            // delete database just in case
            sqlLite.TryDropDatabase();
            LogManager.ThrowExceptions = true;

            try
            {
                sqlLite.CreateDatabase();

                var connectionString = sqlLite.GetConnectionString();
                string dbProvider = GetSQLiteDbProvider();

                DatabaseTarget testTarget = new DatabaseTarget("TestSqliteTargetInstallProgram");
                testTarget.ConnectionString = connectionString;
                testTarget.DBProvider = dbProvider;

                DatabaseCommandInfo installDbCommand = new DatabaseCommandInfo
                {
                    Text = "CREATE TABLE NLogSqlLiteTestAppNames (Id int PRIMARY KEY, Name varchar(100) NULL); " +
                        "INSERT INTO NLogSqlLiteTestAppNames(Id, Name) SELECT 1, @paramOne WHERE NOT EXISTS(SELECT 1 FROM NLogSqlLiteTestAppNames WHERE Name = @paramOne);"
                };
                installDbCommand.Parameters.Add(new DatabaseParameterInfo("@paramOne", "MyApp"));
                testTarget.InstallDdlCommands.Add(installDbCommand);

                //install 
                InstallationContext context = new InstallationContext();
                testTarget.Install(context);

                // check so table is created
                var tableName = sqlLite.IssueScalarQuery("SELECT name FROM sqlite_master WHERE type = 'table' AND name = 'NLogSqlLiteTestAppNames'");
                Assert.Equal("NLogSqlLiteTestAppNames", tableName);

                // returns long
                var logcount = sqlLite.IssueScalarQuery("SELECT count(*) FROM NLogSqlLiteTestAppNames");
                Assert.Equal((long)1, logcount);

                // check if entry was correct
                var entryValue = sqlLite.IssueScalarQuery("SELECT Name FROM NLogSqlLiteTestAppNames WHERE ID = 1");
                Assert.Equal("MyApp", entryValue);
            }
            finally
            {
                sqlLite.TryDropDatabase();
            }
        }

        private void SetupSqliteConfigWithInvalidInstallCommand(string databaseName)
        {
            var nlogXmlConfig = @"
            <nlog xmlns='http://www.nlog-project.org/schemas/NLog.xsd'
                  xmlns:xsi='http://www.w3.org/2001/XMLSchema-instance' throwExceptions='false'>
                <targets>
                    <target name='database' xsi:type='Database' dbProvider='{0}' connectionstring='{1}' 
                        commandText='insert into RethrowingInstallExceptionsTable (Message) values (@message);'>
                        <parameter name='@message' layout='${{message}}' />
                        <install-command text='THIS IS NOT VALID SQL;' />
                    </target>
                </targets>
                <rules>
                    <logger name='*' writeTo='database' />
                </rules>
            </nlog>";

            // Use an in memory SQLite database
            // See https://www.sqlite.org/inmemorydb.html
#if NETSTANDARD
            var connectionString = "Data Source=:memory:";
#else
            var connectionString = "Uri=file::memory:;Version=3";
#endif


            LogManager.Configuration = XmlLoggingConfiguration.CreateFromXmlString(String.Format(nlogXmlConfig, GetSQLiteDbProvider(), connectionString));
        }

        [Fact]
        public void NotRethrowingInstallExceptions()
        {
            using (new NoThrowNLogExceptions())
            {
                SetupSqliteConfigWithInvalidInstallCommand("not_rethrowing_install_exceptions");

                // Default InstallationContext should not rethrow exceptions
                InstallationContext context = new InstallationContext();

                Assert.False(context.IgnoreFailures, "Failures should not be ignored by default");
                Assert.False(context.ThrowExceptions, "Exceptions should not be thrown by default");

                var exRecorded = Record.Exception(() => LogManager.Configuration.Install(context));
                Assert.Null(exRecorded);
            }
        }


        [Fact]
        public void RethrowingInstallExceptions()
        {
            using (new NoThrowNLogExceptions())
            {
                SetupSqliteConfigWithInvalidInstallCommand("rethrowing_install_exceptions");

                InstallationContext context = new InstallationContext()
                {
                    ThrowExceptions = true
                };

                Assert.True(context.ThrowExceptions);  // Sanity check

#if MONO || NETSTANDARD
                Assert.Throws<SqliteException>(() => LogManager.Configuration.Install(context));
#else
                Assert.Throws<SQLiteException>(() => LogManager.Configuration.Install(context));
#endif
            }
        }

        [Fact]
        public void SqlServer_NoTargetInstallException()
        {
            if (IsLinux())
            {
                Console.WriteLine("skipping test SqlServer_NoTargetInstallException because we are running in Travis");
                return;
            }

            bool isAppVeyor = IsAppVeyor();
            SqlServerTest.TryDropDatabase(isAppVeyor);

            try
            {
                SqlServerTest.CreateDatabase(isAppVeyor);

                var connectionString = SqlServerTest.GetConnectionString(isAppVeyor);

                DatabaseTarget testTarget = new DatabaseTarget("TestDbTarget");
                testTarget.ConnectionString = connectionString;

                testTarget.InstallDdlCommands.Add(new DatabaseCommandInfo()
                {
                    CommandType = CommandType.Text,
                    Text = $@"
                    IF EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES where TABLE_SCHEMA = 'dbo' AND TABLE_NAME = 'NLogTestTable')
                        RETURN

                    CREATE TABLE [Dbo].[NLogTestTable] (
                        [ID] [int] IDENTITY(1,1) NOT NULL,
                        [MachineName] [nvarchar](200) NULL)"
                });

                using (var context = new InstallationContext())
                {
                    testTarget.Install(context);
                }

                var tableCatalog = SqlServerTest.IssueScalarQuery(isAppVeyor, @"SELECT TABLE_NAME FROM NLogTest.INFORMATION_SCHEMA.TABLES 
                    WHERE TABLE_TYPE = 'BASE TABLE'
                    AND  TABLE_NAME = 'NLogTestTable'
                ");

                //check if table exists
                Assert.Equal("NLogTestTable", tableCatalog);
            }
            finally
            {
                SqlServerTest.TryDropDatabase(isAppVeyor);
            }
        }

        [Fact]
        public void SqlServer_InstallAndLogMessage()
        {
            if (IsLinux())
            {
                Console.WriteLine("skipping test SqlServer_InstallAndLogMessage because we are running in Travis");
                return;
            }

            bool isAppVeyor = IsAppVeyor();
            SqlServerTest.TryDropDatabase(isAppVeyor);

            try
            {
                SqlServerTest.CreateDatabase(isAppVeyor);

                var connectionString = SqlServerTest.GetConnectionString(IsAppVeyor());
                LogManager.Configuration = XmlLoggingConfiguration.CreateFromXmlString(@"
            <nlog xmlns='http://www.nlog-project.org/schemas/NLog.xsd'
                  xmlns:xsi='http://www.w3.org/2001/XMLSchema-instance' throwExceptions='true'>
                <targets>
                    <target name='database' xsi:type='Database' connectionstring=""" + connectionString + @"""
                        commandText='insert into dbo.NLogSqlServerTest (Uid, LogDate) values (@uid, @logdate);'>
                        <parameter name='@uid' layout='${event-properties:uid}' />
                        <parameter name='@logdate' layout='${date}' />
<install-command ignoreFailures=""false""
                 text=""CREATE TABLE dbo.NLogSqlServerTest (
    Id       int               NOT NULL IDENTITY(1,1) PRIMARY KEY CLUSTERED,
    Uid      uniqueidentifier  NULL,
    LogDate  date              NULL
);""/>

                    </target>
                </targets>
                <rules>
                    <logger name='*' writeTo='database' />
                </rules>
            </nlog>");

                //install 
                InstallationContext context = new InstallationContext();
                LogManager.Configuration.Install(context);

                var tableCatalog = SqlServerTest.IssueScalarQuery(isAppVeyor, @"SELECT TABLE_CATALOG FROM INFORMATION_SCHEMA.TABLES
                 WHERE TABLE_SCHEMA = 'Dbo'
                 AND  TABLE_NAME = 'NLogSqlServerTest'");

                //check if table exists
                Assert.Equal("NLogTest", tableCatalog);

                var logger = LogManager.GetLogger("A");
                var target = LogManager.Configuration.FindTargetByName<DatabaseTarget>("database");

                var uid = new Guid("e7c648b4-3508-4df2-b001-753148659d6d");
                var logEvent = new LogEventInfo(LogLevel.Info, null, null);
                logEvent.Properties["uid"] = uid;
                logger.Log(logEvent);

                var count = SqlServerTest.IssueScalarQuery(isAppVeyor, "SELECT count(1) FROM dbo.NLogSqlServerTest");

                Assert.Equal(1, count);

                var result = SqlServerTest.IssueScalarQuery(isAppVeyor, "SELECT Uid FROM dbo.NLogSqlServerTest");

                Assert.Equal(uid, result);

                var result2 = SqlServerTest.IssueScalarQuery(isAppVeyor, "SELECT LogDate FROM dbo.NLogSqlServerTest");

                Assert.Equal(DateTime.Today, result2);
            }
            finally
            {
                SqlServerTest.TryDropDatabase(isAppVeyor);
            }

        }

#if !NETSTANDARD
        [Fact]
        public void GetProviderNameFromAppConfig()
        {
            LogManager.ThrowExceptions = true;
            var databaseTarget = new DatabaseTarget()
            {
                Name = "myTarget",
                ConnectionStringName = "test_connectionstring_with_providerName",
                CommandText = "notimportant",
            };
            databaseTarget.ConnectionStringsSettings = new ConnectionStringSettingsCollection()
            {
                new ConnectionStringSettings("test_connectionstring_without_providerName", "some connectionstring"),
                new ConnectionStringSettings("test_connectionstring_with_providerName", "some connectionstring",
                    "System.Data.SqlClient"),
            };

            databaseTarget.Initialize(null);
            Assert.NotNull(databaseTarget.ProviderFactory);
            Assert.Equal(typeof(SqlClientFactory), databaseTarget.ProviderFactory.GetType());
        }

        [Fact]
        public void DontRequireProviderNameInAppConfig()
        {
            LogManager.ThrowExceptions = true;
            var databaseTarget = new DatabaseTarget()
            {
                Name = "myTarget",
                ConnectionStringName = "test_connectionstring_without_providerName",
                CommandText = "notimportant",
                DBProvider = "System.Data.SqlClient"
            };

            databaseTarget.ConnectionStringsSettings = new ConnectionStringSettingsCollection()
            {
                new ConnectionStringSettings("test_connectionstring_without_providerName", "some connectionstring"),
                new ConnectionStringSettings("test_connectionstring_with_providerName", "some connectionstring",
                    "System.Data.SqlClient"),
            };

            databaseTarget.Initialize(null);
            Assert.NotNull(databaseTarget.ProviderFactory);
            Assert.Equal(typeof(SqlClientFactory), databaseTarget.ProviderFactory.GetType());
        }

        [Fact]
        public void GetProviderNameFromConnectionString()
        {
            LogManager.ThrowExceptions = true;
            var databaseTarget = new DatabaseTarget()
            {
                Name = "myTarget",
                ConnectionStringName = "test_connectionstring_with_providerName",
                CommandText = "notimportant",
            };

            databaseTarget.ConnectionStringsSettings = new ConnectionStringSettingsCollection()
            {
                new ConnectionStringSettings("test_connectionstring_with_providerName",
                    "metadata=res://*/Model.csdl|res://*/Model.ssdl|res://*/Model.msl;provider=System.Data.SqlClient;provider connection string=\"data source=192.168.0.100;initial catalog=TEST_DB;user id=myUser;password=SecretPassword;multipleactiveresultsets=True;application name=EntityFramework\"",
                    "System.Data.EntityClient"),
            };

            databaseTarget.Initialize(null);
            Assert.NotNull(databaseTarget.ProviderFactory);
            Assert.Equal(typeof(SqlClientFactory), databaseTarget.ProviderFactory.GetType());
            Assert.Equal("data source=192.168.0.100;initial catalog=TEST_DB;user id=myUser;password=SecretPassword;multipleactiveresultsets=True;application name=EntityFramework", ((NLog.Layouts.SimpleLayout)databaseTarget.ConnectionString).FixedText);
        }
#endif

        [Theory]
        [InlineData("localhost", "MyDatabase", "user", "password", "Server=localhost;User id=user;Password=password;Database=MyDatabase")]
        [InlineData("localhost", null, "user", "password", "Server=localhost;User id=user;Password=password;")]
        [InlineData("localhost", "MyDatabase", "user", "'password'", "Server=localhost;User id=user;Password='password';Database=MyDatabase")]
        [InlineData("localhost", "MyDatabase", "user", "\"password\"", "Server=localhost;User id=user;Password=\"password\";Database=MyDatabase")]
        [InlineData("localhost", "MyDatabase", "user", "pa;ssword", "Server=localhost;User id=user;Password='pa;ssword';Database=MyDatabase")]
        [InlineData("localhost", "MyDatabase", "user", "pa'ssword", "Server=localhost;User id=user;Password=\"pa'ssword\";Database=MyDatabase")]
        [InlineData("localhost", "MyDatabase", "user", "pa'\"ssword", "Server=localhost;User id=user;Password=\"pa'\"\"ssword\";Database=MyDatabase")]
        [InlineData("localhost", "MyDatabase", "user", "pa\"ssword", "Server=localhost;User id=user;Password='pa\"ssword';Database=MyDatabase")]
        [InlineData("localhost", "MyDatabase", "user", "", "Server=localhost;User id=user;Password=;Database=MyDatabase")]
        [InlineData("localhost", "MyDatabase", null, "password", "Server=localhost;Trusted_Connection=SSPI;Database=MyDatabase")]
        public void DatabaseConnectionStringTest(string host, string database, string username, string password, string expected)
        {
            // Arrange
            var databaseTarget = new NonLoggingDatabaseTarget()
            {
                CommandText = "DoSomething",
                Name = "myTarget",
                DBHost = host,
                DBDatabase = database,
                DBUserName = username,
                DBPassword = password
            };

            var logEventInfo = LogEventInfo.CreateNullEvent();

            // Act
            var result = databaseTarget.GetRenderedConnectionString(logEventInfo);

            // Assert
            Assert.Equal(expected, result);
        }

        [Theory]
        [InlineData("password", "password")]
        [InlineData("", "")]
        [InlineData("password'", "\"password'\"")]
        public void DatabaseConnectionStringViaVariableTest(string password, string expectedPassword)
        {
            // Arrange
            var databaseTarget = new NonLoggingDatabaseTarget()
            {
                CommandText = "DoSomething",
                Name = "myTarget",
                DBHost = "localhost",
                DBDatabase = "MyDatabase",
                DBUserName = "user",
                DBPassword = "${event-properties:myPassword}"
            };

            var logEventInfo = LogEventInfo.Create(LogLevel.Debug, "logger1", "message1");
            logEventInfo.Properties["myPassword"] = password;

            // Act
            var result = databaseTarget.GetRenderedConnectionString(logEventInfo);

            // Assert
            var expected = $"Server=localhost;User id=user;Password={expectedPassword};Database=MyDatabase";
            Assert.Equal(expected, result);
        }

        private static void AssertLog(string expectedLog)
        {
            Assert.Equal(expectedLog.Replace("\r", ""), MockDbConnection.Log.Replace("\r", ""));
        }

        private string GetConnectionString(DatabaseTarget dt)
        {
            MockDbConnection.ClearLog();
            dt.DBProvider = typeof(MockDbConnection).AssemblyQualifiedName;
            dt.CommandText = "NotImportant";

            var exceptions = new List<Exception>();
            dt.Initialize(null);
            dt.WriteAsyncLogEvent(new LogEventInfo(LogLevel.Info, "Logger1", "msg1").WithContinuation(exceptions.Add));
            dt.Close();

            return MockDbConnection.LastConnectionString;
        }

        public class MockDbConnection : IDbConnection
        {
            public static string Log { get; private set; }
            public static string LastConnectionString { get; private set; }

            public MockDbConnection()
            {
            }

            public MockDbConnection(string connectionString)
            {
                ConnectionString = connectionString;
            }

            public IDbTransaction BeginTransaction(IsolationLevel il)
            {
                AddToLog("DbTransaction.Begin({0})", il);
                return new MockDbTransaction(this, il);
            }

            public IDbTransaction BeginTransaction()
            {
                return BeginTransaction(IsolationLevel.ReadCommitted);
            }

            public void ChangeDatabase(string databaseName)
            {
                throw new NotImplementedException();
            }

            public void Close()
            {
                AddToLog("Close()");
            }

            public string ConnectionString { get; set; }

            public int ConnectionTimeout => throw new NotImplementedException();

            public IDbCommand CreateCommand()
            {
                return new MockDbCommand() { Connection = this };
            }

            public string Database => throw new NotImplementedException();

            public void Open()
            {
                LastConnectionString = ConnectionString;
                AddToLog("Open('{0}').", ConnectionString);
                if (ConnectionString == "cannotconnect")
                {
                    throw new ApplicationException("Cannot open fake database.");
                }
            }

            public ConnectionState State => throw new NotImplementedException();

            public string AccessToken { get; set; }

            public void Dispose()
            {
                AddToLog("Dispose()");
            }

            public static void ClearLog()
            {
                Log = string.Empty;
            }

            public void AddToLog(string message, params object[] args)
            {
                if (args.Length > 0)
                {
                    message = string.Format(CultureInfo.InvariantCulture, message, args);
                }

                Log += message + "\r\n";
            }
        }

        private class NonLoggingDatabaseTarget : DatabaseTarget
        {
            public string GetRenderedConnectionString(LogEventInfo logEventInfo)
            {
                return base.BuildConnectionString(logEventInfo);
            }
        }

        private class MockDbCommand : IDbCommand
        {
            private int paramCount;
            private IDataParameterCollection parameters;

            public MockDbCommand()
            {
                parameters = new MockParameterCollection(this);
            }

            public void Cancel()
            {
                throw new NotImplementedException();
            }

            public string CommandText { get; set; }

            public int CommandTimeout { get; set; }

            public CommandType CommandType { get; set; }

            public IDbConnection Connection { get; set; }

            public IDbTransaction Transaction { get; set; }

            public IDbDataParameter CreateParameter()
            {
                ((MockDbConnection)Connection).AddToLog("CreateParameter({0})", paramCount);
                return new MockDbParameter(this, paramCount++);
            }

            public int ExecuteNonQuery()
            {
                if (Transaction != null)
                    ((MockDbConnection)Connection).AddToLog("ExecuteNonQuery (DbTransaction={0}): {1}", Transaction.Connection != null ? "Active" : "Disposed", CommandText);
                else
                    ((MockDbConnection)Connection).AddToLog("ExecuteNonQuery: {0}", CommandText);
                if (Connection.ConnectionString == "cannotexecute")
                {
                    throw new ApplicationException("Failure during ExecuteNonQuery");
                }

                return 0;
            }

            public IDataReader ExecuteReader(CommandBehavior behavior)
            {
                throw new NotImplementedException();
            }

            public IDataReader ExecuteReader()
            {
                throw new NotImplementedException();
            }

            public object ExecuteScalar()
            {
                throw new NotImplementedException();
            }

            public IDataParameterCollection Parameters => parameters;

            public void Prepare()
            {
                throw new NotImplementedException();
            }

            public UpdateRowSource UpdatedRowSource
            {
                get => throw new NotImplementedException();
                set => throw new NotImplementedException();
            }

            public void Dispose()
            {
                Transaction = null;
                Connection = null;
            }
        }

        private class MockDbParameter : IDbDataParameter
        {
            private readonly MockDbCommand mockDbCommand;
            private readonly int paramId;
            private string parameterName;
            private object parameterValue;
            private DbType parameterType;

            public MockDbParameter(MockDbCommand mockDbCommand, int paramId)
            {
                this.mockDbCommand = mockDbCommand;
                this.paramId = paramId;
            }

            public DbType DbType
            {
                get { return parameterType; }
                set
                {
                    ((MockDbConnection)mockDbCommand.Connection).AddToLog("Parameter #{0} DbType={1}", paramId, value);
                    parameterType = value;
                }
            }
            public DbType MockDbType
            {
                get { return parameterType; }
                set
                {
                    ((MockDbConnection)mockDbCommand.Connection).AddToLog("Parameter #{0} MockDbType={1}", paramId, value);
                    parameterType = value;
                }
            }

            public ParameterDirection Direction
            {
                get => throw new NotImplementedException();
                set => ((MockDbConnection)mockDbCommand.Connection).AddToLog("Parameter #{0} Direction={1}", paramId,
                    value);
            }

            public bool IsNullable => throw new NotImplementedException();

            public string ParameterName
            {
                get => parameterName;
                set
                {
                    ((MockDbConnection)mockDbCommand.Connection).AddToLog("Parameter #{0} Name={1}", paramId, value);
                    parameterName = value;
                }
            }

            public string SourceColumn
            {
                get => throw new NotImplementedException();
                set => throw new NotImplementedException();
            }

            public DataRowVersion SourceVersion
            {
                get => throw new NotImplementedException();
                set => throw new NotImplementedException();
            }

            public object Value
            {
                get => parameterValue;
                set
                {
                    object valueOutput = value is string valueString ? $"\"{valueString}\"" : value;
                    ((MockDbConnection)mockDbCommand.Connection).AddToLog("Parameter #{0} Value={1}", paramId, valueOutput);
                    parameterValue = value;
                }
            }

            public byte Precision
            {
                get => throw new NotImplementedException();
                set => ((MockDbConnection)mockDbCommand.Connection).AddToLog("Parameter #{0} Precision={1}", paramId,
                    value);
            }

            public byte Scale
            {
                get => throw new NotImplementedException();
                set => ((MockDbConnection)mockDbCommand.Connection).AddToLog("Parameter #{0} Scale={1}", paramId, value);
            }

            public int Size
            {
                get => throw new NotImplementedException();
                set => ((MockDbConnection)mockDbCommand.Connection).AddToLog("Parameter #{0} Size={1}", paramId, value);
            }

            public override string ToString()
            {
                return "Parameter #" + paramId;
            }
        }

        private class MockParameterCollection : IDataParameterCollection
        {
            private readonly MockDbCommand command;

            public MockParameterCollection(MockDbCommand command)
            {
                this.command = command;
            }

            public IEnumerator GetEnumerator()
            {
                throw new NotImplementedException();
            }

            public void CopyTo(Array array, int index)
            {
                throw new NotImplementedException();
            }

            public int Count => throw new NotImplementedException();

            public object SyncRoot => throw new NotImplementedException();

            public bool IsSynchronized => throw new NotImplementedException();

            public int Add(object value)
            {
                ((MockDbConnection)command.Connection).AddToLog("Add Parameter {0}", value);
                return 0;
            }

            public bool Contains(object value)
            {
                throw new NotImplementedException();
            }

            public void Clear()
            {
                throw new NotImplementedException();
            }

            public int IndexOf(object value)
            {
                throw new NotImplementedException();
            }

            public void Insert(int index, object value)
            {
                throw new NotImplementedException();
            }

            public void Remove(object value)
            {
                throw new NotImplementedException();
            }

            public void RemoveAt(int index)
            {
                throw new NotImplementedException();
            }

            object IList.this[int index]
            {
                get => throw new NotImplementedException();
                set => throw new NotImplementedException();
            }

            public bool IsReadOnly => throw new NotImplementedException();

            public bool IsFixedSize => throw new NotImplementedException();

            public bool Contains(string parameterName)
            {
                throw new NotImplementedException();
            }

            public int IndexOf(string parameterName)
            {
                throw new NotImplementedException();
            }

            public void RemoveAt(string parameterName)
            {
                throw new NotImplementedException();
            }

            object IDataParameterCollection.this[string parameterName]
            {
                get => throw new NotImplementedException();
                set => throw new NotImplementedException();
            }
        }

        public class MockDbTransaction : IDbTransaction
        {
            public IDbConnection Connection { get; private set; }

            public IsolationLevel IsolationLevel { get; }

            public MockDbTransaction(IDbConnection connection, IsolationLevel isolationLevel)
            {
                Connection = connection;
                IsolationLevel = isolationLevel;
            }

            public void Commit()
            {
                if (Connection == null)
                    throw new NotSupportedException();
                ((MockDbConnection)Connection).AddToLog("DbTransaction.Commit()");
            }

            public void Dispose()
            {
                ((MockDbConnection)Connection).AddToLog("DbTransaction.Dispose()");
                Connection = null;
            }

            public void Rollback()
            {
                if (Connection == null)
                    throw new NotSupportedException();
                ((MockDbConnection)Connection).AddToLog("DbTransaction.Rollback()");
            }
        }

        public class MockDbFactory : DbProviderFactory
        {
            public static readonly MockDbFactory Instance = new MockDbFactory();

            public override DbConnection CreateConnection()
            {
                return new MockDbConnection2();
            }
        }

        public class MockDbConnection2 : DbConnection
        {
            public static int OpenCount { get; private set; }

            public static string LastOpenConnectionString { get; private set; }

            protected override DbTransaction BeginDbTransaction(IsolationLevel isolationLevel)
            {
                throw new NotImplementedException();
            }

            public override void ChangeDatabase(string databaseName)
            {
                throw new NotImplementedException();
            }

            public override void Close()
            {
                throw new NotImplementedException();
            }

            public override string ConnectionString { get; set; }

            protected override DbCommand CreateDbCommand()
            {
                throw new NotImplementedException();
            }

            public override string DataSource => throw new NotImplementedException();

            public override string Database => throw new NotImplementedException();

            public override void Open()
            {
                LastOpenConnectionString = ConnectionString;
                OpenCount++;
            }

            public override string ServerVersion => throw new NotImplementedException();

            public override ConnectionState State => throw new NotImplementedException();
        }

        private class SQLiteTest
        {
            private string dbName = "NLogTest.sqlite";
            private string connectionString;

            public SQLiteTest(string dbName)
            {
                this.dbName = dbName;
#if NETSTANDARD
                connectionString = "Data Source=" + this.dbName;
#else
                connectionString = "Data Source=" + this.dbName + ";Version=3;";
#endif
            }

            public string GetConnectionString()
            {
                return connectionString;
            }

            public void CreateDatabase()
            {
                if (DatabaseExists())
                {
                    TryDropDatabase();
                }

                SQLiteHandler.CreateDatabase(dbName);
            }

            public bool DatabaseExists()
            {
                return File.Exists(dbName);
            }

            public void TryDropDatabase()
            {
                try
                {
                    if (DatabaseExists())
                    {
                        File.Delete(dbName);
                    }
                }
                catch
                {
                }
            }

            public void IssueCommand(string commandString)
            {
                using (DbConnection connection = SQLiteHandler.GetConnection(connectionString))
                {
                    connection.Open();
                    using (DbCommand command = SQLiteHandler.CreateCommand(commandString, connection))
                    {
                        command.ExecuteNonQuery();
                    }
                }
            }

            public object IssueScalarQuery(string commandString)
            {
                using (DbConnection connection = SQLiteHandler.GetConnection(connectionString))
                {
                    connection.Open();
                    using (DbCommand command = SQLiteHandler.CreateCommand(commandString, connection))
                    {
                        var scalar = command.ExecuteScalar();
                        return scalar;
                    }
                }
            }
        }

        private static class SQLiteHandler
        {
            public static void CreateDatabase(string dbName)
            {
#if NETSTANDARD
                // Using ConnectionString Mode=ReadWriteCreate
#elif MONO
                SqliteConnection.CreateFile(dbName);
#else
                SQLiteConnection.CreateFile(dbName);
#endif
            }

            public static DbConnection GetConnection(string connectionString)
            {
#if NETSTANDARD
                return new SqliteConnection(connectionString + ";Mode=ReadWriteCreate;");
#elif MONO
                return new SqliteConnection(connectionString); 
#else
                return new SQLiteConnection(connectionString);
#endif
            }

            public static DbCommand CreateCommand(string commandString, DbConnection connection)
            {
#if MONO || NETSTANDARD
                return new SqliteCommand(commandString, (SqliteConnection)connection);
#else
                return new SQLiteCommand(commandString, (SQLiteConnection)connection);
#endif
            }
        }

        private static class SqlServerTest
        {
            static SqlServerTest()
            {
            }

            public static string GetConnectionString(bool isAppVeyor)
            {
                string connectionString = string.Empty;
#if !NETSTANDARD
                connectionString = ConfigurationManager.AppSettings["SqlServerTestConnectionString"];
#endif
                if (String.IsNullOrWhiteSpace(connectionString))
                {
                    connectionString = isAppVeyor ? AppVeyorConnectionStringNLogTest : LocalConnectionStringNLogTest;
                }
                return connectionString;
            }

            /// <summary>
            /// AppVeyor connectionstring for SQL 2019, see https://www.appveyor.com/docs/services-databases/
            /// </summary>
            private const string AppVeyorConnectionStringMaster =
                @"Server=(local)\SQL2019;Database=master;User ID=sa;Password=Password12!";

            private const string AppVeyorConnectionStringNLogTest =
                @"Server=(local)\SQL2019;Database=NLogTest;User ID=sa;Password=Password12!";

            private const string LocalConnectionStringMaster =
                @"Data Source=(localdb)\MSSQLLocalDB; Database=master; Integrated Security=True;";

            private const string LocalConnectionStringNLogTest =
                @"Data Source=(localdb)\MSSQLLocalDB; Database=NLogTest; Integrated Security=True;";

            public static void CreateDatabase(bool isAppVeyor)
            {
                var connectionString = GetMasterConnectionString(isAppVeyor);
                IssueCommand(IsAppVeyor(), "CREATE DATABASE NLogTest", connectionString);
            }

            public static bool NLogTestDatabaseExists(bool isAppVeyor)
            {
                var connectionString = GetMasterConnectionString(isAppVeyor);
                var dbId = IssueScalarQuery(IsAppVeyor(), "select db_id('NLogTest')", connectionString);
                return dbId != null && dbId != DBNull.Value;

            }

            private static string GetMasterConnectionString(bool isAppVeyor)
            {
                return isAppVeyor ? AppVeyorConnectionStringMaster : LocalConnectionStringMaster;
            }

            public static void IssueCommand(bool isAppVeyor, string commandString, string connectionString = null)
            {
                using (var connection = new SqlConnection(connectionString ?? GetConnectionString(isAppVeyor)))
                {
                    connection.Open();
                    if (connectionString == null)
                        connection.ChangeDatabase("NLogTest");
                    using (var command = new SqlCommand(commandString, connection))
                    {
                        command.ExecuteNonQuery();
                    }
                }
            }

            public static object IssueScalarQuery(bool isAppVeyor, string commandString, string connectionString = null)
            {
                using (var connection = new SqlConnection(connectionString ?? GetConnectionString(isAppVeyor)))
                {
                    connection.Open();
                    if (connectionString == null)
                        connection.ChangeDatabase("NLogTest");
                    using (var command = new SqlCommand(commandString, connection))
                    {
                        var scalar = command.ExecuteScalar();
                        return scalar;
                    }
                }
            }

            /// <summary>
            /// Try dropping. IF fail, not exception
            /// </summary>
            public static bool TryDropDatabase(bool isAppVeyor)
            {
                try
                {
                    if (NLogTestDatabaseExists(isAppVeyor))
                    {
                        var connectionString = GetMasterConnectionString(isAppVeyor);
                        IssueCommand(isAppVeyor,
                            "ALTER DATABASE [NLogTest] SET SINGLE_USER WITH ROLLBACK IMMEDIATE; DROP DATABASE NLogTest;",
                            connectionString);
                        return true;
                    }
                    return false;

                }
                catch (Exception)
                {

                    //ignore
                    return false;
                }

            }
        }
    }

}
