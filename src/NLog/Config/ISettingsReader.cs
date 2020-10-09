namespace NLog.Config
{
    /// <summary>
    /// 
    /// </summary>
    public interface ISettingsReader
    {
        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="configName"></param>
        /// <param name="envName"></param>
        /// <param name="defaultValue"></param>
        /// <returns></returns>
        T GetSetting<T>(string configName, string envName, T defaultValue);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="configName"></param>
        /// <param name="envName"></param>
        /// <param name="defaultValue"></param>
        /// <returns></returns>
        LogLevel GetSetting(string configName, string envName, LogLevel defaultValue);
    }
}