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
using System.Text;
using System.Diagnostics;
using System.Reflection;
using System.Data;
using System.Collections;

using NLog.Internal;
using NLog.Config;

namespace NLog.Appenders
{
    [Appender("Database")]
    public sealed class DatabaseAppender: Appender
    {
        private Assembly _system_data_assembly = typeof(IDbConnection).Assembly;
        private Type _connectionType = null;
        private string _connectionString = null;
        private bool _keepConnection = true;
        private bool _useTransaction = false;
        private string _dbHost = ".";
        private string _dbUserName = null;
        private string _dbPassword = null;
        private string _dbDatabase = null;
        private Layout _compiledCommandTextLayout = null;
        private ParameterInfoCollection _parameters = new ParameterInfoCollection();
        private IDbConnection _activeConnection = null;
        private string _connectionStringCache = null;

        public DatabaseAppender()
        {
            DBProvider = "sqlserver";
        }

        public string DBProvider
        {
            get
            {
                return _connectionType.FullName;
            }
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

        [RequiredParameter]
        public string ConnectionString
        {
            get
            {
                return _connectionString;
            }
            set
            {
                _connectionString = value;
            }
        }

        public bool KeepConnection
        {
            get
            {
                return _keepConnection;
            }
            set
            {
                _keepConnection = value;
            }
        }

        public bool UseTransactions
        {
            get
            {
                return _useTransaction;
            }
            set
            {
                _useTransaction = value;
            }
        }

        public string DBHost
        {
            get
            {
                return _dbHost;
            }
            set
            {
                _dbHost = value;
            }
        }

        public string DBUserName
        {
            get
            {
                return _dbUserName;
            }
            set
            {
                _dbUserName = value;
            }
        }

        public string DBPassword
        {
            get
            {
                return _dbPassword;
            }
            set
            {
                _dbPassword = value;
            }
        }

        public string DBDatabase
        {
            get
            {
                return _dbDatabase;
            }
            set
            {
                _dbDatabase = value;
            }
        }

        public string CommandText
        {
            get
            {
                return _compiledCommandTextLayout.Text;
            }
            set
            {
                _compiledCommandTextLayout = new Layout(value);
            }
        }

        public Layout CommandTextLayout
        {
            get
            {
                return _compiledCommandTextLayout;
            }
            set
            {
                _compiledCommandTextLayout = value;
            }
        }

        [ArrayParameter(typeof(ParameterInfo), "parameter")]
        public ParameterInfoCollection Parameters
        {
            get
            {
                return _parameters;
            }
        }

        protected internal override void Append(LogEventInfo ev)
        {
            if (_keepConnection)
            {
                lock(this)
                {
                    if (_activeConnection == null)
                        _activeConnection = OpenConnection();
                    DoAppend(ev);
                }
            }
            else
            {
                try
                {
                    _activeConnection = OpenConnection();
                    DoAppend(ev);
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

        private void DoAppend(LogEventInfo ev)
        {
            IDbCommand command = _activeConnection.CreateCommand();
            command.CommandText = CommandTextLayout.GetFormattedMessage(ev);
            foreach (ParameterInfo par in Parameters)
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
                p.Value = par.CompiledLayout.GetFormattedMessage(ev);
                command.Parameters.Add(p);
            }
            command.ExecuteNonQuery();
        }

        private IDbConnection OpenConnection()
        {
            ConstructorInfo constructor = _connectionType.GetConstructor(new Type[]
            {
                typeof(string)
            }

            );
            IDbConnection retVal = (IDbConnection)constructor.Invoke(new object[]
            {
                BuildConnectionString()
            }

            );

            if (retVal != null)
                retVal.Open();

            return retVal;
        }

        private string BuildConnectionString()
        {
            if (_connectionStringCache != null)
                return _connectionStringCache;

            if (_connectionString != null)
                return _connectionString;

            StringBuilder sb = new StringBuilder();

            sb.Append("Server=");
            sb.Append(DBHost);
            if (DBUserName == null)
                sb.Append("Trusted_Connection=SSPI;");
            else
            {
                sb.Append("User id=");
                sb.Append(DBUserName);
                sb.Append(";Password=");
                sb.Append(DBPassword);
                sb.Append(";");
            }

            if (DBDatabase != null)
            {
                sb.Append("Database=");
                sb.Append(DBDatabase);
            }

            _connectionStringCache = sb.ToString();

            InternalLogger.Debug("Connection string: {0}", _connectionStringCache);
            return _connectionStringCache;
        }

        public class ParameterInfo
        {
            public ParameterInfo(){}

            private Layout _compiledlayout;
            private string _name;
            private int _size = 0;
            private byte _precision = 0;
            private byte _scale = 0;

            public string Name
            {
                get
                {
                    return _name;
                }
                set
                {
                    _name = value;
                }
            }

            [RequiredParameter]
            public string Layout
            {
                get
                {
                    return _compiledlayout.Text;
                }
                set
                {
                    _compiledlayout = new Layout(value);
                }
            }

            public Layout CompiledLayout
            {
                get
                {
                    return _compiledlayout;
                }
                set
                {
                    _compiledlayout = value;
                }
            }

            public int Size
            {
                get
                {
                    return _size;
                }
                set
                {
                    _size = value;
                }
            }

            public byte Precision
            {
                get
                {
                    return _precision;
                }
                set
                {
                    _precision = value;
                }
            }

            public byte Scale
            {
                get
                {
                    return _scale;
                }
                set
                {
                    _scale = value;
                }
            }
        }

#region Generated Typesafe Collection Wrapper

        /// <summary>
        /// A collection of elements of type ParameterInfo
        /// </summary>
        public class ParameterInfoCollection: System.Collections.CollectionBase
        {
            /// <summary>
            /// Initializes a new empty instance of the ParameterInfoCollection class.
            /// </summary>
            public ParameterInfoCollection()
            {
                // empty
            }

            /// <summary>
            /// Initializes a new instance of the ParameterInfoCollection class, containing elements
            /// copied from an array.
            /// </summary>
            /// <param name="items">
            /// The array whose elements are to be added to the new ParameterInfoCollection.
            /// </param>
            public ParameterInfoCollection(ParameterInfo[]items)
            {
                this.AddRange(items);
            }

            /// <summary>
            /// Initializes a new instance of the ParameterInfoCollection class, containing elements
            /// copied from another instance of ParameterInfoCollection
            /// </summary>
            /// <param name="items">
            /// The ParameterInfoCollection whose elements are to be added to the new ParameterInfoCollection.
            /// </param>
            public ParameterInfoCollection(ParameterInfoCollection items)
            {
                this.AddRange(items);
            }

            /// <summary>
            /// Adds the elements of an array to the end of this ParameterInfoCollection.
            /// </summary>
            /// <param name="items">
            /// The array whose elements are to be added to the end of this ParameterInfoCollection.
            /// </param>
            public virtual void AddRange(ParameterInfo[]items)
            {
                foreach (ParameterInfo item in items)
                {
                    this.List.Add(item);
                }
            }

            /// <summary>
            /// Adds the elements of another ParameterInfoCollection to the end of this ParameterInfoCollection.
            /// </summary>
            /// <param name="items">
            /// The ParameterInfoCollection whose elements are to be added to the end of this ParameterInfoCollection.
            /// </param>
            public virtual void AddRange(ParameterInfoCollection items)
            {
                foreach (ParameterInfo item in items)
                {
                    this.List.Add(item);
                }
            }

            /// <summary>
            /// Adds an instance of type ParameterInfo to the end of this ParameterInfoCollection.
            /// </summary>
            /// <param name="value">
            /// The ParameterInfo to be added to the end of this ParameterInfoCollection.
            /// </param>
            public virtual void Add(ParameterInfo value)
            {
                this.List.Add(value);
            }

            /// <summary>
            /// Determines whether a specfic ParameterInfo value is in this ParameterInfoCollection.
            /// </summary>
            /// <param name="value">
            /// The ParameterInfo value to locate in this ParameterInfoCollection.
            /// </param>
            /// <returns>
            /// true if value is found in this ParameterInfoCollection;
            /// false otherwise.
            /// </returns>
            public virtual bool Contains(ParameterInfo value)
            {
                return this.List.Contains(value);
            }

            /// <summary>
            /// Return the zero-based index of the first occurrence of a specific value
            /// in this ParameterInfoCollection
            /// </summary>
            /// <param name="value">
            /// The ParameterInfo value to locate in the ParameterInfoCollection.
            /// </param>
            /// <returns>
            /// The zero-based index of the first occurrence of the _ELEMENT value if found;
            /// -1 otherwise.
            /// </returns>
            public virtual int IndexOf(ParameterInfo value)
            {
                return this.List.IndexOf(value);
            }

            /// <summary>
            /// Inserts an element into the ParameterInfoCollection at the specified index
            /// </summary>
            /// <param name="index">
            /// The index at which the ParameterInfo is to be inserted.
            /// </param>
            /// <param name="value">
            /// The ParameterInfo to insert.
            /// </param>
            public virtual void Insert(int index, ParameterInfo value)
            {
                this.List.Insert(index, value);
            }

            /// <summary>
            /// Gets or sets the ParameterInfo at the given index in this ParameterInfoCollection.
            /// </summary>
            public virtual ParameterInfo this[int index]
            {
                get
                {
                    return (ParameterInfo)this.List[index];
                }
                set
                {
                    this.List[index] = value;
                }
            }

            /// <summary>
            /// Removes the first occurrence of a specific ParameterInfo from this ParameterInfoCollection.
            /// </summary>
            /// <param name="value">
            /// The ParameterInfo value to remove from this ParameterInfoCollection.
            /// </param>
            public virtual void Remove(ParameterInfo value)
            {
                this.List.Remove(value);
            }

            /// <summary>
            /// Type-specific enumeration class, used by ParameterInfoCollection.GetEnumerator.
            /// </summary>
            public class Enumerator: System.Collections.IEnumerator
            {
                private System.Collections.IEnumerator wrapped;

                public Enumerator(ParameterInfoCollection collection)
                {
                    this.wrapped = ((System.Collections.CollectionBase)collection).GetEnumerator();
                }

                public ParameterInfo Current
                {
                    get
                    {
                        return (ParameterInfo)(this.wrapped.Current);
                    }
                }

                object System.Collections.IEnumerator.Current
                {
                    get
                    {
                        return (ParameterInfo)(this.wrapped.Current);
                    }
                }

                public bool MoveNext()
                {
                    return this.wrapped.MoveNext();
                }

                public void Reset()
                {
                    this.wrapped.Reset();
                }
            }

            /// <summary>
            /// Returns an enumerator that can iterate through the elements of this ParameterInfoCollection.
            /// </summary>
            /// <returns>
            /// An object that implements System.Collections.IEnumerator.
            /// </returns>        
            public new virtual ParameterInfoCollection.Enumerator GetEnumerator()
            {
                return new ParameterInfoCollection.Enumerator(this);
            }
        }

#endregion 
    }
}
