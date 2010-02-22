using System;
using System.Collections.ObjectModel;
using System.IO;
using Microsoft.SmartDevice.Connectivity;

namespace DeployNetCF
{
    class Program
    {
        const string NetCf20PackageId = "ABD785F0-CDA7-41c5-8375-2451A7CBFF26";
        const string NetCf35PackageId = "ABD785F0-CDA7-41c5-8375-2451A7CBFF37";

        static int Main(string[] args)
        {
            try
            {
                if (args.Length != 4)
                {
                    Usage();
                    return 1;
                }

                string platformId = args[0];
                string deviceId = args[1];
                string packageID;

                switch (args[2])
                {
                    case "2.0":
                        packageID = NetCf20PackageId;
                        break;

                    case "3.5":
                        packageID = NetCf35PackageId;
                        break;

                    default:
                        packageID = args[2];
                        break;
                }

                string cabName = args[3];

                // Get the datastore object
                var dsmgr = new DatastoreManager(1033);

                var platform = dsmgr.GetPlatform(new ObjectId(platformId));
                var device = platform.GetDevice(new ObjectId(deviceId));

                Console.WriteLine("Connecting to device...");
                device.Connect();

                FileDeployer fileDeployer = device.GetFileDeployer();

                Console.WriteLine("Deploying package...");
                fileDeployer.DownloadPackage(new ObjectId(packageID));

                Console.WriteLine("Installing package...");
                RemoteProcess installer = device.GetRemoteProcess();
                installer.Start("wceload.exe", cabName);
                while (installer.HasExited() != true)
                {
                    System.Threading.Thread.Sleep(1000);
                }
                var exitCode = installer.GetExitCode();
                if (exitCode != 0)
                {
                    Console.WriteLine("Installation failed. Exit code: {0}", exitCode);
                }
                else
                {
                    Console.WriteLine("Installation succeeded.");
                }
                return exitCode;
            }
            catch (Exception ex)
            {
                Console.WriteLine("ERROR: {0}", ex);
                return 1;
            }
        }

        private static void Usage()
        {
            Console.WriteLine("DeployNetCF {platformId} {deviceId} {package} {cabfilename.cab}");

            var dsmgr = new DatastoreManager(1033);

            Console.WriteLine();
            Console.WriteLine("PlatformId can be:");
            foreach (var platform in dsmgr.GetPlatforms())
            {
                Console.WriteLine("  {0}: {1}", platform.Id, platform.Name);
            }

            Console.WriteLine();
            Console.WriteLine("DevicesId can be:");
            foreach (var platform in dsmgr.GetPlatforms())
            {
                foreach (var device in platform.GetDevices())
                {
                    Console.WriteLine("  {0}: {1}", device.Id, device.Name);
                }
            }

            Console.WriteLine();
            Console.WriteLine("Package ID can be:");
            Console.WriteLine("  ABD785F0-CDA7-41c5-8375-2451A7CBFF26: .NET Compact Framework 2.0");
            Console.WriteLine("  ABD785F0-CDA7-41c5-8375-2451A7CBFF37: .NET Compact Framework 3.5");
        }
    }
}