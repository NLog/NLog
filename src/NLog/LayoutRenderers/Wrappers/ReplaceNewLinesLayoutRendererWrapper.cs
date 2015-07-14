using NLog.Config;
using System;
using System.ComponentModel;

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
        }

        /// <summary>
        /// Post-processes the rendered message. 
        /// </summary>
        /// <param name="text">The text to be post-processed.</param>
        /// <returns>String with newline characters replaced with spaces.</returns>
        protected override string Transform(string text)
        {
            return text.Replace(Environment.NewLine, " ");
        }

    }
}
