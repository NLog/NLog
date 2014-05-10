using System;
using System.Text.RegularExpressions;
using System.Xml;

namespace NLog.Internal
{
	/// <summary>
	///  Helper class for XML
	/// </summary>
	public static class XmlHelper
	{
		// found on http://stackoverflow.com/questions/397250/unicode-regex-invalid-xml-characters/961504#961504
		// filters control characters but allows only properly-formed surrogate sequences
#if !SILVERLIGHT
		private static readonly Regex InvalidXmlChars = new Regex(
			@"(?<![\uD800-\uDBFF])[\uDC00-\uDFFF]|[\uD800-\uDBFF](?![\uDC00-\uDFFF])|[\x00-\x08\x0B\x0C\x0E-\x1F\x7F-\x9F\uFEFF\uFFFE\uFFFF]",
			RegexOptions.Compiled);
#else
		private static readonly Regex InvalidXmlChars = new Regex(
			@"(?<![\uD800-\uDBFF])[\uDC00-\uDFFF]|[\uD800-\uDBFF](?![\uDC00-\uDFFF])|[\x00-\x08\x0B\x0C\x0E-\x1F\x7F-\x9F\uFEFF\uFFFE\uFFFF]");
#endif

		/// <summary>
		/// removes any unusual unicode characters that can't be encoded into XML
		/// </summary>
		private static string RemoveInvalidXmlChars(string text)
		{
			return String.IsNullOrEmpty(text) ? "" : InvalidXmlChars.Replace(text, "");
		}


		/// <summary>
		/// Safe version of WriteAttributeString
		/// </summary>
		/// <param name="writer"></param>
		/// <param name="prefix"></param>
		/// <param name="localName"></param>
		/// <param name="ns"></param>
		/// <param name="value"></param>
		public static void WriteAttributeSafeString(this XmlWriter writer, string prefix, string localName, string ns, string value)
		{
			writer.WriteAttributeString(RemoveInvalidXmlChars(prefix), RemoveInvalidXmlChars(localName), RemoveInvalidXmlChars(ns), RemoveInvalidXmlChars(value));
		}

		/// <summary>
		/// Safe version of WriteAttributeString
		/// </summary>
		/// <param name="writer"></param>
		/// <param name="thread"></param>
		/// <param name="localName"></param>
		public static void WriteAttributeSafeString(this XmlWriter writer, string thread, string localName)
		{
			writer.WriteAttributeString(RemoveInvalidXmlChars(thread), RemoveInvalidXmlChars(localName));
		}



		/// <summary>
		/// Safe version of WriteElementSafeString
		/// </summary>
		/// <param name="writer"></param>
		/// <param name="prefix"></param>
		/// <param name="localName"></param>
		/// <param name="ns"></param>
		/// <param name="value"></param>
		public static void WriteElementSafeString(this XmlWriter writer, string prefix, string localName, string ns, string value)
		{
			writer.WriteElementString(RemoveInvalidXmlChars(prefix), RemoveInvalidXmlChars(localName), RemoveInvalidXmlChars(ns),
			                          RemoveInvalidXmlChars(value));
		}

		/// <summary>
		/// Safe version of WriteCData
		/// </summary>
		/// <param name="writer"></param>
		/// <param name="text"></param>
		public static void WriteSafeCData(this XmlWriter writer, string text)
		{
			writer.WriteCData(RemoveInvalidXmlChars(text));
		}
	}
}
