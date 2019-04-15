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

using System.Collections.Generic;
using System.IO;
using System.Xml;
using NLog.Common;
using NLog.Layouts;

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

        public override int GetHashCode() => (SourcePath ?? "").GetHashCode();
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

        public override int GetHashCode() => (SourcePath ?? "").GetHashCode();
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
            if (!string.IsNullOrEmpty(SourcePath))
            {
                var fileName = SourcePath.Trim();
#if __ANDROID__
                //suport loading config from special assets folder in nlog.config
                if (fileName.StartsWith(AssetsPrefix, StringComparison.OrdinalIgnoreCase))
                {
                    //remove prefix
                    fileName = fileName.Substring(AssetsPrefix.Length);
                    Stream stream = Android.App.Application.Context.Assets.Open(fileName);
                    return XmlReader.Create(stream);
                }
#endif
                return XmlReader.Create(fileName);
            }
            return null;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(this, obj))
                return true;
            if (!(obj is XmlFileConfigurationSource that))
                return false;

            return this.SourcePath == that.SourcePath;
        }

        public override int GetHashCode() => (SourcePath ?? "").GetHashCode();

        public static IEnumerable<IXmlConfigurationSource> IncludeFromPath(string includedFileName, IXmlConfigurationSource parentConfiguration, bool ignoreErrors)
        {
            bool autoReloadDefault = false;

            var fullNewFileName = includedFileName;
            if (parentConfiguration.LocalFolder != null)
            {
                fullNewFileName = Path.Combine(parentConfiguration.LocalFolder, includedFileName);
            }

#if SILVERLIGHT && !WINDOWS_PHONE
            newFileName = newFileName.Replace("\\", "/");
            if (Application.GetResourceStream(new Uri(fullNewFileName, UriKind.Relative)) != null)
#else
            if (File.Exists(fullNewFileName))
#endif
            {
                InternalLogger.Debug("Including file '{0}'", fullNewFileName);
                return new[] {new XmlFileConfigurationSource(fullNewFileName, autoReloadDefault)};
            }

            if (includedFileName.Contains("*"))
            {
                return ConfigureFromFilesByMask(parentConfiguration.LocalFolder, includedFileName, autoReloadDefault);
            }
            else
            {
                if (ignoreErrors)
                {
                    //quick stop for performances
                    InternalLogger.Debug("Skipping included file '{0}' as it can't be found", fullNewFileName);
                    return null;
                }

                throw new FileNotFoundException("Included file not found: " + fullNewFileName);
            }
        }

        /// <summary>
        /// Include (multiple) files by filemask, e.g. *.nlog
        /// </summary>
        /// <param name="baseDirectory">base directory in case if <paramref name="fileMask"/> is relative</param>
        /// <param name="fileMask">relative or absolute fileMask</param>
        /// <param name="autoReloadDefault"></param>
        private static IEnumerable<IXmlConfigurationSource> ConfigureFromFilesByMask(string baseDirectory, string fileMask, bool autoReloadDefault)
        {
            var directory = baseDirectory;

            //if absolute, split to filemask and directory.
            if (Path.IsPathRooted(fileMask))
            {
                directory = Path.GetDirectoryName(fileMask);
                if (directory == null)
                {
                    InternalLogger.Warn("directory is empty for include of '{0}'", fileMask);
                    yield break;
                }

                var filename = Path.GetFileName(fileMask);

                if (filename == null)
                {
                    InternalLogger.Warn("filename is empty for include of '{0}'", fileMask);
                    yield break;
                }
                fileMask = filename;
            }

#if SILVERLIGHT && !WINDOWS_PHONE
            var files = Directory.EnumerateFiles(directory, fileMask);
#else
            var files = Directory.GetFiles(directory, fileMask);
#endif
            foreach (var file in files)
            {
                //note we exclude ourself in ConfigureFromFile
                yield return new XmlFileConfigurationSource(file, autoReloadDefault);
            }
        }

    }
}