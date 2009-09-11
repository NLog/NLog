namespace NLog.Targets.Wrappers
{
    /// <summary>
    /// Impersonation level.
    /// </summary>
    public enum SecurityImpersonationLevel
    {
        /// <summary>
        /// Anonymous Level.
        /// </summary>
        Anonymous = 0,

        /// <summary>
        /// Identification Level.
        /// </summary>
        Identification = 1,

        /// <summary>
        /// Impersonation Level.
        /// </summary>
        Impersonation = 2,

        /// <summary>
        /// Delegation Level.
        /// </summary>
        Delegation = 3
    }
}
