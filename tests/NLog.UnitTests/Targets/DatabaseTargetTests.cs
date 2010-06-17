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

namespace NLog.UnitTests.Targets
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Data;
    using System.Globalization;
    using System.IO;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using NLog.Internal;
    using NLog.Targets;

    [TestClass]
    public class DatabaseTargetTests : NLogTestBase
    {
        private Logger logger = LogManager.GetLogger("NLog.UnitTests.Targets.DatabaseTargetTests");

        [TestMethod]
        public void SimpleDatabaseTest()
        {
            MockDbConnection.ClearLog();
            DatabaseTarget dt = new DatabaseTarget()
            {
                CommandText = "INSERT INTO FooBar VALUES('${message}')",
                ConnectionString = "FooBar",
                DbProvider = typeof(MockDbConnection).AssemblyQualifiedName,
            };

            dt.Initialize();
            Assert.AreSame(typeof(MockDbConnection), dt.ConnectionType);

            List<Exception> exceptions = new List<Exception>();
            dt.WriteAsyncLogEvent(new LogEventInfo(LogLevel.Info, "MyLogger", "msg1").WithContinuation(exceptions.Add));
            dt.WriteAsyncLogEvent(new LogEventInfo(LogLevel.Info, "MyLogger", "msg2").WithContinuation(exceptions.Add));
            dt.WriteAsyncLogEvent(new LogEventInfo(LogLevel.Info, "MyLogger", "msg3").WithContinuation(exceptions.Add));
            foreach (var ex in exceptions)
            {
                Assert.IsNull(ex, Convert.ToString(ex));
            }

            string expectedLog = @"Open('FooBar').
ExecuteNonQuery: INSERT INTO FooBar VALUES('msg1')
Close()
Open('FooBar').
ExecuteNonQuery: INSERT INTO FooBar VALUES('msg2')
Close()
Open('FooBar').
ExecuteNonQuery: INSERT INTO FooBar VALUES('msg3')
Close()
";

            Assert.AreEqual(expectedLog, MockDbConnection.Log);
        }

        [TestMethod]
        public void SimpleBatchedDatabaseTest()
        {
            MockDbConnection.ClearLog();
            DatabaseTarget dt = new DatabaseTarget()
            {
                CommandText = "INSERT INTO FooBar VALUES('${message}')",
                ConnectionString = "FooBar",
                DbProvider = typeof(MockDbConnection).AssemblyQualifiedName,
            };

            dt.Initialize();
            Assert.AreSame(typeof(MockDbConnection), dt.ConnectionType);

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
                Assert.IsNull(ex, Convert.ToString(ex));
            }

            string expectedLog = @"Open('FooBar').
ExecuteNonQuery: INSERT INTO FooBar VALUES('msg1')
ExecuteNonQuery: INSERT INTO FooBar VALUES('msg2')
ExecuteNonQuery: INSERT INTO FooBar VALUES('msg3')
Close()
";

            Assert.AreEqual(expectedLog, MockDbConnection.Log);
        }

        [TestMethod]
        public void KeepConnectionOpenTest()
        {
            MockDbConnection.ClearLog();
            DatabaseTarget dt = new DatabaseTarget()
            {
                CommandText = "INSERT INTO FooBar VALUES('${message}')",
                ConnectionString = "FooBar",
                DbProvider = typeof(MockDbConnection).AssemblyQualifiedName,
                KeepConnection = true,
            };

            dt.Initialize();
            Assert.AreSame(typeof(MockDbConnection), dt.ConnectionType);

            List<Exception> exceptions = new List<Exception>();
            dt.WriteAsyncLogEvent(new LogEventInfo(LogLevel.Info, "MyLogger", "msg1").WithContinuation(exceptions.Add));
            dt.WriteAsyncLogEvent(new LogEventInfo(LogLevel.Info, "MyLogger", "msg2").WithContinuation(exceptions.Add));
            dt.WriteAsyncLogEvent(new LogEventInfo(LogLevel.Info, "MyLogger", "msg3").WithContinuation(exceptions.Add));
            foreach (var ex in exceptions)
            {
                Assert.IsNull(ex, Convert.ToString(ex));
            }

            string expectedLog = @"Open('FooBar').
ExecuteNonQuery: INSERT INTO FooBar VALUES('msg1')
ExecuteNonQuery: INSERT INTO FooBar VALUES('msg2')
ExecuteNonQuery: INSERT INTO FooBar VALUES('msg3')
";

            Assert.AreEqual(expectedLog, MockDbConnection.Log);

            MockDbConnection.ClearLog();
            dt.Close();
            expectedLog = @"Close()
";

            Assert.AreEqual(expectedLog, MockDbConnection.Log);
        }

        [TestMethod]
        public void KeepConnectionOpenBatchedTest()
        {
            MockDbConnection.ClearLog();
            DatabaseTarget dt = new DatabaseTarget()
            {
                CommandText = "INSERT INTO FooBar VALUES('${message}')",
                ConnectionString = "FooBar",
                DbProvider = typeof(MockDbConnection).AssemblyQualifiedName,
                KeepConnection = true,
            };

            dt.Initialize();
            Assert.AreSame(typeof(MockDbConnection), dt.ConnectionType);
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
                Assert.IsNull(ex, Convert.ToString(ex));
            }

            string expectedLog = @"Open('FooBar').
