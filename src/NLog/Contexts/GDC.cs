using System;

namespace NLog.Contexts
{
    /// <summary>
    /// Global Diagnostics Context - used for log4net compatibility.
    /// </summary>
    [Obsolete("Use GlobalDiagnosticsContext instead")]
    public class GDC : GlobalDiagnosticsContext
    {
    }
}
