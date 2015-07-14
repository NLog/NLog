using NLog.Config;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NLog.LayoutRenderers.Wrappers
{

    /// <summary>
    /// Replaces newline characters from the result of another layout renderer with spaces.
    /// </summary>
    [LayoutRenderer("replace-newlines")]
    [AmbientProperty("ReplaceNewLines")]
    [ThreadAgnostic]
    public sealed class ReplaceNewLinesLayoutRendererWrapper : WrapperLayoutRendererBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ReplaceNewLinesLayoutRendererWrapper" /> class.
        /// </summary>
        public ReplaceNewLinesLayoutRendererWrapper()
        {
            this.ReplaceNewLines = true;
        }

        /// <summary>
        /// Gets or sets a value indicating whether lower case conversion should be applied.
        /// </summary>
        /// <value>A value of <c>true</c> if lower case conversion should be applied; otherwise, <c>false</c>.</value>
        /// <docgen category='Transformation Options' order='10' />
        [DefaultValue(true)]
        public bool ReplaceNewLines { get; set; }

        /// <summary>
        /// Post-processes the rendered message. 
        /// </summary>
        /// <param name="text">The text to be post-processed.</param>
        /// <returns>String with newline characters replaced with spaces.</returns>
        protected override string Transform(string text)
        {
            if (this.ReplaceNewLines)
            {
                return text.Replace(Environment.NewLine, " ");
            }
            else
            {
                return text;
            }
        }

    }
}