ExecuteNonQuery: INSERT INTO FooBar VALUES('msg1')
ExecuteNonQuery: INSERT INTO FooBar VALUES('msg2')
ExecuteNonQuery: INSERT INTO FooBar VALUES('msg3')
";

            Assert.AreEqual(expectedLog, MockDbConnection.Log);

            MockDbConnection.ClearLog();
            dt.Close();
            expectedLog = @"Close()
";

            Assert.AreEqual(expectedLog, MockDbConnection.Log);
        }

        [TestMethod]
        public void KeepConnectionOpenTest2()
        {
            MockDbConnection.ClearLog();
            DatabaseTarget dt = new DatabaseTarget()
            {
                CommandText = "INSERT INTO FooBar VALUES('${message}')",
                ConnectionString = "Database=${logger}",
                DbProvider = typeof(MockDbConnection).AssemblyQualifiedName,
                KeepConnection = true,
            };

            dt.Initialize();
            Assert.AreSame(typeof(MockDbConnection), dt.ConnectionType);

            List<Exception> exceptions = new List<Exception>();
            dt.WriteAsyncLogEvent(new LogEventInfo(LogLevel.Info, "MyLogger", "msg1").WithContinuation(exceptions.Add));
            dt.WriteAsyncLogEvent(new LogEventInfo(LogLevel.Info, "MyLogger", "msg2").WithContinuation(exceptions.Add));
            dt.WriteAsyncLogEvent(new LogEventInfo(LogLevel.Info, "MyLogger2", "msg3").WithContinuation(exceptions.Add));
            dt.WriteAsyncLogEvent(new LogEventInfo(LogLevel.Info, "MyLogger", "msg4").WithContinuation(exceptions.Add));
            foreach (var ex in exceptions)
            {
                Assert.IsNull(ex, Convert.ToString(ex));
            }

            string expectedLog = @"Open('Database=MyLogger').
ExecuteNonQuery: INSERT INTO FooBar VALUES('msg1')
ExecuteNonQuery: INSERT INTO FooBar VALUES('msg2')
Close()
Open('Database=MyLogger2').
ExecuteNonQuery: INSERT INTO FooBar VALUES('msg3')
Close()
Open('Database=MyLogger').
ExecuteNonQuery: INSERT INTO FooBar VALUES('msg4')
";

            Assert.AreEqual(expectedLog, MockDbConnection.Log);

            MockDbConnection.ClearLog();
            dt.Close();
            expectedLog = @"Close()
";

            Assert.AreEqual(expectedLog, MockDbConnection.Log);
        }

        [TestMethod]
        public void KeepConnectionOpenBatchedTest2()
        {
            MockDbConnection.ClearLog();
            DatabaseTarget dt = new DatabaseTarget()
            {
                CommandText = "INSERT INTO FooBar VALUES('${message}')",
                ConnectionString = "Database=${logger}",
                DbProvider = typeof(MockDbConnection).AssemblyQualifiedName,
                KeepConnection = true,
            };

            dt.Initialize();

            Assert.AreSame(typeof(MockDbConnection), dt.ConnectionType);

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
                Assert.IsNull(ex, Convert.ToString(ex));
            }

            string expectedLog = @"Open('Database=MyLogger').
