// 
// Copyright (c) 2004-2010 Jaroslaw Kowalski <jaak@jkowalski.net>
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
        public static void AreSame(object o1, object o2)
        {
            MSAssert.AreSame(o1, o2);
        }

        public static void AreSame(object o1, object o2, string message)
        {
            MSAssert.AreSame(o1, o2, message);
        }

        public static void AreNotSame(object o1, object o2)
        {
            MSAssert.AreNotSame(o1, o2);
        }

        public static void AreNotSame(object o1, object o2, string message)
        {
            MSAssert.AreNotSame(o1, o2, message);
        }

        public static void AreEqual(object o1, object o2)
        {
            MSAssert.AreEqual(o1, o2);          
        }

        public static void AreEqual(object o1, object o2, string message)
        {
            MSAssert.AreEqual(o1, o2, message);
        }

        public static void AreNotEqual(object o1, object o2)
        {
            MSAssert.AreNotEqual(o1, o2);
        }

        public static void AreNotEqual(object o1, object o2, string message)
        {
            MSAssert.AreNotEqual(o1, o2, message);
        }

        public static void IsNull(object o)
        {
            MSAssert.IsNull(o);
        }

        public static void IsNull(object o, string message)
        {
            MSAssert.IsNull(o, message);
        }

        public static void IsNotNull(object o)
        {
            MSAssert.IsNotNull(o);
        }

        public static void IsNotNull(object o, string message)
        {
            MSAssert.IsNotNull(o, message);
        }

        public static void IsTrue(bool value)
        {
            MSAssert.IsTrue(value);
        }

        public static void IsTrue(bool value, string message)
        {
            MSAssert.IsTrue(value, message);
        }

        public static void IsInstanceOfType(Type type, object o)
        {
            MSAssert.IsInstanceOfType(o, type);
        }

        public static void Fail(string errorMessage)
        {
            MSAssert.Fail(errorMessage);
        }

        public static void IsFalse(bool value)
        {
            MSAssert.IsFalse(value);
        }
    }
}

#endif