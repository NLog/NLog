using System.Text;
using NLog.LayoutRenderers;

namespace NLog.Layouts
{
    /// <summary>
    /// A specialized layout that renders Log4j-compatible XML events.
    /// </summary>
    [Layout("Log4JXmlEventLayout")]
    public class Log4JXmlEventLayout : Layout
    {
        private Log4JXmlEventLayoutRenderer renderer = new Log4JXmlEventLayoutRenderer();

        /// <summary>
        /// Gets the <see cref="Log4JXmlEventLayoutRenderer"/> instance that renders log events.
        /// </summary>
        public Log4JXmlEventLayoutRenderer Renderer
        {
            get { return this.renderer; }
        }

        /// <summary>
        /// Renders the layout for the specified logging event by invoking layout renderers.
        /// </summary>
        /// <param name="logEvent">The logging event.</param>
        /// <returns>The rendered layout.</returns>
        public override string GetFormattedMessage(LogEventInfo logEvent)
        {
            string cachedValue;

            if (logEvent.TryGetCachedLayoutValue(this, out cachedValue))
            {
                return cachedValue;
            }

            StringBuilder sb = new StringBuilder(this.renderer.GetEstimatedBufferSize(logEvent));

            this.renderer.Append(sb, logEvent);
            logEvent.AddCachedLayoutValue(this, sb.ToString());
            return sb.ToString();
        }

        /// <summary>
        /// Returns the value indicating whether a stack trace and/or the source file
        /// information should be gathered during layout processing.
        /// </summary>
        /// <returns>0 - don't include stack trace<br/>1 - include stack trace without source file information<br/>2 - include full stack trace.</returns>
        public override StackTraceUsage GetStackTraceUsage()
        {
            return this.renderer.GetStackTraceUsage();
        }

        /// <summary>
        /// Returns the value indicating whether this layout includes any volatile 
        /// layout renderers.
        /// </summary>
        /// <returns>A value of <see langword="true" /> when the layout includes at least 
        /// one volatile renderer, <see langword="false"/> otherwise.</returns>
        /// <remarks>
        /// Volatile layout renderers are dependent on information not contained 
        /// in <see cref="LogEventInfo"/> (such as thread-specific data, MDC data, NDC data).
        /// </remarks>
        public override bool IsVolatile()
        {
            return this.renderer.IsVolatile();
        }
    }
}