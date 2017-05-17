using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NLog.Internal
{
    interface ILog4JXmlEvent
    {
        /// <summary>
        /// Gets or sets a value indicating whether to include contents of the <see cref="MappedDiagnosticsContext"/> dictionary.
        /// </summary>
        bool IncludeMdc { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to include contents of the <see cref="MappedDiagnosticsLogicalContext"/> dictionary.
        /// </summary>
        bool IncludeMdlc { get; set; }

        /// <summary>
        /// Gets or sets the option to include all properties from the log events
        /// </summary>
        bool IncludeAllProperties { get; set; }
    }
}
