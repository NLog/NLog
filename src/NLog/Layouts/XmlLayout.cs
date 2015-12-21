// 
// Copyright (c) 2004-2011 Jaroslaw Kowalski <jaak@jkowalski.net>
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
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Xml;
using NLog.Config;
using NLog.Internal;
using NLog.LayoutRenderers;

namespace NLog.Layouts
{
    /// <summary>
    /// An Xml Layout for NLog.
    /// </summary>
    [Layout("XmlLayout")]
    public class XmlLayout : Layout
    {
        private readonly IList<XmlProperty> _properties;

        /// <summary>
        /// Initializes a new instance of the <see cref="XmlLayout"/> class.
        /// </summary>
        public XmlLayout()
        {
            _properties = new List<XmlProperty>();
            Indent = true;
            OmitXmlDeclaration = true;
        }


        /// <summary>
        /// Gets the custom properties for the log event.
        /// </summary>
        [ArrayParameter(typeof (XmlProperty), "property")]
        public IList<XmlProperty> Properties
        {
            get { return _properties; }
        }


        /// <summary>
        /// Gets or sets a value indicating whether to indent the XML elements.
        /// </summary>
        /// <value>
        ///   <c>true</c> if indent; otherwise, <c>false</c>.
        /// </value>
        [DefaultValue(true)]
        public bool Indent { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to omit an XML declaration.
        /// </summary>
        /// <value>
        ///   <c>true</c> to omit XML declaration; otherwise, <c>false</c>.
        /// </value>
        [DefaultValue(true)]
        public bool OmitXmlDeclaration { get; set; }

        /// <summary>
        /// Renders the layout for the specified logging event by invoking layout renderers.
        /// </summary>
        /// <param name="logEvent">The logging event.</param>
        /// <returns>
        /// The rendered layout.
        /// </returns>
        protected override string GetFormattedMessage(LogEventInfo logEvent)
        {
            // add custom properties to log event
            foreach (var xmlProperty in Properties)
            {
                string name = xmlProperty.Name;
                string text = xmlProperty.Layout.Render(logEvent);

                logEvent.Properties[name] =  text;
            }

            var builder = new StringBuilder();
            var writer = new LogEventInfoXmlWriter();
            var settings = new XmlWriterSettings
            {
                Indent = Indent,
                Encoding = Encoding.UTF8,
                OmitXmlDeclaration = OmitXmlDeclaration
            };

            writer.Write(builder, logEvent, settings);

            return builder.ToString();
        }
    }
}
