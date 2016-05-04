// 
// Copyright (c) 2004-2016 Jaroslaw Kowalski <jaak@jkowalski.net>, Kim Christensen, Julian Verdurmen
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

#if !SILVERLIGHT

namespace NLog
{
    using System;
    using System.ComponentModel;
    using JetBrains.Annotations;

    /// <content>
    /// Auto-generated Logger members for binary compatibility with NLog 1.0.
    /// </content>
    [CLSCompliant(false)]
    public partial interface ILoggerBase
    {
        #region Log() overloads

        /// <summary>
        /// Writes the diagnostic message at the specified level.
        /// </summary>
        /// <param name="level">The log level.</param>
        /// <param name="value">A <see langword="object" /> to be written.</param>
        [EditorBrowsable(EditorBrowsableState.Never)]
        void Log(LogLevel level, object value);

        /// <summary>
        /// Writes the diagnostic message at the specified level.
        /// </summary>
        /// <param name="level">The log level.</param>
        /// <param name="formatProvider">An IFormatProvider that supplies culture-specific formatting information.</param>
        /// <param name="value">A <see langword="object" /> to be written.</param>
        [EditorBrowsable(EditorBrowsableState.Never)]
        void Log(LogLevel level, IFormatProvider formatProvider, object value);

        /// <summary>
        /// Writes the diagnostic message at the specified level using the specified parameters.
        /// </summary>
        /// <param name="level">The log level.</param>
        /// <param name="message">A <see langword="string" /> containing format items.</param>
        /// <param name="arg1">First argument to format.</param>
        /// <param name="arg2">Second argument to format.</param>
        [EditorBrowsable(EditorBrowsableState.Never)]
        [StringFormatMethod("message")]
        void Log(LogLevel level, string message, object arg1, object arg2);

        /// <summary>
        /// Writes the diagnostic message at the specified level using the specified parameters.
        /// </summary>
        /// <param name="level">The log level.</param>
        /// <param name="message">A <see langword="string" /> containing format items.</param>
        /// <param name="arg1">First argument to format.</param>
        /// <param name="arg2">Second argument to format.</param>
        /// <param name="arg3">Third argument to format.</param>
        [EditorBrowsable(EditorBrowsableState.Never)]
        [StringFormatMethod("message")]
        void Log(LogLevel level, string message, object arg1, object arg2, object arg3);

        /// <summary>
        /// Writes the diagnostic message at the specified level using the specified value as a parameter and formatting it with the supplied format provider.
        /// </summary>
        /// <param name="level">The log level.</param>
        /// <param name="formatProvider">An IFormatProvider that supplies culture-specific formatting information.</param>
        /// <param name="message">A <see langword="string" /> containing one format item.</param>
        /// <param name="argument">The argument to format.</param>
        [EditorBrowsable(EditorBrowsableState.Never)]
        [StringFormatMethod("message")]
        void Log(LogLevel level, IFormatProvider formatProvider, string message, bool argument);

        /// <summary>
        /// Writes the diagnostic message at the specified level using the specified value as a parameter.
        /// </summary>
        /// <param name="level">The log level.</param>
        /// <param name="message">A <see langword="string" /> containing one format item.</param>
        /// <param name="argument">The argument to format.</param>
        [EditorBrowsable(EditorBrowsableState.Never)]
        [StringFormatMethod("message")]
        void Log(LogLevel level, string message, bool argument);

        /// <summary>
        /// Writes the diagnostic message at the specified level using the specified value as a parameter and formatting it with the supplied format provider.
        /// </summary>
        /// <param name="level">The log level.</param>
        /// <param name="formatProvider">An IFormatProvider that supplies culture-specific formatting information.</param>
        /// <param name="message">A <see langword="string" /> containing one format item.</param>
        /// <param name="argument">The argument to format.</param>
        [EditorBrowsable(EditorBrowsableState.Never)]
        [StringFormatMethod("message")]
        void Log(LogLevel level, IFormatProvider formatProvider, string message, char argument);

