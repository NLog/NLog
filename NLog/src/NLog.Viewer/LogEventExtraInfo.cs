using System;
using System.Xml;
using System.Xml.Serialization;

namespace NLog.Viewer
{
    [XmlType("param")]
	public struct LogEventExtraInfo
	{
        [XmlAttribute("name")]
        public string Name;

        [XmlAttribute("value")]
        public string Value;
	}
}
