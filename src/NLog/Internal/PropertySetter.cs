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

namespace NLog.Internal
{
    using System;
    using System.Reflection;
    using NLog.Common;

    internal class PropertySetter
    {
        private readonly Type _objectType;
        private readonly PropertyInfo _propertyInfo;
        private ReflectionHelpers.LateBoundMethodSingle _propertySetter;

        public static PropertySetter CreatePropertySetter(Type objectType, string propertyName)
        {
            if (TryGetProperty(objectType, propertyName, out var propertyInfo))
            {
                return new PropertySetter(objectType, propertyInfo);
            }

            return null;
        }

        private PropertySetter(Type objectType, PropertyInfo propertyInfo)
        {
            _objectType = objectType;
            _propertyInfo = propertyInfo;
        }

        public bool SetPropertyValue(object instance, object value)
        {
            if (instance == null)
            {
                throw new ArgumentNullException(nameof(instance));
            }

            if (_propertySetter != null)
            {
                _propertySetter.Invoke(instance, value);
            }
            else
            {
                // Generate compiled method if setter works without throwing exception
                var setterMethod = _propertyInfo.GetSetMethod();
                setterMethod.Invoke(instance, new[] { value });
                _propertySetter = ReflectionHelpers.CreateLateBoundMethodSingle(setterMethod);
            }

            return true;
        }

        /// <summary>
        /// Try get the property
        /// </summary>
        private static bool TryGetProperty(Type objectType, string propertyName, out PropertyInfo property)
        {
            if (objectType == null)
            {
                throw new ArgumentNullException(nameof(objectType));
            }

            if (propertyName == null)
            {
                throw new ArgumentNullException(nameof(propertyName));
            }

            property = objectType.GetProperty(propertyName, BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
            if (property == null)
            {
                InternalLogger.Warn("Cannot set {0} on type {1}, property not found", propertyName, objectType.FullName);
                return false;
            }

            if (!property.CanWrite)
            {
                InternalLogger.Warn("Cannot set {0} on type {1}, property not settable", propertyName, objectType.FullName);
                return false;
            }

            return true;
        }
    }
}
