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

#if !NETSTANDARD1_3

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
    [ThreadSafe]
    public class ProcessInfoLayoutRenderer : LayoutRenderer
    {
        private Process _process;
        private ReflectionHelpers.LateBoundMethod _lateBoundPropertyGet;

        /// <summary>
        /// Gets or sets the property to retrieve.
        /// </summary>
        /// <docgen category='Rendering Options' order='10' />
        [DefaultValue("Id"), DefaultParameter]
        public ProcessInfoProperty Property { get; set; } = ProcessInfoProperty.Id;

        /// <summary>
        /// Gets or sets the format-string to use if the property supports it (Ex. DateTime / TimeSpan / Enum)
        /// </summary>
        /// <docgen category='Rendering Options' order='10' />
        [DefaultValue(null)]
        public string Format { get; set; }

        /// <inheritdoc />
        protected override void InitializeLayoutRenderer()
        {
            base.InitializeLayoutRenderer();
            var propertyInfo = typeof(Process).GetProperty(Property.ToString());
            if (propertyInfo == null)
            {
                throw new ArgumentException($"Property '{Property}' not found in System.Diagnostics.Process");
            }

            _lateBoundPropertyGet = ReflectionHelpers.CreateLateBoundMethod(propertyInfo.GetGetMethod());

            _process = Process.GetCurrentProcess();
        }

        /// <inheritdoc />
        protected override void CloseLayoutRenderer()
        {
            if (_process != null)
            {
                _process.Close();
                _process = null;
            }

            base.CloseLayoutRenderer();
        }

        /// <inheritdoc />
        protected override void Append(StringBuilder builder, LogEventInfo logEvent)
        {
            var value = GetValue();
            if (value != null)
            {
                var formatProvider = GetFormatProvider(logEvent);
                builder.AppendFormattedValue(value, Format, formatProvider, ValueFormatter);
            }
        }

        private object GetValue()
        {
            return _lateBoundPropertyGet?.Invoke(_process, null);
        }
    }
}

#endif
