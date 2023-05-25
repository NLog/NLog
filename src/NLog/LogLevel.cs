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

namespace NLog
{
    using NLog.Internal;
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;

    /// <summary>
    /// Defines available log levels.
    /// </summary>
    /// <remarks>
    /// Log levels ordered by severity:<br/>
    /// - <see cref="LogLevel.Trace"/> (Ordinal = 0) : Most verbose level. Used for development and seldom enabled in production.<br/>
    /// - <see cref="LogLevel.Debug"/> (Ordinal = 1) : Debugging the application behavior from internal events of interest.<br/>
    /// - <see cref="LogLevel.Info"/>  (Ordinal = 2) : Information that highlights progress or application lifetime events.<br/>
    /// - <see cref="LogLevel.Warn"/>  (Ordinal = 3) : Warnings about validation issues or temporary failures that can be recovered.<br/>
    /// - <see cref="LogLevel.Error"/> (Ordinal = 4) : Errors where functionality has failed or <see cref="System.Exception"/> have been caught.<br/>
    /// - <see cref="LogLevel.Fatal"/> (Ordinal = 5) : Most critical level. Application is about to abort.<br/>
    /// </remarks>
    [TypeConverter(typeof(Attributes.LogLevelTypeConverter))]
    public sealed class LogLevel : IComparable<LogLevel>, IComparable, IEquatable<LogLevel>, IFormattable
    {
        /// <summary>
        /// Trace log level (Ordinal = 0)
        /// </summary>
        /// <remarks>
        /// Most verbose level. Used for development and seldom enabled in production.
        /// </remarks>
        public static readonly LogLevel Trace = new LogLevel("Trace", 0);

        /// <summary>
        /// Debug log level (Ordinal = 1)
        /// </summary>
        /// <remarks>
        /// Debugging the application behavior from internal events of interest.
        /// </remarks>
        public static readonly LogLevel Debug = new LogLevel("Debug", 1);

        /// <summary>
        /// Info log level (Ordinal = 2)
        /// </summary>
        /// <remarks>
        /// Information that highlights progress or application lifetime events.
        /// </remarks>
        public static readonly LogLevel Info = new LogLevel("Info", 2);

        /// <summary>
        /// Warn log level (Ordinal = 3)
        /// </summary>
        /// <remarks>
        /// Warnings about validation issues or temporary failures that can be recovered.
        /// </remarks>
        public static readonly LogLevel Warn = new LogLevel("Warn", 3);

        /// <summary>
        /// Error log level (Ordinal = 4)
        /// </summary>
        /// <remarks>
        /// Errors where functionality has failed or <see cref="System.Exception"/> have been caught.
        /// </remarks>
        public static readonly LogLevel Error = new LogLevel("Error", 4);

        /// <summary>
        /// Fatal log level (Ordinal = 5)
        /// </summary>
        /// <remarks>
        /// Most critical level. Application is about to abort.
        /// </remarks>
        public static readonly LogLevel Fatal = new LogLevel("Fatal", 5);

        /// <summary>
        /// Off log level (Ordinal = 6)
        /// </summary>
        public static readonly LogLevel Off = new LogLevel("Off", 6);

        private static readonly IList<LogLevel> allLevels = new List<LogLevel> { Trace, Debug, Info, Warn, Error, Fatal, Off }.AsReadOnly();

        private static readonly IList<LogLevel> allLoggingLevels = new List<LogLevel> { Trace, Debug, Info, Warn, Error, Fatal }.AsReadOnly();

        /// <summary>
        /// Gets all the available log levels (Trace, Debug, Info, Warn, Error, Fatal, Off).
        /// </summary>
        public static IEnumerable<LogLevel> AllLevels => allLevels;

        /// <summary>
        ///  Gets all the log levels that can be used to log events (Trace, Debug, Info, Warn, Error, Fatal) 
        ///  i.e <c>LogLevel.Off</c> is excluded.
        /// </summary>
        public static IEnumerable<LogLevel> AllLoggingLevels => allLoggingLevels;

        internal static LogLevel MaxLevel => Fatal;

        internal static LogLevel MinLevel => Trace;

        private readonly int _ordinal;
        private readonly string _name;

        /// <summary>
        /// Initializes a new instance of <see cref="LogLevel"/>.
        /// </summary>
        /// <param name="name">The log level name.</param>
        /// <param name="ordinal">The log level ordinal number.</param>
        private LogLevel(string name, int ordinal)
        {
            _name = name;
            _ordinal = ordinal;
        }

        /// <summary>
        /// Gets the name of the log level.
        /// </summary>
        public string Name => _name;

        /// <summary>
        /// Gets the ordinal of the log level.
        /// </summary>
        public int Ordinal => _ordinal;

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
            if (ReferenceEquals(level1, level2))
                return true;
            else
                return (level1 ?? LogLevel.Off).Equals(level2);
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
            if (ReferenceEquals(level1, level2))
                return false;
            else
                return !(level1 ?? LogLevel.Off).Equals(level2);
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
            if (ReferenceEquals(level1, level2))
                return false;
            else
                return (level1 ?? LogLevel.Off).CompareTo(level2) > 0;
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
            if (ReferenceEquals(level1, level2))
                return true;
            else
                return (level1 ?? LogLevel.Off).CompareTo(level2) >= 0;
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
            if (ReferenceEquals(level1, level2))
                return false;
            else
                return (level1 ?? LogLevel.Off).CompareTo(level2) < 0;
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
            if (ReferenceEquals(level1, level2))
                return true;
            else
                return (level1 ?? LogLevel.Off).CompareTo(level2) <= 0;
        }

