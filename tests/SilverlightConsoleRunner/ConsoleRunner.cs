using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;

namespace SilverlightConsoleRunner
{
    public class ConsoleRunner
    {
        public string XapFile { get; set; }

        public TextWriter LogWriter { get; set; }
             
        public int PassedCount = 0;
        public int FailedCount = 0;
        public int OtherCount = 0;

        public void Run()
        {
            using (var httpServer = new MicroHttpServer())
            {
                httpServer.AddResourceHandler("/XapHost.html", "text/html", typeof(ConsoleRunner), "XapHost.html");
                httpServer.AddResourceHandler("/Silverlight.js", "text/plain", typeof(ConsoleRunner), "Silverlight.js");
                httpServer.AddFileHandler("/xapfile.xap", "application/x-silverlight-app", XapFile);
                httpServer.Start();

                using (var form = new RunnerForm())
                {
                    form.OnLogEvent += OnLogEvent;
                    form.Url = "http://localhost:" + httpServer.ListenPort + "/XapHost.html";
                    form.ShowDialog();
                }

                Log("Passed: {0} Failed: {1} Pass Rate: {2}%", PassedCount, FailedCount, Math.Round(100.0 * PassedCount / (PassedCount + FailedCount + OtherCount), 2));
            }
        }

        private void OnLogEvent(object sender, LogEventArgs e)
        {
            switch (e.Result)
            {
                case "Passed":
                    Console.ForegroundColor = ConsoleColor.Green;
                    PassedCount++;
                    break;

                case "Failed":
                    Console.ForegroundColor = ConsoleColor.Red;
                    FailedCount++;
                    break;

                default:
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    OtherCount++;
                    break;
            }

            this.Log("{2} {0}.{1}", e.ClassName, e.MethodName, e.Result.PadRight(20));
            if (e.Exception != null && e.Result != "Passed")
            {
                Console.ForegroundColor = ConsoleColor.DarkGray;
                this.Log(e.Exception);
            }
        }

        public void Log(string message, params object[] arguments)
        {
            if (arguments.Length > 0)
            {
                message = string.Format(CultureInfo.InvariantCulture, message, arguments);
            }

            Console.WriteLine(message);
            this.LogWriter.WriteLine(message);
        }
    }
}