        /// <summary>
        /// Writes the diagnostic message at the specified level using the specified value as a parameter.
        /// </summary>
        /// <param name="level">The log level.</param>
        /// <param name="message">A <see langword="string" /> containing one format item.</param>
        /// <param name="argument">The argument to format.</param>
        [EditorBrowsable(EditorBrowsableState.Never)]
        [StringFormatMethod("message")]
        void Log(LogLevel level, string message, char argument);

        /// <summary>
        /// Writes the diagnostic message at the specified level using the specified value as a parameter and formatting it with the supplied format provider.
        /// </summary>
        /// <param name="level">The log level.</param>
        /// <param name="formatProvider">An IFormatProvider that supplies culture-specific formatting information.</param>
        /// <param name="message">A <see langword="string" /> containing one format item.</param>
        /// <param name="argument">The argument to format.</param>
        [EditorBrowsable(EditorBrowsableState.Never)]
        [StringFormatMethod("message")]
        void Log(LogLevel level, IFormatProvider formatProvider, string message, byte argument);

        /// <summary>
        /// Writes the diagnostic message at the specified level using the specified value as a parameter.
        /// </summary>
        /// <param name="level">The log level.</param>
        /// <param name="message">A <see langword="string" /> containing one format item.</param>
        /// <param name="argument">The argument to format.</param>
        [EditorBrowsable(EditorBrowsableState.Never)]
        [StringFormatMethod("message")]
        void Log(LogLevel level, string message, byte argument);

        /// <summary>
        /// Writes the diagnostic message at the specified level using the specified value as a parameter and formatting it with the supplied format provider.
        /// </summary>
        /// <param name="level">The log level.</param>
        /// <param name="formatProvider">An IFormatProvider that supplies culture-specific formatting information.</param>
        /// <param name="message">A <see langword="string" /> containing one format item.</param>
        /// <param name="argument">The argument to format.</param>
        [EditorBrowsable(EditorBrowsableState.Never)]
        [StringFormatMethod("message")]
        void Log(LogLevel level, IFormatProvider formatProvider, string message, string argument);

        /// <summary>
        /// Writes the diagnostic message at the specified level using the specified value as a parameter.
        /// </summary>
        /// <param name="level">The log level.</param>
        /// <param name="message">A <see langword="string" /> containing one format item.</param>
        /// <param name="argument">The argument to format.</param>
        [EditorBrowsable(EditorBrowsableState.Never)]
        [StringFormatMethod("message")]
        void Log(LogLevel level, string message, string argument);

        /// <summary>
        /// Writes the diagnostic message at the specified level using the specified value as a parameter and formatting it with the supplied format provider.
        /// </summary>
        /// <param name="level">The log level.</param>
        /// <param name="formatProvider">An IFormatProvider that supplies culture-specific formatting information.</param>
        /// <param name="message">A <see langword="string" /> containing one format item.</param>
        /// <param name="argument">The argument to format.</param>
        [EditorBrowsable(EditorBrowsableState.Never)]
        [StringFormatMethod("message")]
        void Log(LogLevel level, IFormatProvider formatProvider, string message, int argument);

        /// <summary>
        /// Writes the diagnostic message at the specified level using the specified value as a parameter.
        /// </summary>
        /// <param name="level">The log level.</param>
        /// <param name="message">A <see langword="string" /> containing one format item.</param>
        /// <param name="argument">The argument to format.</param>
        [EditorBrowsable(EditorBrowsableState.Never)]
        [StringFormatMethod("message")]
        void Log(LogLevel level, string message, int argument);

        /// <summary>
        /// Writes the diagnostic message at the specified level using the specified value as a parameter and formatting it with the supplied format provider.
        /// </summary>
        /// <param name="level">The log level.</param>
        /// <param name="formatProvider">An IFormatProvider that supplies culture-specific formatting information.</param>
        /// <param name="message">A <see langword="string" /> containing one format item.</param>
        /// <param name="argument">The argument to format.</param>
        [EditorBrowsable(EditorBrowsableState.Never)]
        [StringFormatMethod("message")]
        void Log(LogLevel level, IFormatProvider formatProvider, string message, long argument);

