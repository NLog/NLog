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
    using System.ComponentModel;
    using NLog.Config;
    using NLog.Layouts;

    /// <summary>
    /// A parameter to MethodCall.
    /// </summary>
    [NLogConfigurationItem]
    public class MethodCallParameter
    {
        private readonly ValueTypeLayoutInfo _layoutInfo = new ValueTypeLayoutInfo();

        /// <summary>
        /// Initializes a new instance of the <see cref="MethodCallParameter" /> class.
        /// </summary>
        public MethodCallParameter()
        {
            ParameterType = typeof(string);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MethodCallParameter" /> class.
        /// </summary>
        /// <param name="layout">The layout to use for parameter value.</param>
        public MethodCallParameter(Layout layout)
        {
            ParameterType = typeof(string);
            Layout = layout;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MethodCallParameter" /> class.
        /// </summary>
        /// <param name="parameterName">Name of the parameter.</param>
        /// <param name="layout">The layout.</param>
        public MethodCallParameter(string parameterName, Layout layout)
        {
            ParameterType = typeof(string);
            Name = parameterName;
            Layout = layout;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MethodCallParameter" /> class.
        /// </summary>
        /// <param name="name">The name of the parameter.</param>
        /// <param name="layout">The layout.</param>
        /// <param name="type">The type of the parameter.</param>
        public MethodCallParameter(string name, Layout layout, Type type)
        {
            ParameterType = type;
            Name = name;
            Layout = layout;
        }

        /// <summary>
        /// Gets or sets the name of the parameter.
        /// </summary>
        /// <docgen category='Parameter Options' order='1' />
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the layout that should be use to calculate the value for the parameter.
        /// </summary>
        /// <docgen category='Parameter Options' order='10' />
        [RequiredParameter]
        public Layout Layout { get => _layoutInfo.Layout; set => _layoutInfo.Layout = value; }

        /// <summary>
        /// Obsolete and replaced by <see cref="ParameterType"/> with NLog v4.6.
        /// Gets or sets the type of the parameter. Obsolete alias for <see cref="ParameterType"/>
        /// </summary>
        /// <docgen category='Parameter Options' order='50' />
        [Obsolete("Use property ParameterType instead. Marked obsolete on NLog 4.6")]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public Type Type { get => ParameterType; set => ParameterType = value; }

        /// <summary>
        /// Gets or sets the type of the parameter. 
        /// </summary>
        /// <docgen category='Parameter Options' order='50' />
        public Type ParameterType { get => _layoutInfo.ValueType ?? typeof(string); set => _layoutInfo.ValueType = value; }

        /// <summary>
        /// Gets or sets the fallback value when result value is not available
        /// </summary>
        /// <docgen category='Parameter Options' order='50' />
        public Layout DefaultValue { get => _layoutInfo.DefaultValue; set => _layoutInfo.DefaultValue = value; }

        /// <summary>
        /// Render Result Value
        /// </summary>
        /// <param name="logEvent">Log event for rendering</param>
        /// <returns>Result value when available, else fallback to defaultValue</returns>
        public object RenderValue(LogEventInfo logEvent) => _layoutInfo.RenderValue(logEvent);
    }
}
