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


#if !SILVERLIGHT && !__IOS__ && !__ANDROID__

using System;
using System.Collections.Generic;
using System.Data;
using System.Reflection;
using NLog.Targets;

namespace NLog.Internal
{
    /// <summary>
    /// Set dbType on correct property (e.g. DbType, OleDbType, etc)
    /// </summary>
    internal class DatabaseParameterTypeSetter
    {
        private bool _defaultDbProperty;

        /// <summary>
        /// SQL Command Parameter DbType Property
        /// </summary>
        private PropertyInfo _dbTypeProperty;

        /// <summary>
        /// SQL Command Parameter instance DbType Property Values
        /// </summary>
        private Dictionary<DatabaseParameterInfo, int> _propertyDbTypeValues;
        /// <summary>
        /// Resolve Parameter DbType Property and DbType Value
        /// </summary>
        /// <docgen category='Parameter Options' order='10' />
        public void Resolve(IDbDataParameter parameter, string dbTypePropertyName, IList<DatabaseParameterInfo> parametersInfo)
        {
            Type propertyType;
            if (string.IsNullOrEmpty(dbTypePropertyName) ||
                dbTypePropertyName.Equals(nameof(IDataParameter.DbType), StringComparison.OrdinalIgnoreCase))
            {
                _defaultDbProperty = true;
                propertyType = typeof(DbType);
            }
            else
            {
                _defaultDbProperty = false;
                if (!PropertyHelper.TryGetPropertyInfo(parameter, dbTypePropertyName, out var dbTypeProperty))
                {
                    throw new NLogConfigurationException(
                        "Type '" + parameter.GetType().Name + "' has no property '" + dbTypePropertyName + "'.");
                }

                _dbTypeProperty = dbTypeProperty;
                propertyType = dbTypeProperty.PropertyType;
            }

            _propertyDbTypeValues = new Dictionary<DatabaseParameterInfo, int>();
            foreach (var par in parametersInfo)
            {
                if (!string.IsNullOrEmpty(par.DbType))
                {
                    var dbTypeValue = Enum.Parse(propertyType, par.DbType);
                    _propertyDbTypeValues[par] = (int)dbTypeValue;
                }
            }
        }
        /// <summary>
        /// Set Parameter DbType
        /// </summary>
        public void SetParameterDbType(IDbDataParameter p, DatabaseParameterInfo par)
        {
            if (_propertyDbTypeValues.TryGetValue(par, out var dbType))
            {
                if (_defaultDbProperty)
                {
                    p.DbType = (DbType)dbType;
                }
                else
                {
                    _dbTypeProperty.SetValue(p, dbType, null);
                }
            }
        }


    }
}
#endif
