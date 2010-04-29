using NLog.LayoutRenderers;
using System.Text;
using NLog.Config;
using System.Globalization;
using System;

namespace NLog.Layouts
{
    /// <summary>
    /// A specialized layout that supports header and footer.
    /// </summary>
    [Layout("LayoutWithHeaderAndFooter")]
    public class LayoutWithHeaderAndFooter : ILayout, ILayoutWithHeaderAndFooter
    {
        private ILayout _header = null;
        private ILayout _footer = null;
        private ILayout _layout = null;

        /// <summary>
        /// Initializes a new instance of the <see cref="LayoutWithHeaderAndFooter"/> class.
        /// </summary>
        public LayoutWithHeaderAndFooter()
        {
        }

        /// <summary>
        /// Main layout (can be repeated multiple times)
        /// </summary>
        /// <value></value>
        public ILayout Layout
        {
            get { return _layout; }
            set { _layout = value; }
        }

        /// <summary>
        /// Header
        /// </summary>
        /// <value></value>
        public ILayout Header
        {
            get { return _header; }
            set { _header = value; }
        }

        /// <summary>
        /// Footer
        /// </summary>
        /// <value></value>
        public ILayout Footer
        {
            get { return _footer; }
            set { _footer = value; }
        }

        /// <summary>
        /// Renders the layout for the specified logging event by invoking layout renderers.
        /// </summary>
        /// <param name="logEvent">The logging event.</param>
        /// <returns>The rendered layout.</returns>
        public string GetFormattedMessage(LogEventInfo logEvent)
        {
            return _layout.GetFormattedMessage(logEvent);
        }

        /// <summary>
        /// Returns the value indicating whether a stack trace and/or the source file
        /// information should be gathered during layout processing.
        /// </summary>
        /// <returns>
        /// 0 - don't include stack trace<br/>1 - include stack trace without source file information<br/>2 - include full stack trace
        /// </returns>
        public int NeedsStackTrace()
        {
            int max = Layout.NeedsStackTrace();
            if (Header != null)
                max = Math.Max(max, Header.NeedsStackTrace());
            if (Footer != null)
                max = Math.Max(max, Footer.NeedsStackTrace());
            return max;
        }

        /// <summary>
        /// Returns the value indicating whether this layout includes any volatile
        /// layout renderers.
        /// </summary>
        /// <returns>
        /// 	<see langword="true"/> when the layout includes at least
        /// one volatile renderer, <see langword="false"/> otherwise.
        /// </returns>
        /// <remarks>
        /// Volatile layout renderers are dependent on information not contained
        /// in <see cref="LogEventInfo"/> (such as thread-specific data, MDC data, NDC data).
        /// </remarks>
        public bool IsVolatile()
        {
            if (Layout.IsVolatile())
                return true;

            if (Header != null && Header.IsVolatile())
                return true;

            if (Footer != null && Footer.IsVolatile())
                return true;

            return false;
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
            Layout.Precalculate(logEvent);
            if (Header != null)
                Header.Precalculate(logEvent);
            if (Footer != null)
                Footer.Precalculate(logEvent);
        }

        /// <summary>
        /// Initializes the layout.
        /// </summary>
        public void Initialize()
        {
            Layout.Initialize();
            if (Header != null)
                Header.Initialize();
            if (Footer != null)
                Footer.Initialize();
        }

        /// <summary>
        /// Closes the layout.
        /// </summary>
        public void Close()
        {
            Layout.Close();
            if (Header != null)
                Header.Close();
            if (Footer != null)
                Footer.Close();
        }

        /// <summary>
        /// Add this layout and all sub-layouts to the specified collection..
        /// </summary>
        /// <param name="layouts">The collection of layouts.</param>
        public void PopulateLayouts(LayoutCollection layouts)
        {
            layouts.Add(this);
            Layout.PopulateLayouts(layouts);
            if (Header != null)
                Header.PopulateLayouts(layouts);
            if (Footer != null)
                Footer.PopulateLayouts(layouts);
        }
    }
}
