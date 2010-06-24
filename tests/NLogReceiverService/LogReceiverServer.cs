// 
// Copyright (c) 2004-2010 Jaroslaw Kowalski <jaak@jkowalski.net>
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

namespace NLogReceiverService
{
    using System;
    using System.IO;
    using System.Runtime.Serialization;
    using System.Xml;
    using NLog.LogReceiverService;

    /// <summary>
    /// Sample log receiver server - reads and prints out log messages.
    /// </summary>
    public class LogReceiverServer : ILogReceiverServer
    {
        public void ProcessLogMessages(NLogEvents events)
        {
            Console.WriteLine("ProcessLogMessage:");
            Console.WriteLine("Client: {0}", events.ClientName);
            foreach (var e in events.Events)
            {
                var time = new DateTime(events.BaseTimeUtc + e.TimeDelta, DateTimeKind.Utc);
                Console.WriteLine("#{0} {1} logger: {2} level: {3} msg: {4}", e.Id, time.ToLocalTime(), events.Strings[e.LoggerOrdinal], e.LevelOrdinal, events.Strings[e.MessageOrdinal]);
                for (int i = 0; i < events.LayoutNames.Count; ++i)
                {
                    string stringValue = events.Strings[e.ValueIndexes[i]];
                    Console.WriteLine("  {0} = {1}", events.LayoutNames[i], stringValue);
                }
            }

            string xmlPayload;

            var serializer = new DataContractSerializer(typeof(NLogEvents));
            using (var stringWriter = new StringWriter())
            {
                using (XmlWriter writer = XmlWriter.Create(stringWriter))
                {
                    serializer.WriteObject(writer, events);
                }

                xmlPayload = stringWriter.ToString();
            }

            Console.WriteLine("XML payload: {0} characters {1} events ({2} cpe)", xmlPayload.Length, events.Events.Length, xmlPayload.Length / events.Events.Length);
            Console.WriteLine("{0}", xmlPayload);
        }
    }
}
