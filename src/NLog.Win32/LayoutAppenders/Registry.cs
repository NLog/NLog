// 
// Copyright (c) 2004 Jaroslaw Kowalski <jaak@polbox.com>
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
using System.Text;
using System.IO;

using Microsoft.Win32;

namespace NLog.Win32.LayoutAppenders
{
    [LayoutAppender("registry")]
    public class RegistryLayoutAppender : LayoutAppender
    {
        private string _value = null;
        private string _defaultValue = null;
        private string _key = null;
        private RegistryKey _rootKey = Registry.LocalMachine;
        private string _subKey = null;

        public string Value
        {
            get { return _value; }
            set { _value = value; }
        }
        
        public string DefaultValue
        {
            get { return _defaultValue; }
            set { _defaultValue = value; }
        }
        
        public string Key
        {
            get { return _key; }
            set
            {
                _key = value; 
                int pos = _key.IndexOfAny(new char[] { '\\','/' });
                if (pos >= 0) {
                    string root = _key.Substring(0, pos);
                    switch (root.ToUpper())
                    {
                        case "HKEY_LOCAL_MACHINE":
                        case "HKLM":
                            _rootKey = Registry.LocalMachine;
                            break;

                        case "HKEY_CURRENT_USER":
                        case "HKCU":
                            _rootKey = Registry.CurrentUser;
                            break;

                        default:
                            throw new ArgumentException("Key name is invalid. Root hive not recognized.");
                    }
                    _subKey = _key.Substring(pos + 1).Replace('/', '\\');
                } else {
                    throw new ArgumentException("Key name is invalid");
                }
            }
        }
        
        public override int GetEstimatedBufferSize(LogEventInfo ev)
        {
            return 32;
        }
        
        public override void Append(StringBuilder builder, LogEventInfo ev)
        {
            using (RegistryKey key = _rootKey.OpenSubKey(_subKey))
            {
                builder.Append(key.GetValue(Value, DefaultValue));
            }
        }
    }
}
