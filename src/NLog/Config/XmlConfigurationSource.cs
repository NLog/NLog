// 
// Copyright (c) 2004-2019 Jaroslaw Kowalski <jaak@jkowalski.net>, Kim Christensen, Julian Verdurmen
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

using System.IO;
using System.Xml;

namespace NLog.Config
{
    internal interface IXmlConfigurationSource
    {
        string SourcePath { get; }
        string LocalFolder { get; }
        XmlReader GetReader();
        bool AutoReload { get; set; }
    }

    internal class XmlReaderConfigurationSource : IXmlConfigurationSource
    {
        private readonly XmlReader _reader;

        internal XmlReaderConfigurationSource(XmlReader reader, string fileName)
        {
            _reader = reader;

            SourcePath = fileName;
            LocalFolder = Path.GetDirectoryName(fileName);
        }

        public string SourcePath { get; }
        public string LocalFolder { get; }
        public bool AutoReload { get; set; }

        public XmlReader GetReader() => _reader;

        public override int GetHashCode() => SourcePath.GetHashCode();
    }

    internal class XmlStringConfigurationSource : IXmlConfigurationSource
    {
        private readonly string _xmlContents;

        internal XmlStringConfigurationSource(string xmlContents, string fileName)
        {
            _xmlContents = xmlContents;

            SourcePath = fileName;
            LocalFolder = Path.GetDirectoryName(fileName);
        }

        public string SourcePath { get; }
        public string LocalFolder { get; }
        public bool AutoReload { get; set; }

        public XmlReader GetReader()
        {
            var stringReader = new StringReader(_xmlContents);
            return XmlReader.Create(stringReader, new XmlReaderSettings() { CloseInput = true });
        }
        public override int GetHashCode() => _xmlContents.GetHashCode();
    }

    internal class XmlFileConfigurationSource : IXmlConfigurationSource
    {
        internal XmlFileConfigurationSource(string fileName, bool reload = false)
        {
            SourcePath = fileName;
            LocalFolder = Path.GetDirectoryName(fileName);
            AutoReload = reload;
        }

        public string SourcePath { get; set; }
        public string LocalFolder { get; set; }
        public bool AutoReload { get; set; }

        public XmlReader GetReader()
        {
            return XmlReader.Create(SourcePath);
        }
        public override int GetHashCode() => SourcePath.GetHashCode();
    }
}