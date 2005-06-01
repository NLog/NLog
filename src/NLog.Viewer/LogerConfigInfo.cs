using System;
using System.Xml;
using System.Xml.Serialization;
using System.Drawing;

namespace NLog.Viewer
{
	public class LoggerConfigInfo
	{
        [XmlAttribute("name")]
        public string Name;

        [XmlAttribute("color")]
        public string color;

        public Color Color
        {
            get { return Color.FromName(color); }
        }
	}
}
