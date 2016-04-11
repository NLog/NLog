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

namespace NLog.LogReceiverService
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;

#if WCF_SUPPORTED
    using System.Runtime.Serialization;
    using System.ServiceModel;
#endif
    using System.Xml.Serialization;

    /// <summary>
    /// Wire format for NLog event package.
    /// </summary>
#if WCF_SUPPORTED
    [DataContract(Name = "events", Namespace = LogReceiverServiceConfig.WebServiceNamespace)]
#endif
    [XmlType(Namespace = LogReceiverServiceConfig.WebServiceNamespace)]
    [XmlRoot("events", Namespace = LogReceiverServiceConfig.WebServiceNamespace)]
    [DebuggerDisplay("Count = {Events.Length}")]
    public class NLogEvents
    {
        /// <summary>
        /// Gets or sets the name of the client.
        /// </summary>
        /// <value>The name of the client.</value>
#if WCF_SUPPORTED
        [DataMember(Name = "cli", Order = 0)]
#endif
        [XmlElement("cli", Order = 0)]
        public string ClientName { get; set; }

        /// <summary>
        /// Gets or sets the base time (UTC ticks) for all events in the package.
        /// </summary>
        /// <value>The base time UTC.</value>
#if WCF_SUPPORTED
        [DataMember(Name = "bts", Order = 1)]
#endif
        [XmlElement("bts", Order = 1)]
        public long BaseTimeUtc { get; set; }

        /// <summary>
        /// Gets or sets the collection of layout names which are shared among all events.
        /// </summary>
        /// <value>The layout names.</value>
#if WCF_SUPPORTED
        [DataMember(Name = "lts", Order = 100)]
#endif
        [XmlArray("lts", Order = 100)]
        [XmlArrayItem("l")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly", Justification = "This is needed for serialization.")]
        public StringCollection LayoutNames { get; set; }

        /// <summary>
        /// Gets or sets the collection of logger names.
        /// </summary>
        /// <value>The logger names.</value>
#if WCF_SUPPORTED
        [DataMember(Name = "str", Order = 200)]
#endif
        [XmlArray("str", Order = 200)]
        [XmlArrayItem("l")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly", Justification = "Setter is needed for serialization.")]
        public StringCollection Strings { get; set; }

        /// <summary>
        /// Gets or sets the list of events.
        /// </summary>
        /// <value>The events.</value>
#if WCF_SUPPORTED
        [DataMember(Name = "ev", Order = 1000)]
#endif
        [XmlArray("ev", Order = 1000)]
        [XmlArrayItem("e")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly", Justification = "Setter is needed for serialization.")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays", Justification = "This is for serialization")]
        public NLogEvent[] Events { get; set; }

        /// <summary>
        /// Converts the events to sequence of <see cref="LogEventInfo"/> objects suitable for routing through NLog.
        /// </summary>
        /// <param name="loggerNamePrefix">The logger name prefix to prepend in front of each logger name.</param>
        /// <returns>
        /// Sequence of <see cref="LogEventInfo"/> objects.
        /// </returns>
        public IList<LogEventInfo> ToEventInfo(string loggerNamePrefix)
        {
            var result = new LogEventInfo[this.Events.Length];

            for (int i = 0; i < result.Length; ++i)
            {
                result[i] = this.Events[i].ToEventInfo(this, loggerNamePrefix);
            }

            return result;
        }

        /// <summary>
        /// Converts the events to sequence of <see cref="LogEventInfo"/> objects suitable for routing through NLog.
        /// </summary>
        /// <returns>
        /// Sequence of <see cref="LogEventInfo"/> objects.
        /// </returns>
        public IList<LogEventInfo> ToEventInfo()
        {
            return this.ToEventInfo(string.Empty);
        }
    }
}
