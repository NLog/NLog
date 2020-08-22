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
        public Layout Layout { get; set; }

        /// <summary>
        /// Gets or sets the database parameter DbType.
        /// </summary>
        /// <docgen category='Parameter Options' order='2' />
        [DefaultValue(null)]
        public string DbType { get; set; }

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
        [DefaultValue(typeof(string))]
        public Type ParameterType { get => _parameterType ?? _cachedDbTypeSetter?.ParameterType ?? typeof(string); set => _parameterType = value; }
        private Type _parameterType;

        /// <summary>
        /// Gets or sets convert format of the database parameter value.
        /// </summary>
        /// <docgen category='Parameter Options' order='8' />
        [DefaultValue(null)]
        public string Format { get; set; }

        /// <summary>
        /// Gets or sets the culture used for parsing parameter string-value for type-conversion
        /// </summary>
        /// <docgen category='Parameter Options' order='9' />
        [DefaultValue(null)]
        public CultureInfo Culture { get; set; }

        /// <summary>
        /// Gets or sets whether empty value should translate into DbNull. Requires database column to allow NULL values.
        /// </summary>
        /// <docgen category='Parameter Options' order='8' />
        [DefaultValue(false)]
        public bool AllowDbNull { get; set; }

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

        DbTypeSetter _cachedDbTypeSetter;

        class DbTypeSetter
        {
            private readonly Type _dbPropertyInfoType;
            private readonly string _dbTypeName;
            private readonly PropertyInfo _dbTypeSetter;
            private readonly Enum _dbTypeValue;
            private Action<IDbDataParameter> _dbTypeSetterFast;

            public Type ParameterType { get; }

            public DbTypeSetter(Type dbParameterType, string dbTypeName)
            {
                _dbPropertyInfoType = dbParameterType;
                _dbTypeName = dbTypeName;
                if (!StringHelpers.IsNullOrWhiteSpace(dbTypeName))
                {
                    string[] dbTypeNames = dbTypeName.SplitAndTrimTokens('.');
                    if (dbTypeNames.Length > 1 && !string.Equals(dbTypeNames[0], nameof(System.Data.DbType), StringComparison.OrdinalIgnoreCase))
                    {
                        PropertyInfo propInfo = dbParameterType.GetProperty(dbTypeNames[0], BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);
                        if (propInfo != null)
                        {
                            if (TryParseEnum(dbTypeNames[1], propInfo.PropertyType, out Enum enumType))
                            {
                                _dbTypeSetter = propInfo;
                                _dbTypeValue = enumType;
                                ParameterType = TryParseParameterType(enumType.ToString());
                            }
                        }
                    }
                    else
                    {
                        dbTypeName = dbTypeNames[dbTypeNames.Length - 1];
                        if (!string.IsNullOrEmpty(dbTypeName) && ConversionHelpers.TryParseEnum(dbTypeName, out DbType dbType))
                        {
                            _dbTypeValue = dbType;
                            ParameterType = TryLookupParameterType(dbType);
                            _dbTypeSetterFast = (p) => p.DbType = dbType;
                        }
                    }
                }
            }

            private static Type TryLookupParameterType(DbType dbType)
            {
                switch (dbType)
                {
                    case System.Data.DbType.AnsiString:
                    case System.Data.DbType.String:
                    case System.Data.DbType.AnsiStringFixedLength:
                    case System.Data.DbType.StringFixedLength:
                    case System.Data.DbType.Xml:
                        return typeof(string);
                    case System.Data.DbType.Byte:
                        return typeof(byte);
                    case System.Data.DbType.SByte:
                        return typeof(sbyte);
                    case System.Data.DbType.Boolean:
                        return typeof(bool);
                    case System.Data.DbType.Date:
                    case System.Data.DbType.DateTime:
                    case System.Data.DbType.DateTime2:
                        return typeof(DateTime);
                    case System.Data.DbType.DateTimeOffset:
                        return typeof(DateTimeOffset);
                    case System.Data.DbType.Decimal:
                    case System.Data.DbType.VarNumeric:
                    case System.Data.DbType.Currency:
                        return typeof(decimal);
                    case System.Data.DbType.Double:
                        return typeof(double);
                    case System.Data.DbType.Guid:
                        return typeof(Guid);
                    case System.Data.DbType.Int16:
                        return typeof(short);
                    case System.Data.DbType.Int32:
                        return typeof(int);
                    case System.Data.DbType.Int64:
                        return typeof(long);
                    case System.Data.DbType.Object:
                        return typeof(object);
                    case System.Data.DbType.Single:
                        return typeof(float);
                    case System.Data.DbType.Time:
                        return typeof(TimeSpan);
                    case System.Data.DbType.UInt16:
                        return typeof(ushort);
                    case System.Data.DbType.UInt32:
                        return typeof(uint);
                    case System.Data.DbType.UInt64:
                        return typeof(ulong);
                }

                return null;
            }

            private Type TryParseParameterType(string dbTypeString)
            {
                if (dbTypeString.IndexOf("Date", StringComparison.OrdinalIgnoreCase) >= 0)
                    return typeof(DateTime);
                else if (dbTypeString.IndexOf("Timestamp", StringComparison.OrdinalIgnoreCase) >= 0)
                    return typeof(DateTime);
                else if (dbTypeString.IndexOf("Double", StringComparison.OrdinalIgnoreCase) >= 0)
                    return typeof(double);
                else if (dbTypeString.IndexOf("Decimal", StringComparison.OrdinalIgnoreCase) >= 0)
                    return typeof(decimal);
                else if (dbTypeString.IndexOf("Bool", StringComparison.OrdinalIgnoreCase) >= 0)
                    return typeof(bool);
                else if (dbTypeString.IndexOf("Guid", StringComparison.OrdinalIgnoreCase) >= 0)
                    return typeof(Guid);

                return null;
            }

            public bool IsValid(Type dbParameterType, string dbTypeName)
            {
                if (ReferenceEquals(_dbPropertyInfoType, dbParameterType) && ReferenceEquals(_dbTypeName, dbTypeName))
                {
                    if (_dbTypeSetterFast == null && _dbTypeSetter != null && _dbTypeValue != null)
                    {
                        var dbTypeSetterLambda = ReflectionHelpers.CreateLateBoundMethodSingle(_dbTypeSetter.GetSetMethod());
                        _dbTypeSetterFast = (p) => dbTypeSetterLambda.Invoke(p, _dbTypeValue);
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
                if (!string.IsNullOrEmpty(value) && ConversionHelpers.TryParseEnum(value, enumType, out var enumValueT))
                {
                    enumValue = enumValueT as Enum;
                    return enumValue != null;
                }
                enumValue = null;
                return false;
            }
        }
    }
}