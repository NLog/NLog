using System;

namespace NLog.Contexts
{
    /// <summary>
    /// Nested Diagnostics Context - for log4net compatibility.
    /// </summary>
    [Obsolete("Use NestedDiagnosticsContext")]
    public class NDC : NestedDiagnosticsContext
    {
    }
}
