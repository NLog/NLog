using System.Collections.Generic;
using NLog.Config;
using NLog.Internal;

namespace NLog.Layouts
{
    /// <summary>
    /// A specialized layout that supports header and footer.
    /// </summary>
    [Layout("LayoutWithHeaderAndFooter")]
    public class LayoutWithHeaderAndFooter : Layout
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="LayoutWithHeaderAndFooter"/> class.
        /// </summary>
        public LayoutWithHeaderAndFooter()
        {
        }

        /// <summary>
        /// Gets or sets the body layout (can be repeated multiple times).
        /// </summary>
        /// <value></value>
        public Layout Layout { get; set; }

        /// <summary>
        /// Gets or sets the header layout.
        /// </summary>
        /// <value></value>
        public Layout Header { get; set; }

        /// <summary>
        /// Gets or sets the footer layout.
        /// </summary>
        /// <value></value>
        public Layout Footer { get; set; }

        /// <summary>
        /// Renders the layout for the specified logging event by invoking layout renderers.
        /// </summary>
        /// <param name="logEvent">The logging event.</param>
        /// <returns>The rendered layout.</returns>
        public override string GetFormattedMessage(LogEventInfo logEvent)
        {
            return this.Layout.GetFormattedMessage(logEvent);
        }

        /// <summary>
        /// Gets or sets a value indicating whether stack trace information should be gathered during log event processing. 
        /// </summary>
        /// <returns>A <see cref="StackTraceUsage" /> value that determines stack trace handling.</returns>
        public override StackTraceUsage GetStackTraceUsage()
        {
            StackTraceUsage max = Layout.GetStackTraceUsage();
            if (this.Header != null)
            {
                max = StackTraceUsageUtils.Max(max, this.Header.GetStackTraceUsage());
            }

            if (this.Footer != null)
            {
                max = StackTraceUsageUtils.Max(max, this.Footer.GetStackTraceUsage());
            }

            return max;
        }

        /// <summary>
        /// Returns the value indicating whether this layout includes any volatile
        /// layout renderers.
        /// </summary>
        /// <returns>
        /// A value of <see langword="true"/> when the layout includes at least
        /// one volatile renderer, <see langword="false"/> otherwise.
        /// .</returns>
        /// <remarks>
        /// Volatile layout renderers are dependent on information not contained
        /// in <see cref="LogEventInfo"/> (such as thread-specific data, MDC data, NDC data).
        /// </remarks>
        public override bool IsVolatile()
        {
            if (Layout.IsVolatile())
            {
                return true;
            }

            if (this.Header != null && this.Header.IsVolatile())
            {
                return true;
            }

            if (this.Footer != null && this.Footer.IsVolatile())
            {
                return true;
            }

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
        public override void Precalculate(LogEventInfo logEvent)
        {
            Layout.Precalculate(logEvent);
            if (this.Header != null)
            {
                this.Header.Precalculate(logEvent);
            }

            if (this.Footer != null)
            {
                this.Footer.Precalculate(logEvent);
            }
        }

        /// <summary>
        /// Initializes the layout.
        /// </summary>
        public override void Initialize()
        {
            base.Initialize();
            if (Layout != null)
            {
                Layout.Initialize();
            }

            if (this.Header != null)
            {
                this.Header.Initialize();
            }

            if (this.Footer != null)
            {
                this.Footer.Initialize();
            }
        }

        /// <summary>
        /// Closes the layout.
        /// </summary>
        public override void Close()
        {
            if (Layout != null && Layout.IsInitialized)
            {
                Layout.Close();
            }

            if (this.Header != null && this.Header.IsInitialized)
            {
                this.Header.Close();
            }

            if (this.Footer != null && this.Footer.IsInitialized)
            {
                this.Footer.Close();
            }

            base.Close();
        }

        /// <summary>
        /// Add this layout and all sub-layouts to the specified collection..
        /// </summary>
        /// <param name="layouts">The collection of layouts.</param>
        public override void PopulateLayouts(ICollection<Layout> layouts)
        {
            layouts.Add(this);
            if (Layout != null)
            {
                Layout.PopulateLayouts(layouts);
            }

            if (this.Header != null)
            {
                this.Header.PopulateLayouts(layouts);
            }

            if (this.Footer != null)
            {
                this.Footer.PopulateLayouts(layouts);
            }
        }
    }
}
