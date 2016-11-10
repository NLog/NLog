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
    using Config;
    using LayoutRenderers.Wrappers;
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
        /// Formats the log event as a JSON document for writing.
        /// </summary>
        /// <param name="logEvent">The log event to be formatted.</param>
        /// <returns>A JSON string representation of the log event.</returns>
        protected override string GetFormattedMessage(LogEventInfo logEvent)
        {
            var jsonWrapper = new JsonEncodeLayoutRendererWrapper();
            var sb = new StringBuilder();
            bool first = true;
            bool hasContent = false;

            //Memory profiling pointed out that using a foreach-loop was allocating
            //an Enumerator. Switching to a for-loop avoids the memory allocation.
            for (int i = 0; i < this.Attributes.Count; i++)
            {
                var col = this.Attributes[i];
                jsonWrapper.Inner = col.Layout;
                jsonWrapper.JsonEncode = col.Encode;
                string text = jsonWrapper.Render(logEvent);

                if (!string.IsNullOrEmpty(text))
                {
                    if (!first)
                    {
                        sb.Append(",");
                        AppendIf(!this.SuppressSpaces, sb, " ");
                    }

                    first = false;

                    string format;

                    if(col.Encode)
                    {
                        format = "\"{0}\":{1}\"{2}\"";
                    }
                    else
                    {
                        //If encoding is disabled for current attribute, do not escape the value of the attribute.
                        //This enables user to write arbitrary string value (including JSON).
                        format = "\"{0}\":{1}{2}";
                    }

                    sb.AppendFormat(format, col.Name, !this.SuppressSpaces ? " " : "", text);
                    hasContent = true;
                }
            }

            var result = sb.ToString();

            if (!hasContent && !RenderEmptyObject)
            {
               return string.Empty;
            }

            if (SuppressSpaces)
            {
                return "{" + result + "}";
            }
            return "{ " + result + " }";
        }

        private static void AppendIf<T>(bool condition, StringBuilder stringBuilder, T objectToAppend)
        {
            if (condition)
            {
                stringBuilder.Append(objectToAppend);
            }
        }
    }
}