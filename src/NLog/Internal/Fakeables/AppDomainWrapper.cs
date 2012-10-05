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
            FriendlyName = appDomain.FriendlyName;
#endif
#if !NET_CF && !SILVERLIGHT && !MONO
            appDomain.ProcessExit += OnProcessExit;
            appDomain.DomainUnload += OnDomainUnload;
#endif
        }

        /// <summary>
        /// Gets a the current <see cref="AppDomain"/> wrappered in a <see cref="AppDomainWrapper"/>.
        /// </summary>
        public static AppDomainWrapper CurrentDomain { get { return new AppDomainWrapper(AppDomain.CurrentDomain); } }

#if !SILVERLIGHT && !NET_CF
        /// <summary>
        /// Gets or sets the base directory that the assembly resolver uses to probe for assemblies.
        /// </summary>
        public string BaseDirectory { get; private set; }

        /// <summary>
        /// Gets or sets the name of the configuration file for an application domain.
        /// </summary>
        public string ConfigurationFile { get; private set; }

        /// <summary>
        /// Gets or sets the list of directories under the application base directory that are probed for private assemblies.
        /// </summary>
        public IEnumerable<string> PrivateBinPath { get; private set; }

        /// <summary>
        /// Gets or set the friendly name.
        /// </summary>
        public string FriendlyName { get; private set; }
#endif

#if !NET_CF && !SILVERLIGHT && !MONO
        /// <summary>
        /// Process exit event.
        /// </summary>
        public event EventHandler<EventArgs> ProcessExit;

        /// <summary>
        /// Domain unloaded event.
        /// </summary>
        public event EventHandler<EventArgs> DomainUnload;

        private void OnDomainUnload(object sender, EventArgs e)
        {
            var handler = DomainUnload;
            if (handler != null) handler(sender, e);
        }

        private void OnProcessExit(object sender, EventArgs eventArgs)
        {
            var handler = ProcessExit;
            if (handler != null) handler(sender, eventArgs);
        }
#endif
    }
}