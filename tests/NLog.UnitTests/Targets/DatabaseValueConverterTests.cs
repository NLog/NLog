// 
// Copyright (c) 2004-2018 Jaroslaw Kowalski <jaak@jkowalski.net>, Kim Christensen, Julian Verdurmen
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
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Threading;
using NLog.Targets;
using Xunit;

namespace NLog.UnitTests.Targets
{
    public class DatabaseValueConverterTests
    {
        [Theory]
        [MemberData(nameof(ConvertFromStringTestCases))]
        public void ConvertFromString(string value, DbType dbType, object expected, string format = null)
        {

            var culture = Thread.CurrentThread.CurrentCulture;

            try
            {
                Thread.CurrentThread.CurrentCulture = new CultureInfo("NL-nl");

                // Arrange
                var databaseValueConverter = new DatabaseValueConverter();
                var databaseParameterInfo = new DatabaseParameterInfo("@test", "test layout")
                {
                    Format = format
                };

                // Act
                var result = databaseValueConverter.ConvertFromString(value, dbType, databaseParameterInfo);

                // Assert
                Assert.Equal(expected, result);
            }
            finally
            {
                // Restore
                Thread.CurrentThread.CurrentCulture = culture;
            }
        }

        public static IEnumerable<object[]> ConvertFromStringTestCases()
        {
            yield return new object[] { "true", DbType.Boolean, true };
            yield return new object[] { "True", DbType.Boolean, true };
            yield return new object[] { "1,2", DbType.VarNumeric, (decimal)1.2 };
            yield return new object[] { "1,2", DbType.Currency, (decimal)1.2 };
            yield return new object[] { "1,2", DbType.Decimal, (decimal)1.2 };
            yield return new object[] { "1,2", DbType.Double, (double)1.2 };
            yield return new object[] { "1,2", DbType.Single, (Single)1.2 };
            yield return new object[] { "2:30", DbType.Time, new TimeSpan(0, 2, 30, 0), };
            yield return new object[] { "2018-12-23 22:56", DbType.DateTime, new DateTime(2018, 12, 23, 22, 56, 0), };
            yield return new object[] { "2018-12-23 22:56", DbType.DateTime2, new DateTime(2018, 12, 23, 22, 56, 0), };
            yield return new object[] { "23-12-2018 22:56", DbType.DateTime, new DateTime(2018, 12, 23, 22, 56, 0), "dd-MM-yyyy HH:mm" };
            yield return new object[] { new DateTime(2018, 12, 23, 22, 56, 0).ToString(CultureInfo.InvariantCulture), DbType.DateTime, new DateTime(2018, 12, 23, 22, 56, 0), };
            yield return new object[] { "2018-12-23", DbType.Date, new DateTime(2018, 12, 23, 0, 0, 0), };
            yield return new object[] { "2018-12-23 +2:30", DbType.DateTimeOffset, new DateTimeOffset(2018, 12, 23, 0, 0, 0, new TimeSpan(2, 30, 0)) };
            yield return new object[] { "23-12-2018 22:56 +2:30", DbType.DateTimeOffset, new DateTimeOffset(2018, 12, 23, 22, 56, 0, new TimeSpan(2, 30, 0)), "dd-MM-yyyy HH:mm zzz" };
            yield return new object[] { "3888CCA3-D11D-45C9-89A5-E6B72185D287", DbType.Guid, Guid.Parse("3888CCA3-D11D-45C9-89A5-E6B72185D287") };
            yield return new object[] { "3888CCA3D11D45C989A5E6B72185D287", DbType.Guid, Guid.Parse("3888CCA3-D11D-45C9-89A5-E6B72185D287") };
            yield return new object[] { "3888CCA3D11D45C989A5E6B72185D287", DbType.Guid, Guid.Parse("3888CCA3-D11D-45C9-89A5-E6B72185D287"), "N" };
            yield return new object[] { "3", DbType.Byte, (byte)3 };
            yield return new object[] { "3", DbType.SByte, (sbyte)3 };
            yield return new object[] { "3", DbType.Int16, (short)3 };
            yield return new object[] { " 3 ", DbType.Int16, (short)3 };
            yield return new object[] { "3", DbType.Int32, 3 };
            yield return new object[] { "3", DbType.Int64, (long)3 };
            yield return new object[] { "3", DbType.UInt16, (ushort)3 };
            yield return new object[] { "3", DbType.UInt32, (uint)3 };
            yield return new object[] { "3", DbType.UInt64, (ulong)3 };
            yield return new object[] { "3", DbType.AnsiString, "3" };
            //todo binary
            //todo default

        }

    }
}
