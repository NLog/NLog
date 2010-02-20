using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;

namespace SilverlightConsoleRunner
{
    public class MicroHttpServer : IDisposable
    {
        private Socket socket;
        private readonly Dictionary<string, Func<byte[]>> handlers = new Dictionary<string, Func<byte[]>>();
        private readonly Dictionary<string, string> contentTypes = new Dictionary<string, string>();
        private readonly Regex getRegex = new Regex("^GET (?<url>.*) HTTP/1", RegexOptions.Compiled);

        public MicroHttpServer()
        {
            this.ListenPort = 17788;
        }

        public int ListenPort { get; set; }

        public bool IsRunning { get; set; }

        public void Start()
        {
            socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            socket.Bind(new IPEndPoint(IPAddress.Loopback, this.ListenPort));
            socket.Listen(10);
            socket.BeginAccept(OnAccept, null);
        }

        private void OnAccept(IAsyncResult ar)
        {
            using (Socket acceptedSocket = this.socket.EndAccept(ar))
            {
                socket.BeginAccept(OnAccept, null);
                HandleConnection(acceptedSocket);
            }
        }

        private void HandleConnection(Socket socket)
        {
            NetworkStream ns = new NetworkStream(socket, false);
            StreamReader sr = new StreamReader(ns, Encoding.ASCII);
            StreamWriter sw = new StreamWriter(ns, Encoding.UTF8);
            string url = "";

            try
            {
                string line;
                List<string> lines = new List<string>();
                while ((line = sr.ReadLine()) != null && line.Length > 0)
                {
                    lines.Add(line);
                }

                var match = getRegex.Match(lines[0]);
                if (!match.Success)
                {
                    throw new NotSupportedException("Not supported request format.");
                }

                url = match.Groups["url"].Value;
                byte[] data = this.handlers[url]();

                // Console.WriteLine("Serving: {0} {1} bytes {2}", url, data.Length, contentTypes[url]);
                sw.WriteLine("HTTP/1.0 200 OK");
                sw.WriteLine("Content-Type: {0}", contentTypes[url]);
                sw.WriteLine("Content-Length: {0}", data.Length);
                sw.WriteLine();
                sw.Flush();
                sw.BaseStream.Write(data, 0, data.Length);
            }
            catch (Exception ex)
            {
                Console.WriteLine("ERROR: {0} {1}", url, ex.Message);
                byte[] exceptionData = sw.Encoding.GetBytes(ex.ToString());
                sw.WriteLine("HTTP/1.0 500 Internal eror");
                sw.WriteLine("Content-Type: text/plain");
                sw.WriteLine("Content-Length: {0}", exceptionData.Length);
                sw.WriteLine();
                sw.Flush();
                sw.BaseStream.Write(exceptionData, 0, exceptionData.Length);
            }
        }

        public void Stop()
        {
            if (this.socket != null)
            {
                this.socket.Close();
                this.socket = null;
            }
        }

        public void AddHandler(string url, string contentType, Func<byte[]> handler)
        {
            this.handlers.Add(url, handler);
            this.contentTypes[url] = contentType;
        }

        public void AddFileHandler(string url, string contentType, string fileName)
        {
            this.AddHandler(url, contentType, 
                () => File.ReadAllBytes(fileName));
        }


        public void AddResourceHandler(string url, string contentType, Type typeScope, string resourceName)
        {
            this.AddHandler(url, contentType,
                            () =>
                                {
                                    Stream s = typeScope.Assembly.GetManifestResourceStream(typeScope, resourceName);
                                    byte[] data = new byte[s.Length];
                                    s.Read(data, 0, data.Length);
                                    return data;
                                });
        }

        public void Dispose()
        {
            this.Stop();
        }
    }
}
