//
// Copyright (c) 2004-2024 Jaroslaw Kowalski <jaak@jkowalski.net>, Kim Christensen, Julian Verdurmen
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
    using NLog.Config;

    /// <summary>
    /// Splunk JSON-based, structured log format for Splunk Log Management.
    /// </summary>
    /// <remarks>
    /// <a href="https://github.com/NLog/NLog/wiki/SplunkLayout">See NLog Wiki</a>
    /// </remarks>
    /// <seealso href="https://github.com/NLog/NLog/wiki/SplunkLayout">Documentation on NLog Wiki</seealso>
    /// <example><para>
    /// {
    ///   "time": 1426279439, // epoch time
    ///   "host": "localhost",
    ///   "source": "random-data-generator",
    ///   "sourcetype": "my_sample_data",
    ///   "index": "main",
    ///   "event": { 
    ///     "message": "Something happened",
    ///     "level": "Info"
    ///   }
    /// }
    /// </para></example>
    [Layout("SplunkLayout")]
    [ThreadAgnostic]
    [AppDomainFixedOutput]
    public class SplunkLayout : JsonLayout
    {
        private static readonly DateTime UnixDateStart = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        /// <summary>
        /// Gets the array of attributes for the "event"-section
        /// </summary>
        [ArrayParameter(typeof(JsonAttribute), "splunkevent")]
        public IList<JsonAttribute>? SplunkEvents
        {
            get
            {
                var index = LookupNamedAttributeIndex("event");
                return index >= 0 ? (Attributes[index]?.Layout as JsonLayout)?.Attributes : null;
            }
        }

        /// <summary>
        /// Gets or sets Splunk Message Host-attribute
        /// </summary>
        public Layout SplunkHostName
        {
            get
            {
                var index = LookupNamedAttributeIndex("host");
                return index >= 0 ? Attributes[index].Layout : Layout.Empty;
            }
            set
            {
                var index = LookupNamedAttributeIndex("host");
                if (index >= 0)
                    Attributes[index].Layout = value;
            }
        }

        /// <summary>
        /// Gets or sets Splunk Message Source-attribute. Example the name of the application.
        /// </summary>
        public Layout SplunkSourceName
        {
            get
            {
                var index = LookupNamedAttributeIndex("source");
                return index >= 0 ? Attributes[index].Layout : Layout.Empty;
            }
            set
            {
                var index = LookupNamedAttributeIndex("source");
                if (index >= 0)
                    Attributes[index].Layout = value;
            }
        }

        /// <summary>
        /// Gets or sets Splunk Message SourceType-attribute. SourceType can be used hint for choosing Splunk Indexer
        /// </summary>
        public Layout SplunkSourceType
        {
            get
            {
                var index = LookupNamedAttributeIndex("sourcetype");
                return index >= 0 ? Attributes[index].Layout : Layout.Empty;
            }
            set
            {
                var index = LookupNamedAttributeIndex("sourcetype");
                if (index >= 0)
                    Attributes[index].Layout = value;
            }
        }

        /// <summary>
        /// Gets or sets Splunk Message Index-attribute, that controls which event data is to be indexed.
        /// </summary>
        public Layout SplunkIndex
        {
            get
            {
                var index = LookupNamedAttributeIndex("index");
                return index >= 0 ? Attributes[index].Layout : Layout.Empty;
            }
            set
            {
                var index = LookupNamedAttributeIndex("index");
                if (index >= 0)
                    Attributes[index].Layout = value;
            }
        }

        /// <summary>
        /// Gets or sets the option to include all properties from the log events
        /// </summary>
        public new bool IncludeEventProperties
        {
            get
            {
                var index = LookupNamedAttributeIndex("event");
                return index >= 0 && (Attributes[index].Layout as JsonLayout)?.IncludeEventProperties == true;
            }
            set
            {
                var index = LookupNamedAttributeIndex("event");
                if (index >= 0 && Attributes[index].Layout is JsonLayout jsonLayout)
                    jsonLayout.IncludeEventProperties = value;
            }
        }

        /// <summary>
        /// Gets or sets whether to include the contents of the <see cref="ScopeContext"/> properties-dictionary.
        /// </summary>
        public new bool IncludeScopeProperties
        {
            get
            {
                var index = LookupNamedAttributeIndex("event");
                return index >= 0 && (Attributes[index].Layout as JsonLayout)?.IncludeScopeProperties == true;
            }
            set
            {
                var index = LookupNamedAttributeIndex("event");
                if (index >= 0 && Attributes[index].Layout is JsonLayout jsonLayout)
                    jsonLayout.IncludeScopeProperties = value;
            }
        }

        /// <summary>
        /// Gets or sets the option to exclude null/empty properties from the log event (as JSON)
        /// </summary>
        public new bool ExcludeEmptyProperties
        {
            get
            {
                var index = LookupNamedAttributeIndex("event");
                return index >= 0 && (Attributes[index].Layout as JsonLayout)?.ExcludeEmptyProperties == true;
            }
            set
            {
                var index = LookupNamedAttributeIndex("event");
                if (index >= 0 && Attributes[index].Layout is JsonLayout jsonLayout)
                    jsonLayout.ExcludeEmptyProperties = value;
            }
        }

        /// <summary>
        /// List of property names to exclude when <see cref="IncludeEventProperties"/> is <see langword="true"/>
        /// </summary>
#if NET35
        public new HashSet<string>? ExcludeProperties
#else
        public new ISet<string>? ExcludeProperties
#endif
        {
            get
            {
                var index = LookupNamedAttributeIndex("event");
                return index >= 0 && Attributes[index].Layout is JsonLayout jsonLayout ? jsonLayout.ExcludeProperties : null;
            }
            set
            {
                var index = LookupNamedAttributeIndex("event");
                if (index >= 0 && Attributes[index].Layout is JsonLayout jsonLayout && value != null)
                    jsonLayout.ExcludeProperties = value;
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SplunkLayout"/> class.
        /// </summary>
        public SplunkLayout()
        {
            Attributes.Add(new JsonAttribute("time", Layout.FromMethod((evt) => ToUnixTimeStamp(evt.TimeStamp))) { Encode = false });
            Attributes.Add(new JsonAttribute("host", "${hostname}"));
            Attributes.Add(new JsonAttribute("source", "${processname}"));
            Attributes.Add(new JsonAttribute("sourcetype", Layout.Empty));
            Attributes.Add(new JsonAttribute("index", Layout.Empty));
            Attributes.Add(new JsonAttribute("event", new JsonLayout() { IncludeEventProperties = true }) { Encode = false });
        }

        /// <inheritdoc/>
        protected override void InitializeLayout()
        {
            var index = LookupNamedAttributeIndex("event");
            if (index >= 0 && Attributes[index].Layout is JsonLayout jsonLayout && jsonLayout.Attributes.Count == 0)
            {
                jsonLayout.Attributes.Add(new JsonAttribute("level", "${level}"));
                jsonLayout.Attributes.Add(new JsonAttribute("message", "${message}"));
                jsonLayout.Attributes.Add(new JsonAttribute("logger", "${logger}"));
                jsonLayout.Attributes.Add(new JsonAttribute("exception_type", "${exception:Format=Type}"));
                jsonLayout.Attributes.Add(new JsonAttribute("exception_msg", "${exception:Format=Message}"));
                jsonLayout.Attributes.Add(new JsonAttribute("exception", "${exception:Format=ToString}"));
            }

            base.InitializeLayout();
        }

        internal static decimal ToUnixTimeStamp(DateTime timeStamp)
        {
            return Convert.ToDecimal(timeStamp.ToUniversalTime().Subtract(UnixDateStart).TotalSeconds);
        }

        private int LookupNamedAttributeIndex(string attributeName)
        {
            for (int i = 0; i < Attributes.Count; ++i)
            {
                if (attributeName.Equals(Attributes[i].Name, StringComparison.OrdinalIgnoreCase))
                {
                    return i;
                }
            }
            return -1;
        }
    }
}