        /// <summary>
        /// Gets the <see cref="LogLevel"/> that corresponds to the specified ordinal.
        /// </summary>
        /// <param name="ordinal">The ordinal.</param>
        /// <returns>The <see cref="LogLevel"/> instance. For 0 it returns <see cref="LogLevel.Trace"/>, 1 gives <see cref="LogLevel.Debug"/> and so on.</returns>
        public static LogLevel FromOrdinal(int ordinal)
        {
            switch (ordinal)
            {
                case 0:
                    return Trace;
                case 1:
                    return Debug;
                case 2:
                    return Info;
                case 3:
                    return Warn;
                case 4:
                    return Error;
                case 5:
                    return Fatal;
                case 6:
                    return Off;

                default:
                    throw new ArgumentException($"Unknown loglevel: {ordinal.ToString()}.", nameof(ordinal));
            }
        }

        /// <summary>
        /// Returns the <see cref="NLog.LogLevel"/> that corresponds to the supplied <see langword="string" />.
        /// </summary>
        /// <param name="levelName">The textual representation of the log level.</param>
        /// <returns>The enumeration value.</returns>
        public static LogLevel FromString(string levelName)
        {
            Guard.ThrowIfNull(levelName);

            if (levelName.Equals("Trace", StringComparison.OrdinalIgnoreCase))
            {
                return Trace;
            }

            if (levelName.Equals("Debug", StringComparison.OrdinalIgnoreCase))
            {
                return Debug;
            }

            if (levelName.Equals("Info", StringComparison.OrdinalIgnoreCase))
            {
                return Info;
            }

            if (levelName.Equals("Warn", StringComparison.OrdinalIgnoreCase))
            {
                return Warn;
            }

            if (levelName.Equals("Error", StringComparison.OrdinalIgnoreCase))
            {
                return Error;
            }

            if (levelName.Equals("Fatal", StringComparison.OrdinalIgnoreCase))
            {
                return Fatal;
            }

            if (levelName.Equals("Off", StringComparison.OrdinalIgnoreCase))
            {
                return Off;
            }

            if (levelName.Equals("None", StringComparison.OrdinalIgnoreCase))
            {
                return Off;     // .NET Core Microsoft Extension Logging
            }

            if (levelName.Equals("Information", StringComparison.OrdinalIgnoreCase))
            {
                return Info;    // .NET Core Microsoft Extension Logging
            }

            if (levelName.Equals("Warning", StringComparison.OrdinalIgnoreCase))
            {
                return Warn;    // .NET Core Microsoft Extension Logging
            }

            throw new ArgumentException($"Unknown log level: {levelName}", nameof(levelName));
        }

        /// <summary>
        /// Returns a string representation of the log level.
        /// </summary>
        /// <returns>Log level name.</returns>
        public override string ToString()
        {
            return _name;
        }

        string IFormattable.ToString(string format, IFormatProvider formatProvider)
        {
            if (format is null || (!"D".Equals(format, StringComparison.OrdinalIgnoreCase)))
                return _name;
            else
                return _ordinal.ToString(); // Like Enum.ToString("D")
        }

        /// <inheritdoc/>
        public override int GetHashCode()
        {
            return _ordinal;
        }

        /// <inheritdoc/>
        public override bool Equals(object obj)
        {
            return Equals(obj as LogLevel);
        }

        /// <summary>
        /// Determines whether the specified <see cref="NLog.LogLevel"/> instance is equal to this instance.
        /// </summary>
        /// <param name="other">The <see cref="NLog.LogLevel"/> to compare with this instance.</param>
        /// <returns>Value of <c>true</c> if the specified <see cref="NLog.LogLevel"/> is equal to 
        /// this instance; otherwise, <c>false</c>.</returns>
        public bool Equals(LogLevel other)
        {
            return _ordinal == other?._ordinal;
        }

        /// <summary>
        /// Compares the level to the other <see cref="LogLevel"/> object.
        /// </summary>
        /// <param name="obj">The other object.</param>
        /// <returns>
        /// A value less than zero when this logger's <see cref="Ordinal"/> is 
        /// less than the other logger's ordinal, 0 when they are equal and 
        /// greater than zero when this ordinal is greater than the
        /// other ordinal.
        /// </returns>
        public int CompareTo(object obj)
        {
            return CompareTo((LogLevel)obj);
        }

        /// <summary>
        /// Compares the level to the other <see cref="LogLevel"/> object.
        /// </summary>
        /// <param name="other">The other object.</param>
        /// <returns>
        /// A value less than zero when this logger's <see cref="Ordinal"/> is 
        /// less than the other logger's ordinal, 0 when they are equal and 
        /// greater than zero when this ordinal is greater than the
        /// other ordinal.
        /// </returns>
        public int CompareTo(LogLevel other)
        {
            return _ordinal - (other ?? LogLevel.Off)._ordinal;
        }
    }
}
