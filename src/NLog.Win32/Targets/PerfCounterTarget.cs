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
using System.IO;
using System.Text;
using System.Xml;
using System.Reflection;
using System.Diagnostics;
using System.Collections;

using NLog.Internal;
using System.Net;
using System.Net.Sockets;

using NLog.Config;

namespace NLog.Win32.Targets
{
    /// <summary>
    /// A target wrapper that increments specified perf counter on each write.
    /// </summary>
    [Target("PerfCounter")]
    public class PerfCounterTarget : Target
    {
        private bool _autoCreate = false;
        private string _categoryName;
        private string _counterName;
        private string _instanceName = "";
        private PerformanceCounterType _counterType = PerformanceCounterType.NumberOfItems32;
        private static ArrayList _perfCounterTargets = new ArrayList();
        private PerformanceCounter _perfCounter = null;
        private bool _operational = true;
            
        /// <summary>
        /// Creates a new instance of <see cref="PerfCounterTarget"/>.
        /// </summary>
        public PerfCounterTarget()
        {
            lock(_perfCounterTargets)
            {
                if (!_perfCounterTargets.Contains(this)) _perfCounterTargets.Add(this);
            }
        }



        
        protected override void Write(LogEventInfo logEvent)
        {
            if (!_operational) return;
            if (_perfCounter == null) InitializePerfCounter();
            if (_perfCounter == null) return; //not operational
            
            try 
            {
                _perfCounter.Increment();
            }
            catch(Exception ex) 
            {
                _operational = false;
                if (LogManager.ThrowExceptions) throw ex;
            }
        }
        
        public bool AutoCreate
        {
            get {return _autoCreate; }
            set {_autoCreate = value; }
        }
        
        [RequiredParameter]
        public string CategoryName
        {
            get {return _categoryName; }
            set {_categoryName = value; }
        }
        
        [RequiredParameter]
        public string CounterName
        {
            get {return _counterName; }
            set {_counterName = value; }
        }
        
        public string InstanceName
        {
            get {return _instanceName; }
            set {_instanceName = value; }
        }
        
        
        
        public PerformanceCounterType CounterType
        {
            get { return _counterType; }
            set { _counterType = value; }
        }
        
        private void InitializePerfCounter()
        {
            lock(this)
            {
                _operational = true;
                _perfCounter = null;
                if (_categoryName == null || _counterName == null) 
                {
                    throw new Exception("Missing category name or counter name for target: " + Name);
                    _operational = false;
                    return;
                }
                try
                {
                    
                    if (!PerformanceCounterCategory.Exists(CategoryName) || !PerformanceCounterCategory.CounterExists(CounterName, CategoryName))
                    {
                        ArrayList targets = new ArrayList();
                        bool doCreate = false;
                        foreach(PerfCounterTarget t in _perfCounterTargets)
                        {
                            if (t.CategoryName == CategoryName)
                            {
                                targets.Add(t);
                                if (t.AutoCreate) doCreate = true;
                            }
                        }
                        
                        if (doCreate)
                        {
                            if (PerformanceCounterCategory.Exists(CategoryName))
                            {
                                //we must delete the whole category and rebuild from scratch
                                PerformanceCounterCategory.Delete(CategoryName);
                            }
                            
                            CounterCreationDataCollection ccds = new CounterCreationDataCollection();
                            foreach(PerfCounterTarget t in targets)
                            {
                                CounterCreationData ccd = new CounterCreationData();
                                ccd.CounterName = t.CounterName;
                                ccd.CounterType = t.CounterType;  
                                ccds.Add(ccd);                                    
                            }
                            PerformanceCounterCategory.Create(CategoryName,"Category created by NLog",ccds);
                        }
                        else
                        {
                            throw new Exception(string.Format("Counter does not exist: {0}|{1}", CounterName, CategoryName));
                        }
                    }
                    
                    _perfCounter = new PerformanceCounter(CategoryName, CounterName, InstanceName, false);
                    _operational = true;
                }
                catch(Exception ex)
                {
                    _operational = false;
                    if (LogManager.ThrowExceptions) throw ex;
                }
            }
        }

    }
}
