using System;

namespace NLog.Contexts
{
    /// <summary>
    /// Mapped Diagnostics Context - used for log4net compatibility.
    /// </summary>
    [Obsolete("Use MappedDiagnosticsContext instead")]
    public class MDC : MappedDiagnosticsContext
    {
    }
}
