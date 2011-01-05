namespace NLogReceiverForwarderService
{
    using System;
    using System.ServiceModel;
    using NLog.LogReceiverService;

    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                var uri = new Uri("http://localhost:5000/LogReceiver.svc");
                var host = new ServiceHost(typeof(LogReceiverForwardingService), uri);
                var binding = new BasicHttpBinding();
                host.AddServiceEndpoint(typeof(ILogReceiverServer), binding, uri);
                host.Open();
                Console.WriteLine("Host opened.");
                Console.ReadLine();
                host.Close();
                Console.WriteLine("Host closed.");
            }
            catch (Exception ex)
            {
                Console.WriteLine("ERROR: {0}", ex.ToString());
                Console.ReadLine();
            }
        }
    }
}