        /// <summary>
        /// Writes the diagnostic message at the specified level using the specified value as a parameter.
        /// </summary>
        /// <param name="level">The log level.</param>
        /// <param name="message">A <see langword="string" /> containing one format item.</param>
        /// <param name="argument">The argument to format.</param>
        [EditorBrowsable(EditorBrowsableState.Never)]
        [StringFormatMethod("message")]
        void Log(LogLevel level, string message, long argument);

        /// <summary>
        /// Writes the diagnostic message at the specified level using the specified value as a parameter and formatting it with the supplied format provider.
        /// </summary>
        /// <param name="level">The log level.</param>
        /// <param name="formatProvider">An IFormatProvider that supplies culture-specific formatting information.</param>
        /// <param name="message">A <see langword="string" /> containing one format item.</param>
        /// <param name="argument">The argument to format.</param>
        [EditorBrowsable(EditorBrowsableState.Never)]
        [StringFormatMethod("message")]
        void Log(LogLevel level, IFormatProvider formatProvider, string message, float argument);

        /// <summary>
        /// Writes the diagnostic message at the specified level using the specified value as a parameter.
        /// </summary>
        /// <param name="level">The log level.</param>
        /// <param name="message">A <see langword="string" /> containing one format item.</param>
        /// <param name="argument">The argument to format.</param>
        [EditorBrowsable(EditorBrowsableState.Never)]
        [StringFormatMethod("message")]
        void Log(LogLevel level, string message, float argument);

        /// <summary>
        /// Writes the diagnostic message at the specified level using the specified value as a parameter and formatting it with the supplied format provider.
        /// </summary>
        /// <param name="level">The log level.</param>
        /// <param name="formatProvider">An IFormatProvider that supplies culture-specific formatting information.</param>
        /// <param name="message">A <see langword="string" /> containing one format item.</param>
        /// <param name="argument">The argument to format.</param>
        [EditorBrowsable(EditorBrowsableState.Never)]
        [StringFormatMethod("message")]
        void Log(LogLevel level, IFormatProvider formatProvider, string message, double argument);

        /// <summary>
        /// Writes the diagnostic message at the specified level using the specified value as a parameter.
        /// </summary>
        /// <param name="level">The log level.</param>
        /// <param name="message">A <see langword="string" /> containing one format item.</param>
        /// <param name="argument">The argument to format.</param>
        [EditorBrowsable(EditorBrowsableState.Never)]
        [StringFormatMethod("message")]
        void Log(LogLevel level, string message, double argument);

        /// <summary>
        /// Writes the diagnostic message at the specified level using the specified value as a parameter and formatting it with the supplied format provider.
        /// </summary>
        /// <param name="level">The log level.</param>
        /// <param name="formatProvider">An IFormatProvider that supplies culture-specific formatting information.</param>
        /// <param name="message">A <see langword="string" /> containing one format item.</param>
        /// <param name="argument">The argument to format.</param>
        [EditorBrowsable(EditorBrowsableState.Never)]
        [StringFormatMethod("message")]
        void Log(LogLevel level, IFormatProvider formatProvider, string message, decimal argument);

        /// <summary>
        /// Writes the diagnostic message at the specified level using the specified value as a parameter.
        /// </summary>
        /// <param name="level">The log level.</param>
        /// <param name="message">A <see langword="string" /> containing one format item.</param>
        /// <param name="argument">The argument to format.</param>
        [EditorBrowsable(EditorBrowsableState.Never)]
        [StringFormatMethod("message")]
        void Log(LogLevel level, string message, decimal argument);

