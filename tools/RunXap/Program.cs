namespace RunXap
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using Microsoft.SmartDevice.Connectivity;

    public class XapRunner
    {
        public static void RunXap(string xapPackage, string iconPath, Guid appGuid, string platformName, string deviceName)
        {
            var datastoreManager = new DatastoreManager(1033);
            Platform wp7 = datastoreManager.GetPlatforms().Single(p => p.Name == platformName);
            Device device = wp7.GetDevices().Single(c => c.Name == deviceName);
            try
            {
                Console.WriteLine("Connecting to {0}...", device.Name);
                device.Connect();
                Console.WriteLine("Connected.");
                bool isInstalled = device.IsApplicationInstalled(appGuid);
                RemoteApplication app;

                if (isInstalled)
                {
                    app = device.GetApplication(appGuid);
                    Console.WriteLine("Stopping running instances...");
                    app.TerminateRunningInstances();
                    Console.WriteLine("Application {0} installed. Removing...", appGuid);
                    app.Uninstall();
                }

                Console.WriteLine("Installing XAP...");
                app = device.InstallApplication(appGuid, appGuid, "apps.normal", iconPath, xapPackage);
                Console.WriteLine("Launching app...");
                app.Launch();
                Console.WriteLine("Launched...");
            }
            finally
            {
                if (device.IsConnected())
                {
                    Console.WriteLine("Disconnecting from {0}...", device.Name);
                    device.Disconnect();
                }
            }
        }
    }
}
