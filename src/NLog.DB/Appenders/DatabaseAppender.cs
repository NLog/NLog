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
using System.Data.SqlClient;

using NLog.Internal;
using NLog.Config;

namespace NLog.Appenders
{
    [Appender("Database")]
    public sealed class DatabaseAppender : Appender
    {
        private Type _connectionType = typeof(SqlConnection);
        private string _connectionString = null;
        private bool _keepConnection = true;
        private bool _useTransaction = false;
        private IDbConnection _activeConnection = null;

        public string ConnectionType
        {
            get { return _connectionType.AssemblyQualifiedName; }
            set { _connectionType = Type.GetType(value); }
                
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

        protected override void Append(LogEventInfo ev) {
            if (_keepConnection) {
                lock (this) {
                    if (_activeConnection == null)
                        _activeConnection = OpenConnection();
                    DoAppend(ev);
                }
            } else {
                using (_activeConnection = OpenConnection()) {
                    DoAppend(ev);
                }
            }
        }

        private void DoAppend(LogEventInfo ev) {
        }

        private IDbConnection OpenConnection() {
            ConstructorInfo constructor = _connectionType.GetConstructor(new Type[] { typeof(string) });
            return (IDbConnection)constructor.Invoke(new object[] { _connectionString });
        }
    }
}
