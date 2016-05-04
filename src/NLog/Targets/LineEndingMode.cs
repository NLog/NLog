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

using System;
using System.ComponentModel;
using System.Globalization;
using JetBrains.Annotations;
using NLog.Internal;

namespace NLog.Targets
{
    /// <summary>
    /// Line ending mode.
    /// </summary>
    [TypeConverter(typeof(LineEndingModeConverter))]
    public sealed class LineEndingMode 
    {
        /// <summary>
        /// Insert platform-dependent end-of-line sequence after each line.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security", "CA2104:DoNotDeclareReadOnlyMutableReferenceTypes", Justification = "Type is immutable")]
        public static readonly LineEndingMode Default = new LineEndingMode("Default", EnvironmentHelper.NewLine);

        /// <summary>
        /// Insert CR LF sequence (ASCII 13, ASCII 10) after each line.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security", "CA2104:DoNotDeclareReadOnlyMutableReferenceTypes", Justification = "Type is immutable")]
        public static readonly LineEndingMode CRLF = new LineEndingMode("CRLF", "\r\n");

        /// <summary>
        /// Insert CR character (ASCII 13) after each line.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security", "CA2104:DoNotDeclareReadOnlyMutableReferenceTypes", Justification = "Type is immutable")]
        public static readonly LineEndingMode CR = new LineEndingMode("CR", "\r");

        /// <summary>
        /// Insert LF character (ASCII 10) after each line.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security", "CA2104:DoNotDeclareReadOnlyMutableReferenceTypes", Justification = "Type is immutable")]
        public static readonly LineEndingMode LF = new LineEndingMode("LF", "\n");

        /// <summary>
        /// Do not insert any line ending.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security", "CA2104:DoNotDeclareReadOnlyMutableReferenceTypes", Justification = "Type is immutable")]
        public static readonly LineEndingMode None = new LineEndingMode("None", String.Empty);


        private readonly string name;
        private readonly string newLineCharacters;

        /// <summary>
        /// Gets the name of the LineEndingMode instance.
        /// </summary>
        public string Name 
        {       
            get { return this.name; }
        }

        /// <summary>
        /// Gets the new line characters (value) of the LineEndingMode instance.  
        /// </summary>
        public string NewLineCharacters 
        {
            get { return this.newLineCharacters; }
        }

        private LineEndingMode() { }
        
        /// <summary>
        /// Initializes a new instance of <see cref="LogLevel"/>.
        /// </summary>
        /// <param name="name">The mode name.</param>
        /// <param name="newLineCharacters">The new line characters to be used.</param>
        private LineEndingMode(string name, string newLineCharacters)
        {
            this.name = name;
            this.newLineCharacters = newLineCharacters;
        }


        /// <summary>
        ///  Returns the <see cref="LineEndingMode"/> that corresponds to the supplied <paramref name="name"/>.
        /// </summary>
        /// <param name="name">
        ///  The textual representation of the line ending mode, such as CRLF, LF, Default etc.
        ///  Name is not case sensitive.
        /// </param>
        /// <returns>The <see cref="LineEndingMode"/> value, that corresponds to the <paramref name="name"/>.</returns>
        /// <exception cref="ArgumentOutOfRangeException">There is no line ending mode with the specified name.</exception>
        public static LineEndingMode FromString([NotNull] string name)
        {
            if (name == null) throw new ArgumentNullException("name");

            if (name.Equals(CRLF.Name, StringComparison.OrdinalIgnoreCase)) return CRLF;
            if (name.Equals(LF.Name, StringComparison.OrdinalIgnoreCase)) return LF;
            if (name.Equals(CR.Name, StringComparison.OrdinalIgnoreCase)) return CR;
            if (name.Equals(Default.Name, StringComparison.OrdinalIgnoreCase)) return Default;
            if (name.Equals(None.Name, StringComparison.OrdinalIgnoreCase)) return None;

#if !SILVERLIGHT
            throw new ArgumentOutOfRangeException("name", name, "LineEndingMode is out of range");
#else
            throw new ArgumentOutOfRangeException("name", "LineEndingMode is out of range");
#endif
        }

