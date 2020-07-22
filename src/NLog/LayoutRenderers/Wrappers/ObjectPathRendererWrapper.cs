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
using System.Globalization;
using System.Text;
using NLog.Config;
using NLog.Internal;

namespace NLog.LayoutRenderers.Wrappers
{
    /// <summary>
    /// Render a single property of a object
    /// </summary>
    [LayoutRenderer("Object-Path")]
    [AmbientProperty(nameof(ObjectPath))]
    [ThreadSafe]
    [ThreadAgnostic]
    public sealed class ObjectPathRendererWrapper : WrapperLayoutRendererBase, IRawValue
    {
        private ObjectReflectionCache ObjectReflectionCache => _objectReflectionCache ?? (_objectReflectionCache = new ObjectReflectionCache(LoggingConfiguration.GetServiceProvider()));
        private ObjectReflectionCache _objectReflectionCache;
        private ObjectPropertyPath _objectPropertyPath;

        /// <summary>
        /// Gets or sets the object-property-navigation-path for lookup of nested property
        ///
        /// Shortcut for <see cref="ObjectPath"/>
        /// </summary>
        /// <docgen category="Transformation Options" order="10"/>
        public string Path
        {
            get => ObjectPath;
            set => ObjectPath = value;
        }

        /// <summary>
        /// Gets or sets the object-property-navigation-path for lookup of nested property
        /// </summary>
        /// <docgen category="Transformation Options" order="10"/>
        public string ObjectPath
        {
            get => _objectPropertyPath.Value;
            set => _objectPropertyPath.Value = value;
        }

        /// <summary>
        /// Format string for conversion from object to string.
        /// </summary>
        /// <docgen category="Transformation Options" order="100"/>
        public string Format { get; set; }

        /// <summary>
        /// Gets or sets the culture used for rendering. 
        /// </summary>
        /// <docgen category="Transformation Options" order="100"/>
        public CultureInfo Culture { get; set; } = CultureInfo.InvariantCulture;

        /// <inheritdoc />
        protected override string Transform(string text)
        {
            throw new NotSupportedException();
        }

        /// <inheritdoc />
        protected override void RenderInnerAndTransform(LogEventInfo logEvent, StringBuilder builder, int orgLength)
        {
            if (TryGetRawValue(logEvent, out object rawValue))
            {
                var formatProvider = GetFormatProvider(logEvent, Culture);
                builder.AppendFormattedValue(rawValue, Format, formatProvider, ValueFormatter);
            }
        }

        /// <inheritdoc />
        private bool TryGetRawValue(LogEventInfo logEvent, out object value)
        {
            if (Inner != null &&
                Inner.TryGetRawValue(logEvent, out var rawValue) &&
                ObjectReflectionCache.TryGetObjectProperty(rawValue, _objectPropertyPath.PathNames, out value))
            {
                return true;
            }

            value = null;
            return false;
        }

        bool IRawValue.TryGetRawValue(LogEventInfo logEvent, out object value) => TryGetRawValue(logEvent, out value);
    }
}
