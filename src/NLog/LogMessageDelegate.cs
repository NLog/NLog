namespace NLog
{
    /// <summary>
    /// Returns a log message. Used to defer calculation of 
    /// the log message until it's actually needed.
    /// </summary>
    /// <returns>Log message.</returns>
    public delegate string LogMessageDelegate();
}
