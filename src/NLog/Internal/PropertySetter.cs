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

using System;
using System.Reflection;
using JetBrains.Annotations;
using NLog.Common;

namespace NLog.Internal
{
    /// <summary>
    /// Compiled version for setting a property value.
    /// </summary>
    internal class PropertySetter : PropertySetter<object>
    {
        /// <inheritdoc />
        private PropertySetter([NotNull] PropertyInfo property) : base(property)
        {
        }

        /// <summary>
        /// Try create the property setter. If the property is not found, or not settable, <c>null</c> will be returned
        /// </summary>
        /// <param name="obj">The object to get the type for the instance</param>
        /// <param name="propertyName">The name property to set</param>
        /// <param name="bindingFlags"></param>
        /// <returns></returns>
        [CanBeNull]
        public static PropertySetter TryCreate([NotNull] object obj, [NotNull] string propertyName,
            BindingFlags bindingFlags = BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase) // note: BindingFlags.Default is not available for .NET Standard 1.3
        {
            if (TryGetProperty(obj, propertyName, bindingFlags, out var property))
            {
                return null;
            }

            return new PropertySetter(property);
        }

        /// <summary>
        /// Try create the property setter. If the property is not found, or not settable, <c>null</c> will be returned
        /// </summary>
        /// <param name="obj">The object to get the type for the instance</param>
        /// <param name="propertyName">The name property to set</param>
        /// <param name="bindingFlags"></param>
        /// <returns></returns>
        [CanBeNull]
        public static PropertySetter<T> TryCreate<T>([NotNull] T obj, [NotNull] string propertyName,
            BindingFlags bindingFlags = BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase) // note: BindingFlags.Default is not available for .NET Standard 1.3
        {
            if (PropertySetter<T>.TryGetProperty(obj, propertyName, bindingFlags, out var property))
            {
                return null;
            }

            return new PropertySetter<T>(property);
        }
    }

    /// <summary>
    /// Compiled version for setting a property value.
    /// </summary>
    /// <typeparam name="T">The type of the property. Not used for detection, but only for compile-time safe <see cref="SetValue"/></typeparam>
    internal class PropertySetter<T>
    {
        private readonly ReflectionHelpers.LateBoundMethod _propertySetter;

        /// <summary>
        /// array with 1 item. The property will always have one property
        /// </summary>
        private readonly object[] _arrayWith1Item = new object[1];

        public PropertySetter([NotNull] PropertyInfo property)
        {
            if (property == null)
            {
                throw new ArgumentNullException(nameof(property));
            }

            _propertySetter = ReflectionHelpers.CreateLateBoundMethod(property.GetSetMethod());
        }

        /// <summary>
        /// Try get the property
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="propertyName"></param>
        /// <param name="bindingFlags"></param>
        /// <param name="property"></param>
        /// <returns></returns>
        protected static bool TryGetProperty(T obj, string propertyName, BindingFlags bindingFlags, out PropertyInfo property)
        {
            if (obj == null)
            {
                throw new ArgumentNullException(nameof(obj));
            }

            if (propertyName == null)
            {
                throw new ArgumentNullException(nameof(propertyName));
            }

            var type = obj.GetType();
            property = type.GetProperty(propertyName, bindingFlags);
            if (property == null)
            {
                InternalLogger.Warn("Cannot set {0} on type {1}, property not found", propertyName, type.FullName);
                return true;
            }

            if (!property.CanWrite)
            {
                InternalLogger.Warn("Cannot set {0} on type {1}, property not settable", propertyName, type.FullName);
                return true;
            }

            return false;
        }

        /// <summary>
        /// Set the value on the property
        /// </summary>
        public void SetValue([NotNull] T obj, [CanBeNull] object value)
        {
            if (obj == null)
            {
                throw new ArgumentNullException(nameof(obj));
            }

            _arrayWith1Item[0] = value;
            _propertySetter.Invoke(obj, _arrayWith1Item);
        }
    }
}