// 
// Copyright (c) 2004-2021 Jaroslaw Kowalski <jaak@jkowalski.net>, Kim Christensen, Julian Verdurmen
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

namespace NLog.Internal
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using NLog.Config;
    using NLog.Targets;

    internal sealed class SetupConfigurationTargetBuilder : ISetupConfigurationTargetBuilder, IList<Target>
    {
        private readonly IList<Target> _targets = new List<Target>();
        private string _targetName;

        public SetupConfigurationTargetBuilder(LogFactory logFactory, LoggingConfiguration configuration, string targetName = null)
        {
            Configuration = configuration;
            LogFactory = logFactory;
            _targetName = string.IsNullOrEmpty(targetName) ? null : targetName;
        }

        public LoggingConfiguration Configuration { get; }

        public LogFactory LogFactory { get; }

        public IList<Target> Targets => this;

        private void UpdateTargetName(Target item)
        {
            if (!string.IsNullOrEmpty(_targetName))
            {
                item.Name = _targetName;
                _targetName = string.Empty; // Mark that target-name has been used
            }
            else if (_targetName == string.Empty)
            {
                throw new ArgumentException("Cannot apply the same Target-Name to multiple targets");
            }
        }

        Target IList<Target>.this[int index] { get => _targets[index]; set => _targets[index] = value; }

        int ICollection<Target>.Count => _targets.Count;

        bool ICollection<Target>.IsReadOnly => _targets.IsReadOnly;

        void ICollection<Target>.Add(Target item)
        {
            UpdateTargetName(item);
            _targets.Add(item);
        }

        void ICollection<Target>.Clear()
        {
            _targets.Clear();
        }

        bool ICollection<Target>.Contains(Target item)
        {
            return _targets.Contains(item);
        }

        void ICollection<Target>.CopyTo(Target[] array, int arrayIndex)
        {
            _targets.CopyTo(array, arrayIndex);
        }

        IEnumerator<Target> IEnumerable<Target>.GetEnumerator()
        {
            return _targets.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return _targets.GetEnumerator();
        }

        int IList<Target>.IndexOf(Target item)
        {
            return _targets.IndexOf(item);
        }

        void IList<Target>.Insert(int index, Target item)
        {
            UpdateTargetName(item);
            _targets.Insert(index, item);
        }

        bool ICollection<Target>.Remove(Target item)
        {
            return _targets.Remove(item);
        }

        void IList<Target>.RemoveAt(int index)
        {
            _targets.RemoveAt(index);
        }
    }
}
