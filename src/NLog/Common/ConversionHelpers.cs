// 
// Copyright (c) 2004-2020 Jaroslaw Kowalski <jaak@jkowalski.net>, Kim Christensen, Julian Verdurmen
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
        /// <returns>Returns false if the input value could not be parsed</returns>
        public static bool TryParseEnum<TEnum>(string inputValue, out TEnum resultValue, TEnum defaultValue = default(TEnum)) where TEnum : struct
        {
            if (!TryParseEnum(inputValue, true, out resultValue))
            {
                resultValue = defaultValue;
                return false;
            }
            return true;
        }

        /// <summary>
        /// Converts input string value into <see cref="System.Enum"/>. Parsing is case-insensitive.
        /// </summary>
        /// <param name="inputValue">Input value</param>
        /// <param name="enumType">The type of the enum</param>
        /// <param name="resultValue">Output value. Null if parse failed</param>
        internal static bool TryParseEnum(string inputValue, Type enumType, out object resultValue)
        {
            if (StringHelpers.IsNullOrWhiteSpace(inputValue))
            {
                resultValue = null;
                return false;
            }
            // Note: .NET Standard 2.1 added a public Enum.TryParse(Type)
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
        }

        /// <summary>
        /// Converts the string representation of the name or numeric value of one or more enumerated constants to an equivalent enumerated object. A parameter specifies whether the operation is case-sensitive. The return value indicates whether the conversion succeeded.
        /// </summary>
        /// <typeparam name="TEnum">The enumeration type to which to convert value.</typeparam>
        /// <param name="inputValue">The string representation of the enumeration name or underlying value to convert.</param>
        /// <param name="ignoreCase"><c>true</c> to ignore case; <c>false</c> to consider case.</param>
        /// <param name="resultValue">When this method returns, result contains an object of type TEnum whose value is represented by value if the parse operation succeeds. If the parse operation fails, result contains the default value of the underlying type of TEnum. Note that this value need not be a member of the TEnum enumeration. This parameter is passed uninitialized.</param>
        /// <returns><c>true</c> if the value parameter was converted successfully; otherwise, <c>false</c>.</returns>
        /// <remarks>Wrapper because Enum.TryParse is not present in .net 3.5</remarks>
        internal static bool TryParseEnum<TEnum>(string inputValue, bool ignoreCase, out TEnum resultValue) where TEnum : struct
        {
            if (StringHelpers.IsNullOrWhiteSpace(inputValue))
            {
                resultValue = default(TEnum);
                return false;
            }

#if !NET35
            return Enum.TryParse(inputValue, ignoreCase, out resultValue);
#else
            return TryParseEnum_net3(inputValue, ignoreCase, out resultValue);            
#endif
        }

        /// <summary>
        /// Enum.TryParse implementation for .net 3.5 
        /// 
        /// </summary>
        /// <returns></returns>
        /// <remarks>Don't uses reflection</remarks>
        // ReSharper disable once UnusedMember.Local
        private static bool TryParseEnum_net3<TEnum>(string value, bool ignoreCase, out TEnum result) where TEnum : struct
        {
            var enumType = typeof(TEnum);
            if (!enumType.IsEnum())
                throw new ArgumentException($"Type '{enumType.FullName}' is not an enum");

            if (StringHelpers.IsNullOrWhiteSpace(value))
            {
                result = default(TEnum);
                return false;
            }

            try
            {
                result = (TEnum)Enum.Parse(enumType, value, ignoreCase);
                return true;
            }
            catch (Exception)
            {
                result = default(TEnum);
                return false;
            }
        }
    }
}
