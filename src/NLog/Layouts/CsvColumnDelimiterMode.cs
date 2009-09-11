namespace NLog.Layouts
{
    /// <summary>
    /// Specifies allowed column delimiters.
    /// </summary>
    public enum CsvColumnDelimiterMode
    {
        /// <summary>
        /// Automatically detect from regional settings.
        /// </summary>
        Auto,

        /// <summary>
        /// Comma (ASCII 44).
        /// </summary>
        Comma,

        /// <summary>
        /// Semicolon (ASCII 59).
        /// </summary>
        Semicolon,

        /// <summary>
        /// Tab character (ASCII 9).
        /// </summary>
        Tab,

        /// <summary>
        /// Pipe character (ASCII 124).
        /// </summary>
        Pipe,

        /// <summary>
        /// Space character (ASCII 32).
        /// </summary>
        Space,

        /// <summary>
        /// Custom string, specified by the CustomDelimiter.
        /// </summary>
        Custom,
    }
}
