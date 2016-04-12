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

namespace NLog.Targets
{
    using System;
    using System.Globalization;
    using NLog.Config;
    using NLog.Layouts;

    /// <summary>
    /// A parameter to MethodCall.
    /// </summary>
    [NLogConfigurationItem]
    public class MethodCallParameter
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MethodCallParameter" /> class.
        /// </summary>
        public MethodCallParameter()
        {
            this.Type = typeof(string);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MethodCallParameter" /> class.
        /// </summary>
        /// <param name="layout">The layout to use for parameter value.</param>
        public MethodCallParameter(Layout layout)
        {
            this.Type = typeof(string);
            this.Layout = layout;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MethodCallParameter" /> class.
        /// </summary>
        /// <param name="parameterName">Name of the parameter.</param>
        /// <param name="layout">The layout.</param>
        public MethodCallParameter(string parameterName, Layout layout)
        {
            this.Type = typeof(string);
            this.Name = parameterName;
            this.Layout = layout;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MethodCallParameter" /> class.
        /// </summary>
        /// <param name="name">The name of the parameter.</param>
        /// <param name="layout">The layout.</param>
        /// <param name="type">The type of the parameter.</param>
        public MethodCallParameter(string name, Layout layout, Type type)
        {
            this.Type = type;
            this.Name = name;
            this.Layout = layout;
        }

        /// <summary>
        /// Gets or sets the name of the parameter.
        /// </summary>
        /// <docgen category='Parameter Options' order='10' />
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the type of the parameter.
        /// </summary>
        /// <docgen category='Parameter Options' order='10' />
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1721:PropertyNamesShouldNotMatchGetMethods", Justification = "Backwards compatibility")]
        public Type Type { get; set; }

        /// <summary>
        /// Gets or sets the layout that should be use to calculate the value for the parameter.
        /// </summary>
        /// <docgen category='Parameter Options' order='10' />
        [RequiredParameter]
        public Layout Layout { get; set; }

        internal object GetValue(LogEventInfo logEvent)
        {
            return Convert.ChangeType(this.Layout.Render(logEvent), this.Type, CultureInfo.InvariantCulture);
        }
    }
}
