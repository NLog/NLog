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
    using NLog.Config;
    using NLog.Internal;
    using NLog.Layouts;

    /// <summary>
    /// Information about object-property for the database-connection-object
    /// </summary>
    [NLogConfigurationItem]
    public class DatabaseObjectPropertyInfo
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DatabaseObjectPropertyInfo"/> class.
        /// </summary>
        public DatabaseObjectPropertyInfo()
        {
        }

        /// <summary>
        /// Gets or sets the name for the object-property
        /// </summary>
        /// <docgen category='Connection Options' order='10' />
        [RequiredParameter]
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the value to assign on the object-property
        /// </summary>
        /// <docgen category='Connection Options' order='10' />
        [RequiredParameter]
        public Layout Layout { get; set; }

        /// <summary>
        /// Gets or sets the type of the object-property
        /// </summary>
        /// <docgen category='Connection Options' order='10' />
        [DefaultValue(typeof(string))]
        public Type PropertyType { get; set; } = typeof(string);

        /// <summary>
        /// Gets or sets convert format of the property value
        /// </summary>
        /// <docgen category='Connection Options' order='8' />
        [DefaultValue(null)]
        public string Format { get; set; }

        /// <summary>
        /// Gets or sets the culture used for parsing property string-value for type-conversion
        /// </summary>
        /// <docgen category='Connection Options' order='9' />
        [DefaultValue(null)]
        public CultureInfo Culture { get; set; }

        internal bool SetPropertyValue(object dbObject, object propertyValue)
        {
            var dbConnectionType = dbObject.GetType();
            var propertySetterCache = _propertySetter;
            if (!propertySetterCache.Equals(Name, dbConnectionType))
            {
                var propertySetter = PropertySetter.CreatePropertySetter(dbConnectionType, Name);
                propertySetterCache = new PropertySetterCacheItem(Name, dbConnectionType, propertySetter);
                _propertySetter = propertySetterCache;
            }

            return propertySetterCache.PropertySetter?.SetPropertyValue(dbObject, propertyValue) ?? false;
        }

        private struct PropertySetterCacheItem
        {
            public string PropertyName { get; }
            public Type ObjectType { get; }
            public PropertySetter PropertySetter { get; }

            public PropertySetterCacheItem(string propertyName, Type objectType, PropertySetter propertySetter)
            {
                PropertyName = propertyName;
                ObjectType = objectType;
                PropertySetter = propertySetter;
            }

            public bool Equals(string propertyName, Type objectType)
            {
                return PropertyName == propertyName && ObjectType == objectType;
            }
        }

        PropertySetterCacheItem _propertySetter;
    }
}