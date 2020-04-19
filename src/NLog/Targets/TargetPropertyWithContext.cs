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
    using NLog.Config;
    using NLog.Layouts;

    /// <summary>
    /// Attribute details for <see cref="TargetWithContext"/> 
    /// </summary>
    [NLogConfigurationItem]
    public class TargetPropertyWithContext
    {
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
        }

        /// <summary>
        /// Gets or sets the name of the attribute.
        /// </summary>
        /// <docgen category='Property Options' order='10' />
        [RequiredParameter]
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the layout that will be rendered as the attribute's value.
        /// </summary>
        /// <docgen category='Property Options' order='10' />
        [RequiredParameter]
        public Layout Layout { get; set; }

        /// <summary>
        /// Gets or sets when an empty value should cause the property to be included
        /// </summary>
        [DefaultValue(true)]
        public bool IncludeEmptyValue { get; set; } = true;

        /// <summary>
        /// Gets or sets the type of the property.
        /// </summary>
        [DefaultValue(typeof(string))]
        public Type PropertyType { get; set; } = typeof(string);
    }
}