using System;
using System.ComponentModel;
using System.Linq;
using System.Text;
using NLog.Config;
using NLog.Internal.Fakeables;

namespace NLog.LayoutRenderers
{
    /// <summary>
    ///  Used to render the application domain name.
    ///  </summary>
    [ThreadAgnostic, LayoutRenderer("appdomain")]
    public class AppDomainLayoutRenderer : LayoutRenderer
    {
        private const string ShortFormat = "{0:00}";
        private const string LongFormat = "{0:0000}:{1}";
        private const string LongFormatCode = "Long";
        private const string ShortFormatCode = "Short";

        private readonly IAppDomain _currentDomain;

        /// <summary>
        /// Create a new renderer
        /// </summary>
        public AppDomainLayoutRenderer()
            : this(AppDomainWrapper.CurrentDomain)
        {
        }

        /// <summary>
        /// Create a new renderer
        /// </summary>
        public AppDomainLayoutRenderer(IAppDomain currentDomain)
        {
            _currentDomain = currentDomain;
            Format = LongFormatCode;
        }

        /// <summary>
        /// Format string. Possible values: "Short", "Long" or custom like {0} {1}. Default "Long"
        /// The first parameter is the  <see cref="AppDomain.Id"/>, the second the second the  <see cref="AppDomain.FriendlyName"/>
        /// This string is used in <see cref="string.Format(string,object[])"/>
        /// </summary>
        [DefaultParameter]
        [DefaultValue(LongFormatCode)]
        public string Format { get; set; }

        /// <summary>
        /// Render the layout
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="logEvent"></param>
        protected override void Append(StringBuilder builder, LogEventInfo logEvent)
        {
            var formattingString = GetFormattingString(Format);
            builder.Append(string.Format(formattingString, _currentDomain.Id, _currentDomain.FriendlyName));
        }

        /// <summary>
        /// Convert the formating string
        /// </summary>
        /// <param name="format"></param>
        /// <returns></returns>
        private static string GetFormattingString(string format)
        {
            string formattingString;
            if (format.Equals(LongFormatCode, StringComparison.OrdinalIgnoreCase))
            {
                formattingString = LongFormat;
            }
            else if (format.Equals(ShortFormatCode, StringComparison.OrdinalIgnoreCase))
            {
                formattingString = ShortFormat;
            }
            else
            {
                //custom value;
                formattingString = format;
            }
            return formattingString;
        }
    }
}