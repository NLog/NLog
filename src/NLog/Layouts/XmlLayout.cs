using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NLog.Config;
using NLog.LayoutRenderers;

namespace NLog.Layouts
{
    /// <summary>
    /// An Xml Layout for NLog.
    /// </summary>
    [Layout("XmlLayout")]
    public class XmlLayout : Layout
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="XmlLayout"/> class.
        /// </summary>
        public XmlLayout()
        {
            Renderer = new XmlLayoutRenderer();
            Properties = new List<XmlProperty>();
        }

        /// <summary>
        /// Gets the custom properties for the log event.
        /// </summary>
        [ArrayParameter(typeof(XmlProperty), "property")]
        public IList<XmlProperty> Properties { get; }

        /// <summary>
        /// Gets the layout renderer.
        /// </summary>
        public XmlLayoutRenderer Renderer { get; }

        /// <summary>
        /// Renders the layout for the specified logging event by invoking layout renderers.
        /// </summary>
        /// <param name="logEvent">The logging event.</param>
        /// <returns>
        /// The rendered layout.
        /// </returns>
        protected override string GetFormattedMessage(LogEventInfo logEvent)
        {
            // add custom properties to log event
            foreach (var xmlProperty in Properties)
            {
                string name = xmlProperty.Name;
                string text = xmlProperty.Layout.Render(logEvent);

                logEvent.Properties[name] =  text;
            }

            return this.Renderer.Render(logEvent);
        }
    }
}
