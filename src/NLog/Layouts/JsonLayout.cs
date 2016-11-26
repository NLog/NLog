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
    using Config;
    using System.Collections.Generic;
    using System.Text;

    /// <summary>
    /// A specialized layout that renders JSON-formatted events.
    /// </summary>
    [Layout("JsonLayout")]
    [ThreadAgnostic]
    [AppDomainFixedOutput]
    public class JsonLayout : Layout
    {
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
        /// Formats the log event as a JSON document for writing.
        /// </summary>
        /// <param name="logEvent">The log event to be formatted.</param>
        /// <returns>A JSON string representation of the log event.</returns>
        protected override string GetFormattedMessage(LogEventInfo logEvent)
        {
            StringBuilder sb = null;

            //Memory profiling pointed out that using a foreach-loop was allocating
            //an Enumerator. Switching to a for-loop avoids the memory allocation.
            for (int i = 0; i < this.Attributes.Count; i++)
            {
                var attrib = this.Attributes[i];
                string text = attrib.LayoutWrapper.Render(logEvent);
                if (!string.IsNullOrEmpty(text))
                {
                    bool first = sb == null;
                    if (first)
                    {
                        sb = new StringBuilder(attrib.Name.Length + text.Length + 10);
                        sb.Append(SuppressSpaces ? "{" : "{ ");
                    }
                    AppendJsonAttributeValue(attrib, text, sb, first);
                }
            }

            if (this.IncludeAllProperties && logEvent.HasProperties)
            {
                JsonAttribute dynAttrib = null;
                foreach (var prop in logEvent.Properties)
                {
                    //Determine property name
                    string propName = prop.Key.ToString();

                    //Skips properties in the ExcludeProperties list
                    if (this.ExcludeProperties.Contains(propName)) continue;

                    if (dynAttrib == null)
                        dynAttrib = new JsonAttribute();

                    if (prop.Value == null)
                    {
                        dynAttrib.Name = propName;
                        dynAttrib.Encode = false;    // Don't put quotes around null values;
                        dynAttrib.Layout = "null";
                    }
                    else
                    {
                        System.Type objType = prop.Value.GetType();
                        System.TypeCode objTypeCode = System.Type.GetTypeCode(objType);
                        if (objTypeCode == System.TypeCode.Boolean || IsNumeric(objType, objTypeCode))
                        {
                            dynAttrib.Name = propName;
                            dynAttrib.Encode = false;    //Don't put quotes around numbers or boolean values
                            dynAttrib.Layout = string.Concat("${event-properties:item=", propName, "}");
                        }
                        else
                        {
                            dynAttrib.Name = propName;
                            dynAttrib.Encode = true;
                            dynAttrib.Layout = string.Concat("${event-properties:item=", propName, "}");
                        }
                    }

                    string text = dynAttrib.LayoutWrapper.Render(logEvent);
                    if (!string.IsNullOrEmpty(text))
                    {
                        bool first = sb == null;
                        if (first)
                        {
                            sb = new StringBuilder(dynAttrib.Name.Length + text.Length + 10);
                            sb.Append(SuppressSpaces ? "{" : "{ ");
                        }
                        AppendJsonAttributeValue(dynAttrib, text, sb, first);
                    }
                }
            }

            if (sb == null)
            {
                if (!RenderEmptyObject)
                    return string.Empty;
                else
                    return SuppressSpaces ? "{}" : "{  }";
            }
            sb.Append(SuppressSpaces ? "}" : " }");
            return sb.ToString();
        }

        private void AppendJsonAttributeValue(JsonAttribute attrib, string text, StringBuilder sb, bool first)
        {
            if (!first)
            {
                sb.EnsureCapacity(sb.Length + attrib.Name.Length + text.Length + 12);
                sb.Append(',');
                if (!this.SuppressSpaces)
                    sb.Append(' ');
            }

            sb.Append('"');
            sb.Append(attrib.Name);
            sb.Append('"');
            sb.Append(':');
            if (!this.SuppressSpaces)
                sb.Append(' ');

            if (attrib.Encode)
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
                // "\"{0}\":{1}{2}";
                sb.Append(text);
            }
        }

        private bool IsNumeric(System.Type objType, System.TypeCode typeCode)
        {
            if (objType.IsPrimitive && typeCode != System.TypeCode.Object)
            {
                return typeCode != System.TypeCode.Char && typeCode != System.TypeCode.Boolean;
            }
            else if (typeCode == System.TypeCode.Decimal)
            {
                return true;
            }
            return false;
        }
    }
}