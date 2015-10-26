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

	}

	public class Trait : Attribute
	{
		public Trait(string parm1, string parm2)
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