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

using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using NLog.Internal.Fakeables;

namespace NLog.UnitTests.Mocks
{
    internal sealed class AppEnvironmentMock : IAppEnvironment
    {
        private readonly Func<string, bool> _fileexists;
        private readonly Func<string, XmlReader> _loadXmlFile;
        private readonly Func<string, TextReader> _loadTextFile;

        public AppEnvironmentMock(Func<string, bool> fileExists = null, Func<string, XmlReader> loadXmlFile = null, Func<string, TextReader> loadTextFile = null)
        {
            _fileexists = fileExists != null ? fileExists : (f) => throw new NotSupportedException("FileSystem unavailable");
            _loadXmlFile = loadXmlFile != null ? loadXmlFile : (f) => throw new NotSupportedException("FileSystem unavailable");
            _loadTextFile = loadTextFile != null ? loadTextFile : (f) => throw new NotSupportedException("FileSystem unavailable");
        }

        public int AppDomainId { get; set; }

        public string AppDomainFriendlyName { get; set; } = nameof(AppEnvironmentMock);

        public string AppDomainBaseDirectory { get; set; } = string.Empty;

        public string AppDomainConfigurationFile { get; set; } = string.Empty;

        public string CurrentProcessFilePath { get; set; } = string.Empty;

        public string CurrentProcessBaseName { get; set; } = string.Empty;

        public int CurrentProcessId { get; set; } = 1;

        public string EntryAssemblyLocation { get; set; } = string.Empty;

        public string EntryAssemblyFileName { get; set; } = string.Empty;

        public string UserTempFilePath { get; set; } = string.Empty;

        public IEnumerable<string> AppDomainPrivateBinPath { get; set; } = NLog.Internal.ArrayHelper.Empty<string>();

        public IEnumerable<System.Reflection.Assembly> GetAppDomainRuntimeAssemblies() => NLog.Internal.ArrayHelper.Empty<System.Reflection.Assembly>();


        public event EventHandler ProcessExit;

        public bool FileExists(string path)
        {
            return _fileexists(path);
        }

        public XmlReader LoadXmlFile(string path)
        {
            return _loadXmlFile(path);
        }

        public TextReader LoadTextFile(string path)
        {
            return _loadTextFile(path);
        }

        public void SignalShutdown()
        {
            ProcessExit?.Invoke(this, EventArgs.Empty);
            if (ProcessExit != null)
                throw new InvalidOperationException("Shutdown failed"); // LogFactory should unregister on shutdown
        }
    }
}
