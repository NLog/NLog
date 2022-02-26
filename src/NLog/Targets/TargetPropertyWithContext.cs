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
    using NLog.Config;
    using NLog.Layouts;

    /// <summary>
    /// Attribute details for <see cref="TargetWithContext"/> 
    /// </summary>
    [NLogConfigurationItem]
    public class TargetPropertyWithContext
    {
        private readonly ValueTypeLayoutInfo _layoutInfo = new ValueTypeLayoutInfo();

        /// <summary>
        /// Initializes a new instance of the <see cref="TargetPropertyWithContext" /> class.
        /// </summary>
        public TargetPropertyWithContext() : this(null, null) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="TargetPropertyWithContext" /> class.
        /// </summary>
        /// <param name="name">The name of the attribute.</param>
        /// <param name="layout">The layout of the attribute's value.</param>
        public TargetPropertyWithContext(string name, Layout layout)
        {
            Name = name;
            Layout = layout;
            IncludeEmptyValue = false;
        }

        /// <summary>
        /// Gets or sets the name of the attribute.
        /// </summary>
        /// <docgen category='Layout Options' order='1' />
        [RequiredParameter]
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the layout that will be rendered as the attribute's value.
        /// </summary>
        /// <docgen category='Layout Options' order='10' />
        [RequiredParameter]
        public Layout Layout { get => _layoutInfo.Layout; set => _layoutInfo.Layout = value; }

        /// <summary>
        /// Gets or sets the type of the property.
        /// </summary>
        /// <docgen category='Layout Options' order='100' />
        public Type PropertyType { get => _layoutInfo.ValueType ?? typeof(string); set => _layoutInfo.ValueType = value; }

        /// <summary>
        /// Gets or sets the fallback value when result value is not available
        /// </summary>
        /// <docgen category='Layout Options' order='100' />
        public Layout DefaultValue { get => _layoutInfo.DefaultValue; set => _layoutInfo.DefaultValue = value; }

        /// <summary>
        /// Gets or sets when an empty value should cause the property to be included
        /// </summary>
        /// <docgen category='Layout Options' order='100' />
        public bool IncludeEmptyValue
        {
            get => _includeEmptyValue;
            set
            {
                _includeEmptyValue = value;
                _layoutInfo.ForceDefaultValueNull = !value;
            }
        }
        private bool _includeEmptyValue;

        /// <summary>
        /// Render Result Value
        /// </summary>
        /// <param name="logEvent">Log event for rendering</param>
        /// <returns>Result value when available, else fallback to defaultValue</returns>
        public object RenderValue(LogEventInfo logEvent) => _layoutInfo.RenderValue(logEvent);
    }
}