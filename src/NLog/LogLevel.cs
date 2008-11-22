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

namespace NLog
{
    /// <summary>
    /// Defines available log levels.
    /// </summary>
    public class LogLevel : IComparable
    {
        private string _name;
        private int _ordinal;

        private static LogLevel _trace;
        private static LogLevel _debug;
        private static LogLevel _info;
        private static LogLevel _warn;
        private static LogLevel _error;
        private static LogLevel _fatal;

        private static LogLevel _off;
        private static LogLevel _maxLevel;
        private static LogLevel _minLevel;

        /// <summary>
        /// The Trace level.
        /// </summary>
        public static LogLevel Trace
        {
            get { return _trace; }
        }

        /// <summary>
        /// The Debug level.
        /// </summary>
        public static LogLevel Debug
        {
            get { return _debug; }
        }

        /// <summary>
        /// The Info level.
        /// </summary>
        public static LogLevel Info
        {
            get { return _info; }
        }

        /// <summary>
        /// The Warn level.
        /// </summary>
        public static LogLevel Warn
        {
            get { return _warn; }
        }

        /// <summary>
        /// The Error level.
        /// </summary>
        public static LogLevel Error
        {
            get { return _error; }
        }

        /// <summary>
        /// The Fatal level.
        /// </summary>
        public static LogLevel Fatal
        {
            get { return _fatal; }
        }

        /// <summary>
        /// The Off level.
        /// </summary>
        public static LogLevel Off
        {
            get { return _off; }
        }

        internal static LogLevel MaxLevel
        {
            get { return _maxLevel; }
        }

        internal static LogLevel MinLevel
        {
            get { return _minLevel; }
        }

        private static LogLevel[] _levelByOrdinal;

        static LogLevel()
        {
            int l = 0;

            _trace = new LogLevel("Trace", l++);
            _debug = new LogLevel("Debug", l++);
            _info = new LogLevel("Info", l++);
            _warn = new LogLevel("Warn", l++);
            _error = new LogLevel("Error", l++);
            _fatal = new LogLevel("Fatal", l++);
            _off = new LogLevel("Off", l++);

            _levelByOrdinal = new LogLevel[] { Trace, Debug, Info, Warn, Error, Fatal, Off };
            _minLevel = _levelByOrdinal[0];
            _maxLevel = _levelByOrdinal[_levelByOrdinal.Length - 2]; // ignore the Off level
        }

        // to be changed into public in the future.
        private LogLevel(string name, int ordinal)
        {
            _name = name;
            _ordinal = ordinal;
        }

        /// <summary>
        /// Gets the name of the log level.
        /// </summary>
        public string Name
        {
            get { return _name; }
        }

        internal int Ordinal
        {
            get { return _ordinal; }
        }

        /// <summary>
        /// Gets the <see cref="LogLevel"/> that corresponds to the specified ordinal.
        /// </summary>
        /// <param name="ordinal">The ordinal.</param>
        /// <returns>The <see cref="LogLevel"/> instance. For 0 it returns <see cref="LogLevel.Debug"/>, 1 gives <see cref="LogLevel.Info"/> and so on</returns>
        public static LogLevel FromOrdinal(int ordinal)
        {
            return _levelByOrdinal[ordinal];
        }

        /// <summary>
        /// Compares two <see cref="LogLevel"/> objects 
        /// and returns a value indicating whether 
        /// the first one is less than or equal to the second one.
        /// </summary>
        /// <param name="l1">The first level.</param>
        /// <param name="l2">The second level.</param>
        /// <returns>The value of <c>l1.Ordinal &lt;= l2.Ordinal</c></returns>
        public static bool operator <=(LogLevel l1, LogLevel l2)
        {
            return l1.Ordinal <= l2.Ordinal;
        }

        /// <summary>
        /// Compares two <see cref="LogLevel"/> objects 
        /// and returns a value indicating whether 
        /// the first one is greater than or equal to the second one.
        /// </summary>
        /// <param name="l1">The first level.</param>
        /// <param name="l2">The second level.</param>
        /// <returns>The value of <c>l1.Ordinal &gt;= l2.Ordinal</c></returns>
        public static bool operator >=(LogLevel l1, LogLevel l2)
        {
            return l1.Ordinal >= l2.Ordinal;
        }

        /// <summary>
        /// Compares two <see cref="LogLevel"/> objects 
        /// and returns a value indicating whether 
        /// the first one is less than the second one.
        /// </summary>
        /// <param name="l1">The first level.</param>
        /// <param name="l2">The second level.</param>
        /// <returns>The value of <c>l1.Ordinal &lt; l2.Ordinal</c></returns>
        public static bool operator <(LogLevel l1, LogLevel l2)
        {
            return l1.Ordinal < l2.Ordinal;
        }

        /// <summary>
        /// Compares two <see cref="LogLevel"/> objects 
        /// and returns a value indicating whether 
        /// the first one is greater than the second one.
        /// </summary>
        /// <param name="l1">The first level.</param>
        /// <param name="l2">The second level.</param>
        /// <returns>The value of <c>l1.Ordinal &gt; l2.Ordinal</c></returns>
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
                if (0 == String.Compare(_levelByOrdinal[i].Name, s, StringComparison.InvariantCultureIgnoreCase))
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

        /// <summary>
        /// Compares the level to the other <see cref="LogLevel"/> object.
        /// </summary>
        /// <param name="obj">the object object</param>
        /// <returns>a value less than zero when this logger's <see cref="Ordinal"/> is 
        /// less than the other logger's ordinal, 0 when they are equal and 
        /// greater than zero when this ordinal is greater than the
        /// other ordinal.</returns>
        public int CompareTo(object obj)
        {
            LogLevel l = (LogLevel)obj;
            return this.Ordinal - l.Ordinal;
        }
    } 
}
