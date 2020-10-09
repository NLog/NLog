using System;
using System.Globalization;
using NLog.Internal;

namespace NLog.Config
{
    internal class SettingsReader : ISettingsReader
    {
        public T GetSetting<T>(string configName, string envName, T defaultValue) => 
            TryGetSetting(configName, envName, defaultValue, v => (T)Convert.ChangeType(v, typeof(T), CultureInfo.InvariantCulture));


        public LogLevel GetSetting(string configName, string envName, LogLevel defaultValue) => 
            TryGetSetting(configName, envName, defaultValue, LogLevel.FromString);

        private static T TryGetSetting<T>(string configName, string envName, T defaultValue, Func<string, T> conversionFunc)
        {
            var value = GetSettingString(configName, envName);
            if (value == null)
            {
                return defaultValue;
            }

            try
            {
                return conversionFunc(value);
            }
            catch (Exception exception)
            {
                if (exception.MustBeRethrownImmediately())
                {
                    throw;
                }

                return defaultValue;
            }
        }

        private static string GetSettingString(string configName, string envName)
        {
            try
            {
                var settingValue = GetAppSettings(configName);
                if (settingValue != null)
                    return settingValue;
            }
            catch (Exception ex)
            {
                if (ex.MustBeRethrownImmediately())
                {
                    throw;
                }
            }

            try
            {
                var settingValue = EnvironmentHelper.GetSafeEnvironmentVariable(envName);
                if (!string.IsNullOrEmpty(settingValue))
                    return settingValue;
            }
            catch (Exception ex)
            {
                if (ex.MustBeRethrownImmediately())
                {
                    throw;
                }
            }

            return null;
        }

        private static string GetAppSettings(string configName)
        {
#if !NETSTANDARD
            try
            {
                return System.Configuration.ConfigurationManager.AppSettings[configName];
            }
            catch (Exception ex)
            {
                if (ex.MustBeRethrownImmediately())
                {
                    throw;
                }
            }
#endif
            return null;
        }
    }
}
