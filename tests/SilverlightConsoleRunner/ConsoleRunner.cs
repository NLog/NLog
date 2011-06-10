namespace SilverlightConsoleRunner
{
    using System;
    using System.Diagnostics;
    using System.Globalization;
    using System.IO;
    using System.Net;
    using System.Threading;
    using System.Windows.Forms;
    using RunXap;

    public class ConsoleRunner
    {
        public int FailedCount = 0;
        public int OtherCount = 0;
        public int PassedCount = 0;
        private Form currentForm;
        private ManualResetEvent finished = new ManualResetEvent(false);
        private string DevicePlatformName = "Windows Phone 7";
        private string DeviceName = "Windows Phone Emulator";
        public string IconFile { get; set; }
        public Guid AppGuid { get; set; }
        public string XapFile { get; set; }

        public TextWriter LogWriter { get; set; }

        public string SilverlightVersion { get; set; }

        public void Log(string message)
        {
            Console.Write(message);
            this.LogWriter.Write(message);
            this.LogWriter.Flush();
        }

        public void Run()
        {
            using (var httpServer = new MicroHttpServer())
            {
                httpServer.AddHandler("/Completed", OnTestCompleted);
                httpServer.AddHandler("/TestMethodCompleted", OnTestMethodCompleted);
                
                switch (SilverlightVersion)
                {
                    case "SL2":
                        httpServer.AddResourceHandler("/Silverlight.js", "text/plain", typeof(ConsoleRunner), "Silverlight2.js");
                        httpServer.AddResourceHandler("/XapHost.html", "text/html", typeof(ConsoleRunner), "XapHost2.html");
                        break;

                    case "SL3":
                        httpServer.AddResourceHandler("/Silverlight.js", "text/plain", typeof(ConsoleRunner), "Silverlight3.js");
                        httpServer.AddResourceHandler("/XapHost.html", "text/html", typeof(ConsoleRunner), "XapHost3.html");
                        break;

                    case "SL4":
                        httpServer.AddResourceHandler("/Silverlight.js", "text/plain", typeof(ConsoleRunner), "Silverlight4.js");
                        httpServer.AddResourceHandler("/XapHost.html", "text/html", typeof(ConsoleRunner), "XapHost4.html");
                        break;

                    case "WP7":
                    case "WP71":
                        break;

                    default:
                        throw new NotSupportedException("Unsupported silverlight version: '" + SilverlightVersion + "'"); 
                }

                httpServer.AddFileHandler("/xapfile.xap", "application/x-silverlight-app", this.XapFile);
                httpServer.Start();

                if (this.SilverlightVersion.StartsWith("SL"))
                {
                    using (var form = new RunnerForm())
                    {
                        form.Url = "http://localhost:" + httpServer.ListenPort + "/XapHost.html";
                        this.currentForm = form;
                        form.ShowDialog();
                        this.currentForm = null;
                    }
                }
                else if (this.SilverlightVersion.StartsWith("WP"))
                {
                    XapRunner.RunXap(this.XapFile, this.IconFile, this.AppGuid, this.DevicePlatformName, this.DeviceName);
                }
                else
                {
                    throw new NotImplementedException();
                }

                this.finished.WaitOne();

                this.Log(
                    string.Format(
                        CultureInfo.InvariantCulture,
                        "Passed: {0} Failed: {1} Pass Rate: {2}%",
                        this.PassedCount,
                        this.FailedCount,
                        Math.Round(100.0 * this.PassedCount / (this.PassedCount + this.FailedCount + this.OtherCount), 2)));
            }
        }

        private void OnTestMethodCompleted(HttpListenerContext context)
        {
            ConsoleColor oldColor = Console.ForegroundColor;
            string result = context.Request.QueryString["result"];
            string methodName = context.Request.QueryString["method"];
            string className = context.Request.QueryString["class"];
            switch (result)
            {
                case "Passed":
                    Console.ForegroundColor = ConsoleColor.Green;
                    this.PassedCount++;
                    break;

                case "Failed":
                    Console.ForegroundColor = ConsoleColor.Red;
                    this.FailedCount++;
                    break;

                default:
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    this.OtherCount++;
                    break;
            }

            this.Log(result.PadRight(10));
            Console.ForegroundColor = oldColor;
            this.Log(className + "." + methodName);
            this.Log(Environment.NewLine);
            Console.ForegroundColor = oldColor;
        }

        private void OnTestCompleted(HttpListenerContext context)
        {
            byte[] data = new byte[16384];
            int got;

            using (var fos = File.Create("TestResults.trx"))
            {
                while ((got = context.Request.InputStream.Read(data, 0, data.Length)) > 0)
                {
                    fos.Write(data, 0, got);
                }
            }
            
            context.Response.OutputStream.Close();
            if (this.currentForm != null)
            {
                this.currentForm.Close();
            }

            this.finished.Set();
        }
    }
}