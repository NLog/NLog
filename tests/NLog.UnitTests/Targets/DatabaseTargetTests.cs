// 
// Copyright (c) 2004-2018 Jaroslaw Kowalski <jaak@jkowalski.net>, Kim Christensen, Julian Verdurmen
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
    using System.Data.SqlClient;

#if MONO 
    using Mono.Data.Sqlite;
#elif NETSTANDARD
    using Microsoft.Data.Sqlite;
#else
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
Parameter #0 Value=SomeValue
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
Parameter #0 Value=msg1
Add Parameter Parameter #0
CreateParameter(1)
Parameter #1 Direction=Input
Parameter #1 Name=lvl
Parameter #1 Value=Info
Add Parameter Parameter #1
CreateParameter(2)
Parameter #2 Direction=Input
Parameter #2 Name=lg
Parameter #2 Value=MyLogger
Add Parameter Parameter #2
ExecuteNonQuery: INSERT INTO FooBar VALUES(@msg, @lvl, @lg)
CreateParameter(0)
Parameter #0 Direction=Input
Parameter #0 Name=msg
Parameter #0 Value=msg3
Add Parameter Parameter #0
CreateParameter(1)
Parameter #1 Direction=Input
Parameter #1 Name=lvl
Parameter #1 Value=Debug
Add Parameter Parameter #1
CreateParameter(2)
Parameter #2 Direction=Input
Parameter #2 Name=lg
Parameter #2 Value=MyLogger2
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

        [Fact]
        public void LevelParameterTest()
        {
            MockDbConnection.ClearLog();
            DatabaseTarget dt = new DatabaseTarget()
            {
                CommandText = "INSERT INTO FooBar VALUES(@lvl, @msg)",
                DBProvider = typeof(MockDbConnection).AssemblyQualifiedName,
                KeepConnection = true,
                Parameters =
                {
                    new DatabaseParameterInfo("lvl", "${level:format=Ordinal}"),
                    new DatabaseParameterInfo("msg", "${message}")
                }
            };

            dt.Initialize(null);

            Assert.Same(typeof(MockDbConnection), dt.ConnectionType);

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
Parameter #0 Name=lvl
Parameter #0 Value=2
Add Parameter Parameter #0
CreateParameter(1)
Parameter #1 Direction=Input
Parameter #1 Name=msg
Parameter #1 Value=msg1
Add Parameter Parameter #1
ExecuteNonQuery: INSERT INTO FooBar VALUES(@lvl, @msg)
CreateParameter(0)
Parameter #0 Direction=Input
Parameter #0 Name=lvl
Parameter #0 Value=1
Add Parameter Parameter #0
CreateParameter(1)
Parameter #1 Direction=Input
Parameter #1 Name=msg
Parameter #1 Value=msg3
Add Parameter Parameter #1
ExecuteNonQuery: INSERT INTO FooBar VALUES(@lvl, @msg)
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
        public void ParameterFacetTest()
        {
            MockDbConnection.ClearLog();
            DatabaseTarget dt = new DatabaseTarget()
            {
                CommandText = "INSERT INTO FooBar VALUES(@msg, @lvl, @lg, @date)",
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
                    new DatabaseParameterInfo("date", "${date:universalTime=true:format=yyyy-MM-dd}")
                    {
                        DbType = DbType.Time
                    },
                }
            };

            dt.Initialize(null);

            Assert.Same(typeof(MockDbConnection), dt.ConnectionType);

            string expectedDate = DateTime.UtcNow.ToString("yyyy-MM-dd");

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
Parameter #0 Value=msg1
Add Parameter Parameter #0
CreateParameter(1)
Parameter #1 Direction=Input
Parameter #1 Name=lvl
Parameter #1 Scale=7
Parameter #1 Value=Info
Add Parameter Parameter #1
CreateParameter(2)
Parameter #2 Direction=Input
Parameter #2 Name=lg
Parameter #2 Value=MyLogger
Add Parameter Parameter #2
CreateParameter(3)
Parameter #3 Direction=Input
Parameter #3 Name=date
Parameter #3 DbType=Time
Parameter #3 Value=" + expectedDate + @"
Add Parameter Parameter #3
ExecuteNonQuery: INSERT INTO FooBar VALUES(@msg, @lvl, @lg, @date)
CreateParameter(0)
Parameter #0 Direction=Input
Parameter #0 Name=msg
Parameter #0 Size=9
Parameter #0 Precision=3
Parameter #0 Scale=7
Parameter #0 Value=msg3
Add Parameter Parameter #0
CreateParameter(1)
Parameter #1 Direction=Input
Parameter #1 Name=lvl
Parameter #1 Scale=7
Parameter #1 Value=Debug
Add Parameter Parameter #1
CreateParameter(2)
Parameter #2 Direction=Input
Parameter #2 Name=lg
Parameter #2 Value=MyLogger2
Add Parameter Parameter #2
CreateParameter(3)
Parameter #3 Direction=Input
Parameter #3 Name=date
Parameter #3 DbType=Time
Parameter #3 Value=" + expectedDate + @"
Add Parameter Parameter #3
ExecuteNonQuery: INSERT INTO FooBar VALUES(@msg, @lvl, @lg, @date)
Close()
Dispose()
";
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

            var db = new DatabaseTarget();
            db.CommandText = "not important";
            db.ConnectionString = "cannotconnect";
            db.DBProvider = typeof(MockDbConnection).AssemblyQualifiedName;
            db.Initialize(null);
            db.WriteAsyncLogEvent(LogEventInfo.CreateNullEvent().WithContinuation(exceptions.Add));
            db.Close();

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
        public void DatabaseExceptionTest3()
        {
            MockDbConnection.ClearLog();
            var exceptions = new List<Exception>();

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
            dt.OpenConnection("myConnectionString");
            Assert.Equal(1, MockDbConnection2.OpenCount);
            Assert.Equal("myConnectionString", MockDbConnection2.LastOpenConnectionString);
        }
#endif

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
#if !NETSTANDARD
                Assert.Equal(typeof(SqlConnection), dt.ConnectionType);
#else
                Assert.NotNull(dt.ConnectionType);
#endif
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
                LogManager.Configuration = CreateConfigurationFromString(@"
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
                LogManager.Configuration = CreateConfigurationFromString(@"
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


            LogManager.Configuration = CreateConfigurationFromString(
                String.Format(nlogXmlConfig, GetSQLiteDbProvider(), connectionString)
            );
        }

        [Fact]
        public void NotRethrowingInstallExceptions()
        {
            SetupSqliteConfigWithInvalidInstallCommand("not_rethrowing_install_exceptions");

            // Default InstallationContext should not rethrow exceptions
            InstallationContext context = new InstallationContext();

            Assert.False(context.IgnoreFailures, "Failures should not be ignored by default");
            Assert.False(context.ThrowExceptions, "Exceptions should not be thrown by default");

            var exRecorded = Record.Exception(() => LogManager.Configuration.Install(context));
            Assert.Null(exRecorded);
        }


        [Fact]
        public void RethrowingInstallExceptions()
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

        [Fact]
        public void SqlServer_NoTargetInstallException()
        {
            if (IsTravis())
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
            if (IsTravis())
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
                LogManager.Configuration = CreateConfigurationFromString(@"
            <nlog xmlns='http://www.nlog-project.org/schemas/NLog.xsd'
                  xmlns:xsi='http://www.w3.org/2001/XMLSchema-instance' throwExceptions='true'>
                <targets>
                    <target name='database' xsi:type='Database' connectionstring=""" + connectionString + @"""
                        commandText='insert into dbo.NLogSqlServerTest (Uid) values (@uid);'>
                        <parameter name='@uid' layout='${event-properties:uid}' />
<install-command ignoreFailures=""false""
                 text=""CREATE TABLE dbo.NLogSqlServerTest (
    Id       int               NOT NULL IDENTITY(1,1) PRIMARY KEY CLUSTERED,
    Uid      uniqueidentifier  NULL
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
        [InlineData("usetransactions='false'", true)]
        [InlineData("usetransactions='true'", true)]
        [InlineData("", false)]
        public void WarningForObsoleteUseTransactions(string property, bool printWarning)
        {
            LoggingConfiguration c = CreateConfigurationFromString($@"
            <nlog ThrowExceptions='true'>
                <targets>
                    <target type='database' {property} name='t1' commandtext='fake sql' connectionstring='somewhere' />
                </targets>
                <rules>
                      <logger name='*' writeTo='t1'>
                       
                      </logger>
                    </rules>
            </nlog>");

            StringWriter writer1 = new StringWriter()
            {
                NewLine = "\n"
            };
            InternalLogger.LogWriter = writer1;
            var t = c.FindTargetByName<DatabaseTarget>("t1");
            t.Initialize(null);
            var internalLog = writer1.ToString();

            if (printWarning)
            {
                Assert.Contains("obsolete", internalLog, StringComparison.OrdinalIgnoreCase);
                Assert.Contains("usetransactions", internalLog, StringComparison.OrdinalIgnoreCase);
            }
            else
            {
                Assert.DoesNotContain("obsolete", internalLog, StringComparison.OrdinalIgnoreCase);
                Assert.DoesNotContain("usetransactions", internalLog, StringComparison.OrdinalIgnoreCase);
            }
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
                throw new NotImplementedException();
            }

            public IDbTransaction BeginTransaction()
            {
                throw new NotImplementedException();
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
                    throw new InvalidOperationException("Cannot open fake database.");
                }
            }

            public ConnectionState State => throw new NotImplementedException();

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

            public IDbDataParameter CreateParameter()
            {
                ((MockDbConnection)Connection).AddToLog("CreateParameter({0})", paramCount);
                return new MockDbParameter(this, paramCount++);
            }

            public int ExecuteNonQuery()
            {
                ((MockDbConnection)Connection).AddToLog("ExecuteNonQuery: {0}", CommandText);
                if (Connection.ConnectionString == "cannotexecute")
                {
                    throw new InvalidOperationException("Failure during ExecuteNonQuery");
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

            public IDbTransaction Transaction { get; set; }

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
                get => parameterType;
                set
                {
                    ((MockDbConnection)mockDbCommand.Connection).AddToLog("Parameter #{0} DbType={1}", paramId, value);
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
                    ((MockDbConnection)mockDbCommand.Connection).AddToLog("Parameter #{0} Value={1}", paramId, value);
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
            /// AppVeyor connectionstring for SQL 2016, see https://www.appveyor.com/docs/services-databases/
            /// </summary>
            private const string AppVeyorConnectionStringMaster =
                @"Server=(local)\SQL2016;Database=master;User ID=sa;Password=Password12!";

            private const string AppVeyorConnectionStringNLogTest =
                @"Server=(local)\SQL2016;Database=NLogTest;User ID=sa;Password=Password12!";

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