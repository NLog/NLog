//
// Copyright (c) 2004-2024 Jaroslaw Kowalski <jaak@jkowalski.net>, Kim Christensen, Julian Verdurmen
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
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.Globalization;
    using System.Linq;
    using System.Threading;
    using NLog.Common;
    using NLog.Internal;
    using NLog.Layouts;
    using NLog.Targets;
    using NLog.Targets.Wrappers;

    /// <summary>
    /// Keeps logging configuration and provides simple API to modify it.
    /// </summary>
    ///<remarks>This class is thread-safe.<c>.ToList()</c> is used for that purpose.</remarks>
    public class LoggingConfiguration
    {
        private readonly IDictionary<string, Target> _targets = new Dictionary<string, Target>(StringComparer.OrdinalIgnoreCase);
        private List<object> _configItems = new List<object>();

        private bool _missingServiceTypes;

        private readonly ConfigVariablesDictionary _variables;

        /// <summary>
        /// Gets the factory that will be configured
        /// </summary>
        public LogFactory LogFactory { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="LoggingConfiguration" /> class.
        /// </summary>
        public LoggingConfiguration()
            : this(LogManager.LogFactory)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="LoggingConfiguration" /> class.
        /// </summary>
        public LoggingConfiguration(LogFactory logFactory)
        {
            LogFactory = logFactory ?? LogManager.LogFactory;
            _variables = new ConfigVariablesDictionary(this);
            DefaultCultureInfo = LogFactory._defaultCultureInfo;
        }

        /// <summary>
        /// Gets the variables defined in the configuration or assigned from API
        /// </summary>
        /// <remarks>Name is case insensitive.</remarks>
        public IDictionary<string, Layout> Variables => _variables;

        /// <summary>
        /// Gets a collection of named targets specified in the configuration.
        /// </summary>
        /// <returns>
        /// A list of named targets.
        /// </returns>
        /// <remarks>
        /// Unnamed targets (such as those wrapped by other targets) are not returned.
        /// </remarks>
        public ReadOnlyCollection<Target> ConfiguredNamedTargets => GetAllTargetsThreadSafe().AsReadOnly();

        /// <summary>
        /// Gets the collection of file names which should be watched for changes by NLog.
        /// </summary>
        [Obsolete("NLog LogFactory no longer supports FileWatcher. Marked obsolete with NLog v6")]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public virtual IEnumerable<string> FileNamesToWatch => ArrayHelper.Empty<string>();

        /// <summary>
        /// Gets the collection of logging rules.
        /// </summary>
        public IList<LoggingRule> LoggingRules => _loggingRules;
        private readonly List<LoggingRule> _loggingRules = new List<LoggingRule>();

        internal LoggingRule[] GetLoggingRulesThreadSafe() { lock (_loggingRules) return _loggingRules.ToArray(); }
        private void AddLoggingRulesThreadSafe(LoggingRule rule) { lock (_loggingRules) _loggingRules.Add(rule); }

        private bool TryGetTargetThreadSafe(string name, out Target target) { lock (_targets) return _targets.TryGetValue(name, out target); }
        private List<Target> GetAllTargetsThreadSafe() { lock (_targets) return _targets.Values.ToList(); }

        private Target? RemoveTargetThreadSafe(string name)
        {
            Target? target;
            lock (_targets)
            {
                if (_targets.TryGetValue(name, out target))
                {
                    _targets.Remove(name);
                }
            }

            if (target != null)
            {
                InternalLogger.Debug("Unregistered target {0}(Name={1})", target.GetType(), target.Name);
            }

            return target;
        }

        private void AddTargetThreadSafe(Target target, string? targetAlias = null)
        {
            lock (_targets)
            {
                if (targetAlias is null || string.IsNullOrEmpty(targetAlias))
                {
                    targetAlias = target.Name ?? string.Empty;
                    if (_targets.ContainsKey(targetAlias))
                        return;
                }

                if (_targets.TryGetValue(targetAlias, out var oldTarget) && ReferenceEquals(oldTarget, target))
                    return;

                _targets[targetAlias] = target;
            }

            if (!string.IsNullOrEmpty(target.Name) && !string.Equals(target.Name, targetAlias, StringComparison.OrdinalIgnoreCase))
            {
                InternalLogger.Info("Registered target {0}(Name={1}) (Extra alias={2})", target.GetType(), target.Name, targetAlias);
            }
            else
            {
                InternalLogger.Info("Registered target {0}(Name={1})", target.GetType(), target.Name);
            }
        }

        /// <summary>
        /// Gets or sets the default culture info to use as <see cref="LogEventInfo.FormatProvider"/>.
        /// </summary>
        /// <value>
        /// Specific culture info or null to use <see cref="CultureInfo.CurrentCulture"/>
        /// </value>
        public CultureInfo? DefaultCultureInfo { get; set; }

        /// <summary>
        /// Gets all targets.
        /// </summary>
        public ReadOnlyCollection<Target> AllTargets
        {
            get
            {
                var configTargets = new HashSet<Target>(_configItems.OfType<Target>().Concat(GetAllTargetsThreadSafe()), SingleItemOptimizedHashSet<Target>.ReferenceEqualityComparer.Default);
                return configTargets.ToList().AsReadOnly();
            }
        }

        /// <summary>
        /// Inserts NLog Config Variable without overriding NLog Config Variable assigned from API
        /// </summary>
        internal void InsertParsedConfigVariable(string key, Layout value)
        {
            _variables.InsertParsedConfigVariable(key, value, LogFactory.KeepVariablesOnReload);
        }

        /// <summary>
        /// Lookup NLog Config Variable Layout
        /// </summary>
        internal bool TryLookupDynamicVariable(string key, out Layout value)
        {
            return _variables.TryLookupDynamicVariable(key, out value);
        }

        /// <summary>
        /// Registers the specified target object. The name of the target is read from <see cref="Target.Name"/>.
        /// </summary>
        /// <param name="target">
        /// The target object with a non <see langword="null"/> <see cref="Target.Name"/>
        /// </param>
        /// <exception cref="ArgumentNullException">when <paramref name="target"/> is <see langword="null"/></exception>
        public void AddTarget(Target target)
        {
            Guard.ThrowIfNull(target);

            InternalLogger.Debug("Adding target {0}(Name={1})", target.GetType(), target.Name);

            if (string.IsNullOrEmpty(target.Name)) { throw new ArgumentException(nameof(target) + ".Name cannot be empty", nameof(target)); }

            AddTargetThreadSafe(target, target.Name);
        }

        /// <summary>
        /// Registers the specified target object under a given name.
        /// </summary>
        /// <param name="name">Name of the target.</param>
        /// <param name="target">The target object.</param>
        /// <exception cref="ArgumentException">when <paramref name="name"/> is <see langword="null"/></exception>
        /// <exception cref="ArgumentNullException">when <paramref name="target"/> is <see langword="null"/></exception>
        public void AddTarget(string name, Target target)
        {
            Guard.ThrowIfNull(name);
            Guard.ThrowIfNull(target);

            InternalLogger.Debug("Adding target {0}(Name={1})", target.GetType(), string.IsNullOrEmpty(name) ? target.Name : name);

            if (string.IsNullOrEmpty(name)) { throw new ArgumentException("Target name cannot be empty", nameof(name)); }

            AddTargetThreadSafe(target, name);
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
        public Target? FindTargetByName(string name)
        {
            Guard.ThrowIfNull(name);

            if (TryGetTargetThreadSafe(name, out var target))
            {
                return target;
            }

            return null;
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
        public TTarget? FindTargetByName<TTarget>(string name)
            where TTarget : Target
        {
            Guard.ThrowIfNull(name);

            var target = FindTargetByName(name);
            if (target is TTarget specificTarget)
            {
                return specificTarget;
            }
            else if (target is WrapperTargetBase wrapperTarget)
            {
                if (wrapperTarget.WrappedTarget is TTarget wrappedTarget)
                    return wrappedTarget;
                else if (wrapperTarget.WrappedTarget is WrapperTargetBase nestedWrapperTarget && nestedWrapperTarget.WrappedTarget is TTarget nestedTarget)
                    return nestedTarget;
            }

            return null;
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
            Guard.ThrowIfNull(targetName);

            var target = FindTargetByName(targetName);
            if (target is null)
            {
                throw new NLogRuntimeException($"Target '{targetName}' not found");
            }

            AddRule(minLevel, maxLevel, target, loggerNamePattern, false);
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
            Guard.ThrowIfNull(target);
            AddRule(minLevel, maxLevel, target, loggerNamePattern, false);
        }

        /// <summary>
        /// Add a rule with min- and maxLevel.
        /// </summary>
        /// <param name="minLevel">Minimum log level needed to trigger this rule.</param>
        /// <param name="maxLevel">Maximum log level needed to trigger this rule.</param>
        /// <param name="target">Target to be written to when the rule matches.</param>
        /// <param name="loggerNamePattern">Logger name pattern. It may include the '*' wildcard at the beginning, at the end or at both ends.</param>
        /// <param name="final">Gets or sets a value indicating whether to quit processing any further rule when this one matches.</param>
        public void AddRule(LogLevel minLevel, LogLevel maxLevel, Target target, string loggerNamePattern, bool final)
        {
            Guard.ThrowIfNull(target);
            AddLoggingRulesThreadSafe(new LoggingRule(loggerNamePattern, minLevel, maxLevel, target) { Final = final });
            AddTargetThreadSafe(target);
        }

        /// <summary>
        /// Add a rule object.
        /// </summary>
        /// <param name="rule">rule object to add</param>
        public void AddRule(LoggingRule rule)
        {
            Guard.ThrowIfNull(rule);
            AddLoggingRulesThreadSafe(rule);
        }

        /// <summary>
        /// Add a rule for one loglevel.
        /// </summary>
        /// <param name="level">log level needed to trigger this rule. </param>
        /// <param name="targetName">Name of the target to be written when the rule matches.</param>
        /// <param name="loggerNamePattern">Logger name pattern. It may include the '*' wildcard at the beginning, at the end or at both ends.</param>
        public void AddRuleForOneLevel(LogLevel level, string targetName, string loggerNamePattern = "*")
        {
            Guard.ThrowIfNull(targetName);

            var target = FindTargetByName(targetName);
            if (target is null)
            {
                throw new NLogConfigurationException($"Target '{targetName}' not found");
            }

            AddRuleForOneLevel(level, target, loggerNamePattern, false);
        }

        /// <summary>
        /// Add a rule for one loglevel.
        /// </summary>
        /// <param name="level">log level needed to trigger this rule. </param>
        /// <param name="target">Target to be written to when the rule matches.</param>
        /// <param name="loggerNamePattern">Logger name pattern. It may include the '*' wildcard at the beginning, at the end or at both ends.</param>
        public void AddRuleForOneLevel(LogLevel level, Target target, string loggerNamePattern = "*")
        {
            Guard.ThrowIfNull(target);
            AddRuleForOneLevel(level, target, loggerNamePattern, false);
        }

        /// <summary>
        /// Add a rule for one loglevel.
        /// </summary>
        /// <param name="level">log level needed to trigger this rule. </param>
        /// <param name="target">Target to be written to when the rule matches.</param>
        /// <param name="loggerNamePattern">Logger name pattern. It may include the '*' wildcard at the beginning, at the end or at both ends.</param>
        /// <param name="final">Gets or sets a value indicating whether to quit processing any further rule when this one matches.</param>
        public void AddRuleForOneLevel(LogLevel level, Target target, string loggerNamePattern, bool final)
        {
            Guard.ThrowIfNull(target);
            var loggingRule = new LoggingRule(loggerNamePattern, target) { Final = final };
            loggingRule.EnableLoggingForLevel(level);
            AddLoggingRulesThreadSafe(loggingRule);
            AddTargetThreadSafe(target);
        }

        /// <summary>
        /// Add a rule for all loglevels.
        /// </summary>
        /// <param name="targetName">Name of the target to be written when the rule matches.</param>
        /// <param name="loggerNamePattern">Logger name pattern. It may include the '*' wildcard at the beginning, at the end or at both ends.</param>
        public void AddRuleForAllLevels(string targetName, string loggerNamePattern = "*")
        {
            Guard.ThrowIfNull(targetName);

            var target = FindTargetByName(targetName);
            if (target is null)
            {
                throw new NLogRuntimeException($"Target '{targetName}' not found");
            }

            AddRuleForAllLevels(target, loggerNamePattern, false);
        }

        /// <summary>
        /// Add a rule for all loglevels.
        /// </summary>
        /// <param name="target">Target to be written to when the rule matches.</param>
        /// <param name="loggerNamePattern">Logger name pattern. It may include the '*' wildcard at the beginning, at the end or at both ends.</param>
        public void AddRuleForAllLevels(Target target, string loggerNamePattern = "*")
        {
            Guard.ThrowIfNull(target);
            AddRuleForAllLevels(target, loggerNamePattern, false);
        }

        /// <summary>
        /// Add a rule for all loglevels.
        /// </summary>
        /// <param name="target">Target to be written to when the rule matches.</param>
        /// <param name="loggerNamePattern">Logger name pattern. It may include the '*' wildcard at the beginning, at the end or at both ends.</param>
        /// <param name="final">Gets or sets a value indicating whether to quit processing any further rule when this one matches.</param>
        public void AddRuleForAllLevels(Target target, string loggerNamePattern, bool final)
        {
            Guard.ThrowIfNull(target);
            var loggingRule = new LoggingRule(loggerNamePattern, target) { Final = final };
            loggingRule.EnableLoggingForLevels(LogLevel.MinLevel, LogLevel.MaxLevel);
            AddLoggingRulesThreadSafe(loggingRule);
            AddTargetThreadSafe(target);
        }

        /// <summary>
        /// Lookup the logging rule with matching <see cref="LoggingRule.RuleName"/>
        /// </summary>
        /// <param name="ruleName">The name of the logging rule to be found.</param>
        /// <returns>Found logging rule or <see langword="null"/> when not found.</returns>
        public LoggingRule? FindRuleByName(string ruleName)
        {
            Guard.ThrowIfNull(ruleName);

            var loggingRules = GetLoggingRulesThreadSafe();
            for (int i = loggingRules.Length - 1; i >= 0; i--)
            {
                if (string.Equals(loggingRules[i].RuleName, ruleName, StringComparison.OrdinalIgnoreCase))
                {
                    return loggingRules[i];
                }
            }
            return null;
        }

        /// <summary>
        /// Removes the specified named logging rule with matching <see cref="LoggingRule.RuleName"/>
        /// </summary>
        /// <param name="ruleName">The name of the logging rule to be removed.</param>
        /// <returns>Found one or more logging rule to remove, or <see langword="false"/> when not found.</returns>
        public bool RemoveRuleByName(string ruleName)
        {
            Guard.ThrowIfNull(ruleName);

            HashSet<LoggingRule> removedRules = new HashSet<LoggingRule>();
            var loggingRules = GetLoggingRulesThreadSafe();
            foreach (var loggingRule in loggingRules)
            {
                if (string.Equals(loggingRule.RuleName, ruleName, StringComparison.OrdinalIgnoreCase))
                {
                    removedRules.Add(loggingRule);
                }
            }

            if (removedRules.Count > 0)
            {
                lock (LoggingRules)
                {
                    for (int i = LoggingRules.Count - 1; i >= 0; i--)
                    {
                        if (removedRules.Contains(LoggingRules[i]))
                        {
                            LoggingRules.RemoveAt(i);
                        }
                    }
                }
            }

            return removedRules.Count > 0;
        }

        /// <summary>
        /// Loads the NLog LoggingConfiguration from its original source (Ex. read from original config-file after it was updated)
        /// </summary>
        /// <returns>
        /// A new instance of <see cref="LoggingConfiguration"/> that represents the updated configuration.
        /// </returns>
        /// <remarks>Must assign the returned object to LogManager.Configuration to activate it</remarks>
        public virtual LoggingConfiguration Reload()
        {
            return this;
        }

        /// <summary>
        /// Allow this new configuration to capture state from the old configuration
        /// </summary>
        /// <param name="oldConfig">Old config that is about to be replaced</param>
        /// <remarks>Checks KeepVariablesOnReload and copies all NLog Config Variables assigned from API into the new config</remarks>
        protected void PrepareForReload(LoggingConfiguration oldConfig)
        {
            if (LogFactory.KeepVariablesOnReload)
            {
                _variables.PrepareForReload(oldConfig._variables);
            }
        }

        /// <summary>
        /// Notify the configuration when <see cref="LogFactory.Configuration"/> has been assigned / unassigned.
        /// </summary>
        /// <param name="logFactory">LogFactory that configuration has been assigned to.</param>
        protected internal virtual void OnConfigurationAssigned(LogFactory? logFactory)
        {
            if (!ReferenceEquals(logFactory, LogFactory) && logFactory != null)
            {
                if (ReferenceEquals(LogFactory, LogManager.LogFactory))
                    InternalLogger.Info("Configuration assigned to local LogFactory, but constructed using global LogFactory");
                else
                    InternalLogger.Info("Configuration assigned to LogFactory, but constructed using other LogFactory");
            }
        }

        /// <summary>
        /// Removes the specified named target.
        /// </summary>
        /// <param name="name">Name of the target.</param>
        public void RemoveTarget(string name)
        {
            Guard.ThrowIfNull(name);

            HashSet<Target> removedTargets = new HashSet<Target>();
            var removedTarget = RemoveTargetThreadSafe(name);
            if (removedTarget != null)
            {
                removedTargets.Add(removedTarget);
            }

            if (!string.IsNullOrEmpty(name) || removedTarget != null)
            {
                CleanupRulesForRemovedTarget(name, removedTarget, removedTargets);
            }

            if (removedTargets.Count > 0)
            {
                // Refresh active logger-objects, so they stop using the removed target
                //  - Can be called even if no LoggingConfiguration is loaded (will not trigger a config load)
                LogFactory.ReconfigExistingLoggers();

                // Perform flush and close after having stopped logger-objects from using the target
                ManualResetEvent flushCompleted = new ManualResetEvent(false);
                foreach (var target in removedTargets)
                {
                    flushCompleted.Reset();
                    target.Flush((ex) => flushCompleted.Set());
                    flushCompleted.WaitOne(TimeSpan.FromSeconds(15));
                    target.Close();
                }
            }
        }

        private void CleanupRulesForRemovedTarget(string name, Target? removedTarget, HashSet<Target> removedTargets)
        {
            var loggingRules = GetLoggingRulesThreadSafe();

            foreach (var rule in loggingRules)
            {
                var targetList = rule.GetTargetsThreadSafe();
                foreach (var target in targetList)
                {
                    if (ReferenceEquals(removedTarget, target) || (!string.IsNullOrEmpty(name) && target.Name == name))
                    {
                        removedTargets.Add(target);
                        rule.RemoveTargetThreadSafe(target);
                    }
                }
            }
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
            Guard.ThrowIfNull(installationContext);

            InitializeAll();
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
                    if (exception.MustBeRethrownImmediately() || installationContext.ThrowExceptions)
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
            Guard.ThrowIfNull(installationContext);

            InitializeAll();

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
            var targetList = GetAllTargetsThreadSafe();
            foreach (Target target in targetList)
            {
                InternalLogger.Debug("{0}", target);
            }

            InternalLogger.Debug("Rules:");
            foreach (LoggingRule rule in GetLoggingRulesThreadSafe())
            {
                InternalLogger.Debug("{0}", rule);
            }

            InternalLogger.Debug("--- End of NLog configuration dump ---");
        }

        internal HashSet<Target> GetAllTargetsToFlush()
        {
            var uniqueTargets = new HashSet<Target>(SingleItemOptimizedHashSet<Target>.ReferenceEqualityComparer.Default);
            foreach (var rule in GetLoggingRulesThreadSafe())
            {
                var targetList = rule.GetTargetsThreadSafe();
                foreach (var target in targetList)
                {
                    uniqueTargets.Add(target);
                }
            }

            return uniqueTargets;
        }

        /// <summary>
        /// Validates the configuration.
        /// </summary>
        internal void ValidateConfig()
        {
            var roots = new List<object>();

            foreach (LoggingRule rule in GetLoggingRulesThreadSafe())
            {
                roots.Add(rule);
            }

            var targetList = GetAllTargetsThreadSafe();
            foreach (Target target in targetList)
            {
                roots.Add(target);
            }

            _configItems = ObjectGraphScanner.FindReachableObjects<object>(ConfigurationItemFactory.Default, true, roots.ToArray());
        }

        internal void InitializeAll()
        {
            bool firstInitializeAll = _configItems.Count == 0;

            if (firstInitializeAll && (LogFactory.ThrowExceptions || LogManager.ThrowExceptions))
            {
                InternalLogger.Info("LogManager.ThrowExceptions = true can crash the application! Use only for unit-testing and last resort troubleshooting.");
            }

            ValidateConfig();

            if (firstInitializeAll && _targets.Count > 0)
            {
                CheckUnusedTargets();
            }

            // initialize all config items starting from most nested first
            // so that whenever the container is initialized its children have already been
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
                    if (exception.MustBeRethrown(initialize as IInternalLoggerContext))
                    {
                        throw;
                    }
                }

                if (initialize is Target target && target.InitializeException is NLogDependencyResolveException)
                {
                    _missingServiceTypes = true;
                }
            }
        }

        internal void CheckForMissingServiceTypes(Type serviceType)
        {
            if (_missingServiceTypes)
            {
                bool missingServiceTypes = false;

                var allTargets = AllTargets;
                foreach (var target in allTargets)
                {
                    if (target.InitializeException is NLogDependencyResolveException resolveException)
                    {
                        missingServiceTypes = true;

                        if (typeof(IServiceProvider).IsAssignableFrom(serviceType) || IsMissingServiceType(resolveException, serviceType))
                        {
                            target.Close(); // Close Target to allow re-initialize
                        }
                    }
                }

                _missingServiceTypes = missingServiceTypes;

                if (missingServiceTypes)
                {
                    InitializeAll();
                }
            }
        }

        private static bool IsMissingServiceType(NLogDependencyResolveException resolveException, Type serviceType)
        {
            if (resolveException.ServiceType.IsAssignableFrom(serviceType))
            {
                return true;
            }

            if (resolveException.InnerException is NLogDependencyResolveException dependencyResolveException)
            {
                return IsMissingServiceType(dependencyResolveException, serviceType);
            }

            return false;
        }

        private List<IInstallable> GetInstallableItems()
        {
            return _configItems.OfType<IInstallable>().ToList();
        }

        private List<ISupportsInitialize> GetSupportsInitializes(bool reverse = false)
        {
            var items = _configItems.OfType<ISupportsInitialize>();
            if (reverse)
            {
                items = items.Reverse();
            }
            return items.ToList();
        }

        /// <summary>
        /// Replace a simple variable with a value. The original value is removed and thus we cannot redo this in a later stage.
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        internal string ExpandSimpleVariables(string? input)
        {
            return ExpandSimpleVariables(input, out var _);
        }

        internal string ExpandSimpleVariables(string? input, out string? matchingVariableName)
        {
            var output = input;
            matchingVariableName = null;

            if (output != null && !StringHelpers.IsNullOrWhiteSpace(output) && Variables.Count > 0 && output.IndexOf('$') >= 0)
            {
                var culture = StringComparison.OrdinalIgnoreCase;

                foreach (var kvp in _variables)
                {
                    var layout = kvp.Value;

                    if (layout is null)
                    {
                        continue;
                    }

                    var layoutText = string.Concat("${", kvp.Key, "}");
                    if (output.IndexOf(layoutText, culture) < 0)
                    {
                        continue;
                    }

                    //this value is set from xml and that's a string. Because of that, we can use SimpleLayout here.
                    if (layout is SimpleLayout simpleLayout)
                    {
                        output = StringHelpers.Replace(output, layoutText, simpleLayout.OriginalText, culture);
                        matchingVariableName = null;
                    }
                    else
                    {
                        if (string.Equals(layoutText, input?.Trim() ?? string.Empty, culture))
                        {
                            matchingVariableName = kvp.Key;
                        }
                    }
                }
            }

            return output ?? string.Empty;
        }

        /// <summary>
        /// Checks whether unused targets exist. If found any, just write an internal log at Warn level.
        /// <remarks>If initializing not started or failed, then checking process will be canceled</remarks>
        /// </summary>
        internal void CheckUnusedTargets()
        {
            if (!InternalLogger.IsWarnEnabled)
                return;

            var configuredNamedTargets = GetAllTargetsThreadSafe(); //assign to variable because `GetAllTargetsThreadSafe` computes a new list every time.
            InternalLogger.Debug("Unused target checking is started... Rule Count: {0}, Target Count: {1}", LoggingRules.Count, configuredNamedTargets.Count);

            var targetNamesAtRules = new HashSet<string>(GetLoggingRulesThreadSafe().SelectMany(r => r.Targets).Select(t => t.Name));
            var allTargets = AllTargets;
            ILookup<Target?, Target> wrappedTargets = allTargets.OfType<WrapperTargetBase>().ToLookup(wt => wt.WrappedTarget, wt => (Target)wt);
            ILookup<Target?, Target> compoundTargets = allTargets.OfType<CompoundTargetBase>().SelectMany(wt => wt.Targets.Select(t => new KeyValuePair<Target?, Target>(t, wt))).ToLookup(p => p.Key, p => p.Value);

            bool IsUnusedInList(Target target1, ILookup<Target?, Target> targets)
            {
                if (targets.Contains(target1))
                {
                    foreach (var wrapperTarget in targets[target1])
                    {
                        if (targetNamesAtRules.Contains(wrapperTarget.Name))
                            return false;

                        if (wrappedTargets.Contains(wrapperTarget))
                            return false;

                        if (compoundTargets.Contains(wrapperTarget))
                            return false;
                    }
                }

                return true;
            }

            int unusedCount = configuredNamedTargets.Count((target) =>
            {
                if (targetNamesAtRules.Contains(target.Name))
                    return false;

                if (!IsUnusedInList(target, wrappedTargets))
                    return false;

                if (!IsUnusedInList(target, compoundTargets))
                    return false;

                InternalLogger.Warn("Unused target detected. Add a rule for this target to the configuration. TargetName: {0}", target.Name);
                return true;
            });

            InternalLogger.Debug("Unused target checking is completed. Total Rule Count: {0}, Total Target Count: {1}, Unused Target Count: {2}", LoggingRules.Count, configuredNamedTargets.Count, unusedCount);
        }

        internal AsyncContinuation? FlushAllTargets(AsyncContinuation flushCompletion)
        {
            var pendingTargets = GetAllTargetsToFlush();
            if (pendingTargets.Count == 0)
            {
                flushCompletion.Invoke(null);
                return null;
            }

            InternalLogger.Trace("Flushing all {0} targets...", pendingTargets.Count);

            Exception? lastException = null;

            Action<Target, Exception?> flushAction = (t, ex) =>
            {
                if (ex != null)
                {
                    InternalLogger.Warn(ex, "Flush failed for target {0}(Name={1})", t.GetType(), t.Name);
                }
                bool completed = false;
                lock (pendingTargets)
                {
                    if (ex != null)
                        lastException = ex;
                    if (pendingTargets.Remove(t) && pendingTargets.Count == 0)
                        completed = true;
                }
                if (completed)
                {
                    if (lastException != null)
                        InternalLogger.Warn("Flush completed with errors");
                    else
                        InternalLogger.Debug("Flush completed");
                    flushCompletion.Invoke(lastException);
                }
            };

            foreach (var target in pendingTargets.ToArray())
            {
                var flushTarget = target;
                AsyncHelpers.StartAsyncTask(s =>
                {
                    try
                    {
                        flushTarget.Flush(ex =>
                        {
                            flushAction(flushTarget, ex);
                        });
                    }
                    catch (Exception ex)
                    {
                        flushAction(flushTarget, ex);
                        throw;
                    }
                }, null);
            }

            AsyncContinuation flushTimeoutHandler = (ex) =>
            {
                lock (pendingTargets)
                {
                    foreach (var pendingTarget in pendingTargets)
                        InternalLogger.Warn("Flush timeout for target {0}(Name={1})", pendingTarget.GetType(), pendingTarget.Name);
                    pendingTargets.Clear();
                }
            };

            return flushTimeoutHandler;
        }

        internal ITargetWithFilterChain[] BuildLoggerConfiguration(string loggerName, LogLevel globalLogLevel)
        {
            if (LoggingRules.Count == 0 || LogLevel.Off.Equals(globalLogLevel))
                return TargetWithFilterChain.NoTargetsByLevel;

            var loggingRules = GetLoggingRulesThreadSafe();
            var targetsByLevel = TargetWithFilterChain.BuildLoggerConfiguration(loggerName, loggingRules, globalLogLevel);
            if (InternalLogger.IsDebugEnabled && !DumpTargetConfigurationForLogger(loggerName, targetsByLevel))
            {
                InternalLogger.Debug("Targets not configured for Logger: {0}", loggerName);
            }

            return targetsByLevel ?? TargetWithFilterChain.NoTargetsByLevel;
        }

        private static bool DumpTargetConfigurationForLogger(string loggerName, TargetWithFilterChain[] targetsByLevel)
        {
            if (targetsByLevel is null)
                return false;

            System.Text.StringBuilder? sb = null;
            for (int i = 0; i <= LogLevel.MaxLevel.Ordinal; ++i)
            {
                if (sb != null)
                {
                    sb.Length = 0;
                    sb.AppendFormat(CultureInfo.InvariantCulture, "Logger {0} [{1}] =>", loggerName, LogLevel.FromOrdinal(i));
                }

                for (TargetWithFilterChain? afc = targetsByLevel[i]; afc != null; afc = afc.NextInChain)
                {
                    if (sb is null)
                    {
                        InternalLogger.Debug("Targets configured when LogLevel >= {0} for Logger: {1}", LogLevel.FromOrdinal(i), loggerName);
                        sb = new System.Text.StringBuilder();
                        sb.AppendFormat(CultureInfo.InvariantCulture, "Logger {0} [{1}] =>", loggerName, LogLevel.FromOrdinal(i));
                    }

                    sb.AppendFormat(CultureInfo.InvariantCulture, " {0}", afc.Target.Name);
                    if (afc.FilterChain.Count > 0)
                    {
                        sb.AppendFormat(CultureInfo.InvariantCulture, " ({0} filters)", afc.FilterChain.Count);
                    }
                }

                if (sb != null)
                    InternalLogger.Debug(sb.ToString());
            }

            return sb != null;
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            ICollection<Target> targets = GetAllTargetsToFlush();
            if (targets.Count == 0)
                targets = GetAllTargetsThreadSafe();
            if (targets.Count == 0)
                targets = AllTargets;

            if (targets.Count > 0 && targets.Count < 5)
                return $"TargetNames={string.Join(", ", targets.Select(t => t.Name).Where(n => !string.IsNullOrEmpty(n)).ToArray())}, ConfigItems={_configItems.Count}";
            else
                return $"Targets={targets.Count}, ConfigItems={_configItems.Count}";
        }
    }
}
