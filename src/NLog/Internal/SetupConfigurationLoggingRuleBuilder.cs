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
    using System.Collections;
    using System.Collections.Generic;
    using NLog.Config;
    using NLog.Targets;

    internal class SetupConfigurationLoggingRuleBuilder : ISetupConfigurationLoggingRuleBuilder, IList<Target>
    {
        public SetupConfigurationLoggingRuleBuilder(LogFactory logFactory, LoggingConfiguration configuration, string loggerNamePattern = null, string ruleName = null)
        {
            LoggingRule = new LoggingRule(ruleName) { LoggerNamePattern = loggerNamePattern ?? "*" };
            Configuration = configuration;
            LogFactory = logFactory;
        }

        /// <inheritdoc/>
        public LoggingRule LoggingRule { get; }

        /// <inheritdoc/>
        public LoggingConfiguration Configuration { get; }

        /// <inheritdoc/>
        public LogFactory LogFactory { get; }

        /// <summary>
        /// Collection of targets that should be written to
        /// </summary>
        public IList<Target> Targets => this;

        Target IList<Target>.this[int index] { get => LoggingRule.Targets[index]; set => LoggingRule.Targets[index] = value; }

        int ICollection<Target>.Count => LoggingRule.Targets.Count;

        bool ICollection<Target>.IsReadOnly => LoggingRule.Targets.IsReadOnly;

        void ICollection<Target>.Add(Target item)
        {
            if (!Configuration.LoggingRules.Contains(LoggingRule))
            {
                Configuration.LoggingRules.Add(LoggingRule);
            }

            LoggingRule.Targets.Add(item);
        }

        void ICollection<Target>.Clear()
        {
            LoggingRule.Targets.Clear();
        }

        bool ICollection<Target>.Contains(Target item)
        {
            return LoggingRule.Targets.Contains(item);
        }

        void ICollection<Target>.CopyTo(Target[] array, int arrayIndex)
        {
            LoggingRule.Targets.CopyTo(array, arrayIndex);
        }

        IEnumerator<Target> IEnumerable<Target>.GetEnumerator()
        {
            return LoggingRule.Targets.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return LoggingRule.Targets.GetEnumerator();
        }

        int IList<Target>.IndexOf(Target item)
        {
            return LoggingRule.Targets.IndexOf(item);
        }

        void IList<Target>.Insert(int index, Target item)
        {
            LoggingRule.Targets.Insert(index, item);
        }

        bool ICollection<Target>.Remove(Target item)
        {
            return LoggingRule.Targets.Remove(item);
        }

        void IList<Target>.RemoveAt(int index)
        {
            LoggingRule.Targets.RemoveAt(index);
        }
    }
}