ExecuteNonQuery: INSERT INTO FooBar VALUES('msg1')
ExecuteNonQuery: INSERT INTO FooBar VALUES('msg2')
ExecuteNonQuery: INSERT INTO FooBar VALUES('msg4')
Close()
Open('Database=MyLogger2').
ExecuteNonQuery: INSERT INTO FooBar VALUES('msg3')
";

            Assert.AreEqual(expectedLog, MockDbConnection.Log);

            MockDbConnection.ClearLog();
            dt.Close();
            expectedLog = @"Close()
";

            Assert.AreEqual(expectedLog, MockDbConnection.Log);
        }

        [TestMethod]
        public void ParameterTest()
        {
            MockDbConnection.ClearLog();
            DatabaseTarget dt = new DatabaseTarget()
            {
                CommandText = "INSERT INTO FooBar VALUES(@msg, @lvl, @lg)",
                DbProvider = typeof(MockDbConnection).AssemblyQualifiedName,
                KeepConnection = true,
                Parameters =
                {
                    new DatabaseParameterInfo("msg", "${message}"),
                    new DatabaseParameterInfo("lvl", "${level}"),
                    new DatabaseParameterInfo("lg", "${logger}")
                        
                }
            };

            dt.Initialize();

            Assert.AreSame(typeof(MockDbConnection), dt.ConnectionType);

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
                Assert.IsNull(ex, Convert.ToString(ex));
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

            Assert.AreEqual(expectedLog, MockDbConnection.Log);

            MockDbConnection.ClearLog();
            dt.Close();
            expectedLog = @"Close()
";

            Assert.AreEqual(expectedLog, MockDbConnection.Log);
        }

        [TestMethod]
        public void ParameterFacetTest()
        {
            MockDbConnection.ClearLog();
            DatabaseTarget dt = new DatabaseTarget()
            {
                CommandText = "INSERT INTO FooBar VALUES(@msg, @lvl, @lg)",
                DbProvider = typeof(MockDbConnection).AssemblyQualifiedName,
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

            dt.Initialize();

            Assert.AreSame(typeof(MockDbConnection), dt.ConnectionType);

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
                Assert.IsNull(ex, Convert.ToString(ex));
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
ExecuteNonQuery: INSERT INTO FooBar VALUES(@msg, @lvl, @lg)
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
ExecuteNonQuery: INSERT INTO FooBar VALUES(@msg, @lvl, @lg)
Close()
";
            Assert.AreEqual(expectedLog, MockDbConnection.Log);
        }

        [TestMethod]
        public void ConnectionStringBuilderTest1()
        {
            DatabaseTarget dt;

            dt = new DatabaseTarget();
            Assert.AreEqual("Server=.;Trusted_Connection=SSPI;", this.GetConnectionString(dt));

            dt = new DatabaseTarget();
            dt.DbHost = "${logger}";
            Assert.AreEqual("Server=Logger1;Trusted_Connection=SSPI;", this.GetConnectionString(dt));

            dt = new DatabaseTarget();
            dt.DbHost = "HOST1";
            dt.DbDatabase= "${logger}";
            Assert.AreEqual("Server=HOST1;Trusted_Connection=SSPI;Database=Logger1", this.GetConnectionString(dt));

            dt = new DatabaseTarget();
            dt.DbHost = "HOST1";
            dt.DbDatabase = "${logger}";
            dt.DbUserName = "user1";
            dt.DbPassword = "password1";
            Assert.AreEqual("Server=HOST1;User id=user1;Password=password1;Database=Logger1", this.GetConnectionString(dt));

            dt = new DatabaseTarget();
            dt.ConnectionString = "customConnectionString42";
            dt.DbHost = "HOST1";
            dt.DbDatabase = "${logger}";
            dt.DbUserName = "user1";
            dt.DbPassword = "password1";
            Assert.AreEqual("customConnectionString42", this.GetConnectionString(dt));
        }

        [TestMethod]
        public void DatabaseExceptionTest1()
        {
            MockDbConnection.ClearLog();
            var exceptions = new List<Exception>();

            var db = new DatabaseTarget();
            db.CommandText = "not important";
            db.ConnectionString = "cannotconnect";
            db.DbProvider = typeof(MockDbConnection).AssemblyQualifiedName;
            db.Initialize();
            db.WriteAsyncLogEvent(LogEventInfo.CreateNullEvent().WithContinuation(exceptions.Add));
            db.Close();

            Assert.AreEqual(1, exceptions.Count);
            Assert.IsNotNull(exceptions[0]);
            Assert.AreEqual("Cannot open fake database.", exceptions[0].Message);
            Assert.AreEqual("Open('cannotconnect').\r\n", MockDbConnection.Log);
        }

        [TestMethod]
        public void DatabaseExceptionTest2()
        {
            MockDbConnection.ClearLog();
            var exceptions = new List<Exception>();

            var db = new DatabaseTarget();
            db.CommandText = "not important";
            db.ConnectionString = "cannotexecute";
            db.KeepConnection = true;
            db.DbProvider = typeof(MockDbConnection).AssemblyQualifiedName;
            db.Initialize();
            db.WriteAsyncLogEvent(LogEventInfo.CreateNullEvent().WithContinuation(exceptions.Add));
            db.WriteAsyncLogEvent(LogEventInfo.CreateNullEvent().WithContinuation(exceptions.Add));
            db.WriteAsyncLogEvent(LogEventInfo.CreateNullEvent().WithContinuation(exceptions.Add));
            db.Close();

            Assert.AreEqual(3, exceptions.Count);
            Assert.IsNotNull(exceptions[0]);
            Assert.IsNotNull(exceptions[1]);
            Assert.IsNotNull(exceptions[2]);
            Assert.AreEqual("Failure during ExecuteNonQuery", exceptions[0].Message);
            Assert.AreEqual("Failure during ExecuteNonQuery", exceptions[1].Message);
            Assert.AreEqual("Failure during ExecuteNonQuery", exceptions[2].Message);

            string expectedLog = @"Open('cannotexecute').
ExecuteNonQuery: not important
Close()
Open('cannotexecute').
ExecuteNonQuery: not important
Close()
Open('cannotexecute').
ExecuteNonQuery: not important
Close()
";
            Assert.AreEqual(expectedLog, MockDbConnection.Log);
        }

        [TestMethod]
        public void DatabaseExceptionTest3()
        {
            MockDbConnection.ClearLog();
            var exceptions = new List<Exception>();

            var db = new DatabaseTarget();
            db.CommandText = "not important";
            db.ConnectionString = "cannotexecute";
            db.KeepConnection = true;
            db.DbProvider = typeof(MockDbConnection).AssemblyQualifiedName;
            db.Initialize();
            db.WriteAsyncLogEvents(
                LogEventInfo.CreateNullEvent().WithContinuation(exceptions.Add),
                LogEventInfo.CreateNullEvent().WithContinuation(exceptions.Add),
                LogEventInfo.CreateNullEvent().WithContinuation(exceptions.Add));
            db.Close();

            Assert.AreEqual(3, exceptions.Count);
            Assert.IsNotNull(exceptions[0]);
            Assert.IsNotNull(exceptions[1]);
            Assert.IsNotNull(exceptions[2]);
            Assert.AreEqual("Failure during ExecuteNonQuery", exceptions[0].Message);
            Assert.AreEqual("Failure during ExecuteNonQuery", exceptions[1].Message);
            Assert.AreEqual("Failure during ExecuteNonQuery", exceptions[2].Message);

            string expectedLog = @"Open('cannotexecute').
ExecuteNonQuery: not important
Close()
Open('cannotexecute').
ExecuteNonQuery: not important
Close()
Open('cannotexecute').
ExecuteNonQuery: not important
Close()
";
            Assert.AreEqual(expectedLog, MockDbConnection.Log);
        }

        private string GetConnectionString(DatabaseTarget dt)
        {
            MockDbConnection.ClearLog();
            dt.DbProvider = typeof(MockDbConnection).AssemblyQualifiedName;
            dt.CommandText = "NotImportant";

            var exceptions = new List<Exception>();
            dt.Initialize();
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
                this.ConnectionString = connectionString;
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

            public int ConnectionTimeout
            {
                get { throw new NotImplementedException(); }
            }

            public IDbCommand CreateCommand()
            {
                return new MockDbCommand() { Connection = this };
            }

            public string Database
            {
                get { throw new NotImplementedException(); }
            }

            public void Open()
            {
                LastConnectionString = this.ConnectionString;
                AddToLog("Open('{0}').", this.ConnectionString);
                if (this.ConnectionString == "cannotconnect")
                {
                    throw new InvalidOperationException("Cannot open fake database.");
                }
            }

            public ConnectionState State
            {
                get { throw new NotImplementedException(); }
            }

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
                this.parameters = new MockParameterCollection(this);
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
                ((MockDbConnection)this.Connection).AddToLog("CreateParameter({0})", this.paramCount);
                return new MockDbParameter(this, paramCount++);
            }

            public int ExecuteNonQuery()
            {
                ((MockDbConnection)this.Connection).AddToLog("ExecuteNonQuery: {0}", this.CommandText);
                if (this.Connection.ConnectionString == "cannotexecute")
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

            public IDataParameterCollection Parameters
            {
                get { return parameters; }
            }

            public void Prepare()
            {
                throw new NotImplementedException();
            }

            public IDbTransaction Transaction { get; set; }

            public UpdateRowSource UpdatedRowSource
            {
                get
                {
                    throw new NotImplementedException();
                }
                set
                {
                    throw new NotImplementedException();
                }
            }

            public void Dispose()
            {
                throw new NotImplementedException();
            }
        }

        private class MockDbParameter : IDbDataParameter
        {
            private readonly MockDbCommand mockDbCommand;
            private readonly int paramId;

            public MockDbParameter(MockDbCommand mockDbCommand, int paramId)
            {
                this.mockDbCommand = mockDbCommand;
                this.paramId = paramId;
            }

            public DbType DbType
            {
                get { throw new NotImplementedException(); }
                set { throw new NotImplementedException(); }
            }

            public ParameterDirection Direction
            {
                get { throw new NotImplementedException(); }
                set { ((MockDbConnection)mockDbCommand.Connection).AddToLog("Parameter #{0} Direction={1}", paramId, value); }
            }

            public bool IsNullable
            {
                get { throw new NotImplementedException(); }
            }

            public string ParameterName
            {
                get { throw new NotImplementedException(); }
                set { ((MockDbConnection)mockDbCommand.Connection).AddToLog("Parameter #{0} Name={1}", paramId, value); }
            }

            public string SourceColumn
            {
                get { throw new NotImplementedException(); }
                set { throw new NotImplementedException(); }
            }

            public DataRowVersion SourceVersion
            {
                get { throw new NotImplementedException(); }
                set { throw new NotImplementedException(); }
            }

            public object Value
            {
                get { throw new NotImplementedException(); }
                set { ((MockDbConnection)mockDbCommand.Connection).AddToLog("Parameter #{0} Value={1}", paramId, value); }
            }

            public byte Precision
            {
                get { throw new NotImplementedException(); }
                set { ((MockDbConnection)mockDbCommand.Connection).AddToLog("Parameter #{0} Precision={1}", paramId, value); }
            }

            public byte Scale
            {
                get { throw new NotImplementedException(); }
                set { ((MockDbConnection)mockDbCommand.Connection).AddToLog("Parameter #{0} Scale={1}", paramId, value); }
            }

            public int Size
            {
                get { throw new NotImplementedException(); }
                set { ((MockDbConnection)mockDbCommand.Connection).AddToLog("Parameter #{0} Size={1}", paramId, value); }
            }

            public override string ToString()
            {
                return "Parameter #" + this.paramId;
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

            public int Count
            {
                get { throw new NotImplementedException(); }
            }

            public object SyncRoot
            {
                get { throw new NotImplementedException(); }
            }

            public bool IsSynchronized
            {
                get { throw new NotImplementedException(); }
            }

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
                get { throw new NotImplementedException(); }
                set { throw new NotImplementedException(); }
            }

            public bool IsReadOnly
            {
                get { throw new NotImplementedException(); }
            }

            public bool IsFixedSize
            {
                get { throw new NotImplementedException(); }
            }

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
                get { throw new NotImplementedException(); }
                set { throw new NotImplementedException(); }
            }
        }
    }
}

#endif