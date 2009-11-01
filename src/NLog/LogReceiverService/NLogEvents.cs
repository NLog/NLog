// 
// Copyright (c) 2004-2009 Jaroslaw Kowalski <jaak@jkowalski.net>
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
#if !NET_CF
    [DebuggerDisplay("Count = {Events.Length}")]
#endif
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
        public ListOfStrings LayoutNames { get; set; }

        /// <summary>
        /// Gets or sets the collection of logger names.
        /// </summary>
        /// <value>The logger names.</value>
#if WCF_SUPPORTED
        [DataMember(Name = "lg", Order = 200)]
#endif
        [XmlArray("lg", Order = 200)]
        [XmlArrayItem("l")]
        public ListOfStrings LoggerNames { get; set; }

        /// <summary>
        /// Gets or sets the list of events.
        /// </summary>
        /// <value>The events.</value>
#if WCF_SUPPORTED
        [DataMember(Name = "ev", Order = 1000)]
#endif
        [XmlArray("ev", Order = 1000)]
        [XmlArrayItem("e")]
        public NLogEvent[] Events { get; set; }
    }
}
