// 
// Copyright (c) 2004-2016 Jaroslaw Kowalski <jaak@jkowalski.net>, Kim Christensen, Julian Verdurmen
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

using System.Data;

#if !SILVERLIGHT && !__IOS__ && !__ANDROID__

namespace NLog.Targets
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Globalization;
    using System.Reflection;
    using NLog.Config;
    using NLog.Internal;
    using NLog.Layouts;

    /// <summary>
    /// Represents SQL command parameter converter.
    /// </summary>
    public class DatabaseParameterConverter
    {
        /// <summary>
        /// SQL Command Parameter DbType Property
        /// </summary>
        private PropertyInfo DbTypeProperty { get; set; }
        /// <summary>
        /// SQL Command Parameter instance DbType Property Values
        /// </summary>
        private Dictionary<DatabaseParameterInfo, object> PropertyDbTypeValues { get; set; }
        /// <summary>
        /// Resolve Parameter DbType Property and DbType Value
        /// </summary>
        /// <docgen category='Parameter Options' order='10' />
        public void Resolve(IDbDataParameter p, string dbTypePropertyName, IList<DatabaseParameterInfo> parameters)
        {
            PropertyInfo dbTypeProperty;
            if (!PropertyHelper.TryGetPropertyInfo(p, dbTypePropertyName, out dbTypeProperty))
            {
                throw new NLogConfigurationException(
                    "Type '" + p.GetType().Name + "' has no property '" + dbTypePropertyName + "'.");
            }
            this.DbTypeProperty = dbTypeProperty;
            this.PropertyDbTypeValues = new Dictionary<DatabaseParameterInfo, object>();
            foreach (var par in parameters)
            {
                if (string.IsNullOrEmpty(par.DbType)) continue;
                var dbTypeValue = Enum.Parse(dbTypeProperty.PropertyType, par.DbType);
                this.PropertyDbTypeValues[par] = dbTypeValue;
            }
        }
        /// <summary>
        /// Set Parameter DbType
        /// </summary>
        public void SetParameterDbType(IDbDataParameter p, DatabaseParameterInfo par)
        {
            this.DbTypeProperty.SetValue(p, this.PropertyDbTypeValues[par], null);
        }
        /// <summary>
        /// Set Parameter Value
        /// </summary>
        public void SetParameterValue(IDbDataParameter p, DatabaseParameterInfo par, string value)
        {
            p.Value = ConvertTo(p, par, value);
        }
        /// <summary>
        /// convert layout value to parameter value
        /// </summary>
        protected virtual object ConvertTo(IDbDataParameter p, DatabaseParameterInfo par, string value)
        {
            string format = par.Format;
            switch (p.DbType)
            {
                case DbType.String:
                    return value;
                case DbType.Boolean:
                    return bool.Parse(value);
                case DbType.Decimal:
                case DbType.Currency:
                case DbType.VarNumeric:
                    return decimal.Parse(value);
                case DbType.Double:
                    return double.Parse(value);
                case DbType.Single:
                    return float.Parse(value);
                case DbType.DateTime:
                case DbType.DateTime2:
                case DbType.Date:
                case DbType.Time:
                    var dateFormat = string.IsNullOrEmpty(format) ? "yyyy/MM/dd HH:mm:ss.fff" : format;
                    return DateTime.ParseExact(value, dateFormat, null);
                case DbType.DateTimeOffset:
                    var dateOffsetFormat = string.IsNullOrEmpty(format) ? "yyyy/MM/dd HH:mm:ss.fff zzz" : format;
                    return DateTimeOffset.ParseExact(value, dateOffsetFormat, null);
                case DbType.Guid:
#if NET3_5
                    return new Guid(value);
#else
                    return string.IsNullOrEmpty(format) ? Guid.Parse(value) : Guid.ParseExact(value, format);
#endif
                case DbType.Byte:
                    return byte.Parse(value);
                case DbType.SByte:
                    return sbyte.Parse(value);
                case DbType.Int16:
                    return short.Parse(value);
                case DbType.Int32:
                    return int.Parse(value);
                case DbType.Int64:
                    return long.Parse(value);
                case DbType.UInt16:
                    return ushort.Parse(value);
                case DbType.UInt32:
                    return uint.Parse(value);
                case DbType.UInt64:
                    return ulong.Parse(value);
                case DbType.Binary:
                    return ConvertToBinary(p, value, format);
                default:
                    return value;
            }
        }
        /// <summary>
        /// convert layout value to parameter value
        /// </summary>
        protected virtual object ConvertToBinary(IDbDataParameter p, string value, string format)
        {
            var byteCount = value.Length / 2;
            var buffer = new byte[byteCount];
            for (int i = 0; i < byteCount; i++)
            {
                buffer[i] = byte.Parse(value.Substring(i * 2, 2), NumberStyles.AllowHexSpecifier);
            }
            return buffer;
        }
    }
}
#endif