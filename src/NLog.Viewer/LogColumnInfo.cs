using System;
using System.Xml;
using System.Xml.Serialization;

namespace NLog.Viewer
{
	public class LogColumnInfo
	{
        [XmlAttribute("name")]
        public string Name;

        [XmlAttribute("visible")]
        public bool Visible;

        [XmlAttribute("width")]
        public int Width;
	}
}
