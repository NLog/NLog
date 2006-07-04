// 
// Copyright (c) 2004,2005 Jaroslaw Kowalski <jkowalski@users.sourceforge.net>
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
using System.Net;
using System.Net.Sockets;
using System.Xml;
using System.IO;
using System.Xml.Serialization;
using System.Windows.Forms;
using System.Collections.Specialized;

using NLogViewer.Configuration;
using NLogViewer.Events;
using NLogViewer.Parsers;
using System.ComponentModel;
using System.Drawing.Design;
using System.Windows.Forms.Design;
using NLogViewer.Receivers.UI;

namespace NLogViewer.Receivers
{
    [LogEventReceiver("FILE", "File Receiver", "Reads from a file")]
    public class FileReceiver : LogEventReceiverWithParserSkeleton, IWizardConfigurable
    {
        private string _fileName;
        private bool _monitorChanges = true;

        public FileReceiver()
        {
        }

        [Editor(typeof(FileNameEditor), typeof(UITypeEditor))]
        public string FileName
        {
            get { return _fileName; }
            set { _fileName = value; }
        }

        public bool MonitorChanges
        {
            get { return _monitorChanges; }
            set { _monitorChanges = value; }
        }

        public override void InputThread()
        {
            try
            {
                using (FileStream stream = new FileStream(FileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                {
                    using (ILogEventParserInstance parserInstance = Parser.Begin(stream))
                    {
                        while (!InputThreadQuitRequested())
                        {
                            long startingPos = stream.Position;
                            LogEvent logEventInfo = parserInstance.ReadNext();
                            if (logEventInfo == null)
                                break;
                            EventReceived(logEventInfo);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }

        public IWizardPage GetWizardPage()
        {
            return new FileReceiverPropertyPage(this);
        }
    }
}
