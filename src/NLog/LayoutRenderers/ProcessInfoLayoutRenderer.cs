// 
// Copyright (c) 2004-2017 Jaroslaw Kowalski <jaak@jkowalski.net>, Kim Christensen, Julian Verdurmen
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

#if !SILVERLIGHT

namespace NLog.LayoutRenderers
{
    using System;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Reflection;
    using System.Text;
    using NLog.Config;
    using NLog.Internal;

    /// <summary>
    /// The information about the running process.
    /// </summary>
    [LayoutRenderer("processinfo")]
    public class ProcessInfoLayoutRenderer : LayoutRenderer
    {
        private Process _process;

        private PropertyInfo _propertyInfo;
        private ReflectionHelpers.LateBoundMethod _lateBoundPropertyGet;

        /// <summary>
        /// Initializes a new instance of the <see cref="ProcessInfoLayoutRenderer" /> class.
        /// </summary>
        public ProcessInfoLayoutRenderer()
        {
            this.Property = ProcessInfoProperty.Id;
        }

        /// <summary>
        /// Gets or sets the property to retrieve.
        /// </summary>
        /// <docgen category='Rendering Options' order='10' />
        [DefaultValue("Id"), DefaultParameter]
        public ProcessInfoProperty Property { get; set; }

        /// <summary>
        /// Gets or sets the format-string to use if the property supports it (Ex. DateTime / TimeSpan / Enum)
        /// </summary>
        /// <docgen category='Rendering Options' order='10' />
        [DefaultValue(null)]
        public string Format { get; set; }

        /// <summary>
        /// Initializes the layout renderer.
        /// </summary>
        protected override void InitializeLayoutRenderer()
        {
            base.InitializeLayoutRenderer();
            this._propertyInfo = typeof(Process).GetProperty(this.Property.ToString());
            if (this._propertyInfo == null)
            {
                throw new ArgumentException("Property '" + this._propertyInfo + "' not found in System.Diagnostics.Process");
            }

            _lateBoundPropertyGet = ReflectionHelpers.CreateLateBoundMethod(this._propertyInfo.GetGetMethod());

            this._process = Process.GetCurrentProcess();
        }

        /// <summary>
        /// Closes the layout renderer.
        /// </summary>
        protected override void CloseLayoutRenderer()
        {
            if (this._process != null)
            {
                this._process.Close();
                this._process = null;
            }

            base.CloseLayoutRenderer();
        }

        /// <summary>
        /// Renders the selected process information.
        /// </summary>
        /// <param name="builder">The <see cref="StringBuilder"/> to append the rendered data to.</param>
        /// <param name="logEvent">Logging event.</param>
        protected override void Append(StringBuilder builder, LogEventInfo logEvent)
        {
            if (this._lateBoundPropertyGet != null)
            {
                var formatProvider = GetFormatProvider(logEvent);
                var value = this._lateBoundPropertyGet(this._process, null);
                builder.AppendFormattedValue(value, this.Format, formatProvider);
            }
        }
    }
}

#endif
