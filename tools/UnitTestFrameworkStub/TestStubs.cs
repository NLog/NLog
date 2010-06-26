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

// just enough to compile NLog unit tests

namespace Microsoft.VisualStudio.TestTools.UnitTesting
{
    using System;

    public class TestClassAttribute : Attribute { }

    public class TestMethodAttribute : Attribute { }

    public class TestInitializeAttribute : Attribute { }

    public class TestCleanupAttribute : Attribute { }

    public class AssemblyInitializeAttribute : Attribute { }

    public class ExpectedExceptionAttribute : Attribute
    {
        public ExpectedExceptionAttribute(Type exceptionType)
        {
            this.ExceptionType = exceptionType;
        }

        public Type ExceptionType { get; private set; }
    }

    public class TestContext
    {
        
    }

    public class Assert
    {
        public static void AreSame(object o1, object o2)
        {
            AreSame(o1, o2, "Objects are not the same: " + GetObjectInfo(o1) + " and " + GetObjectInfo(o2));
        }

        public static void AreSame(object o1, object o2, string message)
        {
            IsTrue(ReferenceEquals(o1, o2), message);
        }

        public static void AreNotSame(object o1, object o2)
        {
            AreNotSame(o1, o2, "Objects are the same: " + GetObjectInfo(o1) + " and " + GetObjectInfo(o2));
        }

        public static void AreNotSame(object o1, object o2, string message)
        {
            IsTrue(!ReferenceEquals(o1, o2), message);
        }

        public static void AreEqual(object o1, object o2)
        {
            AreEqual(o1, o2, "Objects are not equal: " + GetObjectInfo(o1) + " and " + GetObjectInfo(o2));
        }

        private static string GetObjectInfo(object obj)
        {
            if (obj == null)
            {
                return "null";
            }

            return obj + "[" + obj.GetType() + "]";
       }

        public static void AreEqual(object o1, object o2, string message)
        {
            IsTrue(Equals(o1, o2), message);
        }

        public static void AreNotEqual(object o1, object o2)
        {
            AreNotEqual(o1, o2, "Objects are equal: " + GetObjectInfo(o1) + " and " + GetObjectInfo(o2));
        }

        public static void AreNotEqual(object o1, object o2, string message)
        {
            IsTrue(!Equals(o1, o2), message);
        }

        public static void IsNull(object o)
        {
            IsNull(o, "Object is not null: " + GetObjectInfo(o));
        }

        public static void IsNull(object o, string message)
        {
            IsTrue(o == null, message);
        }

        public static void IsNotNull(object o)
        {
            IsNotNull(o, "Object is not null: " + GetObjectInfo(o));
        }

        public static void IsNotNull(object o, string message)
        {
            IsTrue(o != null, message);
        }

        public static void IsTrue(bool value)
        {
            IsTrue(value, "Assertion failed.");
        }

        public static void IsTrue(bool value, string message)
        {
            if (!value)
            {
                Fail(message);
            }
        }

        public static void IsInstanceOfType(object o, Type type)
        {
            IsTrue(type.IsInstanceOfType(o), "Object " + GetObjectInfo(o) + " is not an instance of " + type);
        }

        public static void Fail(string errorMessage)
        {
            throw new TestFailureException(errorMessage);
        }

        public static void IsFalse(bool value)
        {
            IsTrue(!value);
        }
    }

    public class TestFailureException : Exception
    {
        public TestFailureException(string message)
            : base(message)
        {
        }
    }
}
