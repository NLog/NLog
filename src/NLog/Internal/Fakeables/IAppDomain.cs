namespace NLog.Internal.Fakeables
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// Interface for fakeable the current <see cref="AppDomain"/>. Not fully implemented, please methods/properties as necessary.
    /// </summary>
    public interface IAppDomain
    {
        /// <summary>
        /// Gets or sets the base directory that the assembly resolver uses to probe for assemblies.
        /// </summary>
        string BaseDirectory { get; }

        /// <summary>
        /// Gets or sets the name of the configuration file for an application domain.
        /// </summary>
        string ConfigurationFile { get; }

        /// <summary>
        /// Gets or sets the list of directories under the application base directory that are probed for private assemblies.
        /// </summary>
        IEnumerable<string> PrivateBinPath { get; }

        /// <summary>
        /// Gets or set the friendly name.
        /// </summary>
        string FriendlyName { get; }

        /// <summary>
        /// Process exit event.
        /// </summary>
        event EventHandler<EventArgs> ProcessExit;
        
        /// <summary>
        /// Domain unloaded event.
        /// </summary>
        event EventHandler<EventArgs> DomainUnload;
    }
}