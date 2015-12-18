using System;
using System.Text;
using NLog.Internal;

namespace NLog.LayoutRenderers
{
    /// <summary>
    /// An Xml Layout Renderer for NLog.
    /// </summary>
    [LayoutRenderer("xml")]
    public class XmlLayoutRenderer : LayoutRenderer
    {
        /// <summary>
        /// Appends the specified builder.
        /// </summary>
        /// <param name="builder">The builder.</param>
        /// <param name="logEventInfo">The log event information.</param>
        protected override void Append(StringBuilder builder, LogEventInfo logEventInfo)
        {
            var writer = new LogEventInfoXmlWriter();
            writer.Write(builder, logEventInfo);
        }
    }
}
