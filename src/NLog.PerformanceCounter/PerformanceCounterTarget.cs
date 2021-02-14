// 
// Copyright (c) 2004-2020 Jaroslaw Kowalski <jaak@jkowalski.net>, Kim Christensen, Julian Verdurmen
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

namespace NLog.Targets
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Linq;
    using NLog.Common;
    using NLog.Config;
    using NLog.Layouts;

    /// <summary>
    /// Increments specified performance counter on each write.
    /// </summary>
    /// <seealso href="https://github.com/nlog/nlog/wiki/PerformanceCounter-target">Documentation on NLog Wiki</seealso>
    /// <example>
    /// <p>
    /// To set up the target in the <a href="config.html">configuration file</a>, 
    /// use the following syntax:
    /// </p>
    /// <code lang="XML" source="examples/targets/Configuration File/PerfCounter/NLog.config" />
    /// <p>
    /// This assumes just one target and a single rule. More configuration
    /// options are described <a href="config.html">here</a>.
    /// </p>
    /// <p>
    /// To set up the log target programmatically use code like this:
    /// </p>
    /// <code lang="C#" source="examples/targets/Configuration API/PerfCounter/Simple/Example.cs" />
    /// </example>
    /// <remarks>
    /// TODO:
    /// 1. Unable to create a category allowing multiple counter instances (.Net 2.0 API only, probably)
    /// 2. Is there any way of adding new counters without deleting the whole category?
    /// 3. There should be some mechanism of resetting the counter (e.g every day starts from 0), or auto-switching to 
    ///    another counter instance (with dynamic creation of new instance). This could be done with layouts. 
    /// </remarks>
    [Target("PerfCounter")]
    [Target("PerformanceCounter")]
    public class PerformanceCounterTarget : Target, IInstallable
    {
        private PerformanceCounter perfCounter;
        private bool initialized;
        private bool created;

        /// <summary>
        /// Initializes a new instance of the <see cref="PerformanceCounterTarget" /> class.
        /// </summary>
        public PerformanceCounterTarget()
        {
            CounterType = PerformanceCounterType.NumberOfItems32;
            IncrementValue = new SimpleLayout("1");
            InstanceName = string.Empty;
            CounterHelp = string.Empty;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PerformanceCounterTarget" /> class.
        /// </summary>
        /// <param name="name">Name of the target.</param>
        public PerformanceCounterTarget(string name) : this()
        {
            Name = name;
        }

        /// <summary>
        /// Gets or sets a value indicating whether performance counter should be automatically created.
        /// </summary>
        /// <docgen category='Performance Counter Options' order='10' />
        public bool AutoCreate { get; set; }

        /// <summary>
        /// Gets or sets the name of the performance counter category.
        /// </summary>
        /// <docgen category='Performance Counter Options' order='10' />
        [RequiredParameter]
        public string CategoryName { get; set; }

        /// <summary>
        /// Gets or sets the name of the performance counter.
        /// </summary>
        /// <docgen category='Performance Counter Options' order='10' />
        [RequiredParameter]
        public string CounterName { get; set; }

        /// <summary>
        /// Gets or sets the performance counter instance name.
        /// </summary>
        /// <docgen category='Performance Counter Options' order='10' />
        public string InstanceName { get; set; }

        /// <summary>
        /// Gets or sets the counter help text.
        /// </summary>
        /// <docgen category='Performance Counter Options' order='10' />
        public string CounterHelp { get; set; }

        /// <summary>
        /// Gets or sets the performance counter type.
        /// </summary>
        /// <docgen category='Performance Counter Options' order='10' />
        [DefaultValue(PerformanceCounterType.NumberOfItems32)]
        public PerformanceCounterType CounterType { get; set; }

        /// <summary>
        /// The value by which to increment the counter.
        /// </summary>
        /// <docgen category='Performance Counter Options' order='10' />
        [DefaultValue(1)]
        public Layout IncrementValue { get; set; }

        /// <summary>
        /// Performs installation which requires administrative permissions.
        /// </summary>
        /// <param name="installationContext">The installation context.</param>
        public void Install(InstallationContext installationContext)
        {
            // categories must be installed together, so we must find all PerfCounter targets in the configuration file
            string categoryName = CategoryName;
            if (string.IsNullOrEmpty(categoryName))
            {
                installationContext.Trace("Blank Category '{0}' cannot be installed.", categoryName);
                return;
            }

            var countersInCategory = LoggingConfiguration?.AllTargets.OfType<PerformanceCounterTarget>().Where(c => c.CategoryName == categoryName).ToList();
            if (countersInCategory?.Any(c => c.created)==true)
            {
                installationContext.Trace("Category '{0}' has already been installed.", categoryName);
                return;
            }

            if (countersInCategory == null)
                countersInCategory = new List<PerformanceCounterTarget>(new[] { this });
            else if (!countersInCategory.Contains(this))
                countersInCategory.Add(this);

            try
            {
                PerformanceCounterCategoryType categoryType;
                CounterCreationDataCollection ccds = GetCounterCreationDataCollection(countersInCategory, out categoryType);

                if (PerformanceCounterCategory.Exists(categoryName))
                {
                    installationContext.Debug("Deleting category '{0}'", categoryName);
                    PerformanceCounterCategory.Delete(categoryName);
                }

                installationContext.Debug("Creating category '{0}' with {1} counter(s) (Type: {2})", categoryName, ccds.Count, categoryType);
                foreach (CounterCreationData c in ccds)
                {
                    installationContext.Trace("  Counter: '{0}' Type: ({1}) Help: {2}", c.CounterName, c.CounterType, c.CounterHelp);
                }

                PerformanceCounterCategory.Create(categoryName, "Category created by NLog", categoryType, ccds);
            }
            catch (Exception exception)
            {
                if (LogManager.ThrowExceptions)
                    throw;

                if (installationContext.IgnoreFailures)
                {
                    installationContext.Warning("Error creating category '{0}': {1}", categoryName, exception.Message);
                }
                else
                {
                    installationContext.Error("Error creating category '{0}': {1}", categoryName, exception.Message);
                    throw;
                }
            }
            finally
            {
                foreach (var t in countersInCategory)
                {
                    t.created = true;
                }
            }
        }

        /// <summary>
        /// Performs uninstallation which requires administrative permissions.
        /// </summary>
        /// <param name="installationContext">The installation context.</param>
        public void Uninstall(InstallationContext installationContext)
        {
            string categoryName = CategoryName;

            if (PerformanceCounterCategory.Exists(categoryName))
            {
                installationContext.Debug("Deleting category '{0}'", categoryName);
                PerformanceCounterCategory.Delete(categoryName);
            }
            else
            {
                installationContext.Debug("Category '{0}' does not exist.", categoryName);
            }
        }

        /// <summary>
        /// Determines whether the item is installed.
        /// </summary>
        /// <param name="installationContext">The installation context.</param>
        /// <returns>
        /// Value indicating whether the item is installed or null if it is not possible to determine.
        /// </returns>
        public bool? IsInstalled(InstallationContext installationContext)
        {
            if (!PerformanceCounterCategory.Exists(CategoryName))
            {
                return false;
            }

            return PerformanceCounterCategory.CounterExists(CounterName, CategoryName);
        }

        /// <summary>
        /// Increments the configured performance counter.
        /// </summary>
        /// <param name="logEvent">Log event.</param>
        protected override void Write(LogEventInfo logEvent)
        {
            if (EnsureInitialized())
            {
                string incrementValueString = IncrementValue.Render(logEvent);
                long incrementValue;
                if (long.TryParse(incrementValueString, out incrementValue))
                    perfCounter.IncrementBy(incrementValue);
                else
                    InternalLogger.Error("{0}: Error incrementing PerfCounter {1}. IncrementValue must be an integer but was <{2}>", this, CounterName, incrementValueString);
            }
        }

        /// <summary>
        /// Closes the target and releases any unmanaged resources.
        /// </summary>
        protected override void CloseTarget()
        {
            base.CloseTarget();

            if (perfCounter != null)
            {
                perfCounter.Close();
                perfCounter = null;
            }

            initialized = false;
        }

        private static CounterCreationDataCollection GetCounterCreationDataCollection(IEnumerable<PerformanceCounterTarget> countersInCategory, out PerformanceCounterCategoryType categoryType)
        {
            categoryType = PerformanceCounterCategoryType.SingleInstance;

            var ccds = new CounterCreationDataCollection();
            foreach (var counter in countersInCategory)
            {
                if (!string.IsNullOrEmpty(counter.InstanceName))
                {
                    categoryType = PerformanceCounterCategoryType.MultiInstance;
                }

                ccds.Add(new CounterCreationData(counter.CounterName, counter.CounterHelp, counter.CounterType));
            }

            return ccds;
        }

        /// <summary>
        /// Ensures that the performance counter has been initialized.
        /// </summary>
        /// <returns>True if the performance counter is operational, false otherwise.</returns>
        private bool EnsureInitialized()
        {
            if (!initialized)
            {
                initialized = true;

                if (AutoCreate)
                {
                    using (var context = new InstallationContext())
                    {
                        Install(context);
                    }
                }

                try
                {
                    perfCounter = new PerformanceCounter(CategoryName, CounterName, InstanceName, false);
                }
                catch (Exception exception)
                {
                    if (LogManager.ThrowExceptions)
                        throw;

                    InternalLogger.Error(exception, "{0}: Cannot open performance counter {1}/{2}/{3}.", this, CategoryName, CounterName, InstanceName);
                }
            }

            return perfCounter != null;
        }
    }
}