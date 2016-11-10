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

using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using NLog.Config;

namespace NLog.LayoutRenderers
{
    using System;
    using System.Globalization;
    using System.Text;

    /// <summary>
    /// Log event context data.
    /// </summary>
    [LayoutRenderer("all-event-properties")]
    [ThreadAgnostic]
    public class AllEventPropertiesLayoutRenderer : LayoutRenderer
    {
        private string format;

        /// <summary>
        /// Initializes a new instance of the <see cref="AllEventPropertiesLayoutRenderer"/> class.
        /// </summary>
        public AllEventPropertiesLayoutRenderer()
        {
            this.Separator = ", ";
            this.Format = "[key]=[value]";
        }

        /// <summary>
        /// Gets or sets string that will be used to separate key/value pairs.
        /// </summary>
        /// <docgen category='Rendering Options' order='10' />
        public string Separator { get; set; }

#if NET4_5

        /// <summary>
        /// Also render the caller information attributes? (<see cref="CallerMemberNameAttribute"/>,
        /// <see cref="CallerFilePathAttribute"/>, <see cref="CallerLineNumberAttribute"/>). 
        /// 
        /// See https://msdn.microsoft.com/en-us/library/hh534540.aspx
        /// </summary>
       [DefaultValue(false)] 
       public bool IncludeCallerInformation { get; set; }


#endif

        /// <summary>
        /// Gets or sets how key/value pairs will be formatted.
        /// </summary>
        /// <docgen category='Rendering Options' order='10' />
        public string Format
        {
            get { return format; }
            set
            {
                if (!value.Contains("[key]"))
                    throw new ArgumentException("Invalid format: [key] placeholder is missing.");

                if (!value.Contains("[value]"))
                    throw new ArgumentException("Invalid format: [value] placeholder is missing.");

                format = value;
            }
        }

        /// <summary>
        /// Renders all log event's properties and appends them to the specified <see cref="StringBuilder" />.
        /// </summary>
        /// <param name="builder">The <see cref="StringBuilder"/> to append the rendered data to.</param>
        /// <param name="logEvent">Logging event.</param>
        protected override void Append(StringBuilder builder, LogEventInfo logEvent)
        {
            bool first = true;

            foreach (var property in GetProperties(logEvent))
            {
                if (!first)
                {
                    builder.Append(Separator);
                }

                first = false;

                var formatProvider = GetFormatProvider(logEvent);

                var key = Convert.ToString(property.Key, formatProvider);
                var value = Convert.ToString(property.Value, formatProvider);
                var pair = Format.Replace("[key]", key)
                                 .Replace("[value]", value);

                builder.Append(pair);
            }
        }


        #if NET4_5

        /// <summary>
        /// The names of caller information attributes.
        /// https://msdn.microsoft.com/en-us/library/hh534540.aspx
        /// </summary>
        private static HashSet<string> CallerInformationAttributeNames = new HashSet<string>
        {
            {"CallerMemberName"},
            {"CallerFilePath"},
            {"CallerLineNumber"},
        };

        /// <summary>
        /// Also render the call attributes? (<see cref="CallerMemberNameAttribute"/>,
        /// <see cref="CallerFilePathAttribute"/>, <see cref="CallerLineNumberAttribute"/>). 
        /// </summary>
        /// 
#endif


        private IDictionary<object, object> GetProperties(LogEventInfo logEvent)
        {
#if NET4_5

            if (this.IncludeCallerInformation)
            {
                return logEvent.Properties; 
            }
            //filter CallerInformationAttributeNames
            return logEvent.Properties.Where(p => !CallerInformationAttributeNames.Contains(p.Key)).ToDictionary(p => p.Key, p => p.Value);

#else

            return logEvent.Properties;
#endif
        }
    }
}