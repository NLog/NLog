// 
// Copyright (c) 2004 Jaroslaw Kowalski <jaak@polbox.com>
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
// * Neither the name of the Jaroslaw Kowalski nor the names of its 
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
using System.Collections.Specialized;

using NLog;
using NLog.Appenders;
using NLog.Filters;

namespace NLog.Config
{
    public class AppenderRule
    {
        internal enum MatchMode
        {
            All,
            None,
            Equals,
            StartsWith,
            EndsWith,
            Contains,
        }

        private string _loggerNamePattern;
        private MatchMode _loggerNameMatchMode;
        private string _loggerNameMatchArgument;

        private bool[] _logLevels = new bool[(int)LogLevel.MaxLevel + 1];
        private ArrayList _appenderNames = new ArrayList();
        private AppenderCollection _appenders = new AppenderCollection();
        private FilterCollection _filters = new FilterCollection();
        private bool _final = false;

        public IList AppenderNames
        {
            get { return _appenderNames; }
        }

        public AppenderCollection Appenders
        {
            get { return _appenders; }
        }

        public FilterCollection Filters
        {
            get { return _filters; }
        }

        public bool Final
        {
            get { return _final; }
            set { _final = value; }
        }

        public AppenderRule()
        {
        }

        public AppenderRule(string loggerNamePattern, string appenderName, params LogLevel[] levels)
        {
            _loggerNamePattern = loggerNamePattern;
            _appenderNames.Add(appenderName);
            foreach (LogLevel ll in levels)
            {
                _logLevels[(int)ll] = true;
            }
        }

        public void EnableLoggingForLevel(LogLevel level)
        {
            _logLevels[(int)level] = true;
        }

        public void DisableLoggingForLevel(LogLevel level)
        {
            _logLevels[(int)level] = false;
        }

        public bool IsLoggingEnabledForLevel(LogLevel level)
        {
            return _logLevels[(int)level];
        }

        public string LoggerNamePattern
        {
            get { return _loggerNamePattern; }
            set 
            {
                _loggerNamePattern = value;
                int firstPos = _loggerNamePattern.IndexOf('*');
                int lastPos = _loggerNamePattern.LastIndexOf('*');

                if (firstPos < 0)
                {
                    _loggerNameMatchMode = MatchMode.Equals;
                    _loggerNameMatchArgument = value;
                    return;
                }

                if (firstPos == lastPos)
                {
                    string before = LoggerNamePattern.Substring(0, firstPos);
                    string after = LoggerNamePattern.Substring(firstPos + 1);

                    if (before.Length > 0) 
                    {
                        _loggerNameMatchMode = MatchMode.StartsWith;
                        _loggerNameMatchArgument = before;
                        return;
                    }

                    if (after.Length > 0) 
                    {
                        _loggerNameMatchMode = MatchMode.EndsWith;
                        _loggerNameMatchArgument = after;
                        return;
                    }
                    return;
                }

                // *text*
                if (firstPos == 0 && lastPos == LoggerNamePattern.Length - 1) 
                {
                    string text = LoggerNamePattern.Substring(1, LoggerNamePattern.Length - 2);
                    _loggerNameMatchMode = MatchMode.Contains;
                    _loggerNameMatchArgument = text;
                    return;
                }

                _loggerNameMatchMode = MatchMode.None;
                _loggerNameMatchArgument = String.Empty;
            }
        }

        public bool Matches(string loggerName) 
        {
            switch (_loggerNameMatchMode)
            {
                case MatchMode.All:
                    return true;

                default:
                case MatchMode.None:
                    return false;

                case MatchMode.Equals:
                    return String.CompareOrdinal(loggerName, _loggerNameMatchArgument) == 0;

                case MatchMode.StartsWith:
                    return loggerName.StartsWith(_loggerNameMatchArgument);

                case MatchMode.EndsWith:
                    return loggerName.EndsWith(_loggerNameMatchArgument);

                case MatchMode.Contains:
                    return loggerName.IndexOf(_loggerNameMatchArgument) >= 0;
            }
        }

        public void Resolve(LoggingConfiguration configuration)
        {
            foreach (string s in AppenderNames) 
            {
                Appender app = configuration.FindAppenderByName(s);

                if (app != null) 
                {
                    Appenders.Add(app);
                }
            }
        }
    }
}
