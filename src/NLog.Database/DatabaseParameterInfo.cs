// 
// Copyright (c) 2004-2021 Jaroslaw Kowalski <jaak@jkowalski.net>, Kim Christensen, Julian Verdurmen
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
    using System.ComponentModel;
    using System.Data;
    using System.Globalization;
    using System.Reflection;
    using NLog.Common;
    using NLog.Config;
    using NLog.Internal;
    using NLog.Layouts;

    /// <summary>
    /// Represents a parameter to a Database target.
    /// </summary>
    [NLogConfigurationItem]
    public class DatabaseParameterInfo
    {
        private readonly ValueTypeLayoutInfo _layoutInfo = new ValueTypeLayoutInfo();

        /// <summary>
        /// Initializes a new instance of the <see cref="DatabaseParameterInfo" /> class.
        /// </summary>
        public DatabaseParameterInfo()
            : this(null, null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DatabaseParameterInfo" /> class.
        /// </summary>
        /// <param name="parameterName">Name of the parameter.</param>
        /// <param name="parameterLayout">The parameter layout.</param>
        public DatabaseParameterInfo(string parameterName, Layout parameterLayout)
        {
            Name = parameterName;
            Layout = parameterLayout;
        }

        /// <summary>
        /// Gets or sets the database parameter name.
        /// </summary>
        /// <docgen category='Parameter Options' order='0' />
        [RequiredParameter]
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the layout that should be use to calculate the value for the parameter.
        /// </summary>
        /// <docgen category='Parameter Options' order='1' />
        [RequiredParameter]
        public Layout Layout { get => _layoutInfo.Layout; set => _layoutInfo.Layout = value; }

        /// <summary>
        /// Gets or sets the database parameter DbType.
        /// </summary>
        /// <docgen category='Parameter Options' order='2' />
        [DefaultValue(null)]
        public string DbType
        {
            get => _dbType;
            set
            {
                _dbType = value;
                if (!string.IsNullOrEmpty(_dbType))
                {
                    if (ParameterType == null || ParameterType == _dbParameterType)
                    {
                        var dbParameterType = TryParseDbType(DbType);
                        if (dbParameterType != null)
                        {
                            ParameterType = dbParameterType;
                        }
                        _dbParameterType = dbParameterType;
                    }
                }
                else if (_dbParameterType != null && ParameterType == _dbParameterType)
                {
                    ParameterType = null;
                }
            }
        }
        private string _dbType;
        private Type _dbParameterType;

        /// <summary>
        /// Gets or sets the database parameter size.
        /// </summary>
        /// <docgen category='Parameter Options' order='3' />
        [DefaultValue(0)]
        public int Size { get; set; }

        /// <summary>
        /// Gets or sets the database parameter precision.
        /// </summary>
        /// <docgen category='Parameter Options' order='4' />
        [DefaultValue(0)]
        public byte Precision { get; set; }

        /// <summary>
        /// Gets or sets the database parameter scale.
        /// </summary>
        /// <docgen category='Parameter Options' order='5' />
        [DefaultValue(0)]
        public byte Scale { get; set; }

        /// <summary>
        /// Gets or sets the type of the parameter.
        /// </summary>
        /// <docgen category='Parameter Options' order='6' />
        public Type ParameterType
        {
            get => _layoutInfo.ValueType;
            set
            {
                _dbParameterType = null;
                _layoutInfo.ValueType = value;
            }
        }

        /// <summary>
        /// Gets or sets the fallback value when result value is not available
        /// </summary>
        public Layout DefaultValue { get => _layoutInfo.DefaultValue; set => _layoutInfo.DefaultValue = value; }

        /// <summary>
        /// Gets or sets convert format of the database parameter value.
        /// </summary>
        /// <docgen category='Parameter Options' order='8' />
        [DefaultValue(null)]
        public string Format { get => _layoutInfo.ValueParseFormat; set => _layoutInfo.ValueParseFormat = value; }

        /// <summary>
        /// Gets or sets the culture used for parsing parameter string-value for type-conversion
        /// </summary>
        /// <docgen category='Parameter Options' order='9' />
        [DefaultValue(null)]
        public CultureInfo Culture { get => _layoutInfo.ValueParseCulture; set => _layoutInfo.ValueParseCulture = value; }

        /// <summary>
        /// Gets or sets whether empty value should translate into DbNull. Requires database column to allow NULL values.
        /// </summary>
        /// <docgen category='Parameter Options' order='8' />
        [DefaultValue(false)]
        public bool AllowDbNull
        {
            get => _allowDbNull;
            set
            {
                _allowDbNull = value;
                if (value)
                    DefaultValue = new Layout<DBNull>(DBNull.Value);
                else if (DefaultValue is Layout<DBNull>)
                    DefaultValue = null;
            }
        }
        private bool _allowDbNull;

        /// <summary>
        /// Render Result Value
        /// </summary>
        /// <param name="logEvent">Log event for rendering</param>
        /// <returns>Result value when available, else fallback to defaultValue</returns>
        public object RenderValue(LogEventInfo logEvent) => _layoutInfo.RenderValue(logEvent);

        internal bool SetDbType(IDbDataParameter dbParameter)
        {
            if (!string.IsNullOrEmpty(DbType))
            {
                if (_cachedDbTypeSetter == null || !_cachedDbTypeSetter.IsValid(dbParameter.GetType(), DbType))
                {
                    _cachedDbTypeSetter = new DbTypeSetter(dbParameter.GetType(), DbType);
                }

                return _cachedDbTypeSetter.SetDbType(dbParameter);
            }

            return true;    // DbType not in use
        }

        private static Type TryParseDbType(string dbTypeString)
        {
            string[] dbTypeNames = dbTypeString?.Split(new[] { '.' }, StringSplitOptions.RemoveEmptyEntries);
            dbTypeString = dbTypeNames?.Length > 0 ? dbTypeNames[dbTypeNames.Length - 1] : dbTypeString;

            if (string.IsNullOrEmpty(dbTypeString))
                return null;
            if (dbTypeString.IndexOf("Bool", StringComparison.OrdinalIgnoreCase) >= 0)
                return typeof(bool);
            if (dbTypeString.IndexOf("Timestamp", StringComparison.OrdinalIgnoreCase) >= 0)
                return typeof(DateTime);
            if (dbTypeString.IndexOf(nameof(System.Data.DbType.DateTimeOffset), StringComparison.OrdinalIgnoreCase) >= 0)
                return typeof(DateTimeOffset);
            if (dbTypeString.IndexOf(nameof(System.Data.DbType.DateTime), StringComparison.OrdinalIgnoreCase) >= 0)
                return typeof(DateTime);
            if (dbTypeString.IndexOf(nameof(System.Data.DbType.Date), StringComparison.OrdinalIgnoreCase) >= 0)
                return typeof(DateTime);
            if (dbTypeString.IndexOf(nameof(System.Data.DbType.Decimal), StringComparison.OrdinalIgnoreCase) >= 0)
                return typeof(decimal);
            if (dbTypeString.IndexOf(nameof(System.Data.DbType.Guid), StringComparison.OrdinalIgnoreCase) >= 0)
                return typeof(Guid);
            if (dbTypeString.IndexOf(nameof(System.Data.DbType.String), StringComparison.OrdinalIgnoreCase) >= 0)
                return typeof(string);
            if (dbTypeString.Equals(nameof(System.Data.DbType.Xml), StringComparison.OrdinalIgnoreCase))
                return typeof(string);
            if (dbTypeString.Equals(nameof(System.Data.DbType.UInt16), StringComparison.OrdinalIgnoreCase))
                return typeof(ushort);
            if (dbTypeString.Equals(nameof(System.Data.DbType.UInt32), StringComparison.OrdinalIgnoreCase))
                return typeof(uint);
            if (dbTypeString.Equals(nameof(System.Data.DbType.UInt64), StringComparison.OrdinalIgnoreCase))
                return typeof(ulong);
            if (dbTypeString.Equals(nameof(System.Data.DbType.Int16), StringComparison.OrdinalIgnoreCase))
                return typeof(short);
            if (dbTypeString.Equals(nameof(System.Data.DbType.Int32), StringComparison.OrdinalIgnoreCase))
                return typeof(int);
            if (dbTypeString.Equals(nameof(System.Data.DbType.Int64), StringComparison.OrdinalIgnoreCase))
                return typeof(long);
            if (dbTypeString.Equals(nameof(System.Data.DbType.Double), StringComparison.OrdinalIgnoreCase))
                return typeof(double);
            if (dbTypeString.Equals(nameof(System.Data.DbType.Single), StringComparison.OrdinalIgnoreCase))
                return typeof(float);
            if (dbTypeString.Equals(nameof(System.Data.DbType.Object), StringComparison.OrdinalIgnoreCase))
                return typeof(object);
            if (dbTypeString.Equals(nameof(System.Data.DbType.SByte), StringComparison.OrdinalIgnoreCase))
                return typeof(sbyte);
            if (dbTypeString.Equals(nameof(System.Data.DbType.Byte), StringComparison.OrdinalIgnoreCase))
                return typeof(byte);
            if (dbTypeString.Equals(nameof(System.Data.DbType.VarNumeric), StringComparison.OrdinalIgnoreCase))
                return typeof(decimal);
            if (dbTypeString.Equals(nameof(System.Data.DbType.Currency), StringComparison.OrdinalIgnoreCase))
                return typeof(decimal);
            if (dbTypeString.Equals(nameof(System.Data.DbType.Time), StringComparison.OrdinalIgnoreCase))
                return typeof(TimeSpan);

            return null;
        }

        DbTypeSetter _cachedDbTypeSetter;

        class DbTypeSetter
        {
            private readonly Type _dbPropertyInfoType;
            private readonly string _dbTypeName;
            private readonly PropertyInfo _dbTypeSetter;
            private readonly Enum _dbTypeValue;
            private Action<IDbDataParameter> _dbTypeSetterFast;

            public DbTypeSetter(Type dbParameterType, string dbTypeName)
            {
                _dbPropertyInfoType = dbParameterType;
                _dbTypeName = dbTypeName?.Trim();
                if (!string.IsNullOrEmpty(_dbTypeName))
                {
                    string[] dbTypeNames = _dbTypeName.Split(new[] { '.' }, StringSplitOptions.RemoveEmptyEntries);
                    if (dbTypeNames.Length > 1 && !string.Equals(dbTypeNames[0], nameof(System.Data.DbType), StringComparison.OrdinalIgnoreCase))
                    {
                        PropertyInfo propInfo = dbParameterType.GetProperty(dbTypeNames[0], BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);
                        if (propInfo != null && TryParseEnum(dbTypeNames[1], propInfo.PropertyType, out Enum enumType))
                        {
                            _dbTypeSetter = propInfo;
                            _dbTypeValue = enumType;
                        }
                    }
                    else
                    {
                        dbTypeName = dbTypeNames[dbTypeNames.Length - 1];
                        if (!string.IsNullOrEmpty(dbTypeName) && ConversionHelpers.TryParseEnum(dbTypeName, out DbType dbType))
                        {
                            _dbTypeValue = dbType;
                            _dbTypeSetterFast = (p) => p.DbType = dbType;
                        }
                    }
                }
            }

            public bool IsValid(Type dbParameterType, string dbTypeName)
            {
                if (ReferenceEquals(_dbPropertyInfoType, dbParameterType) && ReferenceEquals(_dbTypeName, dbTypeName))
                {
                    if (_dbTypeSetterFast == null && _dbTypeSetter != null && _dbTypeValue != null)
                    {
                        var propertySetter = _dbTypeSetter.CreatePropertySetter();
                        _dbTypeSetterFast = (p) => propertySetter.Invoke(p, _dbTypeValue);
                    }
                    return true;
                }
                return false;
            }

            public bool SetDbType(IDbDataParameter dbParameter)
            {
                if (_dbTypeSetterFast != null)
                {
                    _dbTypeSetterFast.Invoke(dbParameter);
                    return true;
                }
                else if (_dbTypeSetter != null && _dbTypeValue != null)
                {
                    _dbTypeSetter.SetValue(dbParameter, _dbTypeValue, null);
                    return true;
                }
                return false;
            }

            private static bool TryParseEnum(string value, Type enumType, out Enum enumValue)
            {
                if (!string.IsNullOrEmpty(value))
                {
                    // Note: .NET Standard 2.1 added a public Enum.TryParse(Type)
                    try
                    {
                        enumValue = Enum.Parse(enumType, value, true) as Enum;
                        return true;
                    }
                    catch (ArgumentException)
                    {
                        enumValue = null;
                        return false;
                    }
                }
                else
                {
                    enumValue = null;
                    return false;
                }
            }
        }
    }
}