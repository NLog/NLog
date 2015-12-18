using System;
using NLog.Config;

namespace NLog.Layouts
{
    /// <summary>
    /// A custom xml property for the log event.
    /// </summary>
    [NLogConfigurationItem]
    public class XmlProperty
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="XmlProperty"/> class.
        /// </summary>
        public XmlProperty()
            : this(null, null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="XmlProperty"/> class.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="layout">The layout.</param>
        public XmlProperty(string name, Layout layout)
        {
            this.Name = name;
            this.Layout = layout;
        }

        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        /// <value>
        /// The name.
        /// </value>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the layout.
        /// </summary>
        /// <value>
        /// The layout.
        /// </value>
        [RequiredParameter]
        public Layout Layout { get; set; }
    }
}