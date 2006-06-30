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
using System.Reflection;
using System.Collections.Specialized;
using System.IO;

using System.Collections.Generic;

using NLogViewer.Configuration;
using NLogViewer.Parsers;

namespace NLogViewer.Receivers
{
	public class LogReceiverFactory
	{
        private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();
        private static Dictionary<string, LogEventReceiverInfo> _name2receiver = new Dictionary<string, LogEventReceiverInfo>();
        private static List<LogEventReceiverInfo> _receivers = new List<LogEventReceiverInfo>();

        public static IList<LogEventReceiverInfo> Receivers
        {
            get { return _receivers; }
        }

        static LogReceiverFactory()
        {
            try
            {
                AddReceiversFromAssembly(typeof(LogReceiverFactory).Assembly);
                foreach (string assemblyName in NLogViewerConfiguration.Configuration.ExtensionAssemblies)
                {
                    Assembly extensionAssembly = Assembly.Load(assemblyName);
                    AddReceiversFromAssembly(extensionAssembly);
                }
            }
            catch (Exception)
            {
            }
        }

        public static void AddReceiverInfo(LogEventReceiverInfo ri)
        {
            _name2receiver[ri.Name] = ri;
            _receivers.Add(ri);
        }

        private static void AddReceiversFromAssembly(Assembly assembly)
        {
            foreach (Type t in assembly.GetExportedTypes())
            {
                if (!t.IsDefined(typeof(LogEventReceiverAttribute), false))
                    continue;

                LogEventReceiverAttribute attr = (LogEventReceiverAttribute)Attribute.GetCustomAttribute(t, typeof(LogEventReceiverAttribute), false);
                LogEventReceiverInfo ri = new LogEventReceiverInfo();

                ri.Name = attr.Name;
                ri.Summary = attr.Summary;
                ri.Description = attr.Description;
                ri.Type = t;
                AddReceiverInfo(ri);
                logger.Debug("Adding receiver to factory {0} ({1})", ri, t.AssemblyQualifiedName);
            }
        }

        public static ILogEventReceiver CreateLogReceiver(string type, List<ConfigurationParameter> parameters)
        {
            if (!_name2receiver.ContainsKey(type))
                throw new ArgumentException("Unknown receiver type: " + type);

            LogEventReceiverInfo ri = _name2receiver[type];
            object o = Activator.CreateInstance(ri.Type);
            ILogEventReceiver receiver = (ILogEventReceiver)o;

            ConfigurationParameter.ApplyConfigurationParameters(receiver, parameters);
            return receiver;
        }

        public static LogEventReceiverInfo FindReceiverByType(Type t)
        {
            foreach (LogEventReceiverInfo leri in _name2receiver.Values)
            {
                if (t == leri.Type)
                    return leri;
            }
            return null;
        }
	}
}
