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

namespace NLog.Layouts
{
    using System;
    using System.Collections.Generic;
    using System.Text;
    using NLog.Config;

    /// <summary>
    /// A specialized layout that renders JSON-formatted events.
    /// </summary>
    [Layout("JsonLayout")]
    [ThreadAgnostic]
    [AppDomainFixedOutput]
    public class JsonLayout : Layout
    {
        private IJsonConverter JsonConverter
        {
            get { return _jsonConverter ?? (_jsonConverter = ConfigurationItemFactory.Default.JsonConverter); }
            set { _jsonConverter = value; }
        }
        private IJsonConverter _jsonConverter = null;

        /// <summary>
        /// Initializes a new instance of the <see cref="JsonLayout"/> class.
        /// </summary>
        public JsonLayout()
        {
            this.Attributes = new List<JsonAttribute>();
            this.RenderEmptyObject = true;
            this.IncludeAllProperties = false;
            this.ExcludeProperties = new HashSet<string>();
        }

        /// <summary>
        /// Gets the array of attributes' configurations.
        /// </summary>
        /// <docgen category='CSV Options' order='10' />
        [ArrayParameter(typeof(JsonAttribute), "attribute")]
        public IList<JsonAttribute> Attributes { get; private set; }

        /// <summary>
        /// Gets or sets the option to suppress the extra spaces in the output json
        /// </summary>
        public bool SuppressSpaces { get; set; }

        /// <summary>
        /// Gets or sets the option to render the empty object value {}
        /// </summary>
        public bool RenderEmptyObject { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to include contents of the <see cref="MappedDiagnosticsContext"/> dictionary.
        /// </summary>
        public bool IncludeMdc { get; set; }

#if NET4_0 || NET4_5
        /// <summary>
        /// Gets or sets a value indicating whether to include contents of the <see cref="MappedDiagnosticsLogicalContext"/> dictionary.
        /// </summary>
        public bool IncludeMdlc { get; set; }
#endif

        /// <summary>
        /// Gets or sets the option to include all properties from the log events
        /// </summary>
        public bool IncludeAllProperties { get; set; }

        /// <summary>
        /// List of property names to exclude when <see cref="IncludeAllProperties"/> is true
        /// </summary>
#if NET3_5
        public HashSet<string> ExcludeProperties { get; set; }
#else
        public ISet<string> ExcludeProperties { get; set; }
#endif

        /// <summary>
        /// Initializes the layout.
        /// </summary>
        protected override void InitializeLayout()
        {
            base.InitializeLayout();
            if (IncludeMdc)
            {
                base.ThreadAgnostic = false;
            }
#if NET4_0 || NET4_5
            if (IncludeMdlc)
            {
                base.ThreadAgnostic = false;
            }
#endif
        }

        /// <summary>
        /// Closes the layout.
        /// </summary>
        protected override void CloseLayout()
        {
            JsonConverter = null;
            base.CloseLayout();
        }

        /// <summary>
        /// Formats the log event as a JSON document for writing.
        /// </summary>
        /// <param name="logEvent">The logging event.</param>
        /// <param name="target">Initially empty <see cref="StringBuilder"/> for the result</param>
        protected override void RenderFormattedMessage(LogEventInfo logEvent, StringBuilder target)
        {
            RenderJsonFormattedMessage(logEvent, target);
            if (target.Length == 0 && RenderEmptyObject)
            {
                target.Append(SuppressSpaces ? "{}" : "{  }");
            }
        }

        /// <summary>
        /// Formats the log event as a JSON document for writing.
        /// </summary>
        /// <param name="logEvent">The log event to be formatted.</param>
        /// <returns>A JSON string representation of the log event.</returns>
        protected override string GetFormattedMessage(LogEventInfo logEvent)
        {
            return RenderAllocateBuilder(logEvent);
        }

        private void RenderJsonFormattedMessage(LogEventInfo logEvent, StringBuilder sb)
        {
            //Memory profiling pointed out that using a foreach-loop was allocating
            //an Enumerator. Switching to a for-loop avoids the memory allocation.
            for (int i = 0; i < this.Attributes.Count; i++)
            {
                var attrib = this.Attributes[i];
                string text = attrib.LayoutWrapper.Render(logEvent);
                if (!string.IsNullOrEmpty(text))
                {
                    AppendJsonAttributeValue(attrib.Name, attrib.Encode, text, sb);
                }
            }

            if (this.IncludeMdc)
            {
                foreach (string key in MappedDiagnosticsContext.GetNames())
                {
                    if (string.IsNullOrEmpty(key))
                        continue;
                    object propertyValue = MappedDiagnosticsContext.GetObject(key);
                    AppendJsonPropertyValue(key, propertyValue, sb);
                }
            }

#if NET4_0 || NET4_5
            if (this.IncludeMdlc)
            {
                foreach (string key in MappedDiagnosticsLogicalContext.GetNames())
                {
                    if (string.IsNullOrEmpty(key))
                        continue;
                    object propertyValue = MappedDiagnosticsLogicalContext.GetObject(key);
                    AppendJsonPropertyValue(key, propertyValue, sb);
                }
            }
#endif

            if (this.IncludeAllProperties && logEvent.HasProperties)
            {
                foreach (var prop in logEvent.Properties)
                {
                    //Determine property name
                    string propName = Internal.XmlHelper.XmlConvertToString(prop.Key ?? string.Empty);
                    if (string.IsNullOrEmpty(propName))
                        continue;

                    //Skips properties in the ExcludeProperties list
                    if (this.ExcludeProperties.Contains(propName))
                        continue;

                    AppendJsonPropertyValue(propName, prop.Value, sb);
                }
            }

            CompleteJsonMessage(sb);
        }

        private void BeginJsonProperty(StringBuilder sb, string propName)
        {
            bool first = sb.Length == 0;
            if (first)
            {
                sb.Append(SuppressSpaces ? "{" : "{ ");
            }
            else
            {
                sb.Append(',');
                if (!this.SuppressSpaces)
                    sb.Append(' ');
            }

            sb.Append('"');
            sb.Append(propName);
            sb.Append('"');
            sb.Append(':');
            if (!this.SuppressSpaces)
                sb.Append(' ');
        }

        private void CompleteJsonMessage(StringBuilder sb)
        {
            if (sb.Length > 0)
                sb.Append(SuppressSpaces ? "}" : " }");
        }

        private void AppendJsonPropertyValue(string propName, object propertyValue, StringBuilder sb)
        {
            BeginJsonProperty(sb, propName);
            JsonConverter.SerializeObject(propertyValue, sb);
        }

        private void AppendJsonAttributeValue(string attributeName, bool attributeQuote, string text, StringBuilder sb)
        {
            BeginJsonProperty(sb, attributeName);

            if (attributeQuote)
            {
                // "\"{0}\":{1}\"{2}\""
                sb.Append('"');
                sb.Append(text);
                sb.Append('"');
            }
            else
            {
                //If encoding is disabled for current attribute, do not escape the value of the attribute.
                //This enables user to write arbitrary string value (including JSON).
                // "\"{0}\":{1}{2}"
                sb.Append(text);
            }
        }
    }
}