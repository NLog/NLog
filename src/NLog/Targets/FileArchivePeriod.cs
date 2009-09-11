namespace NLog.Targets
{
    /// <summary>
    /// Modes of archiving files based on time.
    /// </summary>
    public enum FileArchivePeriod
    {
        /// <summary>
        /// Don't archive based on time.
        /// </summary>
        None,

        /// <summary>
        /// Archive every year.
        /// </summary>
        Year,

        /// <summary>
        /// Archive every month.
        /// </summary>
        Month,

        /// <summary>
        /// Archive daily.
        /// </summary>
        Day,

        /// <summary>
        /// Archive every hour.
        /// </summary>
        Hour,

        /// <summary>
        /// Archive every minute.
        /// </summary>
        Minute
    }
}
