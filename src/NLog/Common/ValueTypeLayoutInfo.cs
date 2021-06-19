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

using System;
using System.Globalization;
using System.Reflection;
using NLog.Config;
using NLog.Layouts;

namespace NLog.Common
{
    /// <summary>
    /// Typed Value that is easily configured from NLog.config file
    /// </summary>
    [NLogConfigurationItem]
    public abstract class ValueTypeLayoutInfo
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ValueTypeLayoutInfo" /> class.
        /// </summary>
        protected ValueTypeLayoutInfo()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ValueTypeLayoutInfo" /> class.
        /// </summary>
        /// <param name="layout">Dynamic value</param>
        protected ValueTypeLayoutInfo(Layout layout)
            :this(layout, typeof(string))
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ValueTypeLayoutInfo" /> class.
        /// </summary>
        /// <param name="layout">Dynamic value</param>
        /// <param name="valueType">Expected value type</param>
        protected ValueTypeLayoutInfo(Layout layout, Type valueType)
        {
            _valueType = valueType ?? typeof(string);
            Layout = layout;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ValueTypeLayoutInfo" /> class.
        /// </summary>
        /// <param name="fixedValue">Fixed value</param>
        protected ValueTypeLayoutInfo(object fixedValue)
        {
            if (fixedValue is Layout layout)
            {
                _valueType = typeof(string);
                Layout = layout;
            }
            else
            {
                _valueType = fixedValue?.GetType() ?? typeof(string);
                _layout = _innerLayout = CreateTypedLayout(_valueType, fixedValue);
            }
        }

        /// <summary>
        /// Gets or sets the layout that will render the result value
        /// </summary>
        [RequiredParameter]
        public Layout Layout
        {
            get => _layout ?? (_layout = CreateTypedLayout(ValueType, ExtractExistingValue(_innerLayout)));  // Ensure the correct Layout-object is initialized by config, and scanned by NLog Target
            set
            {
                _defaultValueObject = null;
                if (value is ITypedLayout typedLayout)
                {
                    _layout = _innerLayout = value;
                    _valueType = typedLayout.ValueType;
                    _typedLayout = typedLayout;
                }
                else
                {
                    _layout = null;
                    _innerLayout = value;
                    _typedLayout = null;
                }
            }
        }
        private Layout _layout;
        private Layout _innerLayout;
        private ITypedLayout _typedLayout;

        /// <summary>
        /// Gets or sets the result value type, for conversion of layout rendering output
        /// </summary>
        protected virtual Type ValueType
        {
            get => _valueType;
            set
            {
                _valueType = value;
                _layout = null;
                if (!ReferenceEquals(_innerLayout, _typedLayout) && !ReferenceEquals(_innerLayout, null))
                    _typedLayout = null;
                _defaultValueObject = null;
            }
        }
        private Type _valueType;

        /// <summary>
        /// Gets or sets the fallback value when result value is not available
        /// </summary>
        public Layout DefaultValue
        {
            get => _defaultValue;
            set
            {
                _defaultValue = value;
                _defaultValueObject = null;
                _typedDefaultValue = value as ITypedLayout;
            }
        }
        private Layout _defaultValue;
        private ITypedLayout _typedDefaultValue;
        private object _defaultValueObject;

        /// <summary>
        /// Gets or sets format used for parsing parameter string-value for type-conversion
        /// </summary>
        protected string ValueParseFormat
        {
            get => _valueParseFormat;
            set
            {
                _valueParseFormat = value;
                if (!ReferenceEquals(_innerLayout, _typedLayout) && !ReferenceEquals(_innerLayout, null))
                    _typedLayout = null;
                _defaultValueObject = null;
            }
        }
        private string _valueParseFormat;

        /// <summary>
        /// Gets or sets the culture used for parsing parameter string-value for type-conversion
        /// </summary>
        /// <docgen category='Parameter Options' order='9' />
        protected CultureInfo ValueParseCulture
        {
            get => _valueParseCulture;
            set
            {
                _valueParseCulture = value;
                if (!ReferenceEquals(_innerLayout, _typedLayout) && !ReferenceEquals(_innerLayout, null))
                    _typedLayout = null;
                _defaultValueObject = null;
            }
        }
        private CultureInfo _valueParseCulture = CultureInfo.InvariantCulture;

        /// <summary>
        /// Render Result Value
        /// </summary>
        /// <param name="logEvent">Log event for rendering</param>
        /// <returns>Result value when available, else fallback to defaultValue</returns>
        public object RenderValue(LogEventInfo logEvent)
        {
            var defaultValue = _defaultValueObject ?? (_defaultValueObject = CreateTypedDefaultValue());

            var layout = Layout;
            if (_typedLayout != null)
            {
                return _typedLayout.RenderValue(logEvent, defaultValue);
            }
            else if (layout != null)
            {
                var valueType = _valueType ?? (_valueType = ValueType ?? typeof(string));
                if (valueType == typeof(object) && layout.TryGetRawValue(logEvent, out var rawValue))
                {
                    return rawValue;
                }

                var stringValue = layout.Render(logEvent) ?? string.Empty;
                if (string.IsNullOrEmpty(stringValue))
                {
                    return defaultValue;
                }

                return stringValue;
            }
            else
            {
                return null;
            }
        }

        private Layout CreateTypedLayout(Type valueType, object existingValue)
        {
            if (ReferenceEquals(valueType, null) || valueType == typeof(string) || valueType == typeof(object))
                return existingValue as Layout ?? _innerLayout;

            try
            {
                var concreteType = typeof(Layout<>).MakeGenericType(valueType);
                var constructorParams = existingValue is Layout ? new object[] { existingValue, ValueParseFormat, ValueParseCulture } : new object[] { existingValue };
                var typedLayout = (Layout)Activator.CreateInstance(concreteType, BindingFlags.Instance | BindingFlags.Public, null, constructorParams, null);
                _typedLayout = typedLayout as ITypedLayout;
                return typedLayout;
            }
            catch (TargetInvocationException ex)
            {
                if (ex.InnerException != null)
                    throw ex.InnerException;
                throw;
            }
        }

        private object ExtractExistingValue(object innerValue)
        {
            if (innerValue is SimpleLayout simpleLayout)
                return simpleLayout;
            else if (innerValue is ITypedLayout typedLayout)
                return typedLayout.InnerLayout ?? typedLayout.FixedObjectValue;
            else if (innerValue is Layout innerLayout)
                return innerLayout.Render(LogEventInfo.CreateNullEvent());
            else
                return innerValue;
        }

        private object CreateTypedDefaultValue()
        {
            if (_typedDefaultValue?.IsFixed == true)
                return _typedDefaultValue.FixedObjectValue;

            if (ReferenceEquals(_defaultValue, null))
                return string.Empty;

            if (_typedLayout == null)
                return _defaultValue?.Render(LogEventInfo.CreateNullEvent()) ?? string.Empty;

            if (_typedLayout.IsFixed)
                return string.Empty;

            var defaultStringValue = _defaultValue?.Render(LogEventInfo.CreateNullEvent()) ?? string.Empty;
            if (string.IsNullOrEmpty(defaultStringValue))
                return string.Empty;

            if (_typedLayout.TryParseValueFromString(defaultStringValue, out object defaultObjectValue))
                return defaultObjectValue ?? string.Empty;

            return string.Empty;
        }
    }
}
