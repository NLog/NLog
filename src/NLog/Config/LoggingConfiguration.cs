// 
// Copyright (c) 2004-2011 Jaroslaw Kowalski <jaak@jkowalski.net>
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

using System.Globalization;
using System.Linq;
using NLog.Layouts;

namespace NLog.Config
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using JetBrains.Annotations;

    using NLog.Common;
    using NLog.Internal;
    using NLog.Targets;

    /// <summary>
    /// Keeps logging configuration and provides simple API
    /// to modify it.
    /// </summary>
    ///<remarks>This class is thread-safe.<c>.ToList()</c> is used for that purpose.</remarks>
    public class LoggingConfiguration
    {
        private readonly IDictionary<string, Target> targets =
            new Dictionary<string, Target>(StringComparer.OrdinalIgnoreCase);

        private List<object> configItems = new List<object>();

        /// <summary>
        /// Variables defined in xml or in API. name is case case insensitive. 
        /// </summary>
        private readonly Dictionary<string, SimpleLayout> variables = new Dictionary<string, SimpleLayout>(StringComparer.OrdinalIgnoreCase);

        /// <summary>
        /// Initializes a new instance of the <see cref="LoggingConfiguration" /> class.
        /// </summary>
        public LoggingConfiguration()
        {
            this.LoggingRules = new List<LoggingRule>();
        }

        /// <summary>
        /// Use the old exception log handling of NLog 3.0? 
        /// </summary>
        [Obsolete("This option will be removed in NLog 5")]
        public bool ExceptionLoggingOldStyle { get; set; }

        /// <summary>
        /// Gets the variables defined in the configuration.
        /// </summary>
        public IDictionary<string, SimpleLayout> Variables
        {
            get
            {
                return variables;
            }
        }

        /// <summary>
        /// Gets a collection of named targets specified in the configuration.
        /// </summary>
        /// <returns>
        /// A list of named targets.
        /// </returns>
        /// <remarks>
        /// Unnamed targets (such as those wrapped by other targets) are not returned.
        /// </remarks>
        public ReadOnlyCollection<Target> ConfiguredNamedTargets
        {
            get { return new List<Target>(this.targets.Values).AsReadOnly(); }
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
        /// Gets or sets the default culture info to use as <see cref="LogEventInfo.FormatProvider"/>.
        /// </summary>
        /// <value>
        /// Specific culture info or null to use <see cref="CultureInfo.CurrentCulture"/>
        /// </value>
        [CanBeNull]
        public CultureInfo DefaultCultureInfo { get; set; }

        /// <summary>
        /// Gets all targets.
        /// </summary>
        public ReadOnlyCollection<Target> AllTargets
        {
            get
            {
                var configTargets = this.configItems.OfType<Target>();
                return configTargets.Concat(targets.Values).ToList().AsReadOnly();
            }
        }

        /// <summary>
        /// Registers the specified target object. The name of the target is read from <see cref="Target.Name"/>.
        /// </summary>
        /// <param name="target">
        /// The target object with a non <see langword="null"/> <see cref="Target.Name"/>
        /// </param>
        /// <exception cref="ArgumentNullException">when <paramref name="target"/> is <see langword="null"/></exception>
        public void AddTarget([NotNull] Target target)
        {
            if (target == null) throw new ArgumentNullException("target");
            AddTarget(target.Name, target);
        }

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
        /// Finds the target with the specified name and specified type.
        /// </summary>
        /// <param name="name">
        /// The name of the target to be found.
        /// </param>
        /// <typeparam name="TTarget">Type of the target</typeparam>
        /// <returns>
        /// Found target or <see langword="null"/> when the target is not found of not of type <typeparamref name="TTarget"/>
        /// </returns>
        public TTarget FindTargetByName<TTarget>(string name)
            where TTarget : Target
        {
            return FindTargetByName(name) as TTarget;
        }

        /// <summary>
        /// Add a rule with min- and maxLevel.
        /// </summary>
        /// <param name="minLevel">Minimum log level needed to trigger this rule.</param>
        /// <param name="maxLevel">Maximum log level needed to trigger this rule.</param>
        /// <param name="targetName">Name of the target to be written when the rule matches.</param>
        /// <param name="loggerNamePattern">Logger name pattern. It may include the '*' wildcard at the beginning, at the end or at both ends.</param>
        public void AddRule(LogLevel minLevel, LogLevel maxLevel, string targetName, string loggerNamePattern = "*")
        {
            var target = FindTargetByName(targetName);
            if (target == null)
            {
                throw new NLogRuntimeException("Target '{0}' not found", targetName);
            }

            AddRule(minLevel, maxLevel, target, loggerNamePattern);
        }

        /// <summary>
        /// Add a rule with min- and maxLevel.
        /// </summary>
        /// <param name="minLevel">Minimum log level needed to trigger this rule.</param>
        /// <param name="maxLevel">Maximum log level needed to trigger this rule.</param>
        /// <param name="target">Target to be written to when the rule matches.</param>
        /// <param name="loggerNamePattern">Logger name pattern. It may include the '*' wildcard at the beginning, at the end or at both ends.</param>
        public void AddRule(LogLevel minLevel, LogLevel maxLevel, Target target, string loggerNamePattern = "*")
        {
            LoggingRules.Add(new LoggingRule(loggerNamePattern, minLevel, maxLevel, target));
        }

        /// <summary>
        /// Add a rule for one loglevel.
        /// </summary>
        /// <param name="level">log level needed to trigger this rule. </param>
        /// <param name="targetName">Name of the target to be written when the rule matches.</param>
        /// <param name="loggerNamePattern">Logger name pattern. It may include the '*' wildcard at the beginning, at the end or at both ends.</param>
        public void AddRuleForOneLevel(LogLevel level, string targetName, string loggerNamePattern = "*")
        {
            var target = FindTargetByName(targetName);
            if (target == null)
            {
                throw new NLogConfigurationException("Target '{0}' not found", targetName);
            }

            AddRuleForOneLevel(level, target, loggerNamePattern);
        }

        /// <summary>
        /// Add a rule for one loglevel.
        /// </summary>
        /// <param name="level">log level needed to trigger this rule. </param>
        /// <param name="target">Target to be written to when the rule matches.</param>
        /// <param name="loggerNamePattern">Logger name pattern. It may include the '*' wildcard at the beginning, at the end or at both ends.</param>
        public void AddRuleForOneLevel(LogLevel level, Target target, string loggerNamePattern = "*")
        {
            var loggingRule = new LoggingRule(loggerNamePattern, target);
            loggingRule.EnableLoggingForLevel(level);
            LoggingRules.Add(loggingRule);
        }

        /// <summary>
        /// Add a rule for alle loglevels.
        /// </summary>
        /// <param name="targetName">Name of the target to be written when the rule matches.</param>
        /// <param name="loggerNamePattern">Logger name pattern. It may include the '*' wildcard at the beginning, at the end or at both ends.</param>
        public void AddRuleForAllLevels(string targetName, string loggerNamePattern = "*")
        {
            var target = FindTargetByName(targetName);
            if (target == null)
            {
                throw new NLogRuntimeException("Target '{0}' not found", targetName);
            }

            AddRuleForAllLevels(target, loggerNamePattern);
        }

        /// <summary>
        /// Add a rule for alle loglevels.
        /// </summary>
        /// <param name="target">Target to be written to when the rule matches.</param>
        /// <param name="loggerNamePattern">Logger name pattern. It may include the '*' wildcard at the beginning, at the end or at both ends.</param>
        public void AddRuleForAllLevels(Target target, string loggerNamePattern = "*")
        {
            var loggingRule = new LoggingRule(loggerNamePattern, target);
            loggingRule.EnableLoggingForLevels(LogLevel.MinLevel, LogLevel.MaxLevel);
            LoggingRules.Add(loggingRule);
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
        /// Installs target-specific objects on current system.
        /// </summary>
        /// <param name="installationContext">The installation context.</param>
        /// <remarks>
        /// Installation typically runs with administrative permissions.
        /// </remarks>
        public void Install(InstallationContext installationContext)
        {
            if (installationContext == null)
            {
                throw new ArgumentNullException("installationContext");
            }

            this.InitializeAll();
            var configItemsList = GetInstallableItems();
            foreach (IInstallable installable in configItemsList)
            {
                installationContext.Info("Installing '{0}'", installable);

                try
                {
                    installable.Install(installationContext);
                    installationContext.Info("Finished installing '{0}'.", installable);
                }
                catch (Exception exception)
                {
                    InternalLogger.Error(exception, "Install of '{0}' failed.", installable);
                    if (exception.MustBeRethrownImmediately())
                    {
                        throw;
                    }

                    installationContext.Error("Install of '{0}' failed: {1}.", installable, exception);
                }
            }
        }


        /// <summary>
        /// Uninstalls target-specific objects from current system.
        /// </summary>
        /// <param name="installationContext">The installation context.</param>
        /// <remarks>
        /// Uninstallation typically runs with administrative permissions.
        /// </remarks>
        public void Uninstall(InstallationContext installationContext)
        {
            if (installationContext == null)
            {
                throw new ArgumentNullException("installationContext");
            }

            this.InitializeAll();

            var configItemsList = GetInstallableItems();
            foreach (IInstallable installable in configItemsList)
            {
                installationContext.Info("Uninstalling '{0}'", installable);

                try
                {
                    installable.Uninstall(installationContext);
                    installationContext.Info("Finished uninstalling '{0}'.", installable);
                }
                catch (Exception exception)
                {
                    InternalLogger.Error(exception, "Uninstall of '{0}' failed.", installable);
                    if (exception.MustBeRethrownImmediately())
                    {
                        throw;
                    }

                    installationContext.Error("Uninstall of '{0}' failed: {1}.", installable, exception);
                }
            }
        }

        /// <summary>
        /// Closes all targets and releases any unmanaged resources.
        /// </summary>
        internal void Close()
        {
            InternalLogger.Debug("Closing logging configuration...");
            var supportsInitializesList = GetSupportsInitializes();
            foreach (ISupportsInitialize initialize in supportsInitializesList)
            {
                InternalLogger.Trace("Closing {0}", initialize);
                try
                {
                    initialize.Close();
                }
                catch (Exception exception)
                {
                    InternalLogger.Warn(exception, "Exception while closing.");

                    if (exception.MustBeRethrown())
                    {
                        throw;
                    }


                }
            }

            InternalLogger.Debug("Finished closing logging configuration.");
        }

        /// <summary>
        /// Log to the internal (NLog) logger the information about the <see cref="Target"/> and <see
        /// cref="LoggingRule"/> associated with this <see cref="LoggingConfiguration"/> instance.
        /// </summary>
        /// <remarks>
        /// The information are only recorded in the internal logger if Debug level is enabled, otherwise nothing is 
        /// recorded.
        /// </remarks>
        internal void Dump()
        {
            if (!InternalLogger.IsDebugEnabled)
            {
                return;
            }

            InternalLogger.Debug("--- NLog configuration dump ---");
            InternalLogger.Debug("Targets:");
            var targetList = this.targets.Values.ToList();
            foreach (Target target in targetList)
            {
                InternalLogger.Debug("{0}", target);
            }

            InternalLogger.Debug("Rules:");
            var loggingRules = this.LoggingRules.ToList();
            foreach (LoggingRule rule in loggingRules)
            {
                InternalLogger.Debug("{0}", rule);
            }

            InternalLogger.Debug("--- End of NLog configuration dump ---");
        }

        /// <summary>
        /// Flushes any pending log messages on all appenders.
        /// </summary>
        /// <param name="asyncContinuation">The asynchronous continuation.</param>
        internal void FlushAllTargets(AsyncContinuation asyncContinuation)
        {
            var uniqueTargets = new List<Target>();
            var loggingRules = this.LoggingRules.ToList();
            foreach (var rule in loggingRules)
            {
                var targetList = rule.Targets.ToList();
                foreach (var target in targetList)
                {
                    if (!uniqueTargets.Contains(target))
                    {
                        uniqueTargets.Add(target);
                    }
                }
            }

            AsyncHelpers.ForEachItemInParallel(uniqueTargets, asyncContinuation, (target, cont) => target.Flush(cont));
        }

        /// <summary>
        /// Validates the configuration.
        /// </summary>
        internal void ValidateConfig()
        {
            var roots = new List<object>();

            var loggingRules = this.LoggingRules.ToList();
            foreach (LoggingRule rule in loggingRules)
            {
                roots.Add(rule);
            }

            var targetList = this.targets.Values.ToList();
            foreach (Target target in targetList)
            {
                roots.Add(target);
            }

            this.configItems = ObjectGraphScanner.FindReachableObjects<object>(roots.ToArray());

            // initialize all config items starting from most nested first
            // so that whenever the container is initialized its children have already been
            InternalLogger.Info("Found {0} configuration items", this.configItems.Count);

            foreach (object o in this.configItems)
            {
                PropertyHelper.CheckRequiredParameters(o);
            }
        }

        internal void InitializeAll()
        {
            this.ValidateConfig();

            var supportsInitializes = GetSupportsInitializes(true);
            foreach (ISupportsInitialize initialize in supportsInitializes)
            {
                InternalLogger.Trace("Initializing {0}", initialize);

                try
                {
                    initialize.Initialize(this);
                }
                catch (Exception exception)
                {
                    if (exception.MustBeRethrown())
                    {
                        throw;
                    }

                    if (LogManager.ThrowExceptions)
                    {
                        throw new NLogConfigurationException("Error during initialization of " + initialize, exception);
                    }
                }
            }
        }


        internal void EnsureInitialized()
        {
            this.InitializeAll();
        }

        private List<IInstallable> GetInstallableItems()
        {
            return this.configItems.OfType<IInstallable>().ToList();
        }

        private List<ISupportsInitialize> GetSupportsInitializes(bool reverse = false)
        {
            var items = this.configItems.OfType<ISupportsInitialize>();
            if (reverse)
            {
                items = items.Reverse();
            }
            return items.ToList();
        }
    }
}