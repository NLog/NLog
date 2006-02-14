using System;
using System.Xml;
using System.Configuration;

namespace NLogViewer.Configuration
{
    public class ConfigSectionHandler: IConfigurationSectionHandler
    {
        object IConfigurationSectionHandler.Create(object parent, object configContext, XmlNode section)
        {
            return new NLogViewerConfiguration((XmlElement)section);
        }
    }
}
