// 
// Copyright (c) 2004-2006 Jaroslaw Kowalski <jaak@jkowalski.net>
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
using System.Collections;
using System.Xml;
using System.Globalization;
using System.Reflection;

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
        internal TargetDictionary _targets = new TargetDictionary();
        internal TargetCollection _aliveTargets = new TargetCollection();
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
            if (name == null)
                throw new ArgumentException("name", "Target name cannot be null");
            InternalLogger.Debug("Registering target {0}: {1}", name, target.GetType().FullName);
            _targets[name.ToLower(CultureInfo.InvariantCulture)] = target;
        }

        /// <summary>
        /// Removes the specified named target.
        /// </summary>
        /// <param name="name">Name of the target.</param>
        public void RemoveTarget(string name)
        {
            _targets.Remove(name.ToLower(CultureInfo.InvariantCulture));
        }

        /// <summary>
        /// Finds the target with the specified name.
        /// </summary>
        /// <param name="name">The name of the target to be found.</param>
        /// <returns>Found target or <see langword="null" /> when the target is not found.</returns>
        public Target FindTargetByName(string name)
        {
            return _targets[name.ToLower(CultureInfo.InvariantCulture)];
        }

        /// <summary>
        /// The collection of logging rules
        /// </summary>
        public LoggingRuleCollection LoggingRules
        {
            get { return _loggingRules; }
        }

        /// <summary>
        /// A collection of file names which should be watched for changes by NLog.
        /// </summary>
        public virtual ICollection FileNamesToWatch
        {
            get { return null; }
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
        internal void FlushAllTargets(TimeSpan timeout)
        {
            foreach (Target target in _targets.Values)
            {
                try
                {
                    target.Flush(timeout);
                
                }
                catch (Exception ex)
                {
                    InternalLogger.Error("Error while flushing target: {0} {1}", target.Name, ex); 
                }
            }
        }

        internal void InitializeAll()
        {
            foreach (LoggingRule r in LoggingRules)
            {
                foreach (Target t in r.Targets)
                {
                    if (!_aliveTargets.Contains(t))
                        _aliveTargets.Add(t);
                }
            }

            foreach (Target target in _aliveTargets)
            {
                try
                {
                    target.Initialize();
                
                }
                catch (Exception ex)
                {
                    InternalLogger.Error("Error while initializing target: {0} {1}", target.Name, ex); 
                }
            }
        }

        /// <summary>
        /// Returns a collection of named targets specified in the configuration.
        /// </summary>
        /// <returns>A <see cref="TargetCollection"/> object that contains a list of named targets.</returns>
        /// <remarks>
        /// Unnamed targets (such as those wrapped by other targets) are not returned.
        /// </remarks>
        public TargetCollection GetConfiguredNamedTargets()
        {
            TargetCollection tc = new TargetCollection();
            foreach (Target t in _targets.Values)
            {
                tc.Add(t);
            }
            return tc;
        }


        /// <summary>
        /// Closes all targets and releases any unmanaged resources.
        /// </summary>
        public void Close()
        {
            InternalLogger.Debug("Closing logging configuration...");
            foreach (Target target in _aliveTargets)
            {
                try
                {
                    InternalLogger.Debug("Closing target {1} ({0})", target.Name, target.GetType().FullName);
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
