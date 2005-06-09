// 
// Copyright (c) 2004,2005 Jaroslaw Kowalski <jkowalski@users.sourceforge.net>
// 
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

namespace NLog
{
    /// <summary>
    /// Defines available log levels.
    /// </summary>
    public class LogLevel
    {
        private string _name;
        private string _uppercaseName;
        private string _lowercaseName;
        private int _ordinal;

        /// <summary>
        /// The Debug level.
        /// </summary>
        public static readonly LogLevel Debug = new LogLevel("Debug", 0);

        /// <summary>
        /// The Info level.
        /// </summary>
        public static readonly LogLevel Info = new LogLevel("Info", 1);

        /// <summary>
        /// The Warn level.
        /// </summary>
        public static readonly LogLevel Warn = new LogLevel("Warn", 2);

        /// <summary>
        /// The Error level.
        /// </summary>
        public static readonly LogLevel Error = new LogLevel("Error", 3);

        /// <summary>
        /// The Fatal level.
        /// </summary>
        public static readonly LogLevel Fatal = new LogLevel("Fatal", 4);

        /// <summary>
        /// The Off level.
        /// </summary>
        public static readonly LogLevel Off = new LogLevel("Off", 5);

        public static readonly LogLevel MaxLevel = Fatal;

        private static LogLevel[] _levelByOrdinal;

        static LogLevel()
        {
            Debug = new LogLevel("Debug", 0);
            Info = new LogLevel("Info", 1);
            Warn = new LogLevel("Warn", 2);
            Error = new LogLevel("Error", 3);
            Fatal = new LogLevel("Fatal", 4);
            Off = new LogLevel("Off", 5);

            _levelByOrdinal = new LogLevel[] { Debug, Info, Warn, Error, Fatal, Off };
            MaxLevel = Fatal;
        }

        public LogLevel(string name, int ordinal)
        {
            _name = name;
            _uppercaseName = name.ToUpper();
            _lowercaseName = name.ToLower();
            _ordinal = ordinal;
        }

        public string Name
        {
            get { return _name; }
        }

        internal int Ordinal
        {
            get { return _ordinal; }
        }

        public static LogLevel FromOrdinal(int ordinal)
        {
            return _levelByOrdinal[ordinal];
        }

        public static bool operator <=(LogLevel l1, LogLevel l2)
        {
            return l1.Ordinal <= l2.Ordinal;
        }

        public static bool operator >=(LogLevel l1, LogLevel l2)
        {
            return l1.Ordinal >= l2.Ordinal;
        }

        public static bool operator <(LogLevel l1, LogLevel l2)
        {
            return l1.Ordinal < l2.Ordinal;
        }

        public static bool operator >(LogLevel l1, LogLevel l2)
        {
            return l1.Ordinal > l2.Ordinal;
        }

        /// <summary>
        /// Returns the <see cref="T:NLog.LogLevel"/> that corresponds to the supplied <see langword="string" />.
        /// </summary>
        /// <param name="s">the texual representation of the log level</param>
        /// <returns>the enumeration value.</returns>
        public static LogLevel FromString(string s)
        {
            // case sensitive search first
            for (int i = 0; i < _levelByOrdinal.Length; ++i)
            {
                if (_levelByOrdinal[i].Name == s)
                    return _levelByOrdinal[i];
            }

            // case insensitive search
            for (int i = 0; i < _levelByOrdinal.Length; ++i)
            {
                if (0 == String.Compare(_levelByOrdinal[i].Name, s, true))
                    return _levelByOrdinal[i];
            }
            throw new ArgumentException("Unknown log level: " + s);
        }

        /// <summary>
        /// Returns a string representation of the log level.
        /// </summary>
        /// <returns>Log level name.</returns>
        public override string ToString()
        {
            return Name;
        }
    } 
}
