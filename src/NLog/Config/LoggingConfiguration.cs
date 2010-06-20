// 
// Copyright (c) 2004-2010 Jaroslaw Kowalski <jaak@jkowalski.net>
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

namespace NLog.Config
{
    using System;
    using System.Collections.Generic;
    using System.Reflection;

    using NLog.Common;
    using NLog.Internal;
    using NLog.Targets;

    /// <summary>
    /// Keeps logging configuration and provides simple API
    /// to modify it.
    /// </summary>
    public class LoggingConfiguration
    {
        private readonly IDictionary<string, Target> targets =
            new Dictionary<string, Target>(StringComparer.OrdinalIgnoreCase);

        private INLogConfigurationItem[] configItems;

        /// <summary>
        /// Initializes a new instance of the <see cref="LoggingConfiguration" /> class.
        /// </summary>
        public LoggingConfiguration()
        {
            this.LoggingRules = new List<LoggingRule>();
        }

        /// <summary>
        /// Gets the collection of file names which should be watched for changes by NLog.
        /// </summary>
        public virtual IEnumerable<string> FileNamesToWatch
        {
            get { return new string[0]; }
        }

        /// <summary>
        /// Gets the collection of logging rules.
        /// </summary>
        public IList<LoggingRule> LoggingRules { get; private set; }

        /// <summary>
        /// Registers the specified target object under a given name.
        /// </summary>
        /// <param name="name">
        /// Name of the target.
        /// </param>
        /// <param name="target">
        /// The target object.
        /// </param>
        public void AddTarget(string name, Target target)
        {
            if (name == null)
            {
                throw new ArgumentException("Target name cannot be null", "name");
            }

            InternalLogger.Debug("Registering target {0}: {1}", name, target.GetType().FullName);
            this.targets[name] = target;
        }

        /// <summary>
        /// Finds the target with the specified name.
        /// </summary>
        /// <param name="name">
        /// The name of the target to be found.
        /// </param>
        /// <returns>
        /// Found target or <see langword="null"/> when the target is not found.
        /// </returns>
        public Target FindTargetByName(string name)
        {
            Target value;

            if (!this.targets.TryGetValue(name, out value))
            {
                return null;
            }

            return value;
        }

        /// <summary>
        /// Returns a collection of named targets specified in the configuration.
        /// </summary>
        /// <returns>
        /// A list of named targets.
        /// </returns>
        /// <remarks>
        /// Unnamed targets (such as those wrapped by other targets) are not returned.
        /// </remarks>
        public IList<Target> GetConfiguredNamedTargets()
        {
            return new List<Target>(this.targets.Values);
        }

        /// <summary>
        /// Called by LogManager when one of the log configuration files changes.
        /// </summary>
        /// <returns>
        /// A new instance of <see cref="LoggingConfiguration"/> that represents the updated configuration.
        /// </returns>
        public virtual LoggingConfiguration Reload()
        {
            return this;
        }

        /// <summary>
        /// Removes the specified named target.
        /// </summary>
        /// <param name="name">
        /// Name of the target.
        /// </param>
        public void RemoveTarget(string name)
        {
            this.targets.Remove(name);
        }

        /// <summary>
        /// Closes all targets and releases any unmanaged resources.
        /// </summary>
        internal void Close()
        {
            InternalLogger.Debug("Closing logging configuration...");
            foreach (ISupportsInitialize initialize in EnumerableHelpers.OfType<ISupportsInitialize>(this.configItems))
            {
                InternalLogger.Trace("Closing {0}", initialize);
                try
                {
                    initialize.Close();
                }
                catch (Exception ex)
                {
                    InternalLogger.Warn("Exception while closing {0}", ex);
                }
            }

            InternalLogger.Debug("Finished closing logging configuration.");
        }

        internal void Dump()
        {
            InternalLogger.Debug("--- NLog configuration dump. ---");
            InternalLogger.Debug("Targets:");
            foreach (Target target in this.targets.Values)
            {
                InternalLogger.Info("{0}", target);
            }

            InternalLogger.Debug("Rules:");
            foreach (LoggingRule rule in this.LoggingRules)
            {
                InternalLogger.Info("{0}", rule);
            }

            InternalLogger.Debug("--- End of NLog configuration dump ---");
        }

        /// <summary>
        /// Flushes any pending log messages on all appenders.
        /// </summary>
        /// <param name="asyncContinuation">The asynchronous continuation.</param>
        internal void FlushAllTargets(AsyncContinuation asyncContinuation)
        {
            List<Target> targets = new List<Target>();
            foreach (var rule in this.LoggingRules)
            {
                foreach (var t in rule.Targets)
                {
                    if (!targets.Contains(t))
                    {
                        targets.Add(t);
                    }
                }
            }

            AsyncHelpers.ForEachItemInParallel(targets, asyncContinuation, (target, cont) => target.Flush(cont));
        }

        internal void InitializeAll()
        {
            var roots = new List<INLogConfigurationItem>();
            foreach (LoggingRule r in this.LoggingRules)
            {
                roots.Add(r);
            }

            foreach (Target target in this.targets.Values)
            {
                roots.Add(target);
            }

            this.configItems = ObjectGraphScanner.FindReachableObjects<INLogConfigurationItem>(roots.ToArray());

            // initialize all config items starting from most nested first
            // so that whenever the container is initialized its children have already been
            InternalLogger.Info("Found {0} configuration items", this.configItems.Length);

            foreach (INLogConfigurationItem o in this.configItems)
            {
                PropertyHelper.CheckRequiredParameters(o);
            }

            foreach (ISupportsInitialize initialize in EnumerableHelpers.Reverse(EnumerableHelpers.OfType<ISupportsInitialize>(this.configItems)))
            {
                InternalLogger.Trace("Initializing {0}", initialize);
                try
                {
                    initialize.Initialize();
                }
                catch (Exception ex)
                {
                    throw new NLogConfigurationException("Error during initialization of " + initialize, ex);
                }
            }
        }
    }
}