        /// <summary>
        /// Writes the diagnostic message at the specified level using the specified value as a parameter and formatting it with the supplied format provider.
        /// </summary>
        /// <param name="level">The log level.</param>
        /// <param name="formatProvider">An IFormatProvider that supplies culture-specific formatting information.</param>
        /// <param name="message">A <see langword="string" /> containing one format item.</param>
        /// <param name="argument">The argument to format.</param>
        [EditorBrowsable(EditorBrowsableState.Never)]
        [StringFormatMethod("message")]
        void Log(LogLevel level, IFormatProvider formatProvider, string message, object argument);

        /// <summary>
        /// Writes the diagnostic message at the specified level using the specified value as a parameter.
        /// </summary>
        /// <param name="level">The log level.</param>
        /// <param name="message">A <see langword="string" /> containing one format item.</param>
        /// <param name="argument">The argument to format.</param>
        [EditorBrowsable(EditorBrowsableState.Never)]
        [StringFormatMethod("message")]
        void Log(LogLevel level, string message, object argument);

        /// <summary>
        /// Writes the diagnostic message at the specified level using the specified value as a parameter and formatting it with the supplied format provider.
        /// </summary>
        /// <param name="level">The log level.</param>
        /// <param name="formatProvider">An IFormatProvider that supplies culture-specific formatting information.</param>
        /// <param name="message">A <see langword="string" /> containing one format item.</param>
        /// <param name="argument">The argument to format.</param>
        [EditorBrowsable(EditorBrowsableState.Never)]
        [StringFormatMethod("message")]
        void Log(LogLevel level, IFormatProvider formatProvider, string message, sbyte argument);

        /// <summary>
        /// Writes the diagnostic message at the specified level using the specified value as a parameter.
        /// </summary>
        /// <param name="level">The log level.</param>
        /// <param name="message">A <see langword="string" /> containing one format item.</param>
        /// <param name="argument">The argument to format.</param>    
        [EditorBrowsable(EditorBrowsableState.Never)]
        [StringFormatMethod("message")]
        void Log(LogLevel level, string message, sbyte argument);

        /// <summary>
        /// Writes the diagnostic message at the specified level using the specified value as a parameter and formatting it with the supplied format provider.
        /// </summary>
        /// <param name="level">The log level.</param>
        /// <param name="formatProvider">An IFormatProvider that supplies culture-specific formatting information.</param>
        /// <param name="message">A <see langword="string" /> containing one format item.</param>
        /// <param name="argument">The argument to format.</param>        
        [EditorBrowsable(EditorBrowsableState.Never)]
        [StringFormatMethod("message")]
        void Log(LogLevel level, IFormatProvider formatProvider, string message, uint argument);

        /// <summary>
        /// Writes the diagnostic message at the specified level using the specified value as a parameter.
        /// </summary>
        /// <param name="level">The log level.</param>
        /// <param name="message">A <see langword="string" /> containing one format item.</param>
        /// <param name="argument">The argument to format.</param>

        [EditorBrowsable(EditorBrowsableState.Never)]
        [StringFormatMethod("message")]
        void Log(LogLevel level, string message, uint argument);

        /// <summary>
        /// Writes the diagnostic message at the specified level using the specified value as a parameter and formatting it with the supplied format provider.
        /// </summary>
        /// <param name="level">The log level.</param>
        /// <param name="formatProvider">An IFormatProvider that supplies culture-specific formatting information.</param>
        /// <param name="message">A <see langword="string" /> containing one format item.</param>
        /// <param name="argument">The argument to format.</param>

        [EditorBrowsable(EditorBrowsableState.Never)]
        [StringFormatMethod("message")]
        void Log(LogLevel level, IFormatProvider formatProvider, string message, ulong argument);

        /// <summary>
        /// Writes the diagnostic message at the specified level using the specified value as a parameter.
        /// </summary>
        /// <param name="level">The log level.</param>
        /// <param name="message">A <see langword="string" /> containing one format item.</param>
        /// <param name="argument">The argument to format.</param>

        [EditorBrowsable(EditorBrowsableState.Never)]
        [StringFormatMethod("message")]
        void Log(LogLevel level, string message, ulong argument);

        #endregion
    }
}

#endif
