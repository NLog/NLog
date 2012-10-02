namespace NLog.Internal.Fakeables
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// Adapter for <see cref="AppDomain"/> to <see cref="IAppDomain"/>
    /// </summary>
    public class AppDomainWrapper : IAppDomain
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AppDomainWrapper"/> class.
        /// </summary>
        /// <param name="appDomain">The <see cref="AppDomain"/> to wrap.</param>
        public AppDomainWrapper(AppDomain appDomain)
        {
#if !SILVERLIGHT && !NET_CF
            BaseDirectory = appDomain.BaseDirectory;
            ConfigurationFile = appDomain.SetupInformation.ConfigurationFile;

            string privateBinPath = appDomain.SetupInformation.PrivateBinPath;
            PrivateBinPath = string.IsNullOrEmpty(privateBinPath)
                                 ? new string[] {}
                                 : appDomain.SetupInformation.PrivateBinPath.Split(new[] {';'},
                                                                                   StringSplitOptions.RemoveEmptyEntries);
#endif
        }

        /// <summary>
        /// Gets or sets the base directory that the assembly resolver uses to probe for assemblies.
        /// </summary>
        public string BaseDirectory { get; set; }

        /// <summary>
        /// Gets or sets the name of the configuration file for an application domain.
        /// </summary>
        public string ConfigurationFile { get; set; }

        /// <summary>
        /// Gets or sets the list of directories under the application base directory that are probed for private assemblies.
        /// </summary>
        public IEnumerable<string> PrivateBinPath { get; set; }
    }
}