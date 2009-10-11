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

using System;

namespace NLog
{
    /// <summary>
    /// Defines available log levels.
    /// </summary>
    public class LogLevel : IComparable
    {
        private static readonly LogLevel[] levelByOrdinal;

        /// <summary>
        /// Trace log level.
        /// </summary>
        public static readonly LogLevel Trace;

        /// <summary>
        /// Debug log level.
        /// </summary>
        public static readonly LogLevel Debug;

        /// <summary>
        /// Info log level.
        /// </summary>
        public static readonly LogLevel Info;

        /// <summary>
        /// Warn log level.
        /// </summary>
        public static readonly LogLevel Warn;

        /// <summary>
        /// Error log level.
        /// </summary>
        public static readonly LogLevel Error;

        /// <summary>
        /// Fatal log level.
        /// </summary>
        public static readonly LogLevel Fatal;

        /// <summary>
        /// Off log level.
        /// </summary>
        public static readonly LogLevel Off;

        /// <summary>
        /// Initializes static members of the LogLevel class.
        /// </summary>
        static LogLevel()
        {
            int ordinal = 0;

            Trace = new LogLevel("Trace", ordinal++);
            Debug = new LogLevel("Debug", ordinal++);
            Info = new LogLevel("Info", ordinal++);
            Warn = new LogLevel("Warn", ordinal++);
            Error = new LogLevel("Error", ordinal++);
            Fatal = new LogLevel("Fatal", ordinal++);
            Off = new LogLevel("Off", ordinal);

            levelByOrdinal = new[] { Trace, Debug, Info, Warn, Error, Fatal, Off };
            MinLevel = levelByOrdinal[0];
            MaxLevel = levelByOrdinal[levelByOrdinal.Length - 2]; // ignore the Off level
        }

        // to be changed into public in the future.
        private LogLevel(string name, int ordinal)
        {
            this.Name = name;
            this.Ordinal = ordinal;
        }

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
        /// the first one is equal to the second one.
        /// </summary>
        /// <param name="level1">The first level.</param>
        /// <param name="level2">The second level.</param>
        /// <returns>The value of <c>level1.Ordinal == level2.Ordinal</c>.</returns>
        public static bool operator ==(LogLevel level1, LogLevel level2)
        {
            return level1.Ordinal == level2.Ordinal;
        }

        /// <summary>
        /// Compares two <see cref="LogLevel"/> objects 
        /// and returns a value indicating whether 
        /// the first one is not equal to the second one.
        /// </summary>
        /// <param name="level1">The first level.</param>
        /// <param name="level2">The second level.</param>
        /// <returns>The value of <c>level1.Ordinal != level2.Ordinal</c>.</returns>
        public static bool operator !=(LogLevel level1, LogLevel level2)
        {
            return level1.Ordinal != level2.Ordinal;
        }

        /// <summary>
        /// Compares two <see cref="LogLevel"/> objects 
        /// and returns a value indicating whether 
        /// the first one is greater than the second one.
        /// </summary>
        /// <param name="level1">The first level.</param>
        /// <param name="level2">The second level.</param>
        /// <returns>The value of <c>level1.Ordinal &gt; level2.Ordinal</c>.</returns>
        public static bool operator >(LogLevel level1, LogLevel level2)
        {
            return level1.Ordinal > level2.Ordinal;
        }

        /// <summary>
        /// Compares two <see cref="LogLevel"/> objects 
        /// and returns a value indicating whether 
        /// the first one is greater than or equal to the second one.
        /// </summary>
        /// <param name="level1">The first level.</param>
        /// <param name="level2">The second level.</param>
        /// <returns>The value of <c>level1.Ordinal &gt;= level2.Ordinal</c>.</returns>
        public static bool operator >=(LogLevel level1, LogLevel level2)
        {
            return level1.Ordinal >= level2.Ordinal;
        }

        /// <summary>
        /// Compares two <see cref="LogLevel"/> objects 
        /// and returns a value indicating whether 
        /// the first one is less than the second one.
        /// </summary>
        /// <param name="level1">The first level.</param>
        /// <param name="level2">The second level.</param>
        /// <returns>The value of <c>level1.Ordinal &lt; level2.Ordinal</c>.</returns>
        public static bool operator <(LogLevel level1, LogLevel level2)
        {
            return level1.Ordinal < level2.Ordinal;
        }

        /// <summary>
        /// Compares two <see cref="LogLevel"/> objects 
        /// and returns a value indicating whether 
        /// the first one is less than or equal to the second one.
        /// </summary>
        /// <param name="level1">The first level.</param>
        /// <param name="level2">The second level.</param>
        /// <returns>The value of <c>level1.Ordinal &lt;= level2.Ordinal</c>.</returns>
        public static bool operator <=(LogLevel level1, LogLevel level2)
        {
            return level1.Ordinal <= level2.Ordinal;
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
        /// <param name="levelName">The texual representation of the log level.</param>
        /// <returns>The enumeration value.</returns>
        public static LogLevel FromString(string levelName)
        {
            // case sensitive search first
            for (int i = 0; i < levelByOrdinal.Length; ++i)
            {
                if (0 == String.Compare(levelByOrdinal[i].Name, levelName, StringComparison.OrdinalIgnoreCase))
                {
                    return levelByOrdinal[i];
                }
            }

            // case insensitive search
            for (int i = 0; i < levelByOrdinal.Length; ++i)
            {
                if (0 == String.Compare(levelByOrdinal[i].Name, levelName, StringComparison.OrdinalIgnoreCase))
                {
                    return levelByOrdinal[i];
                }
            }

            throw new ArgumentException("Unknown log level: " + levelName);
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
        /// Returns a hash code for this instance.
        /// </summary>
        /// <returns>
        /// A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table. 
        /// </returns>
        public override int GetHashCode()
        {
            return this.Ordinal;
        }

        /// <summary>
        /// Determines whether the specified <see cref="System.Object"/> is equal to this instance.
        /// </summary>
        /// <param name="obj">The <see cref="System.Object"/> to compare with this instance.</param>
        /// <returns>
        /// Value of <c>true</c> if the specified <see cref="System.Object"/> is equal to this instance; otherwise, <c>false</c>.
        /// </returns>
        /// <exception cref="T:System.NullReferenceException">
        /// The <paramref name="obj"/> parameter is null.
        /// </exception>
        public override bool Equals(object obj)
        {
            LogLevel other = obj as LogLevel;
            if ((object)other == null)
            {
                return false;
            }

            return this.Ordinal == other.Ordinal;
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
