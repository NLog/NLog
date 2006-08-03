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

        public LayoutWithHeaderAndFooter()
        {
        }

        public ILayout Layout
        {
            get { return _layout; }
            set { _layout = value; }
        }

        public ILayout Header
        {
            get { return _header; }
            set { _header = value; }
        }

        public ILayout Footer
        {
            get { return _footer; }
            set { _footer = value; }
        }

        public string GetFormattedMessage(LogEventInfo logEvent)
        {
            return _layout.GetFormattedMessage(logEvent);
        }

        public int NeedsStackTrace()
        {
            int max = Layout.NeedsStackTrace();
            if (Header != null)
                max = Math.Max(max, Header.NeedsStackTrace());
            if (Footer != null)
                max = Math.Max(max, Footer.NeedsStackTrace());
            return max;
        }

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

        public void Precalculate(LogEventInfo logEvent)
        {
            Layout.Precalculate(logEvent);
            if (Header != null)
                Header.Precalculate(logEvent);
            if (Footer != null)
                Footer.Precalculate(logEvent);
        }

        public void Initialize()
        {
            Layout.Initialize();
            if (Header != null)
                Header.Initialize();
            if (Footer != null)
                Footer.Initialize();
        }

        public void Close()
        {
            Layout.Close();
            if (Header != null)
                Header.Close();
            if (Footer != null)
                Footer.Close();
        }

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
