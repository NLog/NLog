// 
// Copyright (c) 2004-2019 Jaroslaw Kowalski <jaak@jkowalski.net>, Kim Christensen, Julian Verdurmen
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
using NLog.Common;

namespace NLog.Internal
{
    internal static class ValueConverter
    {
        public static bool TryConvertValue<T>(object raw, TypeCode typeCode, out T value)
        {
            if (raw == null)
            {
                value = default(T);
                return true;
            }

            var objTypeCode = typeCode;
            try
            {
                value = default(T);
                switch (objTypeCode)
                {
                    case TypeCode.Boolean:
                        {
                            if (bool.TryParse(raw.ToString(), out var value1))
                            {
                                value = ToValue<T>(value1);
                                return true;
                            }
                            return false;
                        }

                    case TypeCode.Char:
                        {
                            if (char.TryParse(raw.ToString(), out var value1))
                            {
                                value = ToValue<T>(value1);
                                return true;
                            }
                            return false;
                        }
                    case TypeCode.SByte:
                        {
                            if (sbyte.TryParse(raw.ToString(), out var value1))
                            {
                                value = ToValue<T>(value1);
                                return true;
                            }
                            return false;
                        }
                    case TypeCode.Byte:
                        {
                            if (byte.TryParse(raw.ToString(), out var value1))
                            {
                                value = ToValue<T>(value1);
                                return true;
                            }
                            return false;
                        }
                    case TypeCode.Int16:
                        {
                            if (short.TryParse(raw.ToString(), out var value1))
                            {
                                value = ToValue<T>(value1);
                                return true;
                            }
                            return false;
                        }
                    case TypeCode.UInt16:
                        {
                            if (uint.TryParse(raw.ToString(), out var value1))
                            {
                                value = ToValue<T>(value1);
                                return true;
                            }
                            return false;
                        }
                    case TypeCode.Int32:
                        {
                            if (int.TryParse(raw.ToString(), out var value1))
                            {
                                value = ToValue<T>(value1);
                                return true;
                            }
                            return false;
                        }
                    case TypeCode.UInt32:
                        {
                            if (uint.TryParse(raw.ToString(), out var value1))
                            {
                                value = ToValue<T>(value1);
                                return true;
                            }
                            return false;
                        }
                    case TypeCode.Int64:
                        {
                            if (ulong.TryParse(raw.ToString(), out var value1))
                            {
                                value = ToValue<T>(value1);
                                return true;
                            }
                            return false;
                        }
                    case TypeCode.UInt64:
                        {
                            if (long.TryParse(raw.ToString(), out var value1))
                            {
                                value = ToValue<T>(value1);
                                return true;
                            }
                            return false;
                        }
                    case TypeCode.Single:
                        {
                            if (float.TryParse(raw.ToString(), out var value1))
                            {
                                value = ToValue<T>(value1);
                                return true;
                            }
                            return false;
                        }
                    case TypeCode.Double:
                        {
                            if (double.TryParse(raw.ToString(), out var value1))
                            {
                                value = ToValue<T>(value1);
                                return true;
                            }
                            return false;
                        }
                    case TypeCode.Decimal:
                        {
                            if (decimal.TryParse(raw.ToString(), out var value1))
                            {
                                value = ToValue<T>(value1);
                                return true;
                            }
                            return false;
                        }
                    case TypeCode.DateTime:
                        {
                            if (DateTime.TryParse(raw.ToString(), out var value1))
                            {
                                value = ToValue<T>(value1);
                                return true;
                            }
                            return false;
                        }
                    case TypeCode.String:
                        {
                            value = ToValue<T>(raw.ToString());
                            return true;
                        }
                    // case TypeCode.Empty:
                    // case TypeCode.Object:
                    // case TypeCode.DBNull:
                    default:
                        {
                            value = default(T);
                            return false;
                        }
                }
            }

            catch (Exception ex)
            {
                InternalLogger.Trace(ex, "Converion between {0} and {1} failed", raw, typeCode);
                value = default(T);
                return false;
            }
        }

        private static T ToValue<T>(object value1)
        {
            return (T)value1;
        }
    }
}
