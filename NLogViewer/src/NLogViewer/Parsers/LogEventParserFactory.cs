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

namespace NLogViewer.Parsers
{
	public class LogEventParserFactory
	{
        private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        private static Dictionary<string, LogEventParserInfo> _name2parser = new Dictionary<string, LogEventParserInfo>();
        private static List<LogEventParserInfo> _parsers = new List<LogEventParserInfo>();

        public static IList<LogEventParserInfo> Parsers
        {
            get { return _parsers; }
        }

        static LogEventParserFactory()
        {
            try
            {
                AddParsersFromAssembly(typeof(LogEventParserFactory).Assembly);
                foreach (string assemblyName in NLogViewerConfiguration.Configuration.ExtensionAssemblies)
                {
                    Assembly extensionAssembly = Assembly.Load(assemblyName);
                    AddParsersFromAssembly(extensionAssembly);
                }
            }
            catch (Exception)
            {
            }
        }

        public static void AddParserInfo(LogEventParserInfo ri)
        {
            _name2parser[ri.Name] = ri;
            _parsers.Add(ri);
        }

        private static void AddParsersFromAssembly(Assembly assembly)
        {
            foreach (Type t in assembly.GetExportedTypes())
            {
                if (!t.IsDefined(typeof(LogEventParserAttribute), false))
                    continue;

                LogEventParserAttribute attr = (LogEventParserAttribute)Attribute.GetCustomAttribute(t, typeof(LogEventParserAttribute), false);
                LogEventParserInfo ri = new LogEventParserInfo();

                ri.Name = attr.Name;
                ri.Summary = attr.Summary;
                ri.Description = attr.Description;
                ri.Type = t;
                AddParserInfo(ri);
                logger.Debug("Adding parser to factory {0} ({1})", ri, t.AssemblyQualifiedName);
            }
        }

        public static ILogEventParser CreateLogParser(string type, List<ConfigurationParameter> parameters)
        {
            if (!_name2parser.ContainsKey(type))
                throw new ArgumentException("Unknown parser type: " + type);

            LogEventParserInfo ri = _name2parser[type];
            object o = Activator.CreateInstance(ri.Type);
            ILogEventParser parser = (ILogEventParser)o;
            ConfigurationParameter.ApplyConfigurationParameters(parser, parameters);

            return parser;
        }
	}
}
