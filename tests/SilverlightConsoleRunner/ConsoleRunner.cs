namespace SilverlightConsoleRunner
{
    using System;
    using System.Globalization;
    using System.IO;

    public class ConsoleRunner
    {
        public int FailedCount = 0;
        public int OtherCount = 0;
        public int PassedCount = 0;
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

                    default:
                        throw new NotSupportedException("Unsupported silverlight version: '" + SilverlightVersion + "'"); 
                }

                httpServer.AddFileHandler("/xapfile.xap", "application/x-silverlight-app", this.XapFile);
                httpServer.Start();

                using (var form = new RunnerForm())
                {
                    form.OnLogEvent += this.OnLogEvent;
                    form.OnCompleted += this.OnCompleted;
                    form.Url = "http://localhost:" + httpServer.ListenPort + "/XapHost.html";
                    form.ShowDialog();
                }

                this.Log(
                    string.Format(
                        CultureInfo.InvariantCulture,
                        "Passed: {0} Failed: {1} Pass Rate: {2}%",
                        this.PassedCount,
                        this.FailedCount,
                        Math.Round(100.0 * this.PassedCount / (this.PassedCount + this.FailedCount + this.OtherCount), 2)));
            }
        }

        private void OnCompleted(object sender, TestCompletedEventArgs e)
        {
            File.WriteAllText("TestResults.trx", e.TrxFileContents);
        }

        private void OnLogEvent(object sender, LogEventArgs e)
        {
            ConsoleColor oldColor = Console.ForegroundColor;

            switch (e.Result)
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

            this.Log(e.Result.PadRight(20));
            Console.ForegroundColor = oldColor;
            this.Log(e.ClassName + "." + e.MethodName);
            this.Log(Environment.NewLine);
            if (e.Exception != null && e.Result != "Passed")
            {
                Console.ForegroundColor = ConsoleColor.DarkGray;
                this.Log(e.Exception);
                this.Log(Environment.NewLine);
            }

            Console.ForegroundColor = oldColor;
        }
    }
}