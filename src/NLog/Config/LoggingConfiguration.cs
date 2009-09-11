// 
// Copyright (c) 2004-2009 Jaroslaw Kowalski <jaak@jkowalski.net>
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
using System.Collections.Generic;
using System.Globalization;
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
        private IDictionary<string, Target> targets = new Dictionary<string, Target>();
        private ICollection<Target> aliveTargets = new List<Target>();
        private IList<LoggingRule> loggingRules = new List<LoggingRule>();

        /// <summary>
        /// Initializes a new instance of the LoggingConfiguration class.
        /// </summary>
        public LoggingConfiguration()
        {
        }

        /// <summary>
        /// Gets the collection of logging rules.
        /// </summary>
        public IList<LoggingRule> LoggingRules
        {
            get { return this.loggingRules; }
        }

        /// <summary>
        /// Gets the collection of file names which should be watched for changes by NLog.
        /// </summary>
        public virtual IEnumerable<string> FileNamesToWatch
        {
            get { return new string[0]; }
        }

        /// <summary>
        /// Registers the specified target object under a given name.
        /// </summary>
        /// <param name="name">Name of the target.</param>
        /// <param name="target">The target object.</param>
        public void AddTarget(string name, Target target)
        {
            if (name == null)
            {
                throw new ArgumentException("Target name cannot be null", "name");
            }

            InternalLogger.Debug(CultureInfo.InvariantCulture, "Registering target {0}: {1}", name, target.GetType().FullName);
            this.targets[name.ToLower(CultureInfo.InvariantCulture)] = target;
        }

        /// <summary>
        /// Removes the specified named target.
        /// </summary>
        /// <param name="name">Name of the target.</param>
        public void RemoveTarget(string name)
        {
            this.targets.Remove(name.ToLower(CultureInfo.InvariantCulture));
        }

        /// <summary>
        /// Finds the target with the specified name.
        /// </summary>
        /// <param name="name">The name of the target to be found.</param>
        /// <returns>Found target or <see langword="null" /> when the target is not found.</returns>
        public Target FindTargetByName(string name)
        {
            Target value;

            if (!this.targets.TryGetValue(name.ToLower(CultureInfo.InvariantCulture), out value))
            {
                return null;
            }

            return value;
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
        /// Returns a collection of named targets specified in the configuration.
        /// </summary>
        /// <returns>A list of named targets.</returns>
        /// <remarks>
        /// Unnamed targets (such as those wrapped by other targets) are not returned.
        /// </remarks>
        public IList<Target> GetConfiguredNamedTargets()
        {
            return new List<Target>(this.targets.Values);
        }

        /// <summary>
        /// Closes all targets and releases any unmanaged resources.
        /// </summary>
        public void Close()
        {
            InternalLogger.Debug(CultureInfo.InvariantCulture, "Closing logging configuration...");
            foreach (Target target in this.aliveTargets)
            {
                try
                {
                    InternalLogger.Debug(CultureInfo.InvariantCulture, "Closing target {1} ({0})", target.Name, target.GetType().FullName);
                    target.Close();
                }
                catch (Exception ex)
                {
                    InternalLogger.Error(CultureInfo.InvariantCulture, "Error while closing target: {0} {1}", target.Name, ex); 
                }
            }

            InternalLogger.Debug(CultureInfo.InvariantCulture, "Finished closing logging configuration.");
        }

        /// <summary>
        /// Flushes any pending log messages on all appenders.
        /// </summary>
        /// <param name="timeout">The timeout.</param>
        internal void FlushAllTargets(TimeSpan timeout)
        {
            foreach (Target target in this.targets.Values)
            {
                try
                {
                    target.Flush(timeout);
                }
                catch (Exception ex)
                {
                    InternalLogger.Error(CultureInfo.InvariantCulture, "Error while flushing target: {0} {1}", target.Name, ex);
                }
            }
        }

        internal void InitializeAll()
        {
            foreach (LoggingRule r in this.LoggingRules)
            {
                foreach (Target t in r.Targets)
                {
                    if (!this.aliveTargets.Contains(t))
                    {
                        this.aliveTargets.Add(t);
                    }
                }
            }

            foreach (Target target in this.aliveTargets)
            {
                try
                {
                    target.Initialize();
                }
                catch (Exception ex)
                {
                    InternalLogger.Error(CultureInfo.InvariantCulture, "Error while initializing target: {0} {1}", target.Name, ex);
                }
            }
        }

        internal void Dump()
        {
            InternalLogger.Debug(CultureInfo.InvariantCulture, "--- NLog configuration dump. ---");
            InternalLogger.Debug(CultureInfo.InvariantCulture, "Targets:");
            foreach (Target target in this.targets.Values)
            {
                InternalLogger.Info("{0}", target);
            }

            InternalLogger.Debug(CultureInfo.InvariantCulture, "Rules:");
            foreach (LoggingRule rule in this.LoggingRules)
            {
                InternalLogger.Info("{0}", rule);
            }

            InternalLogger.Debug(CultureInfo.InvariantCulture, "--- End of NLog configuration dump ---");
        }
    }
}
