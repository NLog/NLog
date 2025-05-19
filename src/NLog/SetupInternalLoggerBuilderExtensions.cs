//
// Copyright (c) 2004-2024 Jaroslaw Kowalski <jaak@jkowalski.net>, Kim Christensen, Julian Verdurmen
//
// All rights reserved.
//
// Redistribution and use in source and binary forms, with or without
// modification, are permitted provided that the following conditions
// are met:
//
// * Redistributions of source code must retain the above copyright notice,
//   this list of conditions and the following disclaimer.
//
// * Redistributions in binary form must reproduce the above copyright notice,
//   this list of conditions and the following disclaimer in the documentation
//   and/or other materials provided with the distribution.
//
// * Neither the name of Jaroslaw Kowalski nor the names of its
//   contributors may be used to endorse or promote products derived from this
//   software without specific prior written permission.
//
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS"
// AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE
// IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE
// ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT OWNER OR CONTRIBUTORS BE
// LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR
// CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF
// SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS
// INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN
// CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE)
// ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF
// THE POSSIBILITY OF SUCH DAMAGE.
//

namespace NLog
{
    using System;
    using System.Globalization;
    using System.IO;
    using NLog.Common;
    using NLog.Config;
    using NLog.Internal;

    /// <summary>
    /// Extension methods to setup NLog <see cref="InternalLogger"/> options
    /// </summary>
    public static class SetupInternalLoggerBuilderExtensions
    {
        /// <summary>
        /// Configures <see cref="InternalLogger.LogLevel"/>
        /// </summary>
        public static ISetupInternalLoggerBuilder SetMinimumLogLevel(this ISetupInternalLoggerBuilder setupBuilder, LogLevel logLevel)
        {
            var orgValue = InternalLogger.LogLevel;
            InternalLogger.LogLevel = logLevel;
            if (InternalLogger.LogLevel != LogLevel.Off && orgValue == LogLevel.Off)
            {
                var orgLogFile = InternalLogger.LogFile;
                if (!string.IsNullOrEmpty(orgLogFile))
                {
                    InternalLogger.LogFile = orgLogFile;    // InternalLogger.LogFile property-setter skips directory creation when LogLevel.Off
                }
            }
            return setupBuilder;
        }

        /// <summary>
        /// Configures <see cref="InternalLogger.LogFile"/>
        /// </summary>
        public static ISetupInternalLoggerBuilder LogToFile(this ISetupInternalLoggerBuilder setupBuilder, string fileName)
        {
            InternalLogger.LogFile = fileName;
            return setupBuilder;
        }

        /// <summary>
        /// Configures <see cref="InternalLogger.LogToConsole"/>
        /// </summary>
        public static ISetupInternalLoggerBuilder LogToConsole(this ISetupInternalLoggerBuilder setupBuilder, bool enabled)
        {
            InternalLogger.LogToConsole = enabled;
            return setupBuilder;
        }

        /// <summary>
        /// Configures <see cref="InternalLogger.LogWriter"/>
        /// </summary>
        public static ISetupInternalLoggerBuilder LogToWriter(this ISetupInternalLoggerBuilder setupBuilder, TextWriter writer)
        {
            InternalLogger.LogWriter = writer;
            return setupBuilder;
        }

        /// <summary>
        /// Configures <see cref="InternalLogger.InternalEventOccurred"/>
        /// </summary>
        public static ISetupInternalLoggerBuilder AddEventSubscription(this ISetupInternalLoggerBuilder setupBuilder, InternalEventOccurredHandler eventSubscriber)
        {
            InternalLogger.InternalEventOccurred += eventSubscriber;
            return setupBuilder;
        }

        /// <summary>
        /// Configures <see cref="InternalLogger.InternalEventOccurred"/>
        /// </summary>
        public static ISetupInternalLoggerBuilder RemoveEventSubscription(this ISetupInternalLoggerBuilder setupBuilder, InternalEventOccurredHandler eventSubscriber)
        {
            InternalLogger.InternalEventOccurred -= eventSubscriber;
            return setupBuilder;
        }

        /// <summary>
        /// Resets the InternalLogger configuration without resolving default values from Environment-variables or App.config
        /// </summary>
        public static ISetupInternalLoggerBuilder ResetConfig(this ISetupInternalLoggerBuilder setupBuilder)
        {
            InternalLogger.Reset();
            return setupBuilder;
        }

        /// <summary>
        /// Configure the InternalLogger properties from Environment-variables and App.config using <see cref="InternalLogger.Reset"/>
        /// </summary>
        /// <remarks>
        /// Recognizes the following environment-variables:
        ///
        /// - NLOG_INTERNAL_LOG_LEVEL
        /// - NLOG_INTERNAL_LOG_FILE
        /// - NLOG_INTERNAL_LOG_TO_CONSOLE
        /// - NLOG_INTERNAL_LOG_TO_CONSOLE_ERROR
        /// - NLOG_INTERNAL_INCLUDE_TIMESTAMP
        ///
        /// Legacy .NetFramework platform will also recognizes the following app.config settings:
        ///
        /// - nlog.internalLogLevel
        /// - nlog.internalLogFile
        /// - nlog.internalLogToConsole
        /// - nlog.internalLogToConsoleError
        /// - nlog.internalLogIncludeTimestamp
        /// </remarks>
        public static ISetupInternalLoggerBuilder SetupFromEnvironmentVariables(this ISetupInternalLoggerBuilder setupBuilder)
        {
            InternalLogger.LogLevel = GetSetting("nlog.internalLogLevel", "NLOG_INTERNAL_LOG_LEVEL", LogLevel.Off);
            InternalLogger.IncludeTimestamp = GetSetting("nlog.internalLogIncludeTimestamp", "NLOG_INTERNAL_INCLUDE_TIMESTAMP", true);
            InternalLogger.LogToConsole = GetSetting("nlog.internalLogToConsole", "NLOG_INTERNAL_LOG_TO_CONSOLE", false);
            InternalLogger.LogToConsoleError = GetSetting("nlog.internalLogToConsoleError", "NLOG_INTERNAL_LOG_TO_CONSOLE_ERROR", false);
            InternalLogger.LogFile = GetSetting("nlog.internalLogFile", "NLOG_INTERNAL_LOG_FILE", string.Empty);
            return setupBuilder;
        }

        private static string? GetAppSettings(string configName)
        {
#if NETFRAMEWORK
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

        private static string? GetSettingString(string configName, string envName)
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
                string settingValue = EnvironmentHelper.GetSafeEnvironmentVariable(envName);
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

        private static LogLevel GetSetting(string configName, string envName, LogLevel defaultValue)
        {
            var value = GetSettingString(configName, envName);
            if (value is null)
            {
                return defaultValue;
            }

            try
            {
                return LogLevel.FromString(value);
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

        private static T GetSetting<T>(string configName, string envName, T defaultValue)
        {
            var value = GetSettingString(configName, envName);
            if (value is null)
            {
                return defaultValue;
            }

            try
            {
                return (T)Convert.ChangeType(value, typeof(T), CultureInfo.InvariantCulture);
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
    }
}
