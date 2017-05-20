using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NLog.Targets
{
    /// <summary>
    /// Options for JSON serialisation
    /// </summary>
    public class JsonSerializeOptions
    {
        /// <summary>
        /// Add quotes arround object keys?
        /// </summary>
        [DefaultValue(true)]
        public bool QuoteKeys { get; set; }

        /// <summary>
        /// Formatprovider for value
        /// </summary>
        public IFormatProvider FormatProvider { get; set; }

        /// <summary>
        /// Format string for value
        /// </summary>
        public string Format { get; set; }

        /// <summary>
        /// Should non-ascii characters be encoded
        /// </summary>
        public bool EscapeUnicode { get; set; }


        /// <summary>Initializes a new instance of the <see cref="T:System.Object" /> class.</summary>
        public JsonSerializeOptions()
        {
            QuoteKeys = true;
        }
    }
}
