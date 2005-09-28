using System;
using System.Xml;
using System.Configuration;
using System.Collections;
using System.Collections.Specialized;

namespace NLogViewer
{
    public class NLogViewerConfiguration
    {
        private StringCollection _extensionAssemblies = new StringCollection();

        public NLogViewerConfiguration(XmlElement element)
        {
            foreach (XmlElement el in element.SelectNodes("extensions/assembly"))
            {
                string name = el.GetAttribute("name");
                _extensionAssemblies.Add(name);
            }
        }

        public StringCollection ExtensionAssemblies
        {
            get { return _extensionAssemblies; }
        }

        public static NLogViewerConfiguration Configuration
        {
            get { return (NLogViewerConfiguration)ConfigurationSettings.GetConfig("nlogviewer"); }
        }
    }
}
