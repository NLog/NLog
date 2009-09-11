namespace NLog.Targets
{
    /// <summary>
    /// Specifies the way archive numbering is performed.
    /// </summary>
    public enum ArchiveNumberingMode
    {
        /// <summary>
        /// Sequence style numbering. The most recent archive has the highest number.
        /// </summary>
        Sequence,

        /// <summary>
        /// Rolling style numbering (the most recent is always #0 then #1, ..., #N.
        /// </summary>
        Rolling,
    }

}
