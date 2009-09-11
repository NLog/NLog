namespace NLog.Targets
{
    /// <summary>
    /// Line ending mode.
    /// </summary>
    public enum LineEndingMode
    {
        /// <summary>
        /// Insert platform-dependent end-of-line sequence after each line.
        /// </summary>
        Default,

        /// <summary>
        /// Insert CR LF sequence (ASCII 13, ASCII 10) after each line.
        /// </summary>
        CRLF,

        /// <summary>
        /// Insert CR character (ASCII 13) after each line.
        /// </summary>
        CR,

        /// <summary>
        /// Insert LF character (ASCII 10) after each line.
        /// </summary>
        LF,

        /// <summary>
        /// Don't insert any line ending.
        /// </summary>
        None,
    }
}
