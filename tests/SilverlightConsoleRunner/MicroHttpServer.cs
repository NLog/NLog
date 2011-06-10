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
        private HttpListener httpListener;
        private readonly Dictionary<Regex, Action<HttpListenerContext>> handlers = new Dictionary<Regex, Action<HttpListenerContext>>();
        
        public MicroHttpServer()
        {
            this.ListenPort = 17788;
        }

        public int ListenPort { get; set; }

        public bool IsRunning { get; set; }

        public void Start()
        {
            httpListener = new HttpListener();
            httpListener.Prefixes.Add("http://localhost:" + this.ListenPort + "/");
            httpListener.Start();
            httpListener.BeginGetContext(this.OnNewRequest, null);
        }

        private void OnNewRequest(IAsyncResult ar)
        {
            var context = httpListener.EndGetContext(ar);
            httpListener.BeginGetContext(this.OnNewRequest, null);
            foreach (var d in handlers)
            {
                var m = d.Key.Match(context.Request.Url.LocalPath);
                if (m.Success)
                {
                    try
                    {
                        d.Value(context);
                        context.Response.OutputStream.Close();
                    }
                    catch (Exception ex)
                    {
                        context.Response.StatusCode = 500;
                        context.Response.ContentType = "text/plain";
                        using (var sw = new StreamWriter(context.Response.OutputStream))
                        {
                            sw.WriteLine("ERROR: {0}", ex);
                        }
                    }
                    return;
                }
            }

            context.Response.StatusCode = 404;
            context.Response.ContentType = "text/plain";
            using (var sw = new StreamWriter(context.Response.OutputStream))
            {
                sw.WriteLine("Not found.");
            }

        }

        public void Stop()
        {
            if (this.httpListener != null)
            {
                this.httpListener.Abort();
                this.httpListener.Close();
                this.httpListener = null;
            }
        }

        public void AddHandler(string url, Action<HttpListenerContext> handler)
        {
            this.handlers.Add(new Regex(url), handler);
        }

        public void AddFileHandler(string url, string contentType, string fileName)
        {
            this.AddHandler(url,
                context =>
                    {
                        byte[] data = File.ReadAllBytes(fileName);
                        context.Response.ContentType = contentType;
                        context.Response.OutputStream.Write(data, 0, data.Length);
                        context.Response.OutputStream.Close();
                    });
        }


        public void AddResourceHandler(string url, string contentType, Type typeScope, string resourceName)
        {
            this.AddHandler(url,
                            context =>
                                {
                                    Stream s = typeScope.Assembly.GetManifestResourceStream(typeScope, resourceName);
                                    byte[] data = new byte[s.Length];
                                    s.Read(data, 0, data.Length);
                                    context.Response.ContentType = contentType;
                                    context.Response.OutputStream.Write(data, 0, data.Length);
                                    context.Response.OutputStream.Close();
                                });
        }

        public void Dispose()
        {
            this.Stop();
        }
    }
}
