using NLog.LayoutRenderers;
using System.Text;
namespace NLog.Layouts
{
    /// <summary>
    /// A specialized layout that renders Log4j-compatible XML events.
    /// </summary>
    [Layout("Log4JXmlEventLayout")]
    public class Log4JXmlEventLayout : ILayout
    {
        private Log4JXmlEventLayoutRenderer _renderer = new Log4JXmlEventLayoutRenderer();

        /// <summary>
        /// Returns the <see cref="Log4JXmlEventLayoutRenderer"/> instance that renders log events.
        /// </summary>
        public Log4JXmlEventLayoutRenderer Renderer
        {
            get { return _renderer; }
        }

        /// <summary>
        /// Renders the layout for the specified logging event by invoking layout renderers.
        /// </summary>
        /// <param name="logEvent">The logging event.</param>
        /// <returns>The rendered layout.</returns>
        public string GetFormattedMessage(LogEventInfo logEvent)
        {
            string cachedValue = logEvent.GetCachedLayoutValue(this);
            if (cachedValue != null)
                return cachedValue;

            StringBuilder sb = new StringBuilder(_renderer.GetEstimatedBufferSize(logEvent));

            _renderer.Append(sb, logEvent);
            logEvent.AddCachedLayoutValue(this, sb.ToString());
            return sb.ToString();
        }

        /// <summary>
        /// Returns the value indicating whether a stack trace and/or the source file
        /// information should be gathered during layout processing.
        /// </summary>
        /// <returns>0 - don't include stack trace<br/>1 - include stack trace without source file information<br/>2 - include full stack trace</returns>
        public int NeedsStackTrace()
        {
            return _renderer.NeedsStackTrace();
        }

        /// <summary>
        /// Returns the value indicating whether this layout includes any volatile 
        /// layout renderers.
        /// </summary>
        /// <returns><see langword="true" /> when the layout includes at least 
        /// one volatile renderer, <see langword="false"/> otherwise.</returns>
        /// <remarks>
        /// Volatile layout renderers are dependent on information not contained 
        /// in <see cref="LogEventInfo"/> (such as thread-specific data, MDC data, NDC data).
        /// </remarks>
        public bool IsVolatile()
        {
            return _renderer.IsVolatile();
        }

        /// <summary>
        /// Precalculates the layout for the specified log event and stores the result
        /// in per-log event cache.
        /// </summary>
        /// <param name="logEvent">The log event.</param>
        /// <remarks>
        /// Calling this method enables you to store the log event in a buffer
        /// and/or potentially evaluate it in another thread even though the 
        /// layout may contain thread-dependent renderer.
        /// </remarks>
        public void Precalculate(LogEventInfo logEvent)
        {
            GetFormattedMessage(logEvent);
        }

        /// <summary>
        /// Initializes the layout.
        /// </summary>
        public void Initialize()
        {
        }

        /// <summary>
        /// Closes the layout.
        /// </summary>
        public void Close()
        {
        }

        /// <summary>
        /// Add this layout and all sub-layouts to the specified collection..
        /// </summary>
        /// <param name="layouts">The collection of layouts.</param>
        public void PopulateLayouts(LayoutCollection layouts)
        {
            layouts.Add(this);
        }
    }
}