        /// <summary>
        /// Compares two <see cref="LineEndingMode"/> objects and returns a 
        /// value indicating whether the first one is equal to the second one.
        /// </summary>
        /// <param name="mode1">The first level.</param>
        /// <param name="mode2">The second level.</param>
        /// <returns>The value of <c>mode1.NewLineCharacters == mode2.NewLineCharacters</c>.</returns>
        public static bool operator ==(LineEndingMode mode1, LineEndingMode mode2)
        {
            if (ReferenceEquals(mode1, null))
            {
                return ReferenceEquals(mode2, null);
            }

            if (ReferenceEquals(mode2, null))
            {
                return false;
            }

            return mode1.NewLineCharacters == mode2.NewLineCharacters;
        }

        /// <summary>
        /// Compares two <see cref="LineEndingMode"/> objects and returns a 
        /// value indicating whether the first one is not equal to the second one.
        /// </summary>
        /// <param name="mode1">The first mode</param>
        /// <param name="mode2">The second mode</param>
        /// <returns>The value of <c>mode1.NewLineCharacters != mode2.NewLineCharacters</c>.</returns>
        public static bool operator !=(LineEndingMode mode1, LineEndingMode mode2)
        {
            if (ReferenceEquals(mode1, null))
            {
                return !ReferenceEquals(mode2, null);
            }

            if (ReferenceEquals(mode2, null))
            {
                return true;
            }

            return mode1.NewLineCharacters != mode2.NewLineCharacters;
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
        /// A hash code for this instance, suitable for use in hashing algorithms 
        /// and data structures like a hash table. 
        /// </returns>
        public override int GetHashCode()
        {
            return this.NewLineCharacters.GetHashCode();
        }

        /// <summary>
        /// Determines whether the specified <see cref="System.Object"/> is 
        /// equal to this instance.
        /// </summary>
        /// <param name="obj">The <see cref="System.Object"/> to compare with 
        /// this instance.</param>
        /// <returns>
        /// Value of <c>true</c> if the specified <see cref="System.Object"/> 
        /// is equal to this instance; otherwise, <c>false</c>.
        /// </returns>
        /// <exception cref="T:System.NullReferenceException">
        /// The <paramref name="obj"/> parameter is null.
        /// </exception>
        public override bool Equals(object obj)
        {
            LineEndingMode other = obj as LineEndingMode;
            if ((object)other == null)
            {
                return false;
            }

            return this.NewLineCharacters == other.NewLineCharacters;
        }



        /// <summary>
        /// Provides a type converter to convert <see cref="LineEndingMode"/> objects to and from other representations.
        /// </summary>
        public class LineEndingModeConverter : TypeConverter
        {
            /// <summary>
            /// Returns whether this converter can convert an object of the given type to the type of this converter, using the specified context.
            /// </summary>
            /// <returns>
            /// true if this converter can perform the conversion; otherwise, false.
            /// </returns>
            /// <param name="context">An <see cref="T:System.ComponentModel.ITypeDescriptorContext"/> that provides a format context. </param><param name="sourceType">A <see cref="T:System.Type"/> that represents the type you want to convert from. </param>
            public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
            {
                return sourceType == typeof(string) || base.CanConvertFrom(context, sourceType);
            }

            /// <summary>
            /// Converts the given object to the type of this converter, using the specified context and culture information.
            /// </summary>
            /// <returns>
            /// An <see cref="T:System.Object"/> that represents the converted value.
            /// </returns>
            /// <param name="context">An <see cref="T:System.ComponentModel.ITypeDescriptorContext"/> that provides a format context. </param><param name="culture">The <see cref="T:System.Globalization.CultureInfo"/> to use as the current culture. </param><param name="value">The <see cref="T:System.Object"/> to convert. </param><exception cref="T:System.NotSupportedException">The conversion cannot be performed. </exception>
            public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
            {
                var name = value as string;
                return name != null ? LineEndingMode.FromString(name) : base.ConvertFrom(context, culture, value);
            }
        }
    }
}
