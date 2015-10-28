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

#if __IOS__ || __ANDROID__

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using MonoTouch.Foundation;
using MonoTouch.UIKit;

namespace NLog.UnitTests
{
	public class Fact : NUnit.Framework.TestAttribute
	{
        public string Skip { get; set; }
	}

	public class Trait : Attribute
	{
		public Trait(string parm1, string parm2)
		{

		}
	}
    public class TheoryAttribute2 : NUnit.Framework.TheoryAttribute
    {
        public string Skip { get; set; }
    }

    public class InlineData : NUnit.Framework.TestCaseAttribute
    {
        public InlineData(params object[] arguments) : base(arguments)
        {
        }

        public InlineData(object arg) : base(arg)
        {
        }

        public InlineData(object arg1, object arg2) : base(arg1, arg2)
        {
        }

        public InlineData(object arg1, object arg2, object arg3) : base(arg1, arg2, arg3)
        {
        }
    }

    public class PropertyData : NUnit.Framework.TestCaseSourceAttribute
    {
        public PropertyData(string sourceName) : base(sourceName)
        {
        }

        public PropertyData(Type sourceType, string sourceName) : base(sourceType, sourceName)
        {
        }

        public PropertyData(Type sourceType) : base(sourceType)
        {
        }
    }

}

namespace NUnit.Framework.NLog
{
	public class Assert : NUnit.Framework.Assert
	{
		public static void Equal(object expected, object actual)
		{
			AreEqual(expected, actual);
		}

        public static void NotEqual(object value1, object value2)
		{
			AreNotEqual(value1,value2);
		}

		public static void Same(object expected, object actual)
		{
			AreSame(expected,actual);
		}

		public static void NotSame(object expected, object actual)
		{
			AreNotSame(expected, actual);
		}


        public static void Contains(object expected, object actual)
        {
#if !DEBUG
#error fixen
#endif
        }        public static void ThrowsDelegate(object expected, object actual)
        {
#if !DEBUG
#error fixen
#endif
        }

        public static void IsType(Type type, object value)
		{
			
			AreEqual(type, value.GetType());
		}

		public static void IsType<T>(object value)
		{
			AreEqual(typeof(T), value.GetType());
		}

		public static void DoesNotContain(object value, IEnumerable collection)
		{
			bool found = false;
			foreach (var item in collection)
			{
				if (item == value)
				{
					found = true;
					break;
				}
			}
			AreNotEqual(true,found);
		}
	}

}

#endif