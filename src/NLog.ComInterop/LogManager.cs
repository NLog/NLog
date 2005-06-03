// 
// Copyright (c) 2004 Jaroslaw Kowalski <jkowalski@users.sourceforge.net>
// 
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
// * Neither the name of the Jaroslaw Kowalski nor the names of its 
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
using System.Runtime.InteropServices;

using NLog;
using NLog.Internal;
using NLog.Config;

namespace NLog.ComInterop
{
    /// <summary>
    /// NLog COM Interop LogManager implementation
    /// </summary>
    [ComVisible(true)]
    [ProgId("NLog.LogManager")]
    [Guid("9a7e8d84-72e4-478a-9a05-23c7ef0cfca8")]
    [ClassInterface(ClassInterfaceType.None)]
    public class LogManager: ILogManager
    {
        void ILogManager.LoadConfigFromFile(string fileName)
        {
            NLog.LogManager.Configuration = new XmlLoggingConfiguration(fileName);
        }

        bool ILogManager.InternalLogToConsole
        {
            get { return NLog.Internal.InternalLogger.LogToConsole; }
            set { NLog.Internal.InternalLogger.LogToConsole = value; }
        }

        string ILogManager.InternalLogLevel
        {
            get { return NLog.Internal.InternalLogger.LogLevel.ToString(); }
            set { NLog.Internal.InternalLogger.LogLevel = NLog.Logger.LogLevelFromString(value); }
        }

        string ILogManager.InternalLogFile
        {
            get { return NLog.Internal.InternalLogger.LogFile; }
            set { NLog.Internal.InternalLogger.LogFile = value; }
        }

        ILogger ILogManager.GetLogger(string name)
        {
            ILogger l = new Logger();
            l.LoggerName = name;
            return l;
        }
    }
}
