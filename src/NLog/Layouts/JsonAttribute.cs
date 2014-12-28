using NLog.Config;

namespace NLog.Layouts
{
    /// <summary>
    /// JSON attribute.
    /// </summary>
    [NLogConfigurationItem]
    [ThreadAgnostic]
    public class JsonAttribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="JsonAttribute" /> class.
        /// </summary>
        public JsonAttribute() : this(null, null) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="JsonAttribute" /> class.
        /// </summary>
        /// <param name="name">The name of the attribute.</param>
        /// <param name="layout">The layout of the attribute's value.</param>
        public JsonAttribute(string name, Layout layout)
        {
            this.Name = name;
            this.Layout = layout;
        }

        /// <summary>
        /// Gets or sets the name of the attribute.
        /// </summary>
        [RequiredParameter]
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the layout that will be rendered as the attribute's value.
        /// </summary>
        [RequiredParameter]
        public Layout Layout { get; set; }
    }
}