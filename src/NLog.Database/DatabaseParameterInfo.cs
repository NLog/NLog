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
        private static readonly Dictionary<string, Type> _typesByDbTypeName =
            new Dictionary<string, Type>(StringComparer.OrdinalIgnoreCase)
            {
                { nameof(System.Data.DbType.AnsiString), typeof(string) },
                // { nameof(System.Data.DbType.Binary), typeof(byte[]) },  // not supported
                { nameof(System.Data.DbType.Byte), typeof(byte) },
                { nameof(System.Data.DbType.Boolean), typeof(bool) },
                { nameof(System.Data.DbType.Currency), typeof(decimal) },
                { nameof(System.Data.DbType.Date), typeof(DateTime) }, // DateOnly when .net framework will be deprecated
                { nameof(System.Data.DbType.DateTime), typeof(DateTime) },
                { nameof(System.Data.DbType.Decimal), typeof(decimal) },
                { nameof(System.Data.DbType.Double), typeof(double) },
                { nameof(System.Data.DbType.Guid), typeof(Guid) },
                { nameof(System.Data.DbType.Int16), typeof(short) },
                { nameof(System.Data.DbType.Int32), typeof(int) },
                { nameof(System.Data.DbType.Int64), typeof(long) },
                { nameof(System.Data.DbType.Object), typeof(object) }, // not sure if we should support this
                { nameof(System.Data.DbType.SByte), typeof(sbyte) },
                { nameof(System.Data.DbType.Single), typeof(float) },
                { nameof(System.Data.DbType.String), typeof(string) },
                { nameof(System.Data.DbType.Time), typeof(TimeSpan) }, // TimeOnly when .net framework will be deprecated
                { nameof(System.Data.DbType.UInt16), typeof(ushort) },
                { nameof(System.Data.DbType.UInt32), typeof(uint) },
                { nameof(System.Data.DbType.UInt64), typeof(ulong) },
                { nameof(System.Data.DbType.VarNumeric), typeof(decimal) },
                { nameof(System.Data.DbType.AnsiStringFixedLength), typeof(string) },
                { nameof(System.Data.DbType.StringFixedLength), typeof(string) },
                { nameof(System.Data.DbType.Xml), typeof(string) },
                { nameof(System.Data.DbType.DateTime2), typeof(DateTime) },
                { nameof(System.Data.DbType.DateTimeOffset), typeof(DateTimeOffset) }
            };

        private Func<IDbDataParameter, bool>? _dbTypeSetter;

        private readonly ValueTypeLayoutInfo _layoutInfo = new ValueTypeLayoutInfo();

        /// <summary>
        /// Initializes a new instance of the <see cref="DatabaseParameterInfo" /> class.
        /// </summary>
        public DatabaseParameterInfo()
            : this(string.Empty, Layout.Empty)
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
        /// Initializes a new instance of the <see cref="DatabaseParameterInfo" /> class.
        /// </summary>
        /// <param name="parameterName">Name of the parameter.</param>
        /// <param name="parameterLayout">The parameter layout.</param>
        /// <param name="dbTypeSetter">Method-delegate to perform custom initialization database-parameter. Ex. for AOT to assign custom DbType.</param>
        public DatabaseParameterInfo(string parameterName, Layout parameterLayout, Action<IDbDataParameter> dbTypeSetter)
            : this(parameterName, parameterLayout)
        {
            if (dbTypeSetter is null) throw new ArgumentNullException(nameof(dbTypeSetter));

            _dbTypeSetter = (dbParameter) =>
            {
                dbTypeSetter.Invoke(dbParameter);
                return true;
            };
        }

        /// <summary>
        /// Gets or sets the database parameter name.
        /// </summary>
        /// <docgen category='Parameter Options' order='0' />
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the layout used for rendering the database-parameter value.
        /// </summary>
        /// <docgen category='Parameter Options' order='1' />
        public Layout Layout { get => _layoutInfo.Layout; set => _layoutInfo.Layout = value; }

        /// <summary>
        /// Gets or sets the database parameter DbType.
        /// </summary>
        /// <remarks>
        /// Not compatible with AOT since using type-reflection to convert into Enum and assigning value.
        /// </remarks>
        /// <docgen category='Parameter Options' order='2' />
        public string DbType
        {
            get => _dbTypeEnum.HasValue ? _dbTypeEnum.ToString() : _dbType;
            set
            {
                _dbType = value;
                _dbTypeEnum = null;

                if (!string.IsNullOrEmpty(_dbType))
                {
                    if (ParameterType is null || ParameterType == _dbParameterType)
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

                _dbTypeSetter = AssignParameterDbType;
            }
        }
        private string _dbType = string.Empty;
        private Type? _dbParameterType;

        /// <summary>
        /// Gets or sets the database parameter DbType (without reflection logic)
        /// </summary>
        public DbType DbTypeEnum
        {
            get => _dbTypeEnum ?? default;
            set
            {
                _dbTypeEnum = value;
                _dbType = string.Empty;

                if (ParameterType is null || ParameterType == _dbParameterType)
                {
                    var dbParameterType = TryParseDbType(value.ToString());
                    if (dbParameterType != null)
                    {
                        ParameterType = dbParameterType;
                    }
                    _dbParameterType = dbParameterType;
                }

                _dbTypeSetter = (dbParam) =>
                {
                    dbParam.DbType = value;
                    return true;
                };
            }
        }
        private DbType? _dbTypeEnum;

        /// <summary>
        /// Gets or sets the database parameter size.
        /// </summary>
        /// <docgen category='Parameter Options' order='3' />
        public int Size { get; set; }

        /// <summary>
        /// Gets or sets the database parameter precision.
        /// </summary>
        /// <docgen category='Parameter Options' order='4' />
        public byte Precision { get; set; }

        /// <summary>
        /// Gets or sets the database parameter scale.
        /// </summary>
        /// <docgen category='Parameter Options' order='5' />
        public byte Scale { get; set; }

        /// <summary>
        /// Gets or sets the type of the parameter.
        /// </summary>
        /// <docgen category='Parameter Options' order='6' />
        public Type? ParameterType
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
        /// <docgen category='Parameter Options' order='7' />
        public Layout? DefaultValue { get => _layoutInfo.DefaultValue; set => _layoutInfo.DefaultValue = value; }

        /// <summary>
        /// Gets or sets convert format of the database parameter value.
        /// </summary>
        /// <docgen category='Parameter Options' order='8' />
        public string? Format { get => _layoutInfo.ValueParseFormat; set => _layoutInfo.ValueParseFormat = value; }

        /// <summary>
        /// Gets or sets the culture used for parsing parameter string-value for type-conversion
        /// </summary>
        /// <docgen category='Parameter Options' order='9' />
        public CultureInfo? Culture { get => _layoutInfo.ValueParseCulture; set => _layoutInfo.ValueParseCulture = value; }

        /// <summary>
        /// Gets or sets whether empty value should translate into DbNull. Requires database column to allow NULL values.
        /// </summary>
        /// <docgen category='Parameter Options' order='10' />
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
        public object? RenderValue(LogEventInfo logEvent) => _layoutInfo.RenderValue(logEvent);

        internal bool SetDbType(IDbDataParameter dbParameter)
        {
            return _dbTypeSetter?.Invoke(dbParameter) ?? true;
        }

        private static Type? TryParseDbType(string dbTypeName)
        {
            // retrieve the type name if a full name is given
            dbTypeName = dbTypeName is null ? string.Empty : dbTypeName.Substring(dbTypeName.LastIndexOf('.') + 1).Trim();

            if (string.IsNullOrEmpty(dbTypeName))
                return null;

            return _typesByDbTypeName.TryGetValue(dbTypeName, out var type) ? type : null;
        }

        private bool AssignParameterDbType(IDbDataParameter dbParameter)
        {
            if (_cachedDbTypeSetter is null || !_cachedDbTypeSetter.IsValid(dbParameter.GetType(), DbType))
            {
                _cachedDbTypeSetter = new DbTypeSetter(dbParameter.GetType(), _dbType);
            }

            return _cachedDbTypeSetter.SetDbType(dbParameter);
        }

        private DbTypeSetter? _cachedDbTypeSetter;

        private sealed class DbTypeSetter
        {
            private readonly Type _dbParameterType;
            private readonly string _dbTypeName;
            private readonly Action<IDbDataParameter>? _dbTypeSetter;

            public DbTypeSetter(Type dbParameterType, string dbTypeName)
            {
                _dbParameterType = dbParameterType;
                _dbTypeName = dbTypeName?.Trim() ?? string.Empty;
                if (!string.IsNullOrEmpty(_dbTypeName))
                {
                    string[] dbTypeNames = _dbTypeName.Split(new[] { '.' }, StringSplitOptions.RemoveEmptyEntries);
                    if (dbTypeNames.Length > 1 && !string.Equals(dbTypeNames[0], nameof(System.Data.DbType), StringComparison.OrdinalIgnoreCase))
                    {
                        _dbTypeSetter = BuildCustomDbSetter(dbParameterType, dbTypeNames[0], dbTypeNames[1]);
                    }
                    else
                    {
                        dbTypeName = dbTypeNames[dbTypeNames.Length - 1];
                        if (!string.IsNullOrEmpty(dbTypeName) && ConversionHelpers.TryParseEnum(dbTypeName, out DbType dbType))
                        {
                            _dbTypeSetter = (p) => p.DbType = dbType;
                        }
                        else
                        {
                            InternalLogger.Error("DatabaseTarget: Failed to resolve enum to assign DbType={0}", dbTypeName);
                        }
                    }
                }
                else
                {
                    _dbTypeSetter = (p) => { };
                }
            }

            private static Action<IDbDataParameter>? BuildCustomDbSetter(Type dbParameterType, string dbTypePropertyName, string dbTypeEnumValue)
            {
                PropertyInfo propInfo = dbParameterType.GetProperty(dbTypePropertyName, BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);
                if (propInfo is null)
                {
                    InternalLogger.Error("DatabaseTarget: Failed to resolve type {0} property '{1}' to assign DbType={2}", dbParameterType, dbTypePropertyName, dbTypeEnumValue);
                    return null;
                }

                if (!TryParseEnum(dbTypeEnumValue, propInfo.PropertyType, out var dbType) || dbType is null)
                {
                    InternalLogger.Error("DatabaseTarget: Failed to resolve enum {0} to assign DbType={1}", propInfo.PropertyType, dbTypeEnumValue);
                    return null;
                }

                var propertySetter = propInfo.CreatePropertySetter();
                return (p) => propertySetter.Invoke(p, dbType);
            }

            public bool IsValid(Type dbParameterType, string dbTypeName)
            {
                return ReferenceEquals(_dbParameterType, dbParameterType) && string.Equals(_dbTypeName, dbTypeName, StringComparison.OrdinalIgnoreCase);
            }

            public bool SetDbType(IDbDataParameter dbParameter)
            {
                if (_dbTypeSetter != null)
                {
                    _dbTypeSetter.Invoke(dbParameter);
                    return true;
                }
                return false;
            }

            private static bool TryParseEnum(string value, Type enumType, out Enum? enumValue)
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

                enumValue = null;
                return false;
            }
        }
    }
}
