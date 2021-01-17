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

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using NLog.Config;
using NLog.Layouts;

namespace NLog.Internal
{
    [DebuggerDisplay("Count = {Count}")]
    internal class ConfigVariablesDictionary : IDictionary<string, Layout>
    {
        private readonly ThreadSafeDictionary<string, Layout> _variables = new ThreadSafeDictionary<string, Layout>(StringComparer.OrdinalIgnoreCase);
        private readonly LoggingConfiguration _configuration;
        private ThreadSafeDictionary<string, Layout> _dynamicVariables;
        private ThreadSafeDictionary<string, bool> _apiVariables;

        public ConfigVariablesDictionary(LoggingConfiguration configuration)
        {
            _configuration = configuration;
        }

        public void InsertParsedConfigVariable(string key, Layout value, bool keepVariablesOnReload)
        {
            if (keepVariablesOnReload && _apiVariables?.ContainsKey(key)==true && _variables.ContainsKey(key))
                return;

            _variables[key] = value;
            _dynamicVariables?.Remove(key);
        }

        public bool TryLookupDynamicVariable(string key, out Layout dynamicLayout)
        {
            if (_dynamicVariables == null)
            {
                if (!_variables.TryGetValue(key, out dynamicLayout))
                    return false;

                System.Threading.Interlocked.CompareExchange(ref _dynamicVariables, new ThreadSafeDictionary<string, Layout>(_variables.Comparer), null);
            }

            bool variableExists = true;
            if (!_dynamicVariables.TryGetValue(key, out dynamicLayout))
            {
                variableExists = false;

                if (_variables.TryGetValue(key, out dynamicLayout))
                {
                    variableExists = true;

                    if (dynamicLayout != null)
                    {
                        dynamicLayout.Initialize(_configuration);
                        if (!dynamicLayout.ThreadSafe)
                        {
                            dynamicLayout = new ThreadSafeWrapLayout(dynamicLayout);
                            dynamicLayout.Initialize(_configuration);
                        }
                    }
                    
                    _dynamicVariables[key] = dynamicLayout;
                }
            }

            return variableExists;
        }

        public void PrepareForReload(ConfigVariablesDictionary oldVariables)
        {
            if (oldVariables._apiVariables != null)
            {
                foreach (var item in oldVariables._apiVariables)
                {
                    if (oldVariables._variables.TryGetValue(item.Key, out var value))
                    {
                        _variables[item.Key] = value;   // Reload will close the old-config and initialize the new-config (disconnects layout from old-config)
                        RegisterApiVariable(item.Key);
                    }
                }
            }
        }

        public int Count => _variables.Count;

        public ICollection<string> Keys => _variables.Keys;

        public ICollection<Layout> Values => _variables.Values;

        public bool ContainsKey(string key) => _variables.ContainsKey(key);

        public bool TryGetValue(string key, out Layout value) => _variables.TryGetValue(key, out value);

        IEnumerator<KeyValuePair<string, Layout>> IEnumerable<KeyValuePair<string, Layout>>.GetEnumerator() => _variables.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => _variables.GetEnumerator();

        public ThreadSafeDictionary<string, Layout>.Enumerator GetEnumerator() => _variables.GetEnumerator();

        public Layout this[string key]
        {
            get
            {
                return _variables[key];
            }
            set
            {
                _variables[key] = value;
                RegisterApiVariable(key);
            }
        }

        public void Add(string key, Layout value)
        {
            _variables.Add(key, value);
            RegisterApiVariable(key);
        }

        public bool Remove(string key)
        {
            _apiVariables?.Remove(key);
            _dynamicVariables?.Remove(key);
            return _variables.Remove(key);
        }

        public void Clear()
        {
            _variables.Clear();
            _apiVariables?.Clear();
            _dynamicVariables?.Clear();
        }

        bool ICollection<KeyValuePair<string, Layout>>.IsReadOnly => false;

        bool ICollection<KeyValuePair<string, Layout>>.Contains(KeyValuePair<string, Layout> item) => _variables.Contains(item);

        void ICollection<KeyValuePair<string, Layout>>.CopyTo(KeyValuePair<string, Layout>[] array, int arrayIndex) => _variables.CopyTo(array, arrayIndex);

        void ICollection<KeyValuePair<string, Layout>>.Add(KeyValuePair<string, Layout> item) => Add(item.Key, item.Value);

        bool ICollection<KeyValuePair<string, Layout>>.Remove(KeyValuePair<string, Layout> item) => Remove(item.Key);

        private void RegisterApiVariable(string key)
        {
            if (_apiVariables == null)
            {
                System.Threading.Interlocked.CompareExchange(ref _apiVariables, new ThreadSafeDictionary<string, bool>(_variables.Comparer), null);
            }
            _apiVariables[key] = true;
            _dynamicVariables?.Remove(key);
        }

        [ThreadAgnostic]
        [ThreadSafe]
        [AppDomainFixedOutput]
        class ThreadSafeWrapLayout : Layout
        {
            private readonly object _lockObject = new object();

            public Layout Unsafe { get; }

            public ThreadSafeWrapLayout(Layout layout)
            {
                Unsafe = layout;
                ThreadSafe = true;
                ThreadAgnostic = true;
            }

            protected override void InitializeLayout()
            {
                lock (_lockObject)
                {
                    base.InitializeLayout();
                    ThreadSafe = true;
                }
            }

            public override void Precalculate(LogEventInfo logEvent)
            {
                lock (_lockObject)
                    Unsafe.Precalculate(logEvent);
            }

            internal override void PrecalculateBuilder(LogEventInfo logEvent, StringBuilder target)
            {
                lock (_lockObject)
                    Unsafe.PrecalculateBuilderInternal(logEvent, target);
            }

            protected override string GetFormattedMessage(LogEventInfo logEvent)
            {
                lock (_lockObject)
                    return Unsafe.RenderAllocateBuilder(logEvent);
            }

            protected override void RenderFormattedMessage(LogEventInfo logEvent, StringBuilder target)
            {
                lock (_lockObject)
                    Unsafe.RenderAppendBuilder(logEvent, target);
            }

            public override string ToString()
            {
                return Unsafe.ToString();
            }
        }
    }
}
