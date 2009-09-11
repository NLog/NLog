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

#if !NET_CF && !SILVERLIGHT

using System;
using System.Collections;
using System.ComponentModel;
using System.Diagnostics;
using NLog.Config;
using NLog.Targets;

namespace NLog.Win32.Targets
{
    /// <summary>
    /// Increments specified performance counter on each write.
    /// </summary>
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
    public class PerformanceCounterTarget : Target
    {
        private static ArrayList perfCounterTargets = new ArrayList();

        private PerformanceCounter perfCounter;
        private bool operational = true;

        /// <summary>
        /// Initializes a new instance of the PerfCounterTarget class.
        /// </summary>
        public PerformanceCounterTarget()
        {
            this.CounterType = PerformanceCounterType.NumberOfItems32;
            this.InstanceName = string.Empty;

            lock (perfCounterTargets)
            {
                if (!perfCounterTargets.Contains(this))
                {
                    perfCounterTargets.Add(this);
                }
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether performance counter should be automatically created.
        /// </summary>
        public bool AutoCreate { get; set; }

        /// <summary>
        /// Gets or sets the name of the performance counter category.
        /// </summary>
        [RequiredParameter]
        public string CategoryName { get; set; }

        /// <summary>
        /// Gets or sets the name of the performance counter.
        /// </summary>
        [RequiredParameter]
        public string CounterName { get; set; }

        /// <summary>
        /// Gets or sets the performance counter instance name.
        /// </summary>
        public string InstanceName { get; set; }

        /// <summary>
        /// Gets or sets the performance counter type.
        /// </summary>
        public PerformanceCounterType CounterType { get; set; }

        /// <summary>
        /// Increments the configured performance counter.
        /// </summary>
        /// <param name="logEvent">Log event.</param>
        protected internal override void Write(LogEventInfo logEvent)
        {
            if (!this.operational)
            {
                return;
            }

            if (this.perfCounter == null)
            {
                this.InitializePerfCounter();
            }

            if (this.perfCounter == null)
            {
                // not operational
                return;
            }

            bool ok = false;

            try
            {
                this.perfCounter.Increment();
                ok = true;
            }
            finally
            {
                this.operational = ok;
            }
        }

        private void InitializePerfCounter()
        {
            lock (this)
            {
                this.operational = true;
                try
                {
                    if (this.perfCounter != null)
                    {
                        this.perfCounter.Close(); 
                        this.perfCounter = null;
                    }

                    if (this.CategoryName == null || this.CounterName == null)
                    {
                        throw new Win32Exception("Missing category name or counter name for target: " + this.Name);
                    }

                    if (!PerformanceCounterCategory.Exists(this.CategoryName) || !PerformanceCounterCategory.CounterExists(this.CounterName, this.CategoryName))
                    {
                        ArrayList targets = new ArrayList();
                        bool doCreate = false;
                        foreach (PerformanceCounterTarget t in perfCounterTargets)
                        {
                            if (t.CategoryName == this.CategoryName)
                            {
                                targets.Add(t);
                                if (t.AutoCreate)
                                {
                                    doCreate = true;
                                }
                            }
                        }

                        if (doCreate)
                        {
                            if (PerformanceCounterCategory.Exists(this.CategoryName))
                            {
                                // delete the whole category and rebuild from scratch
                                PerformanceCounterCategory.Delete(this.CategoryName);
                            }

                            CounterCreationDataCollection ccds = new CounterCreationDataCollection();
                            foreach (PerformanceCounterTarget t in targets)
                            {
                                CounterCreationData ccd = new CounterCreationData();
                                ccd.CounterName = t.CounterName;
                                ccd.CounterType = t.CounterType;
                                ccds.Add(ccd);
                            }

                            PerformanceCounterCategory.Create(
                                this.CategoryName,
                                "Category created by NLog",
                                (this.InstanceName != null) ? PerformanceCounterCategoryType.MultiInstance : PerformanceCounterCategoryType.SingleInstance,
                                ccds);
                        }
                        else
                        {
                            throw new Win32Exception(string.Format("Counter does not exist: {0}|{1}", this.CounterName, this.CategoryName));
                        }
                    }

                    this.perfCounter = new PerformanceCounter(this.CategoryName, this.CounterName, this.InstanceName, false);
                    this.operational = true;
                }
                catch (Exception)
                {
                    this.operational = false;
                    this.perfCounter = null;
                    if (LogManager.ThrowExceptions)
                    {
                        throw;
                    }
                }
            }
        }
    }
}

#endif
