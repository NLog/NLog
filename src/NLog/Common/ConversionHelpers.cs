//
// Copyright (c) 2004-2024 Jaroslaw Kowalski <jaak@jkowalski.net>, Kim Christensen, Julian Verdurmen
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

namespace NLog.Common
{
    using System;
    using System.ComponentModel;
    using NLog.Internal;

    /// <summary>
    /// String Conversion Helpers
    /// </summary>
    public static class ConversionHelpers
    {
        /// <summary>
        /// Converts input string value into <see cref="System.Enum"/>. Parsing is case-insensitive.
        /// </summary>
        /// <param name="inputValue">Input value</param>
        /// <param name="resultValue">Output value</param>
        /// <param name="defaultValue">Default value</param>
        /// <returns>Returns <see langword="false"/> if the input value could not be parsed</returns>
        [Obsolete("Instead use .NET method Enum.TryParse<TEnum>(). Marked obsolete with NLog v6.1")]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public static bool TryParseEnum<TEnum>(string inputValue, out TEnum resultValue, TEnum defaultValue = default(TEnum)) where TEnum : struct
        {
            if (!TryParseEnum(inputValue, true, out resultValue))
            {
                resultValue = defaultValue;
                return false;
            }
            return true;
        }

        /// <summary>Try parse string as enum value (case-insensitive). Returns default enum value for null/whitespace input.</summary>
        /// <param name="inputValue">String to parse.</param>
        /// <param name="enumType">Target enum type.</param>
        /// <param name="resultValue">Parsed enum value, or <see langword="null"/> on failure.</param>
        /// <returns><see langword="true"/> if parsed successfully or input was null/whitespace; otherwise <see langword="false"/>.</returns>
        internal static bool TryParseEnum(string inputValue, Type enumType, out object? resultValue)
        {
            if (StringHelpers.IsNullOrWhiteSpace(inputValue))
            {
                if (enumType.IsEnum)
                {
                    resultValue = Enum.ToObject(enumType, 0);
                    return true;
                }
                resultValue = null;
                return false;
            }

#if NETSTANDARD2_1_OR_GREATER || NET
            return Enum.TryParse(enumType, inputValue, true, out resultValue);
#else
            if (!enumType.IsEnum)
                throw new ArgumentException($"Type '{enumType.FullName}' is not an enum");

            try
            {
                resultValue = Enum.Parse(enumType, inputValue, true);
                return true;
            }
            catch (ArgumentException)
            {
                resultValue = null;
                return false;
            }
#endif
        }

        /// <summary>Try parse string as enum value. Returns default enum value for null/whitespace input.</summary>
        /// <typeparam name="TEnum">Target enum type.</typeparam>
        /// <param name="inputValue">String to parse.</param>
        /// <param name="ignoreCase"><see langword="true"/> to ignore case; <see langword="false"/> to consider case.</param>
        /// <param name="resultValue">Parsed enum value, or default on failure.</param>
        /// <returns><see langword="true"/> if parsed successfully or input was null/whitespace; otherwise <see langword="false"/>.</returns>
        /// <remarks>Wrapper because Enum.TryParse is not present in .NET 3.5</remarks>
        internal static bool TryParseEnum<TEnum>(string inputValue, bool ignoreCase, out TEnum resultValue) where TEnum : struct
        {
            if (StringHelpers.IsNullOrWhiteSpace(inputValue))
            {
                resultValue = default(TEnum);
                return true;
            }

#if !NET35
            return Enum.TryParse<TEnum>(inputValue, ignoreCase, out resultValue);
#else
            var enumType = typeof(TEnum);
            if (!enumType.IsEnum)
                throw new ArgumentException($"Type '{enumType.FullName}' is not an enum");

            try
            {
                resultValue = (TEnum)Enum.Parse(enumType, inputValue, ignoreCase);
                return true;
            }
            catch (Exception)
            {
                resultValue = default(TEnum);
                return false;
            }
#endif
        }
    }
}
