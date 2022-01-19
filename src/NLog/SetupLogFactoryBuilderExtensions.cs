// 
// Copyright (c) 2004-2021 Jaroslaw Kowalski <jaak@jkowalski.net>, Kim Christensen, Julian Verdurmen
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
    using NLog.Config;

    /// <summary>
    /// Extension methods to setup general option before loading NLog LoggingConfiguration
    /// </summary>
    public static class SetupLogFactoryBuilderExtensions
    {
        /// <summary>
        /// Configures the global time-source used for all logevents
        /// </summary>
        /// <remarks>
        /// Available by default: <see cref="NLog.Time.AccurateLocalTimeSource"/>, <see cref="NLog.Time.AccurateUtcTimeSource"/>, <see cref="NLog.Time.FastLocalTimeSource"/>, <see cref="NLog.Time.FastUtcTimeSource"/>
        /// </remarks>
        public static ISetupLogFactoryBuilder SetTimeSource(this ISetupLogFactoryBuilder configBuilder, NLog.Time.TimeSource timeSource)
        {
            NLog.Time.TimeSource.Current = timeSource;
            return configBuilder;
        }

        /// <summary>
        /// Configures the global time-source used for all logevents to use <see cref="NLog.Time.AccurateUtcTimeSource"/>
        /// </summary>
        public static ISetupLogFactoryBuilder SetTimeSourcAccurateUtc(this ISetupLogFactoryBuilder configBuilder)
        {
            return configBuilder.SetTimeSource(NLog.Time.AccurateUtcTimeSource.Current);
        }

        /// <summary>
        /// Configures the global time-source used for all logevents to use <see cref="NLog.Time.AccurateLocalTimeSource"/>
        /// </summary>
        public static ISetupLogFactoryBuilder SetTimeSourcAccurateLocal(this ISetupLogFactoryBuilder configBuilder)
        {
            return configBuilder.SetTimeSource(NLog.Time.AccurateLocalTimeSource.Current);
        }

        /// <summary>
        /// Updates the dictionary <see cref="GlobalDiagnosticsContext"/> ${gdc:item=} with the name-value-pair
        /// </summary>
        public static ISetupLogFactoryBuilder SetGlobalContextProperty(this ISetupLogFactoryBuilder configBuilder, string name, object value)
        {
            GlobalDiagnosticsContext.Set(name, value);
            return configBuilder;
        }

        /// <summary>
        /// Sets whether to automatically call <see cref="LogFactory.Shutdown"/> on AppDomain.Unload or AppDomain.ProcessExit
        /// </summary>
        public static ISetupLogFactoryBuilder SetAutoShutdown(this ISetupLogFactoryBuilder configBuilder, bool enabled)
        {
            configBuilder.LogFactory.AutoShutdown = enabled;
            return configBuilder;
        }

        /// <summary>
        /// Sets the default culture info to use as <see cref="LogEventInfo.FormatProvider"/>.
        /// </summary>
        public static ISetupLogFactoryBuilder SetDefaultCultureInfo(this ISetupLogFactoryBuilder configBuilder, System.Globalization.CultureInfo cultureInfo)
        {
            configBuilder.LogFactory.DefaultCultureInfo = cultureInfo;
            return configBuilder;
        }

        /// <summary>
        /// Sets the global log level threshold. Log events below this threshold are not logged.
        /// </summary>
        public static ISetupLogFactoryBuilder SetGlobalThreshold(this ISetupLogFactoryBuilder configBuilder, LogLevel logLevel)
        {
            configBuilder.LogFactory.GlobalThreshold = logLevel;
            return configBuilder;
        }

        /// <summary>
        /// Gets or sets a value indicating whether <see cref="NLogConfigurationException"/> should be thrown on configuration errors
        /// </summary>
        public static ISetupLogFactoryBuilder SetThrowConfigExceptions(this ISetupLogFactoryBuilder configBuilder, bool enabled)
        {
            configBuilder.LogFactory.ThrowConfigExceptions = enabled;
            return configBuilder;
        }

        /// <summary>
        /// Mark Assembly as hidden, so Assembly methods are excluded when resolving ${callsite} from StackTrace
        /// </summary>
        public static ISetupLogFactoryBuilder AddCallSiteHiddenAssembly(this ISetupLogFactoryBuilder configBuilder, System.Reflection.Assembly assembly)
        {
            LogManager.AddHiddenAssembly(assembly);
            return configBuilder;
        }
    }
}
