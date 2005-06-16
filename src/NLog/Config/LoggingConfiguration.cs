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
using System.Collections;
using System.Xml;

using NLog;
using NLog.Internal;
using NLog.Targets;

namespace NLog.Config
{
    /// <summary>
    /// Keeps logging configuration and provides simple API
    /// to modify it.
    /// </summary>
    public class LoggingConfiguration
    {
        private TargetDictionary _targets = new TargetDictionary();
        private LoggingRuleCollection _loggingRules = new LoggingRuleCollection();

        /// <summary>
        /// Creates new instance of LoggingConfiguration object.
        /// </summary>
        public LoggingConfiguration(){}

        /// <summary>
        /// Registers the specified target object under a given name.
        /// </summary>
        /// <param name="name">Name of the target.</param>
        /// <param name="target">The target object.</param>
        public void AddTarget(string name, Target target)
        {
            _targets[name] = target;
        }

        /// <summary>
        /// Finds the target with the specified name.
        /// </summary>
        /// <param name="name">The name of the target to be found.</param>
        /// <returns>Found target or <see langword="null" /> when the target is not found.</returns>
        public Target FindTargetByName(string name)
        {
            return _targets[name];
        }

        /// <summary>
        /// The collection of logging rules
        /// </summary>
        public LoggingRuleCollection LoggingRules
        {
            get
            {
                return _loggingRules;
            }
        }

        internal TargetDictionary Targets
        {
            get { return _targets; }
        }

        /// <summary>
        /// A collection of file names which should be watched for changes by NLog.
        /// </summary>
        public virtual ICollection FileNamesToWatch
        {
            get
            {
                return null;
            }
        }

        /// <summary>
        /// Called by LogManager when one of the log configuration files changes.
        /// </summary>
        /// <returns>A new instance of <see cref="LoggingConfiguration" /> that represents the updated configuration.</returns>
        public virtual LoggingConfiguration Reload()
        {
            return this;
        }

        
        /// <summary>
        /// Flushes any pending log messages on all appenders.
        /// </summary>
        public void FlushAllTargets()
        {
            foreach (Target target in _targets.Values)
            {
                try
                {
                    target.Flush();
                
                }
                catch (Exception ex)
                {
                    InternalLogger.Error("Error while flushing target: {0} {1}", target.Name, ex); 
                }
            }
        }

        /// <summary>
        /// Closes all targets and releases any unmanaged resources.
        /// </summary>
        public void Close()
        {
            foreach (Target target in _targets.Values)
            {
                try
                {
                    target.Close();
                
                }
                catch (Exception ex)
                {
                    InternalLogger.Error("Error while closing target: {0} {1}", target.Name, ex); 
                }
            }
        }
    }
}
