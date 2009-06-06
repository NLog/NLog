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
        private static LogLevel[] levelByOrdinal;

        /// <summary>
        /// Initializes static members of the LogLevel class.
        /// </summary>
        static LogLevel()
        {
            int l = 0;

            Trace = new LogLevel("Trace", l++);
            Debug = new LogLevel("Debug", l++);
            Info = new LogLevel("Info", l++);
            Warn = new LogLevel("Warn", l++);
            Error = new LogLevel("Error", l++);
            Fatal = new LogLevel("Fatal", l++);
            Off = new LogLevel("Off", l++);

            levelByOrdinal = new LogLevel[] { Trace, Debug, Info, Warn, Error, Fatal, Off };
            MaxLevel = levelByOrdinal[0];
            MaxLevel = levelByOrdinal[levelByOrdinal.Length - 2]; // ignore the Off level
        }

        // to be changed into public in the future.
        private LogLevel(string name, int ordinal)
        {
            this.Name = name;
            this.Ordinal = ordinal;
        }

        /// <summary>
        /// Gets the Debug level.
        /// </summary>
        public static LogLevel Debug { get; private set; }

        /// <summary>
        /// Gets the Error level.
        /// </summary>
        public static LogLevel Error { get; private set; }

        /// <summary>
        /// Gets the Fatal level.
        /// </summary>
        public static LogLevel Fatal { get; private set; }

        /// <summary>
        /// Gets the Info level.
        /// </summary>
        public static LogLevel Info { get; private set; }

        /// <summary>
        /// Gets the Off level.
        /// </summary>
        public static LogLevel Off { get; private set; }

        /// <summary>
        /// Gets the Trace level.
        /// </summary>
        public static LogLevel Trace { get; private set; }

        /// <summary>
        /// Gets the Warn level.
        /// </summary>
        public static LogLevel Warn { get; private set; }

        /// <summary>
        /// Gets the name of the log level.
        /// </summary>
        public string Name { get; private set; }

        internal static LogLevel MaxLevel { get; set; }

        internal static LogLevel MinLevel { get; set; }

        internal int Ordinal { get; private set; }

        /// <summary>
        /// Compares two <see cref="LogLevel"/> objects 
        /// and returns a value indicating whether 
        /// the first one is greater than the second one.
        /// </summary>
        /// <param name="l1">The first level.</param>
        /// <param name="l2">The second level.</param>
        /// <returns>The value of <c>l1.Ordinal &gt; l2.Ordinal</c>.</returns>
        public static bool operator >(LogLevel l1, LogLevel l2)
        {
            return l1.Ordinal > l2.Ordinal;
        }

        /// <summary>
        /// Compares two <see cref="LogLevel"/> objects 
        /// and returns a value indicating whether 
        /// the first one is greater than or equal to the second one.
        /// </summary>
        /// <param name="l1">The first level.</param>
        /// <param name="l2">The second level.</param>
        /// <returns>The value of <c>l1.Ordinal &gt;= l2.Ordinal</c>.</returns>
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
        /// <returns>The value of <c>l1.Ordinal &lt; l2.Ordinal</c>.</returns>
        public static bool operator <(LogLevel l1, LogLevel l2)
        {
            return l1.Ordinal < l2.Ordinal;
        }

        /// <summary>
        /// Compares two <see cref="LogLevel"/> objects 
        /// and returns a value indicating whether 
        /// the first one is less than or equal to the second one.
        /// </summary>
        /// <param name="l1">The first level.</param>
        /// <param name="l2">The second level.</param>
        /// <returns>The value of <c>l1.Ordinal &lt;= l2.Ordinal</c>.</returns>
        public static bool operator <=(LogLevel l1, LogLevel l2)
        {
            return l1.Ordinal <= l2.Ordinal;
        }

        /// <summary>
        /// Gets the <see cref="LogLevel"/> that corresponds to the specified ordinal.
        /// </summary>
        /// <param name="ordinal">The ordinal.</param>
        /// <returns>The <see cref="LogLevel"/> instance. For 0 it returns <see cref="LogLevel.Debug"/>, 1 gives <see cref="LogLevel.Info"/> and so on.</returns>
        public static LogLevel FromOrdinal(int ordinal)
        {
            return levelByOrdinal[ordinal];
        }

        /// <summary>
        /// Returns the <see cref="T:NLog.LogLevel"/> that corresponds to the supplied <see langword="string" />.
        /// </summary>
        /// <param name="levelString">The texual representation of the log level.</param>
        /// <returns>The enumeration value.</returns>
        public static LogLevel FromString(string levelString)
        {
            // case sensitive search first
            for (int i = 0; i < levelByOrdinal.Length; ++i)
            {
                if (levelByOrdinal[i].Name == levelString)
                {
                    return levelByOrdinal[i];
                }
            }

            // case insensitive search
            for (int i = 0; i < levelByOrdinal.Length; ++i)
            {
                if (0 == String.Compare(levelByOrdinal[i].Name, levelString, StringComparison.InvariantCultureIgnoreCase))
                {
                    return levelByOrdinal[i];
                }
            }

            throw new ArgumentException("Unknown log level: " + levelString);
        }

        /// <summary>
        /// Returns a string representation of the log level.
        /// </summary>
        /// <returns>Log level name.</returns>
        public override string ToString()
        {
            return this.Name;
        }

        /// <summary>
        /// Compares the level to the other <see cref="LogLevel"/> object.
        /// </summary>
        /// <param name="obj">
        /// The object object.
        /// </param>
        /// <returns>
        /// A value less than zero when this logger's <see cref="Ordinal"/> is 
        /// less than the other logger's ordinal, 0 when they are equal and 
        /// greater than zero when this ordinal is greater than the
        /// other ordinal.
        /// </returns>
        public int CompareTo(object obj)
        {
            var level = (LogLevel)obj;

            return this.Ordinal - level.Ordinal;
        }
    } 
}
