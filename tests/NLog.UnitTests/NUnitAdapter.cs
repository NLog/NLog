// 
// Copyright (c) 2004-2011 Jaroslaw Kowalski <jaak@jkowalski.net>
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

#if !NUNIT

namespace NUnit.Framework
{
    using System;
    using MSAssert = Microsoft.VisualStudio.TestTools.UnitTesting.Assert;

    internal class Assert
    {
        public static void AreSame(object expected, object actual)
        {
            MSAssert.AreSame(expected, actual);
        }

        public static void AreSame(object expected, object actual, string message)
        {
            MSAssert.AreSame(expected, actual, message);
        }

        public static void AreNotSame(object expected, object actual)
        {
            MSAssert.AreNotSame(expected, actual);
        }

        public static void AreNotSame(object expected, object actual, string message)
        {
            MSAssert.AreNotSame(expected, actual, message);
        }

        public static void AreEqual(object expected, object actual)
        {
            MSAssert.AreEqual(expected, actual);          
        }

        public static void AreEqual(object expected, object actual, string message)
        {
            MSAssert.AreEqual(expected, actual, message);
        }

        public static void AreNotEqual(object expected, object actual)
        {
            MSAssert.AreNotEqual(expected, actual);
        }

        public static void AreNotEqual(object expected, object actual, string message)
        {
            MSAssert.AreNotEqual(expected, actual, message);
        }

        public static void IsNull(object value)
        {
            MSAssert.IsNull(value);
        }

        public static void IsNull(object value, string message)
        {
            MSAssert.IsNull(value, message);
        }

        public static void IsNotNull(object value)
        {
            MSAssert.IsNotNull(value);
        }

        public static void IsNotNull(object value, string message)
        {
            MSAssert.IsNotNull(value, message);
        }

        public static void IsTrue(bool value)
        {
            MSAssert.IsTrue(value);
        }

        public static void IsTrue(bool value, string message)
        {
            MSAssert.IsTrue(value, message);
        }

        public static void IsInstanceOfType(Type type, object value)
        {
            MSAssert.IsInstanceOfType(value, type);
        }

        public static void Fail(string errorMessage)
        {
            MSAssert.Fail(errorMessage);
        }

        public static void IsFalse(bool value)
        {
            MSAssert.IsFalse(value);
        }

        public static void IsFalse(bool value, string message)
        {
            MSAssert.IsFalse(value, message);
        }
    }
}

#endif