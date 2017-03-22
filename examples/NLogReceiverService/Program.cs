namespace NLogReceiverService
{
    using System;
    using System.ServiceModel;
    using System.ServiceModel.Web;

    class Program
    {
        static void Main(string[] args)
        {
            // open web service host which serves cross-domain policy files
            var webServiceHost = new WebServiceHost(typeof(CrossDomainPolicyServer), new Uri("http://localhost:8000"));
            webServiceHost.Open();

            //var webServiceHost2 = new WebServiceHost(typeof(CrossDomainPolicyServer), new Uri("http://localhost:943"));
            //webServiceHost2.Open();

            var policyServer = new PolicyServer();

            // open log receiver server
            var host = new ServiceHost(typeof(LogReceiverServer), new Uri("http://localhost:8000/"));
            host.Open();
            Console.WriteLine("Host opened");
            Console.Write("Press ENTER to close");
            Console.ReadLine();
            webServiceHost.Close();
            //webServiceHost2.Close();
            host.Close();
        }
    }
}
